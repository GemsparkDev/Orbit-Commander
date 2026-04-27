using Microsoft.Xna.Framework;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main.MissionComponents;
public interface IPlayerSpawner
{
    public void Spawn();
}
public class PlayerSpawner(Vector2 _spawn) : IPlayerSpawner
{
    public void Spawn()
    {
        Engine.SaveGame.Player.Position = _spawn;
        Engine.SaveGame.Player.Velocity = Vector2.Zero;
    }
}
public class DropSpawner(float _distance) : IPlayerSpawner
{
    public void Spawn()
    {
        Engine.SaveGame.Player.Position = new Vector2(0, -_distance);
        Engine.SaveGame.CurrentMission.Add(Entity.NewDropPod(new Vector2(0, -_distance), 0));
        Engine.SaveGame.Player.Dock();
    }
}
public class GliderSpawner(Vector2 _spawn, float _distance) : IPlayerSpawner
{
    public void Spawn()
    {
        Engine.SaveGame.Player.Position = _spawn;
        Engine.SaveGame.CurrentMission.Add(Entity.NewGlider(_spawn, _distance));
        Engine.SaveGame.Player.Dock();
    }
}
public class CustomSpawner(Vector2 _spawn) : IPlayerSpawner
{
    public void Spawn()
    {
        Engine.SaveGame.Player.Position = _spawn;
        Engine.SaveGame.Player.Dock();
    }
}
