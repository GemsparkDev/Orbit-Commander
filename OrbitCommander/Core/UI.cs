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
    public static Window PauseMenu { get; } = new Window(Vector2.Zero, Assets.Get(Sprites.LargePanel));
    //public static TabbedWindow MainMenu { get; } = new TabbedWindow(center, Assets.Get(Sprites.GargantuanPanel), Assets.Get(Sprites.Tab), Assets.Get(Sprites.SelectedTab), Assets.Get(Sound.Interact), 3) { enabled = true, icons = [Assets.Get(Sprites.PlayIcon), Assets.Get(Sprites.SettingsIcon)] };
    public static Screen GlobalMainMenu { get; } = new Screen() { IsEnabled = true };
    public static TabbedWindow TerminalMenu { get; } = new TabbedWindow(new Vector2(-1, 0), Assets.Get(Sprites.Terminal), null, null, null, null) { alignment = Alignment.Left, IsEnabled = true }; //0: Mothership menu, 1: Keybind menu, 2: Floppy menu, 3: Player menu
    public static TabbedWindow FuseMenu { get; } = new TabbedWindow(new Vector2(1, 0), Assets.Get(Sprites.RightSidePanel), null, null, null, null) { alignment = Alignment.Right, IsEnabled = true }; //0: Fuse menu, 1: Settings menu
    public static TabbedWindow MissionSelect { get; } = new TabbedWindow(new Vector2(-1, 0), Assets.Get(Sprites.GargantuanPanel), Assets.Get(Sprites.Tab), Assets.Get(Sprites.SelectedTab), Assets.Get(Sound.Interact), 2)
    { icons = [Assets.Get(Sprites.PlanetIcon), Assets.Get(Sprites.RepairIcon)], alignment = Alignment.Left };
    public static Window PickupDroneMenu { get; } = new Window(Vector2.Zero, Assets.Get(Sprites.LargePanel));
    public static Window SaveMenu { get; } = new Window(Vector2.Zero, Assets.Get(Sprites.GargantuanPanel));
    public static Window LoadMenu { get; } = new Window(Vector2.Zero, Assets.Get(Sprites.GargantuanPanel));
    public static TabbedWindow UpgradeMenu { get; } = new TabbedWindow(Vector2.Zero, Assets.Get(Sprites.GargantuanPanel),
        Assets.Get(Sprites.Tab), Assets.Get(Sprites.SelectedTab), Assets.Get(Sound.Interact), 2);
    public static Window SettingsMenu { get; } = new Window(Vector2.Zero, Assets.Get(Sprites.GargantuanPanel));
    public static Screen GlobalMenu { get; } = new Screen() { IsEnabled = true };
    public static Screen CutsceneGlobalMenu { get; } = new Screen() { IsEnabled = true };
    public static Window HackMenu { get; } = new Window(Vector2.Zero, Assets.Get(Sprites.LargePanel));
    public static Window EscapeMenu { get; } = new Window(Vector2.Zero, Assets.Get(Sprites.LargePanel));
    public static Window DebugMenu { get; } = new Window(-Vector2.One, Assets.Get(Sprites.GargantuanPanel)) { alignment = Alignment.TopLeft };

    //Main Menu
    public static Button PatchedConicsToggle { get; } = new Button(new Vector2(-10, 50), Assets.Get(Sprites.SwitchOn), Assets.TextFont, $"Patched Conics: {SaveGame.PatchedConics}", Color.White, Assets.Get(Sprites.SwitchOff));
    public static Button ShaderToggle { get; } = new Button(new Vector2(-10, 70), Assets.Get(Sprites.SwitchOn), Assets.TextFont, $"Shader: {SaveGame.UseShader}", Color.White, Assets.Get(Sprites.SwitchOff)) { };
    public static TerminalSlider SFXSlider { get; } = new TerminalSlider(Engine.Line, Assets.Get(Sprites.Knob), new Vector2(50, -50), new Vector2(50, 5), false, [Color.White, Color.Gray]);
    public static TerminalSlider MusicSlider { get; } = new TerminalSlider(Engine.Line, Assets.Get(Sprites.Knob), new Vector2(50, -65), new Vector2(50, 5), false, [Color.White, Color.Gray]);
    public static TerminalSlider UIScaleSlider { get; } = new TerminalSlider(Engine.Line, Assets.Get(Sprites.Knob), new Vector2(30, -20), new Vector2(50, 5), false, [Color.White, Color.Gray]);
    public static Decal SFXVolume { get; } = new Decal(new Vector2(-10, -50), Assets.TextFont, "Sound: 100%", Color.White, 5);
    public static Decal MusicVolume { get; } = new Decal(new Vector2(-10, -65), Assets.TextFont, "Music: 100%", Color.White, 5);
    public static Decal UIScale { get; } = new Decal(new Vector2(-10, -35), Assets.TextFont, $"UI Scale: {Math.Truncate((UIScaleSlider.Intervals[0] + 1) * 10) / 10}", Color.White, 5);
    public static TerminalButton SingleplayerButton { get; } = new TerminalButton(new Vector2(0, 0), Assets.TextFont, "Singleplayer", Color.White, 10);
    public static TerminalButton ExitButton { get; } = new TerminalButton(new Vector2(0, 40), Assets.TextFont, "Exit", Color.White, 10);
    public static TerminalButton LoadButton { get; } = new TerminalButton(new Vector2(0, 20), Assets.TextFont, "Load", Color.White, 10);
    public static Decal WindowType { get; } = new Decal(new Vector2(-75, -40), null, Assets.TextFont, "Borderless Window", Color.White, 6);
    public static TerminalButton NextWindowType { get; } = new TerminalButton(new Vector2(-75, -65), Assets.TextFont, "Next Window", Color.White, 6);
    public static Decal Resolution { get; } = new Decal(new Vector2(-10, 10), null, Assets.TextFont, "1920 x 1080", Color.White, 6);
    public static TerminalButton NextResolution { get; } = new TerminalButton(new Vector2(-30, 20), Assets.TextFont, "Next Resolution", Color.White, 10);
    public static TerminalButton ApplyChanges { get; } = new TerminalButton(new Vector2(-10, -10), Assets.TextFont, "Apply changes", Color.White, 10);
    public static TerminalButton[] NextModule { get; } = new TerminalButton[5];
    public static TerminalButton[] PrevModule { get; } = new TerminalButton[5];
    public static Decal[] ModuleSelection { get; } = new Decal[5];
    //TODO: Readd keybind inputs
    public static readonly List<Keys> keys = [Keys.W, Keys.S, Keys.A, Keys.D, Keys.Space, Keys.C, Keys.E, Keys.I, Keys.LeftControl, Keys.F, Keys.Q, Keys.RightShift, Keys.LeftShift, Keys.R, Keys.Escape, Keys.Tab];
    public static Decal[] KeybindTexts { get; } = new Decal[keys.Count];
    public static TerminalButton[] KeybindInputs { get; } = new TerminalButton[keys.Count];

    public static Button SetModules { get; } = new Button(new Vector2(0, 50), Assets.Get(Sprites.Button), Assets.TextFont, "Apply", Color.White);
    //Pause Menu
    public static Button AbortButton { get; } = new Button(new Vector2(0, -20), Assets.Get(Sprites.WideButton), Assets.TextFont, "Abort", Color.White);
    public static Button SettingsButton { get; } = new Button(new Vector2(0, 20), Assets.Get(Sprites.WideButton), Assets.TextFont, "Options", Color.White);

    //Settings Menu
    public static Button PauseMenuButton { get; } = new Button(new Vector2(80, 45), Assets.Get(Sprites.WideButton), Assets.TextFont, "Back", Color.White);

    //Mothership Menu
    public static ItemSlot<Pickup> FurnaceSlot { get; } = new ItemSlot<Pickup>(new Vector2(-20, 0), Assets.Get(Sprites.EmptySlot), -1);
    public static Decal RequiredCraftsText { get; } = new Decal(new Vector2(0) + new Vector2(0, -6), Assets.TextFont, "25", Color.White, 10);
    public static Slider FurnaceSlider { get; } = new Slider(Engine.Line, new Vector2(-20, -GlobalMainMenu.Size.Y / 6), new Vector2(60, 2), true, [new Color(255, 239, 85), new Color(50, 51, 67)]);

    //Player Menu
    public static Slider EnemySlider { get; } = new Slider(Engine.Line, new Vector2(0, -TerminalMenu.Size.Y / 3), new Vector2(50, 2), true, [Color.White, Color.Gray]);
    public static Decal WaveText { get; } = new Decal(new Vector2(-20, 0), Assets.TextFont, "0", Color.White, 10);
    public static Decal EnemiesLeft { get; } = new Decal(new Vector2(0, 0), Assets.TextFont, "0", Color.Red, 10);
    public static Decal Overlay { get; } = new Decal(new Vector2(-10.5f, 49f), Assets.Get(Sprites.Overlay)) { color = Color.White * 0.5f };
    //Mission Select Menu
    public static Decal MissionName { get; } = new Decal(new Vector2(0, -30), Assets.TextFont, "Name", Color.White, 10);
    public static Decal MissionDescription { get; } = new Decal(new Vector2(0, -15), Assets.TextFont, "Description", Color.Gray, 3f);
    public static Button PrevMission { get; } = new Button(new Vector2(-75, 20), Assets.Get(Sprites.Button), Assets.TextFont, "Prev", Color.LightBlue);
    public static Button NextMission { get; } = new Button(new Vector2(75, 20), Assets.Get(Sprites.Button), Assets.TextFont, "Next", Color.LightBlue);
    public static Button SelectMission { get; } = new Button(new Vector2(0, 20), Assets.Get(Sprites.Button), Assets.TextFont, "Launch!", Color.Yellow);
    public static Decal IsComplete { get; } = new Decal(new Vector2(0, 45), Assets.TextFont, "Not Complete", Color.Red, 10);
    public static Button CreateFuse { get; } = new Button(new Vector2(-85, -45), Assets.Get(Sprites.Button), Assets.TextFont, "Queue Fuse", Color.Yellow);
    public static Button SmeltScrap { get; } = new Button(new Vector2(-85, -15), Assets.Get(Sprites.Button), Assets.TextFont, "Queue Smelt", Color.Yellow);
    public static Button RepairModule { get; } = new Button(new Vector2(-85, 15), Assets.Get(Sprites.Button), Assets.TextFont, "Queue Module", Color.Yellow);
    public static Button CancelQueue { get; } = new Button(new Vector2(-85, 45), Assets.Get(Sprites.Button), Assets.TextFont, "Cancel Latest", Color.Red);
    public static Button SaveButton { get; } = new Button(new Vector2(0, -60), Assets.Get(Sprites.WideButton), Assets.TextFont, "Save & Exit", Color.LightBlue);
    public static Button ExitWithoutSave { get; } = new Button(new Vector2(0, -80), Assets.Get(Sprites.WideButton), Assets.TextFont, "Exit", Color.White);
    public static Decal AlertText { get; } = new Decal(new Vector2(0, 60), Assets.TextFont, "", Color.Yellow, 10);

    //Pickup Drone Menu
    public static Button LaunchButton { get; } = new Button(new Vector2(-20, 0), Assets.Get(Sprites.Button), Assets.TextFont, "Leave", Color.LightBlue);

    //Save and Load Menu
    public static Button SaveToFile { get; } = new Button(Vector2.Zero, Assets.Get(Sprites.Button), Assets.TextFont, "Save", Color.White);
    public static Button LoadFromFile { get; } = new Button(Vector2.Zero, Assets.Get(Sprites.Button), Assets.TextFont, "Load", Color.White);
    public static Button PrevSave { get; } = new Button(new Vector2(-100, 0), Assets.Get(Sprites.Button), Assets.TextFont, "Prev", Color.White);
    public static Button NextSave { get; } = new Button(new Vector2(100, 0), Assets.Get(Sprites.Button), Assets.TextFont, "Next", Color.White);
    public static Button DeleteSave { get; } = new Button(new Vector2(100, 40), Assets.Get(Sprites.Button), Assets.TextFont, "Delete", Color.White);
    public static Textbox Name { get; } = new Textbox(new Vector2(0, -40), Assets.Get(Sprites.Button), Assets.TextFont);
    public static Decal LoadedName { get; } = new Decal(new Vector2(0, 40), Assets.TextFont, "", Color.White, 10);
    public static Button SaveBack { get; } = new Button(new Vector2(-100, 40), Assets.Get(Sprites.Button), Assets.TextFont, "Back", Color.White);
    public static Button LoadBack { get; } = new Button(new Vector2(-100, 40), Assets.Get(Sprites.Button), Assets.TextFont, "Back", Color.White);

    //Global Menu
    public static Decal Timer { get; } = new Decal(new Vector2(-50, 0), Assets.TextFont, $"{Engine.IngameTime.DrawText}", Color.White, 10);
    public static Slider PlayerHealth { get; } = new Slider(Engine.Line, new Vector2(5, 5), new Vector2(150, 15), true, [Color.Red, Color.White, new Color(0.2f, 0.2f, 0.2f)]);
    public static Slider PlayerSpecialHealth { get; } = new Slider(Engine.Line, new Vector2(5, 5), new Vector2(150, 15), true, [Color.Transparent, Color.Transparent]);
    public static Slider PlayerAmmo { get; } = new Slider(Engine.Line, new Vector2(5, 15), new Vector2(100, 2), true, [Color.Yellow, Color.DarkGray]);
    public static Slider PlayerAbility { get; } = new Slider(Engine.Line, new Vector2(5, 15), new Vector2(100, 10), true, [Color.Cyan, Color.DarkGray]);
    public static Slider Thermometer { get; } = new Slider(Engine.Line, new Vector2(0, 15), new Vector2(100, 10), true, [new Color(25, 25, 25), Color.Transparent, new Color(25, 25, 25)]);

    //Upgrade Menu
    public static Decal TraderChat { get; } = new Decal(Vector2.Zero, Assets.TextFont,
        "Hey there!" +
        "\nIf you get me some rare materials, I can improve your sensors." +
        "\nI'm also willing to upgrade some of your other modules for 5 scrap and retool upgraded sensors for 1.", Color.White, 8);
    public static Button LidarUpgrade { get; } = new Button(new Vector2(75, 0), Assets.Get(Sprites.Button), Assets.TextFont, "Lidar", Color.Green);
    public static Button RadarUpgrade { get; } = new Button(new Vector2(0, 0), Assets.Get(Sprites.Button), Assets.TextFont, "Radar", Color.Green);
    public static Button PulseEmitterUpgrade { get; } = new Button(new Vector2(-75, 0), Assets.Get(Sprites.Button), Assets.TextFont, "Pulse", Color.Green);
    public static Button UpgradeHull { get; } = new Button(new Vector2(0, 0), Assets.Get(Sprites.Button), Assets.TextFont, "Upgrade Hull", Color.Green);
    public static Button UpgradeGuns { get; } = new Button(new Vector2(0, 0), Assets.Get(Sprites.Button), Assets.TextFont, "Upgrade Guns", Color.Green);
    public static Button UpgradeEngine { get; } = new Button(new Vector2(0, 0), Assets.Get(Sprites.Button), Assets.TextFont, "Upgrade Engines", Color.Green);
    public static Button UpgradeCore { get; } = new Button(new Vector2(0, 0), Assets.Get(Sprites.Button), Assets.TextFont, "Upgrade Core", Color.Green);
    public static Decal UpgradeText { get; } = new Decal(new Vector2(-30, -20), Assets.TextFont, "", Color.White, 10);

    //Fuse Menu
    public static Decal[] StatusLights { get; } = new Decal[5];
    public static Slider RestartSwitch { get; } = new Slider(Engine.Line, new Vector2(15, 70), Assets.DimsOf(Sprites.SwitchFive) + new Vector2(2, 4), false, [Color.Transparent, Color.Transparent]);
    public static Decal Switch { get; } = new Decal(RestartSwitch.Offset / UIManager.UIScale, Assets.Get(Sprites.SwitchFive));
    public static UIElements.Stack<Fuse> FuseCounter { get; } = new UIElements.Stack<Fuse>(new Vector2(-5, -70), Assets.Get(Sprites.Button), 1, Assets.Get(Sprites.Fuse), new Vector2(-Assets.Get(Sprites.Button).Width / 2 * 4 / 5, 0), new Vector2(8, 0), delegate () { return new Fuse(Color.White); });
    public static ItemSlot<Fuse>[,] Fuses { get; } = new ItemSlot<Fuse>[4, 5];
    public static Decal[] ModuleIcons { get; } = new Decal[5];
    public static Decal FuseDetailing { get; } = new Decal(new Vector2(30, 0), Assets.Get(Sprites.FuseDetailing));
    public static Dial FuseDial { get; } = new Dial(Assets.Get(Sprites.Indicator), new Vector2(55, -58), Assets.Get(Sprites.Dial));
    public static Button FuseClose { get; } = new Button(new Vector2(-Assets.Get(Sprites.RightSidePanel).Width / 2 + Assets.Get(Sprites.ToggleButton).Width / 2, 0), Assets.Get(Sprites.RightSideOpen));
    public static Decal FuseText { get; } = new Decal(FuseDial.Offset / UIManager.UIScale + new Vector2(0, 5), Assets.TextFont, "Instability", Color.Black, 5);

    //Misc
    public static Button TerminalClose { get; } = new Button(new Vector2(Assets.Get(Sprites.Terminal).Width / 2 - Assets.Get(Sprites.ToggleButton).Width / 2, 0), Assets.Get(Sprites.ToggleButton));
    public static ItemSlot<Pickup>[] InventorySlots { get; set; } = new ItemSlot<Pickup>[4];
    public static ItemSlot<Pickup>[] MissionSelectSlots { get; set; } = new ItemSlot<Pickup>[4];
    public static ItemSlot<Module>[] ModuleSlots { get; private set; } = new ItemSlot<Module>[5];
    public static ItemSlot<Weapon> SecondarySlot { get; private set; } = new ItemSlot<Weapon>(new Vector2(-TerminalMenu.Size.X / 4 - 25, 50), Assets.Get(Sprites.EmptySlot), (int)Core.ModuleType.Guns);

    public static int windowType = 1;
    public static readonly Vector2[] resolutions = [new Vector2(1920, 1080), new Vector2(640, 480)];
    public static int selectedResolution = 0;
    public static readonly Modules[] setModules = [Modules.Hull, Modules.Basic, Modules.Engines, Modules.CloakingModifier, Modules.Dash];

    //Hack menu
    public static Button HackButton { get; } = new Button(Vector2.Zero, Assets.Get(Sprites.Button), Assets.TextFont, "Hack", Color.Yellow);
    public static Slider HackTimer { get; } = new Slider(Engine.Line, new Vector2(0, 50), new Vector2(50, 2), true, [Color.Yellow, new Color(0.1f, 0.1f, 0.1f)]);

    //Restart Terminal
    public static Decal DeadFile { get; } = new Decal(new Vector2(-10, 0), Assets.Get(Sprites.DeadFile));
    public static Button EscapeButton { get; } = new Button(Vector2.Zero, Assets.Get(Sprites.Button), Assets.TextFont, "Escape!", Color.White);

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
            PatchedConicsToggle.Text = $"Patched Conics: {SaveGame.PatchedConics}";
        };
        ShaderToggle.RisingInteract += delegate
        {
            SaveGame.UseShader = !SaveGame.UseShader;
            ShaderToggle.Text = $"Shader: {SaveGame.UseShader}";
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
                (FurnaceSlot.Item, Engine.UIManager.selectedIcon) = (Engine.UIManager.selectedIcon as Pickup, FurnaceSlot.Item as IData);
                return;
            }
            Events.SendMessage(Message.MothershipUpdateFurnace);
        };
        RestartSwitch.ContinuousInteract += delegate
        {
            if (Engine.SaveGame.Player.restartCd > 0)
            {
                if (!UIManager.NewInput[0])
                {
                    SoundManager.PlayGlobalSound(Assets.Get(Sound.Fail));
                }
                return;
            }
            if (RestartSwitch.Intervals[0] < 0.2f)
            {
                if (Engine.SaveGame.Player.IsEnabled)
                {
                    Events.SendMessage(Message.RestartModules);
                    SoundManager.PlayGlobalSound(Assets.Get(Sound.Undock));
                    Engine.SaveGame.Player.IsEnabled = false;
                    Events.UpdateModulesStatus();
                }
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
                    Engine.SaveGame.Player.IsEnabled = true;
                    SoundManager.PlayGlobalSound(Assets.Get(Sound.Dock));
                    Events.UpdateModulesStatus();
                }
            }
        };
        RestartSwitch.SetInterval(1, 1);
        FuseCounter.RisingInteract += delegate { Engine.SaveGame.Player.UpdateSpares(); };

        TerminalClose.ContinuousInteract += delegate
        {
            TerminalMenu.Position = new Vector2(Math.Clamp(UIManager.NewPosition.X - TerminalMenu.Size.X * UIManager.UIScale + Assets.DimsOf(Sprites.RightSideOpen).X * UIManager.UIScale / 2, 
                -TerminalMenu.Size.X * UIManager.UIScale + Assets.DimsOf(Sprites.RightSideOpen).X * UIManager.UIScale, 0), TerminalMenu.Position.Y);
        };
        FuseClose.ContinuousInteract += delegate
        {
            Events.UpdateModulesStatus();
            FuseMenu.Position = new Vector2(Math.Clamp(
                UIManager.NewPosition.X + FuseMenu.Size.X * UIManager.UIScale - Assets.DimsOf(Sprites.RightSideOpen).X * UIManager.UIScale / 2,
                Engine.BackBuffer.X,
                Engine.BackBuffer.X + FuseMenu.Size.X * UIManager.UIScale - Assets.DimsOf(Sprites.RightSideOpen).X * UIManager.UIScale), FuseMenu.Position.Y);
            if (Engine.UIManager.selectedIcon is Fuse)
            {
                Engine.UIManager.selectedIcon = null;
                FuseCounter.Count++;
                Engine.SaveGame.Player.UpdateSpares();
            }
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
        SettingsButton.RisingInteract += delegate { PauseMenu.IsEnabled = false; SettingsMenu.IsEnabled = true; };
        PauseMenuButton.RisingInteract += delegate { PauseMenu.IsEnabled = true; SettingsMenu.IsEnabled = false; };
        CreateFuse.RisingInteract += delegate
        {
            if (Engine.SaveGame.QueuedItems.Count < 10)
            {
                Engine.SaveGame.QueuedItems.Add(new FuseQueue());
            }
        };
        var tooltip = new Window(Vector2.Zero, wideButton);
        tooltip.AddWidget(new Decal(new Vector2(0, -3), Assets.TextFont, "Queue fuse construction. Cheap but delicate.", Color.White, 3f));
        tooltip.AddWidget(new Decal(new Vector2(0, 3), Assets.TextFont, "Required time: 10 waves.", Color.White, 3f));
        CreateFuse.AddTooltip(tooltip);
        SmeltScrap.RisingInteract += delegate
        {
            if (Engine.UIManager.selectedIcon != null)
            {
                foreach (var item in MissionSelectSlots)
                {
                    if (item.Item == null && Engine.SaveGame.QueuedItems.Count < 10)
                    {
                        item.Item = Engine.UIManager.selectedIcon as Pickup;
                        Engine.UIManager.selectedIcon = null;
                        Engine.SaveGame.QueuedItems.Add(new SmeltQueue(item));
                        Events.UpdateInventory();
                        return;
                    }
                }
            }
        };
        tooltip = new Window(Vector2.Zero, wideButton);
        tooltip.AddWidget(new Decal(new Vector2(0, -3), Assets.TextFont, "Drag pickup over button to queue scrap melting.", Color.White, 3f));
        tooltip.AddWidget(new Decal(new Vector2(0, 3), Assets.TextFont, "Required time: 10 waves. Gains additional metal per scrap.", Color.White, 3f));
        SmeltScrap.AddTooltip(tooltip);
        RepairModule.RisingInteract += delegate
        {
            if (Engine.UIManager.selectedIcon as Module != null)
            {
                foreach (var item in MissionSelectSlots)
                {
                    if (item.Item == null && Engine.SaveGame.QueuedItems.Count < 10)
                    {
                        item.Item = Engine.UIManager.selectedIcon as Module;
                        Engine.UIManager.selectedIcon = null;
                        Engine.SaveGame.QueuedItems.Add(new RepairQueue(item));
                        Events.UpdateInventory();
                        return;
                    }
                }
            }
        };
        tooltip = new Window(Vector2.Zero, wideButton);
        tooltip.AddWidget(new Decal(new Vector2(0, -3), Assets.TextFont, "Drag module over button to queue repair.\nRequired time: 20 waves. Requires no metal to repair.", Color.White, 3f));
        RepairModule.AddTooltip(tooltip);
        CancelQueue.RisingInteract += delegate
        {
            if (Engine.SaveGame.QueuedItems.Count != 0)
            {
                Engine.SaveGame.QueuedItems.RemoveAt(Engine.SaveGame.QueuedItems.Count - 1);
            }
        };
        SaveButton.RisingInteract += delegate { SaveMenu.IsEnabled = true; Events.GetSave(); };
        ExitWithoutSave.RisingInteract += delegate { Events.QuitToMenu(); };
        LoadButton.RisingInteract += delegate { GlobalMainMenu.IsEnabled = false; LoadMenu.IsEnabled = true; Events.GetSave(); };

        Name.RisingInteract += delegate { Engine.SaveGame.Name = Name.Text; };
        SaveToFile.RisingInteract += delegate { Util.Save(); };
        LoadFromFile.RisingInteract += delegate
        {
            CurrentGameState.SwitchState(new Loading(Engine.Load, LoadingStage.Complete));
        };
        SaveBack.RisingInteract += (delegate { MissionSelect.IsEnabled = true; SaveMenu.IsEnabled = false; });
        LoadBack.RisingInteract += (delegate { GlobalMainMenu.IsEnabled = true; LoadMenu.IsEnabled = false; });

        LidarUpgrade.RisingInteract += delegate { Events.UpgradeSensors(SensorType.Lidar); };
        RadarUpgrade.RisingInteract += delegate { Events.UpgradeSensors(SensorType.Radar); };
        PulseEmitterUpgrade.RisingInteract += delegate { Events.UpgradeSensors(SensorType.PulseEmitter); };
        tooltip = new Window(Vector2.Zero, wideButton);
        tooltip.AddWidget(new Decal(new Vector2(0, -3), Assets.TextFont, "Drag module over button to queue repair.\nRequired time: 20 waves. Requires no metal to repair.", Color.White, 3f));
        UpgradeHull.AddTooltip(tooltip);
        UpgradeHull.RisingInteract += delegate
        {
            Events.UpgradeModule(ModuleType.Hull, Engine.SaveGame.Player.modules[Core.ModuleType.Hull]);
        };
        UpgradeGuns.RisingInteract += delegate
        {
            Events.UpgradeModule(ModuleType.Guns, Engine.SaveGame.Player.modules[Core.ModuleType.Guns]);
        };
        UpgradeEngine.RisingInteract += delegate
        {
            Events.UpgradeModule(ModuleType.Engines, Engine.SaveGame.Player.modules[Core.ModuleType.Engines]);
        };
        UpgradeCore.RisingInteract += delegate
        {
            Events.UpgradeModule(ModuleType.Core, Engine.SaveGame.Player.modules[Core.ModuleType.Core]);
        };

        HackButton.RisingInteract += delegate { Events.SendMessage(Message.Hack); };

        EscapeButton.RisingInteract += delegate { Events.SendMessage(Message.EscapeDroneLeave); };

        GlobalMainMenu.AddWidget(ExitButton, (int)Alignment.TopLeft);
        GlobalMainMenu.AddWidget(SingleplayerButton, (int)Alignment.TopLeft);
        GlobalMainMenu.AddWidget(LoadButton, (int)Alignment.TopLeft);
        for (int i = 0; i < keys.Count; i++)
        {
            var binding = i; //Saving to a variable prevents delegate weirdness
            var key = keys[binding];
            TerminalMenu.AddWidget(KeybindTexts[i] = new Decal(new Vector2(-120 + Assets.TextFont.MeasureString($"{binding}").X / 2.55f, 12 * i - 80), Assets.TextFont, $"{binding}", Color.White, 8), 1);
            var button = new TerminalButton(new Vector2(60, 12 * i - 80), Assets.TextFont, $"{key}", Color.White, 8);
            button.RisingInteract += delegate
            {
                var keys = Keyboard.GetState().GetPressedKeys();
                if (keys.Length > 0)
                {
                    keys[binding] = keys[0];
                    button.Text = $"{keys[0]}";
                }
            };
            TerminalMenu.AddWidget(KeybindInputs[i] = button, 1);
        }
        TerminalMenu.AddWidget(TerminalClose);

        for (int i = 0; i < NextModule.Length; i++)
        {
            int module = i;
            DebugMenu.AddWidget(NextModule[i] = new TerminalButton(new Vector2(120, 25 * i - 40), Assets.TextFont, $"Next", Color.White, 10), (int)Alignment.Center);
            int index = i;
            NextModule[i].RisingInteract += delegate
                {
                    if (Engine.Self.LoadingStage != LoadingStage.Complete)
                    {
                        return;
                    }
                    var nextModule = (Modules)Math.Clamp((int)(setModules[module] + 1), 0, (int)(Modules.End - 1)); Events.SetModules();
                    if (ItemFactory.moduleData[nextModule].ID == index)
                    {
                        setModules[module] = nextModule;
                    }
                    Events.SetModules();
                };
            DebugMenu.AddWidget(PrevModule[i] = new TerminalButton(new Vector2(-120, 25 * i - 40), Assets.TextFont, $"Prev", Color.White, 10), (int)Alignment.Center);
            PrevModule[i].RisingInteract += delegate
                {
                    if (Engine.Self.LoadingStage != LoadingStage.Complete)
                    {
                        return;
                    }
                    var nextModule = (Modules)Math.Clamp((int)(setModules[module] - 1), 0, (int)(Modules.End - 1));
                    if (ItemFactory.moduleData[nextModule].ID == index)
                    {
                        setModules[module] = nextModule;
                    }
                    Events.SetModules();
                };
            DebugMenu.AddWidget(ModuleSelection[i] = new Decal(new Vector2(0, 25 * i - 40), Assets.TextFont, "Loading...", Color.White, 10), (int)Alignment.Center);
        }
        DebugMenu.AddWidget(SetModules);
        SetModules.RisingInteract += delegate
        {
            for (ModuleType i = ModuleType.Hull; i <= ModuleType.Core; i++)
            {
                Engine.SaveGame.Player.modules[i] = ItemFactory.moduleData[setModules[(int)i]].Retrieve();
            }
        };

        PauseMenu.AddWidget(AbortButton);
        PauseMenu.AddWidget(SettingsButton);

        SettingsMenu.AddWidget(PauseMenuButton);
        SettingsMenu.AddWidget(PatchedConicsToggle);
        SettingsMenu.AddWidget(SFXSlider);
        SettingsMenu.AddWidget(MusicSlider);
        SettingsMenu.AddWidget(UIScaleSlider);
        SettingsMenu.AddWidget(SFXVolume);
        SettingsMenu.AddWidget(MusicVolume);
        SettingsMenu.AddWidget(UIScale);
        SettingsMenu.AddWidget(ShaderToggle);
        SettingsMenu.AddWidget(WindowType);
        SettingsMenu.AddWidget(NextWindowType);
        SettingsMenu.AddWidget(Resolution);
        SettingsMenu.AddWidget(NextResolution);
        SettingsMenu.AddWidget(ApplyChanges);

        TerminalMenu.AddWidget(FurnaceSlider, 0);
        TerminalMenu.AddWidget(FurnaceSlot, 0);
        TerminalMenu.AddWidget(RequiredCraftsText, 0);
        for (int i = 0; i < 3; i++)
        {
            TerminalMenu.AddWidget(TerminalClose, i);
        }
        TerminalMenu.AddWidget(Overlay, 0);

        TerminalMenu.AddWidget(EnemySlider, 3);
        TerminalMenu.AddWidget(WaveText, 3);
        TerminalMenu.AddWidget(TerminalClose, 3);
        TerminalMenu.AddWidget(EnemiesLeft, 3);
        TerminalMenu.AddWidget(Overlay, 3);

        FuseMenu.AddWidget(PatchedConicsToggle, 1);
        FuseMenu.AddWidget(SFXSlider, 1);
        FuseMenu.AddWidget(MusicSlider, 1);
        FuseMenu.AddWidget(UIScaleSlider, 1);
        FuseMenu.AddWidget(SFXVolume, 1);
        FuseMenu.AddWidget(MusicVolume, 1);
        FuseMenu.AddWidget(UIScale, 1);
        FuseMenu.AddWidget(ShaderToggle, 1);
        FuseMenu.AddWidget(WindowType, 1);
        FuseMenu.AddWidget(NextWindowType, 1);
        FuseMenu.AddWidget(Resolution, 1);
        FuseMenu.AddWidget(NextResolution, 1);
        FuseMenu.AddWidget(ApplyChanges, 1);
        FuseMenu.AddWidget(FuseClose, 1);

        MissionSelect.AddWidget(MissionName, 0);
        MissionSelect.AddWidget(MissionDescription, 0);
        MissionSelect.AddWidget(PrevMission, 0);
        MissionSelect.AddWidget(NextMission, 0);
        MissionSelect.AddWidget(SelectMission, 0);
        MissionSelect.AddWidget(IsComplete, 0);
        MissionSelect.AddWidget(CreateFuse, 1);
        MissionSelect.AddWidget(SmeltScrap, 1);
        MissionSelect.AddWidget(RepairModule, 1);
        MissionSelect.AddWidget(CancelQueue, 1);
        MissionSelect.AddWidget(SaveButton, 0);
        MissionSelect.AddWidget(AlertText, 0);
        MissionSelect.AddWidget(ExitWithoutSave, 0);

        PickupDroneMenu.AddWidget(LaunchButton);

        SaveMenu.AddWidget(SaveToFile);
        SaveMenu.AddWidget(PrevSave);
        SaveMenu.AddWidget(NextSave);
        SaveMenu.AddWidget(DeleteSave);
        SaveMenu.AddWidget(Name);
        SaveMenu.AddWidget(LoadedName);
        SaveMenu.AddWidget(SaveBack);

        LoadMenu.AddWidget(LoadFromFile);
        LoadMenu.AddWidget(PrevSave);
        LoadMenu.AddWidget(NextSave);
        LoadMenu.AddWidget(DeleteSave);
        LoadMenu.AddWidget(LoadedName);
        LoadMenu.AddWidget(LoadBack);

        UpgradeMenu.AddWidget(TraderChat, 0);
        UpgradeMenu.AddWidget(LidarUpgrade, 1);
        UpgradeMenu.AddWidget(RadarUpgrade, 1);
        UpgradeMenu.AddWidget(PulseEmitterUpgrade, 1);
        UpgradeMenu.AddWidget(UpgradeText, 2);
        UpgradeMenu.AddWidget(UpgradeHull, 2);
        UpgradeMenu.AddWidget(UpgradeGuns, 2);
        UpgradeMenu.AddWidget(UpgradeEngine, 2);
        UpgradeMenu.AddWidget(UpgradeCore, 2);

        GlobalMenu.AddWidget(Timer, (int)Alignment.TopRight);
        GlobalMenu.AddWidget(PlayerHealth, (int)Alignment.TopLeft);
        GlobalMenu.AddWidget(PlayerSpecialHealth, (int)Alignment.TopLeft);
        GlobalMenu.AddWidget(PlayerAbility, (int)Alignment.TopLeft);
        GlobalMenu.AddWidget(PlayerAmmo, (int)Alignment.TopLeft);
        GlobalMenu.AddWidget(Thermometer, (int)Alignment.Top);
        PlayerSpecialHealth.SetInterval(1, 1);
        PlayerHealth.Intervals = [1, 1];

        float xOffset = Assets.DimsOf(Sprites.LargePanel).X / 4 + Assets.DimsOf(Sprites.EmptySlot).X / 1.4142f;
        for (int x = 0; x < ModuleSlots.GetLength(0); x++)
        {
            ItemSlot<Module> slot;
            if (x % 2 == 0)
            {
                slot = new ItemSlot<Module>(new Vector2(xOffset, Assets.DimsOf(Sprites.EmptySlot).Y * x / 2
                    - Assets.DimsOf(Sprites.EmptySlot).Y), Assets.Get(Sprites.EmptySlot), x);
            }
            else
            {
                slot = new ItemSlot<Module>(new Vector2(Assets.DimsOf(Sprites.EmptySlot).X / 1.4142f + xOffset,
                    Assets.DimsOf(Sprites.EmptySlot).Y * x / 2 - Assets.DimsOf(Sprites.EmptySlot).Y), Assets.Get(Sprites.EmptySlot), x);
            }
            ModuleSlots[x] = slot;
            slot.RisingInteract += delegate
            {
                if (Engine.SaveGame.Player.isExpired) //No module repair after death
                {
                    return;
                }
                var item = slot.Item;
                if (item == null)
                {
                    return;
                }
                if (UIManager.Self.selectedIcon is Pickup pickup && pickup is not Module)
                {
                    if (item.Type is Modules.EmergencyEngine)
                    {
                        slot.Item = new StandardEngine();
                        UIManager.Self.selectedIcon = null;
                    }
                    else if (item.Type is Modules.PointDefense)
                    {
                        slot.Item = new Basic();
                        UIManager.Self.selectedIcon = null;
                    }
                    else
                    {
                        Events.RepairModule(item);
                    }
                }
            };
            slot.RisingInteract += delegate
            {
                if (Engine.SaveGame.Player.isExpired) //No module replacement after death
                {
                    return;
                }
                var icon = UIManager.Self.selectedIcon as Module;
                if (slot.Item == null)
                {
                    slot.Item = icon;
                    UIManager.Self.selectedIcon = null;
                }
                else if (icon != null && icon.Type is Modules.EmergencyEngine or Modules.PointDefense or Modules.EmptyModule)
                {
                    UIManager.Self.selectedIcon = null;
                }
                Events.SyncModules();
            };
            TerminalMenu.AddWidget(ModuleSlots[x]);
            MissionSelect.AddWidget(ModuleSlots[x], 1);
        }
        for (int i = 0; i < InventorySlots.GetLength(0); i++)
        {
            InventorySlots[i] = new ItemSlot<Pickup>(new Vector2(Assets.DimsOf(Sprites.LargePanel).X / 4,
                Assets.DimsOf(Sprites.EmptySlot).Y * (i + 1) - Assets.DimsOf(Sprites.LargePanel).X / 2), Assets.Get(Sprites.EmptySlot), -1);
            MissionSelectSlots[i] = new ItemSlot<Pickup>(new Vector2(Assets.DimsOf(Sprites.LargePanel).X / 2,
                Assets.DimsOf(Sprites.EmptySlot).Y * (i + 1) - Assets.DimsOf(Sprites.LargePanel).X / 2), Assets.Get(Sprites.EmptySlot), -1);
            TerminalMenu.AddWidget(InventorySlots[i], 0);
            PickupDroneMenu.AddWidget(InventorySlots[i]);
            MissionSelect.AddWidget(InventorySlots[i], 1);
            MissionSelect.AddWidget(MissionSelectSlots[i], 1);
            InventorySlots[i].RisingInteract += delegate { Events.UpdateInventory(); };
            MissionSelectSlots[i].RisingInteract += delegate { Events.UpdateInventory(); };
        }
        MissionSelect.AddWidget(SecondarySlot, 1);
        SecondarySlot.RisingInteract += delegate
        {
            if (Engine.SaveGame.Player.isExpired) //No module repair after death
            {
                return;
            }
            var item = SecondarySlot.Item;
            if (item == null)
            {
                return;
            }

            if (UIManager.Self.selectedIcon is Pickup pickup && pickup is not Module)
            {
                Events.RepairModule(item);
            }
        };
        SecondarySlot.RisingInteract += delegate
        {
            Events.SyncModules();
        };

        FuseMenu.AddWidget(FuseDetailing, 0);
        FuseMenu.AddWidget(RestartSwitch, 0);
        FuseMenu.AddWidget(Switch, 0);
        FuseMenu.AddWidget(FuseCounter, 0);
        for (int i = 0; i < 4; i++)
        {
            for (int j = -2; j < 3; j++)
            {
                var fuse = new ItemSlot<Fuse>(new Vector2(i * 11 + 2, j * 20 + 0.5f), Assets.Get(Sprites.FuseSlot), -1);
                //Not sure why this works, don't touch
                int x = j + 2;
                int y = i;
                fuse.RisingInteract += delegate
                {
                    Engine.SaveGame.Player.ToggleFuse(x, y);
                };
                Fuses[i, j + 2] = fuse;
                FuseMenu.AddWidget(fuse, 0);
            }
        }
        for (int i = 0; i < 5; i++)
        {
            float y = (i - 2) * 20 + 0.5f;
            FuseMenu.AddWidget(ModuleIcons[i] = new Decal(new Vector2(-16.5f, y + 0.5f), null), 0);
            FuseMenu.AddWidget(StatusLights[i] = new Decal(new Vector2(-33f, y), Assets.Get(Sprites.LEDGlow)), 0);
        }
        FuseMenu.AddWidget(FuseClose, 0);
        FuseMenu.AddWidget(FuseDial, 0);
        FuseMenu.AddWidget(FuseText, 0);

        HackMenu.AddWidget(HackButton);
        HackMenu.AddWidget(HackTimer);

        TerminalMenu.AddWidget(TerminalClose, 2);
        TerminalMenu.AddWidget(DeadFile, 2);
        TerminalMenu.AddWidget(Overlay, 2);

        EscapeMenu.AddWidget(EscapeButton);

        Engine.UIManager.AddContainer(PauseMenu);
        Engine.UIManager.AddContainer(TerminalMenu);
        Engine.UIManager.AddContainer(MissionSelect);
        Engine.UIManager.AddContainer(PickupDroneMenu);
        Engine.UIManager.AddContainer(SaveMenu);
        Engine.UIManager.AddContainer(LoadMenu);
        Engine.UIManager.AddContainer(UpgradeMenu);
        Engine.UIManager.AddContainer(SettingsMenu);
        Engine.UIManager.AddContainer(HackMenu);
        Engine.UIManager.AddContainer(FuseMenu);
        Engine.UIManager.AddContainer(EscapeMenu);
        Engine.UIManager.AddContainer(DebugMenu);

        Engine.UIManager.ScreenWindow = GlobalMenu;
    }
}
