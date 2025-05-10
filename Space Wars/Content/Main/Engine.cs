using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using UILib.Content.Main;
using Space_Wars.Content.Main.Particles;
using Space_Wars.Content.Main.Entities;
using System.Linq;
using System.Diagnostics;

namespace Space_Wars.Content.Main;

public class Engine : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private Decal fpsCounter;
    private Decal fpsOneSec;
    private Decal fpsLowest;
    private List<float> fpsSamples = new();
    private Decal timer;
    public static UIManager UIManager { get; private set; }
    public static EntityManager EntityManager { get; private set; }
    public static Camera Camera { get; private set; }
    public static Vector2 ScreenSize { get; private set; }
    public static Vector2 MousePositionOffset { get; set; }
    public static Timespan IngameTime { get; set; } = new();
    public static float DeltaSeconds { get; private set; }
    public static readonly float timeScale = 1f;
    public static readonly int targetFramerate = 60;
    public static readonly float enemyHitboxModifier = 1.2f;
    public static Texture2D Line { get; private set; }
    public static bool DebugMode { get; private set; }
    public static bool PatchedConics { get; private set; } = true;
    public static readonly List<string> debugLog = new();
    public static float UIScale { get; private set; } = 2f;
    public static ItemSlot<Module>[] ModuleSlots { get; private set; }
    public static ItemSlot<Pickup>[,] InventorySlots { get; private set; }
    public static float ScreenShakeFactor { get; private set; } = 0;
    public static Engine Self { get; private set; }

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
        TargetElapsedTime = TimeSpan.FromSeconds(1d / (double)(targetFramerate * timeScale));

        ScreenSize = new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
        DebugMode = false;
        Line = new Texture2D(graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
        Line.SetData(new[] { Color.White });

        Camera = new Camera(GraphicsDevice.Viewport)
        {
            Origin = ScreenSize / 2,
            Zoom = new Vector2(1f, 1f),
            Position = Vector2.Zero
        };

        UIManager = new UIManager();
        EntityManager = new EntityManager();
        AddUIElements();
        CurrentGameState.SwitchState(new MainMenu());

        IsMouseVisible = false;
    }
    private void AddUIElements()
    {
        Texture2D largePanel = Assets.Get(Sprite.LargePanel);
        Texture2D wideButton = Assets.Get(Sprite.WideButton);
        List<Texture2D> iconGroup1 = new() { Assets.Get(Sprite.PlayIcon), Assets.Get(Sprite.SettingsIcon) };
        List<Texture2D> iconGroup2 = new() { Assets.Get(Sprite.SmeltIcon), Assets.Get(Sprite.RepairIcon), Assets.Get(Sprite.VictoryIcon) };
        List<Texture2D> iconGroup3 = new() { Assets.Get(Sprite.PlanetIcon), Assets.Get(Sprite.RepairIcon) };

        var tabTexture = Assets.Get(Sprite.Tab);
        var selectedTabTexture = Assets.Get(Sprite.SelectedTab);
        var selectSound = Assets.Get(Sound.Interact);
        var PauseMenu = new Window(ScreenSize / 2, largePanel, 1) { enabled = false };
        var PlayerMenu = new Window(new Vector2(Assets.Get(Sprite.Terminal).Width, ScreenSize.Y / 2), Assets.Get(Sprite.Terminal), 1) { enabled = false };
        var GarageMenu = new Window(ScreenSize / 2, Assets.Get(Sprite.GargantuanPanel), 1) { enabled = false };
        var MainMenu = new TabbedWindow(ScreenSize / 2, Assets.Get(Sprite.GargantuanPanel), tabTexture, selectedTabTexture, selectSound, 2, 1) { enabled = true, icons = iconGroup1 };
        var MothershipMenu = new TabbedWindow(new Vector2(Assets.Get(Sprite.Terminal).Width, ScreenSize.Y / 2), Assets.Get(Sprite.Terminal), tabTexture, selectedTabTexture, selectSound, 3, 1) { enabled = false, icons = iconGroup2 };
        var MissionSelect = new TabbedWindow(ScreenSize / 2, Assets.Get(Sprite.GargantuanPanel), tabTexture, selectedTabTexture, selectSound, 2) { enabled = false, icons = iconGroup3 };

        var patchedConicsToggle = new Button(new Vector2(0, -MainMenu.Size.Y / 4), wideButton, Assets.TextFont, $"Patched Conics: {PatchedConics}", Color.White);
        var sfxSlider = new Slider(Line, Assets.Get(Sprite.Knob), new Vector2(25, 0), 50, false, Color.White, Color.Gray);
        var musicSlider = new Slider(Line, Assets.Get(Sprite.Knob), new Vector2(25, -15), 50, false, Color.White, Color.Gray);
        sfxSlider.SetInterval(1, 1);
        musicSlider.SetInterval(1, 0);
        var sfxVolume = new Decal(new Vector2(-30, 0), Assets.TextFont, "Sound: 100%", Color.White, 5);
        var musicVolume = new Decal(new Vector2(-30, -15), Assets.TextFont, "Music: 100%", Color.White, 5);

        var singleplayerButton = new Button(new Vector2(0, -MainMenu.Size.Y / 4), wideButton, Assets.TextFont, "Singleplayer", Color.White);
        var exitButton = new Button(new Vector2(0, MainMenu.Size.Y / 4), wideButton, Assets.TextFont, "Exit", Color.White);
        var quitToMenuButton = new Button(new Vector2(0, 20), wideButton, Assets.TextFont, "Quit to Menu", Color.White);
        var quitToMissionButton = new Button(new Vector2(0, -20), wideButton, Assets.TextFont, "Return", Color.White);
        var titleName = new Decal(new Vector2(0, -MainMenu.Size.Y) - Assets.DimsOf(Sprite.Title) / 2, Assets.Get(Sprite.Title));

        var furnaceSlot = new ItemSlot<Pickup>(new Vector2(-20, 0), Assets.Get(Sprite.EmptySlot), UIManager, -1);
        var garageButton = new Button(new Vector2(0, -MainMenu.Size.Y / 4), wideButton, Assets.TextFont, "To Garage", Color.White);
        var craftButton = new Button(new Vector2(0, MainMenu.Size.Y / 4), Assets.Get(Sprite.Button), Assets.TextFont, "Repair", Color.LightBlue);
        var requiredCraftsText = new Decal(new Vector2(0) + new Vector2(0, -6), Assets.TextFont, "25", Color.White, 10);
        var furnaceSlider = new Slider(Line, new Vector2(-50, -MainMenu.Size.Y / 6), 60, true, new Color(255, 239, 85), new Color(50, 51, 67));
        var craftingSlider = new Slider(Line, new Vector2(0 - 30, -MainMenu.Size.Y / 4), 60, true, Color.Cyan, Color.Gray);

        var repairSlot = new ItemSlot<Module>(new Vector2(-GarageMenu.Size.X / 4 - 25, 0), Assets.Get(Sprite.EmptySlot), UIManager, -1);
        var mothershipScrap = new Decal(new Vector2(GarageMenu.Size.X / 2.2f, 20) - GarageMenu.Size / 2, Assets.TextFont, "0", Color.Gray, 10);
        var repairText = new Decal(new Vector2(-GarageMenu.Size.X / 4 - 60 / 2.5f, 40), Assets.TextFont, "", Color.White, 10);
        var garagePlayerImage = new Decal(new Vector2(GarageMenu.Size.X / 4, 0) - Assets.DimsOf(Sprite.PlayerUI) / 2, Assets.Get(Sprite.PlayerUI));
        var validConfigText = new Decal(-GarageMenu.Size / 4 + new Vector2(20, GarageMenu.Size.Y / 1.5f), Assets.TextFont, "Ready for Combat", Color.Green, 10);
        var repairButton = new Button(new Vector2(-GarageMenu.Size.X / 4 - 25, -40), Assets.Get(Sprite.Button), Assets.TextFont, "Repair", Color.LightBlue);

        var enemySlider = new Slider(Line, new Vector2(-30, -PlayerMenu.Size.Y / 3), 50, true, Color.White, Color.Gray);
        var waveText = new Decal(new Vector2(-5, 0), Assets.TextFont, "0", Color.White, 10);
        var partStatus = new Decal(new Vector2(0, 20), Assets.TextFont, "All systems go", Color.Green, 10);
        var restartButton = new Button(new Vector2(0, 50), Assets.Get(Sprite.Button), Assets.TextFont, "Restart", Color.LightBlue);
        var restartSlider = new Slider(Line, new Vector2(-25, 63), 50, true, Color.Cyan, Color.Black);

        var missionName = new Decal(new Vector2(0, -30), Assets.TextFont, "Name", Color.White, 10);
        var missionDescription = new Decal(new Vector2(0, -15), Assets.TextFont, "Description", Color.Gray, 3f);
        var prevMission = new Button(new Vector2(-75, 20), Assets.Get(Sprite.Button), Assets.TextFont, "Prev", Color.LightBlue);
        var nextMission = new Button(new Vector2(75, 20), Assets.Get(Sprite.Button), Assets.TextFont, "Next", Color.LightBlue);
        var selectMission = new Button(new Vector2(0, 20), Assets.Get(Sprite.Button), Assets.TextFont, "Launch!", Color.Yellow);
        var isComplete = new Decal(new Vector2(0, 45), Assets.TextFont, "Not Complete", Color.Red, 10);

        var globalSidePanelOpen = new Button(new Vector2(Assets.Get(Sprite.ToggleButton).Width / 2, ScreenSize.Y / 4), Assets.Get(Sprite.ToggleButton));
        fpsCounter = new Decal(new Vector2(ScreenSize.X / 2 - 20, 10), Assets.TextFont, "60", Color.White, 10);
        fpsOneSec = new Decal(new Vector2(ScreenSize.X / 2 - 20, 23), Assets.TextFont, "60", Color.White, 10);
        fpsLowest = new Decal(new Vector2(ScreenSize.X / 2 - 20, 36), Assets.TextFont, "60", Color.White, 10);
        timer = new Decal(new Vector2(ScreenSize.X / 2 - 100, 10), Assets.TextFont, $"{IngameTime.DrawText}", Color.White, 10);
        var sidePanelClose = new Button(new Vector2(-Assets.Get(Sprite.ToggleButton).Width / 2 + Assets.Get(Sprite.Terminal).Width / 2, 0), Assets.Get(Sprite.ToggleButton));

        patchedConicsToggle.AddBehaviour(delegate
        {
            PatchedConics = !PatchedConics;
            patchedConicsToggle.text = $"Patched Conics: {PatchedConics}";
        });

        exitButton.AddBehaviour(delegate ()
        {
            Exit();
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
        });
        sfxSlider.AddBehaviour(delegate ()
        {
            float i = sfxSlider.sliderInterval;
            SoundManager.SFXVolume = i;
            UIManager.SFXVolume = i;
            sfxVolume.text = $"Sound: {Math.Round(i * 100)}%";
        });
        musicSlider.AddBehaviour(delegate ()
        {
            float i = musicSlider.sliderInterval;
            SoundManager.MusicVolume = i;
            musicVolume.text = $"Music: {Math.Round(i * 100)}%";
        });

        singleplayerButton.AddBehaviour(EventHandler.MissionSelectTrigger);
        quitToMenuButton.AddBehaviour(EventHandler.QuitToMenu);
        quitToMissionButton.AddBehaviour(EventHandler.MissionSelectTrigger);
        garageButton.AddBehaviour(EventHandler.GarageTrigger);
        repairButton.AddBehaviour(EventHandler.RepairModule);
        craftButton.AddBehaviour(EventHandler.CraftItem);
        repairSlot.AddBehaviour(EventHandler.UpdateRepairText);
        furnaceSlot.AddBehaviour(EventHandler.UpdateFurnace);
        restartButton.AddBehaviour(EventHandler.RestartModules);
        globalSidePanelOpen.AddBehaviour(EventHandler.ToggleDockingMenus);
        sidePanelClose.AddBehaviour(EventHandler.ToggleDockingMenus);
        prevMission.AddBehaviour(EntityManager.PrevMission);
        nextMission.AddBehaviour(EntityManager.NextMission);
        selectMission.AddBehaviour(Startgame);

        MainMenu.AddWidget(exitButton as IFunctional, 0);
        MainMenu.AddWidget(singleplayerButton as IFunctional, 0);
        MainMenu.AddWidget(titleName, 0);
        MainMenu.AddWidget(titleName, 1);
        MainMenu.AddWidget(patchedConicsToggle as IFunctional, 1);
        MainMenu.AddWidget(sfxSlider as IFunctional, 1);
        MainMenu.AddWidget(musicSlider as IFunctional, 1);
        MainMenu.AddWidget(sfxVolume, 1);
        MainMenu.AddWidget(musicVolume, 1);

        PauseMenu.AddWidget(quitToMenuButton as IFunctional);
        PauseMenu.AddWidget(quitToMissionButton as IFunctional);

        GarageMenu.AddWidget(mothershipScrap);
        GarageMenu.AddWidget(repairButton as IFunctional);
        GarageMenu.AddWidget(repairSlot as IFunctional);
        GarageMenu.AddWidget(repairText);
        GarageMenu.AddWidget(garagePlayerImage);
        GarageMenu.AddWidget(validConfigText);

        MothershipMenu.AddWidget(furnaceSlider as IFunctional, 0);
        MothershipMenu.AddWidget(furnaceSlot as IFunctional, 0);
        MothershipMenu.AddWidget(garageButton as IFunctional, 1);
        MothershipMenu.AddWidget(craftingSlider as IFunctional, 2);
        MothershipMenu.AddWidget(requiredCraftsText, 2);
        MothershipMenu.AddWidget(craftButton as IFunctional, 2);

        PlayerMenu.AddWidget(enemySlider as IFunctional);
        PlayerMenu.AddWidget(waveText);
        PlayerMenu.AddWidget(partStatus);
        PlayerMenu.AddWidget(restartButton as IFunctional);
        PlayerMenu.AddWidget(restartSlider as IFunctional);
        PlayerMenu.AddWidget(sidePanelClose as IFunctional);

        MissionSelect.AddWidget(missionName, 0);
        MissionSelect.AddWidget(missionDescription, 0);
        MissionSelect.AddWidget(prevMission as IFunctional, 0);
        MissionSelect.AddWidget(nextMission as IFunctional, 0);
        MissionSelect.AddWidget(selectMission as IFunctional, 0);
        MissionSelect.AddWidget(isComplete, 0);

        for (int i = 0; i < 3; i++)
        {
            MothershipMenu.AddWidget(sidePanelClose as IFunctional, i);
        }

        UIManager.AddGlobalWidget(globalSidePanelOpen as IFunctional);
        UIManager.AddGlobalWidget(fpsCounter);
        UIManager.AddGlobalWidget(fpsOneSec);
        UIManager.AddGlobalWidget(fpsLowest);
        UIManager.AddGlobalWidget(timer);

        ModuleSlots = new ItemSlot<Module>[5];
        InventorySlots = new ItemSlot<Pickup>[1, 4];
        for (int x = 0; x < ModuleSlots.GetLength(0); x++)
        {
            if (x % 2 == 0)
            {
                ModuleSlots[x] = new ItemSlot<Module>(new Vector2(-30,Assets.DimsOf(Sprite.EmptySlot).Y * x / 2 
                    - Assets.DimsOf(Sprite.EmptySlot).Y), Assets.Get(Sprite.EmptySlot), UIManager, x);
            }
            else
            {
                ModuleSlots[x] = new ItemSlot<Module>(new Vector2(Assets.DimsOf(Sprite.EmptySlot).X / 1.4142f - 30,
                    Assets.DimsOf(Sprite.EmptySlot).Y * x / 2 - Assets.DimsOf(Sprite.EmptySlot).Y), Assets.Get(Sprite.EmptySlot), UIManager, x);
            }
            GarageMenu.AddWidget(ModuleSlots[x] as IFunctional);
            MissionSelect.AddWidget(ModuleSlots[x] as IFunctional, 1);
            ModuleSlots[x].AddBehaviour(new Action(EventHandler.UpdateModules));
        }
        for (int x = 0; x < InventorySlots.GetLength(0); x++)
        {
            for (int y = 0; y < InventorySlots.GetLength(1); y++)
            {
                InventorySlots[x, y] = new ItemSlot<Pickup>(new Vector2(Assets.DimsOf(Sprite.EmptySlot).X * x + Assets.DimsOf(Sprite.LargePanel).X / 4,
                    Assets.DimsOf(Sprite.EmptySlot).Y * (y+1) - Assets.DimsOf(Sprite.LargePanel).X / 2), Assets.Get(Sprite.EmptySlot), UIManager, -1);
                MothershipMenu.AddWidget(InventorySlots[x, y] as IFunctional, 0);
                InventorySlots[x, y].AddBehaviour(new Action(EventHandler.UpdateInventory));
            }
        }

        UIManager.AddContainer(MainMenu);
        UIManager.AddContainer(PauseMenu);
        UIManager.AddContainer(PlayerMenu);
        UIManager.AddContainer(MothershipMenu);
        UIManager.AddContainer(GarageMenu);
        UIManager.AddContainer(MissionSelect);
    }
    public static void Startgame()
    {
        UIManager.DisableAll();
        ParticleManager.Initialize();
        Player player = EntityManager.Initialize();
        EventHandler.InitializeGameSpace(player);
        SoundManager.Initialize(player);
        EventHandler.UpdateModulesStatus();
        SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
        SoundManager.ChangeTrack(Assets.Get(Sound.main));
        EntityManager.CurrentMission.PlayIntroCutscene();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        Assets.LoadAssets(Content);
    }

    protected override void Update(GameTime gameTime)
    {
        Input.Update();
        UIManager.Update();
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
        if (DebugMode)
        {
            fpsSamples.Add(DeltaSeconds);
            if (fpsSamples.Count > 60)
            {
                fpsSamples.RemoveAt(0);
            }
            fpsCounter.text = $"{(int)(1 / DeltaSeconds)}";
            float average = 0;
            foreach (var sample in fpsSamples)
            {
                average += sample;
            }
            average /= 60;
            fpsOneSec.text = $"{(int)(1 / average)}";
            fpsLowest.text = $"{(int)(1 / fpsSamples.Max())}";
            timer.text = $"{IngameTime.DrawText}";
        }
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
        ScreenShakeFactor += _val * _val / (ScreenShakeFactor + _val);
    }
    public static Vector2 ToUnitVector(float _angle)
    {
        return new Vector2(MathF.Sin(_angle), -MathF.Cos(_angle));
    }
    public static float Lerp(float _valueOne, float _valueTwo, float _length)
    {
        return _valueOne * (1 - _length) + _valueTwo * _length;
    }
    public static void DrawFilledLine(SpriteBatch _spriteBatch, Vector2 _position, Rectangle _sourceRectangle, float _percentFilled, Color _lowerColor, Color _higherColor)
    {
        _spriteBatch.Draw(Line,_position,_sourceRectangle,_lowerColor);

        _spriteBatch.Draw(Line, _position, new Rectangle(_sourceRectangle.Location, new Point((int)(_sourceRectangle.Width * _percentFilled), _sourceRectangle.Height)), _higherColor);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        Camera.Origin = ScreenSize / 2 - MousePositionOffset;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, transformMatrix: Camera.ViewMatrix);
        CurrentGameState.Draw(spriteBatch);
        spriteBatch.End();
        
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
        UIManager.Draw(spriteBatch);
        if (!UIManager.LockMouseInput)
        {
            spriteBatch.Draw(Assets.Get(Sprite.Cursor), new Vector2(Mouse.GetState().X, Mouse.GetState().Y), null, Color.White, 0, Vector2.Zero, 1, 0, 0.5f);
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
                Vector2 textPosition = new(35, 20 + 15 * i * UIScale);
                try
                {
                    spriteBatch.DrawString(Assets.TextFont, $"{i + 1}: {debugLog[i]}", textPosition, Color.White, 0, Vector2.Zero, UIScale, SpriteEffects.None, 0.45f);
                }
                catch (Exception e)
                {
                    spriteBatch.DrawString(Assets.TextFont, $"{i + 1}: {e.Message}", textPosition, Color.Red, 0, Vector2.Zero, UIScale, SpriteEffects.None, 0.45f);
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
    public readonly float Seconds
    {
        get { return Duration % 60; }
    }
    public readonly float Minutes
    {
        get { return (int)(Duration / 60) % 60; }
    }
    public readonly float Hours
    {
        get { return (int)(Duration / 3600); }
    }
    public readonly string DrawText
    {
        get { return $"{Hours:00}:{Minutes:00}:{Seconds:00.00}"; }
    }
}
