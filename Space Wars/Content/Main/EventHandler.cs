
using Microsoft.Xna.Framework;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;
using UILib.Content.Main;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System;
using Space_Wars.Content.Main.Components;

namespace Space_Wars.Content.Main;

public class EventHandler
{
    private static Player player;
    private static readonly List<Message> eventLog = new();

    public static bool AcknowledgeMessage(Message _message)
    {
        return eventLog.Remove(_message);
    }
    public static bool SendMessage(Message _message)
    {
        if(eventLog.Contains(_message))
        {
            return false;
        }
        eventLog.Add(_message);
        return true;
    }
    public static void InitializeGameSpace(Player _player)
    {
        player = _player;
    }
    public static void QuitToMenu()
    {
        player.velocity = Vector2.Zero;
        Engine.IngameTime = new();
        Engine.MousePositionOffset = Vector2.Zero;
        Engine.UIManager.ToggleMenu((int)Containers.PauseMenu);
        Engine.UIManager.ToggleMenu((int)Containers.MainMenu);
        ParticleManager.Initialize();
        SoundManager.SetAllSounds(false);
        SoundManager.Initialize(player);  
        SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
        Engine.Camera.Position = Vector2.Zero;
        CurrentGameState.SwitchState(new MainMenu());
    }
    public static void RepairModule()
    {
        var repairSlot = Engine.UIManager.GetFuncWidget((int)Containers.GarageMenu, 1) as ItemSlot<Module>;
        Module daughterModule;
        if (repairSlot.daughterItem != null)
        {
            daughterModule = repairSlot.daughterItem;
        }
        else
        { 
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Fail));
            return;
        }
        if (daughterModule.Health < 20 && EntityManager.CurrentMission.MissionScrap >= 1)
        {
            daughterModule.Health = 20;
            daughterModule.isFailed = false;
            EntityManager.CurrentMission.MissionScrap -= 1;
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
            UpdateRepairText();
            Decal mothershipScrap = Engine.UIManager.GetWidget((int)Containers.GarageMenu, 0) as Decal;
            mothershipScrap.text = EntityManager.CurrentMission.MissionScrap.ToString();
        }
        else
        {
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Fail));
        }
    }
    public static void UpdateRepairText()
    {
        Decal repairText = Engine.UIManager.GetWidget((int)Containers.GarageMenu, 1) as Decal;
        var slot = Engine.UIManager.GetFuncWidget((int)Containers.GarageMenu, 1) as ItemSlot<Module>;
        Module daughterModule = slot.daughterItem;
        if (daughterModule != null)
        {
            repairText.text = $"{daughterModule.Health}/20";
        }
        else
        {
            repairText.text = "";
        }
    }

    public static void UpdateInventoryUI(DockableComponent dockableComponent)
    {
        for (int y = 0; y < dockableComponent.Inventory.GetLength(1); y++)
        {
            for (int x = 0; x < dockableComponent.Inventory.GetLength(0); x++)
            {
                Engine.InventorySlots[x, y].daughterItem = dockableComponent.Inventory[x, y];
            }
        }
    }
    public static void UpdateInventory()
    {
        SendMessage(Message.MothershipUpdateInventory);
    }
    public static void UpdateModulesUI()
    {
        for (int x = 0; x < Engine.ModuleSlots.Length; x++)
        {
            Engine.ModuleSlots[x].daughterItem = player.modules.ElementAt(x).Value;
        }
    }
    public static void UpdateModules()
    {
        Decal validConfigText = Engine.UIManager.GetWidget((int)Containers.GarageMenu,3) as Decal;
        for (int x = 0; x < player.modules.Count; x++)
        {
            player.modules[(ModuleType)x] = Engine.ModuleSlots[x].daughterItem;
        }
        foreach (KeyValuePair<ModuleType, Module> module in player.modules)
        {
            if (module.Value == null)
            {
                validConfigText.text = "";
                return;
            }
        }
        validConfigText.text = "Ready for Combat";
    }
    public static void UpdateFurnaceUI(float _value, float _maxValue, Pickup furnaceItem)
    {
        var furnaceSlot = Engine.UIManager.GetFuncWidget((int)Containers.MothershipMenu, 1) as ItemSlot<Pickup>;
        furnaceSlot.daughterItem = furnaceItem;
        (Engine.UIManager.GetFuncWidget((int)Containers.MothershipMenu,0) as Slider).SetInterval(_value, _maxValue);
        (Engine.UIManager.GetWidget((int)Containers.GarageMenu, 0) as Decal).text = EntityManager.CurrentMission.MissionScrap.ToString();
    }
    public static void UpdateFurnace()
    {
        SendMessage(Message.MothershipUpdateFurnace);
    }
    public static void CraftItem()
    {
        if(EntityManager.CurrentMission.MissionScrap >= 1)
        {
            EntityManager.CurrentMission.MissionScrap -= 1;
            SendMessage(Message.MothershipCraftItem);
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
        }
        else
        {
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Fail));
        }
    }
    public static void UpdateCraftingUI(float _value, float _maxValue, int requiredCraftsLeft)
    {
        (Engine.UIManager.GetFuncWidget((int)Containers.MothershipMenu,3) as Slider).SetInterval(_value, _maxValue);
        (Engine.UIManager.GetWidget((int)Containers.GarageMenu, 0) as Decal).text = EntityManager.CurrentMission.MissionScrap.ToString();
        (Engine.UIManager.GetWidget((int)Containers.MothershipMenu,0) as Decal).text = requiredCraftsLeft.ToString();
    }
    public static void UpdateRestartSlider(float _value, float _maxValue)
    {
        (Engine.UIManager.GetFuncWidget((int)Containers.PlayerMenu, 2) as Slider).SetInterval(_value, _maxValue);
    }
    public static void UpdateEnemyCountdownUI(float _value, float _maxValue, float _wave)
    {
        (Engine.UIManager.GetFuncWidget((int)Containers.PlayerMenu,0) as Slider).sliderInterval = _value / _maxValue;
        (Engine.UIManager.GetWidget((int)Containers.PlayerMenu,0) as Decal).text = $"{_wave}";
    }
    public static void GarageTrigger()
    {
        foreach(KeyValuePair<ModuleType, Module> module in player.modules)
        {
            if(module.Value == null)
            {
                return;
            }
        }
        SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
        Engine.UIManager.ToggleMenu((int)Containers.MothershipMenu);
        Engine.UIManager.ToggleMenu((int)Containers.GarageMenu);
        if (Engine.UIManager.GetContainer((int)Containers.GarageMenu).enabled)
        {
            CurrentGameState.SwitchState(new Garage());
        }
        else
        {
            CurrentGameState.SwitchState(new PlayingGame());
        }
    }
    public static void MissionSelectTrigger()
    {
        SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
        Container missionSelect = Engine.UIManager.GetContainer((int)Containers.MissionMenu);
        if (!Engine.UIManager.ToggleToMenu(missionSelect))
        {
            missionSelect.enabled = !missionSelect.enabled;
        }
        if (missionSelect.enabled)
        {
            CurrentGameState.SwitchState(new MissionSelect());
            return;
        }
        CurrentGameState.SwitchState(new PlayingGame());
    }
    public static void UpdateMissionText()
    {
        Container missionSelect = Engine.UIManager.GetContainer((int)Containers.MissionMenu);
        Mission mission = EntityManager.CurrentMission;
        (missionSelect.GetWidget(0) as Decal).text = mission.Name;
        (missionSelect.GetWidget(1) as Decal).text = mission.Description;
        (missionSelect.GetWidget(2) as Decal).text = mission.Completed ? "Completed" : "Not Completed";
        (missionSelect.GetWidget(2) as Decal).textColor = mission.Completed ? Color.Green : Color.Red;
    }
    public static void UpdateScrapText()
    {
        (Engine.UIManager.GetWidget((int)Containers.GarageMenu, 0) as Decal).text = EntityManager.CurrentMission.MissionScrap.ToString();
    }
    public static void UpdateModulesStatus()
    {
        string text = "";
        foreach (var module in player.modules)
        {
            if (module.Value.isFailed)
            {
                text = text + module.Key + ", ";
            }
        }
        Decal textSource = Engine.UIManager.GetWidget((int)Containers.PlayerMenu, 1) as Decal;
        if (text == "")
        {
            textSource.text = "All systems go!";
            textSource.textColor = Color.Green;
        }
        else
        {
            text = text.Remove(text.Length - 2, 1);
            text += "has failed!";
            textSource.text = text;
            textSource.textColor = Color.Red;
        }
    }
    public static void DisableDockingMenus()
    {
        Engine.UIManager.GetContainer((int)Containers.MothershipMenu).enabled = false;
        Engine.UIManager.GetContainer((int)Containers.PickupDroneMenu).enabled = false;
        Engine.UIManager.GetContainer((int)Containers.PlayerMenu).enabled = false;
    }
    public static void ToggleDockingMenus()
    {
        SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
        SendMessage(Message.ToggleTerminal);
    }
    public static void RestartModules()
    {
        SendMessage(Message.RestartModules);
    }
    public static void UpdateFuseUI(bool[,] _fuses, int _fusesRemaining)
    {
        var menu = Engine.UIManager.GetContainer((int)Containers.FuseMenu);
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                (menu.GetFuncWidget(i + j * 5) as Widget).color = _fuses[i, j] ? Color.White : Color.Gray;
            }
        }
        menu.GetWidget(0).text = $"{_fusesRemaining}";
    }
}
