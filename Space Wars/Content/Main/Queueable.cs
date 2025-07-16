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
    public int Cost { get; private set; }
    public string Name { get; }

    public bool IsExpired => !CanConstruct() || Cost == 0;
    public int MaxCost { get; }
    public Texture2D Texture { get; }
    public Queueable(int _cost, Texture2D _texture, string _name)
    {
        Cost = _cost;
        Name = _name;
        MaxCost = _cost;
        Texture = _texture;
    }
    public Queueable(List<string> _data, LoadLogger _logger, int _maxCost, Texture2D _texture, string _name)
    {
        _logger.Try(delegate { Cost = Int32.Parse(_data[1]); }, 0);
        Name = _name;
        MaxCost = _maxCost;
        Texture = _texture;
    }
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
    public static Queueable Deserialize(string _data, LoadLogger _logger)
    {
        List<string> disassembly = SaveGame.Disassemble(_data);
        if (disassembly[0] == "")
        {
            return null;
        }
        if (disassembly[0] == "Fuse")
        {
            return new FuseQueue(disassembly, _logger);
        }
        if (disassembly[0] == "Repair")
        {
            return new RepairQueue(disassembly, _logger);
        }
        if (disassembly[0] == "Smelt")
        {
            return new SmeltQueue(disassembly, _logger);
        }
        throw new IOException();
    }
    protected abstract void Construct();
    protected abstract bool CanConstruct();
    public abstract string Serialize();
}
public class FuseQueue : Queueable
{
    public FuseQueue() : base(1, Assets.Get(Sprite.Fuse), "Fuse") { }
    public FuseQueue(List<string> _disassembly, LoadLogger _logger) : base(_disassembly, _logger, 1, Assets.Get(Sprite.Fuse), "Fuse") { }
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
public class RepairQueue : Queueable
{
    ItemSlot<Pickup> module;
    public RepairQueue(ItemSlot<Pickup> _module) : base(2, Assets.Get(Sprite.HullModule), "Repair")
    {
        module = _module;
    }
    public RepairQueue(List<string> _data, LoadLogger _logger) : base(_data, _logger, 2, Assets.Get(Sprite.HullModule), "Repair")
    {
        _logger.Try(delegate { module = Engine.MissionSelectSlots[Int32.Parse(_data[2])]; }, 2);
    }
    protected override void Construct()
    {
        var storedModule = module.daughterItem as Module;
        storedModule.Health = storedModule.MaxHealth;
    }
    protected override bool CanConstruct()
    {
        return (module.daughterItem as Module) != null;
    }
    public override string Serialize()
    {
        for(int i = 0; i < Engine.MissionSelectSlots.Length; i++)
        {
            if (Engine.MissionSelectSlots[i] == module)
            {
                return $"{{{Name},{Cost},{i}}}";
            }
        }
        //Prevents invalid queueables from being saved
        return "{}";
    }
}
public class SmeltQueue : Queueable
{
    private ItemSlot<Pickup> pickup;
    public SmeltQueue(ItemSlot<Pickup> _pickup) : base(2, Assets.Get(Sprite.RealMetalScrap), "Smelt")
    {
        pickup = _pickup;
    }
    public SmeltQueue(List<String> _data, LoadLogger _logger) : base(_data, _logger, 2, Assets.Get(Sprite.RealMetalScrap), "Smelt")
    {
        _logger.Try(delegate { pickup = Engine.MissionSelectSlots[Int32.Parse(_data[2])]; }, 2);
    }
    protected override void Construct()
    {
        if (pickup.daughterItem is Module)
        {
            Engine.SaveGame.Scrap += 3;
        }
        else
        {
            Engine.SaveGame.Scrap += 2;
        }
        pickup.daughterItem.isExpired = true;
        pickup.daughterItem = null;
    }
    protected override bool CanConstruct()
    {
        return pickup.daughterItem != null;
    }
    public override string Serialize()
    {
        for (int i = 0; i < Engine.MissionSelectSlots.Length; i++)
        {
            if (Engine.MissionSelectSlots[i] == pickup)
            {
                return $"{{{Name},{Cost},{i}}}";
            }
        }
        return "{}";
    }
}
