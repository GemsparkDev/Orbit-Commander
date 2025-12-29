
using Microsoft.Xna.Framework;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;
using UILib.Content.Main;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System;

namespace Space_Wars.Content.Main;

public static class EventHandler
{
    private static Player player => Engine.SaveGame.Player;
    private static readonly List<Message> eventLog = [];

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
    public static void QuitToMenu()
    {
        player.velocity = Vector2.Zero;
        Engine.IngameTime = new();
        Engine.MousePositionOffset = Vector2.Zero;
        Engine.UIManager.DisableAll();
        UI.MainMenu.enabled = true;
        ParticleManager.Initialize();
        SoundManager.SetAllSounds(false);
        SoundManager.Initialize();  
        SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
        Engine.Camera.Position = Vector2.Zero;
        CurrentGameState.SwitchState(new MainMenu());
    }
    public static void RepairModule()
    {
        Module daughterModule;
        if (UI.RepairSlot.daughterItem != null)
        {
            daughterModule = UI.RepairSlot.daughterItem;
        }
        else
        { 
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Fail));
            return;
        }
        if (daughterModule.Health < 20 && Engine.SaveGame.Scrap >= 1)
        {
            daughterModule.Health = 20;
            daughterModule.isFailed = false;
            Engine.SaveGame.Scrap -= 1;
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
            UpdateRepairText();
            UI.MothershipScrap.text = Engine.SaveGame.Scrap.ToString();
        }
        else
        {
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Fail));
        }
    }
    public static void UpdateRepairText()
    {
        Module daughterModule = UI.RepairSlot.daughterItem;
        if (daughterModule != null)
        {
            UI.RepairText.text = $"{daughterModule.Health}/20";
        }
        else
        {
            UI.RepairText.text = "";
        }
    }
    public static void UpdateInventoryUI()
    {
        for (int i = 0; i < UI.InventorySlots.Length; i++)
        {
            UI.InventorySlots[i].daughterItem = Engine.SaveGame.Inventory[i];
        }
        for (int i = 0; i < UI.MissionSelectSlots.Length; i++)
        {
            UI.MissionSelectSlots[i].daughterItem = Engine.SaveGame.MissionSelectInventory[i];
        }
    }
    public static void UpdateInventory()
    {
        for (int i = 0; i < UI.InventorySlots.Length; i++)
        {
            Engine.SaveGame.Inventory[i] = UI.InventorySlots[i].daughterItem;
        }
        for (int i = 0; i < UI.MissionSelectSlots.Length; i++)
        {
            Engine.SaveGame.MissionSelectInventory[i] = UI.MissionSelectSlots[i].daughterItem;
        }
    }
    public static void UpdateModulesUI()
    {
        for (int x = 0; x < UI.ModuleSlots.Length; x++)
        {
            UI.ModuleSlots[x].daughterItem = player.modules.ElementAt(x).Value;
        }
        UI.SecondarySlot.daughterItem = player.SecondaryWeapon;
    }
    public static void UpdateModules()
    {
        foreach (var module in UI.ModuleSlots)
        {
            if (module.daughterItem == null)
            {
                UI.ValidConfigText.text = "";
                return;
            }
        }
        UI.ValidConfigText.text = "Ready for Combat";
    }
    public static bool SyncModules()
    {
        foreach (var module in UI.ModuleSlots)
        {
            if (module.daughterItem == null)
            {
                return false;
            }
        }
        for (int x = 0; x < player.modules.Count; x++)
        {
            player.modules[(ModuleType)x] = UI.ModuleSlots[x].daughterItem;
        }
        player.SecondaryWeapon = UI.SecondarySlot.daughterItem;
        return true;
    }
    public static void UpdateFurnaceUI(float _value, float _maxValue, Pickup furnaceItem)
    {
        UI.FurnaceSlot.daughterItem = furnaceItem;
        UI.FurnaceSlider.SetInterval(_value, _maxValue);
        UI.MothershipScrap.text = Engine.SaveGame.Scrap.ToString();
    }
    public static void UpdateFurnace()
    {
        SendMessage(Message.MothershipUpdateFurnace);
    }
    public static void CraftItem()
    {
        if(Engine.SaveGame.Scrap >= 1)
        {
            Engine.SaveGame.Scrap -= 1;
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
        UI.CraftingSlider.SetInterval(_value, _maxValue);
        UI.MothershipScrap.text = Engine.SaveGame.Scrap.ToString();
        UI.RequiredCraftsText.text = requiredCraftsLeft.ToString();
    }
    public static void UpdateRestartSlider(float _value, float _maxValue)
    {
        UI.RestartSlider.SetInterval(_value, _maxValue);
    }
    public static void UpdateEnemyCountdownUI(float _value, float _maxValue, float _wave)
    {
        UI.EnemySlider.Intervals[0] = _value / _maxValue;
        UI.WaveText.text = $"{_wave}";
    }
    public static void GarageTrigger()
    {
        SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
        UI.MothershipMenu.enabled = !UI.MothershipMenu.enabled;
        UI.GarageMenu.enabled = !UI.GarageMenu.enabled;
        if (UI.GarageMenu.enabled)
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
        if (!Engine.UIManager.ToggleToMenu(UI.MissionSelect))
        {
            UI.MissionSelect.enabled = !UI.MissionSelect.enabled;
        }
        if (UI.MissionSelect.enabled)
        {
            CurrentGameState.SwitchState(new MissionSelect());
            return;
        }
        CurrentGameState.SwitchState(new PlayingGame());
    }
    public static void UpdateMissionText()
    {
        Mission mission = Engine.SaveGame.CurrentMission;
        bool completed = Engine.SaveGame.CurrentMissionCompleted;
        bool isDangerous = Engine.SaveGame.FleetSystem > Engine.EntityManager.Systems[Engine.SaveGame.CurrentMissionIndex].system;
        UI.MissionName.text = mission.Name;
        UI.MissionDescription.text = mission.Description;
        UI.IsComplete.text = completed ? "Completed" : "Not Completed";
        UI.IsComplete.textColor = completed ? Color.Green : Color.Red;
        UI.SelectMission.textColor = completed && mission.relaunchable ? Color.Gray : Color.Yellow;
        UI.AlertText.text = isDangerous ? "Danger: Fleet Detected" : "";
    }
    public static void UpdateScrapText()
    {
        UI.MothershipScrap.text = Engine.SaveGame.Scrap.ToString();
    }
    public static void UpdateModulesStatus()
    {
        for (int i = 0; i < UI.StatusLights.Length; i++)
        {
            if(!Engine.SaveGame.Player.IsEnabled)
            {
                UI.StatusLights[i].color = new Color(50, 50, 50);
                continue;
            }
            UI.StatusLights[i].color = (Engine.SaveGame.Player.modules[(ModuleType)i].isFailed ? Color.Red : Color.Green);
        }
    }
    public static void DisableDockingMenus()
    {
        UI.MothershipMenu.enabled = false;
        UI.PickupDroneMenu.enabled = false;
        UI.PlayerMenu.enabled = false;
    }
    public static void ToggleDockingMenus()
    {
        SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
        SendMessage(Message.ToggleTerminal);
    }
    public static void UpdateFuseUI(bool[,] _fuses, int _spareFuses)
    {
        float totalFuses = 0;
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                //Active fuse is white, no fuse is gray, disabled fuse is red
                Color color;
                if (!_fuses[i, j])
                {
                    color = Color.Gray;
                }
                else if (!_fuses[(int)ModuleType.Core, j])
                {
                    color = Color.Red;
                }
                else
                {
                    color = Color.White;
                    totalFuses++;
                }
                UI.Fuses[j, i].color = color;
            }
        }
        UI.FuseCounter.text = $"{_spareFuses}";
        string text;
        if (totalFuses < 13)
        {
            text = "Low";
        }
        else if (totalFuses is >= 13 and <= 17)
        {
            text = "Med";
        }
        else
        {
            text = "High";
        }
        UI.FragilityTextbox.text = "Fuse Fragility: " + text;
        UI.FragilityTextbox.textColor = new Color(Math.Clamp(totalFuses / 5 - 2, 0, 1), Math.Clamp(4 - totalFuses / 5, 0, 1), 0);
        Color[] possibleColors = [ Color.Red, Color.Orange, Color.Yellow, Color.White, Color.Cyan ];
        for (int i = 0; i < 5; i++)
        {
            var decal = UI.ModuleIcons[i];
            int count = 0;
            for (int j = 0; j < 4; j++)
            {
                switch (i)
                {
                    case (int)ModuleType.Core:
                        count += _fuses[(int)ModuleType.Core, j] ? 1 : 0;
                        break;
                    default:
                        bool fuse = _fuses[i, j];
                        count += (fuse && _fuses[(int)ModuleType.Core, j]) ? 1 : 0;
                        break;
                }
            }
            decal.color = possibleColors[count];
        }
    }
    public static void SetFuseModuleDecals(Texture2D[] moduleTextures)
    {
        for (int i = 0; i < 5; i++)
        {
            UI.ModuleIcons[i].SetTexture(moduleTextures[i]);
        }
    }
    public static void GetSave()
    {
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), $"Content\\Saves\\Save_{Engine.SaveSlot}.txt");
        if (File.Exists(filePath))
        {
            using var outputFile = new StreamReader(filePath);
            string text = outputFile.ReadLine();
            if (text != null)
            {
                UI.LoadedName.text = SaveGame.Disassemble(text)[0];
            }
            return;
        }
        UI.LoadedName.text = "Empty";
    }
    public static void DeleteSave()
    {
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), $"Content\\Saves\\Save_{Engine.SaveSlot}.txt");
        File.Delete(filePath);
    }
    public static void UpgradeSensors(Module _module)
    {
        //Only need one specialized parts
        if (Engine.SaveGame.Player.modules[ModuleType.Sensors] is not Sensors)
        {
            if (Engine.SaveGame.Scrap > 1)
            {
                Engine.SaveGame.Scrap--;
                Engine.SaveGame.Player.modules[ModuleType.Sensors] = _module;
            }
            else
            {
                return;
            }
        }
        Construct firstScrap = null;
        ItemSlot<Pickup> slot = null;
        foreach (var item in UI.MissionSelectSlots)
        {
            if (item.daughterItem is Construct && (item.daughterItem as Construct).Type == Constructs.SpecializedParts)
            {
                firstScrap = (Construct)item.daughterItem;
                slot = item;
                break;
            }
        }
        if (firstScrap != null)
        {
            foreach (var item in UI.InventorySlots)
            {
                if (item.daughterItem is Construct && (item.daughterItem as Construct).Type == Constructs.SpecializedParts)
                {
                    firstScrap = (Construct)item.daughterItem;
                    slot = item;
                    break;
                }
            }
        }
        if (firstScrap != null)
        {
            slot.daughterItem = null;
            firstScrap.isExpired = true;
            Engine.SaveGame.Player.modules[ModuleType.Sensors] = _module;
        }
    }
    public static void UpgradeModule(ModuleType _slot, Module _moduleType)
    {
        string text;
        if (Engine.SaveGame.Scrap < 5)
        {
            UI.UpgradeText.text = "Smelt 5 scrap to upgrade.";
            return;
        }
        var upgrades = new Dictionary<Modules, Module>
            {
                { Modules.Flamethrower, new PrismArray() },
                { Modules.Fireball, new MatrixLauncher() },
                { Modules.Sniper, new Antimaterial() },
                { Modules.LMG, new Torch() },
            };
        if (!upgrades.TryGetValue(_moduleType.Type, out Module value))
        {
            UI.UpgradeText.text = "Selected module cannot be upgraded.";
            return;
        }
        text = $"{_moduleType.Name} has been upgraded to {value.Name}.";
        UI.UpgradeText.text = text;
        Engine.SaveGame.Player.modules[_slot] = value;
        Engine.SaveGame.Scrap -= 5;
    }
    public static void SetModules()
    {
        for(int i = 0; i < 5; i++)
        {
            UI.Module[i].text = ItemFactory.moduleData[UI.setModules[i]].Name;
        }
    }
}
