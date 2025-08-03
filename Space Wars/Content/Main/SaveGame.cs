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
    public int System { get; set; } = 0;
    public int CurrentMissionIndex { get; set; } = 0;
    private bool giveWeapon = true;
    public bool[] CompletedMissions { get; } = new bool[Engine.EntityManager.MissionLength];
    public Player Player { get; } = new Player(Vector2.Zero, Vector2.Zero, 0, 0);
    public Pickup[] Inventory { get; } = new Pickup[4];
    public Pickup[] MissionSelectInventory { get; } = new Pickup[4];
    public List<Queueable> QueuedItems { get; private set; } = [];

    private Mission currentMission;
    public Mission CurrentMission 
    { 
        get 
        { 
            currentMission ??= Engine.EntityManager.GetMission(CurrentMissionIndex); 
            return currentMission; 
        } 
        set 
        { 
            currentMission = value; 
        } 
    }
    public bool CurrentMissionCompleted => CompletedMissions[CurrentMissionIndex];
    public bool GiveWeapon { get { giveWeapon = !giveWeapon; return !giveWeapon; } }
    public SaveGame() { }
    public SaveGame(string _serialization)
    {
        List<string> disassembly = Disassemble(_serialization);
        var logger = new LoadLogger();
        int i = 0;

        logger.Try(delegate { Name = (disassembly[i]).ToString(); }, i++);
        logger.Try(delegate { Scrap = Int32.Parse(disassembly[i]); }, i++);
        logger.Try(delegate { System = Int32.Parse(disassembly[i]); }, i++);
        logger.Try(delegate { CurrentMissionIndex = Int32.Parse(disassembly[i]); }, i++);
        logger.Try(delegate { giveWeapon = bool.Parse(disassembly[i]); }, i++);
        List<string> array = [];
        logger.Try(delegate { array = Disassemble(disassembly[i]); }, i++);
        for (int j = 0; j < CompletedMissions.Length; j++)
        {
            logger.Try(delegate { CompletedMissions[j] = bool.Parse(array[j]); }, j);
        }
        Player player = null;
        logger.Try(delegate{ player = new Player(disassembly[i], logger); }, i++);
        Player = (player ?? new Player(Vector2.Zero, Vector2.Zero, 0, 0));
        array = [];
        logger.Try(delegate { array = Disassemble(disassembly[i]); }, i++);
        for (int j = 0; j < Inventory.Length; j++)
        {
            logger.Try(delegate { Inventory[j] = ItemFactory.TryDeserialize(array[j], logger); }, 6);
        }
        array = [];
        logger.Try(delegate { array = Disassemble(disassembly[i]); }, i++);
        for (int j = 0; j < MissionSelectInventory.Length; j++)
        {
            logger.Try(delegate { MissionSelectInventory[j] = ItemFactory.TryDeserialize(array[j], logger); }, j);
        }
        array = [];
        logger.Try(delegate { array = Disassemble(disassembly[i]); }, i++);
        if(Int32.TryParse(array[0], out int _i))
        {
            for (int j = 0; j < _i; j++)
            {
                logger.Try(delegate { QueuedItems.Add(Queueable.Deserialize(array[j + 1], logger)); }, j+1);
            }
        }
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
        CurrentMissionIndex = Math.Clamp(_count, 0, Engine.EntityManager.Missions() - 1);
        currentMission = Engine.EntityManager.GetMission(CurrentMissionIndex);
        EventHandler.UpdateMissionText();
    }
    public void NextMission()
    {
        CurrentMissionIndex = Math.Clamp(CurrentMissionIndex + 1, 0, Engine.EntityManager.Missions() - 1);
        currentMission = Engine.EntityManager.GetMission(CurrentMissionIndex);
        EventHandler.UpdateMissionText();
    }
    public void PrevMission()
    {
        CurrentMissionIndex = Math.Clamp(CurrentMissionIndex - 1, 0, Engine.EntityManager.Missions() - 1);
        currentMission = Engine.EntityManager.GetMission(CurrentMissionIndex);
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
        QueuedItems = (from item in QueuedItems where !item.IsExpired select item).ToList();
        Engine.Save();
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
        return $"{Name},{Scrap},{System},{CurrentMissionIndex},{giveWeapon},{{{string.Join(",", CompletedMissions)}}},{Player.Serialize()},{{{inv}}},{{{globalInv}}},{{{queueables}}}";
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
