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
    public ItemSlot Parent { get; set; }
    public Texture2D Texture { get { return itemData.Sprite; } }
    public Window Tooltip { get; } = new Window(Vector2.Zero, Assets.Get(Sprite.WideButton));
    public String Name { get { return itemData.Name; } }
    public virtual Color Color { get { return itemData.Color; } }
    private int hitsLeft = 3;
    protected float invincibilityCooldown = 0;
    public int ID
    {
        get { return itemData.ID; }
    }
    public Pickup(ItemData _itemData, Texture2D _worldTexture, Color _worldColor, Vector2 _position, Vector2 _velocity, float _angularVelocity)
    {
        texture = _worldTexture;
        Engine.WriteLine(ColliderRadius);
        itemData = _itemData;
        position = _position;
        velocity = _velocity;
        angle = 0;
        angularVelocity = _angularVelocity;
        color = _worldColor;
        Tooltip.AddWidget(new Decal(new Vector2(-Tooltip.Size.X / 3, 0) - new Vector2(_itemData.Sprite.Width, _itemData.Sprite.Height)/2, _itemData.Sprite));
        Tooltip.AddWidget(new Decal(new Vector2(0, -5), Assets.TextFont, _itemData.Name, Color.White,  5f));
        isFriendly = true;
    }

    public override void Update()
    {
        if (!EntityManager.Player.leashedMaterials.Contains(this))
        {
            if (EntityManager.DistanceSqr(EntityManager.Player, this) < 1375 && EntityManager.Player.leashedMaterials.Count < 3 && EntityManager.Player.canGatherResources == true)
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
        var nearestProjectile = EntityManager.NearestProjectile(this);
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
        ParticleManager.Add(new Particle(null, 1, position + new Vector2(0, -1), new Vector2(0, -1.5f), 0, 0, 1, true, Color.Orange, Color.Red) { drawText = $"Integrity: {hitsLeft}" });
        SoundManager.PlaySound(Assets.Get(Sound.Death), position);
        Engine.ShakeScreen(10 / ((position - Engine.Camera.Position).Length() + 150));
        if (hitsLeft > 0)
        {
            hitsLeft--;
            if (_damage >= 10)
            {
                hitsLeft--;
            }
            invincibilityCooldown = 1;
        }
        else
        {
            isExpired = true;
        }
    }
}
public static class ItemFactory
{
    //Items
    private static Dictionary<ItemType, (ItemData data, Sprite sprite)> itemData = new()
    {
        { ItemType.Scrap, (new ItemData(Sprite.RealMetalScrap, "Metal Salvage", 1, Color.White), Sprite.MetalScrap) },
    };
    private static Dictionary<ModuleType, (ModuleData data, Sprite sprite)> moduleData = new()
    {
        { ModuleType.Hull, (new ModuleData(Sprite.HullModule, "Hull", (int)ModuleType.Hull, 20, Array.Empty<float>(), 1), Sprite.HullModule) },

        { ModuleType.Basic, (new ModuleData(Sprite.RealGunModule, "Basic", (int)ModuleType.Guns, 20, Array.Empty<float>(), 0), Sprite.GunModule) },
        { ModuleType.Spiral, (new ModuleData(Sprite.RealGunModule, "Spiral", (int)ModuleType.Guns, 20, Array.Empty<float>(), 1), Sprite.GunModule) },
        { ModuleType.Shotgun, (new ModuleData(Sprite.RealGunModule, "Shotgun", (int)ModuleType.Guns, 20, Array.Empty<float>(), 2), Sprite.GunModule) },
        { ModuleType.Missile, (new ModuleData(Sprite.RealMissileModule, "Missile", (int)ModuleType.Guns, 20, Array.Empty<float>(), 3), Sprite.MissileModule) },
        { ModuleType.LMG, (new ModuleData(Sprite.RealGunModule, "Light Machine Gun", (int)ModuleType.Guns, 20, Array.Empty<float>(), 4), Sprite.GunModule) },
        { ModuleType.Sniper, (new ModuleData(Sprite.RealSniperModule, "Railgun", (int)ModuleType.Guns, 20, Array.Empty<float>(), 5), Sprite.SniperModule)},

        { ModuleType.Engines, (new ModuleData(Sprite.EngineModule, "Engines", (int)ModuleType.Engines, 20, Array.Empty<float>(), 0), Sprite.EngineModule) },

        { ModuleType.Sensors, (new ModuleData(Sprite.SensorModule, "Sensors", (int)ModuleType.Sensors, 20, Array.Empty<float>(), 0), Sprite.SensorModule) },

        { ModuleType.Core, (new ModuleData(Sprite.CoreModule, "Core", (int)ModuleType.Core, 20, Array.Empty<float>(), 0), Sprite.CoreModule) }
    };
    public static Pickup NewScrap(Vector2 _position = new Vector2(), Vector2 _velocity = new Vector2(), float _angularVelocity = 0)
    {
        return new Pickup(itemData[0].data, Assets.Get(itemData[0].sprite), Color.Cyan, _position, _velocity, _angularVelocity);
    }
    public static Pickup GetItem(ItemType _item, Vector2 _position = new Vector2(), Vector2 _velocity = new Vector2(), float _angularVelocity = 0)
    {
        return new Pickup(itemData[_item].data, Assets.Get(itemData[_item].sprite), Color.Cyan, _position, _velocity, _angularVelocity);
    }
    public static Module GetItem(ModuleType _item, Vector2 _position = new Vector2(), Vector2 _velocity = new Vector2(), float _angularVelocity = 0)
    {
        return new Module(moduleData[_item].data, Assets.Get(moduleData[_item].sprite), Color.Cyan, _position, _velocity, _angularVelocity);
    }
}
public class ItemData
{
    private Sprite sprite;
    public Texture2D Sprite
    {
        get { return Assets.Get(sprite); }
    }
    public string Name { get; private set; }
    public int ID { get; private set; }
    public Color Color { get; private set; }
    public ItemData(Sprite _sprite, String _name, int _id, Color _color)
    {
        sprite = _sprite;
        Name = _name;
        ID = _id;
        Color = _color;
    }
}
public class ModuleData : ItemData
{
    public float MaxHealth { get; private set; }
    public float[] Cost { get; private set; }
    public int WeaponID { get; private set; }
    public ModuleData(Sprite _sprite, String _name, int _id, int _health, float[] _cost, int _weaponId) : base(_sprite, _name, _id, Color.White)
    {
        MaxHealth = _health;
        Cost = _cost;
        WeaponID = _weaponId;
    }
}
public enum ItemType
{
    Scrap
}
