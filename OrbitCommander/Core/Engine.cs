using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OrbitCommander.Components;
using OrbitCommander.Entities;
using OrbitCommander.MissionComponents;
using OrbitCommander.Particles;
using OrbitCommander.Story;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UILib.Content;
using System.Diagnostics;

namespace OrbitCommander.Core;

public class Engine : Game
{
    private static readonly List<(string log, Color color)> debugLog = [];
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private RenderTarget2D renderTarget;
    public static UIManager UIManager { get; private set; }
    public static DialogueManager DialogueManager { get; private set; }
    public static SaveGame SaveGame { get; private set; }
    public static Camera Camera { get; private set; }
    public static Engine Self { get; private set; }
    public static Texture2D Line { get; private set; }
    public static Vector2 BackBuffer { get => new Vector2(Self.graphics.PreferredBackBufferWidth, Self.graphics.PreferredBackBufferHeight); set { Self.graphics.PreferredBackBufferWidth = (int)value.X;  Self.graphics.PreferredBackBufferHeight = (int)value.Y; Self.graphics.ApplyChanges(); }  } //Application size
    public static Timespan IngameTime { get; set; } = new();
    public static float DeltaSeconds { get; private set; }
    private readonly float timeScale = 1f;
    private readonly int targetFramerate = 60;
    public static float ScreenShakeFactor { get; private set; } = 0;
    public static int SaveSlot { get; private set; } = 0;
    private List<IActor> ShaderExceptions { get; } = [];
    public LoadingStage LoadingStage { get; private set; } = LoadingStage.Preload;
    public static float Time { get; private set; } = 0;
    List<Vector2> relativePositions = [];
    public bool HandMode { get; private set; } = false; //0 = cursor, 1 = hand
    Vector2 handPos = Vector2.Zero;
    public Vector2 TrueCursorPosition { get; private set; } = Vector2.Zero;
    Vector2 prevPos = Vector2.Zero;
    private Task loadingThread;
    public Engine()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        Self = this;
    }
    protected override void Initialize()
    {
        base.Initialize();
        Window.Title = "Orbit Commander";
        graphics.PreferredBackBufferWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
        graphics.PreferredBackBufferHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
        //Window.IsBorderless = true;
        //graphics.IsFullScreen = true;
        graphics.ApplyChanges();

        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1d / targetFramerate);

        renderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += new EventHandler<EventArgs>(ClientSizeChanged);

        IsMouseVisible = false;
    }
    private static void ClientSizeChanged(object sender, EventArgs e)
    {
        BackBuffer = new Vector2(Self.Window.ClientBounds.Width, Self.Window.ClientBounds.Height);
    }
    protected override void LoadContent()
    {
        loadingThread = Task.Factory.StartNew((Action)(() =>
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Line = new Texture2D(graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            Line.SetData([Color.White]);

            Assets.LoadStageOne(Content);

            UIManager = new UIManager((Func<Vector2>)(() => BackBuffer));

            //UI behaviors that need special permission
            UI.SingleplayerButton.RisingInteract += delegate
            {
                CurrentGameState.SwitchState(new Loading(delegate ()
                {
                    SaveGame = new();
                    Events.UpdateModulesUI();
                    Startgame();
                }, LoadingStage.Complete));
            };
            UI.PrevSave.RisingInteract += delegate
            {
                SaveSlot = Math.Clamp(SaveSlot - 1, 0, 10);
                Events.GetSave();
            };
            UI.NextSave.RisingInteract += delegate
            {
                SaveSlot = Math.Clamp(SaveSlot + 1, 0, 10);
                Events.GetSave();
            };
            UI.ApplyChanges.RisingInteract += delegate
            {
                Self.Window.IsBorderless = UI.windowType == 1;
                Self.graphics.IsFullScreen = UI.windowType == 2;
                Self.graphics.PreferredBackBufferWidth = (int)UI.resolutions[UI.selectedResolution].X;
                Self.graphics.PreferredBackBufferHeight = (int)UI.resolutions[UI.selectedResolution].Y;
                BackBuffer = UI.resolutions[UI.selectedResolution];
                Self.graphics.ApplyChanges();
            };
            UI.AddUIElements();
            Camera = new Camera(Vector2.Zero, (Vector2)(BackBuffer / 2), 1f, 0);
            DialogueManager = new DialogueManager();
            CurrentGameState.SwitchState(new MainMenu());
            LoadingStage = LoadingStage.MainMenu;

            Assets.LoadFinal(Content);
            Events.SetModules();
            LoadingStage = LoadingStage.Complete;
        }));
        
    }
    public static void Startgame()
    {
        ParticleManager.Initialize();
        SaveGame.CurrentMission = Mission.missions[SaveGame.CurrentMissionIndex].instance();
        SoundManager.Initialize();
        Events.UpdateModulesStatus();
        SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
        ScreenShakeFactor = 0;
        SaveGame.Player.Progression = Mission.missions[SaveGame.CurrentMissionIndex].data.PlayerProgression;
        //Prevents scrap and debuffs from persisting between missions.
        SaveGame.Player.leashedMaterials.Clear();
        SaveGame.Player.GetComponent<Statuses>().Clear();
        SaveGame.CurrentMission.Initialize();
    }
    public static void Load()
    {
        if(Self.LoadingStage != LoadingStage.Complete) { return; }
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), $"Content\\Saves\\Save_{SaveSlot}.txt");
        string text = "";
        using (var outputFile = new StreamReader(filePath))
        text = outputFile.ReadLine();
        if (text != "")
        {
            SaveGame = new SaveGame(text);
            CurrentGameState.SwitchState(new MissionSelect());
        }
    }
    public void QueueShaderException(IActor _exception)
    {
        if (!ShaderExceptions.Contains(_exception))
        {
            ShaderExceptions.Add(_exception);
        }
    }
    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (LoadingStage == LoadingStage.Preload)
        {
            return;
        }
        Input.Update();
        if (HandMode)
        {
            handPos = handPos * 0.9f + Input.MousePosition.Direction * 0.1f;
            prevPos = TrueCursorPosition;
            if (IsActive)
            {
                UIManager.Update(handPos, [Mouse.GetState().LeftButton == ButtonState.Pressed, Mouse.GetState().RightButton == ButtonState.Pressed]);
            }
        }
        else
        {
            float distance = Vector2.Distance(handPos, new Vector2(BackBuffer.X / 2, BackBuffer.Y * 0.8f));
            float speed = BackBuffer.X / (BackBuffer.X + distance * 10);
            handPos = handPos * (1 - speed) + new Vector2(BackBuffer.X/2, BackBuffer.Y * 0.8f) * (speed);
            prevPos = TrueCursorPosition;
            if(distance < 50)
            {
                TrueCursorPosition = TrueCursorPosition * 0.5f + Input.MousePosition.Direction * 0.5f;
                if (IsActive)
                {
                    UIManager.Update(new Vector2(-1000, -1000), [Mouse.GetState().LeftButton == ButtonState.Pressed, Mouse.GetState().RightButton == ButtonState.Pressed]);
                }
            }
        }
        if(Input.Tab.IsDown && !Input.Tab.WasDown)
        {
            HandMode = !HandMode;
        }
        SoundManager.Update();
        CurrentGameState.Update();

        DeltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds * timeScale;
        if (ScreenShakeFactor > 0)
        {
            ScreenShakeFactor -= DeltaSeconds;
        }
        else
        {
            ScreenShakeFactor = 0;
        }
        UI.Timer.Text = $"{IngameTime.DrawText}";
        Time += DeltaSeconds;
    }
    public static void WriteLine<T>(T arg, Color _color = default)
    {
        debugLog.Insert(0, ($"{arg?.ToString()}", _color == default ? Color.White : _color));
    }
    public static void ShakeScreen(float _val)
    {
        ScreenShakeFactor = Math.Min(ScreenShakeFactor + _val * _val / (ScreenShakeFactor + _val), 1);
    }
    public Texture2D RenderAtmosphere(float _atmosphereRadius, float _atmosphereStrength, float _planetRadius, Color _color, Atmosphere _planet)
    {
        var renderTarget = new RenderTarget2D(GraphicsDevice, (int)(_atmosphereRadius * 2), (int)(_atmosphereRadius * 2));
        GraphicsDevice.SetRenderTarget(renderTarget);
        GraphicsDevice.Clear(Color.Transparent);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
        float start = _planetRadius;
        if (_atmosphereStrength > 2)
        {
            start = Math.Max(_planetRadius - BackBuffer.Length() / 2, 0);
        }
        for (float r = start; r < _atmosphereRadius; r += MathF.Sqrt(36 + 36 / MathF.Pow(_planet.GetAtmosphereDensity(r), 2)))
        {
            float iterations = MathF.PI * MathF.PI * r / 6 + 4;
            float offset = 1;
            if (_atmosphereStrength > 5)
            {
                offset = MathF.Sin(r) / 4 + 1;
            }
            for (float t = MathF.Tau / MathF.Ceiling(iterations) / 2; t < MathF.Tau; t += MathF.Tau / MathF.Ceiling(iterations))
            {
                spriteBatch.Draw(Assets.Get(Sprites.Circle), new Vector2(_atmosphereRadius, _atmosphereRadius) + Util.ToUnitVector(t) * r, null, _color * MathF.Tanh(_planet.GetAtmosphereDensity(r) / 4f) * offset, t, Assets.DimsOf(Sprites.Circle) / 2, 1, 0, 0);
            }
        }
        spriteBatch.End();
        GraphicsDevice.SetRenderTarget(null);
        return renderTarget;
    }
    public static void DrawFilledLine(SpriteBatch _spriteBatch, Vector2 _position, Rectangle _sourceRectangle, float _percentFilled, Color _lowerColor, Color _higherColor)
    {
        _spriteBatch.Draw(Line, _position, _sourceRectangle, _lowerColor);
        _spriteBatch.Draw(Line, _position, new Rectangle(_sourceRectangle.Location, new Point((int)(_sourceRectangle.Width * _percentFilled), _sourceRectangle.Height)), _higherColor);
    }
    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        if (LoadingStage == LoadingStage.Preload)
        {
            return;
        }

        Camera.Origin = new Vector2(1920, 1080) / 2; //TODO: Make sure this updates when the rendertarget size updates!
        //Renders gamespace to a rendertarget, then renders render target with a shader
        GraphicsDevice.SetRenderTarget(renderTarget);
        GraphicsDevice.Clear(SaveGame.ColorScheme.Background());
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, transformMatrix: Camera.Transform);
        CurrentGameState.Draw(spriteBatch);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

        Vector2 newPosition = TrueCursorPosition;
        float count = 2;
        float i = 0;
        relativePositions.Add(TrueCursorPosition - prevPos);
        if (relativePositions.Count > count)
        {
            relativePositions.RemoveAt(0);
        }
        foreach (var pos in relativePositions)
        {
            newPosition -= pos;
        }
        Texture2D trailTexture = Assets.Get(Sprites.Circle);
        Texture2D mouseTexture;
        Color mouseColor;
        if (Input.LMB.IsDown)
        {
            mouseTexture = Assets.Get(Sprites.ClickedCursor);
            mouseColor = Color.Red;
        }
        else
        {
            mouseTexture = Assets.Get(Sprites.Cursor);
            mouseColor = Color.White;
        }
        for (int j = 0; j < relativePositions.Count; j++)
        {
            Vector2 relativePosition = relativePositions[j];
            float angle = Util.ToAngle(relativePosition);
            Vector2 nextRelative = (j < relativePositions.Count - 1) ? relativePositions[j + 1] : Vector2.Zero;
            float distance = (relativePosition != Vector2.Zero) ? relativePosition.Length() : 1;
            if (distance < 10)
            {
                newPosition += relativePosition;
                continue;
            }
            for (i = i; i < distance; i += 4)
            {
                float lerp = i / distance;
                //TODO; Figure out why the color isn't smooth.
                float t1 = 40 / (distance + 40) * (1 - lerp) + 40 / (nextRelative.Length() + 40) * (lerp);
                float transparency = MathF.Sqrt(t1) * MathF.Sqrt((j + 1) / count) * 0.1f;
                if (distance < 25)
                {
                    transparency *= Math.Clamp((distance - 10) / 15, 0, 1);
                }
                //new Color(24, 108, 80)
                spriteBatch.Draw(trailTexture, newPosition + relativePosition * lerp, null, mouseColor * transparency, angle, UIManager.DimsOf(trailTexture) / 2, UIManager.UIScale / 16 * UIManager.DimsOf(mouseTexture) * (j / count * (1 - lerp) * 0.5f + (j + 1) / count * lerp * 0.5f + 0.5f), 0, 0.5f);
            }
            i -= distance;
            newPosition += relativePosition;
        }
        spriteBatch.Draw(mouseTexture, TrueCursorPosition, null, mouseColor * (25 / (25 + (TrueCursorPosition - prevPos).Length())), 0, UIManager.DimsOf(mouseTexture) / 2, UIManager.UIScale / 2, 0, 0.5f);
        spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(new Color(50, 50, 50));
        Vector2 backbuffer = BackBuffer;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, Assets.GlobalShader);
        spriteBatch.Draw(renderTarget, backbuffer / 2, null, Color.White, 0, new Vector2(renderTarget.Bounds.Width, renderTarget.Bounds.Height) / 2, Math.Max(backbuffer.X/1920, backbuffer.Y/1080), 0, 0);
        spriteBatch.End();

        //Rendering some components without the shader
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null);
        DialogueManager.Draw(spriteBatch);
        UIManager.Draw(spriteBatch);
        foreach (var exception in ShaderExceptions)
        {
            exception.Draw(spriteBatch);
        }
        spriteBatch.Draw(Assets.Get(Sprites.Hand), handPos, null, Color.White, Util.ToAngle(handPos - new Vector2(BackBuffer.X * 3 / 4, BackBuffer.Y * 2)), Vector2.Zero, UIManager.UIScale * 2, 0, 0.5f);
        ShaderExceptions.Clear();

        if (SaveGame.DebugMode)
        {
            int logCount = debugLog.Count;
            int offset = 0;
            if (logCount > 10)
            {
                logCount = 10;
            }
            for (int log = 0; log < logCount; log++)
            {
                Vector2 textPosition = new(35, 20 + 20 * offset * UIManager.UIScale);
                try
                {
                    spriteBatch.DrawString(Assets.TextFont, $"{log + 1}: {debugLog[log].log}", textPosition, debugLog[log].color, 0, Vector2.Zero, UIManager.UIScale, SpriteEffects.None, 0.45f);
                    offset += debugLog[log].log.Split('\n').Length;
                }
                catch (Exception e)
                {
                    spriteBatch.DrawString(Assets.TextFont, $"{log + 1}: {e.Message}", textPosition, Color.Red, 0, Vector2.Zero, UIManager.UIScale, SpriteEffects.None, 0.45f);
                }
            }
        }
        spriteBatch.End();
    }
}
public struct Timespan
{
    public float Duration { get; set; }
    public readonly float Seconds => Duration % 60;
    public readonly float Minutes => (int)(Duration / 60) % 60;
    public readonly float Hours => (int)(Duration / 3600);
    public readonly string DrawText => $"{Hours:00}:{Minutes:00}:{Seconds:00.00}";
}
