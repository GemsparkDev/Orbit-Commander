using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Space_Wars.Content.Main.Particles;
using System;

namespace Space_Wars.Content.Main.Entities
{
    public abstract class Projectile : Entity
    {
        private Random random = new ();
        public Entity targetEntity;
        public float timeLeft = 10;
        public override void Update()
        {
            AI();

            position += velocity * Engine.deltaSeconds * 60;
            angle += angularVelocity * Engine.deltaSeconds * 60;
            timeLeft -= Engine.deltaSeconds;
            if(timeLeft <= 0)
            {
                ParticleManager.Add(new Particle(texture, 1, position, velocity, angle, 0, 1, true, color, Color.Black));
                isExpired = true;
            }
        }
        public abstract void AI();
        public override void Collide(int damage)
        {
            int particles = random.Next(2, 4);
            for(int i = 0; i < particles; i++)
            {
                float angle = -(float)random.NextDouble() * MathF.PI / 2 - MathF.PI/4 + MathF.Atan2(velocity.X, -velocity.Y);
                Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (float)(random.NextDouble() * 2 + 2);
                ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), 0.25f, position, particleVelocity, angle, 0, 1, false, Color.Orange, Color.Black));
            }
            isExpired = true;
        }
    }

    public class PulseShot : Projectile
    {
        bool isHoming;
        public PulseShot(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, bool _isFriendly, int _damage, bool _isHoming = false)
        {
            position = _position;
            velocity = _velocity;
            angle = _angle;
            angularVelocity = _angularVelocity;
            isFriendly = _isFriendly;
            entityType = EntityType.Projectile;
            texture = Assets.Get(Sprite.PulseShot);
            damage = _damage;
            isHoming = _isHoming;
            if (isFriendly == true) { color = Color.Orange; }
            else if (isFriendly == false) { color = Color.Red; }
        }

        public override void AI()
        {
            Entity nearestEnemy;
            if (isFriendly == true)
            {
                nearestEnemy = EntityManager.NearestEnemy(this);
            }
            else
            {
                nearestEnemy = EntityManager.player;
            }
            if(isHoming == true)
            {
                if(nearestEnemy != null)
                {
                    float distanceSqr = EntityManager.DistanceSqr(nearestEnemy, this);
                    Vector2 direction = Vector2.Normalize(nearestEnemy.position - position);
                    velocity += (direction) / (MathF.Sqrt(distanceSqr / 100));
                }
            }
            EntityManager.Collide(this, nearestEnemy);
        }
    }
    public class SpiralShot : Projectile
    {
        int sign;
        float time;
        Vector2 positionNormal;
        public SpiralShot(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, bool _isFriendly, int _damage, bool _isOffset)
        {
            position = _position;
            velocity = _velocity;
            angle = _angle;
            angularVelocity = _angularVelocity;
            isFriendly = _isFriendly;
            entityType = EntityType.Projectile;
            texture = Assets.Get(Sprite.SpiralShot);
            damage = _damage;
            time = 0;
            if(_isOffset == true) { sign = -1; }
            else { sign = 1; }
            if (isFriendly == true) { color = Color.Orange; }
            else if (isFriendly == false) { color = Color.Red; }
        }

        public override void AI()
        {
            time += Engine.deltaSeconds;
            positionNormal = Vector2.Normalize(new Vector2(MathF.Cos(angle), MathF.Sin(angle))) * MathF.Sin((time * 9) - MathF.PI / 2) * sign;
            position += positionNormal * Engine.deltaSeconds * 60;
            if (isFriendly == true)
            {
                EntityManager.Collide(this, EntityManager.NearestEnemy(this));
            }
            if (isFriendly == false)
            {
                EntityManager.Collide(this, EntityManager.player);
            }
        }
    }
}
