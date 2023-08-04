using Microsoft.Xna.Framework;
using System;

namespace Space_Wars.Content.Main.Entities
{
    public abstract class Projectile : Entity
    {
        public Entity targetEntity;
        public override void Update()
        {
            AI();

            position += velocity * Engine.deltaSeconds * 60;
            angle += angularVelocity * Engine.deltaSeconds * 60;
        }
        public abstract void AI();
    }

    public class PulseShot : Projectile
    {
        public PulseShot(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, bool _isFriendly, int _damage)
        {
            position = _position;
            velocity = _velocity;
            angle = _angle;
            angularVelocity = _angularVelocity;
            isFriendly = _isFriendly;
            entityType = EntityType.Projectile;
            texture = Assets.Sprites["PulseShot"];
            damage = _damage;
            if (isFriendly == true) { color = new Color(0, 255, 0); }
            else if (isFriendly == false) { color = Color.Red; }
        }
        public override void Collide(int damage)
        {
            isExpired = true;
        }

        public override void AI()
        {
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

    public class MothershipArrow : Projectile
    {
        public MothershipArrow(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, bool _isfriendly)
        {
            position = _position;
            velocity = _velocity;
            angle = _angle;
            angularVelocity = _angularVelocity;
            isFriendly = _isfriendly;
            entityType = EntityType.Projectile;
            texture = Assets.Sprites["Arrow"];
            damage = 0;
        }
        public override void Collide(int damage)
        {

        }

        public override void AI()
        {

        }
    }
    public class SpiralShot : Projectile
    {
        bool isOffset;
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
            texture = Assets.Sprites["PulseShot"];
            damage = _damage;
            isOffset = _isOffset;
            time = 0;
            if(isOffset == true) { sign = -1; }
            else { sign = 1; }
            if (isFriendly == true) { color = new Color(0, 255, 0); }
            else if (isFriendly == false) { color = Color.Red; }
        }
        public override void Collide(int damage)
        {
            isExpired = true;
        }

        public override void AI()
        {
            time += Engine.deltaSeconds;
            positionNormal = Vector2.Normalize(new Vector2(-velocity.Y, velocity.X)) * MathF.Sin((time * 9) - MathF.PI / 2) * sign;
            position += positionNormal * Engine.deltaSeconds * 60;
            angle = MathF.Atan2(positionNormal.X + velocity.X, -positionNormal.Y + -velocity.Y);
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
