using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;
using UILib.Content.Main;

namespace Space_Wars.Content.Main;
public abstract class Queueable
{
    public Queueable(int _cost, Texture2D _texture)
    {
        Cost = _cost;
        MaxCost = _cost;
        Texture = _texture;
    }
    public bool IsExpired => !CanConstruct() || Cost == 0;
    public int Cost { get; private set; }
    public int MaxCost { get; }
    public Texture2D Texture { get; }
    public int AttemptConstruct(int _time)
    {
        if (CanConstruct())
        {
            if (_time >= Cost)
            {
                Construct();
                Cost = 0;
                return _time - Cost;
            }
            Cost -= _time;
            return 0;
        }
        return _time;
    }
    protected abstract void Construct();
    protected abstract bool CanConstruct();
}
public class FuseQueue() : Queueable(1, Assets.Get(Sprite.Fuse))
{
    protected override void Construct()
    {
        Engine.SaveGame.Player.AddFuse();
    }
    protected override bool CanConstruct()
    {
        return true;
    }
}
public class RepairQueue(ItemSlot<Pickup> _module) : Queueable(2, Assets.Get(Sprite.HullModule))
{
    protected override void Construct()
    {
        var module = _module.daughterItem as Module;
        module.Health = module.MaxHealth;
    }
    protected override bool CanConstruct()
    {
        return (_module.daughterItem as Module) != null;
    }
}
public class SmeltQueue(ItemSlot<Pickup> _pickup) : Queueable(2, Assets.Get(Sprite.RealMetalScrap))
{
    protected override void Construct()
    {
        if (_pickup.daughterItem is Module)
        {
            Engine.SaveGame.Scrap += 3;
        }
        else
        {
            Engine.SaveGame.Scrap += 2;
        }
        _pickup.daughterItem = null;
    }
    protected override bool CanConstruct()
    {
        return _pickup.daughterItem != null;
    }
}
