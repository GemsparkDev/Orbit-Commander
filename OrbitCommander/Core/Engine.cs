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

    private static UIManager realUI;
    private static UIManager virtualUI;
    public static UIManager RealUI
    {
        get //Lazy initialization prevents static ordering issues
        {
            return realUI ??= new UIManager();
        }
    }
    public static UIManager VirtualUI
    {
        get
        {
            return virtualUI ??= new UIManager();
        }
    }
    public static Cursor Cursor { get; private set; }
    public static DialogueManager DialogueManager { get; private set; }
    public static SaveGame SaveGame { get; private set; }
    public static Camera Camera { get; private set; }
    public static Engine Self { get; private set; }
    public static Texture2D Line { get; private set; }
    public static Vector2 BackBuffer 
    {
        get => new Vector2(Self.graphics.PreferredBackBufferWidth, Self.graphics.PreferredBackBufferHeight);
        set
        {
            if (Self.graphics.PreferredBackBufferWidth != (int)value.X)
            {
                Self.graphics.PreferredBackBufferWidth = (int)value.X;
            }
            if (Self.graphics.PreferredBackBufferHeight != (int)value.Y)
            {
                Self.graphics.PreferredBackBufferHeight = (int)value.Y;
            }
            Self.graphics.ApplyChanges();
        }  
    } //Application size
    public static Timespan IngameTime { get; set; } = new();
    public static float DeltaSeconds { get; private set; }
    private readonly float timeScale = 1f;
    private readonly int targetFramerate = 60;
    public static float ScreenShakeFactor { get; private set; } = 0;
    public static int SaveSlot { get; private set; } = 0;
    private List<IActor> ShaderExceptions { get; } = [];
    public LoadingStage LoadingStage { get; private set; } = LoadingStage.Preload;
    public static float Time { get; private set; } = 0;
    public int StoppedFrames { get; set; } = 0;
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
        Cursor = new Cursor(BackBuffer/2);

        IsMouseVisible = false;
    }
    private static void ClientSizeChanged(object sender, EventArgs e)
    {
        BackBuffer = new Vector2(Self.Window.ClientBounds.Width, Self.Window.ClientBounds.Height);
    }
    protected override void LoadContent()
    {
        loadingThread = Task.Factory.StartNew((() =>
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Line = new Texture2D(graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            Line.SetData([Color.White]);

            Assets.LoadStageOne(Content);

            UIManager.GetBackBuffer = () => BackBuffer;

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
                BackBuffer = UI.resolutions[UI.selectedResolution];
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
        SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
        ScreenShakeFactor = 0;
        SaveGame.CurrentMission.Initialize();
    }
    public static void Load()
    {
        throw new NotImplementedException();
        if(Self.LoadingStage != LoadingStage.Complete) { return; }
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), $"Content\\Saves\\Save_{SaveSlot}.txt");
        string text = "";
        using (var outputFile = new StreamReader(filePath))
        text = outputFile.ReadLine();
        if (text != "")
        {
            //SaveGame = new SaveGame(text);
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
        if(StoppedFrames > 0)
        {
            StoppedFrames--;
            return;
        }
        Input.Update();
        Cursor.Update();
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
        if(SaveGame == null || SaveGame.Player == null || SaveGame.Player.IsEnabled)
        {
            VirtualUI.Draw(spriteBatch);
            Cursor.DrawCursor(spriteBatch);
        }
        spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(new Color(50, 50, 50));
        Vector2 backbuffer = BackBuffer;

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, Assets.GlobalShader);
        spriteBatch.Draw(renderTarget, backbuffer / 2, null, Color.White, 0, new Vector2(renderTarget.Bounds.Width, renderTarget.Bounds.Height) / 2, Math.Max(backbuffer.X/1920, backbuffer.Y/1080), 0, 0);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null);
        DialogueManager.Draw(spriteBatch);
        RealUI.Draw(spriteBatch);
        foreach (var exception in ShaderExceptions)
        {
            exception.Draw(spriteBatch);
        }
        Cursor.DrawHand(spriteBatch);
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
