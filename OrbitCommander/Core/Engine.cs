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
    public static Vector2 ScreenSize { get; private set; } //Render target size
    public static Vector2 BackBuffer { get; private set; } //Monitor size
    public static Vector2 MousePositionOffset { get; set; }
    public static Timespan IngameTime { get; set; } = new();
    public static float DeltaSeconds { get; private set; }
    private readonly float timeScale = 1f;
    private readonly int targetFramerate = 60;
    public static float ScreenShakeFactor { get; private set; } = 0;
    public static int SaveSlot { get; private set; } = 0;
    private List<IActor> ShaderExceptions { get; } = [];
    public LoadingStage LoadingStage { get; private set; } = LoadingStage.Preload;

    public static float Time { get; private set; } = 0;

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

        BackBuffer = new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
        ScreenSize = new Vector2(1920 * BackBuffer.Y / 1080, 1080);

        IsMouseVisible = false;
    }
    protected override void LoadContent()
    {
        var loading = Task.Factory.StartNew(() =>
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Line = new Texture2D(graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            Line.SetData([Color.White]);

            Assets.LoadStageOne(Content);

            UIManager = new UIManager();
            UIManager.BackBuffer = BackBuffer;

            //UI behaviors that need special permission
            UI.SingleplayerButton.AddBehaviour(delegate()
            {
                UIManager.DisableAll();
                CurrentGameState.SwitchState(new Loading(delegate ()
                {
                    SaveGame = new();
                    Events.UpdateModulesUI();
                    Startgame();
                }, LoadingStage.Complete));
            });
            UI.PrevSave.AddBehaviour(delegate
            {
                SaveSlot = Math.Clamp(SaveSlot - 1, 0, 10);
                Events.GetSave();
            });
            UI.NextSave.AddBehaviour(delegate
            {
                SaveSlot = Math.Clamp(SaveSlot + 1, 0, 10);
                Events.GetSave();
            });
            UI.ApplyChanges.AddBehaviour(delegate ()
            {
                Self.Window.IsBorderless = UI.type == 1;
                Self.graphics.IsFullScreen = UI.type == 2;
                Self.graphics.PreferredBackBufferWidth = (int)UI.resolutions[UI.selectedResolution].X;
                Self.graphics.PreferredBackBufferHeight = (int)UI.resolutions[UI.selectedResolution].Y;
                BackBuffer = UI.resolutions[UI.selectedResolution];
                UIManager.BackBuffer = BackBuffer;
                Self.graphics.ApplyChanges();
            });
            UI.AddUIElements();
            renderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            Camera = new Camera(Vector2.Zero, ScreenSize / 2, 1f, 0);
            DialogueManager = new DialogueManager();
            CurrentGameState.SwitchState(new MainMenu());
            LoadingStage = LoadingStage.MainMenu;

            Assets.LoadFinal(Content);
            Events.SetModules();
            LoadingStage = LoadingStage.Complete;
        });
    }
    public static void Startgame()
    {
        UIManager.DisableAll();
        ParticleManager.Initialize();
        SaveGame.CurrentMission = Mission.missions[SaveGame.CurrentMissionIndex].instance();
        SoundManager.Initialize();
        Events.UpdateModulesStatus();
        SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
        ScreenShakeFactor = 0;
        SaveGame.Player.Progression = Mission.missions[SaveGame.CurrentMissionIndex].data.PlayerProgression;
        SaveGame.Player.leashedMaterials.Clear();
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
            Events.MissionSelectTrigger(new MissionSelect());
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
        if (IsActive)
        {
            UIManager.Update();
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
        UI.Timer.text = $"{IngameTime.DrawText}";
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

        Camera.Origin = ScreenSize / 2 - MousePositionOffset;
        //Renders gamespace to a rendertarget, then renders render target with a shader
        GraphicsDevice.SetRenderTarget(renderTarget);
        GraphicsDevice.Clear(SaveGame.ColorScheme.Background());
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, transformMatrix: Camera.Transform);
        CurrentGameState.Draw(spriteBatch);
        spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(new Color(50, 50, 50));
        int renderCoord = (int)(BackBuffer.Y / 0.5625f);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, Assets.GlobalShader);
        spriteBatch.Draw(renderTarget, new Rectangle((int)(BackBuffer.X - ScreenSize.X), 0, renderCoord, (int)BackBuffer.Y), Color.White);
        spriteBatch.End();

        //Rendering some components without the shader
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
        DialogueManager.Draw(spriteBatch);
        UIManager.Draw(spriteBatch);
        foreach (var exception in ShaderExceptions)
        {
            exception.Draw(spriteBatch);
        }
        ShaderExceptions.Clear();
        if (Input.NewMouseState.LeftButton == ButtonState.Released)
        {
            spriteBatch.Draw(Assets.Get(Sprites.Cursor), new Vector2(Mouse.GetState().X, Mouse.GetState().Y), null, Color.White, 0, Vector2.Zero, UIManager.UIScale / 2, 0, 0.5f);
        }
        else
        {
            spriteBatch.Draw(Assets.Get(Sprites.ClickedCursor), new Vector2(Mouse.GetState().X, Mouse.GetState().Y), null, Color.White, 0, Vector2.Zero, UIManager.UIScale / 2, 0, 0.5f);
        }
        if (SaveGame.DebugMode)
        {
            int logCount = debugLog.Count;
            int offset = 0;
            if (logCount > 10)
            {
                logCount = 10;
            }
            for (int i = 0; i < logCount; i++)
            {
                Vector2 textPosition = new(35, 20 + 20 * offset * UIManager.UIScale);
                try
                {
                    spriteBatch.DrawString(Assets.TextFont, $"{i + 1}: {debugLog[i].log}", textPosition, debugLog[i].color, 0, Vector2.Zero, UIManager.UIScale, SpriteEffects.None, 0.45f);
                    offset += debugLog[i].log.Split('\n').Length;
                }
                catch (Exception e)
                {
                    spriteBatch.DrawString(Assets.TextFont, $"{i + 1}: {e.Message}", textPosition, Color.Red, 0, Vector2.Zero, UIManager.UIScale, SpriteEffects.None, 0.45f);
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
