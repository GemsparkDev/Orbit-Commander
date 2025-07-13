using Space_Wars.Content.Main.Entities;
using System;
using System.Numerics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Space_Wars.Content.Main;
[Serializable()]
public class SaveGame
{
    public Player Player { get; } = new Player(Vector2.Zero, Vector2.Zero, 0, 0);
    public int Scrap { get; set; }
    public bool[] CompletedMissions { get; } = new bool[Engine.EntityManager.MissionLength];
    public bool CurrentMissionCompleted => CompletedMissions[CurrentMissionIndex];
    public int System { get; set; } = 0;
    public int CurrentMissionIndex { get; set; } = 0;
    private Mission currentMission;
    public Mission CurrentMission { get { currentMission ??= Engine.EntityManager.GetMission(CurrentMissionIndex); return currentMission; } set { currentMission = value; } }
    private bool giveWeapon = true;
    public bool GiveWeapon { get { giveWeapon = !giveWeapon;  return !giveWeapon; } }
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
    public void Serialize()
    {
        throw new NotImplementedException();
    }
}
