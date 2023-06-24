using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Diagnostics;

namespace Space_Wars.Content.Main.Entities
{
    public abstract class Projectile : Entity
    {
        public Entity targetEntity;
        public override void Update()
        {
            AI();

            Position += Velocity;
            Angle += AngularVelocity;
        }
        public abstract void AI();
    }

    public class PulseShot : Projectile
    {
        public PulseShot(Vector2 position, Vector2 velocity, float angle, float angularVelocity, bool isFriendly)
        {
            Position = position;
            Velocity = velocity;
            Angle = angle;
            AngularVelocity = angularVelocity;
            IsFriendly = isFriendly;
            entityType = EntityType.Projectile;
            Texture = Assets.Sprites["PulseShot"];
            damage = 10;
        }
        public override void Collide(int damage)
        {
            IsExpired = true;
        }

        public override void AI()
        {
            if (IsFriendly == true)
            {
                EntityManager.Collide(this, EntityManager.NearestEnemy(this));
            }
            if (IsFriendly == false)
            {
                EntityManager.Collide(this, EntityManager.player);
            }
        }
    }
    public class Scrap : Projectile
    {
        public Scrap(Vector2 position, Vector2 velocity, float angle, float angularVelocity)
        {
            Position = position;
            Velocity = velocity;
            Angle = angle;
            AngularVelocity = angularVelocity;
            IsFriendly = false;
            entityType = EntityType.Projectile;
            Texture = Assets.Sprites["Metal Scrap"];
            damage = 0;
        }
        public override void Collide(int damage)
        {
            IsExpired = true;
            EntityManager.player.mothership.resources.scrap += 1;
        }

        public override void AI()
        {
            if (EntityManager.player.leashedMaterial == null)
            {
                if (EntityManager.DistanceSqr(EntityManager.player, this) < 1250)
                {
                    EntityManager.player.leashedMaterial = this;
                }
            }
            else if (EntityManager.player.leashedMaterial == this)
            {
                if (EntityManager.DistanceSqr(this, EntityManager.player) > 1250)
                {
                    Velocity += (EntityManager.player.Position - Position).ToUnitVector(0) / 8;
                }
                else
                {
                    Velocity = EntityManager.player.Velocity / 1.1f;
                }
                ClampVelocity(MathF.Sqrt(MathF.Pow(EntityManager.player.Velocity.X, 2) + MathF.Pow(EntityManager.player.Velocity.Y, 2)) + 1);
            }
        }
    }

    public class MothershipArrow : Projectile
    {
        public MothershipArrow(Vector2 position, Vector2 velocity, float angle, float angularVelocity, bool isfriendly)
        {
            Position = position;
            Velocity = velocity;
            Angle = angle;
            AngularVelocity = angularVelocity;
            IsFriendly = isfriendly;
            entityType = EntityType.Projectile;
            Texture = Assets.Sprites["Arrow"];
            damage = 0;
        }
        public override void Collide(int damage)
        {
        }

        public override void AI()
        {

        }
    }
}
