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
public abstract class Queueable(int _cost, Texture2D _texture, string _name)
{
    public Queueable Deserialize(string _data, LoadLogger _logger)
    {
        List<string> disassembly = SaveGame.Disassemble(_data);
        if (disassembly[0] == "")
        {
            return null;
        }
        if (disassembly[0] == "Fuse")
        {

        }
        if (disassembly[0] == "Repair")
        {

        }
        if (disassembly[0] == "Smelt")
        {

        }
        throw new IOException();
    }
    public int Cost { get; private set; } = _cost;
    public string Name { get; } = _name;

    public bool IsExpired => !CanConstruct() || Cost == 0;
    public int MaxCost { get; } = _cost;
    public Texture2D Texture => _texture;
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
    public abstract string Serialize();
}
public class FuseQueue() : Queueable(1, Assets.Get(Sprite.Fuse), "Fuse")
{
    protected override void Construct()
    {
        Engine.SaveGame.Player.AddFuse();
    }
    protected override bool CanConstruct()
    {
        return true;
    }
    public override string Serialize()
    {
        return $"{{{Name},{Cost}}}";
    }
}
public class RepairQueue(ItemSlot<Pickup> _module) : Queueable(2, Assets.Get(Sprite.HullModule), "Repair")
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
    public override string Serialize()
    {
        for(int i = 0; i < Engine.MissionSelectSlots.Length; i++)
        {
            if (Engine.MissionSelectSlots[i] == _module)
            {
                return $"{{{Name},{Cost},{i}}}";
            }
        }
        return $"{{{Name},{Cost},{0}}}";
    }
}
public class SmeltQueue(ItemSlot<Pickup> _pickup) : Queueable(2, Assets.Get(Sprite.RealMetalScrap), "Smelt")
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
    public override string Serialize()
    {
        for (int i = 0; i < Engine.MissionSelectSlots.Length; i++)
        {
            if (Engine.MissionSelectSlots[i] == _pickup)
            {
                return $"{{{Name},{Cost},{i}}}";
            }
        }
        return $"{{{Name},{Cost},{0}}}";
    }
}
