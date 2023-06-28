using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Reflection.Metadata;

namespace Space_Wars.Content.Main.Entities
{
    public class Enemy : Entity
    {
        private List<IEnumerator<int>> behaviours = new();
        public float TargetAngle = 0;
        public Player player = EntityManager.player;
        public float Cooldown;
        public int Health;
        public int MaxHealth;
        public Vector2 TargetVelocity = Vector2.Zero;
        public Vector2 TargetVector;
        public Enemy(Vector2 position, Vector2 velocity, float angle, float angularVelocity, int damage, int health, Texture2D texture)
        {
            Position = position;
            Velocity = velocity;
            Angle = angle;
            AngularVelocity = angularVelocity;
            entityType = EntityType.Enemy;
            Damage = damage;
            IsFriendly = false;
            Texture = texture;
            Color = Color.Yellow;
            Health = health;
            MaxHealth = Health;
            Cooldown = 2.5f;
        }
        IEnumerable<int> Fighter()
        {
            while (true)
            {
                TargetAngle = (player.Position - Position + (player.Velocity - Velocity)).ToDirection(0);
                if (EntityManager.DistanceSqr(this, player) > MathF.Pow(150, 2))
                {
                    GoToEntity(player, 2);
                }
                else
                {
                    if (Cooldown <= 0)
                    {
                        EntityManager.Add(new PulseShot(Position, Angle.ToUnitVector(0) * 8, Angle, 0, false));
                        Engine.PlaySound(Assets.SoundFX["Fire_1"], Position);
                        Cooldown = 1;
                    }
                }
                RotateTowards(TargetAngle);
                LowerCooldown();

                if (Health < 0)
                {
                    IsExpired = true;
                    Engine.PlaySound(Assets.SoundFX["Death"], Position);
                    EntityManager.Add(new Scrap(Position, Vector2.Zero, Angle, AngularVelocity));
                }
                if (Health > MaxHealth)
                {
                    Health = MaxHealth;
                }

                yield return 0;
            }
        }
        IEnumerable<int> Carrier()
        {
            while (true)
            {
                TargetAngle = ((player.Position - Position) + (player.Velocity - Velocity)).ToDirection(0);
                if (EntityManager.DistanceSqr(this, player) > MathF.Pow(225, 2))
                {
                    GoToEntity(player, 1);
                }
                else if(EntityManager.DistanceSqr(this, player) < MathF.Pow(75, 2))
                {
                    GoToEntity(player, -1);
                    if (Cooldown <= 0)
                    {
                        EntityManager.Add(new PulseShot(Position, Angle.ToUnitVector(0) * 8, Angle, 0, false));
                        Engine.PlaySound(Assets.SoundFX["Fire_1"], Position);
                        Cooldown = 0.25f;
                    }
                }
                else
                {
                    if (Cooldown <= 0)
                    {
                        NewFighter(Position, Angle.ToUnitVector(0) * 8, Angle, 0);
                        Engine.PlaySound(Assets.SoundFX["Fire_2"], Position);
                        Cooldown = 5;
                    }
                }
                RotateTowards(TargetAngle);
                LowerCooldown();

                if (Health < 0)
                {
                    IsExpired = true;
                    Engine.PlaySound(Assets.SoundFX["Death"], Position);
                    EntityManager.Add(new Scrap(Position, Vector2.Zero, Angle, AngularVelocity));
                }
                if (Health > MaxHealth)
                {
                    Health = MaxHealth;
                }

                yield return 0;
            }
        }
        IEnumerable<int> Sniper()
        {
            while (true)
            {
                float timeToHit = 0;
                float prevTimeToHit = 0;
                Vector2 playerIterativePosition = player.Position;
                for(int i = 0; i < 1; i++)
                {
                    timeToHit = MathF.Sqrt(EntityManager.DistanceSqr(Position, playerIterativePosition))/16;
                    playerIterativePosition += player.Velocity * (timeToHit - prevTimeToHit);
                    prevTimeToHit = timeToHit;
                }
                Engine.WriteLine(timeToHit);
                TargetAngle = (playerIterativePosition - Position).ToDirection(0);
                if (EntityManager.DistanceSqr(this, player) > MathF.Pow(400, 2))
                {
                    GoToEntity(player, 1);
                }
                else
                {
                    if (Cooldown <= 0 && MathF.Abs(TargetAngle-Angle) < 0.1f)
                    {
                        EntityManager.Add(new PulseShot(Position, Angle.ToUnitVector(0) * 16, Angle, 0, false));
                        Engine.PlaySound(Assets.SoundFX["Fire_3"], Position);
                        Cooldown = 2.5f;
                    }
                }
                RotateTowards(TargetAngle);
                LowerCooldown();

                if (Health < 0)
                {
                    IsExpired = true;
                    Engine.PlaySound(Assets.SoundFX["Death"], Position);
                    EntityManager.Add(new Scrap(Position, Vector2.Zero, Angle, AngularVelocity));
                }
                if (Health > MaxHealth)
                {
                    Health = MaxHealth;
                }

                yield return 0;
            }
        }
        public static Enemy NewFighter(Vector2 position, Vector2 velocity, float angle, float angularVelocity)
        {
            Enemy enemy = new(position, velocity, angle, angularVelocity, 10, 10, Assets.Sprites["Fighter"]);
            enemy.AddBehaviour(enemy.Fighter());
            EntityManager.Add(enemy);
            return enemy;
        }
        public static Enemy NewCarrier(Vector2 position, Vector2 velocity, float angle, float angularVelocity)
        {
            Enemy enemy = new(position, velocity, angle, angularVelocity, 10, 20, Assets.Sprites["Cruiser"]);
            enemy.AddBehaviour(enemy.Carrier());
            EntityManager.Add(enemy);
            return enemy;
        }
        public static Enemy NewSniper(Vector2 position, Vector2 velocity, float angle, float angularVelocity)
        {
            Enemy enemy = new(position, velocity, angle, angularVelocity, 25, 10, Assets.Sprites["Sniper"]);
            enemy.AddBehaviour(enemy.Sniper());
            EntityManager.Add(enemy);
            return enemy;
        }

        public void LowerCooldown()
        {
            if (Cooldown > 0)
            {
                Cooldown -= Engine.deltaSeconds;
            }
        }
        public void RotateTowards(float angle)
        {
            //Rotates toward target angle
            if (Angle > angle && AngularVelocity > -0.05f)
            {
                AngularVelocity -= 0.025f;
            }
            if (Angle < angle && AngularVelocity < 0.05f)
            {
                AngularVelocity += 0.025f;
            }
        }
        public void GoToEntity(Entity entity, float speed)
        {
            //Relative velocity of the player from the drone
            TargetVelocity = entity.Velocity - Velocity;
            //Vector towards the entity with a force of 1 unit
            TargetVector = (entity.Position - Position).ToUnitVector(0);
            //Matches the entity velocity plus a 1 unit vector towards the player
            Velocity += TargetVector * speed * Engine.deltaSeconds * 10;
        }

        private void AddBehaviour(IEnumerable<int> behaviour)
        {
            behaviours.Add(behaviour.GetEnumerator());
        }

        private void ApplyBehaviours()
        {
            for (int i = 0; i < behaviours.Count; i++)
            {
                if (!behaviours[i].MoveNext())
                    behaviours.RemoveAt(i--);
            }
        }

        public override void Update()
        {
            if (Angle - TargetAngle >= Math.PI)
            {
                Angle -= MathF.PI * 2;
            }
            if (Angle - TargetAngle <= -Math.PI)
            {
                Angle += MathF.PI * 2;
            }

            Position += Velocity;
            Angle += AngularVelocity;
            Velocity *= 0.8f;

            ApplyBehaviours();
        }

        public override void Collide(int damage)
        {
            Health -= damage;
            if (damage > 0)
            {
                Engine.PlaySound(Assets.SoundFX["Hit"], Position);
            }
        }
    }
}
