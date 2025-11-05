using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Particles;
using System.Collections.Generic;
using System.Runtime.Intrinsics;

namespace Space_Wars.Content.Main.Entities;
public class Construct : Pickup
{
    public new Constructs Type { get; }

    private float cooldown = 0;
    private ParticleEmitter attackRadius;
    public Construct(Constructs _type, Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, int _stealth = 0, bool _isFriendly = true)
        : base(ItemFactory.constructData[_type], _position, _velocity, _angularVelocity, ItemFactory.constructData[_type].Integrity)
    {
        angle = _angle;
        Type = _type;
        int radius = 0;
        if (_type == Constructs.Bomb)
        {
            isFriendly = false;
            radius = 100;
        }
        else if (_type == Constructs.Trap)
        {
            radius = 300;
        }
        attackRadius = new ParticleEmitter(Assets.Get(Sprite.Dot), position, radius, new Color(255, 0, 0));
        isFriendly = _isFriendly;
        StealthAbility = _stealth;
        if (!_isFriendly)
        {
            color = Color.Red;
        }
    }
    public Construct(Constructs _type, List<string> _disassembly, LoadLogger _logger)
    : base(ItemFactory.constructData[_type], _disassembly, _logger)
    {
        Type = _type;
        int radius = 0;
        if (_type == Constructs.Bomb)
        {
            isFriendly = false;
            radius = 100;
        }
        else if (_type == Constructs.Trap)
        {
            radius = 300;
        }
        attackRadius = new ParticleEmitter(Assets.Get(Sprite.Dot), position, radius, new Color(255, 0, 0));
    }
    public override void Update()
    {
        if (cooldown > 0)
        {
            cooldown -= Engine.DeltaSeconds;
        }
        attackRadius.position = position;
        Entity nearestEnemy;
        switch (Type)
        {
            case Constructs.Barricade:
                velocity = Vector2.Zero;
                angle = MathF.Atan2(position.X, -position.Y);
                nearestEnemy = Engine.EntityManager.NearestEnemy(new Enemy(position, Vector2.Zero, 0, 0, 0, null, true));
                if (cooldown <= 0 && nearestEnemy != null && Vector2.Distance(nearestEnemy.position, position) < 300)
                {
                    var dir = Vector2.Normalize(nearestEnemy.position - position);
                    Engine.EntityManager.Add(new PulseShot(position, dir * 10, MathF.Atan2(dir.X, -dir.Y), 0, true, 5, true)); 
                    SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
                    cooldown = 1.5f;
                }
                if (Engine.DebugMode)
                {
                    attackRadius.Update();
                }
                break;
            case Constructs.Trap:
                velocity = Vector2.Zero;
                nearestEnemy = Engine.EntityManager.NearestEnemy(new Enemy(position, Vector2.Zero, 0, 0, 0, null, true));
                if (cooldown <= 0 && nearestEnemy != null && Vector2.Distance(nearestEnemy.position, position) < 800)
                {
                    var dir = Vector2.Normalize(nearestEnemy.position - position);
                    var enemies = Engine.EntityManager.Hitscan(position, dir, 800, true, out Vector2 _end, (isFriendly ? -1 : 1));
                    foreach (var enemy in enemies)
                    {
                        enemy.Collide(10);
                    }
                    for (int i = 0; i < (_end - position).Length() / 4; i++)
                    {
                        ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), 1, position + dir * 4 * i, Vector2.Zero, Util.ToAngle(dir), 0, Color.Red, Color.Transparent));
                    }
                    SoundManager.PlaySound(Assets.Get(Sound.LMGFire), position);
                    cooldown = 0.75f;
                }
                if (Engine.DebugMode)
                {
                    attackRadius.Update();
                }
                break;
            case Constructs.Bomb:
                var nearestProjectile = Engine.EntityManager.NearestProjectile(this, !isFriendly);
                if (nearestProjectile != null && Vector2.Distance(nearestProjectile.position, position) < ColliderRadius + nearestProjectile.ColliderRadius)
                {
                    Collide(nearestProjectile.damage);
                    nearestProjectile.Collide(1);
                }
                attackRadius.Update();
                break;
            case Constructs.Furnace:
                velocity *= Util.FIED(0.2f);    
                Vector2 offset = Util.RotateVector2(new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) * 5, angle);
                ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), 1, position + offset, velocity, angle, 0, Color.Orange, Color.Transparent));
                var nearestPickup = Engine.EntityManager.NearestItem(this, false);
                if (nearestPickup == null)
                {
                    break;
                }
                Vector2 relativePosition = nearestPickup.position - position;
                if (relativePosition.X < 7 && relativePosition.X > -7 && relativePosition.Y < 7 && relativePosition.Y > -7)
                {
                    nearestPickup.isExpired = true;
                    if (nearestPickup is Module)
                    {
                        Engine.SaveGame.Scrap += 3;
                    }
                    else
                    {
                        Engine.SaveGame.Scrap++;
                    }
                    SoundManager.PlaySound(Assets.Get(Sound.Full), position);
                }
                break;
            default:
                break;
        }
        base.Update();
    }
    public override bool Collide(int _damage, bool _ignoreImmunity = false)
    {
        bool isActive = !isExpired;
        bool result = base.Collide(_damage);
        if (Type == Constructs.Bomb && isExpired && isActive)
        {
            var tex = Assets.Get(Sprite.Explosion);
            ParticleManager.Add(new Particle(tex, 3, position, Vector2.Zero, 0, 0, Color.White, Color.Transparent));
            Engine.EntityManager.Explode(100, 100, position);
        }
        return result;
    }
    public new string Serialize()
    {
        return $"{{{Type},{SerializeAttributes()}}}";
    }
}
public class ConstructData(Sprite _realSprite, Sprite _virtualSprite, String _name, int _id, int _integrity)
    : ItemData(_realSprite, _virtualSprite, _name, _id, Color.White)
{
    public int Integrity { get; } = _integrity;
}
public enum Constructs
{
    Barricade,
    Trap,
    Bomb,
    SpecializedParts,
    Furnace
}