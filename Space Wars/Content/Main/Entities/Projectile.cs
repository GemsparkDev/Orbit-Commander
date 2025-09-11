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
    public override void Collide(int _damage, bool _ignoreImmunity)
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
            var relativePosition = Vector2.Normalize(nearestEnemy.position - position);
            var normalDirection = Vector2.Normalize(new Vector2(velocity.Y, -velocity.X));
            float dot = relativePosition.X * normalDirection.X + relativePosition.Y * normalDirection.Y;
            velocity += normalDirection * MathF.Sqrt(MathF.Abs(dot)) * MathF.Sign(dot) / 10;
        }
        EntityManager.Collide(this, nearestEnemy);
    }
}
public class SpiralShot : Projectile
{
    int offset;
    float time;
    public SpiralShot(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, bool _isFriendly, int _damage, bool _isOffset, int _stealth = 0)
        : base(Assets.Get(Sprite.SpiralShot), _position, _velocity, _angle, _angularVelocity, _isFriendly, _damage, _stealth)
    {
        entityType = EntityType.Projectile;
        time = 0;
        offset = _isOffset ? 1 : -1;
        color = _isFriendly ? Color.Orange : Color.Red;
    }
    public override void AI()
    {
        position += velocity * Engine.DeltaSeconds * 60;
        angle += angularVelocity * Engine.DeltaSeconds * 60;
        time += Engine.DeltaSeconds;
        Vector2 posOffset = Engine.ToUnitVector(angle) * MathF.Cos(time * 8) * offset * 4;
        position += new Vector2(posOffset.Y, -posOffset.X);
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
        col.A = 0;
        beam = new(Assets.Get(Sprite.Dot), 0.5f, position, angle, 0, 0, 50f, color, EmitterType.EmissionOverDistance) { particleFadeToColor = col };
    }
    public override void AI()
    {
        var nearestEnemy = Engine.EntityManager.Hitscan(position, velocity, velocity.Length() * Engine.DeltaSeconds * 60, false, out Vector2 end, (isFriendly ? -1 : 1));
        position = end;
        angle += angularVelocity * Engine.DeltaSeconds * 60;
        beam.position = position;
        beam.Update();
        if (nearestEnemy.Count > 0)
        {
            nearestEnemy[0].Collide(damage);
            Collide(1, false);
        }
    }
}
public class GrapplingHook : Projectile
{
    internal interface ILatchable
    {
        public Vector2 Position { get; }
        public bool IsExpired { get; }
        public void ApplyForce(Vector2 _force);
    }
    internal class LatchedEntity(Entity _entity) : ILatchable
    {
        public Vector2 Position => _entity.position;
        public bool IsExpired => _entity.isExpired;

        public void ApplyForce(Vector2 _force)
        {
            _entity.velocity -= _force;
        }
    }
    internal class LatchedPlanet(Planet _planet, Vector2 _position) : ILatchable
    {
        private Vector2 offset = Vector2.Normalize(_position - _planet.position) * _planet.radius;
        public Vector2 Position => _planet.position + offset;
        public bool IsExpired => false;

        //Prevents deorbiting planets
        public void ApplyForce(Vector2 _force) { }
    }
    private Entity parent;
    private ILatchable target;
    private float maxDistance = 800;
    public bool IsHooked => target != null;
    public GrapplingHook(Vector2 _position, Vector2 _velocity, float _angle, Entity _parent, bool _isFriendly = true) : base(Assets.Get(Sprite.Microshot), _position, _velocity, _angle, 0, _isFriendly, 0, 0)
    {
        parent = _parent;
        color = _isFriendly ? new Color(0, 255, 255) : Color.Red;
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
                var direction = Vector2.Normalize(position - parent.position);
                var force = direction * (distance - maxDistance) * Engine.DeltaSeconds;
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
            var planet = Engine.SaveGame.CurrentMission.IsColliding(position + velocity * Engine.DeltaSeconds * 60);
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
            var direction = Vector2.Normalize(parent.position - position);
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
        var direction = Vector2.Normalize(parent.position - position);
        float angle = MathF.Atan2(direction.Y, direction.X);
        float distance = Vector2.Distance(parent.position, position);
        float trans = distance * distance / maxDistance / maxDistance;
        for (float i = 0; i < distance / texture.Height / 2; i++)
        {
            _spriteBatch.Draw(texture, position + direction * i * texture.Height * 2, null, color * trans, angle, new Vector2(texture.Width, texture.Height)/2, 1, 0, 0);
        }
        base.Draw(_spriteBatch);
    }
    public override void Collide(int _damage, bool _ignoreImmunity)
    {

    }
}
public class FlameBolt : Projectile
{
    float piercingCooldown = 0;
    float maxTimeLeft;
    private ParticleEmitter emitter;
    public override float ColliderRadius 
    { 
        get 
        {
            float radius = 0;
            if (emitter == null)
            {
                return radius;
            }
            if (emitter.EmitterType == EmitterType.Circle)
            {
                return emitter.particleVelocity;
            }
            return Math.Min((maxTimeLeft - timeLeft) * emitter.particleVelocity, emitter.particleVelocity * emitter.particleTimeAlive) * 60;
        } 
    }
    public FlameBolt(Vector2 _position, Vector2 _velocity, bool _isFriendly, int _damage, float _timeLeft = 0.7f, float _particleVelocity = 1, int _stealth = 0)
        : base(Assets.Get(Sprite.Circle), _position, _velocity, 0, 0, _isFriendly, _damage, _stealth)
    {
        emitter = new ParticleEmitter(Assets.Get(Sprite.Circle), 0.75f, Vector2.Zero, 0, MathF.Tau, _particleVelocity, 1500, new Color(1f, 1f, 0.25f, 1f), EmitterType.EmissionOverTime) { particleFadeToColor = new Color(1f, 0, 0, 0) };
        entityType = EntityType.Projectile;
        color = Color.Transparent;
        timeLeft = _timeLeft;
        maxTimeLeft = _timeLeft;
    }
    public FlameBolt(Vector2 _position, Vector2 _velocity, bool _isFriendly, int _damage, ParticleEmitter _emitter, float _timeLeft = 0.7f, int _stealth = 0)
        : base(Assets.Get(Sprite.Circle), _position, _velocity, 0, 0, _isFriendly, _damage, _stealth)
    {
        emitter = _emitter;
        entityType = EntityType.Projectile;
        color = Color.Transparent;
        timeLeft = _timeLeft;
        maxTimeLeft = _timeLeft;
    }
    public override void AI()
    {
        collider.particleVelocity = ColliderRadius;
        emitter.position = position;
        emitter.Update();
        emitter.offsetVelocity = velocity;
        position += velocity * Engine.DeltaSeconds * 60;
        angle += angularVelocity * Engine.DeltaSeconds * 60;
        emitter.particleTimeAlive = Math.Min(1, MathF.Sqrt(timeLeft));
        if (emitter.EmitterType == EmitterType.Circle)
        {
            emitter.particleVelocity = Math.Min((maxTimeLeft - timeLeft), emitter.particleTimeAlive) * 60;
        }
        if (piercingCooldown > 0)
        {
            piercingCooldown -= Engine.DeltaSeconds;
        }
        Entity nearestEnemy = Engine.EntityManager.NearestEnemy(this);
        if (nearestEnemy != null)
        {
            float combinedRadius = ColliderRadius + nearestEnemy.ColliderRadius;
            if (piercingCooldown <= 0 && isFriendly != nearestEnemy.isFriendly && EntityManager.DistanceSqr(this, nearestEnemy) <= combinedRadius * combinedRadius)
            {
                piercingCooldown = 0.05f;
                nearestEnemy.Collide(damage);
                nearestEnemy.StatusHolder.ApplyStatus(new Fire(2));
            }
        }
    }
}
public class Explosive : Projectile
{
    ParticleEmitter radius;
    ParticleEmitter activationRadius;
    float explosionRadius;
    float time = 0;
    Vector3 col;
    public Explosive(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, bool _isFriendly, int _damage, float _radius, int _stealth = 0) 
        : base(Assets.Get(Sprite.Explosive), _position, _velocity, _angle, _angularVelocity, _isFriendly, _damage, _stealth)
    {
        explosionRadius = _radius;
        radius = new ParticleEmitter(Assets.Get(Sprite.Dot), _position, _radius, Color.Red * 0.5f);
        activationRadius = new ParticleEmitter(Assets.Get(Sprite.Dot), _position, _radius / 2, Color.Red * 0.25f);
        timeLeft = 4;
        col = _isFriendly ? new Vector3(1, 0.65f, 0) : new Vector3(1, 0, 0);
        color = _isFriendly ? Color.Orange : Color.Red;
    }
    public override void AI()
    {
        time += Engine.DeltaSeconds;
        position += velocity * Engine.DeltaSeconds * 60;
        velocity *= (1 - Engine.DeltaSeconds);
        angle += angularVelocity * Engine.DeltaSeconds * 60 + MathF.Sin(time * 4) / 15;
        angularVelocity *= (1 - Engine.DeltaSeconds * 2);
        radius.position = position;
        activationRadius.position = position;
        if (isFriendly)
        {
            radius.Update();
            activationRadius.Update();
        }
        var nearestEnemy = Engine.EntityManager.NearestEnemy(this);
        if (nearestEnemy != null)
        {
            float val = MathF.Cos(time * 100 / ((Math.Abs(Vector2.Distance(nearestEnemy.position, position) - explosionRadius / 2) + 1))) / 4 + 0.75f;
            color = new Color(col.X * val + (1 - val), col.Y * val + (1 - val), col.Z * val + (1 - val));
            if (explosionRadius / 2 > Vector2.Distance(nearestEnemy.position, position))
            {
                isExpired = true;
            }
        }
        if (isExpired)
        {
            int particles = Engine.Random.Next(15, 25);
            for (int i = 0; i < particles; i++)
            {
                float angle = Engine.Random.NextSingle() * MathF.PI * 2;
                Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Engine.Random.NextSingle() * 2 + 2);
                ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), 0.25f, position, particleVelocity + velocity, angle, 0, Color.Yellow, new Color(255, 0, 0, 0)));
            }
            particles = Engine.Random.Next(8, 16);
            for (int i = 0; i < particles; i++)
            {
                float angle = Engine.Random.NextSingle() * MathF.PI * 2;
                Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Engine.Random.NextSingle() * 2 + 2);
                ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.25f, position, particleVelocity + velocity, angle, 0, Color.DarkSlateGray, Color.Transparent));
            }
            Engine.EntityManager.Explode(damage, explosionRadius, position);
            Engine.ShakeScreen(150 / ((position - Engine.Camera.Position).Length() + 300));
            SoundManager.PlaySound(Assets.Get(Sound.Death), position);
        }
    }
}
public class Spewer : Projectile
{
    float cooldown = 0.1f;
    public Spewer(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, bool _isFriendly, int _damage, int _stealth = 0)
        : base(Assets.Get(Sprite.Explosive), _position, _velocity, _angle, _angularVelocity, _isFriendly, _damage, _stealth)
    {
        entityType = EntityType.Projectile;
        color = _isFriendly ? Color.Orange : Color.Red;
    }
    public override void AI()
    {
        position += velocity * Engine.DeltaSeconds * 60;
        angle += angularVelocity * Engine.DeltaSeconds * 60;
        if (cooldown > 0)
        {
            cooldown -= Engine.DeltaSeconds;
        }
        else
        {
            float angle = Engine.Random.NextSingle() * MathF.Tau;
            Vector2 dir = Engine.ToUnitVector(angle);
            Engine.EntityManager.Add(new PulseShot(position, velocity + dir * 6, angle, 0, isFriendly, damage, true));
            cooldown = 0.1f;
            SoundManager.PlaySound(Assets.Get(Sound.LMGFire), position);
        }
    }
}


