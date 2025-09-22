using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using UILib.Content.Main;
using Space_Wars.Content.Main.Particles;
using Space_Wars.Content.Main.Entities;
using System.IO;
using Space_Wars.Content.Main.Story;

namespace Space_Wars.Content.Main;

public class Engine : Game
{
    private static readonly List<string> debugLog = [];
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private RenderTarget2D renderTarget;
    public static UIManager UIManager { get; private set; }
    public static EntityManager EntityManager { get; private set; }
    public static DialogueManager DialogueManager { get; private set; }
    public static SaveGame SaveGame { get; private set; }
    public static Camera Camera { get; private set; }
    public static Engine Self { get; private set; }
    public static Texture2D Line { get; private set; }
    public static Vector2 ScreenSize => new(1920, 1080); //Render target size
    public static Vector2 BackBuffer { get; private set; } //Monitor size
    public static Vector2 MousePositionOffset { get; set; }
    public static Timespan IngameTime { get; set; } = new();
    public static float DeltaSeconds { get; private set; }
    private readonly float timeScale = 1f;
    private readonly int targetFramerate = 60;
    public static float EnemyHitboxModifier { get; private set; } = 1.2f;
    public static bool DebugMode { get; private set; }
    public static bool PatchedConics { get; private set; } = true;
    public static bool UseShader { get; private set; } = true;
    public static float ScreenShakeFactor { get; private set; } = 0;
    public static int SaveSlot { get; private set; } = 0;

    public Engine()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        Self = this;
    }
    protected override void Initialize()
    {
        base.Initialize();
        Window.Title = ("Orbit Commander");
        graphics.PreferredBackBufferWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
        graphics.PreferredBackBufferHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
        //graphics.IsFullScreen = true;
        graphics.ApplyChanges();

        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1d / (double)(targetFramerate));
        
        BackBuffer = new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
        DebugMode = false;
        Line = new Texture2D(graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
        Line.SetData(new[] { Color.White });

        Camera = new Camera(Vector2.Zero, ScreenSize / 2, 1f, 0);

        UIManager = new UIManager();
        UIManager.BackBuffer = BackBuffer;
        EntityManager = new EntityManager();
        DialogueManager = new DialogueManager();
        AddUIElements();
        CurrentGameState.SwitchState(new MainMenu());
        renderTarget = new RenderTarget2D(GraphicsDevice, (int)ScreenSize.X, (int)ScreenSize.Y);

        IsMouseVisible = false;
    }
    private static void AddUIElements()
    {
        //UI behaviors that need special permission
        UI.PatchedConicsToggle.AddBehaviour(delegate
        {
            PatchedConics = !PatchedConics;
            UI.PatchedConicsToggle.text = $"Patched Conics: {PatchedConics}";
        });
        UI.SingleplayerButton.AddBehaviour(delegate ()
        {
            SaveGame = new();
            EventHandler.UpdateModulesUI();
            Startgame();
        });
        UI.ShaderToggle.AddBehaviour(delegate () { UseShader = !UseShader; UI.ShaderToggle.text = $"Shader: {UseShader}"; });
        UI.PrevSave.AddBehaviour(delegate
        {
            SaveSlot = Math.Clamp(SaveSlot - 1, 0, 10);
            EventHandler.GetSave();
        });
        UI.NextSave.AddBehaviour(delegate
        {
            SaveSlot = Math.Clamp(SaveSlot + 1, 0, 10);
            EventHandler.GetSave();
        });
        UI.SFXSlider.AddBehaviour(delegate ()
        {
            float i = UI.SFXSlider.sliderInterval;
            SoundManager.SFXVolume = i;
            UIManager.SFXVolume = i;
            UI.SFXVolume.text = $"Sound: {Math.Round(i * 100)}%";
        });
        UI.UIScaleSlider.AddBehaviour(delegate ()
        {
            float i = UI.UIScaleSlider.sliderInterval;
            UIManager.UIScale = (i + 1f) * BackBuffer.X / ScreenSize.X;
            UI.UIScale.text = $"UI Scale: {Math.Truncate((i + 1) * 10) / 10}";
        });
        UI.AddUIElements();
    }
    public static void Startgame()
    {
        UIManager.DisableAll();
        ParticleManager.Initialize();
        EntityManager.Initialize();
        SoundManager.Initialize();
        EventHandler.UpdateModulesStatus();
        SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
        SaveGame.CurrentMission.PlayIntroCutscene();
        ScreenShakeFactor = 0;
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        Assets.LoadAssets(Content);
    }
    public static void Save()
    {
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), $"Content\\Saves\\Save_{SaveSlot}.txt");
        using (var outputFile = new StreamWriter(filePath))
        {
            outputFile.WriteLine(SaveGame.Serialize());
        }
        EventHandler.QuitToMenu();
    }
    public static void Load()
    {
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), $"Content\\Saves\\Save_{SaveSlot}.txt");
        string text = "";
        using (var outputFile = new StreamReader(filePath))
        {
            text = outputFile.ReadLine();
        }
        if (text != "")
        {
            SaveGame = new SaveGame(text);
            EventHandler.MissionSelectTrigger();
        }
    }
    protected override void Update(GameTime gameTime)
    {
        Input.Update();
        if (IsActive)
        {
            UIManager.Update();
        }
        SoundManager.Update();
        CurrentGameState.Update();

        if (Input.OldState.IsKeyUp(Keys.OemTilde) && Input.NewState.IsKeyDown(Keys.OemTilde))
        {
            DebugMode = !DebugMode;
        }
        DeltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds * timeScale;
        if(ScreenShakeFactor > 0)
        {
            ScreenShakeFactor -= DeltaSeconds;
        }
        else
        {
            ScreenShakeFactor = 0;
        }
        UI.Timer.text = $"{IngameTime.DrawText}";
        base.Update(gameTime);
    }
    public static Pickup MoveSelectedPickup()
    {
        return UIManager.MoveSelectedIcon() as Pickup;
    }
    public static void WriteLine<T>(T arg)
    {
        String stringLog = arg?.ToString();
        debugLog.Insert(0, $"{stringLog}");
    }
    public static void ShakeScreen(float _val)
    {
        ScreenShakeFactor = Math.Min(ScreenShakeFactor + _val * _val / (ScreenShakeFactor + _val), 1);
    }
    public static void DrawFilledLine(SpriteBatch _spriteBatch, Vector2 _position, Rectangle _sourceRectangle, float _percentFilled, Color _lowerColor, Color _higherColor)
    {
        _spriteBatch.Draw(Line,_position,_sourceRectangle,_lowerColor);
        _spriteBatch.Draw(Line, _position, new Rectangle(_sourceRectangle.Location, new Point((int)(_sourceRectangle.Width * _percentFilled), _sourceRectangle.Height)), _higherColor);
    }
    protected override void Draw(GameTime gameTime)
    {
        Camera.Origin = ScreenSize / 2 - MousePositionOffset;

        //Render to renderTarget
        GraphicsDevice.SetRenderTarget(renderTarget);
        GraphicsDevice.Clear(Color.Black);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, transformMatrix: Camera.Transform);
        CurrentGameState.Draw(spriteBatch);
        spriteBatch.End();

        //Render renderTarget with custom bloom shader
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, Assets.GlobalShader);
        spriteBatch.Draw(renderTarget, new Rectangle(0, 0, (int)BackBuffer.X, (int)BackBuffer.Y), Color.White);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
        DialogueManager.Draw(spriteBatch);
        UIManager.Draw(spriteBatch);
        if (!UIManager.LockMouseInput)
        {
            spriteBatch.Draw(Assets.Get(Sprite.Cursor), new Vector2(Mouse.GetState().X, Mouse.GetState().Y), null, Color.White, 0, Vector2.Zero, UIManager.UIScale / 2, 0, 0.5f);
        }
        if (DebugMode)
        {
            int logCount = debugLog.Count;
            if (logCount > 10)
            {
                logCount = 10;
            }
            for (int i = 0; i < logCount; i++)
            {
                Vector2 textPosition = new(35, 20 + 15 * i * UIManager.UIScale);
                try
                {
                    spriteBatch.DrawString(Assets.TextFont, $"{i + 1}: {debugLog[i]}", textPosition, Color.White, 0, Vector2.Zero, UIManager.UIScale, SpriteEffects.None, 0.45f);
                }
                catch (Exception e)
                {
                    spriteBatch.DrawString(Assets.TextFont, $"{i + 1}: {e.Message}", textPosition, Color.Red, 0, Vector2.Zero, UIManager.UIScale, SpriteEffects.None, 0.45f);
                }
            }
        }
        spriteBatch.End();
        base.Draw(gameTime);
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
public static class Util
{
    public static Random Random { get; } = new();
    public static Vector2 ToUnitVector(float _angle)
    {
        return new Vector2(MathF.Sin(_angle), -MathF.Cos(_angle));
    }
    public static float Lerp(float _valueOne, float _valueTwo, float _length)
    {
        return _valueOne * (1 - _length) + _valueTwo * _length;
    }
    public static float ToAngle(Vector2 _direction)
    {
        //Rotated 90 degrees due to asset rotation
        return MathF.Atan2(_direction.X, -_direction.Y);
    }
    public static float OneToNegOne()
    {
        return Random.NextSingle() * 2 - 1f;
    }
    //Frame independent exponential decay 
    public static float FIED(float _decayPerSecond)
    {
        return MathF.Pow(_decayPerSecond, Engine.DeltaSeconds);
    }
    public static Vector2 RotateVector2(Vector2 v, float a)
    {
        float cos = MathF.Cos(a);
        float sin = MathF.Sin(a);
        return new Vector2(v.X * cos - v.Y * sin, v.X * sin + v.Y * cos);
    }
    public static void Explode(Vector2 _position, Vector2 _velocity, int _damage, float _radius)
    {
        int particles = Random.Next(15, 25);
        for (int i = 0; i < particles; i++)
        {
            float angle = Random.NextSingle() * MathF.PI * 2;
            Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Util.Random.NextSingle() * 2 + 2);
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), 0.25f, _position - _velocity, particleVelocity + _velocity, angle, 0, Color.Yellow, new Color(255, 0, 0, 0)));
        }
        particles = Random.Next(8, 16);
        for (int i = 0; i < particles; i++)
        {
            float angle = Random.NextSingle() * MathF.PI * 2;
            Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Util.Random.NextSingle() * 2 + 2);
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.25f, _position - _velocity, particleVelocity + _velocity, angle, 0, Color.DarkSlateGray, Color.Transparent));
        }
        Engine.EntityManager.Explode(_damage, _radius, _position);
        Engine.ShakeScreen(150 / ((_position - Engine.Camera.Position).Length() + 300));
    }
}
