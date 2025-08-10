using System;
using UILib.Content.Main;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Space_Wars.Content.Main.Entities;
using static Space_Wars.Content.Main.Engine;
using System.Collections.Generic;

namespace Space_Wars.Content.Main;
public static class UI
{
    private static Vector2 center = Engine.BackBuffer / 2;
    public static Window PauseMenu { get; } = new Window(center, Assets.Get(Sprite.LargePanel));
    public static Window PlayerMenu { get; } = new Window(new Vector2(0, center.Y), Assets.Get(Sprite.Terminal)) { alignment = Alignment.Left };
    public static Window GarageMenu { get; } = new Window(center, Assets.Get(Sprite.GargantuanPanel));
    public static TabbedWindow MainMenu { get; } = new TabbedWindow(center, Assets.Get(Sprite.GargantuanPanel), Assets.Get(Sprite.Tab), Assets.Get(Sprite.SelectedTab), Assets.Get(Sound.Interact), 2)
    { enabled = true, icons = [Assets.Get(Sprite.PlayIcon), Assets.Get(Sprite.SettingsIcon)] };
    public static TabbedWindow MothershipMenu { get; } = new TabbedWindow(new Vector2(0, center.Y), Assets.Get(Sprite.Terminal), Assets.Get(Sprite.Tab), Assets.Get(Sprite.SelectedTab), Assets.Get(Sound.Interact), 3)
    { icons = [Assets.Get(Sprite.SmeltIcon), Assets.Get(Sprite.RepairIcon), Assets.Get(Sprite.VictoryIcon)], alignment = Alignment.Left };
    public static TabbedWindow MissionSelect { get; } = new TabbedWindow(new Vector2(0, center.Y), Assets.Get(Sprite.GargantuanPanel), Assets.Get(Sprite.Tab), Assets.Get(Sprite.SelectedTab), Assets.Get(Sound.Interact), 2)
    { icons = [Assets.Get(Sprite.PlanetIcon), Assets.Get(Sprite.RepairIcon)], alignment = Alignment.Left };
    public static Window PickupDroneMenu { get; } = new Window(center, Assets.Get(Sprite.LargePanel));
    public static Window FuseMenu { get; } = new Window(center, Assets.Get(Sprite.LargePanel));
    public static Window SaveMenu { get; } = new Window(center, Assets.Get(Sprite.GargantuanPanel));
    public static Window LoadMenu { get; } = new Window(center, Assets.Get(Sprite.GargantuanPanel));

    //Main Menu Widgets
    public static Button PatchedConicsToggle { get; } = new Button(new Vector2(0, -MainMenu.Size.Y / 4), Assets.Get(Sprite.WideButton), Assets.TextFont, $"Patched Conics: {PatchedConics}", Color.White);
    public static Slider SFXSlider { get; } = new Slider(Engine.Line, Assets.Get(Sprite.Knob), new Vector2(25, 0), new Vector2(50, 2), false, Color.White, Color.Gray);
    public static Slider MusicSlider { get; } = new Slider(Engine.Line, Assets.Get(Sprite.Knob), new Vector2(25, -15), new Vector2(50, 2), false, Color.White, Color.Gray);
    public static Slider UIScaleSlider { get; } = new Slider(Engine.Line, Assets.Get(Sprite.Knob), new Vector2(25, 15), new Vector2(50, 2), false, Color.White, Color.Gray);
    public static Decal SFXVolume { get; } = new Decal(new Vector2(-35, 0), Assets.TextFont, "Sound: 100%", Color.White, 5);
    public static Decal MusicVolume { get; } = new Decal(new Vector2(-35, -15), Assets.TextFont, "Music: 100%", Color.White, 5);
    public static Decal UIScale { get; } = new Decal(new Vector2(-35, 15), Assets.TextFont, $"UI Scale: {Math.Truncate((UIScaleSlider.sliderInterval + 1) * 10) / 10}", Color.White, 5);
    public static Button ShaderToggle { get; } = new Button(new Vector2(0, MainMenu.Size.Y / 4), Assets.Get(Sprite.WideButton), Assets.TextFont, $"Shader: {UseShader}", Color.White);
    public static Button SingleplayerButton { get; } = new Button(new Vector2(0, -MainMenu.Size.Y / 4), Assets.Get(Sprite.WideButton), Assets.TextFont, "Singleplayer", Color.White);
    public static Button ExitButton { get; } = new Button(new Vector2(0, MainMenu.Size.Y / 4), Assets.Get(Sprite.WideButton), Assets.TextFont, "Exit", Color.White);
    public static Button QuitToMissionButton { get; } = new Button(new Vector2(0, -20), Assets.Get(Sprite.WideButton), Assets.TextFont, "Return", Color.White);
    public static Decal TitleName { get; } = new Decal(new Vector2(0, -MainMenu.Size.Y), Assets.Get(Sprite.Title));
    public static Button LoadButton { get; } = new Button(new Vector2(0, 0), Assets.Get(Sprite.WideButton), Assets.TextFont, "Load", Color.White);

    //Mothership Menu
    public static ItemSlot<Pickup> FurnaceSlot { get; } = new ItemSlot<Pickup>(new Vector2(-20, 0), Assets.Get(Sprite.EmptySlot), Engine.UIManager, -1);
    public static Button GarageButton { get; } = new Button(new Vector2(0, -MainMenu.Size.Y / 4), Assets.Get(Sprite.WideButton), Assets.TextFont, "To Garage", Color.White);
    public static Button CraftButton { get; } = new Button(new Vector2(0, MainMenu.Size.Y / 4), Assets.Get(Sprite.Button), Assets.TextFont, "Repair", Color.LightBlue);
    public static Decal RequiredCraftsText { get; } = new Decal(new Vector2(0) + new Vector2(0, -6), Assets.TextFont, "25", Color.White, 10);
    public static Slider FurnaceSlider { get; } = new Slider(Engine.Line, new Vector2(-20, -MainMenu.Size.Y / 6), new Vector2(60, 2), true, new Color(255, 239, 85), new Color(50, 51, 67));
    public static Slider CraftingSlider { get; } = new Slider(Engine.Line, new Vector2(0, -MainMenu.Size.Y / 4), new Vector2(60, 2), true, Color.Cyan, Color.Gray);

    //Garage Menu
    public static Button RepairButton { get; } = new Button(new Vector2(-GarageMenu.Size.X / 4 - 25, -40), Assets.Get(Sprite.Button), Assets.TextFont, "Repair", Color.LightBlue);
    public static ItemSlot<Module> RepairSlot { get; } = new ItemSlot<Module>(new Vector2(-GarageMenu.Size.X / 4 - 25, 0), Assets.Get(Sprite.EmptySlot), Engine.UIManager, -1);
    public static Decal MothershipScrap { get; } = new Decal(new Vector2(GarageMenu.Size.X / 2.2f, 20) - GarageMenu.Size / 2, Assets.TextFont, "0", Color.Gray, 10);
    public static Decal RepairText { get; } = new Decal(new Vector2(-GarageMenu.Size.X / 4 - 60 / 2.5f, 40), Assets.TextFont, "", Color.White, 10);
    public static Decal GaragePlayerImage { get; } = new Decal(new Vector2(GarageMenu.Size.X / 4, 0), Assets.Get(Sprite.PlayerUI));
    public static Decal ValidConfigText { get; } = new Decal(-GarageMenu.Size / 4 + new Vector2(20, GarageMenu.Size.Y / 1.5f), Assets.TextFont, "Ready for Combat", Color.Green, 10);

    //Player Menu
    public static Slider EnemySlider { get; } = new Slider(Engine.Line, new Vector2(0, -PlayerMenu.Size.Y / 3), new Vector2(50, 2), true, Color.White, Color.Gray);
    public static Decal WaveText { get; } = new Decal(new Vector2(-5, 0), Assets.TextFont, "0", Color.White, 10);
    public static Decal PartStatus { get; } = new Decal(new Vector2(0, 20), Assets.TextFont, "All systems go", Color.Green, 10);
    public static Button RestartButton { get; } = new Button(new Vector2(0, 50), Assets.Get(Sprite.Button), Assets.TextFont, "Restart", Color.LightBlue);
    public static Slider RestartSlider { get; } = new Slider(Engine.Line, new Vector2(0, 63), new Vector2(50, 2), true, Color.Cyan, Color.Black);

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

    //Fuse Menu
    public static Decal FuseCounter { get; } = new Decal(new Vector2(-20, -10), Assets.TextFont, "0", Color.Yellow, 10);
    public static Button[,] Fuses { get; } = new Button[4, 5];
    public static List<Decal> ModuleIcons { get; } = [];

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
    public static Slider PlayerHealth { get; } = new Slider(Engine.Line, new Vector2(5, 5), new Vector2(150, 15), true, Color.Red, Color.DarkGray);
    public static Slider PlayerAbility { get; } = new Slider(Engine.Line, new Vector2(5, 15), new Vector2(100, 10), true, Color.Cyan, Color.DarkGray);

    //Misc
    public static Button SidePanelClose { get; } = new Button(new Vector2(-Assets.Get(Sprite.ToggleButton).Width / 2 + Assets.Get(Sprite.Terminal).Width / 2, 0), Assets.Get(Sprite.ToggleButton));
    public static ItemSlot<Pickup>[] InventorySlots { get; set; } = new ItemSlot<Pickup>[4];
    public static ItemSlot<Pickup>[] MissionSelectSlots { get; set; } = new ItemSlot<Pickup>[4];
    public static ItemSlot<Module>[] ModuleSlots { get; private set; } = new ItemSlot<Module>[5];

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
            float i = MusicSlider.sliderInterval;
            SoundManager.MusicVolume = i;
            MusicVolume.text = $"Music: {Math.Round(i * 100)}%";
        });
        SFXSlider.SetInterval(1, 1);
        MusicSlider.SetInterval(1, 1);
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
        RestartButton.AddBehaviour(delegate () { EventHandler.SendMessage(Message.RestartModules); });
        GlobalSidePanelOpen.AddBehaviour(EventHandler.ToggleDockingMenus);
        SidePanelClose.AddBehaviour(EventHandler.ToggleDockingMenus);
        PrevMission.AddBehaviour(delegate () { Engine.SaveGame.PrevMission(); });
        NextMission.AddBehaviour(delegate () { Engine.SaveGame.NextMission(); });
        SelectMission.AddBehaviour(delegate () { if (EventHandler.SyncModules()) { Startgame(); } });
        LaunchButton.AddBehaviour(delegate () { EventHandler.SendMessage(Message.EscapeDroneLeave); });
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
        tooltip.AddWidget(new Decal(new Vector2(0, -3), Assets.TextFont, "Drag module over button to queue repair.", Color.White, 3f));
        tooltip.AddWidget(new Decal(new Vector2(0, 3), Assets.TextFont, "Required time: 20 waves. Requires no metal to repair.", Color.White, 3f));
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

        PauseMenu.AddWidget(QuitToMissionButton as IFunctional);

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

        PlayerMenu.AddWidget(EnemySlider as IFunctional);
        PlayerMenu.AddWidget(WaveText);
        PlayerMenu.AddWidget(PartStatus);
        PlayerMenu.AddWidget(RestartButton as IFunctional);
        PlayerMenu.AddWidget(RestartSlider as IFunctional);
        PlayerMenu.AddWidget(SidePanelClose as IFunctional);

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

        FuseMenu.AddWidget(FuseCounter);
        for (int i = 0; i < 4; i++)
        {
            for (int j = -2; j < 3; j++)
            {
                var fuse = new Button(new Vector2(i * 10, j * 20), Assets.Get(Sprite.Fuse));
                //Performs a shallow copy of the instance variable
                int x = j + 2;
                int y = i;
                fuse.AddBehaviour(delegate () { Engine.SaveGame.Player.ToggleFuse(x, y); });
                Fuses[i, j + 2] = fuse;
                FuseMenu.AddWidget(fuse as IFunctional);
            }
        }
        for (int i = 0; i < 5; i++)
        {
            var decal = new Decal(new Vector2(40, (i - 2) * 20 - 8), null);
            ModuleIcons.Add(decal);
            FuseMenu.AddWidget(decal);
        }

        Engine.UIManager.ScreenWindow.AddWidget(GlobalSidePanelOpen as IFunctional, (int)Alignment.Left);
        Engine.UIManager.ScreenWindow.AddWidget(Timer, (int)Alignment.TopRight);
        Engine.UIManager.ScreenWindow.AddWidget(PlayerHealth as IFunctional, (int)Alignment.TopLeft);
        Engine.UIManager.ScreenWindow.AddWidget(PlayerAbility as IFunctional, (int)Alignment.TopLeft);

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
        Engine.UIManager.AddContainer(MainMenu);
        Engine.UIManager.AddContainer(PauseMenu);
        Engine.UIManager.AddContainer(PlayerMenu);
        Engine.UIManager.AddContainer(MothershipMenu);
        Engine.UIManager.AddContainer(GarageMenu);
        Engine.UIManager.AddContainer(MissionSelect);
        Engine.UIManager.AddContainer(PickupDroneMenu);
        Engine.UIManager.AddContainer(FuseMenu);
        Engine.UIManager.AddContainer(SaveMenu);
        Engine.UIManager.AddContainer(LoadMenu);
    }
}
