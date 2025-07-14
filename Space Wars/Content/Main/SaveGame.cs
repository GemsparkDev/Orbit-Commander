using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using UILib.Content.Main;

namespace Space_Wars.Content.Main;
public class SaveGame
{
    public int Scrap { get; set; }
    public int System { get; set; } = 0;
    public int CurrentMissionIndex { get; set; } = 0;
    private bool giveWeapon = true;
    public bool[] CompletedMissions { get; } = new bool[Engine.EntityManager.MissionLength];
    public Player Player { get; } = new Player(Vector2.Zero, Vector2.Zero, 0, 0);
    private Mission currentMission;
    public static ItemSlot<Pickup>[,] InventorySlots { get; set; }
    public static ItemSlot<Pickup>[] MissionSelectItems { get; set; } = new ItemSlot<Pickup>[5];
    public List<Queueable> QueuedItems { get; private set; } = [];

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

        logger.Try(delegate { Scrap = Int32.Parse(disassembly[0]); },0);
        logger.Try(delegate { System = Int32.Parse(disassembly[1]); }, 1);
        logger.Try(delegate { CurrentMissionIndex = Int32.Parse(disassembly[2]); }, 2);
        logger.Try(delegate { giveWeapon = bool.Parse(disassembly[3]); }, 3);
        List<string> array = Disassemble(disassembly[4]);
        for (int j = 0; j < CompletedMissions.Length; j++)
        {
            logger.Try(delegate { CompletedMissions[j] = bool.Parse(array[j]);}, j);
        }
        Player = new Player(disassembly[5], logger);
        //logger.Log();
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
    }
    public string Serialize()
    {
        return $"{Scrap},{System},{CurrentMissionIndex},{giveWeapon},{{{string.Join(",", CompletedMissions)}}},{Player.Serialize()}";
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
        Debug.WriteLine("The following errors occurred during loading:");
        foreach (var (index, error) in exceptions)
        {
            Debug.WriteLine($"{index}, {error}");
        }
        exceptions.Clear();
    }
}
