using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Space_Wars.Content.Main.Particles;
using System;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Space_Wars.Content.Main.Entities;

public abstract class Projectile : Entity
{
    private Random random = new ();
    public Entity targetEntity;
    public float timeLeft = 8;
    //Projectiles should always be able to hit potential targets
    public override int SensingAbility { get { return 99; } }
    public Projectile(Texture2D _texture, Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, bool _isFriendly, int _damage, int _stealth)
        : base(_texture, _position, _velocity, _angle, _angularVelocity, _damage, _isFriendly)
    {
        StealthAbility = _stealth;
    }
    public override void Update()
    {
        AI();

        timeLeft -= Engine.DeltaSeconds;
        if(timeLeft <= 0)
        {
            ParticleManager.Add(new Particle(texture, 1, position, velocity, angle, 0, 1, true, color, Color.Black));
            isExpired = true;
        }
        base.Update();
    }
    public abstract void AI();
    public override void Collide(int _damage)
    {
        int particles = random.Next(2, 4);
        for(int i = 0; i < particles; i++)
        {
            float angle = -(float)random.NextDouble() * MathF.PI / 2 - MathF.PI/4 + MathF.Atan2(velocity.X, -velocity.Y);
            Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (float)(random.NextDouble() * 2 + 2);
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), 0.25f, position, particleVelocity, angle, 0, 1, false, color, Color.Black));
        }
        //Shaking is too intense with high fire rate weapons
        //Engine.ShakeScreen(100f * (float)damage / ((position - Engine.camera.Position).Length() + 1000f));
        isExpired = true;
    }
}

public class PulseShot : Projectile
{
    bool isHoming;
    public PulseShot(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, bool _isFriendly, int _damage, bool _isHoming = false, int _stealth = 0) 
        : base(Assets.Get(Sprite.PulseShot), _position, _velocity, _angle, _angularVelocity, _isFriendly, _damage, _stealth)
    {
        entityType = EntityType.Projectile;
        isHoming = _isHoming;
        color = _isFriendly ? Color.Orange : Color.Red;
    }

    public override void AI()
    {
        position += velocity * Engine.DeltaSeconds * 60;
        angle += angularVelocity * Engine.DeltaSeconds * 60;
        Entity nearestEnemy = Engine.EntityManager.NearestEnemy(this);
        if(isHoming && nearestEnemy != null)
        {
            Vector2 relativePosition = Vector2.Normalize(nearestEnemy.position - position);
            Vector2 normalDirection = Vector2.Normalize(new Vector2(velocity.Y, -velocity.X));
            float dot = relativePosition.X * normalDirection.X + relativePosition.Y * normalDirection.Y;
            velocity += normalDirection * MathF.Sqrt(MathF.Abs(dot)) * MathF.Sign(dot) / 10;
        }
        EntityManager.Collide(this, nearestEnemy);
    }
}
public class SpiralShot : Projectile
{
    bool isOffset;
    float time;
    Vector2 positionNormal;
    public SpiralShot(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, bool _isFriendly, int _damage, bool _isOffset, int _stealth = 0)
        : base(Assets.Get(Sprite.SpiralShot), _position, _velocity, _angle, _angularVelocity, _isFriendly, _damage, _stealth)
    {
        entityType = EntityType.Projectile;
        time = 0;
        isOffset = _isOffset;
        color = _isFriendly ? Color.Orange : Color.Red;
    }

    public override void AI()
    {
        position += velocity * Engine.DeltaSeconds * 60;
        angle += angularVelocity * Engine.DeltaSeconds * 60;
        time += Engine.DeltaSeconds;
        positionNormal = Vector2.Normalize(new Vector2(MathF.Cos(angle), MathF.Sin(angle))) * MathF.Sin((time * 9) - MathF.PI / 2) * (isOffset ? 1 : -1);
        position += positionNormal * Engine.DeltaSeconds * 60;
        EntityManager.Collide(this, Engine.EntityManager.NearestEnemy(this));
    }
}
public class AssassinShot : Projectile
{
    ParticleEmitter beam;
    public AssassinShot(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, bool _isFriendly, int _damage, int _stealth = 0)
        : base(Assets.Get(Sprite.Missile), _position, _velocity, _angle, _angularVelocity, _isFriendly, _damage, _stealth)
    {
        entityType = EntityType.Projectile;
        color = _isFriendly ? Color.Orange : Color.Red;
        timeLeft = 3;
        beam = new(Assets.Get(Sprite.Dot), 0.5f, position, angle, 0, 0, 0, 0.5f, 1, true, color, Color.Gold, EmitterType.EmissionOverDistance);
    }
    public override void AI()
    {
        int check = (int)velocity.Length() / 6;
        for (int i = 0; i < check; i++)
        {
            position += velocity / check * Engine.DeltaSeconds * 60;
            angle += angularVelocity * Engine.DeltaSeconds * 60;
            EntityManager.Collide(this, Engine.EntityManager.NearestEnemy(this));
            if (Engine.EntityManager.CurrentMission.Planet.IsColliding(position))
            {
                isExpired = true;
            }
            if (isExpired)
            {
                beam.position = position;
                beam.Update();
                return;
            }
        }
        beam.position = position;
        beam.Update();
    }
}
