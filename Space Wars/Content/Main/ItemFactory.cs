using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.IO;

namespace Space_Wars.Content.Main;
public static class ItemFactory
{
    private static Player Player => Engine.SaveGame.Player;
    //Items
    public readonly static Dictionary<Items, ItemData> itemData = new()
    {
        { Items.Scrap, (new ItemData(Sprite.RealMetalScrap,Sprite.MetalScrap, "Metal Salvage", 1, Color.White)) },
    };
    public readonly static Dictionary<Modules, ModuleData> moduleData = new()
    {
        { Modules.Hull, new ModuleData(Sprite.HullModule, Sprite.HullModule, "Hull", (int)ModuleType.Hull, 20, delegate{ Player.Hull(); }) },
        { Modules.Shield, new ModuleData(Sprite.HullModule, Sprite.HullModule, "Shield", (int)ModuleType.Hull, 16, delegate{ Player.Shield(); }) },
        { Modules.Stealth, new ModuleData(Sprite.HullModule, Sprite.HullModule, "Stealth", (int)ModuleType.Hull, 18, delegate{ Player.Stealth(); }) },
        { Modules.Reflective, new ModuleData(Sprite.HullModule, Sprite.HullModule, "Reflective", (int)ModuleType.Hull, 18, delegate{ Player.Reflective(); }) },

        { Modules.Basic, new ModuleData(Sprite.RealGunModule, Sprite.GunModule, "Basic", (int)ModuleType.Guns, 20, delegate{ Player.Basic(); }) },
        { Modules.Spiral, new ModuleData(Sprite.RealSpiralModule,Sprite.SpiralModule, "Spiral", (int)ModuleType.Guns, 20, delegate{ Player.Spiral(); }) },
        { Modules.Shotgun, new ModuleData(Sprite.RealGunModule,Sprite.GunModule, "Shotgun", (int)ModuleType.Guns, 20, delegate{ Player.Shotgun(); }) },
        { Modules.Missile, new ModuleData(Sprite.RealMissileModule,Sprite.MissileModule, "Missile", (int)ModuleType.Guns, 18, delegate{ Player.Missile(); }) },
        { Modules.LMG, new ModuleData(Sprite.RealGunModule,Sprite.GunModule, "Light Machine Gun", (int)ModuleType.Guns, 20, delegate{ Player.LMG(); }) },
        { Modules.Sniper, new ModuleData(Sprite.RealSniperModule,Sprite.SniperModule, "Railgun", (int)ModuleType.Guns, 20, delegate{ Player.Sniper(); })},
        { Modules.Crossbow, new ModuleData(Sprite.RealCrossbowModule,Sprite.CrossbowModule, "Crossbow", (int)ModuleType.Guns, 20, delegate{ Player.Silenced(); })},
        { Modules.Flamethrower, new ModuleData(Sprite.RealFlamethrowerModule,Sprite.FlamethrowerModule, "Flamethrower", (int)ModuleType.Guns, 18, delegate{ Player.Flamethrower(); })},
        { Modules.Fireball, new ModuleData(Sprite.RealCrossbowModule,Sprite.CrossbowModule, "Fireball", (int)ModuleType.Guns, 18, delegate{ Player.Fireball(); })},
        { Modules.GrenadeLauncher, new ModuleData(Sprite.RealCrossbowModule,Sprite.CrossbowModule, "Grenade Launcher", (int)ModuleType.Guns, 20, delegate{ Player.GrenadeLauncher(); })},
        { Modules.Spewer, new ModuleData(Sprite.RealCrossbowModule,Sprite.CrossbowModule, "Spewer", (int)ModuleType.Guns, 15, delegate{ Player.Spewer(); })},
        { Modules.Antimaterial, new ModuleData(Sprite.RealCrossbowModule,Sprite.CrossbowModule, "Antimaterial Rifle", (int)ModuleType.Guns, 15, delegate{ Player.Antimaterial(); })},
        { Modules.Triangle, new ModuleData(Sprite.RealCrossbowModule,Sprite.CrossbowModule, "Triangle", (int)ModuleType.Guns, 20, delegate{ Player.Triangle(); })},
        { Modules.PrismArray, new ModuleData(Sprite.RealCrossbowModule,Sprite.CrossbowModule, "Prism Array", (int)ModuleType.Guns, 15, delegate{ Player.PrismArray(); })},
        { Modules.MatrixLauncher, new ModuleData(Sprite.RealCrossbowModule,Sprite.CrossbowModule, "Matrix Launcher", (int)ModuleType.Guns, 15, delegate{ Player.MatrixLauncher(); })},

        { Modules.Engines, new ModuleData(Sprite.EngineModule, Sprite.EngineModule, "Engines", (int)ModuleType.Engines, 20, delegate(){ Player.StandardEngine(); }) },
        { Modules.Plasma, new ModuleData(Sprite.EngineModule, Sprite.EngineModule, "Plasma", (int)ModuleType.Engines, 15, delegate(){ Player.PlasmaEngine();  }) },
        { Modules.Work, new ModuleData(Sprite.EngineModule, Sprite.EngineModule, "Work", (int)ModuleType.Engines, 25, delegate(){ Player.WorkEngine();  }) },

        { Modules.Sensors, new ModuleData(Sprite.SensorModule,Sprite.SensorModule, "Sensors", (int)ModuleType.Sensors, 20, delegate{ }) },
        { Modules.Lidar, new ModuleData(Sprite.SensorModule,Sprite.SensorModule, "Lidar", (int)ModuleType.Sensors, 20, delegate{ Player.Lidar(); }) },
        { Modules.Radar, new ModuleData(Sprite.SensorModule,Sprite.SensorModule, "Radar", (int)ModuleType.Sensors, 20, delegate{ Player.Radar(); }) },
        { Modules.PulseEmitter, new ModuleData(Sprite.SensorModule,Sprite.SensorModule, "Pulse Emitter", (int)ModuleType.Sensors, 20, delegate{ Player.PulseEmitter(); }) },

        { Modules.Dash, new ModuleData(Sprite.CoreModule,Sprite.CoreModule, "Dash Core", (int)ModuleType.Core, 20, delegate{ Player.Dash(); }) },
        { Modules.GrapplingHook, new ModuleData(Sprite.CoreModule, Sprite.CoreModule, "Grapple Core", (int)ModuleType.Core, 20, delegate{ Player.SummonGrapplingHook(); }) },
        { Modules.SummonShield, new ModuleData(Sprite.CoreModule, Sprite.CoreModule, "Shield Core", (int)ModuleType.Core, 20, delegate{ Player.SummonShield(); }) },
        { Modules.Nanomachines, new ModuleData(Sprite.CoreModule, Sprite.CoreModule, "Nanomachines", (int)ModuleType.Core, 20, delegate{ Player.Nanomachines(); }) },
        { Modules.CreateFighter, new ModuleData(Sprite.CoreModule, Sprite.CoreModule, "Construct fighter", (int)ModuleType.Core, 20, delegate{ Player.CreateFighter(); }) },

    };
    public readonly static Dictionary<Constructs, ConstructData> constructData = new() 
    {
        { Constructs.Barricade, new ConstructData(Sprite.RealBarricade, Sprite.Barricade, "Barricade", 1, 20) },
        { Constructs.Trap, new ConstructData(Sprite.RealTrap, Sprite.Trap, "Trap", 1, 8) },
        { Constructs.Bomb, new ConstructData(Sprite.RealBomb, Sprite.Bomb, "Bomb", 1, 3) }
    };
    public static Pickup NewScrap(Vector2 _position = default, Vector2 _velocity = default, float _angularVelocity = 0)
    {
        return new Pickup(itemData[0], _position, _velocity, _angularVelocity);
    }
    public static Pickup TryDeserialize(string _data, LoadLogger _logger)
    {
        List<string> disassembly = SaveGame.Disassemble(_data);
        if (disassembly[0] == "")
        {
            return null;
        }
        if (Enum.TryParse<Items>(disassembly[0], true, out Items result1))
        {
            return new Pickup(itemData[result1], disassembly, _logger);
        }
        if (Enum.TryParse<Modules>(disassembly[0], true, out Modules result2))
        {
            return new Module(result2, disassembly, _logger);
        }
        if (Enum.TryParse<Constructs>(disassembly[0], true, out Constructs result3))
        {
            return new Construct(result3, disassembly, _logger);
        }
        throw new IOException("The module could not be parsed properly.");
    }
}
