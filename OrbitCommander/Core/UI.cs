using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using UILib.Content;
using static OrbitCommander.Core.Engine;
using System.Diagnostics;
using OrbitCommander.Entities;
using OrbitCommander.Components;
using OrbitCommander.UIElements;

namespace OrbitCommander.Core;
public static class UI
{
    private static Vector2 center = BackBuffer / 2;
    public static Window PauseMenu { get; } = new Window(center, Assets.Get(Sprites.LargePanel));
    public static Window PlayerMenu { get; } = new Window(new Vector2(0, center.Y), Assets.Get(Sprites.Terminal)) { alignment = Alignment.Left };
    public static Window GarageMenu { get; } = new Window(center, Assets.Get(Sprites.GargantuanPanel));
    //public static TabbedWindow MainMenu { get; } = new TabbedWindow(center, Assets.Get(Sprites.GargantuanPanel), Assets.Get(Sprites.Tab), Assets.Get(Sprites.SelectedTab), Assets.Get(Sound.Interact), 3) { enabled = true, icons = [Assets.Get(Sprites.PlayIcon), Assets.Get(Sprites.SettingsIcon)] };
    public static Screen GlobalMainMenu { get; } = new Screen() { enabled = true };
    public static TabbedWindow MothershipMenu { get; } = new TabbedWindow(new Vector2(0, center.Y), Assets.Get(Sprites.Terminal), Assets.Get(Sprites.Tab), Assets.Get(Sprites.SelectedTab), Assets.Get(Sound.Interact), 3)
    { icons = [Assets.Get(Sprites.SmeltIcon), Assets.Get(Sprites.RepairIcon), Assets.Get(Sprites.VictoryIcon)], alignment = Alignment.Left };
    public static TabbedWindow MissionSelect { get; } = new TabbedWindow(new Vector2(0, center.Y), Assets.Get(Sprites.GargantuanPanel), Assets.Get(Sprites.Tab), Assets.Get(Sprites.SelectedTab), Assets.Get(Sound.Interact), 2)
    { icons = [Assets.Get(Sprites.PlanetIcon), Assets.Get(Sprites.RepairIcon)], alignment = Alignment.Left };
    public static Window PickupDroneMenu { get; } = new Window(center, Assets.Get(Sprites.LargePanel));
    public static Window SaveMenu { get; } = new Window(center, Assets.Get(Sprites.GargantuanPanel));
    public static Window LoadMenu { get; } = new Window(center, Assets.Get(Sprites.GargantuanPanel));
    public static TabbedWindow UpgradeMenu { get; } = new TabbedWindow(center, Assets.Get(Sprites.GargantuanPanel),
        Assets.Get(Sprites.Tab), Assets.Get(Sprites.SelectedTab), Assets.Get(Sound.Interact), 2);
    public static Window SettingsMenu { get; } = new Window(center, Assets.Get(Sprites.GargantuanPanel));
    public static Screen GlobalMenu { get; } = new Screen() { enabled = true };
    public static Screen CutsceneGlobalMenu { get; } = new Screen() { enabled = true };
    public static Window HackMenu { get; } = new Window(center, Assets.Get(Sprites.LargePanel));
    public static Window FloppyTerminal { get; } = new Window(new Vector2(0, center.Y), Assets.Get(Sprites.Terminal)) { alignment = Alignment.Left };
    public static Window FuseMenu { get; } = new Window(new Vector2(BackBuffer.X, center.Y), Assets.Get(Sprites.RightSidePanel)) { alignment = Alignment.Right };
    public static Window EscapeMenu { get; } = new Window(center, Assets.Get(Sprites.LargePanel));
    public static Window MenuSettings { get; } = new Window(new Vector2(center.X * 2, center.Y), Assets.Get(Sprites.RightSidePanel)) { alignment = Alignment.Right };
    public static Window KeyBinds { get; } = new Window(new Vector2(0, center.Y), Assets.Get(Sprites.Terminal)) { alignment = Alignment.Left };

    //Main Menu
    public static Button PatchedConicsToggle { get; } = new Button(new Vector2(-10, 50), Assets.Get(Sprites.SwitchOn), Assets.TextFont, $"Patched Conics: {SaveGame.PatchedConics}", Color.White, Assets.Get(Sprites.SwitchOff));
    public static Button ShaderToggle { get; } = new Button(new Vector2(-10, 70), Assets.Get(Sprites.SwitchOn), Assets.TextFont, $"Shader: {SaveGame.UseShader}", Color.White, Assets.Get(Sprites.SwitchOff)) { };
    public static TerminalSlider SFXSlider { get; } = new TerminalSlider(Line, Assets.Get(Sprites.Knob), new Vector2(50, -50), new Vector2(50, 5), false, [Color.White, Color.Gray]);
    public static TerminalSlider MusicSlider { get; } = new TerminalSlider(Line, Assets.Get(Sprites.Knob), new Vector2(50, -65), new Vector2(50, 5), false, [Color.White, Color.Gray]);
    public static TerminalSlider UIScaleSlider { get; } = new TerminalSlider(Line, Assets.Get(Sprites.Knob), new Vector2(50, -35), new Vector2(50, 5), false, [Color.White, Color.Gray]);
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
    public static Decal[] Module { get; } = new Decal[5];
    public static Decal[] KeybindTexts { get; } = new Decal[Input.Keybinds.Count];
    public static TerminalButton[] KeybindInputs { get; } = new TerminalButton[Input.Keybinds.Count];

    //Pause Menu
    public static Button QuitToMissionButton { get; } = new Button(new Vector2(0, -20), Assets.Get(Sprites.WideButton), Assets.TextFont, "Return", Color.White);
    public static Button SettingsButton { get; } = new Button(new Vector2(0, 20), Assets.Get(Sprites.WideButton), Assets.TextFont, "Options", Color.White);

    //Settings Menu
    public static Button PauseMenuButton { get; } = new Button(new Vector2(80, 45), Assets.Get(Sprites.WideButton), Assets.TextFont, "Back", Color.White);

    //Mothership Menu
    public static ItemSlot<Pickup> FurnaceSlot { get; } = new ItemSlot<Pickup>(new Vector2(-20, 0), Assets.Get(Sprites.EmptySlot), Engine.UIManager, -1);
    public static Button GarageButton { get; } = new Button(new Vector2(0, -GlobalMainMenu.Size.Y / 4), Assets.Get(Sprites.WideButton), Assets.TextFont, "To Garage", Color.White);
    public static Button CraftButton { get; } = new Button(new Vector2(0, GlobalMainMenu.Size.Y / 4), Assets.Get(Sprites.Button), Assets.TextFont, "Repair", Color.LightBlue);
    public static Decal RequiredCraftsText { get; } = new Decal(new Vector2(0) + new Vector2(0, -6), Assets.TextFont, "25", Color.White, 10);
    public static Slider FurnaceSlider { get; } = new Slider(Line, new Vector2(-20, -GlobalMainMenu.Size.Y / 6), new Vector2(60, 2), true, [new Color(255, 239, 85), new Color(50, 51, 67)]);
    public static Slider CraftingSlider { get; } = new Slider(Line, new Vector2(0, -GlobalMainMenu.Size.Y / 4), new Vector2(60, 2), true, [Color.Cyan, Color.Gray]);

    //Garage Menu
    public static Button RepairButton { get; } = new Button(new Vector2(-GarageMenu.Size.X / 4 - 25, -40), Assets.Get(Sprites.Button), Assets.TextFont, "Repair", Color.LightBlue);
    public static ItemSlot<Pickup> RepairSlot { get; } = new ItemSlot<Pickup>(new Vector2(-GarageMenu.Size.X / 4 - 25, 0), Assets.Get(Sprites.EmptySlot), Engine.UIManager, [0, 1, 2, 3, 4, 5]); //Contains all module ids and the construct id
    public static Decal MothershipScrap { get; } = new Decal(new Vector2(GarageMenu.Size.X / 2.2f, 20) - GarageMenu.Size / 2, Assets.TextFont, "0", Color.Gray, 10);
    public static Decal RepairText { get; } = new Decal(new Vector2(-GarageMenu.Size.X / 4 - 60 / 2.5f, 40), Assets.TextFont, "", Color.White, 10);
    public static Decal GaragePlayerImage { get; } = new Decal(new Vector2(GarageMenu.Size.X / 4, 0), Assets.Get(Sprites.PlayerUI));
    public static Decal ValidConfigText { get; } = new Decal(-GarageMenu.Size / 4 + new Vector2(20, GarageMenu.Size.Y / 1.5f), Assets.TextFont, "Ready for Combat", Color.Green, 10);

    //Player Menu
    public static Slider EnemySlider { get; } = new Slider(Line, new Vector2(0, -PlayerMenu.Size.Y / 3), new Vector2(50, 2), true, [Color.White, Color.Gray]);
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
    public static Button GlobalSidePanelOpen { get; } = new Button(Vector2.Zero, Assets.Get(Sprites.ToggleButton));
    public static Button GlobalFusePanelOpen { get; } = new Button(Vector2.Zero, Assets.Get(Sprites.RightSideOpen));
    public static Decal Timer { get; } = new Decal(new Vector2(-50, 0), Assets.TextFont, $"{IngameTime.DrawText}", Color.White, 10);
    public static Slider PlayerHealth { get; } = new Slider(Line, new Vector2(5, 5), new Vector2(150, 15), true, [Color.Red, Color.White, new Color(0.2f, 0.2f, 0.2f)]);
    public static Slider PlayerSpecialHealth { get; } = new Slider(Line, new Vector2(5, 5), new Vector2(150, 15), true, [Color.Transparent, Color.Transparent]);
    public static Slider PlayerAmmo { get; } = new Slider(Line, new Vector2(5, 15), new Vector2(100, 2), true, [Color.Yellow, Color.DarkGray]);
    public static Slider PlayerAbility { get; } = new Slider(Line, new Vector2(5, 15), new Vector2(100, 10), true, [Color.Cyan, Color.DarkGray]);
    public static Slider Thermometer { get; } = new Slider(Line, new Vector2(0, 15), new Vector2(100, 10), true, [new Color(25, 25, 25), Color.Transparent, new Color(25, 25, 25)]);

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
    public static Slider RestartSwitch { get; } = new Slider(Line, new Vector2(15, 70), Assets.DimsOf(Sprites.SwitchFive) + new Vector2(2, 4), false, [Color.Transparent, Color.Transparent]);
    public static Decal Switch { get; } = new Decal(RestartSwitch.Offset / UILib.Content.UIManager.UIScale, Assets.Get(Sprites.SwitchFive));
    public static Stack<Fuse> FuseCounter { get; } = new UIElements.Stack<Fuse>(new Vector2(-5, -70), Assets.Get(Sprites.Button), 1, Assets.Get(Sprites.Fuse), new Vector2(-Assets.Get(Sprites.Button).Width / 2 * 4 / 5, 0), new Vector2(8, 0), delegate () { return new Fuse(Color.White); });
    public static ItemSlot<Fuse>[,] Fuses { get; } = new ItemSlot<Fuse>[4, 5];
    public static Decal[] ModuleIcons { get; } = new Decal[5];
    public static Decal FuseDetailing { get; } = new Decal(new Vector2(30, 0), Assets.Get(Sprites.FuseDetailing));
    public static Dial FuseDial { get; } = new Dial(Assets.Get(Sprites.Indicator), new Vector2(55, -58), Assets.Get(Sprites.Dial));
    public static Button FuseMenuClose { get; } = new Button(new Vector2(-Assets.Get(Sprites.RightSidePanel).Width / 2 + Assets.Get(Sprites.ToggleButton).Width / 2, 0), Assets.Get(Sprites.RightSideOpen));
    public static Decal FuseText { get; } = new Decal(FuseDial.Offset / UILib.Content.UIManager.UIScale + new Vector2(0, 5), Assets.TextFont, "Instability", Color.Black, 5);

    //Misc
    public static Button SidePanelClose { get; } = new Button(new Vector2(Assets.Get(Sprites.Terminal).Width / 2 - Assets.Get(Sprites.ToggleButton).Width / 2, 0), Assets.Get(Sprites.ToggleButton));
    public static ItemSlot<Pickup>[] InventorySlots { get; set; } = new ItemSlot<Pickup>[4];
    public static ItemSlot<Pickup>[] MissionSelectSlots { get; set; } = new ItemSlot<Pickup>[4];
    public static ItemSlot<Module>[] ModuleSlots { get; private set; } = new ItemSlot<Module>[5];
    public static ItemSlot<Weapon> SecondarySlot { get; private set; } = new ItemSlot<Weapon>(new Vector2(-GarageMenu.Size.X / 4 - 25, 50), Assets.Get(Sprites.EmptySlot), Engine.UIManager, (int)ModuleType.Guns);

    public static int windowType = 1;
    public static readonly Vector2[] resolutions = [new Vector2(1920, 1080), new Vector2(640, 480)];
    public static int selectedResolution = 0;
    public static readonly Modules[] setModules = [Modules.Hull, Modules.Basic, Modules.Engines, Modules.CloakingModifier, Modules.Dash];

    //Hack menu
    public static Button HackButton { get; } = new Button(Vector2.Zero, Assets.Get(Sprites.Button), Assets.TextFont, "Hack", Color.Yellow);
    public static Slider HackTimer { get; } = new Slider(Line, new Vector2(0, 50), new Vector2(50, 2), true, [Color.Yellow, new Color(0.1f, 0.1f, 0.1f)]);

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

        PatchedConicsToggle.AddBehaviour(delegate
        {
            SaveGame.PatchedConics = !SaveGame.PatchedConics;
            PatchedConicsToggle.Text = $"Patched Conics: {SaveGame.PatchedConics}";
        });
        ShaderToggle.AddBehaviour(delegate () 
        { 
            SaveGame.UseShader = !SaveGame.UseShader; 
            ShaderToggle.Text = $"Shader: {SaveGame.UseShader}"; 
        });
        SFXSlider.AddBehaviour(delegate ()
        {
            float i = SFXSlider.Intervals[0];
            SoundManager.SFXVolume = i;
            UILib.Content.UIManager.SFXVolume = i;
            SFXVolume.Text = $"Sound: {Math.Round(i * 100)}%";
        });
        UIScaleSlider.AddBehaviour(delegate ()
        {
            float i = UIScaleSlider.Intervals[0];
            UIScale.Text = $"UI Scale: {Math.Truncate((i + 1) * 10) / 10}";
            if (Input.NewMouseState.LeftButton == ButtonState.Released)
            {
                UILib.Content.UIManager.UIScale = (i + 1f) * BackBuffer.X / ScreenSize.X;
            }
        });
        ExitButton.AddBehaviour(delegate ()
        {
            Self.Exit();
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
        });
        MusicSlider.AddBehaviour(delegate ()
        {
            float i = MusicSlider.Intervals[0];
            SoundManager.MusicVolume = i;
            MusicVolume.Text = $"Music: {Math.Round(i * 100)}%";
        });
        NextWindowType.AddBehaviour(delegate ()
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
        }); //Write to config?
        NextResolution.AddBehaviour(delegate ()
        {
            selectedResolution++;
            if (selectedResolution >= resolutions.Length)
            {
                selectedResolution = 0;
            }
            Resolution.Text = $"{resolutions[selectedResolution].X} x {resolutions[selectedResolution].Y}";
        });
        SFXSlider.SetInterval(1, 1);
        MusicSlider.SetInterval(0, 1);
        UIScaleSlider.SetInterval(1, 1);

        SFXSlider.ApplyBehaviours();
        MusicSlider.ApplyBehaviours();
        UIScaleSlider.ApplyBehaviours();

        QuitToMissionButton.AddBehaviour(delegate () { Events.MissionSelectTrigger(new MissionSelect()); });
        GarageButton.AddBehaviour(Events.GarageTrigger);
        RepairButton.AddBehaviour(Events.RepairItem);
        var tooltip = new Window(Vector2.Zero, wideButton);
        tooltip.AddWidget(new Decal(new Vector2(0, 0), Assets.TextFont, "1 metal to repair", Color.White, 3f));
        RepairButton.AddTooltip(tooltip);
        CraftButton.AddBehaviour(Events.CraftItem);
        tooltip = new Window(Vector2.Zero, wideButton);
        tooltip.AddWidget(new Decal(new Vector2(0, 0), Assets.TextFont, "1 metal to repair", Color.White, 3f));
        CraftButton.AddTooltip(tooltip);
        RepairSlot.AddBehaviour(Events.UpdateRepairText);
        FurnaceSlot.AddBehaviour(delegate()
        {
            if(FurnaceSlot.daughterItem != null && !FurnaceSlot.daughterItem.HasComponent<Smelt>())
            {
                (FurnaceSlot.daughterItem, Engine.UIManager.selectedIcon) = (Engine.UIManager.selectedIcon as Pickup, FurnaceSlot.daughterItem as IData);
                return;
            }
            Events.SendMessage(Message.MothershipUpdateFurnace);
        });
        RestartSwitch.AddBehaviour(
            delegate ()
            {
                if (Engine.SaveGame.Player.restartCd > 0)
                {
                    if (Input.OldMouseState.LeftButton == ButtonState.Released)
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
            });
        RestartSwitch.SetInterval(1, 1);
        FuseCounter.AddBehaviour(delegate () { Engine.SaveGame.Player.UpdateSpares(); });

        GlobalSidePanelOpen.AddBehaviour(delegate () 
        {
            if(Engine.UIManager.ScreenWindow == GlobalMainMenu)
            {
                KeyBinds.enabled = true;
            }
            else
            {
                Events.ToggleDockingMenus();
            }
        });
        GlobalFusePanelOpen.AddBehaviour(delegate ()
        {
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
            if(Engine.UIManager.ScreenWindow == GlobalMainMenu)
            {
                MenuSettings.enabled = true;
            }
            else
            {
                Events.UpdateModulesStatus();
                FuseMenu.enabled = true;
            }
        });
        SidePanelClose.AddBehaviour(delegate {
            KeyBinds.enabled = false;
            Events.ToggleDockingMenus();
        });
        FuseMenuClose.AddBehaviour(delegate ()
        {
            MenuSettings.enabled = false;
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
            FuseMenu.enabled = false;
            if(Engine.UIManager.selectedIcon is Fuse)
            {
                Engine.UIManager.selectedIcon = null;
                FuseCounter.Count++;
                Engine.SaveGame.Player.UpdateSpares();
            }
        });
        PrevMission.AddBehaviour(delegate () { Engine.SaveGame.PrevMission(); }); //Do not remove outer delegate
        NextMission.AddBehaviour(delegate () { Engine.SaveGame.NextMission(); }); //Doing so causes exception due to null savegame
        SelectMission.AddBehaviour(delegate ()
        {
            if ((Mission.missions[Engine.SaveGame.CurrentMissionIndex].data.IsRelaunchable || !Engine.SaveGame.CurrentMissionCompleted) && Events.SyncModules())
            {
                Startgame();
            }
        });
        LaunchButton.AddBehaviour(delegate () { Events.SendMessage(Message.EscapeDroneLeave); });
        SettingsButton.AddBehaviour(delegate () { PauseMenu.enabled = false; SettingsMenu.enabled = true; });
        PauseMenuButton.AddBehaviour(delegate () { PauseMenu.enabled = true; SettingsMenu.enabled = false; });
        CreateFuse.AddBehaviour(delegate ()
        {
            if (Engine.SaveGame.QueuedItems.Count < 10)
            {
                Engine.SaveGame.QueuedItems.Add(new FuseQueue());
            }
        });
        tooltip = new Window(Vector2.Zero, wideButton);
        tooltip.AddWidget(new Decal(new Vector2(0, -3), Assets.TextFont, "Queue fuse construction. Cheap but delicate.", Color.White, 3f));
        tooltip.AddWidget(new Decal(new Vector2(0, 3), Assets.TextFont, "Required time: 10 waves.", Color.White, 3f));
        CreateFuse.AddTooltip(tooltip);
        SmeltScrap.AddBehaviour(delegate ()
        {
            if (Engine.UIManager.selectedIcon != null)
            {
                foreach (var item in MissionSelectSlots)
                {
                    if (item.daughterItem == null && Engine.SaveGame.QueuedItems.Count < 10)
                    {
                        item.daughterItem = Engine.UIManager.selectedIcon as Pickup;
                        Engine.UIManager.selectedIcon = null;
                        Engine.SaveGame.QueuedItems.Add(new SmeltQueue(item));
                        Events.UpdateInventory();
                        return;
                    }
                }
            }
        });
        tooltip = new Window(Vector2.Zero, wideButton);
        tooltip.AddWidget(new Decal(new Vector2(0, -3), Assets.TextFont, "Drag pickup over button to queue scrap melting.", Color.White, 3f));
        tooltip.AddWidget(new Decal(new Vector2(0, 3), Assets.TextFont, "Required time: 10 waves. Gains additional metal per scrap.", Color.White, 3f));
        SmeltScrap.AddTooltip(tooltip);
        RepairModule.AddBehaviour(delegate ()
        {
            if (Engine.UIManager.selectedIcon as Module != null)
            {
                foreach (var item in MissionSelectSlots)
                {
                    if (item.daughterItem == null && Engine.SaveGame.QueuedItems.Count < 10)
                    {
                        item.daughterItem = Engine.UIManager.selectedIcon as Module;
                        Engine.UIManager.selectedIcon = null;
                        Engine.SaveGame.QueuedItems.Add(new RepairQueue(item));
                        Events.UpdateInventory();
                        return;
                    }
                }
            }
        });
        tooltip = new Window(Vector2.Zero, wideButton);
        tooltip.AddWidget(new Decal(new Vector2(0, -3), Assets.TextFont, "Drag module over button to queue repair.\nRequired time: 20 waves. Requires no metal to repair.", Color.White, 3f));
        RepairModule.AddTooltip(tooltip);
        CancelQueue.AddBehaviour(delegate ()
        {
            if (Engine.SaveGame.QueuedItems.Count != 0)
            {
                Engine.SaveGame.QueuedItems.RemoveAt(Engine.SaveGame.QueuedItems.Count - 1);
            }
        });
        SaveButton.AddBehaviour(delegate { Engine.UIManager.DisableAll(); SaveMenu.enabled = true; Events.GetSave(); });
        LoadButton.AddBehaviour(delegate { GlobalMainMenu.enabled = false; LoadMenu.enabled = true; Events.GetSave(); });

        Name.AddBehaviour(delegate { Engine.SaveGame.Name = Name.Text; });
        SaveToFile.AddBehaviour(Util.Save);
        LoadFromFile.AddBehaviour(delegate() 
        {
            Engine.UIManager.DisableAll();
            CurrentGameState.SwitchState(new Loading(Load, LoadingStage.Complete)); 
        });
        SaveBack.AddBehaviour(delegate { MissionSelect.enabled = true; SaveMenu.enabled = false; });
        LoadBack.AddBehaviour(delegate { GlobalMainMenu.enabled = true; LoadMenu.enabled = false; });

        LidarUpgrade.AddBehaviour(delegate { Events.UpgradeSensors(SensorType.Lidar); });
        RadarUpgrade.AddBehaviour(delegate { Events.UpgradeSensors(SensorType.Radar); });
        PulseEmitterUpgrade.AddBehaviour(delegate { Events.UpgradeSensors(SensorType.PulseEmitter); });
        tooltip = new Window(Vector2.Zero, wideButton);
        tooltip.AddWidget(new Decal(new Vector2(0, -3), Assets.TextFont, "Drag module over button to queue repair.\nRequired time: 20 waves. Requires no metal to repair.", Color.White, 3f));
        UpgradeHull.AddTooltip(tooltip);
        UpgradeHull.AddBehaviour(delegate
        {
            Events.UpgradeModule(ModuleType.Hull, Engine.SaveGame.Player.modules[ModuleType.Hull]);
        });
        UpgradeGuns.AddBehaviour(delegate
        {
            Events.UpgradeModule(ModuleType.Guns, Engine.SaveGame.Player.modules[ModuleType.Guns]);
        });
        UpgradeEngine.AddBehaviour(delegate
        {
            Events.UpgradeModule(ModuleType.Engines, Engine.SaveGame.Player.modules[ModuleType.Engines]);
        });
        UpgradeCore.AddBehaviour(delegate
        {
            Events.UpgradeModule(ModuleType.Core, Engine.SaveGame.Player.modules[ModuleType.Core]);
        });

        HackButton.AddBehaviour(delegate { Events.SendMessage(Message.Hack); });

        EscapeButton.AddBehaviour(delegate { Events.SendMessage(Message.EscapeDroneLeave); });

        GlobalMainMenu.AddWidget(ExitButton, (int)Alignment.TopLeft);
        GlobalMainMenu.AddWidget(SingleplayerButton, (int)Alignment.TopLeft);
        GlobalMainMenu.AddWidget(GlobalSidePanelOpen, (int)Alignment.Left);
        GlobalMainMenu.AddWidget(GlobalFusePanelOpen, (int)Alignment.Right);
        GlobalMainMenu.AddWidget(LoadButton, (int)Alignment.TopLeft);
        for (int i = 0; i < Input.Keybinds.Count; i++)
        {
            var binding = (Binding)i; //Saving to a variable prevents delegate weirdness
            var key = Input.Keybinds[binding];
            KeyBinds.AddWidget(KeybindTexts[i] = new Decal(new Vector2(-120 + Assets.TextFont.MeasureString($"{binding}").X / 2.55f, 12 * i - 80), Assets.TextFont, $"{binding}", Color.White, 8), (int)Alignment.TopRight);
            var button = new TerminalButton(new Vector2(60, 12 * i - 80), Assets.TextFont, $"{key}", Color.White, 8);
            button.AddBehaviour(delegate ()
            {
                var keys = Input.NewState.GetPressedKeys();
                if (keys.Length > 0)
                {
                    Input.Keybinds[binding] = keys[0];
                    button.Text = $"{keys[0]}";
                }
            });
            KeyBinds.AddWidget(KeybindInputs[i] = button, (int)Alignment.TopRight);
        }
        KeyBinds.AddWidget(SidePanelClose);

        for (int i = 0; i < NextModule.Length; i++)
        {
            int module = i;
            GlobalMainMenu.AddWidget(NextModule[i] = new TerminalButton(new Vector2(120, 25 * i - 40), Assets.TextFont, $"Next", Color.White, 10), (int)Alignment.Center);
            int index = i;
            NextModule[i].AddBehaviour(
                delegate () 
                {
                    if (Self.LoadingStage != LoadingStage.Complete)
                    {
                        return;
                    }
                    var nextModule = (Modules)Math.Clamp((int)(setModules[module] + 1), 0, (int)(Modules.End - 1)); Events.SetModules();
                    if (ItemFactory.moduleData[nextModule].ID == index)
                    {
                        setModules[module] = nextModule;
                    }
                    Events.SetModules();
                });
            GlobalMainMenu.AddWidget(PrevModule[i] = new TerminalButton(new Vector2(-120, 25 * i - 40), Assets.TextFont, $"Prev", Color.White, 10), (int)Alignment.Center);
            PrevModule[i].AddBehaviour(
                delegate () 
                {
                    if (Self.LoadingStage != LoadingStage.Complete)
                    {
                        return;
                    }
                    var nextModule = (Modules)Math.Clamp((int)(setModules[module] - 1), 0, (int)(Modules.End - 1));
                    if (ItemFactory.moduleData[nextModule].ID == index)
                    {
                        setModules[module] = nextModule;
                    }
                    Events.SetModules(); 
                });
            GlobalMainMenu.AddWidget(Module[i] = new Decal(new Vector2(0, 25 * i - 40), Assets.TextFont, "Loading...", Color.White, 10), (int)Alignment.Center);
        }

        PauseMenu.AddWidget(QuitToMissionButton);
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

        GarageMenu.AddWidget(MothershipScrap);
        GarageMenu.AddWidget(RepairButton);
        GarageMenu.AddWidget(RepairSlot);
        GarageMenu.AddWidget(RepairText);
        GarageMenu.AddWidget(GaragePlayerImage);
        GarageMenu.AddWidget(ValidConfigText);

        MothershipMenu.AddWidget(FurnaceSlider, 0);
        MothershipMenu.AddWidget(FurnaceSlot, 0);
        MothershipMenu.AddWidget(GarageButton, 1);
        MothershipMenu.AddWidget(CraftingSlider, 2);
        MothershipMenu.AddWidget(RequiredCraftsText, 2);
        MothershipMenu.AddWidget(CraftButton, 2);
        for (int i = 0; i < 3; i++)
        {
            MothershipMenu.AddWidget(SidePanelClose, i);
        }
        MothershipMenu.AddWidget(Overlay, 0);
        MothershipMenu.AddWidget(Overlay, 1);
        MothershipMenu.AddWidget(Overlay, 2);

        PlayerMenu.AddWidget(EnemySlider);
        PlayerMenu.AddWidget(WaveText);
        PlayerMenu.AddWidget(SidePanelClose);
        PlayerMenu.AddWidget(EnemiesLeft);
        PlayerMenu.AddWidget(Overlay);

        MenuSettings.AddWidget(PatchedConicsToggle, (int)Alignment.Top);
        MenuSettings.AddWidget(SFXSlider, (int)Alignment.Top);
        MenuSettings.AddWidget(MusicSlider, (int)Alignment.Top);
        MenuSettings.AddWidget(UIScaleSlider, (int)Alignment.Top);
        MenuSettings.AddWidget(SFXVolume, (int)Alignment.Top);
        MenuSettings.AddWidget(MusicVolume, (int)Alignment.Top);
        MenuSettings.AddWidget(UIScale, (int)Alignment.Top);
        MenuSettings.AddWidget(ShaderToggle, (int)Alignment.Top);
        MenuSettings.AddWidget(WindowType, (int)Alignment.Top);
        MenuSettings.AddWidget(NextWindowType, (int)Alignment.Top);
        MenuSettings.AddWidget(Resolution, (int)Alignment.Top);
        MenuSettings.AddWidget(NextResolution, (int)Alignment.Top);
        MenuSettings.AddWidget(ApplyChanges, (int)Alignment.Top);
        MenuSettings.AddWidget(FuseMenuClose);

        MissionSelect.AddWidget(MissionName, 0);
        MissionSelect.AddWidget(MissionDescription, 0);
        MissionSelect.AddWidget(PrevMission, 0);
        MissionSelect.AddWidget(NextMission, 0);
        MissionSelect.AddWidget(SelectMission, 0);
        MissionSelect.AddWidget(IsComplete, 0);
        MissionSelect.AddWidget(ValidConfigText, 1);
        MissionSelect.AddWidget(CreateFuse, 1);
        MissionSelect.AddWidget(SmeltScrap, 1);
        MissionSelect.AddWidget(RepairModule, 1);
        MissionSelect.AddWidget(CancelQueue, 1);
        MissionSelect.AddWidget(SaveButton, 0);
        MissionSelect.AddWidget(AlertText, 0);

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

        GlobalMenu.AddWidget(GlobalSidePanelOpen, (int)Alignment.Left);
        GlobalMenu.AddWidget(GlobalFusePanelOpen, (int)Alignment.Right);
        GlobalMenu.AddWidget(Timer, (int)Alignment.TopRight);
        GlobalMenu.AddWidget(PlayerHealth, (int)Alignment.TopLeft);
        GlobalMenu.AddWidget(PlayerSpecialHealth, (int)Alignment.TopLeft);
        GlobalMenu.AddWidget(PlayerAbility, (int)Alignment.TopLeft);
        GlobalMenu.AddWidget(PlayerAmmo, (int)Alignment.TopLeft);
        GlobalMenu.AddWidget(Thermometer, (int)Alignment.Top);
        PlayerSpecialHealth.SetInterval(1, 1);
        PlayerHealth.Intervals = [1, 1];

        for (int x = 0; x < ModuleSlots.GetLength(0); x++)
        {
            if (x % 2 == 0)
            {
                ModuleSlots[x] = new ItemSlot<Module>(new Vector2(-30, Assets.DimsOf(Sprites.EmptySlot).Y * x / 2
                    - Assets.DimsOf(Sprites.EmptySlot).Y), Assets.Get(Sprites.EmptySlot), Engine.UIManager, x);
            }
            else
            {
                ModuleSlots[x] = new ItemSlot<Module>(new Vector2(Assets.DimsOf(Sprites.EmptySlot).X / 1.4142f - 30,
                    Assets.DimsOf(Sprites.EmptySlot).Y * x / 2 - Assets.DimsOf(Sprites.EmptySlot).Y), Assets.Get(Sprites.EmptySlot), Engine.UIManager, x);
            }
            GarageMenu.AddWidget(ModuleSlots[x]);
            MissionSelect.AddWidget(ModuleSlots[x], 1);
            ModuleSlots[x].AddBehaviour(Events.UpdateModules);
        }
        for (int i = 0; i < InventorySlots.GetLength(0); i++)
        {
            InventorySlots[i] = new ItemSlot<Pickup>(new Vector2(Assets.DimsOf(Sprites.LargePanel).X / 4,
                Assets.DimsOf(Sprites.EmptySlot).Y * (i + 1) - Assets.DimsOf(Sprites.LargePanel).X / 2), Assets.Get(Sprites.EmptySlot), Engine.UIManager, -1);
            MissionSelectSlots[i] = new ItemSlot<Pickup>(new Vector2(Assets.DimsOf(Sprites.LargePanel).X / 2,
                Assets.DimsOf(Sprites.EmptySlot).Y * (i + 1) - Assets.DimsOf(Sprites.LargePanel).X / 2), Assets.Get(Sprites.EmptySlot), Engine.UIManager, -1);
            MothershipMenu.AddWidget(InventorySlots[i], 0);
            PickupDroneMenu.AddWidget(InventorySlots[i]);
            MissionSelect.AddWidget(InventorySlots[i], 1);
            MissionSelect.AddWidget(MissionSelectSlots[i], 1);
            InventorySlots[i].AddBehaviour(Events.UpdateInventory);
            MissionSelectSlots[i].AddBehaviour(Events.UpdateInventory);
        }
        GarageMenu.AddWidget(SecondarySlot);
        MissionSelect.AddWidget(SecondarySlot, 1);

        FuseMenu.AddWidget(FuseDetailing, (int)Alignment.Center);
        FuseMenu.AddWidget(RestartSwitch, (int)Alignment.Center);
        FuseMenu.AddWidget(Switch, (int)Alignment.Center);
        FuseMenu.AddWidget(FuseCounter, (int)Alignment.Center);
        for (int i = 0; i < 4; i++)
        {
            for (int j = -2; j < 3; j++)
            {
                var fuse = new ItemSlot<Fuse>(new Vector2(i * 11 + 2, j * 20 + 0.5f), Assets.Get(Sprites.FuseSlot), Engine.UIManager, -1);
                //Not sure why this works, don't touch
                int x = j + 2;
                int y = i;
                fuse.AddBehaviour(delegate ()
                {
                    Engine.SaveGame.Player.ToggleFuse(x, y);
                });
                Fuses[i, j + 2] = fuse;
                FuseMenu.AddWidget(fuse, (int)Alignment.Center);
            }
        }
        for (int i = 0; i < 5; i++)
        {
            float y = (i - 2) * 20 + 0.5f;
            FuseMenu.AddWidget(ModuleIcons[i] = new Decal(new Vector2(-16.5f, y + 0.5f), null), (int)Alignment.Center);
            FuseMenu.AddWidget(StatusLights[i] = new Decal(new Vector2(-33f, y), Assets.Get(Sprites.LEDGlow)), (int)Alignment.Center);
        }
        FuseMenu.AddWidget(FuseMenuClose);
        FuseMenu.AddWidget(FuseDial);
        FuseMenu.AddWidget(FuseText);

        CutsceneGlobalMenu.AddWidget(GlobalSidePanelOpen, (int)Alignment.Left);
        CutsceneGlobalMenu.AddWidget(GlobalFusePanelOpen, (int)Alignment.Right);

        HackMenu.AddWidget(HackButton);
        HackMenu.AddWidget(HackTimer);

        FloppyTerminal.AddWidget(SidePanelClose);
        FloppyTerminal.AddWidget(DeadFile);
        FloppyTerminal.AddWidget(Overlay);

        EscapeMenu.AddWidget(EscapeButton);

        Engine.UIManager.AddContainer(PauseMenu);
        Engine.UIManager.AddContainer(PlayerMenu);
        Engine.UIManager.AddContainer(MothershipMenu);
        Engine.UIManager.AddContainer(GarageMenu);
        Engine.UIManager.AddContainer(MissionSelect);
        Engine.UIManager.AddContainer(PickupDroneMenu);
        Engine.UIManager.AddContainer(SaveMenu);
        Engine.UIManager.AddContainer(LoadMenu);
        Engine.UIManager.AddContainer(UpgradeMenu);
        Engine.UIManager.AddContainer(SettingsMenu);
        Engine.UIManager.AddContainer(HackMenu);
        Engine.UIManager.AddContainer(FloppyTerminal);
        Engine.UIManager.AddContainer(FuseMenu);
        Engine.UIManager.AddContainer(EscapeMenu);
        Engine.UIManager.AddContainer(MenuSettings);
        Engine.UIManager.AddContainer(KeyBinds);

        Engine.UIManager.ScreenWindow = GlobalMenu;
    }
}
