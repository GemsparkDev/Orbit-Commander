using Microsoft.Xna.Framework;
using Space_Wars.Content.Main.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
public class DropSpawner(Vector2 _spawn) : IPlayerSpawner
{
    public void Spawn()
    {
        Engine.SaveGame.Player.Position = _spawn;
        Engine.EntityManager.Add(Enemy.NewDropPod(_spawn, 0));
        Engine.SaveGame.Player.Dock();
    }
}
public class GliderSpawner(Vector2 _spawn, float _distance) : IPlayerSpawner
{
    public void Spawn()
    {
        Engine.SaveGame.Player.Position = _spawn;
        Engine.EntityManager.Add(Enemy.NewGlider(_spawn, _distance));
        Engine.SaveGame.Player.Dock();
    }
}
