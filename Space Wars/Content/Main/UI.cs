using System;
using UILib.Content.Main;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Space_Wars.Content.Main.Entities;
using static Space_Wars.Content.Main.Engine;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace Space_Wars.Content.Main;
public static class UI
{
    private static Vector2 center = BackBuffer / 2;
    public static Window PauseMenu { get; } = new Window(center, Assets.Get(Sprite.LargePanel));
    public static Window PlayerMenu { get; } = new Window(new Vector2(center.X, 0), Assets.Get(Sprite.Terminal)) { alignment = Alignment.Top };
    public static Window GarageMenu { get; } = new Window(center, Assets.Get(Sprite.GargantuanPanel));
    public static TabbedWindow MainMenu { get; } = new TabbedWindow(center, Assets.Get(Sprite.GargantuanPanel), Assets.Get(Sprite.Tab), Assets.Get(Sprite.SelectedTab), Assets.Get(Sound.Interact), 3)
    { enabled = true, icons = [Assets.Get(Sprite.PlayIcon), Assets.Get(Sprite.SettingsIcon)] };
    public static TabbedWindow MothershipMenu { get; } = new TabbedWindow(new Vector2(center.X, 0), Assets.Get(Sprite.Terminal), Assets.Get(Sprite.Tab), Assets.Get(Sprite.SelectedTab), Assets.Get(Sound.Interact), 3)
    { icons = [Assets.Get(Sprite.SmeltIcon), Assets.Get(Sprite.RepairIcon), Assets.Get(Sprite.VictoryIcon)], alignment = Alignment.Top };
    public static TabbedWindow MissionSelect { get; } = new TabbedWindow(new Vector2(0, center.Y), Assets.Get(Sprite.GargantuanPanel), Assets.Get(Sprite.Tab), Assets.Get(Sprite.SelectedTab), Assets.Get(Sound.Interact), 2)
    { icons = [Assets.Get(Sprite.PlanetIcon), Assets.Get(Sprite.RepairIcon)], alignment = Alignment.Left };
    public static Window PickupDroneMenu { get; } = new Window(center, Assets.Get(Sprite.LargePanel));
    public static Window SaveMenu { get; } = new Window(center, Assets.Get(Sprite.GargantuanPanel));
    public static Window LoadMenu { get; } = new Window(center, Assets.Get(Sprite.GargantuanPanel));
    public static TabbedWindow UpgradeMenu { get; } = new TabbedWindow(center, Assets.Get(Sprite.GargantuanPanel),
        Assets.Get(Sprite.Tab), Assets.Get(Sprite.SelectedTab), Assets.Get(Sound.Interact), 2);
    public static Window SettingsMenu { get; } = new Window(center, Assets.Get(Sprite.GargantuanPanel));
    public static Screen GlobalMenu { get; } = new Screen() { enabled = true };
    public static Window HackMenu { get; } = new Window(center, Assets.Get(Sprite.LargePanel));

    //Main Menu
    public static Button PatchedConicsToggle { get; } = new Button(new Vector2(0, -MainMenu.Size.Y / 4), Assets.Get(Sprite.WideButton), Assets.TextFont, $"Patched Conics: {PatchedConics}", Color.White);
    public static Slider SFXSlider { get; } = new Slider(Engine.Line, Assets.Get(Sprite.Knob), new Vector2(25, 0), new Vector2(50, 2), false, [Color.White, Color.Gray]);
    public static Slider MusicSlider { get; } = new Slider(Engine.Line, Assets.Get(Sprite.Knob), new Vector2(25, -15), new Vector2(50, 2), false, [Color.White, Color.Gray]);
    public static Slider UIScaleSlider { get; } = new Slider(Engine.Line, Assets.Get(Sprite.Knob), new Vector2(25, 15), new Vector2(50, 2), false, [Color.White, Color.Gray]);
    public static Decal SFXVolume { get; } = new Decal(new Vector2(-35, 0), Assets.TextFont, "Sound: 100%", Color.White, 5);
    public static Decal MusicVolume { get; } = new Decal(new Vector2(-35, -15), Assets.TextFont, "Music: 100%", Color.White, 5);
    public static Decal UIScale { get; } = new Decal(new Vector2(-35, 15), Assets.TextFont, $"UI Scale: {Math.Truncate((UIScaleSlider.Intervals[0] + 1) * 10) / 10}", Color.White, 5);
    public static Button ShaderToggle { get; } = new Button(new Vector2(0, MainMenu.Size.Y / 4), Assets.Get(Sprite.WideButton), Assets.TextFont, $"Shader: {UseShader}", Color.White);
    public static Button SingleplayerButton { get; } = new Button(new Vector2(0, -MainMenu.Size.Y / 4), Assets.Get(Sprite.WideButton), Assets.TextFont, "Singleplayer", Color.White);
    public static Button ExitButton { get; } = new Button(new Vector2(0, MainMenu.Size.Y / 4), Assets.Get(Sprite.WideButton), Assets.TextFont, "Exit", Color.White);
    public static Decal TitleName { get; } = new Decal(new Vector2(0, -MainMenu.Size.Y), Assets.Get(Sprite.Title));
    public static Button LoadButton { get; } = new Button(new Vector2(0, 0), Assets.Get(Sprite.WideButton), Assets.TextFont, "Load", Color.White);
    public static Decal WindowType { get; } = new Decal(new Vector2(-80, -15), null, Assets.TextFont, "Borderless Window", Color.White, 10);
    public static Button NextWindowType { get; } = new Button(new Vector2(-150, 10), Assets.Get(Sprite.Button), Assets.TextFont, "Next", Color.White );
    public static Decal Resolution { get; } = new Decal(new Vector2(-50, 45), null, Assets.TextFont, "1920 x 1080", Color.White, 10);
    public static Button NextResolution { get; } = new Button(new Vector2(-150, 45), Assets.Get(Sprite.Button), Assets.TextFont, "Next Resolution", Color.White);
    public static Button ApplyChanges { get; } = new Button(new Vector2(-50, 10), Assets.Get(Sprite.Button), Assets.TextFont, "Apply changes", Color.White);
    public static Button[] NextModule { get; } = new Button[5];
    public static Button[] PrevModule { get; } = new Button[5];
    public static Decal[] Module { get; } = new Decal[5];

    //Pause Menu
    public static Button QuitToMissionButton { get; } = new Button(new Vector2(0, -20), Assets.Get(Sprite.WideButton), Assets.TextFont, "Return", Color.White);
    public static Button SettingsButton { get; } = new Button(new Vector2(0, 20), Assets.Get(Sprite.WideButton), Assets.TextFont, "Options", Color.White);

    //Settings Menu
    public static Button PauseMenuButton { get; } = new Button(new Vector2(80, 45), Assets.Get(Sprite.WideButton), Assets.TextFont, "Back", Color.White);

    //Mothership Menu
    public static ItemSlot<Pickup> FurnaceSlot { get; } = new ItemSlot<Pickup>(new Vector2(-20, 0), Assets.Get(Sprite.EmptySlot), Engine.UIManager, -1);
    public static Button GarageButton { get; } = new Button(new Vector2(0, -MainMenu.Size.Y / 4), Assets.Get(Sprite.WideButton), Assets.TextFont, "To Garage", Color.White);
    public static Button CraftButton { get; } = new Button(new Vector2(0, MainMenu.Size.Y / 4), Assets.Get(Sprite.Button), Assets.TextFont, "Repair", Color.LightBlue);
    public static Decal RequiredCraftsText { get; } = new Decal(new Vector2(0) + new Vector2(0, -6), Assets.TextFont, "25", Color.White, 10);
    public static Slider FurnaceSlider { get; } = new Slider(Engine.Line, new Vector2(-20, -MainMenu.Size.Y / 6), new Vector2(60, 2), true, [new Color(255, 239, 85), new Color(50, 51, 67)]);
    public static Slider CraftingSlider { get; } = new Slider(Engine.Line, new Vector2(0, -MainMenu.Size.Y / 4), new Vector2(60, 2), true, [Color.Cyan, Color.Gray]);

    //Garage Menu
    public static Button RepairButton { get; } = new Button(new Vector2(-GarageMenu.Size.X / 4 - 25, -40), Assets.Get(Sprite.Button), Assets.TextFont, "Repair", Color.LightBlue);
    public static ItemSlot<Module> RepairSlot { get; } = new ItemSlot<Module>(new Vector2(-GarageMenu.Size.X / 4 - 25, 0), Assets.Get(Sprite.EmptySlot), Engine.UIManager, -1);
    public static Decal MothershipScrap { get; } = new Decal(new Vector2(GarageMenu.Size.X / 2.2f, 20) - GarageMenu.Size / 2, Assets.TextFont, "0", Color.Gray, 10);
    public static Decal RepairText { get; } = new Decal(new Vector2(-GarageMenu.Size.X / 4 - 60 / 2.5f, 40), Assets.TextFont, "", Color.White, 10);
    public static Decal GaragePlayerImage { get; } = new Decal(new Vector2(GarageMenu.Size.X / 4, 0), Assets.Get(Sprite.PlayerUI));
    public static Decal ValidConfigText { get; } = new Decal(-GarageMenu.Size / 4 + new Vector2(20, GarageMenu.Size.Y / 1.5f), Assets.TextFont, "Ready for Combat", Color.Green, 10);

    //Player Menu
    public static Slider EnemySlider { get; } = new Slider(Engine.Line, new Vector2(0, -PlayerMenu.Size.Y / 3), new Vector2(50, 2), true, [Color.White, Color.Gray]);
    public static Decal WaveText { get; } = new Decal(new Vector2(-20, 0), Assets.TextFont, "0", Color.White, 10);
    public static Decal EnemiesLeft { get; } = new Decal(new Vector2(0, 0), Assets.TextFont, "0", Color.Red, 10);
    public static Decal Overlay { get; } = new Decal(new Vector2(7.5f, 37.5f), Assets.Get(Sprite.Overlay)) { color = Color.White * 0.5f };

    //Mission Select Menu
    public static Decal MissionName { get; } = new Decal(new Vector2(0, -30), Assets.TextFont, "Name", Color.White, 10);
    public static Decal MissionDescription { get; } = new Decal(new Vector2(0, -15), Assets.TextFont, "Description", Color.Gray, 3f);
    public static Button PrevMission { get; } = new Button(new Vector2(-75, 20), Assets.Get(Sprite.Button), Assets.TextFont, "Prev", Color.LightBlue);
    public static Button NextMission { get; } = new Button(new Vector2(75, 20), Assets.Get(Sprite.Button), Assets.TextFont, "Next", Color.LightBlue);
    public static Button SelectMission { get; } = new Button(new Vector2(0, 20), Assets.Get(Sprite.Button), Assets.TextFont, "Launch!", Color.Yellow);
    public static Decal IsComplete { get; } = new Decal(new Vector2(0, 45), Assets.TextFont, "Not Complete", Color.Red, 10);
    public static Button CreateFuse { get; } = new Button(new Vector2(-85, -45), Assets.Get(Sprite.Button), Assets.TextFont, "Queue Fuse", Color.Yellow);
    public static Button SmeltScrap { get; } = new Button(new Vector2(-85, -15), Assets.Get(Sprite.Button), Assets.TextFont, "Queue Smelt", Color.Yellow);
    public static Button RepairModule { get; } = new Button(new Vector2(-85, 15), Assets.Get(Sprite.Button), Assets.TextFont, "Queue Module", Color.Yellow);
    public static Button CancelQueue { get; } = new Button(new Vector2(-85, 45), Assets.Get(Sprite.Button), Assets.TextFont, "Cancel Latest", Color.Red);
    public static Button SaveButton { get; } = new Button(new Vector2(0, -60), Assets.Get(Sprite.WideButton), Assets.TextFont, "Save & Exit", Color.LightBlue);
    public static Decal AlertText { get; } = new Decal(new Vector2(0, 60), Assets.TextFont, "", Color.Yellow, 10);

    //Pickup Drone Menu
    public static Button LaunchButton { get; } = new Button(new Vector2(-20, 0), Assets.Get(Sprite.Button), Assets.TextFont, "Leave", Color.LightBlue);

    //Save and Load Menu
    public static Button SaveToFile { get; } = new Button(Vector2.Zero, Assets.Get(Sprite.Button), Assets.TextFont, "Save", Color.White);
    public static Button LoadFromFile { get; } = new Button(Vector2.Zero, Assets.Get(Sprite.Button), Assets.TextFont, "Load", Color.White);
    public static Button PrevSave { get; } = new Button(new Vector2(-100, 0), Assets.Get(Sprite.Button), Assets.TextFont, "Prev", Color.White);
    public static Button NextSave { get; } = new Button(new Vector2(100, 0), Assets.Get(Sprite.Button), Assets.TextFont, "Next", Color.White);
    public static Button DeleteSave { get; } = new Button(new Vector2(100, 40), Assets.Get(Sprite.Button), Assets.TextFont, "Delete", Color.White);
    public static Textbox Name { get; } = new Textbox(new Vector2(0, -40), Assets.Get(Sprite.Button), Assets.TextFont);
    public static Decal LoadedName { get; } = new Decal(new Vector2(0, 40), Assets.TextFont, "", Color.White, 10);
    public static Button SaveBack { get; } = new Button(new Vector2(-100, 40), Assets.Get(Sprite.Button), Assets.TextFont, "Back", Color.White);
    public static Button LoadBack { get; } = new Button(new Vector2(-100, 40), Assets.Get(Sprite.Button), Assets.TextFont, "Back", Color.White);

    //Global Menu
    public static Button GlobalSidePanelOpen { get; } = new Button(Vector2.Zero, Assets.Get(Sprite.ToggleButton));
    public static Decal Timer { get; } = new Decal(new Vector2(-50, 0), Assets.TextFont, $"{Engine.IngameTime.DrawText}", Color.White, 10);
    public static Slider PlayerHealth { get; } = new Slider(Line, new Vector2(5, 5), new Vector2(150, 15), true, [Color.Red, Color.White, new Color(0.2f, 0.2f, 0.2f)]);
    public static Slider PlayerSpecialHealth { get; } = new Slider(Line, new Vector2(5, 5), new Vector2(150, 15), true, [Color.Transparent, Color.Transparent]);
    public static Slider PlayerAmmo { get; } = new Slider(Line, new Vector2(5, 15), new Vector2(100, 2), true, [Color.Yellow, Color.DarkGray]);
    public static Slider PlayerAbility { get; } = new Slider(Line, new Vector2(5, 15), new Vector2(100, 10), true, [Color.Cyan, Color.DarkGray]);

    //Upgrade Menu
    public static Decal TraderChat { get; } = new Decal(Vector2.Zero, Assets.TextFont, 
        "Hey there!" +
        "\nIf you get me some rare materials, I can improve your sensors." +
        "\nI'm also willing to upgrade some of your other modules for 5 scrap and retool upgraded sensors for 1.", Color.White, 8);
    public static Button LidarUpgrade { get; } = new Button(new Vector2(75, 0), Assets.Get(Sprite.Button), Assets.TextFont, "Lidar", Color.Green);
    public static Button RadarUpgrade { get; } = new Button(new Vector2(0, 0), Assets.Get(Sprite.Button), Assets.TextFont, "Radar", Color.Green);
    public static Button PulseEmitterUpgrade { get; } = new Button(new Vector2(-75, 0), Assets.Get(Sprite.Button), Assets.TextFont, "Pulse", Color.Green);
    public static Button UpgradeHull { get; } = new Button(new Vector2(0, 0), Assets.Get(Sprite.Button), Assets.TextFont, "Upgrade Hull", Color.Green);
    public static Button UpgradeGuns { get; } = new Button(new Vector2(0, 0), Assets.Get(Sprite.Button), Assets.TextFont, "Upgrade Guns", Color.Green);
    public static Button UpgradeEngine { get; } = new Button(new Vector2(0, 0), Assets.Get(Sprite.Button), Assets.TextFont, "Upgrade Engines", Color.Green);
    public static Button UpgradeCore { get; } = new Button(new Vector2(0, 0), Assets.Get(Sprite.Button), Assets.TextFont, "Upgrade Core", Color.Green);
    public static Decal UpgradeText { get; } = new Decal(new Vector2(-30, -20), Assets.TextFont, "", Color.White, 10);

    //Repair Menu
    public static Slider RestartSlider { get; } = new Slider(Engine.Line, new Vector2(-100 + Engine.BackBuffer.X / 2, 50), new Vector2(50, 2), true, [Color.Cyan, Color.Black]);
    public static Decal[] StatusLights { get; } = new Decal[5];
    public static Slider RestartSwitch { get; } = new Slider(Engine.Line, new Vector2(-100 + Engine.BackBuffer.X / 2, 0), Assets.DimsOf(Sprite.SwitchOne) + new Vector2(2, 4), false, [Color.Transparent, Color.Transparent]);
    public static Decal Switch { get; } = new Decal(RestartSwitch.Offset + new Vector2(RestartSwitch.Size.X - Engine.BackBuffer.X / 2, 0), Assets.Get(Sprite.SwitchFive));
    public static Decal FuseCounter { get; } = new Decal(new Vector2(-60 + Engine.BackBuffer.X / 2, -55), Assets.TextFont, "0", Color.Yellow, 10);
    public static Button[,] Fuses { get; } = new Button[4, 5];
    public static Decal[] ModuleIcons { get; } = new Decal[5];
    public static Decal FragilityTextbox { get; } = new Decal(new Vector2(Engine.BackBuffer.X / 2, -55), Assets.TextFont, "Fuse Fragility: Med", Color.Yellow, 6);

    //Misc
    public static Button SidePanelClose { get; } = new Button(new Vector2(0, -Assets.Get(Sprite.ToggleButton).Height / 2 + Assets.Get(Sprite.Terminal).Height / 2), Assets.Get(Sprite.ToggleButton));
    public static ItemSlot<Pickup>[] InventorySlots { get; set; } = new ItemSlot<Pickup>[4];
    public static ItemSlot<Pickup>[] MissionSelectSlots { get; set; } = new ItemSlot<Pickup>[4];
    public static ItemSlot<Module>[] ModuleSlots { get; private set; } = new ItemSlot<Module>[5];
    public static ItemSlot<Module> SecondarySlot { get; private set; } = new ItemSlot<Module>(new Vector2(-GarageMenu.Size.X / 4 - 25, 50), Assets.Get(Sprite.EmptySlot), Engine.UIManager, (int)ModuleType.Guns);

    public static int type = 1;
    public static Vector2[] resolutions = [ new Vector2(1920, 1080), new Vector2(640, 480) ];
    public static int selectedResolution = 0;
    public static Modules[] setModules = [ Modules.Hull, Modules.AdaptiveShotgun, Modules.Engines, Modules.Sensors, Modules.Dash ];

    //Hack menu
    public static Button HackButton { get; } = new Button(Vector2.Zero, Assets.Get(Sprite.Button), Assets.TextFont, "Hack", Color.Yellow);
    public static Slider HackTimer { get; } = new Slider(Engine.Line, new Vector2(0, 50), new Vector2(50, 2), true, [Color.Yellow, new Color(0.1f, 0.1f, 0.1f)]);

    public static void AddUIElements()
    {
        Texture2D largePanel = Assets.Get(Sprite.LargePanel);
        Texture2D wideButton = Assets.Get(Sprite.WideButton);

        //Menus
        var tabTexture = Assets.Get(Sprite.Tab);
        var selectedTabTexture = Assets.Get(Sprite.SelectedTab);
        var selectSound = Assets.Get(Sound.Interact);

        ExitButton.AddBehaviour(delegate ()
        {
            Self.Exit();
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
        });
        MusicSlider.AddBehaviour(delegate ()
        {
            float i = MusicSlider.Intervals[0];
            SoundManager.MusicVolume = Math.Clamp(MathF.Pow(10, i - 0.954242509439f) - 0.111111111111f, 0, 1);
            MusicVolume.text = $"Music: {Math.Round(i * 100)}%";
        });
        NextWindowType.AddBehaviour(delegate() 
        {
            type++;
            if(type > 2)
            {
                type -= 3;
            }
            switch(type)
            {
                case 0:
                    WindowType.text = "Windowed";
                    break;
                case 1:
                    WindowType.text = "Borderless Windowed";
                    break;
                case 2:
                    WindowType.text = "Fullscreen";
                    break;
                default:
                    break;
            }
        }); //Write to config?
        NextResolution.AddBehaviour(delegate () 
        {
            selectedResolution++;
            if(selectedResolution >= resolutions.Length)
            {
                selectedResolution = 0;
            }
            Resolution.text = $"{resolutions[selectedResolution].X} x {resolutions[selectedResolution].Y}";
        });
        SFXSlider.SetInterval(1, 1);
        MusicSlider.SetInterval(0, 1);
        UIScaleSlider.SetInterval(1, 1);

        SFXSlider.ApplyBehaviours();
        MusicSlider.ApplyBehaviours();
        UIScaleSlider.ApplyBehaviours();

        QuitToMissionButton.AddBehaviour(EventHandler.MissionSelectTrigger);
        GarageButton.AddBehaviour(EventHandler.GarageTrigger);
        RepairButton.AddBehaviour(EventHandler.RepairModule);
        var tooltip = new Window(Vector2.Zero, wideButton);
        tooltip.AddWidget(new Decal(new Vector2(0, 0), Assets.TextFont, "1 metal to repair", Color.White, 3f));
        RepairButton.AddTooltip(tooltip);
        CraftButton.AddBehaviour(EventHandler.CraftItem);
        tooltip = new Window(Vector2.Zero, wideButton);
        tooltip.AddWidget(new Decal(new Vector2(0, 0), Assets.TextFont, "1 metal to repair", Color.White, 3f));
        CraftButton.AddTooltip(tooltip);
        RepairSlot.AddBehaviour(EventHandler.UpdateRepairText);
        FurnaceSlot.AddBehaviour(EventHandler.UpdateFurnace);
        RestartSwitch.AddBehaviour(
            delegate () 
            {
                if(Engine.SaveGame.Player.IsRestarting)
                {
                    if (Input.OldMouseState.LeftButton == ButtonState.Released)
                    {
                        SoundManager.PlayGlobalSound(Assets.Get(Sound.Fail));
                    }
                    return;
                }
                if(RestartSwitch.Intervals[0] < 0.05f)
                {
                    if(Engine.SaveGame.Player.IsEnabled)
                    {
                        EventHandler.SendMessage(Message.RestartModules);
                        SoundManager.PlayGlobalSound(Assets.Get(Sound.Undock));
                        Engine.SaveGame.Player.IsEnabled = false;
                        EventHandler.UpdateModulesStatus();
                    }
                    Switch.SetTexture(Assets.Get(Sprite.SwitchOne));
                }
                if (RestartSwitch.Intervals[0] is > 0.05f and < 0.3f)
                {
                    Switch.SetTexture(Assets.Get(Sprite.SwitchTwo));
                }
                if (RestartSwitch.Intervals[0] is > 0.3f and < 0.7f)
                {
                    Switch.SetTexture(Assets.Get(Sprite.SwitchThree));
                }
                if (RestartSwitch.Intervals[0] is > 0.7f and < 0.95f)
                {
                    Switch.SetTexture(Assets.Get(Sprite.SwitchFour));
                }
                if (RestartSwitch.Intervals[0] > 0.95f)
                {
                    Switch.SetTexture(Assets.Get(Sprite.SwitchFive));
                    if (!Engine.SaveGame.Player.IsEnabled)
                    {
                        Engine.SaveGame.Player.IsEnabled = true;
                        SoundManager.PlayGlobalSound(Assets.Get(Sound.Dock));
                        EventHandler.UpdateModulesStatus();
                    }
                }
            });
        RestartSwitch.SetInterval(1, 1);

        GlobalSidePanelOpen.AddBehaviour(EventHandler.ToggleDockingMenus);
        SidePanelClose.AddBehaviour(EventHandler.ToggleDockingMenus);
        PrevMission.AddBehaviour(delegate () { Engine.SaveGame.PrevMission(); }); //Do not remove outer delegate
        NextMission.AddBehaviour(delegate() { Engine.SaveGame.NextMission(); }); //Doing so causes exception due to null savegame
        SelectMission.AddBehaviour(delegate () { if ((Engine.SaveGame.CurrentMission.relaunchable || !Engine.SaveGame.CurrentMissionCompleted) && EventHandler.SyncModules()) { Startgame(); } });
        LaunchButton.AddBehaviour(delegate () { EventHandler.SendMessage(Message.EscapeDroneLeave); });
        SettingsButton.AddBehaviour(delegate() { PauseMenu.enabled = false; SettingsMenu.enabled = true; });
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
                        EventHandler.UpdateInventory();
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
                        EventHandler.UpdateInventory();
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
        SaveButton.AddBehaviour(delegate { Engine.UIManager.DisableAll(); SaveMenu.enabled = true; EventHandler.GetSave(); });
        LoadButton.AddBehaviour(delegate { MainMenu.enabled = false; LoadMenu.enabled = true; EventHandler.GetSave(); });

        Name.AddBehaviour(delegate { Engine.SaveGame.Name = Name.text; });
        SaveToFile.AddBehaviour(Save);
        LoadFromFile.AddBehaviour(Load);
        SaveBack.AddBehaviour(delegate { MissionSelect.enabled = true; SaveMenu.enabled = false; });
        LoadBack.AddBehaviour(delegate { MainMenu.enabled = true; LoadMenu.enabled = false; });

        LidarUpgrade.AddBehaviour(delegate { EventHandler.UpgradeSensors(new Lidar()); });
        RadarUpgrade.AddBehaviour(delegate { EventHandler.UpgradeSensors(new Radar()); });
        PulseEmitterUpgrade.AddBehaviour(delegate { EventHandler.UpgradeSensors(new PulseEmitter()); });
        tooltip = new Window(Vector2.Zero, wideButton);
        tooltip.AddWidget(new Decal(new Vector2(0, -3), Assets.TextFont, "Drag module over button to queue repair.\nRequired time: 20 waves. Requires no metal to repair.", Color.White, 3f));
        UpgradeHull.AddTooltip(tooltip);
        UpgradeHull.AddBehaviour(delegate 
        {
            EventHandler.UpgradeModule(ModuleType.Hull, Engine.SaveGame.Player.modules[ModuleType.Hull]);
        });
        UpgradeGuns.AddBehaviour(delegate
        {
            EventHandler.UpgradeModule(ModuleType.Guns, Engine.SaveGame.Player.modules[ModuleType.Guns]);
        });
        UpgradeEngine.AddBehaviour(delegate
        {
            EventHandler.UpgradeModule(ModuleType.Engines, Engine.SaveGame.Player.modules[ModuleType.Engines]);
        });
        UpgradeCore.AddBehaviour(delegate
        {
            EventHandler.UpgradeModule(ModuleType.Core, Engine.SaveGame.Player.modules[ModuleType.Core]);
        });

        HackButton.AddBehaviour(delegate { EventHandler.SendMessage(Message.Hack); });

        MainMenu.AddWidget(ExitButton as IFunctional, 0);
        MainMenu.AddWidget(SingleplayerButton as IFunctional, 0);
        MainMenu.AddWidget(TitleName, 0);
        MainMenu.AddWidget(TitleName, 1);
        MainMenu.AddWidget(PatchedConicsToggle as IFunctional, 1);
        MainMenu.AddWidget(SFXSlider as IFunctional, 1);
        MainMenu.AddWidget(MusicSlider as IFunctional, 1);
        MainMenu.AddWidget(UIScaleSlider as IFunctional, 1);
        MainMenu.AddWidget(SFXVolume, 1);
        MainMenu.AddWidget(MusicVolume, 1);
        MainMenu.AddWidget(UIScale, 1);
        MainMenu.AddWidget(ShaderToggle as IFunctional, 1);
        MainMenu.AddWidget(LoadButton as IFunctional, 0);
        MainMenu.AddWidget(WindowType, 1);
        MainMenu.AddWidget(NextWindowType as IFunctional, 1);
        MainMenu.AddWidget(Resolution, 1);
        MainMenu.AddWidget(NextResolution as IFunctional, 1);
        MainMenu.AddWidget(ApplyChanges as IFunctional, 1);

        for(int i = 0; i < NextModule.Length; i++)
        {
            int module = i;
            MainMenu.AddWidget((NextModule[i] = new Button(new Vector2(60, 25 * i - 40), Assets.Get(Sprite.Button), Assets.TextFont, $"{i + 5}", Color.White)) as IFunctional, 2);
            NextModule[i].AddBehaviour(delegate () { setModules[module] = (Modules)Math.Clamp((int)(setModules[module] + 1), 0, (int)(Modules.End - 1)); EventHandler.SetModules(); });
            MainMenu.AddWidget((PrevModule[i] = new Button(new Vector2(-60, 25 * i - 40), Assets.Get(Sprite.Button), Assets.TextFont, $"{i}", Color.White)) as IFunctional, 2);
            PrevModule[i].AddBehaviour(delegate () { setModules[module] = (Modules)Math.Clamp((int)(setModules[module] - 1), 0, (int)(Modules.End - 1)); EventHandler.SetModules(); });
            MainMenu.AddWidget(Module[i] = new Decal(new Vector2(0, 25 * i - 40), Assets.TextFont, "", Color.White, 10), 2);
        }
        EventHandler.SetModules();

        PauseMenu.AddWidget(QuitToMissionButton as IFunctional);
        PauseMenu.AddWidget(SettingsButton as IFunctional);

        SettingsMenu.AddWidget(PauseMenuButton as IFunctional);
        SettingsMenu.AddWidget(PatchedConicsToggle as IFunctional);
        SettingsMenu.AddWidget(SFXSlider as IFunctional);
        SettingsMenu.AddWidget(MusicSlider as IFunctional);
        SettingsMenu.AddWidget(UIScaleSlider as IFunctional);
        SettingsMenu.AddWidget(SFXVolume);
        SettingsMenu.AddWidget(MusicVolume);
        SettingsMenu.AddWidget(UIScale);
        SettingsMenu.AddWidget(ShaderToggle as IFunctional);

        GarageMenu.AddWidget(MothershipScrap);
        GarageMenu.AddWidget(RepairButton as IFunctional);
        GarageMenu.AddWidget(RepairSlot as IFunctional);
        GarageMenu.AddWidget(RepairText);
        GarageMenu.AddWidget(GaragePlayerImage);
        GarageMenu.AddWidget(ValidConfigText);

        MothershipMenu.AddWidget(FurnaceSlider as IFunctional, 0);
        MothershipMenu.AddWidget(FurnaceSlot as IFunctional, 0);
        MothershipMenu.AddWidget(GarageButton as IFunctional, 1);
        MothershipMenu.AddWidget(CraftingSlider as IFunctional, 2);
        MothershipMenu.AddWidget(RequiredCraftsText, 2);
        MothershipMenu.AddWidget(CraftButton as IFunctional, 2);
        for (int i = 0; i < 3; i++)
        {
            MothershipMenu.AddWidget(SidePanelClose as IFunctional, i);
        }
        MothershipMenu.AddWidget(Overlay, 0);
        MothershipMenu.AddWidget(Overlay, 1);
        MothershipMenu.AddWidget(Overlay, 2);

        PlayerMenu.AddWidget(EnemySlider as IFunctional);
        PlayerMenu.AddWidget(WaveText);
        PlayerMenu.AddWidget(SidePanelClose as IFunctional);
        PlayerMenu.AddWidget(EnemiesLeft);
        PlayerMenu.AddWidget(Overlay);

        MissionSelect.AddWidget(MissionName, 0);
        MissionSelect.AddWidget(MissionDescription, 0);
        MissionSelect.AddWidget(PrevMission as IFunctional, 0);
        MissionSelect.AddWidget(NextMission as IFunctional, 0);
        MissionSelect.AddWidget(SelectMission as IFunctional, 0);
        MissionSelect.AddWidget(IsComplete, 0);
        MissionSelect.AddWidget(ValidConfigText, 1);
        MissionSelect.AddWidget(CreateFuse as IFunctional, 1);
        MissionSelect.AddWidget(SmeltScrap as IFunctional, 1);
        MissionSelect.AddWidget(RepairModule as IFunctional, 1);
        MissionSelect.AddWidget(CancelQueue as IFunctional, 1);
        MissionSelect.AddWidget(SaveButton as IFunctional, 0);
        MissionSelect.AddWidget(AlertText, 0);

        PickupDroneMenu.AddWidget(LaunchButton as IFunctional);

        SaveMenu.AddWidget(SaveToFile as IFunctional);
        SaveMenu.AddWidget(PrevSave as IFunctional);
        SaveMenu.AddWidget(NextSave as IFunctional);
        SaveMenu.AddWidget(DeleteSave as IFunctional);
        SaveMenu.AddWidget(Name as IFunctional);
        SaveMenu.AddWidget(LoadedName);
        SaveMenu.AddWidget(SaveBack as IFunctional);

        LoadMenu.AddWidget(LoadFromFile as IFunctional);
        LoadMenu.AddWidget(PrevSave as IFunctional);
        LoadMenu.AddWidget(NextSave as IFunctional);
        LoadMenu.AddWidget(DeleteSave as IFunctional);
        LoadMenu.AddWidget(LoadedName);
        LoadMenu.AddWidget(LoadBack as IFunctional);

        UpgradeMenu.AddWidget(TraderChat, 0);
        UpgradeMenu.AddWidget(LidarUpgrade as IFunctional, 1);
        UpgradeMenu.AddWidget(RadarUpgrade as IFunctional, 1);
        UpgradeMenu.AddWidget(PulseEmitterUpgrade as IFunctional, 1);
        UpgradeMenu.AddWidget(UpgradeText, 2);
        UpgradeMenu.AddWidget(UpgradeHull as IFunctional, 2);
        UpgradeMenu.AddWidget(UpgradeGuns as IFunctional, 2);
        UpgradeMenu.AddWidget(UpgradeEngine as IFunctional, 2);
        UpgradeMenu.AddWidget(UpgradeCore as IFunctional, 2);

        GlobalMenu.AddWidget(GlobalSidePanelOpen as IFunctional, (int)Alignment.Top);
        GlobalMenu.AddWidget(Timer, (int)Alignment.TopRight);
        GlobalMenu.AddWidget(PlayerHealth as IFunctional, (int)Alignment.TopLeft);
        GlobalMenu.AddWidget(PlayerSpecialHealth as IFunctional, (int)Alignment.TopLeft);
        GlobalMenu.AddWidget(PlayerAbility as IFunctional, (int)Alignment.TopLeft);
        GlobalMenu.AddWidget(PlayerAmmo as IFunctional, (int)Alignment.TopLeft);
        PlayerSpecialHealth.SetInterval(1, 1);
        PlayerHealth.Intervals = [1, 1];

        for (int x = 0; x < ModuleSlots.GetLength(0); x++)
        {
            if (x % 2 == 0)
            {
                ModuleSlots[x] = new ItemSlot<Module>(new Vector2(-30, Assets.DimsOf(Sprite.EmptySlot).Y * x / 2
                    - Assets.DimsOf(Sprite.EmptySlot).Y), Assets.Get(Sprite.EmptySlot), Engine.UIManager, x);
            }
            else
            {
                ModuleSlots[x] = new ItemSlot<Module>(new Vector2(Assets.DimsOf(Sprite.EmptySlot).X / 1.4142f - 30,
                    Assets.DimsOf(Sprite.EmptySlot).Y * x / 2 - Assets.DimsOf(Sprite.EmptySlot).Y), Assets.Get(Sprite.EmptySlot), Engine.UIManager, x);
            }
            GarageMenu.AddWidget(ModuleSlots[x] as IFunctional);
            MissionSelect.AddWidget(ModuleSlots[x] as IFunctional, 1);
            ModuleSlots[x].AddBehaviour(EventHandler.UpdateModules);
        }
        for (int i = 0; i < InventorySlots.GetLength(0); i++)
        {
            InventorySlots[i] = new ItemSlot<Pickup>(new Vector2(Assets.DimsOf(Sprite.LargePanel).X / 4,
                Assets.DimsOf(Sprite.EmptySlot).Y * (i + 1) - Assets.DimsOf(Sprite.LargePanel).X / 2), Assets.Get(Sprite.EmptySlot), Engine.UIManager, -1);
            MissionSelectSlots[i] = new ItemSlot<Pickup>(new Vector2(Assets.DimsOf(Sprite.LargePanel).X / 2,
                Assets.DimsOf(Sprite.EmptySlot).Y * (i + 1) - Assets.DimsOf(Sprite.LargePanel).X / 2), Assets.Get(Sprite.EmptySlot), Engine.UIManager, -1);
            MothershipMenu.AddWidget(InventorySlots[i] as IFunctional, 0);
            PickupDroneMenu.AddWidget(InventorySlots[i] as IFunctional);
            MissionSelect.AddWidget(InventorySlots[i] as IFunctional, 1);
            MissionSelect.AddWidget(MissionSelectSlots[i] as IFunctional, 1);
            InventorySlots[i].AddBehaviour(EventHandler.UpdateInventory);
            MissionSelectSlots[i].AddBehaviour(EventHandler.UpdateInventory);
        }
        GarageMenu.AddWidget(SecondarySlot as IFunctional);
        MissionSelect.AddWidget(SecondarySlot as IFunctional, 1);

        GlobalMenu.AddWidget(RestartSwitch as IFunctional, (int)Alignment.Center);
        GlobalMenu.AddWidget(RestartSlider as IFunctional, (int)Alignment.Center);
        GlobalMenu.AddWidget(Switch, (int)Alignment.Center);
        GlobalMenu.AddWidget(FuseCounter, (int)Alignment.Center);
        for (int i = 0; i < 4; i++)
        {
            for (int j = -2; j < 3; j++)
            {
                var fuse = new Button(new Vector2(i * 10 + Engine.BackBuffer.X / 2, j * 20), Assets.Get(Sprite.Fuse));
                //Not sure why this works, don't touch
                int x = j + 2;
                int y = i;
                fuse.AddBehaviour(delegate () 
                { 
                    Engine.SaveGame.Player.ToggleFuse(x, y);
                });
                Fuses[i, j + 2] = fuse;
                GlobalMenu.AddWidget(fuse as IFunctional, (int)Alignment.Center);
            }
        }
        GlobalMenu.AddWidget(FragilityTextbox, (int)Alignment.Center);
        for (int i = 0; i < 5; i++)
        {
            float y = (i - 2) * 20;
            GlobalMenu.AddWidget(ModuleIcons[i] = new Decal(new Vector2(-15 + Engine.BackBuffer.X / 2, y), null), (int)Alignment.Center);
            GlobalMenu.AddWidget(StatusLights[i] = new Decal(new Vector2(-30 + Engine.BackBuffer.X / 2, y), Assets.Get(Sprite.Circle)), (int)Alignment.Center);
        }

        HackMenu.AddWidget(HackButton as IFunctional);
        HackMenu.AddWidget(HackTimer as IFunctional);

        Engine.UIManager.AddContainer(MainMenu);
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

        Engine.UIManager.ScreenWindow = GlobalMenu;
    }
}
