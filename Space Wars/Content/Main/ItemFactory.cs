using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Space_Wars.Content.Main;
public static class ItemFactory
{
    private static Player Player => Engine.SaveGame.Player;
    //Items
    private static Dictionary<ItemType, ItemData> itemData = new()
    {
        { ItemType.Scrap, (new ItemData(Sprite.RealMetalScrap,Sprite.MetalScrap, "Metal Salvage", 1, Color.White)) },
    };
    private static Dictionary<Modules, ModuleData> moduleData = new()
    {
        { Modules.Hull, new ModuleData(Sprite.HullModule, Sprite.HullModule, "Hull", (int)ModuleType.Hull, 20, delegate{ Player.Hull(); }, Modules.Hull) },

        { Modules.Basic, new ModuleData(Sprite.RealGunModule, Sprite.GunModule, "Basic", (int)ModuleType.Guns, 20, delegate{ Player.Basic(); }, Modules.Basic) },
        { Modules.Spiral, new ModuleData(Sprite.RealSpiralModule,Sprite.SpiralModule, "Spiral", (int)ModuleType.Guns, 20, delegate{ Player.Spiral(); }, Modules.Spiral) },
        { Modules.Shotgun, new ModuleData(Sprite.RealGunModule,Sprite.GunModule, "Shotgun", (int)ModuleType.Guns, 20, delegate{ Player.Shotgun(); }, Modules.Shotgun) },
        { Modules.Missile, new ModuleData(Sprite.RealMissileModule,Sprite.MissileModule, "Missile", (int)ModuleType.Guns, 20, delegate{ Player.Missile(); }, Modules.Missile) },
        { Modules.LMG, new ModuleData(Sprite.RealGunModule,Sprite.GunModule, "Light Machine Gun", (int)ModuleType.Guns, 20, delegate{ Player.LMG(); }, Modules.LMG) },
        { Modules.Sniper, new ModuleData(Sprite.RealSniperModule,Sprite.SniperModule, "Railgun", (int)ModuleType.Guns, 20, delegate{ Player.Sniper(); }, Modules.Sniper)},
        { Modules.Crossbow, new ModuleData(Sprite.RealCrossbowModule,Sprite.CrossbowModule, "Crossbow", (int)ModuleType.Guns, 20, delegate{ Player.Silenced(); }, Modules.Crossbow)},
        { Modules.Flamethrower, new ModuleData(Sprite.RealFlamethrowerModule,Sprite.FlamethrowerModule, "Flamethrower", (int)ModuleType.Guns, 20, delegate{ Player.Flamethrower(); }, Modules.Flamethrower)},
        { Modules.Fireball, new ModuleData(Sprite.RealCrossbowModule,Sprite.CrossbowModule, "Fireball", (int)ModuleType.Guns, 20, delegate{ Player.Fireball(); }, Modules.Fireball)},
        { Modules.GrenadeLauncher, new ModuleData(Sprite.RealCrossbowModule,Sprite.CrossbowModule, "Grenade Launcher", (int)ModuleType.Guns, 20, delegate{ Player.GrenadeLauncher(); }, Modules.GrenadeLauncher)},

        { Modules.Engines, new ModuleData(Sprite.EngineModule, Sprite.EngineModule, "Engines", (int)ModuleType.Engines, 20, delegate(){ }, Modules.Engines) },

        { Modules.Sensors, new ModuleData(Sprite.SensorModule,Sprite.SensorModule, "Sensors", (int)ModuleType.Sensors, 20, delegate{ }, Modules.Sensors) },

        { Modules.Dash, new ModuleData(Sprite.CoreModule,Sprite.CoreModule, "Dash Core", (int)ModuleType.Core, 20, delegate{ Player.Dash(); }, Modules.Dash) },
        { Modules.GrapplingHook, new ModuleData(Sprite.CoreModule, Sprite.CoreModule, "Grapple Core", (int)ModuleType.Core, 20, delegate{ Player.SummonGrapplingHook(); }, Modules.GrapplingHook) },
        { Modules.SummonShield, new ModuleData(Sprite.CoreModule, Sprite.CoreModule, "Shield Core", (int)ModuleType.Core, 20, delegate{ Player.SummonShield(); }, Modules.SummonShield) },
        { Modules.Nanomachines, new ModuleData(Sprite.CoreModule, Sprite.CoreModule, "Nanomachines", (int)ModuleType.Core, 20, delegate{ Player.Nanomachines(); }, Modules.Nanomachines) },

    };
    private static Dictionary<ConstructType, ConstructData> constructData = new() 
    {
        { ConstructType.Barricade, new ConstructData(Sprite.RealBarricade, Sprite.Barricade, "Barricade", 1, 20) },
        { ConstructType.Trap, new ConstructData(Sprite.RealTrap, Sprite.Trap, "Trap", 1, 8) },
        { ConstructType.Bomb, new ConstructData(Sprite.RealBomb, Sprite.Bomb, "Bomb", 1, 3) }
    };
    public static Pickup NewScrap(Vector2 _position = new Vector2(), Vector2 _velocity = new Vector2(), float _angularVelocity = 0)
    {
        return new Pickup(itemData[0], _position, _velocity, _angularVelocity);
    }
    public static Pickup GetItem(ItemType _item, Vector2 _position = new Vector2(), Vector2 _velocity = new Vector2(), float _angularVelocity = 0)
    {
        return new Pickup(itemData[_item], _position, _velocity, _angularVelocity);
    }
    public static Module GetItem(Modules _item, Vector2 _position = new Vector2(), Vector2 _velocity = new Vector2(), float _angularVelocity = 0)
    {
        return new Module(moduleData[_item], _position, _velocity, _angularVelocity);
    }
    public static Construct GetItem(ConstructType _item, Vector2 _position = new Vector2(), Vector2 _velocity = new Vector2(), float _angularVelocity = 0)
    {
        return new Construct(constructData[_item], _position, _velocity, 0, _angularVelocity, _item);
    }
}
public enum ItemType
{
    Scrap
}
