using Microsoft.Xna.Framework;
using OrbitCommander.Entities;
using OrbitCommander.Core;

namespace OrbitCommander.MissionComponents;
public interface IPlayerSpawner
{
    public void Spawn(Player _player);
}
public class PlayerSpawner(Vector2 _spawn) : IPlayerSpawner
{
    public void Spawn(Player _player)
    {
        _player.Position = _spawn;
        _player.Velocity = Vector2.Zero;
    }
}
public class DropSpawner(float _distance) : IPlayerSpawner
{
    public void Spawn(Player _player)
    {
        _player.Position = new Vector2(0, -_distance);
        Engine.SaveGame.CurrentMission.Add(Entity.NewDropPod(new Vector2(0, -_distance), 0, _player));
        _player.Dock(false, true);
    }
}
public class GliderSpawner(Vector2 _spawn) : IPlayerSpawner
{
    public void Spawn(Player _player)
    {
        _player.Position = _spawn;
        Engine.SaveGame.CurrentMission.Add(Entity.NewGlider(_spawn, -Mission.missions[Engine.SaveGame.CurrentMissionIndex].data.PickupDistance, _player));
        _player.Dock(false, true);
    }
}
public class CustomSpawner(Vector2 _spawn) : IPlayerSpawner
{
    public void Spawn(Player _player)
    {
        _player.Position = _spawn;
        _player.Dock(false, true);
    }
}
