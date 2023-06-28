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
            Damage = 5;
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
            Damage = 0;
        }
        public override void Collide(int damage)
        {
            IsExpired = true;
        }

        public override void AI()
        {   
            if(!EntityManager.player.leashedMaterials.Contains(this))
            {
                if (EntityManager.DistanceSqr(EntityManager.player, this) < 1250 && EntityManager.player.leashedMaterials.Count < 2 && EntityManager.player.canGatherResources == true)
                {
                    EntityManager.player.leashedMaterials.Add(this);
                }
            }
            else
            {
                Vector2 playerVelocity = EntityManager.player.Velocity;
                Vector2 leashPosition = EntityManager.player.Position-EntityManager.player.Angle.ToUnitVector(0)*25;
                float distance = EntityManager.DistanceSqr(Position, leashPosition);
                if(distance > 16)
                {
                    Velocity += (leashPosition - Position).ToUnitVector(0) * Engine.deltaSeconds * distance;
                }
                else
                {
                    Velocity += (playerVelocity - Velocity) / 2;
                }
                ClampVelocity(MathF.Sqrt(playerVelocity.X * playerVelocity.X + playerVelocity.Y * playerVelocity.Y) + 1);
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
            Damage = 0;
        }
        public override void Collide(int damage)
        {
        }

        public override void AI()
        {

        }
    }
}
