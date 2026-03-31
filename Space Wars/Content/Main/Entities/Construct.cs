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
        Angle = _angle;
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
        attackRadius = new ParticleEmitter(Assets.Get(Sprites.Dot), Position, radius, new Color(255, 0, 0));
        isFriendly = _isFriendly;
        StealthAbility = _stealth;
        if (!_isFriendly)
        {
            //Color = Color.Red;
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
        attackRadius = new ParticleEmitter(Assets.Get(Sprites.Dot), Position, radius, new Color(255, 0, 0));
    }
    public override void Update()
    {
        if (cooldown > 0)
        {
            cooldown -= Engine.DeltaSeconds;
        }
        attackRadius.position = Position;
        Entity nearestEnemy;
        switch (Type)
        {
            case Constructs.Barricade:
                Velocity = Vector2.Zero;
                Angle = MathF.Atan2(Position.X, -Position.Y);
                nearestEnemy = Engine.EntityManager.NearestEnemy(new Enemy(Position, Vector2.Zero, 0, 0, 0, null, isFriendly));
                if (cooldown <= 0 && nearestEnemy != null && Vector2.Distance(nearestEnemy.Position, Position) < 300)
                {
                    var dir = Vector2.Normalize(nearestEnemy.Position - Position);
                    Engine.EntityManager.Add(Projectile.NewPulseShot(Position, dir * 10, MathF.Atan2(dir.X, -dir.Y), 0, isFriendly, 5, true)); 
                    SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Position);
                    cooldown = 1.5f;
                }
                if (SaveGame.DebugMode)
                {
                    attackRadius.Update();
                }
                break;
            case Constructs.Trap:
                Velocity = Vector2.Zero;
                nearestEnemy = Engine.EntityManager.NearestEnemy(new Enemy(Position, Vector2.Zero, 0, 0, 0, null, isFriendly));
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
                if (SaveGame.DebugMode)
                {
                    attackRadius.Update();
                }
                break;
            case Constructs.Bomb:
                var nearestProjectile = Engine.EntityManager.NearestProjectile(this, !isFriendly);
                if (nearestProjectile != null && Vector2.Distance(nearestProjectile.Position, Position) < ColliderRadius + nearestProjectile.ColliderRadius)
                {
                    Collide(1); //Colliding with 1 is a bandaid fix!
                    nearestProjectile.Collide(1);
                }
                attackRadius.Update();
                break;
            case Constructs.Furnace:
                Velocity *= Util.FIED(0.2f);    
                foreach(var enemy in Engine.EntityManager.Entities)
                {
                    if(Vector2.DistanceSquared(enemy.Position, Position) < 3600)
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
            var tex = Assets.Get(Sprites.Explosion);
            ParticleManager.Add(new Particle(tex, 3, Position, Vector2.Zero, 0, 0, Color.White, Color.Transparent));
            Engine.EntityManager.Explode(100, 100, Position);
        }
        return result;
    }
    public new string Serialize()
    {
        return $"{{{Type},{SerializeAttributes()}}}";
    }
}
public class ConstructData(Sprites _realSprite, Sprites _virtualSprite, String _name, int _id, int _integrity, Color? _textColor = null)
    : ItemData(_realSprite, _virtualSprite, _name, _id, Color.White, _textColor)
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