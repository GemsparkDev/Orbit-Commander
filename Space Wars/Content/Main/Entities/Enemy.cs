using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Space_Wars.Content.Main.Entities
{
    public class Enemy : Entity
    {
        private List<IEnumerator<int>> behaviours = new();
        public float targetAngle = 0;
        public Player player = EntityManager.player;
        public float cooldown;
        public int health;
        public int maxHealth;
        public Vector2 targetVelocity = Vector2.Zero;
        public Vector2 targetVector;
        public Enemy(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, int _damage, int _health, Texture2D _texture)
        {
            position = _position;
            velocity = _velocity;
            angle = _angle;
            angularVelocity = _angularVelocity;
            entityType = EntityType.Enemy;
            damage = _damage;
            isFriendly = false;
            texture = _texture;
            color = Color.Yellow;
            health = _health;
            maxHealth = health;
            cooldown = 2.5f;
        }
        IEnumerable<int> Fighter()
        {
            while (true)
            {
                targetVector = player.position - position + (player.velocity - velocity);
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                if (EntityManager.DistanceSqr(this, player) > MathF.Pow(150, 2))
                {
                    GoToEntity(player, 2);
                }
                else
                {
                    if (cooldown <= 0)
                    {
                        EntityManager.Add(new PulseShot(position, Engine.ToUnitVector(angle) * 8, angle, 0, false));
                        Engine.PlaySound(Assets.SoundFX["Fire_1"], position);
                        cooldown = 1;
                    }
                }
                RotateTowards(targetAngle);
                LowerCooldown();

                if (health < 0)
                {
                    isExpired = true;
                    Engine.PlaySound(Assets.SoundFX["Death"], position);
                    EntityManager.Add(new Scrap(position, Vector2.Zero, angle, angularVelocity));
                }
                if (health > maxHealth)
                {
                    health = maxHealth;
                }

                yield return 0;
            }
        }
        IEnumerable<int> Carrier()
        {
            while (true)
            {
                targetVector = player.position - position + (player.velocity - velocity);
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                if (EntityManager.DistanceSqr(this, player) > MathF.Pow(225, 2))
                {
                    GoToEntity(player, 1);
                }
                else if (EntityManager.DistanceSqr(this, player) < MathF.Pow(75, 2))
                {
                    GoToEntity(player, -1);
                    if (cooldown <= 0)
                    {
                        EntityManager.Add(new PulseShot(position, Engine.ToUnitVector(angle) * 8, angle, 0, false));
                        Engine.PlaySound(Assets.SoundFX["Fire_1"], position);
                        cooldown = 0.25f;
                    }
                }
                else
                {
                    if (cooldown <= 0)
                    {
                        NewFighter(position, Engine.ToUnitVector(angle) * 8, angle, 0);
                        Engine.PlaySound(Assets.SoundFX["Fire_2"], position);
                        cooldown = 5;
                    }
                }
                RotateTowards(targetAngle);
                LowerCooldown();

                if (health < 0)
                {
                    isExpired = true;
                    Engine.PlaySound(Assets.SoundFX["Death"], position);
                    EntityManager.Add(new Scrap(position, Vector2.Zero, angle, angularVelocity));
                }
                if (health > maxHealth)
                {
                    health = maxHealth;
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
                Vector2 playerIterativePosition = player.position;
                for (int i = 0; i < 1; i++)
                {
                    timeToHit = MathF.Sqrt(EntityManager.DistanceSqr(position, playerIterativePosition)) / 16;
                    playerIterativePosition += player.velocity * (timeToHit - prevTimeToHit);
                    prevTimeToHit = timeToHit;
                }
                Engine.WriteLine(timeToHit);
                targetVector = (playerIterativePosition - position);
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                if (EntityManager.DistanceSqr(this, player) > MathF.Pow(400, 2))
                {
                    GoToEntity(player, 1);
                }
                else
                {
                    if (cooldown <= 0 && MathF.Abs(targetAngle - angle) < 0.1f)
                    {
                        EntityManager.Add(new PulseShot(position, Engine.ToUnitVector(angle) * 16, angle, 0, false));
                        Engine.PlaySound(Assets.SoundFX["Fire_3"], position);
                        cooldown = 2.5f;
                    }
                }
                RotateTowards(targetAngle);
                LowerCooldown();

                if (health < 0)
                {
                    isExpired = true;
                    Engine.PlaySound(Assets.SoundFX["Death"], position);
                    EntityManager.Add(new Scrap(position, Vector2.Zero, angle, angularVelocity));
                }
                if (health > maxHealth)
                {
                    health = maxHealth;
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
            if (cooldown > 0)
            {
                cooldown -= Engine.deltaSeconds;
            }
        }
        public void RotateTowards(float _angle)
        {
            //Rotates toward target angle
            if (angle > _angle && angularVelocity > -0.05f)
            {
                angularVelocity -= 0.025f;
            }
            if (angle < _angle && angularVelocity < 0.05f)
            {
                angularVelocity += 0.025f;
            }
        }
        public void GoToEntity(Entity entity, float speed)
        {
            //Relative velocity of the player from the drone
            targetVelocity = entity.velocity - velocity;
            //Vector towards the entity with a force of 1 unit
            targetVector = Vector2.Normalize(entity.position - position);
            //Matches the entity velocity plus a 1 unit vector towards the player
            velocity += targetVector * speed * Engine.deltaSeconds * 10;
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
            if (angle - targetAngle >= Math.PI)
            {
                angle -= MathF.PI * 2;
            }
            if (angle - targetAngle <= -Math.PI)
            {
                angle += MathF.PI * 2;
            }

            position += velocity;
            angle += angularVelocity;
            velocity *= 0.8f;

            ApplyBehaviours();
        }

        public override void Collide(int damage)
        {
            health -= damage;
            if (damage > 0)
            {
                Engine.PlaySound(Assets.SoundFX["Hit"], position);
            }
        }
    }
}
