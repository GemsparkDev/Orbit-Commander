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
    public int ExtraUpdates { get; protected set; } = 1;
    //Projectiles should always be able to hit potential targets
    public override int SensingAbility { get { return 99; } }
    public Projectile(Texture2D _texture, Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, bool _isFriendly, int _damage, int _stealth)
        : base(_texture, _position, _velocity, _angle, _angularVelocity, _damage, _isFriendly)
    {
        StealthAbility = _stealth;
    }
    public override void Update()
    {
        timeLeft -= Engine.DeltaSeconds;
        if(timeLeft <= 0)
        {
            ParticleManager.Add(new Particle(texture, 1, position, velocity, angle, 0, color, Color.Transparent));
            isExpired = true;
        }
        AI();
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
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), 0.25f, position, particleVelocity, angle, 0, color, Color.Black));
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
        var col = Color.Gold;
        ExtraUpdates = (int)(_velocity.Length() / 6);
        col.A = 0;
        beam = new(Assets.Get(Sprite.Dot), 0.5f, position, angle, 0, 0, 0, 0.5f, color, col, EmitterType.EmissionOverDistance);
    }
    public override void AI()
    {
        position += velocity * Engine.DeltaSeconds * 60 / (float)(ExtraUpdates);
        angle += angularVelocity * Engine.DeltaSeconds * 60 / (float)(ExtraUpdates);
        beam.position = position;
        beam.Update();
        var nearestEnemy = Engine.EntityManager.NearestEnemy(this);
        EntityManager.Collide(this, nearestEnemy);
    }
}
public class GrapplingHook : Projectile
{
    private Entity parent;
    private ILatchable target;
    private float maxDistance = 800;
    public bool IsHooked => target != null;
    public GrapplingHook(Vector2 _position, Vector2 _velocity, float _angle, Entity _parent, bool _isFriendly = true) : base(Assets.Get(Sprite.Microshot), _position, _velocity, _angle, 0, _isFriendly, 0, 0)
    {
        parent = _parent;
        color = _isFriendly ? new Color(0, 255, 255) : new Color(255, 0, 0);
        timeLeft = 60;
    }
    public override void AI()
    {
        velocity *= (1 - Engine.DeltaSeconds) * 0.97f;
        position += velocity * Engine.DeltaSeconds * 60;
        if (target != null)
        {
            position = target.Position;
            float distance = Vector2.Distance(position, parent.position);
            if (distance > maxDistance)
            {
                Vector2 direction = Vector2.Normalize(position - parent.position);
                Vector2 force = direction * (distance - maxDistance) * Engine.DeltaSeconds;
                parent.velocity += force;
                target.ApplyForce(force);
            }
            if (target.IsExpired)
            {
                isExpired = true;
            }
        }
        else
        {
            float distance = Vector2.Distance(position, parent.position);
            if (distance > maxDistance)
            {
                isExpired = true;
            }
            var planet = Engine.EntityManager.CurrentMission.IsColliding(position + velocity * Engine.DeltaSeconds * 60);
            if (planet != null)
            {
                target = new LatchedPlanet(planet, position);
                SoundManager.PlaySound(Assets.Get(Sound.ShieldHit), position);
                maxDistance = distance;
            }
            else
            {
                Entity entity = Engine.EntityManager.NearestEnemy(this);
                if (entity != null && Vector2.Distance(position, entity.position) < (entity.ColliderRadius + ColliderRadius) * 2)
                {
                    target = new LatchedEntity(entity);
                    SoundManager.PlaySound(Assets.Get(Sound.ShieldHit), position);
                    maxDistance = distance;
                }
            }
        }
        if (isExpired)
        {
            velocity = Vector2.Zero;
            Texture2D texture = Assets.Get(Sprite.Dot);
            Vector2 direction = Vector2.Normalize(parent.position - position);
            float angle = MathF.Atan2(direction.Y, direction.X);
            float distance = Vector2.Distance(parent.position, position);
            float trans = distance * distance / maxDistance / maxDistance;
            for (float i = 0; i < distance / texture.Height / 2; i++)
            {
                ParticleManager.Add(new Particle(texture, 1, position + direction * i * texture.Height * 2, velocity, angle, 0, color * trans, Color.Transparent));
            }
            if (timeLeft > 0)
            {
                ParticleManager.Add(new Particle(this.texture, 1, position, velocity, this.angle, 0, color, Color.Transparent));
            }
        }
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        Texture2D texture = Assets.Get(Sprite.Dot);
        Vector2 direction = Vector2.Normalize(parent.position - position);
        float angle = MathF.Atan2(direction.Y, direction.X);
        float distance = Vector2.Distance(parent.position, position);
        float trans = distance * distance / maxDistance / maxDistance;
        for (float i = 0; i < distance / texture.Height / 2; i++)
        {
            _spriteBatch.Draw(texture, position + direction * i * texture.Height * 2, null, color * trans, angle, new Vector2(texture.Width, texture.Height)/2, 1, 0, 0);
        }
        base.Draw(_spriteBatch);
    }
    public override void Collide(int _damage)
    {

    }
}
public interface ILatchable 
{ 
    public Vector2 Position { get; } 
    public bool IsExpired { get; }
    public void ApplyForce(Vector2 _force);
}
public class LatchedEntity(Entity _entity) : ILatchable 
{
    public Vector2 Position => _entity.position;
    public bool IsExpired => _entity.isExpired;

    public void ApplyForce(Vector2 _force)
    {
        _entity.velocity -= _force;
    }
}
public class LatchedPlanet(GravitationalSource _planet, Vector2 _position) : ILatchable 
{
    private Vector2 offset = Vector2.Normalize(_position - _planet.position) * _planet.radius;
    public Vector2 Position => _planet.position + offset;
    public bool IsExpired => false;

    //Prevents deorbiting planets
    public void ApplyForce(Vector2 _force) { }
}

