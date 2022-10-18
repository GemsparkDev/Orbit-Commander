using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main.Entities
{
    public class Mothership : Entity
    {
        private float Health;
        private float MaxHealth;
        public PlayerResources resources;
        public Mothership(Vector2 position, Vector2 velocity, float angle, float angularVelocity)
        {
            Position = position;
            Velocity = velocity;
            Angle = angle;
            AngularVelocity = angularVelocity;
            Texture = Assets.Sprites["Mothership"];
            Health = 100;
            MaxHealth = Health;
            IsFriendly = true;
            entityType = EntityType.None;
        }

        public override void Update()
        {
            if (Health <= 0)
            {
                IsExpired = true;
                Assets.SoundFX["Death"].Play();
            }
            if (Health > MaxHealth)
            {
                Health = MaxHealth;
            }

            Position += Velocity;
            Angle += AngularVelocity;
            AngularVelocity = 0;

            ClampVelocity(2);
            if (EntityManager.player.docked == true)
            {
                EntityManager.player.ClampVelocity(2);
            }
        }
        public override void Collide(int damage)
        {
            Health -= damage;
            if (damage > 0)
            {
                Assets.SoundFX["Hit"].Play();
            }
        }
    }
    public struct PlayerResources
    {
        public int scrap;
        public int titanium;
        public int copper;
    }
}
