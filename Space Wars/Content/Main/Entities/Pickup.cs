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
        Tooltip.AddWidget(new Decal(new Vector2(-Tooltip.Size.X / 3, 0), _itemData.RealSprite));
        Tooltip.AddWidget(new Decal(new Vector2(0, -5), Assets.TextFont, _itemData.Name, Color.White,  5f));
    }

    public override void Update()
    {
        if (!Player.leashedMaterials.Contains(this))
        {
            if (EntityManager.DistanceSqr(Player, this) < 1375 && Player.leashedMaterials.Count < 3 && Player.canGatherResources)
            {
                Player.leashedMaterials.Add(this);
                if (Player.leashedMaterials.Count < 3)
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
            Vector2 playerVelocity = Player.velocity;
            Vector2 leashPosition = Player.position - Engine.ToUnitVector(Player.angle) * 25;
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
        ParticleManager.Add(new Particle(null, 1, position + new Vector2(0, -1), new Vector2(0, -1.5f), 0, 0, Color.Orange, new Color(255, 0, 0, 0)) { drawText = $"Integrity: {hitsLeft}" });
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
