using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OrbitCommander.Components;
using OrbitCommander.Entities;
using OrbitCommander.UIElements;
using UILib.Content;

namespace OrbitCommander.Core;
public static class UI
{
    private static Color color = new Color(26, 26, 26);
    //Real Windows
    public static TabbedWindow TerminalMenu { get; } = new TabbedWindow(new Vector2(0, -1), Assets.Get(Sprites.Terminal), null, null, null, null, Engine.RealUI) { Alignment = Alignment.Top, CurrentTab = 1 }; //0: Player / Fuse menu, 1: Keybind / Settings menu, 2: Floppy menu

    //Virtual Windows
    public static TabbedWindow MissionSelect { get; } = new TabbedWindow(new Vector2(-1, 0), Assets.Get(Sprites.GargantuanPanel), Assets.Get(Sprites.Tab), Assets.Get(Sprites.SelectedTab), Assets.Get(Sound.Interact), 2, Engine.VirtualUI)
    { icons = [Assets.Get(Sprites.PlanetIcon), Assets.Get(Sprites.RepairIcon)], Alignment = Alignment.Left };
    public static Window MothershipMenu { get; } = new Window(new Vector2(0, -0.5f), Assets.Get(Sprites.LargePanel), Engine.VirtualUI);
    public static Window DebugMenu { get; } = new Window(new Vector2(-1, 1), Assets.Get(Sprites.GargantuanPanel), Engine.VirtualUI) { Alignment = Alignment.BottomLeft };
    public static Window HackMenu { get; } = new Window(new Vector2(0, -0.5f), Assets.Get(Sprites.LargePanel), Engine.VirtualUI);
    public static Window EscapeMenu { get; } = new Window(new Vector2(0, -0.5f), Assets.Get(Sprites.LargePanel), Engine.VirtualUI);
    public static Screen GameplayGlobals { get; } = new Screen(Engine.VirtualUI);
    public static Screen MainGlobals { get; } = new Screen(Engine.VirtualUI);
    public static TabbedWindow UpgradeMenu { get; } = new TabbedWindow(Vector2.Zero, Assets.Get(Sprites.GargantuanPanel),
    Assets.Get(Sprites.Tab), Assets.Get(Sprites.SelectedTab), Assets.Get(Sound.Interact), 2, Engine.VirtualUI);
    public static Window PauseMenu { get; } = new Window(Vector2.Zero, Assets.Get(Sprites.LargePanel), Engine.VirtualUI);
    public static Window PickupDroneMenu { get; } = new Window(new Vector2(0, -0.5f), Assets.Get(Sprites.LargePanel), Engine.VirtualUI);
    public static TabbedWindow SavesMenu { get; } = new TabbedWindow(Vector2.Zero, Assets.Get(Sprites.GargantuanPanel), null, null, null, null, Engine.VirtualUI);
    public static TabbedWindow TerminalScreen { get; } = new TabbedWindow(new Vector2(0, -1), Assets.Get(Sprites.TerminalScreen), null, null, null, null, Engine.VirtualUI) { Alignment = TerminalMenu.Alignment };

    //Settings
    public static Button PatchedConicsToggle { get; } = new Button(new Vector2(TerminalMenu.Size.X/ 2.5f, -TerminalMenu.Size.Y/2 * 8 / 10), Assets.Get(Sprites.SwitchOn), TerminalMenu, [0], Assets.Get(Sprites.SwitchOff));
    public static Button ShaderToggle { get; } = new Button(new Vector2(TerminalMenu.Size.X / 2.5f, -TerminalMenu.Size.Y / 2 * 6 / 10), Assets.Get(Sprites.SwitchOn), TerminalMenu, [0], Assets.Get(Sprites.SwitchOff));
    public static Decal PatchedConicsText { get; } = new Decal(new Vector2(TerminalMenu.Size.X / 3f, -TerminalMenu.Size.Y / 2 * 8 / 10), Assets.Ithica, "Patched Conics", Color.White, 5, TerminalMenu, [0]) { WidgetAlignment = Alignment.Right};
    public static Decal ShaderText { get; } = new Decal(new Vector2(TerminalMenu.Size.X / 3f, -TerminalMenu.Size.Y / 2 * 6 / 10), Assets.Ithica, "Shader", Color.White, 5, TerminalMenu, [0]) { WidgetAlignment = Alignment.Right };
    public static Slider SFXSlider { get; } = new Slider(Engine.Line, Assets.Get(Sprites.Knob), new Vector2(TerminalMenu.Size.X / 2.5f, -TerminalMenu.Size.Y / 2 * 4 / 10), new Vector2(50, 5), false, [color, color], TerminalMenu, [0]);
    public static Slider MusicSlider { get; } = new Slider(Engine.Line, Assets.Get(Sprites.Knob), new Vector2(TerminalMenu.Size.X / 2.5f, -TerminalMenu.Size.Y / 2 * 3 / 10), new Vector2(50, 5), false, [color, color], TerminalMenu, [0]);
    public static Slider UIScaleSlider { get; } = new Slider(Engine.Line, Assets.Get(Sprites.Knob), new Vector2(TerminalMenu.Size.X / 2.5f, -TerminalMenu.Size.Y / 2 * 2 / 10), new Vector2(50, 5), false, [color, color], TerminalMenu, [0]);
    public static Decal SFXVolume { get; } = new Decal(new Vector2(TerminalMenu.Size.X / 5f, -TerminalMenu.Size.Y / 2 * 4 / 10), Assets.Ithica, "Sound: 100%", Color.White, 5, TerminalMenu, [0]);
    public static Decal MusicVolume { get; } = new Decal(new Vector2(TerminalMenu.Size.X / 5f, -TerminalMenu.Size.Y / 2 * 3 / 10), Assets.Ithica, "Music: 100%", Color.White, 5, TerminalMenu, [0]);
    public static Decal UIScale { get; } = new Decal(new Vector2(TerminalMenu.Size.X / 5f, -TerminalMenu.Size.Y / 2 * 2 / 10), Assets.Ithica, $"UI Scale: {Math.Truncate((UIScaleSlider.Intervals[0] + 1) * 10) / 10}", Color.White, 5, TerminalMenu, [0]);
    public static Decal WindowType { get; } = new Decal(new Vector2(TerminalMenu.Size.X / 3.5f, 0), null, Assets.Ithica, "Borderless Window", Color.White, 6, TerminalMenu, [0]);
    public static Button NextResolution { get; } = new Button(new Vector2(TerminalMenu.Size.X / 3.5f, TerminalMenu.Size.Y / 2 / 10), Assets.Get(Sprites.Plate), Assets.Ithica, "Next Resolution", Color.White, 6, TerminalMenu, [0]);
    public static Decal Resolution { get; } = new Decal(new Vector2(TerminalMenu.Size.X / 3.5f, TerminalMenu.Size.Y / 2 * 2 / 10), null, Assets.Ithica, "1920 x 1080", Color.White, 6, TerminalMenu, [0]);
    public static Button NextWindowType { get; } = new Button(new Vector2(TerminalMenu.Size.X / 3.5f, TerminalMenu.Size.Y / 2 * 3 / 10), Assets.Get(Sprites.Plate), Assets.Ithica, "Next Window", Color.White, 6, TerminalMenu, [0]);
    public static Button ApplyChanges { get; } = new Button(new Vector2(TerminalMenu.Size.X / 3.5f, TerminalMenu.Size.Y / 2 * 6 / 10), Assets.Get(Sprites.Plate), Assets.Ithica, "Apply changes", Color.White, 6, TerminalMenu, [0]);

    //Terminal Screen
    public static Slider EnemySlider { get; } = new Slider(Engine.Line, new Vector2(0, -TerminalMenu.Size.Y / 3), new Vector2(50, 2), true, [Color.White, Color.Gray], TerminalScreen, [1]);
    public static Slider[] HealthSliders { get; } = new Slider[5];
    public static Decal WaveText { get; } = new Decal(new Vector2(-20, 0), Assets.Rasterforge, "0", Color.White, 10, TerminalScreen, [1]);
    public static Decal EnemiesLeft { get; } = new Decal(new Vector2(0, 0), Assets.Rasterforge, "0", Color.Red, 10, TerminalScreen, [1]);

    //Fuse Menu
    public static Decal[] StatusLights { get; } = new Decal[5];
    public static Slider RestartSwitch { get; } = new Slider(Engine.Line, new Vector2(TerminalMenu.Size.X * 3 / 10, TerminalMenu.Size.Y * 3/10), Assets.DimsOf(Sprites.SwitchFive), false, [Color.Transparent, Color.Transparent], TerminalMenu, [1, 2]);
    public static Decal Switch { get; } = new Decal(RestartSwitch.Offset / UIManager.UIScale, Assets.Get(Sprites.SwitchFive), TerminalMenu, [1, 2]);
    public static UIElements.Stack<Fuse> FuseCounter { get; } = new UIElements.Stack<Fuse>(new Vector2(TerminalMenu.Size.X * 3 / 10, TerminalMenu.Size.Y / 5), Assets.Get(Sprites.Plate), 1, Assets.Get(Sprites.Fuse), 
        new Vector2(-Assets.Get(Sprites.Button).Width / 2 * 4 / 5, 0), new Vector2(8, 0), delegate { return new Fuse(Color.White); }, TerminalMenu, [1, 2]);
    public static ItemSlot<Fuse>[] Fuses { get; } = new ItemSlot<Fuse>[10];
    public static Dial[] ModulePower { get; } = new Dial[5];
    public static Decal[] ModuleIcons { get; } = new Decal[5];
    public static Decal FuseDetailing { get; } = new Decal(Vector2.Zero, Assets.Get(Sprites.FuseDetailing), TerminalMenu, [1, 2]);
    public static Dial FuseDial { get; } = new Dial(Assets.Get(Sprites.Indicator), new Vector2(100, -58), Assets.Get(Sprites.Dial), TerminalMenu, [1, 2]) { IsTurnable = false, Range = MathF.PI/2 * 9/10};
    public static Decal FuseText { get; } = new Decal(FuseDial.Offset / UIManager.UIScale + new Vector2(0, 5), Assets.Rasterforge, "Power", Color.White, 5, TerminalMenu, [1, 2]);

    //Restart Terminal
    public static Decal DeadFile { get; } = new Decal(new Vector2(-10, 0), Assets.Get(Sprites.DeadFile), TerminalMenu, [2]);

    //Terminal All
    public static Button TerminalClose { get; } = new Button(new Vector2(0, Assets.Get(Sprites.Terminal).Height / 2 - Assets.Get(Sprites.TerminalSlider).Height / 2), Assets.Get(Sprites.TerminalSlider), TerminalMenu, [0, 1, 2]);
    public static Decal Overlay { get; private set; }

    //Main Menu
    public static TerminalButton SingleplayerButton { get; } = new TerminalButton(new Vector2(0, 0), Assets.Rasterforge, "Singleplayer", Color.White, 10, MainGlobals, [(int)Alignment.TopLeft]);
    public static TerminalButton ExitButton { get; } = new TerminalButton(new Vector2(0, 40), Assets.Rasterforge, "Exit", Color.White, 10, MainGlobals, [(int)Alignment.TopLeft]);
    public static TerminalButton LoadButton { get; } = new TerminalButton(new Vector2(0, 20), Assets.Rasterforge, "Load", Color.White, 10, MainGlobals, [(int)Alignment.TopLeft]);

    //Gameplay Menu
    public static Decal Timer { get; } = new Decal(new Vector2(-50, 0), Assets.Rasterforge, $"{Engine.IngameTime.DrawText}", Color.White, 10, GameplayGlobals, [(int)Alignment.TopRight]);
    public static Slider PlayerHealth { get; } = new Slider(Engine.Line, new Vector2(5, 5), new Vector2(150, 15), true, [Color.Red, Color.White, new Color(0.2f, 0.2f, 0.2f)], GameplayGlobals, [(int)Alignment.TopLeft]);
    public static Slider PlayerSpecialHealth { get; } = new Slider(Engine.Line, new Vector2(5, 5), new Vector2(150, 15), true, [Color.Transparent, Color.Transparent], GameplayGlobals, [(int)Alignment.TopLeft]);
    public static Slider PlayerAmmo { get; } = new Slider(Engine.Line, new Vector2(5, 15), new Vector2(100, 2), true, [Color.Yellow, Color.DarkGray], TerminalScreen, [1]);
    public static Slider PlayerAbility { get; } = new Slider(Engine.Line, new Vector2(5, 15), new Vector2(100, 10), true, [Color.Cyan, Color.DarkGray], TerminalScreen, [1]);
    public static Slider Thermometer { get; } = new Slider(Engine.Line, new Vector2(0, 15), new Vector2(100, 10), true, [new Color(25, 25, 25), Color.Transparent, new Color(25, 25, 25)], TerminalScreen, [1]);

    public static TerminalButton[] NextModule { get; } = new TerminalButton[5];
    public static TerminalButton[] PrevModule { get; } = new TerminalButton[5];
    public static Decal[] ModuleSelection { get; } = new Decal[5];
    //TODO: Readd keybind inputs
    public static readonly List<Keys> keys = [Keys.W, Keys.S, Keys.A, Keys.D, Keys.Space, Keys.C, Keys.E, Keys.I, Keys.LeftControl, Keys.F, Keys.Q, Keys.RightShift, Keys.LeftShift, Keys.R, Keys.Escape, Keys.Tab];
    public static readonly List<String> text = ["Up", "Down", "Right", "Left", "Undock", "Drop Scrap", "Swap Primary", "", "Toggle Aim Assist", "Drop Scrap", "Ability", "Warp Back", "Warp Forward", "Reload", "Pause", "Tab"];
    public static Decal[] KeybindTexts { get; } = new Decal[keys.Count];
    public static TerminalButton[] KeybindInputs { get; } = new TerminalButton[keys.Count];

    //Debug Menu
    public static Button SetModules { get; } = new Button(new Vector2(0, 50), Assets.Get(Sprites.Button), Assets.Rasterforge, "Apply", Color.White, 10, DebugMenu, [0]);

    //Pause Menu
    public static Button AbortButton { get; } = new Button(new Vector2(0, -20), Assets.Get(Sprites.WideButton), Assets.Rasterforge, "Abort", Color.White, 10, PauseMenu, [0]);

    //Mothership Menu
    public static ItemSlot<Pickup> FurnaceSlot { get; } = new ItemSlot<Pickup>(new Vector2(-MothershipMenu.Size.X / 3, MothershipMenu.Size.Y / 3), Assets.Get(Sprites.EmptySlot), -1, MothershipMenu, [0]);
    public static Decal RequiredCraftsText { get; } = new Decal(new Vector2(-MothershipMenu.Size.X / 6 + Assets.DimsOf(Sprites.EmptySlot).X * 0.5f, MothershipMenu.Size.Y / 3), Assets.Rasterforge, "25", Color.White, 10, MothershipMenu, [0]);
    public static Slider FurnaceSlider { get; } = new Slider(Engine.Line, new Vector2(-MothershipMenu.Size.X / 6 - Assets.DimsOf(Sprites.EmptySlot).X / 4f, MothershipMenu.Size.Y / 6), new Vector2(60, 3), true, [new Color(255, 239, 85), new Color(50, 51, 67)], MothershipMenu, [0]);

    //Mission Select Menu
    public static Decal MissionName { get; } = new Decal(new Vector2(0, -30), Assets.Rasterforge, "Name", Color.White, 10, MissionSelect, [0]);
    public static Decal MissionDescription { get; } = new Decal(new Vector2(0, -15), Assets.Rasterforge, "Description", Color.Gray, 3f, MissionSelect, [0]);
    public static Button PrevMission { get; } = new Button(new Vector2(-75, 20), Assets.Get(Sprites.Button), Assets.Rasterforge, "Prev", Color.LightBlue, 10, MissionSelect, [0]);
    public static Button NextMission { get; } = new Button(new Vector2(75, 20), Assets.Get(Sprites.Button), Assets.Rasterforge, "Next", Color.LightBlue, 10, MissionSelect, [0]);
    public static Button SelectMission { get; } = new Button(new Vector2(0, 20), Assets.Get(Sprites.Button), Assets.Rasterforge, "Launch!", Color.Yellow, 10, MissionSelect, [0]);
    public static Decal IsComplete { get; } = new Decal(new Vector2(0, 45), Assets.Rasterforge, "Not Complete", Color.Red, 10, MissionSelect, [0]);
    public static Button SaveButton { get; } = new Button(new Vector2(0, -60), Assets.Get(Sprites.WideButton), Assets.Rasterforge, "Save & Exit", Color.LightBlue, 10, MissionSelect, [0]);
    public static Decal AlertText { get; } = new Decal(new Vector2(0, 60), Assets.Rasterforge, "", Color.Yellow, 10, MissionSelect, [0]);
    public static Button ExitWithoutSave { get; } = new Button(new Vector2(0, -80), Assets.Get(Sprites.WideButton), Assets.Rasterforge, "Exit", Color.White, 10, MissionSelect, [0]);
    public static Button CreateFuse { get; } = new Button(new Vector2(-85, -45), Assets.Get(Sprites.Button), Assets.Rasterforge, "Queue Fuse", Color.Yellow, 10, MissionSelect, [1]);
    public static Button SmeltScrap { get; } = new Button(new Vector2(-85, -15), Assets.Get(Sprites.Button), Assets.Rasterforge, "Queue Smelt", Color.Yellow, 10, MissionSelect, [1]);
    public static Button RepairModule { get; } = new Button(new Vector2(-85, 15), Assets.Get(Sprites.Button), Assets.Rasterforge, "Queue Module", Color.Yellow, 10, MissionSelect, [1]);
    public static Button CancelQueue { get; } = new Button(new Vector2(-85, 45), Assets.Get(Sprites.Button), Assets.Rasterforge, "Cancel Latest", Color.Red, 10, MissionSelect, [1]);
    public static ItemSlot<Weapon> SecondarySlot { get; private set; } = new ItemSlot<Weapon>(new Vector2(-TerminalMenu.Size.X / 4 - 25, 50), Assets.Get(Sprites.EmptySlot), (int)ModuleType.Guns, MissionSelect, [1]);
    public static Decal StolenModule { get; } = new Decal(Vector2.Zero, null, MissionSelect, [0]);
    public static Decal ModuleText { get; } = new Decal(Vector2.Zero, Assets.Rasterforge, "", Color.White, 10, MissionSelect, [0]);

    //Pickup Drone Menu
    public static Button LaunchButton { get; } = new Button(new Vector2(-20, 0), Assets.Get(Sprites.Button), Assets.Rasterforge, "Leave", Color.LightBlue, 6, PickupDroneMenu, [0]);

    //Saves Menu
    public static Button SaveToFile { get; } = new Button(Vector2.Zero, Assets.Get(Sprites.Button), Assets.Rasterforge, "Save", Color.White, 6, SavesMenu, [1]);
    public static Button LoadFromFile { get; } = new Button(Vector2.Zero, Assets.Get(Sprites.Button), Assets.Rasterforge, "Load", Color.White, 6, SavesMenu, [0]);
    public static Button PrevSave { get; } = new Button(new Vector2(-100, 0), Assets.Get(Sprites.Button), Assets.Rasterforge, "Prev", Color.White, 6, SavesMenu, [0, 1]);
    public static Button NextSave { get; } = new Button(new Vector2(100, 0), Assets.Get(Sprites.Button), Assets.Rasterforge, "Next", Color.White, 6, SavesMenu, [0, 1]);
    public static Button DeleteSave { get; } = new Button(new Vector2(100, 40), Assets.Get(Sprites.Button), Assets.Rasterforge, "Delete", Color.White, 6, SavesMenu, [0, 1]);
    public static Textbox Name { get; } = new Textbox(new Vector2(0, -40), Assets.Get(Sprites.Button), Assets.Rasterforge, SavesMenu, [1]);
    public static Decal LoadedName { get; } = new Decal(new Vector2(0, 40), Assets.Rasterforge, "", Color.White, 6, SavesMenu, [0, 1]);
    public static Button SaveBack { get; } = new Button(new Vector2(-100, 40), Assets.Get(Sprites.Button), Assets.Rasterforge, "Back", Color.White, 6, SavesMenu, [1]);
    public static Button LoadBack { get; } = new Button(new Vector2(-100, 40), Assets.Get(Sprites.Button), Assets.Rasterforge, "Back", Color.White, 6, SavesMenu, [0]);

    //Upgrade Menu
    public static Decal TraderChat { get; } = new Decal(Vector2.Zero, Assets.Rasterforge,
        "Hey there!" +
        "\nIf you get me some rare materials, I can improve your sensors." +
        "\nI'm also willing to upgrade some of your other modules for 5 scrap and retool upgraded sensors for 1.", Color.White, 8, UpgradeMenu, [0]);
    public static Button LidarUpgrade { get; } = new Button(new Vector2(75, 0), Assets.Get(Sprites.Button), Assets.Rasterforge, "Lidar", Color.Green, 6, UpgradeMenu, [1]);
    public static Button RadarUpgrade { get; } = new Button(new Vector2(0, 0), Assets.Get(Sprites.Button), Assets.Rasterforge, "Radar", Color.Green, 6, UpgradeMenu, [1]);
    public static Button PulseEmitterUpgrade { get; } = new Button(new Vector2(-75, 0), Assets.Get(Sprites.Button), Assets.Rasterforge, "Pulse", Color.Green, 6, UpgradeMenu, [1]);
    public static Button UpgradeHull { get; } = new Button(new Vector2(0, 0), Assets.Get(Sprites.Button), Assets.Rasterforge, "Upgrade Hull", Color.Green, 6, UpgradeMenu, [2]);
    public static Button UpgradeGuns { get; } = new Button(new Vector2(0, 0), Assets.Get(Sprites.Button), Assets.Rasterforge, "Upgrade Guns", Color.Green, 6, UpgradeMenu, [2]);
    public static Button UpgradeEngine { get; } = new Button(new Vector2(0, 0), Assets.Get(Sprites.Button), Assets.Rasterforge, "Upgrade Engines", Color.Green, 6, UpgradeMenu, [2]);
    public static Button UpgradeCore { get; } = new Button(new Vector2(0, 0), Assets.Get(Sprites.Button), Assets.Rasterforge, "Upgrade Core", Color.Green, 6, UpgradeMenu, [2]);
    public static Decal UpgradeText { get; } = new Decal(new Vector2(-30, -20), Assets.Rasterforge, "", Color.White, 10, UpgradeMenu, [2]);

    //Hack menu
    public static Button HackButton { get; } = new Button(Vector2.Zero, Assets.Get(Sprites.Button), Assets.Rasterforge, "Hack", Color.Yellow, 6, HackMenu, [0]);
    public static Slider HackTimer { get; } = new Slider(Engine.Line, new Vector2(0, 50), new Vector2(50, 2), true, [Color.Yellow, new Color(0.1f, 0.1f, 0.1f)], HackMenu, [0]);

    //Escape menu
    public static Button EscapeButton { get; } = new Button(Vector2.Zero, Assets.Get(Sprites.Button), Assets.Rasterforge, "Escape!", Color.White, 6, EscapeMenu, [0]);

    //Multiple
    public static ItemSlot<Pickup>[] InventorySlots { get; set; } = new ItemSlot<Pickup>[4];
    public static ItemSlot<Pickup>[] MissionSelectSlots { get; set; } = new ItemSlot<Pickup>[4];
    public static ItemSlot<Module>[] ModuleSlots { get; private set; } = new ItemSlot<Module>[5];

    //Fields
    public static int windowType = 1;
    public static readonly Vector2[] resolutions = [new Vector2(1920, 1080), new Vector2(640, 480)];
    public static int selectedResolution = 0;
    public static readonly Modules[] setModules = [Modules.Hull, Modules.Basic, Modules.Engines, Modules.CloakingModifier, Modules.Dash];

    public static void AddUIElements()
    {
        Texture2D largePanel = Assets.Get(Sprites.LargePanel);
        Texture2D wideButton = Assets.Get(Sprites.WideButton);

        //Menus
        var tabTexture = Assets.Get(Sprites.Tab);
        var selectedTabTexture = Assets.Get(Sprites.SelectedTab);
        var selectSound = Assets.Get(Sound.Interact);

        PatchedConicsToggle.RisingInteract += delegate
        {
            SaveGame.PatchedConics = !SaveGame.PatchedConics;
        };
        ShaderToggle.RisingInteract += delegate
        {
            SaveGame.UseShader = !SaveGame.UseShader;
        };
        MusicSlider.ContinuousInteract += delegate
        {
            float i = MusicSlider.Intervals[0];
            SoundManager.MusicVolume = i;
            MusicVolume.Text = $"Music: {Math.Round(i * 100)}%";
        };
        SFXSlider.ContinuousInteract += delegate
        {
            float i = SFXSlider.Intervals[0];
            SoundManager.SFXVolume = i;
            UIManager.SFXVolume = i;
            SFXVolume.Text = $"Sound: {Math.Round(i * 100)}%";
        };
        UIScaleSlider.FallingInteract += delegate
        {
            float i = UIScaleSlider.Intervals[0];
            UIScale.Text = $"UI Scale: {Math.Truncate((i + 1) * 10) / 10}";
            UIManager.UIScale = (i + 1f);
        };

        SFXSlider.OnContinuousInteract(new Vector2(SFXSlider.Size.X, 0));
        MusicSlider.OnContinuousInteract(new Vector2(-MusicSlider.Size.X, 0));
        UIScaleSlider.OnContinuousInteract(new Vector2(UIScaleSlider.Size.X, 0));

        ExitButton.RisingInteract += delegate
        {
            Engine.Self.Exit();
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
        };
        NextWindowType.RisingInteract += delegate
        {
            windowType++;
            if (windowType > 2)
            {
                windowType -= 3;
            }
            switch (windowType)
            {
                case 0:
                    WindowType.Text = "Windowed";
                    break;
                case 1:
                    WindowType.Text = "Borderless Windowed";
                    break;
                case 2:
                    WindowType.Text = "Fullscreen";
                    break;
                default:
                    break;
            }
        }; //Write to config?
        NextResolution.RisingInteract += delegate
        {
            selectedResolution++;
            if (selectedResolution >= resolutions.Length)
            {
                selectedResolution = 0;
            }
            Resolution.Text = $"{resolutions[selectedResolution].X} x {resolutions[selectedResolution].Y}";
        };

        AbortButton.RisingInteract += delegate
        {
            if (Engine.SaveGame.CurrentMission.IsFailed)
            {
                return;
            }
            Engine.SaveGame.CurrentMission.FailMission();
            CurrentGameState.SwitchState(new PlayingGame());
        };
        FurnaceSlot.RisingInteract += delegate
        {
            if (FurnaceSlot.Item != null && !FurnaceSlot.Item.HasComponent<Smelt>())
            {
                (FurnaceSlot.Item, Engine.VirtualUI.selectedIcon) = (Engine.VirtualUI.selectedIcon as Pickup, FurnaceSlot.Item);
                return;
            }
            Events.SendMessage(Message.MothershipUpdateFurnace);
        };
        RestartSwitch.ContinuousInteract += delegate
        {
            if (Engine.SaveGame.Player.restartCd > 0)
            {
                SoundManager.PlayGlobalSound(Assets.Get(Sound.Fail));
                return;
            }
            if (RestartSwitch.Intervals[0] < 0.2f)
            {
                if (Engine.SaveGame.Player.IsEnabled)
                {
                    Events.SendMessage(Message.RestartModules);
                    Engine.SaveGame.Player.Disable();
                }
                Engine.SaveGame.Player.CanBeReenabled = true;
                Switch.Texture = Assets.Get(Sprites.SwitchOne);
            }
            if (RestartSwitch.Intervals[0] is > 0.2f and < 0.4f)
            {
                Switch.Texture = Assets.Get(Sprites.SwitchTwo);
            }
            if (RestartSwitch.Intervals[0] is > 0.4f and < 0.6f)
            {
                Switch.Texture = Assets.Get(Sprites.SwitchThree);
            }
            if (RestartSwitch.Intervals[0] is > 0.6f and < 0.8f)
            {
                Switch.Texture = Assets.Get(Sprites.SwitchFour);
            }
            if (RestartSwitch.Intervals[0] > 0.8f)
            {
                Switch.Texture = Assets.Get(Sprites.SwitchFive);
                if (!Engine.SaveGame.Player.IsEnabled)
                {
                    Engine.SaveGame.Player.Enable();
                }
            }
        };
        RestartSwitch.SetInterval(1, 1);
        FuseCounter.RisingInteract += delegate { Engine.SaveGame.Player.UpdateSpares(); };

        TerminalClose.ContinuousInteract += delegate
        {
            TerminalMenu.Position = new Vector2(TerminalMenu.Position.X, Math.Clamp(Engine.Cursor.HandPosition.Y - TerminalMenu.Size.Y * UIManager.UIScale + Assets.DimsOf(Sprites.TerminalSlider).Y * UIManager.UIScale / 2, 
                -TerminalMenu.Size.Y * UIManager.UIScale + Assets.DimsOf(Sprites.TerminalSlider).Y * UIManager.UIScale, 0));
            if (Engine.RealUI.selectedIcon is Fuse)
            {
                FuseCounter.OnRisingInteract(Vector2.Zero);
            }
            TerminalScreen.Position = TerminalMenu.Position;
        };
        PrevMission.RisingInteract += delegate { Engine.SaveGame.PrevMission(); }; //Do not remove outer delegate
        NextMission.RisingInteract += delegate { Engine.SaveGame.NextMission(); }; //Doing so causes exception due to null savegame
        SelectMission.RisingInteract += delegate
        {
            if ((Mission.missions[Engine.SaveGame.CurrentMissionIndex].data.IsRelaunchable || !Engine.SaveGame.CurrentMissionCompleted) && Events.SyncModules())
            {
                Engine.Startgame();
            }
        };
        LaunchButton.RisingInteract += delegate { Events.SendMessage(Message.EscapeDroneLeave); };
        CreateFuse.RisingInteract += delegate
        {
            if (Engine.SaveGame.QueuedItems.Count < 10)
            {
                Engine.SaveGame.QueuedItems.Add(new FuseQueue());
            }
        };
        var tooltip = new Window(Vector2.Zero, wideButton, Engine.VirtualUI);
        _ = new Decal(new Vector2(0, -3), Assets.Rasterforge, "Queue fuse construction. Cheap but delicate.", Color.White, 3f, tooltip, [0]);
        _ = new Decal(new Vector2(0, 3), Assets.Rasterforge, "Required time: 10 waves.", Color.White, 3f, tooltip, [0]);
        CreateFuse.AddTooltip(tooltip);
        SmeltScrap.RisingInteract += delegate
        {
            if (Engine.VirtualUI.selectedIcon != null)
            {
                foreach (var item in MissionSelectSlots)
                {
                    if (item.Item == null && Engine.SaveGame.QueuedItems.Count < 10)
                    {
                        item.Item = Engine.VirtualUI.selectedIcon as Pickup;
                        Engine.VirtualUI.selectedIcon = null;
                        Engine.SaveGame.QueuedItems.Add(new SmeltQueue(item));
                        Events.UpdateInventory();
                        return;
                    }
                }
            }
        };
        tooltip = new Window(Vector2.Zero, wideButton, Engine.VirtualUI);
        _ = new Decal(new Vector2(0, -3), Assets.Rasterforge, "Drag pickup over button to queue scrap melting.", Color.White, 3f, tooltip, [0]);
        _ = new Decal(new Vector2(0, 3), Assets.Rasterforge, "Required time: 10 waves. Gains additional metal per scrap.", Color.White, 3f, tooltip, [0]);
        SmeltScrap.AddTooltip(tooltip);
        RepairModule.RisingInteract += delegate
        {
            if (Engine.VirtualUI.selectedIcon as Module != null)
            {
                foreach (var item in MissionSelectSlots)
                {
                    if (item.Item == null && Engine.SaveGame.QueuedItems.Count < 10)
                    {
                        item.Item = Engine.VirtualUI.selectedIcon as Module;
                        Engine.VirtualUI.selectedIcon = null;
                        Engine.SaveGame.QueuedItems.Add(new RepairQueue(item));
                        Events.UpdateInventory();
                        return;
                    }
                }
            }
        };
        tooltip = new Window(Vector2.Zero, wideButton, Engine.VirtualUI);
        _ = new Decal(new Vector2(0, -3), Assets.Rasterforge, "Drag module over button to queue repair.\nRequired time: 20 waves. Requires no metal to repair.", Color.White, 3f, tooltip, [0]);
        RepairModule.AddTooltip(tooltip);
        CancelQueue.RisingInteract += delegate
        {
            if (Engine.SaveGame.QueuedItems.Count != 0)
            {
                Engine.SaveGame.QueuedItems.RemoveAt(Engine.SaveGame.QueuedItems.Count - 1);
            }
        };
        SaveButton.RisingInteract += delegate { SavesMenu.IsEnabled = true; Events.GetSave(); };
        ExitWithoutSave.RisingInteract += delegate { Events.QuitToMenu(); };
        LoadButton.RisingInteract += delegate { MainGlobals.IsEnabled = false; SavesMenu.IsEnabled = true; Events.GetSave(); };

        Name.RisingInteract += delegate { Engine.SaveGame.Name = Name.Text; };
        SaveToFile.RisingInteract += delegate { Util.Save(); };
        LoadFromFile.RisingInteract += delegate
        {
            CurrentGameState.SwitchState(new Loading(Engine.Load, LoadingStage.Complete));
        };
        SaveBack.RisingInteract += (delegate { MissionSelect.IsEnabled = true; SavesMenu.IsEnabled = false; });
        LoadBack.RisingInteract += (delegate { MainGlobals.IsEnabled = true; SavesMenu.IsEnabled = false; });

        LidarUpgrade.RisingInteract += delegate { Events.UpgradeSensors(SensorType.Lidar); };
        RadarUpgrade.RisingInteract += delegate { Events.UpgradeSensors(SensorType.Radar); };
        PulseEmitterUpgrade.RisingInteract += delegate { Events.UpgradeSensors(SensorType.PulseEmitter); };
        tooltip = new Window(Vector2.Zero, wideButton, Engine.VirtualUI);
        _ = new Decal(new Vector2(0, -3), Assets.Rasterforge, "Drag module over button to queue repair.\nRequired time: 20 waves. Requires no metal to repair.", Color.White, 3f, tooltip, [0]);
        UpgradeHull.AddTooltip(tooltip);
        UpgradeHull.RisingInteract += delegate
        {
            Events.UpgradeModule(ModuleType.Hull, Engine.SaveGame.Player.modules[ModuleType.Hull]);
        };
        UpgradeGuns.RisingInteract += delegate
        {
            Events.UpgradeModule(ModuleType.Guns, Engine.SaveGame.Player.modules[ModuleType.Guns]);
        };
        UpgradeEngine.RisingInteract += delegate
        {
            Events.UpgradeModule(ModuleType.Engines, Engine.SaveGame.Player.modules[ModuleType.Engines]);
        };
        UpgradeCore.RisingInteract += delegate
        {
            Events.UpgradeModule(ModuleType.Core, Engine.SaveGame.Player.modules[ModuleType.Core]);
        };

        HackButton.RisingInteract += delegate { Events.SendMessage(Message.Hack); };

        EscapeButton.RisingInteract += delegate { Events.SendMessage(Message.EscapeDroneLeave); };

        for (int i = 0; i < keys.Count; i++)
        {
            var binding = i; //Saving to a variable prevents delegate weirdness
            var key = keys[binding];
            KeybindTexts[i] = new Decal(new Vector2(-150, 10 * i - 85), Assets.Rasterforge, $"{text[binding]}", Color.White, 6, TerminalScreen, [0]) { WidgetAlignment = Alignment.Left };
            KeybindInputs[i] = new TerminalButton(new Vector2(0, 10 * i - 85), Assets.Rasterforge, $"{key}", Color.White, 6, TerminalScreen, [0]) { WidgetAlignment = Alignment.Right };
            KeybindInputs[i].RisingInteract += delegate
            {
                var key = Keyboard.GetState().GetPressedKeys();
                if (key.Length > 0)
                {
                    keys[binding] = key[0];
                    KeybindInputs[binding].Text = $"{key[0]}";
                }
            };
        }

        for (int i = 0; i < NextModule.Length; i++)
        {
            int module = i;
            NextModule[i] = new TerminalButton(new Vector2(120, 25 * i - 40), Assets.Rasterforge, $"Next", Color.White, 10, DebugMenu, [0]);
            int index = i;
            NextModule[i].RisingInteract += delegate
                {
                    if (Engine.Self.LoadingStage != LoadingStage.Complete)
                    {
                        return;
                    }
                    var nextModule = (Modules)Math.Clamp((int)(setModules[module] + 1), 0, (int)(Modules.End - 1)); Events.SetModules();
                    if (ItemFactory.Retrieve(nextModule).ID == index)
                    {
                        setModules[module] = nextModule;
                    }
                    Events.SetModules();
                };
            PrevModule[i] = new TerminalButton(new Vector2(-120, 25 * i - 40), Assets.Rasterforge, $"Prev", Color.White, 10, DebugMenu, [0]);
            PrevModule[i].RisingInteract += delegate
                {
                    if (Engine.Self.LoadingStage != LoadingStage.Complete)
                    {
                        return;
                    }
                    var nextModule = (Modules)Math.Clamp((int)(setModules[module] - 1), 0, (int)(Modules.End - 1));
                    if (ItemFactory.Retrieve(nextModule).ID == index)
                    {
                        setModules[module] = nextModule;
                    }
                    Events.SetModules();
                };
            ModuleSelection[i] = new Decal(new Vector2(0, 25 * i - 40), Assets.Rasterforge, "Loading...", Color.White, 10, DebugMenu, [0]);
        }
        SetModules.RisingInteract += delegate
        {
            for (ModuleType i = ModuleType.Hull; i <= ModuleType.Core; i++)
            {
                Engine.SaveGame.Player.modules[i] = ItemFactory.Retrieve((int)i);
            }
        };

        PlayerSpecialHealth.SetInterval(1, 1);
        PlayerHealth.Intervals = [1, 1];

        float xOffset = Assets.DimsOf(Sprites.LargePanel).X / 6;
        for (int x = 0; x < ModuleSlots.Length; x++)
        {
            ItemSlot<Module> slot;
            if(x%2==0)
            {
                slot = new ItemSlot<Module>(new Vector2(xOffset, Assets.DimsOf(Sprites.EmptySlot).Y * (x/2-1) * 1.1f), Assets.Get(Sprites.EmptySlot), x, MothershipMenu, [0]);
            }
            else
            {
                slot = new ItemSlot<Module>(new Vector2(xOffset + Assets.DimsOf(Sprites.EmptySlot).X * 1.1f, Assets.DimsOf(Sprites.EmptySlot).Y * (x/2-0.5f) * 1.1f), Assets.Get(Sprites.EmptySlot), x, MothershipMenu, [0]);
            }
                ModuleSlots[x] = slot;
            slot.RisingInteract += delegate
            {
                if (Engine.SaveGame.Player.IsCapsule) //No module repair after death
                {
                    return;
                }
                var item = slot.Item;
                if (item == null)
                {
                    return;
                }
                if (Engine.VirtualUI.selectedIcon is Pickup pickup && pickup is not Module)
                {
                    if (item.Type is Modules.EmergencyEngine)
                    {
                        slot.Item = new StandardEngine();
                        Engine.VirtualUI.selectedIcon = null;
                    }
                    else if (item.Type is Modules.PointDefense)
                    {
                        slot.Item = new Basic();
                        Engine.VirtualUI.selectedIcon = null;
                    }
                    else
                    {
                        Events.RepairModule(item);
                    }
                }
            };
            slot.RisingInteract += delegate
            {
                if (Engine.SaveGame.Player.IsCapsule) //No module replacement after death
                {
                    return;
                }
                var icon = Engine.VirtualUI.selectedIcon as Module;
                if (slot.Item == null)
                {
                    slot.Item = icon;
                    Engine.VirtualUI.selectedIcon = null;
                }
                else if (icon != null && icon.Type is Modules.EmergencyEngine or Modules.PointDefense or Modules.EmptyModule)
                {
                    Engine.VirtualUI.selectedIcon = null;
                }
                Events.SyncModules();
            };
            MissionSelect.AddWidget(ModuleSlots[x], 1); //Mothership menu main, this may be removed later
        }
        for (int i = 0; i < InventorySlots.GetLength(0); i++)
        {
            InventorySlots[i] = new ItemSlot<Pickup>(new Vector2(Assets.DimsOf(Sprites.EmptySlot).X * 1.1f * (i%2),
                Assets.DimsOf(Sprites.EmptySlot).Y * 1.1f * ((i/2)%2)) - Assets.DimsOf(Sprites.LargePanel)/3f, Assets.Get(Sprites.EmptySlot), -1, MothershipMenu, [0]);
            MissionSelectSlots[i] = new ItemSlot<Pickup>(new Vector2(Assets.DimsOf(Sprites.LargePanel).X / 2,
                Assets.DimsOf(Sprites.EmptySlot).Y * (i + 1) - Assets.DimsOf(Sprites.LargePanel).X / 2), Assets.Get(Sprites.EmptySlot), -1, MissionSelect, [1]);

            MissionSelect.AddWidget(InventorySlots[i], 1); //Same
            PickupDroneMenu.AddWidget(InventorySlots[i]);

            InventorySlots[i].RisingInteract += delegate { Events.UpdateInventory(); };
            MissionSelectSlots[i].RisingInteract += delegate { Events.UpdateInventory(); };
        }

        SecondarySlot.RisingInteract += delegate
        {
            if (Engine.SaveGame.Player.IsCapsule) //No module repair after death
            {
                return;
            }
            var item = SecondarySlot.Item;
            if (item == null)
            {
                return;
            }

            if (Engine.VirtualUI.selectedIcon is Pickup pickup && pickup is not Module)
            {
                Events.RepairModule(item);
            }
        };
        SecondarySlot.RisingInteract += delegate
        {
            Events.SyncModules();
        };

        for (int i = 0; i < Fuses.Length; i++)
        {
            var fuse = new ItemSlot<Fuse>(new Vector2(i%5 * 11 + 80, 0.5f + 20 * (i/5)), Assets.Get(Sprites.FuseSlot), -1, TerminalMenu, [1]);
            //Not sure why this works, don't touch
            int x = i;
            fuse.RisingInteract += delegate
            {
                Engine.SaveGame.Player.ToggleFuse(x);
            };
            Fuses[i] = fuse;
        }
        for (int i = 0; i < ModulePower.Length; i++)
        {
            var dial = new Dial(Assets.Get(Sprites.TurnableHands), new Vector2(TerminalMenu.Size.X*2/5, i * 24 - TerminalMenu.Size.Y/3), Assets.Get(Sprites.TurnableDial), TerminalMenu, [1, 2]) { Range = MathF.PI * 4 / 6 };
            int x = i;
            dial.ContinuousInteract += delegate
            {
                Engine.SaveGame.Player.SetPower(x, MathF.Round(dial.Value * 4));
                //Events.
            };
            ModulePower[i] = dial;
            float y = (i - 2) * 20 + 0.5f;
            ModuleIcons[i] = new Decal(new Vector2(61f, y -37.5f), null, TerminalMenu, [1]);
            StatusLights[i] = new Decal(new Vector2(44.5f, y-38), Assets.Get(Sprites.LEDGlow), TerminalMenu, [1]);
        }
        for(int i = 0; i < HealthSliders.Length; i++)
        {
            HealthSliders[i] = new Slider(Engine.Line, new Vector2(-20, i * 10 - TerminalScreen.Size.Y/3), new Vector2(30, 3), true, [Color.DarkGray, Color.Cyan], TerminalScreen, [1]);
        }
        Overlay = new Decal(Vector2.Zero, Assets.Get(Sprites.Overlay), TerminalMenu, [0, 1, 2]) { color = Color.White * 0.5f };
    }
}
