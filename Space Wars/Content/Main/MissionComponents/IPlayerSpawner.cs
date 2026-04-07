using Microsoft.VisualBasic;
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
public class DropSpawner(float _distance) : IPlayerSpawner
{
    public void Spawn()
    {
        Engine.SaveGame.Player.Position = new Vector2(0, -_distance);
        Engine.EntityManager.Add(Enemy.NewDropPod(new Vector2(0, -_distance), 0));
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
public class CustomSpawner(Vector2 _spawn) : IPlayerSpawner
{
    public void Spawn()
    {
        Engine.SaveGame.Player.Position = _spawn;    
        Engine.SaveGame.Player.Dock();
    }
}
