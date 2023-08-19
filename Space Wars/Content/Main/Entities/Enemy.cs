using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Particles;
using Space_Wars.Content.Main.UI_Elements;
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
        public ParticleEmitter enemyRange = new(Assets.Sprites["Dot"], Vector2.Zero, 0, 1, Color.Red);
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
            color = Color.Red;
            health = _health;
            maxHealth = health;
            cooldown = 2.5f;
            enemyRange.position = position;
            ParticleManager.Add(enemyRange);
        }
        public Enemy(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, int _damage, int _health, Texture2D _texture, bool _isFriendly)
        {
            position = _position;
            velocity = _velocity;
            angle = _angle;
            angularVelocity = _angularVelocity;
            entityType = EntityType.Enemy;
            damage = _damage;
            isFriendly = _isFriendly;
            texture = _texture;
            color = Color.Red;
            health = _health;
            maxHealth = health;
            cooldown = 2.5f;
            enemyRange.position = position;
            ParticleManager.Add(enemyRange);
        }
        IEnumerable<int> Fighter()
        {
            enemyRange.radius = 250;
            while (true)
            {
                enemyRange.position = position;
                targetVector = player.position - position + (player.velocity - velocity) * 8;
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                if (EntityManager.DistanceSqr(this, player) > 250*250)
                {
                    GoToEntity(player, 3);
                }
                else
                {
                    if (cooldown <= 0)
                    {
                        EntityManager.Add(new PulseShot(position, Engine.ToUnitVector(angle) * 8, angle, 0, false, damage));
                        SoundManager.PlaySound(Assets.SoundFX["Fire_1"], position);
                        cooldown = 1;
                    }
                }
                RotateTowards(targetAngle);
                LowerCooldown();

                if (health <= 0)
                {
                    isExpired = true;
                    SoundManager.PlaySound(Assets.SoundFX["Death"], position);
                    if (EntityManager.RandomWithKarma(8))
                    {
                        EntityManager.Add(ItemFactory.NewScrap(position, Engine.ToUnitVector(angle) * 5, angularVelocity));
                    }
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
            enemyRange.radius = 500;
            while (true)
            {
                enemyRange.position = position;
                targetVector = player.position - position + (player.velocity - velocity);
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                if (EntityManager.DistanceSqr(this, player) > 500*500)
                {
                    GoToEntity(player, 1);
                }
                else if (EntityManager.DistanceSqr(this, player) < MathF.Pow(75, 2))
                {
                    GoToEntity(player, -1);
                    if (cooldown <= 0)
                    {
                        EntityManager.Add(new PulseShot(position, Engine.ToUnitVector(angle) * 8, angle, 0, false, damage));
                        SoundManager.PlaySound(Assets.SoundFX["Fire_1"], position);
                        cooldown = 0.25f;
                    }
                }
                else
                {
                    if (cooldown <= 0)
                    {
                        NewMissile(position, Engine.ToUnitVector(angle) * 2, angle, 0);
                        SoundManager.PlaySound(Assets.SoundFX["Fire_2"], position);
                        cooldown = 5;
                    }
                }
                RotateTowards(targetAngle);
                LowerCooldown();

                if (health <= 0)
                {
                    isExpired = true;
                    SoundManager.PlaySound(Assets.SoundFX["Death"], position);
                    if(EntityManager.RandomWithKarma(3))
                    {
                        EntityManager.Add(ItemFactory.NewScrap(position, Engine.ToUnitVector(angle) * 5, angularVelocity));
                    }
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
            enemyRange.radius = 400;
            while (true)
            {
                enemyRange.position = position;
                float timeToHit;
                float prevTimeToHit = 0;
                Vector2 playerIterativePosition = player.position;
                for (int i = 0; i < 1; i++)
                {
                    timeToHit = MathF.Sqrt(EntityManager.DistanceSqr(position, playerIterativePosition)) / 12;
                    playerIterativePosition += player.velocity * (timeToHit - prevTimeToHit);
                    prevTimeToHit = timeToHit;
                }
                targetVector = (playerIterativePosition - position);
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                if (EntityManager.DistanceSqr(this, player) > 400*400)
                {
                    GoToEntity(player, 1);
                }
                else
                {
                    if (cooldown <= 0 && MathF.Abs(targetAngle - angle) < 0.1f)
                    {
                        EntityManager.Add(new PulseShot(position, Engine.ToUnitVector(angle) * 12, angle, 0, false, damage));
                        SoundManager.PlaySound(Assets.SoundFX["Fire_3"], position);
                        cooldown = 2.5f;
                    }
                }
                RotateTowards(targetAngle);
                LowerCooldown();

                if (health <= 0)
                {
                    isExpired = true;
                    SoundManager.PlaySound(Assets.SoundFX["Death"], position);
                    if(EntityManager.RandomWithKarma(3))
                    {
                        EntityManager.Add(ItemFactory.NewScrap(position, Engine.ToUnitVector(angle) * 5, angularVelocity));
                    }
                }
                if (health > maxHealth)
                {
                    health = maxHealth;
                }

                yield return 0;
            }
        }
        IEnumerable<int> Missile()
        {
            enemyRange.radius = 20;
            while (true)
            {
                enemyRange.position = position;
                targetVector = player.position - position;
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                RotateTowards(targetAngle);
                velocity += Vector2.Normalize(new Vector2(MathF.Sin(angle), -MathF.Cos(angle)));

                if (EntityManager.DistanceSqr(this, player) < 20 * 20)
                {
                    player.Collide(10);
                    SoundManager.PlaySound(Assets.SoundFX["Explosion"], position);
                    isExpired = true;
                }
                if (health <= 0)
                {
                    isExpired = true;
                    SoundManager.PlaySound(Assets.SoundFX["Death"], position);
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
            Enemy enemy = new(position, velocity, angle, angularVelocity, 5, 10, Assets.Sprites["Fighter"]);
            enemy.AddBehaviour(enemy.Fighter());
            return enemy;
        }
        public static Enemy NewCarrier(Vector2 position, Vector2 velocity, float angle, float angularVelocity)
        {
            Enemy enemy = new(position, velocity, angle, angularVelocity, 10, 20, Assets.Sprites["Cruiser"]);
            enemy.AddBehaviour(enemy.Carrier());
            return enemy;
        }
        public static Enemy NewSniper(Vector2 position, Vector2 velocity, float angle, float angularVelocity)
        {
            Enemy enemy = new(position, velocity, angle, angularVelocity, 8, 10, Assets.Sprites["Sniper"]);
            enemy.AddBehaviour(enemy.Sniper());
            return enemy;
        }
        public static Enemy NewMissile(Vector2 position, Vector2 velocity, float angle, float angularVelocity)
        {
            Enemy enemy = new(position, velocity, angle, angularVelocity, 8, 10, Assets.Sprites["Missile"]);
            enemy.AddBehaviour(enemy.Missile());
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
                {
                    behaviours.RemoveAt(i--);
                }
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

            if(Engine.debugMode == true)
            {
                enemyRange.isEmitterActive = true;
            }
            else
            {
                enemyRange.isEmitterActive = false;
            }

            ApplyBehaviours();
        }
        public override void Draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(texture, position + Engine.screenPosition - Engine.mousePositionOffset, null, color, angle, Size / 2, 1, 0, 0.2f);
            //Health bar
            _spriteBatch.Draw(Engine.line, position + Engine.screenPosition - Engine.mousePositionOffset + new Vector2(-texture.Width*2, texture.Height)/2, new Rectangle(0, 0, texture.Width*2, 2),
                Color.Red, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            _spriteBatch.Draw(Engine.line, position + Engine.screenPosition - Engine.mousePositionOffset + new Vector2(-texture.Width*2, texture.Height)/2, new Rectangle(0, 0, texture.Width*2 * health / maxHealth, 2),
                Color.Green, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

            if (Engine.debugMode == true)
            {
                //Draws a line in the direction of motion for X
                _spriteBatch.Draw(Engine.line, position + Engine.screenPosition - Engine.mousePositionOffset, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.White,
                    MathF.Atan2(0, velocity.X), Vector2.Zero, new Vector2(MathF.Abs(velocity.X), 1), SpriteEffects.None, 0.4f);
                //Draws a line in the direction of motion for Y
                _spriteBatch.Draw(Engine.line, position + Engine.screenPosition - Engine.mousePositionOffset, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.White,
                    MathF.Atan2(velocity.Y, 0), Vector2.Zero, new Vector2(MathF.Abs(velocity.Y), 1), SpriteEffects.None, 0.4f);
                //Draws a line in the direction the entity is pointing
                _spriteBatch.Draw(Engine.line, position + Engine.screenPosition - Engine.mousePositionOffset, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.Red,
                    angle - MathF.PI / 2, Vector2.Zero, Vector2.Zero, SpriteEffects.None, 0.4f);
            }
        }

        public override void Collide(int damage)
        {
            health -= damage;
            if (damage > 0)
            {
                SoundManager.PlaySound(Assets.SoundFX["Hit"], position);
            }
        }
    }
}
