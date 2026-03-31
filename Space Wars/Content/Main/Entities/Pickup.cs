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
    private int hitsLeft;
    public Items Type { get; } = Items.Scrap;

    protected ItemData itemData;
    public Texture2D Texture => itemData.RealSprite;
    public Window Tooltip { get; } = new Window(Vector2.Zero, Assets.Get(Sprites.WideButton));
    public String Name => itemData.Name;
    public virtual Color Color => itemData.Color;
    protected float invincibilityCooldown = 5;
    public int ID => itemData.ID;
    private Decal textbox;
    public Pickup(ItemData _itemData, Vector2 _position, Vector2 _velocity, float _angularVelocity, int _integrity = 3)
        : base(_itemData.VirtualSprite, _position, _velocity, 0, _angularVelocity, true)
    {
        itemData = _itemData;
        base.Color = Color.Cyan;
        Tooltip.AddWidget(new Decal(new Vector2(-Tooltip.Size.X / 3, 0), _itemData.RealSprite));
        textbox = new Decal(new Vector2(0, -5), Assets.TextFont, _itemData.Name, _itemData.TextColor, 5f);
        Tooltip.AddWidget(textbox);
        hitsLeft = _integrity;
    }
    public Pickup(ItemData _itemData, List<string> _disassembly, LoadLogger _logger)
        : base(_itemData.VirtualSprite, default, default, 0, 0, true)
    {
        itemData = _itemData;
        base.Color = Color.Cyan;
        Tooltip.AddWidget(new Decal(new Vector2(-Tooltip.Size.X / 3, 0), _itemData.RealSprite));
        textbox = new Decal(new Vector2(0, -5), Assets.TextFont, _itemData.Name, _itemData.TextColor, 5f);
        Tooltip.AddWidget(textbox);
        _logger.Try(delegate { hitsLeft = Int32.Parse(_disassembly[1]);}, 1);
    }
    public void Parse(List<string> _disassembly, LoadLogger _logger)
    {
        _logger.Try(delegate { hitsLeft = Int32.Parse(_disassembly[1]); }, 1);
    }
    public override void Update()
    {
        int index = Player.leashedMaterials.IndexOf(this);
        if (index == -1)
        {
            if (isFriendly == Player.isFriendly && EntityManager.DistanceSqr(Player, this) < 1375 && Player.leashedMaterials.Count < 3 && Player.canGatherResources)
            {
                Player.leashedMaterials.Add(this);
                if (Player.leashedMaterials.Count < 3)
                {
                    SoundManager.PlaySound(Assets.Get(Sound.Interact), Position);
                }
                else
                {
                    SoundManager.PlaySound(Assets.Get(Sound.Full), Position);
                }
            }
            Velocity *= 1 - Engine.DeltaSeconds * 2;
        }
        else
        {
            Entity parent;
            if(index == 0)
            {
                parent = Player;
            }
            else
            {
                parent = Player.leashedMaterials[index - 1];
            }
            var relativePos = Vector2.Normalize(parent.Position - Position);
            Velocity += (parent.Position - relativePos * 20 - Position) * Engine.DeltaSeconds * 0.4f;
            float offset = Util.FIED(0.05f);
            Velocity = parent.Velocity * (1 - offset) + Velocity * (offset);
        }
        Position += Velocity * Engine.DeltaSeconds * 60;
        Angle += AngularVelocity * Engine.DeltaSeconds * 60;
        var nearestProjectile = Engine.EntityManager.NearestProjectile(this, isFriendly);
        if (nearestProjectile != null)
        {
            if (Vector2.Distance(nearestProjectile.Position, this.Position) < nearestProjectile.ColliderRadius + ColliderRadius)
            {
                if(nearestProjectile is Enemy)
                {
                    Collide((nearestProjectile as Enemy).Damage);
                }
                if (nearestProjectile is Projectile)
                {
                    Collide((nearestProjectile as Projectile).Damage);
                }
            }
        }
        if (invincibilityCooldown > 0)
        {
            invincibilityCooldown -= Engine.DeltaSeconds;
        }
        base.Update();
    }
    public override bool Collide(int _damage, bool _ignoreImmunity = false)
    {
        if (_damage <= 0)
        {
            return false;
        }
        if (invincibilityCooldown > 0 && !_ignoreImmunity)
        {
            invincibilityCooldown = 0;
            return false;
        }
        hitsLeft--;
        if (_damage >= 10)
        {
            hitsLeft--;
        }
        //Prevents negative integrity values
        hitsLeft = Math.Max(hitsLeft, 0);
        if (!_ignoreImmunity)
        {
            invincibilityCooldown = 1;
        }
        if (hitsLeft <= 0)
        {
            isExpired = true;
        }
        SoundManager.PlaySound(Assets.Get(Sound.Death), Position);
        Engine.ShakeScreen(10 / ((Position - Engine.Camera.Position).Length() + 150));
        ParticleManager.Add(new Particle(null, 1, Position + new Vector2(0, -1), new Vector2(0, -1.5f), 0, 0, Color.Orange, new Color(255, 0, 0, 0)) { drawText = $"Integrity: {hitsLeft}" });
        return true;
    }
    public virtual string SerializeAttributes()
    {
        return $"{hitsLeft}";
    }
    public virtual string Serialize()
    {
        return $"{{{Type},{hitsLeft}}}";
    }
}
public class ItemData(Sprites _realSprite, Sprites _virtualSprite, String _name, int _id, Color _color, Color? _textColor = null)
{
    public Texture2D RealSprite { get; } = Assets.Get(_realSprite);
    public Texture2D VirtualSprite { get; } = Assets.Get(_virtualSprite);
    public string Name { get; } = _name;
    public int ID { get; } = _id;
    public Color Color { get; } = _color;
    public Color TextColor { get; } = _textColor ?? Color.White;
}
public enum Items
{
    Scrap
}
