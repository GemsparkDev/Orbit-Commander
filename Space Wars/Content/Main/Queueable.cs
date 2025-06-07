using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main;
public abstract class Queueable(int _cost, Texture2D _texture)
{
    public bool IsExpired { get; private set; } = false;
    public Texture2D Texture { get; } = _texture;
    public int AttemptConstruct(int _time)
    {
        if (_time >= _cost)
        {
            Construct();
            IsExpired = true;
            return _time - _cost;
        }
        return _time;
    }
    protected abstract void Construct();
}
public class FuseQueue() : Queueable(1, Assets.Get(Sprite.Fuse))
{
    protected override void Construct()
    {
        Engine.SaveGame.Player.AddFuse();
    }
}
public class RepairQueue : Queueable
{
    private Module module;
    public RepairQueue(Module _module) : base(2, Assets.Get(Sprite.HullModule))
    {
        module = _module;
    }
    protected override void Construct()
    {
        module.Health = module.MaxHealth;
    }
}
public class SmeltQueue : Queueable
{
    private Pickup pickup;
    public SmeltQueue(ref Pickup _pickup) : base(2, Assets.Get(Sprite.RealMetalScrap))
    {
        pickup = _pickup;
    }
    protected override void Construct()
    {
        if (pickup is Module)
        {
            Engine.SaveGame.Scrap += 3;
        }
        else
        {
            Engine.SaveGame.Scrap += 2;
        }
        pickup = null;
    }
}
