using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Components;
using Space_Wars.Content.Main.Particles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UILib.Content.Main;

namespace Space_Wars.Content.Main.Entities;
public class Pickup : Entity, IData
{
    Texture2D IData.Texture => itemData.RealSprite;
    Color IData.Color => itemData.Color;
    public Items Type => itemData.Type;
    protected ItemData itemData;
    public Window Tooltip { get; } = new Window(Vector2.Zero, Assets.Get(Sprites.WideButton));
    public String Name => itemData.Name;

    public float InvincibilityCooldown { get { return GetComponent<Collide>().InvincibilityCooldown; } set { GetComponent<Collide>().InvincibilityCooldown = value; } }
    public int ID => itemData.ID;
    private Decal textbox;
    public Pickup(ItemData _itemData, Vector2 _position, Vector2 _velocity, float _angularVelocity, int _health = 3)
        : base(_itemData.VirtualSprite, _position, _velocity, 0, _angularVelocity, true)
    {
        itemData = _itemData;
        base.Color = Color.Cyan;
        Tooltip.AddWidget(new Decal(new Vector2(-Tooltip.Size.X / 3, 0), _itemData.RealSprite));
        textbox = new Decal(new Vector2(0, -5), Assets.TextFont, _itemData.Name, _itemData.TextColor, 5f);
        Tooltip.AddWidget(textbox);
        AddComponent(new Health(this) { CurrentHealth = _health, MaxHealth = _health});
        AddComponent(new KeyTag(this));
        AddComponent(new Collide(this, delegate(int _damage, bool _ignoreImmunity)
        {
            if (_damage <= 0)
            {
                return false;
            }
            if (InvincibilityCooldown > 0 && !_ignoreImmunity)
            {
                InvincibilityCooldown = 0;
                return false;
            }
            Health-=_damage;
            if (!_ignoreImmunity)
            {
                InvincibilityCooldown = 1;
            }
            if (Health <= 0)
            {
                isExpired = true;
            }
            SoundManager.PlaySound(Assets.Get(Sound.Death), Position);
            Engine.ShakeScreen(10 / ((Position - Engine.Camera.Position).Length() + 150));
            ParticleManager.Add(new Particle(null, 1, Position + new Vector2(0, -1), new Vector2(0, -1.5f), 0, 0, Color.Orange, new Color(255, 0, 0, 0)) { drawText = $"Integrity: {Math.Max(Health, 0)}" });
            return true;
        }));
        InvincibilityCooldown = 5;
    }
    //TODO: Review all serialization!
    public Pickup(ItemData _itemData, List<string> _disassembly, LoadLogger _logger)
        : base(_itemData.VirtualSprite, default, default, 0, 0, true)
    {
        throw new NotImplementedException();
        itemData = _itemData;
        base.Color = Color.Cyan;
        Tooltip.AddWidget(new Decal(new Vector2(-Tooltip.Size.X / 3, 0), _itemData.RealSprite));
        textbox = new Decal(new Vector2(0, -5), Assets.TextFont, _itemData.Name, _itemData.TextColor, 5f);
        Tooltip.AddWidget(textbox);
        _logger.Try(delegate { Health = Int32.Parse(_disassembly[1]);}, 1);
    }
    public void Parse(List<string> _disassembly, LoadLogger _logger)
    {
        _logger.Try(delegate { Health = Int32.Parse(_disassembly[1]); }, 1);
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
        base.Update();
    }
    public virtual string SerializeAttributes()
    {
        return $"{Health}";
    }
    public virtual string Serialize()
    {
        return $"{{{Type},{Health}}}";
    }
    IEnumerable<int> Barricade()
    {
        float cooldown = 0;
        Entity nearestEnemy;
        while (true)
        {
            if (cooldown > 0)
            {
                cooldown -= Engine.DeltaSeconds;
            }
            Velocity = Vector2.Zero;
            Angle = MathF.Atan2(Position.X, -Position.Y);
            nearestEnemy = Engine.EntityManager.NearestEnemy(new Enemy(Position, Vector2.Zero, 0, 0, null, isFriendly));
            if (cooldown <= 0 && nearestEnemy != null && Vector2.Distance(nearestEnemy.Position, Position) < 300)
            {
                var dir = Vector2.Normalize(nearestEnemy.Position - Position);
                Engine.EntityManager.Add(NewPulseShot(Position, dir * 10, MathF.Atan2(dir.X, -dir.Y), 0, isFriendly, 5, true));
                SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Position);
                cooldown = 1.5f;
            }
            GetComponent<Emitter>().ParticleEmitter.isEmitterActive = SaveGame.DebugMode;
            yield return 0;
        }
    }
    public static Pickup NewBarricade(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, int _stealth = 0, bool _isFriendly = true)
    {
        var construct = new Pickup(ItemFactory.itemData[Items.Barricade], _position, _velocity, _angularVelocity, ItemFactory.itemData[Items.Barricade].Integrity);
        construct.AddComponent(new Behaviour(construct).AddBehaviour(construct.Barricade()));
        construct.Angle = _angle;
        construct.StealthAbility = _stealth;
        construct.isFriendly = _isFriendly;
        return construct;
    }
    IEnumerable<int> Trap()
    {
        float cooldown = 0;
        Entity nearestEnemy;
        while (true)
        {
            if (cooldown > 0)
            {
                cooldown -= Engine.DeltaSeconds;
            }
            Velocity = Vector2.Zero;
            nearestEnemy = Engine.EntityManager.NearestEnemy(new Enemy(Position, Vector2.Zero, 0, 0, null, isFriendly));
            if (cooldown <= 0 && nearestEnemy != null && Vector2.Distance(nearestEnemy.Position, Position) < 800)
            {
                var dir = Vector2.Normalize(nearestEnemy.Position - Position);
                var enemies = Engine.EntityManager.Hitscan(Position, dir, 800, true, out Vector2 _end, (isFriendly ? -1 : 1));
                foreach (var enemy in enemies)
                {
                    enemy.Collide(10);
                }
                for (int i = 0; i < (_end - Position).Length() / 4; i++)
                {
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 1, Position + dir * 4 * i, Vector2.Zero, Util.ToAngle(dir), 0, Color.Red, Color.Transparent));
                }
                SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Position);
                cooldown = 0.75f;
            }
            GetComponent<Emitter>().ParticleEmitter.isEmitterActive = SaveGame.DebugMode;
            yield return 0;
        }
    }
    public static Pickup NewTrap(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, int _stealth = 0, bool _isFriendly = true)
    {
        var construct = new Pickup(ItemFactory.itemData[Items.Trap], _position, _velocity, _angularVelocity, ItemFactory.itemData[ Items.Trap].Integrity);
        construct.AddComponent(new Behaviour(construct).AddBehaviour(construct.Trap()));
        construct.AddComponent(new Emitter(construct) { ParticleEmitter = new ParticleEmitter(Assets.Get(Sprites.Dot), _position, 300, new Color(255, 0, 0)) });
        construct.Angle = _angle;
        construct.StealthAbility = _stealth;
        construct.isFriendly = _isFriendly;
        return construct;
    }
    IEnumerable<int> Bomb()
    {
        while (!isExpired)
        {
            var nearestProjectile = Engine.EntityManager.NearestProjectile(this, !isFriendly);
            if (nearestProjectile != null && Vector2.Distance(nearestProjectile.Position, Position) < ColliderRadius + nearestProjectile.ColliderRadius)
            {
                Collide(1); //Colliding with 1 is a bandaid fix!
                nearestProjectile.Collide(1);
            }
            yield return 0;
        }
        var tex = Assets.Get(Sprites.Explosion);
        ParticleManager.Add(new Particle(tex, 3, Position, Vector2.Zero, 0, 0, Color.White, Color.Transparent));
        Engine.EntityManager.Explode(100, 100, Position);
        yield return 1;
    }
    public static Pickup NewBomb(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, int _stealth = 0)
    {
        var construct = new Pickup(ItemFactory.itemData[Items.Bomb], _position, _velocity, _angularVelocity, ItemFactory.itemData[Items.Bomb].Integrity);
        construct.AddComponent(new Behaviour(construct).AddBehaviour(construct.Bomb()));
        construct.AddComponent(new Emitter(construct) { ParticleEmitter = new ParticleEmitter(Assets.Get(Sprites.Dot), _position, 100, new Color(255, 0, 0)) });
        construct.Angle = _angle;
        construct.StealthAbility = _stealth;
        construct.isFriendly = false;
        return construct;
    }
    IEnumerable<int> Furnace()
    {
        float cooldown = 0;
        while (true)
        {
            if (cooldown > 0)
            {
                cooldown -= Engine.DeltaSeconds;
            }
            Velocity *= Util.FIED(0.2f);
            foreach (var enemy in Engine.EntityManager.Entities)
            {
                if (Vector2.DistanceSquared(enemy.Position, Position) < 3600)
                {
                    enemy.ApplyWork(5);
                }
            }
            Vector2 offset = Util.RotateVector2(new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) * 5, Angle);
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 1, Position + offset, Velocity, Angle, 0, Color.Orange, Color.Transparent));
            var nearestPickup = Engine.EntityManager.NearestItem(this, true);
            if (nearestPickup == null)
            {
                break;
            }
            Vector2 relativePosition = nearestPickup.Position - Position;
            if (relativePosition.X < 7 && relativePosition.X > -7 && relativePosition.Y < 7 && relativePosition.Y > -7)
            {
                nearestPickup.Position = Position;
                if (Player.leashedMaterials.Contains(nearestPickup as Pickup))
                {
                    Player.leashedMaterials.Remove(nearestPickup as Pickup);
                }
                cooldown += Engine.DeltaSeconds * 2;
                ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 1, Position + Util.RotateVector2(new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) * 5, Angle),
                    Velocity, Angle, 0, Color.Orange, Color.Transparent));
                if (cooldown > 15)
                {
                    nearestPickup.isExpired = true;
                    cooldown = 0;
                    if (nearestPickup is Module)
                    {
                        Engine.SaveGame.Scrap += 3;
                    }
                    else
                    {
                        Engine.SaveGame.Scrap++;
                    }
                    SoundManager.PlaySound(Assets.Get(Sound.Full), Position);
                }
            }
            yield return 0;
        }
    }
    public static Pickup NewFurnace(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, int _stealth = 0, bool _isFriendly = true)
    {
        var construct = new Pickup(ItemFactory.itemData[Items.Furnace], _position, _velocity, _angularVelocity, ItemFactory.itemData[Items.Furnace].Integrity);
        construct.AddComponent(new Behaviour(construct).AddBehaviour(construct.Bomb()));
        construct.AddComponent(new Emitter(construct) { ParticleEmitter = new ParticleEmitter(Assets.Get(Sprites.Dot), _position, 100, new Color(255, 0, 0)) });
        construct.Angle = _angle;
        construct.StealthAbility = _stealth;
        construct.isFriendly = _isFriendly;
        return construct;
    }
    public static Pickup NewSpecializedParts(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, int _stealth = 0, bool _isFriendly = true)
    {
        var construct = new Pickup(ItemFactory.itemData[Items.SpecializedParts], _position, _velocity, _angularVelocity, ItemFactory.itemData[Items.SpecializedParts].Integrity)
        {
            Angle = _angle,
            StealthAbility = _stealth,
            isFriendly = _isFriendly
        };
        construct.AddComponent(new SpecializedTag(construct));
        return construct;
    }
}
public class ItemData(Sprites _realSprite, Sprites _virtualSprite, String _name, int _id, Color _color, Color? _textColor = null, int _integrity = 3)
{
    public Texture2D RealSprite { get; } = Assets.Get(_realSprite);
    public Texture2D VirtualSprite { get; } = Assets.Get(_virtualSprite);
    public string Name { get; } = _name;
    public int ID { get; } = _id;
    public Color Color { get; } = _color;
    public Color TextColor { get; } = _textColor ?? Color.White;
    public Items Type { get; } = Items.Scrap;
    public int Integrity { get; } = _integrity;
}
public enum Items
{
    Scrap,
    Barricade,
    Trap,
    Bomb,
    SpecializedParts,
    Furnace
}
