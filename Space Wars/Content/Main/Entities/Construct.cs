using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Particles;
using System.Collections.Generic;

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
        switch (Type)
        {
            case Constructs.Barricade:
                velocity = Vector2.Zero;
                angle = MathF.Atan2(position.X, -position.Y);
                break;
            case Constructs.Trap:
                velocity = Vector2.Zero;
                var nearestEnemy = Engine.EntityManager.NearestEnemy(new Enemy(position, Vector2.Zero, 0, 0, 0, null, true));
                if (cooldown <= 0 && nearestEnemy != null && Vector2.Distance(nearestEnemy.position, position) < 300)
                {
                    var dir = Vector2.Normalize(nearestEnemy.position - position);
                    float rot = MathF.PI * 2 / 9;
                    for (float i = 0; i < 9; i++)
                    {
                        float angle = MathF.Atan2(dir.X, -dir.Y);
                        Engine.EntityManager.Add(new PulseShot(position, dir * 10, angle, 0, true, 5, true));
                        dir = new Vector2(dir.X * MathF.Cos(rot) - dir.Y * MathF.Sin(rot), dir.X * MathF.Sin(rot) + dir.Y * MathF.Cos(rot));
                    }
                    SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
                    cooldown = 1.5f;
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
        }
        base.Update();
    }
    public override void Collide(int _damage)
    {
        bool isActive = !isExpired;
        base.Collide(_damage);
        if (Type == Constructs.Bomb && isExpired && isActive)
        {
            var tex = Assets.Get(Sprite.Explosion);
            ParticleManager.Add(new Particle(tex, 3, position, Vector2.Zero, 0, 0, Color.White, Color.Transparent));
            Engine.EntityManager.Explode(100, 100, position);
        }
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
}