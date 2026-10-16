using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrbitCommander.Components;
using OrbitCommander.Entities;
using OrbitCommander.Particles;
using OrbitCommander.UIElements;
using UILib.Content;

namespace OrbitCommander.Core;
public static class Events
{
    private static Player Player => Engine.SaveGame.Player;
    private static readonly List<Message> eventLog = [];

    public static bool AcknowledgeMessage(Message _message)
    {
        return eventLog.Remove(_message);
    }
    public static bool SendMessage(Message _message)
    {
        if (eventLog.Contains(_message))
        {
            return false;
        }
        eventLog.Add(_message);
        return true;
    }
    public static void QuitToMenu()
    {
        Player.Velocity = Vector2.Zero;
        Engine.IngameTime = new();
        UI.MainGlobals.IsEnabled = true;
        ParticleManager.Initialize();
        SoundManager.SetAllSounds(false);
        SoundManager.Initialize();
        SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
        Engine.Camera.Position = Vector2.Zero;
        CurrentGameState.SwitchState(new MainMenu());
    }
    public static void RepairModule(Module item)
    {
        if (item.Health >= item.MaxHealth)
        {
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Fail));
            return;
        }
        //TODO: Reimplement construct overheal
        //daughterModule.GetComponent<Health>().SetOverhealth(daughterModule.MaxHealth + (int)Math.Ceiling(daughterModule.Health * 0.5f) + 5);
        SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
        item.Health = item.MaxHealth;
        item.isFailed = false;
        Engine.VirtualUI.selectedIcon = null;
    }
    public static void UpdateInventoryUI()
    {
        for (int i = 0; i < UI.InventorySlots.Length; i++)
        {
            UI.InventorySlots[i].Item = Engine.SaveGame.Inventory[i];
        }
        for (int i = 0; i < UI.MissionSelectSlots.Length; i++)
        {
            UI.MissionSelectSlots[i].Item = Engine.SaveGame.MissionSelectInventory[i];
        }
    }
    public static void UpdateInventory()
    {
        for (int i = 0; i < UI.InventorySlots.Length; i++)
        {
            Engine.SaveGame.Inventory[i] = UI.InventorySlots[i].Item;
        }
        for (int i = 0; i < UI.MissionSelectSlots.Length; i++)
        {
            Engine.SaveGame.MissionSelectInventory[i] = UI.MissionSelectSlots[i].Item;
        }
    }
    public static void UpdateModulesUI()
    {
        for (int x = 0; x < UI.ModuleSlots.Length; x++)
        {
            UI.ModuleSlots[x].Item = Player.modules[(ModuleType)x];
        }
        UI.SecondarySlot.Item = Player.SecondaryWeapon;
    }
    public static bool SyncModules()
    {
        foreach (var module in UI.ModuleSlots)
        {
            if (module.Item == null)
            {
                return false;
            }
        }
        for (int x = 0; x < Player.modules.Count; x++)
        {
            Player.modules[(ModuleType)x] = UI.ModuleSlots[x].Item;
        }
        Player.SecondaryWeapon = UI.SecondarySlot.Item;
        return true;
    }
    public static void UpdateFurnaceUI(float _value, float _maxValue, Pickup furnaceItem, int requiredCraftsLeft)
    {
        UI.FurnaceSlot.Item = furnaceItem;
        UI.FurnaceSlider.SetInterval(_value, _maxValue);
        UI.RequiredCraftsText.Text = requiredCraftsLeft.ToString();
    }
    public static void UpdateEnemyCountdownUI(float _value, float _maxValue, float _wave)
    {
        UI.EnemySlider.Intervals[0] = _value / _maxValue;
        UI.WaveText.Text = $"{_wave}";
    }
    public static void UpdateMissionText()
    {
        var mission = Mission.missions[Engine.SaveGame.CurrentMissionIndex].data;
        bool completed = Engine.SaveGame.CurrentMissionCompleted;
        bool isDangerous = Engine.SaveGame.FleetSystem > Mission.missions[Engine.SaveGame.CurrentMissionIndex].data.System;
        UI.MissionName.Text = mission.Name;
        UI.MissionDescription.Text = mission.Description;
        UI.IsComplete.Text = completed ? "Completed" : "Not Completed";
        UI.IsComplete.textColor = completed ? Color.Green : Color.Red;
        UI.SelectMission.TextColor = completed && mission.IsRelaunchable ? Color.Gray : Color.Yellow;
        UI.AlertText.Text = isDangerous ? "Danger: Fleet Detected" : "";
    }
    public static void UpdateModulesStatus(bool _isEnabled, IEnumerable<bool> _moduleFailed)
    {
        UI.StatusLights.Zip(_moduleFailed, (l, f) => { l.color = (_isEnabled ? Color.Transparent : (f ? Color.Red : Color.White)); return 0; });
    }
    public static void UpdateFuseUI(int[] _power, bool[] _fuses, int _spareFuses, Player _player)
    {
        UI.FuseCounter.Count = _spareFuses;
        Color[] possibleColors = [Color.Red, Color.Yellow, Color.White, new Color(0, 255, 0), Color.Cyan];

        int power = 0;
        for (int i = 0; i < _fuses.Length; i++)
        {
            if (!_fuses[i])
            {
                UI.Fuses[i].Item = null;
            }
            else
            {
                power += 2;
                UI.Fuses[i].Item = new Fuse(Color.White);
            }
        }

        float usedPower = 0;
        for(int i = 0; i < _power.Length; i++)
        {
            usedPower += _power[i];
            UI.ModulePower[i].Target = (float)(_power[i]) / 4f;
        }

        if(usedPower > power)
        {
            UI.FuseDial.Target = 0;
            foreach(var decal in UI.ModuleIcons)
            {
                decal.color = Color.Red;
            }
            if(_player.IsEnabled)
            {
                _player.Disable();
            }
        }
        else
        {
            UI.FuseDial.Target = usedPower / power;
            for (int i = 0; i < _power.Length; i++)
            {
                UI.ModuleIcons[i].color = possibleColors[_power[i]];
            }
        }
    }
    public static void SetFuseModuleDecals(Texture2D[] _moduleTextures)
    {
        for (int i = 0; i < _moduleTextures.Length; i++)
        {
            UI.ModuleIcons[i].Texture = _moduleTextures[i];
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
                UI.LoadedName.Text = SaveGame.Disassemble(text)[0];
            }
            return;
        }
        UI.LoadedName.Text = "Empty";
    }
    public static void DeleteSave()
    {
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), $"Content\\Saves\\Save_{Engine.SaveSlot}.txt");
        File.Delete(filePath);
    }
    public static Pickup[] GetPickups(int i, out ItemSlot<Pickup>[] _slots)
    {
        var pickups = new Pickup[i];
        var slots = new ItemSlot<Pickup>[i];
        int count = 0;
        foreach (var item in UI.MissionSelectSlots.Concat(UI.InventorySlots))
        {
            if (item.Item != null && !item.Item.HasTag(Tags.IsSpecialized) && item.Item is not Module)
            {
                pickups[count] = item.Item;
                slots[count] = item;
                count++;
            }
            if (count == i)
            {
                break;
            }
        }
        if(count < i)
        {
            pickups = null;
        }
        _slots = slots;
        return pickups;
    }
    public static void UpgradeSensors(SensorType _sensorType)
    {
        var pickups = GetPickups(1, out ItemSlot<Pickup>[] slots);
        if (pickups == null)
        {
            return;
        }
        if (Player.sensorType != SensorType.Basic)
        {
            Player.sensorType = _sensorType;
        }
        slots[0].Item = null;
    }
    public static void UpgradeModule(ModuleType _slot, Module _moduleType)
    {
        string text;
        var pickups = GetPickups(1, out ItemSlot<Pickup>[] slots);
        if (pickups == null)
        {
            UI.UpgradeText.Text = "Acquire 3 scrap to upgrade.";
            return;
        }
        var upgrades = new Dictionary<Modules, Modules>
            {
                { Modules.Flamethrower, Modules.PrismArray },
                { Modules.Fireball, Modules.Flamethrower },
                { Modules.Antimaterial, Modules.Railgun },
                { Modules.LMG, Modules.Torch },
                { Modules.Shotgun, Modules.AdaptiveShotgun },
                { Modules.Missile, Modules.MicroRocketLauncher }

            };
        if (!upgrades.TryGetValue(_moduleType.Type, out Modules value))
        {
            UI.UpgradeText.Text = "Selected module cannot be upgraded.";
            return;
        }
        Module mod = ItemFactory.Retrieve(value);
        text = $"{_moduleType.Name} has been upgraded to {mod.Name}.";
        UI.UpgradeText.Text = text;
        Engine.SaveGame.Player.modules[_slot] = mod;
        foreach(var slot in slots)
        {
            slot.Item = null;
        }
    }
    public static void SetModules()
    {
        for (int i = 0; i < UI.setModules.Length; i++)
        {
            UI.ModuleSelection[i].Text = UI.setModules[i].ToString();
        }
    }
}
