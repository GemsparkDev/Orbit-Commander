using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main;
public abstract class Queueable(int _cost)
{
    public bool IsExpired { get; private set; } = false;
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
public class FuseQueue() : Queueable(1)
{
    protected override void Construct()
    {
        Engine.SaveGame.Player.AddFuse();
    }
}
public class RepairQueue(Module _module) : Queueable(2)
{
    protected override void Construct()
    {
        _module.Health = _module.MaxHealth;
    }
}
public class SmeltQueue : Queueable
{
    private Pickup pickup;
    public SmeltQueue(ref Pickup _pickup) : base(2)
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
