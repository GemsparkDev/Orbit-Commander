using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Particles;
using System;
using System.Collections.Generic;
using UILib.Content.Main;
using System.Diagnostics;

namespace Space_Wars.Content.Main.Entities;
public class Pickup : Entity, IData
{
    internal ItemData itemData;
    public Texture2D Texture => itemData.RealSprite;
    public Window Tooltip { get; } = new Window(Vector2.Zero, Assets.Get(Sprite.WideButton));
    public String Name => itemData.Name;
    public virtual Color Color => itemData.Color;
    private int hitsLeft = 3;
    protected float invincibilityCooldown = 5;
    public int ID => itemData.ID;
    public Pickup(ItemData _itemData, Color _worldColor, Vector2 _position, Vector2 _velocity, float _angularVelocity)
        : base(_itemData.VirtualSprite, _position, _velocity, 0, _angularVelocity, 0, true)
    {
        itemData = _itemData;
        color = _worldColor;
        Tooltip.AddWidget(new Decal(new Vector2(-Tooltip.Size.X / 3, 0) - new Vector2(_itemData.RealSprite.Width, _itemData.RealSprite.Height)/2, _itemData.RealSprite));
        Tooltip.AddWidget(new Decal(new Vector2(0, -5), Assets.TextFont, _itemData.Name, Color.White,  5f));
    }

    public override void Update()
    {
        if (!EntityManager.Player.leashedMaterials.Contains(this))
        {
            if (EntityManager.DistanceSqr(EntityManager.Player, this) < 1375 && EntityManager.Player.leashedMaterials.Count < 3 && EntityManager.Player.canGatherResources)
            {
                EntityManager.Player.leashedMaterials.Add(this);
                if (EntityManager.Player.leashedMaterials.Count < 3)
                {
                    SoundManager.PlaySound(Assets.Get(Sound.Interact), position);
                }
                else
                {
                    SoundManager.PlaySound(Assets.Get(Sound.Full), position);
                }
            }
            velocity /= 2 * Engine.DeltaSeconds + 1;
        }
        else
        {
            Vector2 playerVelocity = EntityManager.Player.velocity;
            Vector2 leashPosition = EntityManager.Player.position - Engine.ToUnitVector(EntityManager.Player.angle) * 25;
            float distance = EntityManager.DistanceSqr(position, leashPosition);
            if (distance > 16)
            {
                velocity += Vector2.Normalize(leashPosition - position) * Engine.DeltaSeconds * distance;
            }
            else
            {
                velocity += (playerVelocity - velocity) / 2;
            }
            ClampVelocity(MathF.Sqrt(playerVelocity.X * playerVelocity.X + playerVelocity.Y * playerVelocity.Y) + 1);
        }
        position += velocity * Engine.DeltaSeconds * 60;
        angle += angularVelocity * Engine.DeltaSeconds * 60;
        var nearestProjectile = Engine.EntityManager.NearestProjectile(this);
        if (nearestProjectile != null)
        {
            if (Vector2.Distance(nearestProjectile.position, this.position) < nearestProjectile.ColliderRadius + ColliderRadius)
            {
                EntityManager.Collide(this, nearestProjectile);
            }
        }
        if (invincibilityCooldown > 0)
        {
            invincibilityCooldown -= Engine.DeltaSeconds;
        }
        base.Update();
    }
    public override void Collide(int _damage)
    {
        if (_damage <= 0)
        {
            return;
        }
        if (invincibilityCooldown > 0)
        {
            invincibilityCooldown = 0;
            return;
        }
        hitsLeft--;
        if (_damage >= 10)
        {
            hitsLeft--;
        }
        //Prevents negative integrity values
        hitsLeft = Math.Max(hitsLeft, 0);
        invincibilityCooldown = 1;
        if (hitsLeft <= 0)
        {
            isExpired = true;
        }
        SoundManager.PlaySound(Assets.Get(Sound.Death), position);
        Engine.ShakeScreen(10 / ((position - Engine.Camera.Position).Length() + 150));
        ParticleManager.Add(new Particle(null, 1, position + new Vector2(0, -1), new Vector2(0, -1.5f), 0, 0, 1, true, Color.Orange, Color.Red) { drawText = $"Integrity: {hitsLeft}" });
    }
}

public static class ItemFactory
{
    //Items
    private static Dictionary<ItemType, ItemData> itemData = new()
    {
        { ItemType.Scrap, (new ItemData(Sprite.RealMetalScrap,Sprite.MetalScrap, "Metal Salvage", 1, Color.White)) },
    };
    private static Dictionary<ModuleType, ModuleData> moduleData = new()
    {
        { ModuleType.Hull, new ModuleData(Sprite.HullModule, Sprite.HullModule, "Hull", (int)ModuleType.Hull, 20, delegate{ EntityManager.Player.Hull(); }) },

        { ModuleType.Basic, new ModuleData(Sprite.RealGunModule, Sprite.GunModule, "Basic", (int)ModuleType.Guns, 20, delegate{ EntityManager.Player.Basic(); }) },
        { ModuleType.Spiral, new ModuleData(Sprite.RealGunModule,Sprite.GunModule, "Spiral", (int)ModuleType.Guns, 20, delegate{ EntityManager.Player.Spiral(); }) },
        { ModuleType.Shotgun, new ModuleData(Sprite.RealGunModule,Sprite.GunModule, "Shotgun", (int)ModuleType.Guns, 20, delegate{ EntityManager.Player.Shotgun(); }) },
        { ModuleType.Missile, new ModuleData(Sprite.RealMissileModule,Sprite.MissileModule, "Missile", (int)ModuleType.Guns, 20, delegate{ EntityManager.Player.Missile(); }) },
        { ModuleType.LMG, new ModuleData(Sprite.RealGunModule,Sprite.GunModule, "Light Machine Gun", (int)ModuleType.Guns, 20, delegate{ EntityManager.Player.LMG(); }) },
        { ModuleType.Sniper, new ModuleData(Sprite.RealSniperModule,Sprite.SniperModule, "Railgun", (int)ModuleType.Guns, 20, delegate{ EntityManager.Player.Sniper(); })},

        { ModuleType.Engines, new ModuleData(Sprite.EngineModule, Sprite.EngineModule, "Engines", (int)ModuleType.Engines, 20, delegate{ EntityManager.Player.Dash(); }) },
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
public class ItemData(Sprite _realSprite, Sprite _virtualSprite, String _name, int _id, Color _color)
{
    public Texture2D RealSprite { get; } = Assets.Get(_realSprite);
    public Texture2D VirtualSprite { get; } = Assets.Get(_virtualSprite);
    public string Name { get; } = _name;
    public int ID { get; } = _id;
    public Color Color { get; } = _color;
}
public enum ItemType
{
    Scrap
}
