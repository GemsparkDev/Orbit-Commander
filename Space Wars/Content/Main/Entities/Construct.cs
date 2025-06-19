using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Particles;

namespace Space_Wars.Content.Main.Entities;
public class Construct : Pickup
{
    private ConstructType type;
    private float cooldown = 0;
    private ParticleEmitter attackRadius;
    public Construct(ConstructData _constructData, Color _worldColor, Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, ConstructType _type)
        : base(_constructData, _worldColor, _position, _velocity, _angularVelocity, _constructData.Integrity)
    {
        angle = _angle;
        type = _type;
        int radius = 0;
        if (_type == ConstructType.Bomb)
        {
            isFriendly = false;
            radius = 100;
        }
        else if (_type == ConstructType.Trap)
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
        switch (type)
        {
            case ConstructType.Barricade:
                velocity = Vector2.Zero;
                angle = MathF.Atan2(position.X, -position.Y);
                break;
            case ConstructType.Trap:
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
            case ConstructType.Bomb:
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
        if (type == ConstructType.Bomb && isExpired && isActive)
        {
            var tex = Assets.Get(Sprite.Explosion);
            ParticleManager.Add(new Particle(tex, 3, position, Vector2.Zero, 0, 0, Color.White, Color.Transparent));
            Engine.EntityManager.Explode(400, 100, position);
        }
    }
}
public class ConstructData(Sprite _realSprite, Sprite _virtualSprite, String _name, int _id, int _integrity)
    : ItemData(_realSprite, _virtualSprite, _name, _id, Color.White)
{
    public int Integrity { get; } = _integrity;
}
public enum ConstructType
{
    Barricade,
    Trap,
    Bomb,
}