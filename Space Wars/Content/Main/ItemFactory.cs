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
    private static Dictionary<ModuleType, ModuleData> moduleData = new()
    {
        { ModuleType.Hull, new ModuleData(Sprite.HullModule, Sprite.HullModule, "Hull", (int)ModuleType.Hull, 20, delegate{ Player.Hull(); }) },

        { ModuleType.Basic, new ModuleData(Sprite.RealGunModule, Sprite.GunModule, "Basic", (int)ModuleType.Guns, 20, delegate{ Player.Basic(); }) },
        { ModuleType.Spiral, new ModuleData(Sprite.RealGunModule,Sprite.GunModule, "Spiral", (int)ModuleType.Guns, 20, delegate{ Player.Spiral(); }) },
        { ModuleType.Shotgun, new ModuleData(Sprite.RealGunModule,Sprite.GunModule, "Shotgun", (int)ModuleType.Guns, 20, delegate{ Player.Shotgun(); }) },
        { ModuleType.Missile, new ModuleData(Sprite.RealMissileModule,Sprite.MissileModule, "Missile", (int)ModuleType.Guns, 20, delegate{ Player.Missile(); }) },
        { ModuleType.LMG, new ModuleData(Sprite.RealGunModule,Sprite.GunModule, "Light Machine Gun", (int)ModuleType.Guns, 20, delegate{ Player.LMG(); }) },
        { ModuleType.Sniper, new ModuleData(Sprite.RealSniperModule,Sprite.SniperModule, "Railgun", (int)ModuleType.Guns, 20, delegate{ Player.Sniper(); })},

        { ModuleType.Engines, new ModuleData(Sprite.EngineModule, Sprite.EngineModule, "Engines", (int)ModuleType.Engines, 20, delegate{ Player.Dash(); }) },
        { ModuleType.GrapplingHook, new ModuleData(Sprite.EngineModule, Sprite.EngineModule, "Grappling Hook", (int)ModuleType.Engines, 20, delegate{ Player.SummonGrapplingHook(); }) },
        //{ ModuleType.Engines, new ModuleData(Sprite.EngineModule, Sprite.EngineModule, "Shield", (int)ModuleType.Engines, 20, delegate{ EntityManager.Player.SummonShield(); }) },

        { ModuleType.Sensors, new ModuleData(Sprite.SensorModule,Sprite.SensorModule, "Sensors", (int)ModuleType.Sensors, 20, delegate{ }) },

        { ModuleType.Core, new ModuleData(Sprite.CoreModule,Sprite.CoreModule, "Core", (int)ModuleType.Core, 20, delegate{ }) }
    };
    public static Pickup NewScrap(Vector2 _position = new Vector2(), Vector2 _velocity = new Vector2(), float _angularVelocity = 0)
    {
        return new Pickup(itemData[0], Color.Cyan, _position, _velocity, _angularVelocity);
    }
    public static Pickup GetItem(ItemType _item, Vector2 _position = new Vector2(), Vector2 _velocity = new Vector2(), float _angularVelocity = 0)
    {
        return new Pickup(itemData[_item], Color.Cyan, _position, _velocity, _angularVelocity);
    }
    public static Module GetItem(ModuleType _item, Vector2 _position = new Vector2(), Vector2 _velocity = new Vector2(), float _angularVelocity = 0)
    {
        return new Module(moduleData[_item], Color.Cyan, _position, _velocity, _angularVelocity);
    }
}
public enum ItemType
{
    Scrap
}
