using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text;

namespace Space_Wars.Content.Main;
public class SaveGame
{
    public string Name { get; set; } = "0";
    public int Scrap { get; set; }
    private int system = 0;
    public int System 
    { 
        get 
        { 
            return system; 
        } 
        set 
        { 
            system = value; 
            FleetSystem = Math.Max(FleetSystem, system); 
        } 
    }
    public int CurrentMissionIndex { get; set; } = 0;
    private bool giveWeapon = true;
    public bool[] CompletedMissions { get; } = new bool[EntityManager.missions.Count];
    public Player Player { get; } = new Player(Vector2.Zero, Vector2.Zero, 0);
    public Pickup[] Inventory { get; } = new Pickup[4];
    public Pickup[] MissionSelectInventory { get; } = new Pickup[4];
    public List<Queueable> QueuedItems { get; private set; } = [];
    public int FleetSystem { get; private set; } = 0;

    private Mission currentMission;
    public Mission CurrentMission 
    { 
        get 
        { 
            currentMission ??= EntityManager.missions[CurrentMissionIndex].instance(); 
            return currentMission; 
        } 
        set 
        { 
            currentMission = value; 
        } 
    }
    public bool CurrentMissionCompleted => CompletedMissions[CurrentMissionIndex];
    public bool GiveWeapon { get { giveWeapon = !giveWeapon; return !giveWeapon; } }
    public static float EnemyHitboxModifier { get; set; } = 1.2f;
    public static bool DebugMode { get; set; } = false;
    public static bool PatchedConics { get; set; } = true;
    public static bool UseShader { get; set; } = true;
    public static ColorScheme ColorScheme { get; set; } = new StandardScheme();
    public SaveGame() { }
    public SaveGame(string _serialization)
    {
        ArgumentNullException.ThrowIfNull(_serialization);
        if (_serialization == "")
        {
            return;
        }
        List<string> disassembly = Disassemble(_serialization);
        var logger = new LoadLogger();

        Name = disassembly[0];
        Scrap = Int32.TryParse(disassembly[1], out int scrap) ? scrap : 0;
        System = System = Int32.TryParse(disassembly[2], out int system) ? system : 0;
        CurrentMissionIndex = Int32.TryParse(disassembly[3], out int index) ? index : 0;
        giveWeapon = !Boolean.TryParse(disassembly[4], out bool give) || give;

        List<string> array = [];
        logger.Try(delegate { array = Disassemble(disassembly[5]); }, 5);
        for (int j = 0; j < CompletedMissions.Length; j++)
        {
            logger.Try(delegate { CompletedMissions[j] = bool.Parse(array[j]); }, j);
        }

        Player player = null;
        logger.Try(delegate{ player = new Player(disassembly[6], logger); }, 6);
        Player = (player ?? new Player(Vector2.Zero, Vector2.Zero, 0));

        array = [];
        logger.Try(delegate { array = Disassemble(disassembly[7]); }, 7);
        for (int j = 0; j < Inventory.Length; j++)
        {
            logger.Try(delegate { Inventory[j] = ItemFactory.TryDeserialize(array[j], logger); }, 6);
        }

        array = [];
        logger.Try(delegate { array = Disassemble(disassembly[8]); }, 8);
        for (int j = 0; j < MissionSelectInventory.Length; j++)
        {
            logger.Try(delegate { MissionSelectInventory[j] = ItemFactory.TryDeserialize(array[j], logger); }, j);
        }

        array = [];
        logger.Try(delegate { array = Disassemble(disassembly[9]); }, 9);
        if(Int32.TryParse(array[0], out int _i))
        {
            for (int j = 0; j < _i; j++)
            {
                logger.Try(delegate { QueuedItems.Add(Queueable.Deserialize(array[j + 1], logger)); }, j+1);
            }
        }
        FleetSystem = Int32.TryParse(disassembly[10], out int fleet) ? fleet : 0;

        logger.Log();
    }
    public static List<string> Disassemble(string _serialization)
    {
        var disassembly = new List<string>() { "" };
        for (int i = 0; i < _serialization.Length; i++)
        {
            char c = _serialization[i];
            if (c == '{')
            {
                int start = i + 1;
                int end = i + 1;
                int depth = 1;
                for (int j = start; j < _serialization.Length; j++)
                {
                    char endChar = _serialization[j];
                    if (endChar == '{')
                    {
                        depth++;
                    }
                    if (endChar == '}')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            end = j;
                            break;
                        }
                    }
                }
                disassembly[^1] = _serialization[start..end];
                i = end;
            }
            else if (c == ',')
            {
                disassembly.Add("");
            }
            else
            {
                disassembly[^1] += c;
            }
        }
        return disassembly;
    }
    public void SetMission(int _count)
    {
        CurrentMissionIndex = Math.Clamp(_count, 0, EntityManager.missions.Count - 1);
        currentMission = EntityManager.missions[CurrentMissionIndex].instance();
        EventHandler.UpdateMissionText();
    }
    public void NextMission()
    {
        CurrentMissionIndex = Math.Clamp(CurrentMissionIndex + 1, 0, EntityManager.missions.Count - 1);
        EventHandler.UpdateMissionText();
    }
    public void PrevMission()
    {
        CurrentMissionIndex = Math.Clamp(CurrentMissionIndex - 1, 0, EntityManager.missions.Count - 1);
        EventHandler.UpdateMissionText();
    }
    public void CompleteMission(int _duration)
    {
        CompletedMissions[CurrentMissionIndex] = true;
        int points = _duration / 10;
        foreach (var item in Engine.SaveGame.QueuedItems)
        {
            points = item.AttemptConstruct(points);
            if (points <= 0)
            {
                break;
            }
        }
        QueuedItems = [.. from item in QueuedItems where !item.IsExpired select item];
        Engine.SaveGame.Player.StatusHolder.Clear();
        Engine.Autosave();
    }
    public string Serialize()
    {
        var inv = new StringBuilder();
        var globalInv = new StringBuilder();
        var queueables = new StringBuilder();
        queueables.Append($"{QueuedItems.Count},");
        foreach (var item in Inventory)
        {
            if (item != null)
            {
                inv.Append($"{item.Serialize()},");
            }
            else
            {
                inv.Append($"{{}},");
            }
        }
        Debug.WriteLine(MissionSelectInventory[0] == null);
        foreach (var item in MissionSelectInventory)
        {
            if (item != null)
            {
                globalInv.Append($"{item.Serialize()},");
            }
            else
            {
                globalInv.Append($"{{}},");
            }
        }
        foreach (var item in QueuedItems)
        {
            queueables.Append($"{item.Serialize()},");
        }
        inv.Remove(inv.Length - 1, 1);
        globalInv.Remove(globalInv.Length - 1, 1);
        queueables.Remove(queueables.Length - 1, 1);
        return $"{Name},{Scrap},{System},{CurrentMissionIndex},{giveWeapon},{{{string.Join(",", CompletedMissions)}}},{Player.Serialize()},{{{inv}}},{{{globalInv}}},{{{queueables}}},{FleetSystem}";
    }
}
public class LoadLogger
{
    private List<(int index, Exception error)> exceptions = [];
    public void Try(Action _assignment, int _index)
    {
        try 
        { 
            _assignment(); 
        } 
        catch (Exception error) 
        {
            exceptions.Add((_index, error));
        }
    }
    public void Log()
    {
        if (exceptions.Count == 0)
        {
            return;
        }
        Debug.WriteLine("The following errors occurred during loading:");
        foreach (var (index, error) in exceptions)
        {
            Debug.WriteLine($"{index}, {error}");
        }
        exceptions.Clear();
    }
}
