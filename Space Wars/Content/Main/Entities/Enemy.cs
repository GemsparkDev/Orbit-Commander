using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Particles;
using Space_Wars.Content.Main.UI_Elements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;

namespace Space_Wars.Content.Main.Entities
{
    public class Enemy : Entity
    {
        private List<IEnumerator<int>> behaviours = new();
        public Player player = EntityManager.player;
        private Random random = new();
        public float targetAngle = 0;
        public float cooldown;
        public int health;
        public int maxHealth;
        private bool deleteOnCollide = false;
        private bool childEnemy = false;
        public Vector2 targetVelocity = Vector2.Zero;
        public Vector2 targetVector;
        public ParticleEmitter enemyRange = new(Assets.Get(Sprite.Dot), Vector2.Zero, 0, 1, Color.Red);
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
            hitSound = Assets.Get(Sound.Hit);
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
            hitSound = Assets.Get(Sound.Hit);
        }
        public override void Update()
        {
            if (Engine.debugMode == true)
            {
                enemyRange.isEmitterActive = true;
            }
            else
            {
                enemyRange.isEmitterActive = false;
            }

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
            Vector2 gravityForce = EntityManager.GetNormalizedAcceleration(position);
            targetVector = Vector2.Normalize(Vector2.Normalize(entity.position - position) + gravityForce * 1.25f);
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
        public override void Collide(int damage)
        {
            if (deleteOnCollide == true)
            {
                health = 0;
                SoundManager.PlaySound(hitSound, position);
                return;
            }
            if (damage > 0)
            {
                SoundManager.PlaySound(hitSound, position);
                health -= damage;
            }
        }
        private void Explode()
        {
            int particles = random.Next(15, 25);
            for (int i = 0; i < particles; i++)
            {
                float angle = (float)random.NextDouble() * MathF.PI * 2;
                Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (float)(random.NextDouble() * 2 + 2);
                ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), 0.25f, position, particleVelocity + velocity, angle, 0, 1, true, Color.Yellow, Color.Red));
            }
            particles = random.Next(8, 16);
            for (int i = 0; i < particles; i++)
            {
                float angle = (float)random.NextDouble() * MathF.PI * 2;
                Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (float)(random.NextDouble() * 2 + 2);
                ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.25f, position, particleVelocity + velocity, angle, 0, 1, true, Color.DarkSlateGray, Color.Black));
            }
        }
        public override void Draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(texture, position - Engine.mousePositionOffset, null, color, angle, Size / 2, 1, 0, 0.2f);
            if(childEnemy == false)
            {
                //Health bar
                _spriteBatch.Draw(Engine.line, position - Engine.mousePositionOffset + new Vector2(-texture.Width * 2, texture.Height) / 2, new Rectangle(0, 0, texture.Width * 2, 2),
                    Color.Red, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                _spriteBatch.Draw(Engine.line, position - Engine.mousePositionOffset + new Vector2(-texture.Width * 2, texture.Height) / 2, new Rectangle(0, 0, texture.Width * 2 * health / maxHealth, 2),
                    Color.Green, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            }

            if (Engine.debugMode == true)
            {
                //Draws a line in the direction of motion for X
                _spriteBatch.Draw(Engine.line, position - Engine.mousePositionOffset, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.White,
                    MathF.Atan2(0, velocity.X), Vector2.Zero, new Vector2(MathF.Abs(velocity.X), 1), SpriteEffects.None, 0.4f);
                //Draws a line in the direction of motion for Y
                _spriteBatch.Draw(Engine.line, position - Engine.mousePositionOffset, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.White,
                    MathF.Atan2(velocity.Y, 0), Vector2.Zero, new Vector2(MathF.Abs(velocity.Y), 1), SpriteEffects.None, 0.4f);
                //Draws a line in the direction the entity is pointing
                _spriteBatch.Draw(Engine.line, position - Engine.mousePositionOffset, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.Red,
                    angle - MathF.PI / 2, Vector2.Zero, Vector2.Zero, SpriteEffects.None, 0.4f);
            }
        }
        IEnumerable<int> Fighter()
        {
            enemyRange.radius = 250;
            while (true)
            {
                enemyRange.position = position;
                targetVector = player.position - position + (player.velocity - velocity) * 8;
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                if (EntityManager.DistanceSqr(this, player) > 250 * 250)
                {
                    GoToEntity(player, 3);
                }
                else
                {
                    if (cooldown <= 0)
                    {
                        EntityManager.Add(new PulseShot(position, Engine.ToUnitVector(angle) * 8, angle, 0, false, damage, true));
                        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
                        cooldown = 1;
                    }
                }
                RotateTowards(targetAngle);
                LowerCooldown();

                if (health <= 0)
                {
                    Explode();
                    isExpired = true;
                    SoundManager.PlaySound(Assets.Get(Sound.Death), position);
                    if (EntityManager.RandomWithKarma(EnemySpawner.enemiesSpawned * 2))
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
                if (EntityManager.DistanceSqr(this, player) > 500 * 500)
                {
                    GoToEntity(player, 1.5f);
                }
                else if (EntityManager.DistanceSqr(this, player) < 75*75)
                {
                    GoToEntity(player, -1);
                    if (cooldown <= 0)
                    {
                        EntityManager.Add(new PulseShot(position, Engine.ToUnitVector(angle) * 8, angle, 0, false, damage));
                        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
                        cooldown = 0.25f;
                    }
                }
                else
                {
                    if (cooldown <= 0)
                    {
                        EntityManager.Add(NewMissile(position, Engine.ToUnitVector(angle) * 2, angle, 0));
                        SoundManager.PlaySound(Assets.Get(Sound.MissileFire), position);
                        cooldown = 5;
                    }
                }
                RotateTowards(targetAngle);
                LowerCooldown();

                if (health <= 0)
                {
                    Explode();
                    isExpired = true;
                    SoundManager.PlaySound(Assets.Get(Sound.Death), position);
                    if (EntityManager.RandomWithKarma(EnemySpawner.enemiesSpawned))
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
                if (EntityManager.DistanceSqr(this, player) > 400 * 400)
                {
                    GoToEntity(player, 2);
                }
                else
                {
                    if (cooldown <= 0 && MathF.Abs(targetAngle - angle) < 0.1f)
                    {
                        EntityManager.Add(new PulseShot(position, Engine.ToUnitVector(angle) * 12, angle, 0, false, damage));
                        SoundManager.PlaySound(Assets.Get(Sound.SniperFire), position);
                        cooldown = 2.5f;
                    }
                }
                RotateTowards(targetAngle);
                LowerCooldown();

                if (health <= 0)
                {
                    Explode();
                    isExpired = true;
                    SoundManager.PlaySound(Assets.Get(Sound.Death), position);
                    if (EntityManager.RandomWithKarma(EnemySpawner.enemiesSpawned))
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
            enemyRange.radius = 10;
            float fuel = 45;
            deleteOnCollide = true;
            ParticleEmitter engineParticles = new(Assets.Get(Sprite.Dot), 0.15f, Vector2.Zero, 0, 45, 2, 0, 450f, 1, true, Color.Yellow, Color.DarkRed, EmitterType.EmissionOverTime);
            ParticleManager.Add(engineParticles);
            while (true)
            {
                enemyRange.position = position;
                velocity /= 0.8f;
                targetVector = Vector2.Normalize(player.position - position);
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                Vector2 normalAcceleration = Vector2.Normalize(new Vector2(velocity.Y, -velocity.X));
                float closingVelocity = Vector2.Dot(targetVector, velocity);
                Vector2 futureTargetVector = (player.position + player.velocity) - (position + velocity);
                float angleRateOfChange = (targetAngle - MathF.Atan2(futureTargetVector.X, -futureTargetVector.Y)) / Engine.deltaSeconds;
                Vector2 accelerationVector = normalAcceleration * closingVelocity * angleRateOfChange * Engine.deltaSeconds * 2;
                Vector2 normalAccelerationVector = Vector2.Normalize(accelerationVector);
                if(accelerationVector.LengthSquared() > 0.75f)
                {
                    accelerationVector = normalAccelerationVector * 0.75f;
                }
                engineParticles.isEmitterActive = false;
                if (fuel > 0)
                {
                    velocity += accelerationVector;
                    if(MathF.Abs(angleRateOfChange) < 0.5f)
                    {
                        Vector2 thrustForce = targetVector * Engine.deltaSeconds * 8;
                        velocity += thrustForce;
                        accelerationVector += thrustForce;
                    }
                    float fuelUsage = accelerationVector.Length();
                    fuel -= fuelUsage;
                    angle = MathF.Atan2(accelerationVector.X, -accelerationVector.Y);

                    engineParticles.isEmitterActive = true;
                }
                engineParticles.sprayAngle = angle + MathF.PI;
                engineParticles.position = position + new Vector2(-MathF.Sin(angle), MathF.Cos(angle)) * 4;
                if (EntityManager.DistanceSqr(this, player) < 10 * 10)
                {
                    player.Collide(8);
                    SoundManager.PlaySound(Assets.Get(Sound.Explosion), position);
                    isExpired = true;
                    engineParticles.isEmitterExpired = true;
                }
                if (health <= 0)
                {
                    Explode();
                    isExpired = true;
                    SoundManager.PlaySound(Assets.Get(Sound.Death), position);
                    engineParticles.isEmitterExpired = true;
                }
                if (health > maxHealth)
                {
                    health = maxHealth;
                }

                yield return 0;
            }
        }
        IEnumerable<int> Shotgunner()
        {
            Random random = new();
            Enemy shield = NewShield(this, 3);
            EntityManager.Add(shield);
            enemyRange.radius = 200;
            while (true)
            {
                enemyRange.position = position;
                targetVector = player.position - position + (player.velocity - velocity) * 8;
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                if (EntityManager.DistanceSqr(this, player) > 200 * 200)
                {
                    GoToEntity(player, 3.5f);
                }
                else
                {
                    if (cooldown <= 0)
                    {
                        int randomBulletCount = random.Next(4, 6);
                        for (int i = 0; i < randomBulletCount; i++)
                        {
                            float angleDegrees = (float)(random.NextDouble() - 0.5) * 30;
                            float offsetAngle = angleDegrees * MathF.PI / 180;
                            Vector2 targetVector = Engine.ToUnitVector(angle + offsetAngle);
                            EntityManager.Add(new PulseShot(position, targetVector * 6, angle + offsetAngle, 0, false, damage, true));
                        }
                        SoundManager.PlaySound(Assets.Get(Sound.ShotgunFire), position);
                        cooldown = 1.2f;
                    }
                }
                if (angle > targetAngle && angularVelocity > -0.02f)
                {
                    angularVelocity -= 0.01f;
                }
                if (angle < targetAngle && angularVelocity < 0.02f)
                {
                    angularVelocity += 0.01f;
                }
                LowerCooldown();

                if (health <= 0)
                {
                    Explode();
                    isExpired = true;
                    SoundManager.PlaySound(Assets.Get(Sound.Death), position);
                    if (EntityManager.RandomWithKarma(EnemySpawner.enemiesSpawned * 2))
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
        IEnumerable<int> Shield(Enemy _parent, float distance, float theta)
        {
            Enemy parent = _parent;
            color = Color.Orange;
            childEnemy = true;
            hitSound = Assets.Get(Sound.ShieldHit);
            while (true)
            {
                angle = parent.angle + theta;
                position = parent.position + new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * distance;
                if(parent.isExpired == true)
                {
                    isExpired = true;
                    parent = null;
                }
                if (health <= 0)
                {
                    isExpired = true;
                    SoundManager.PlaySound(Assets.Get(Sound.Death), position);
                }
                yield return 0;
            }
        }
        IEnumerable<int> Symmetry()
        {
            enemyRange.radius = 500;
            float missileCooldown = 10;
            float missileCount = 0;
            float bulletCount = 0;
            while (true)
            {
                enemyRange.position = position;
                if (missileCooldown > 0)
                {
                    missileCooldown -= Engine.deltaSeconds;
                }
                float speed = (player.position - position).LengthSquared() / 50000 - Vector2.Dot(player.velocity, velocity) / 10;
                float timeToHit;
                float prevTimeToHit = 0;
                Vector2 playerIterativePosition = player.position;
                for (int i = 0; i < 5; i++)
                {
                    timeToHit = MathF.Sqrt(EntityManager.DistanceSqr(position, playerIterativePosition)) / (speed + 8);
                    playerIterativePosition += player.velocity * (timeToHit - prevTimeToHit);
                    prevTimeToHit = timeToHit;
                }
                targetVector = playerIterativePosition - position;
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                if (EntityManager.DistanceSqr(this, player) > 250 * 250)
                {
                    GoToEntity(player, speed);
                }
                else
                {
                    GoToEntity(player, -2);
                }
                if(EntityManager.DistanceSqr(this, player) < 500 * 500)
                {
                    velocity += EntityManager.GetNormalizedAcceleration(position) * Engine.deltaSeconds * 10;
                    if (cooldown <= 0 && missileCooldown > 0)
                    {
                        if (bulletCount < 2)
                        {
                            cooldown = 0.1f;
                            bulletCount += 1;
                        }
                        else
                        {
                            cooldown = (float)maxHealth / (maxHealth * 2 - health);
                            bulletCount = 0;
                        }
                        Vector2 direction = Engine.ToUnitVector(angle);
                        EntityManager.Add(new PulseShot(position, direction * speed + direction * 8, angle, 0, false, damage, true));
                        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
                    }
                    else if(missileCooldown < 0)
                    {
                        if (missileCount < 3)
                        {
                            cooldown += 0.25f;
                            missileCount += 1;
                            missileCooldown = 0.25f;
                        }
                        else
                        {
                            cooldown += 2f;
                            missileCount = 0;
                            missileCooldown = 10;
                        }
                        Vector2 direction = Engine.ToUnitVector(angle + MathF.PI / 2 + MathF.PI * missileCount);
                        EntityManager.Add(NewMissile(position, direction * 5 + velocity, angle, 0));
                        SoundManager.PlaySound(Assets.Get(Sound.MissileFire), position);
                    }
                }
                RotateTowards(targetAngle);
                LowerCooldown();

                if (health <= 0)
                {
                    Explode();
                    int particles = random.Next(3, 5);
                    for (int i = 0; i < particles; i++)
                    {
                        float angle = (float)random.NextDouble() * MathF.PI * 2;
                        Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (float)(random.NextDouble() * 2 + 2) / 2;
                        ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.5f, position, particleVelocity + velocity, angle, 0, 1, true, Color.Yellow, Color.Red));
                    }
                    particles = random.Next(3, 5);
                    for (int i = 0; i < particles; i++)
                    {
                        float angle = (float)random.NextDouble() * MathF.PI * 2;
                        Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (float)(random.NextDouble() * 2 + 2) / 2;
                        ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.5f, position, particleVelocity + velocity, angle, 0, 1, true, Color.DarkSlateGray, Color.Black));
                    }
                    isExpired = true;
                    SoundManager.PlaySound(Assets.Get(Sound.Death), position);
                    EntityManager.Add(ItemFactory.NewScrap(position, Engine.ToUnitVector(angle) * 5, angularVelocity));
                    EntityManager.Add(ItemFactory.NewScrap(position, Engine.ToUnitVector(angle + MathF.PI/6) * 5, angularVelocity));
                    EntityManager.Add(ItemFactory.NewScrap(position, Engine.ToUnitVector(angle - MathF.PI / 6) * 5, angularVelocity));
                }
                if (health > maxHealth)
                {
                    health = maxHealth;
                }

                yield return 0;
            }
        }
        IEnumerable<int> Overload()
        {
            enemyRange.radius = 10;
            float octoshotCooldown = 10;
            int octoshots = 0;
            float shieldCooldown = 7.5f;
            Enemy[] shields = new Enemy[4] {
                NewOverloadShield(this, 14, 0),
                NewOverloadShield(this, 14, MathF.PI),
                NewOverloadShield(this, 14, MathF.PI/2),
                NewOverloadShield(this, 14, MathF.PI*3/2)
            };
            foreach(Enemy shield in shields)
            {
                EntityManager.Add(shield);
            }
            while(true)
            {
                enemyRange.position = position;
                if (shields[0].isExpired && shields[1].isExpired && shields[2].isExpired && shields[3].isExpired)
                {
                    velocity /= 0.825f;
                    if (shieldCooldown > 0)
                    {
                        shieldCooldown -= Engine.deltaSeconds;
                    }
                    else
                    {
                        shieldCooldown = 7.5f;
                        for (int i = 0; i < 4; i++)
                        {
                            shields[i].isExpired = false;
                            shields[i].health = 25;
                            EntityManager.Add(shields[i]);
                        }
                    }
                }
                else if (octoshotCooldown < 0 && octoshots <= 15)
                {
                    velocity /= 0.8f;
                    if (MathF.Abs(angle - targetAngle) < 0.2f)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            float shotAngle = i * 2 * MathF.PI / 8 + targetAngle;
                            Vector2 shotDirection = new(MathF.Sin(shotAngle), -MathF.Cos(shotAngle));
                            EntityManager.Add(new PulseShot(position, velocity + shotDirection * 8, shotAngle, 0, false, 5));
                        }
                        Assets.Get(Sound.PulseFire).Play();
                        octoshots++;
                        if(octoshots >= 15)
                        {
                            octoshots = 0;
                            octoshotCooldown = 10;
                            targetAngle = 0;
                        }
                        targetAngle += MathF.PI/16;
                    }
                    else
                    {
                        RotateTowards(targetAngle);
                    }
                }
                else
                {
                    velocity /= 0.8f;
                    octoshotCooldown -= Engine.deltaSeconds;

                    targetVector = Vector2.Normalize(player.position - position);
                    targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                    Vector2 normalAcceleration = Vector2.Normalize(new Vector2(velocity.Y - 0.001f, -velocity.X));
                    float closingVelocity = Vector2.Dot(targetVector, velocity);
                    Vector2 futureTargetVector = (player.position + player.velocity) - (position + velocity);
                    float angleRateOfChange = (targetAngle - MathF.Atan2(futureTargetVector.X, -futureTargetVector.Y)) / Engine.deltaSeconds;
                    Vector2 accelerationVector = normalAcceleration * closingVelocity * angleRateOfChange * Engine.deltaSeconds * 2;
                    Vector2 normalAccelerationVector = Vector2.Normalize(accelerationVector);
                    if (accelerationVector.LengthSquared() > 0.75f)
                    {
                        accelerationVector = normalAccelerationVector * 0.75f;
                    }
                    velocity += accelerationVector + EntityManager.GetNormalizedAcceleration(position) / 5;
                    angularVelocity = velocity.Length() / 100;
                    Engine.WriteLine(Vector2.Dot(targetVector, velocity));
                    if (MathF.Abs(angleRateOfChange) < 0.5f && Vector2.Dot(targetVector, velocity) < 10)
                    {
                        Vector2 thrustForce = targetVector * 8;
                        velocity += thrustForce * Engine.deltaSeconds;
                    }
                    if (EntityManager.DistanceSqr(this, player) < 10 * 10)
                    {
                        player.Collide(damage);
                        velocity = player.velocity - velocity/2;
                    }
                }
                if(shieldCooldown == 7.5)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = (i * 2 * MathF.PI) / 6;
                        Particle particle = new(Assets.Get(Sprite.Dot), 0.08f, position - velocity, velocity + new Vector2(MathF.Cos(angle), MathF.Sin(angle)), angle, 0, 1, true, new Color(255, 0, 0), new Color(255, 0, 0));
                        ParticleManager.Add(particle);
                    }
                }
                if (health <= 0)
                {
                    Explode();
                    isExpired = true;
                    EntityManager.Add(ItemFactory.NewScrap(position, Engine.ToUnitVector(angle) * 5, angularVelocity));
                    EntityManager.Add(ItemFactory.NewScrap(position, Engine.ToUnitVector(angle + MathF.PI / 6) * 5, angularVelocity));
                    EntityManager.Add(ItemFactory.NewScrap(position, Engine.ToUnitVector(angle - MathF.PI / 6) * 5, angularVelocity));
                    SoundManager.PlaySound(Assets.Get(Sound.Death), position);
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
            Enemy enemy = new(position, velocity, angle, angularVelocity, 5, 8, Assets.Get(Sprite.Fighter));
            enemy.AddBehaviour(enemy.Fighter());
            return enemy;
        }
        public static Enemy NewCarrier(Vector2 position, Vector2 velocity, float angle, float angularVelocity)
        {
            Enemy enemy = new(position, velocity, angle, angularVelocity, 5, 15, Assets.Get(Sprite.Cruiser));
            enemy.AddBehaviour(enemy.Carrier());
            return enemy;
        }
        public static Enemy NewSniper(Vector2 position, Vector2 velocity, float angle, float angularVelocity)
        {
            Enemy enemy = new(position, velocity, angle, angularVelocity, 8, 5, Assets.Get(Sprite.Sniper));
            enemy.AddBehaviour(enemy.Sniper());
            return enemy;
        }
        public static Enemy NewMissile(Vector2 position, Vector2 velocity, float angle, float angularVelocity)
        {
            Enemy enemy = new(position, velocity, angle, angularVelocity, 8, 10, Assets.Get(Sprite.Missile));
            enemy.AddBehaviour(enemy.Missile());
            return enemy;
        }
        public static Enemy NewShotgunner(Vector2 position, Vector2 velocity, float angle, float angularVelocity)
        {
            Enemy enemy = new(position, velocity, angle, angularVelocity, 5, 10, Assets.Get(Sprite.Shotgunner));
            enemy.AddBehaviour(enemy.Shotgunner());
            return enemy;
        }
        public static Enemy NewShield(Enemy parent, float distance)
        {
            Enemy enemy = new(parent.position, parent.velocity, parent.angle, parent.angularVelocity, 0, 100, Assets.Get(Sprite.ShotgunShield));
            enemy.AddBehaviour(enemy.Shield(parent, distance, 0));
            return enemy;
        }
        public static Enemy NewSymmetryBoss(Vector2 position, Vector2 velocity, float angle, float angularVelocity)
        {
            Enemy enemy = new(position, velocity, angle, angularVelocity, 10, 250, Assets.Get(Sprite.SymmetryBoss));
            enemy.AddBehaviour(enemy.Symmetry());
            return enemy;
        }
        public static Enemy NewOverloadBoss(Vector2 position, Vector2 velocity, float angle, float angularVelocity)
        {
            Enemy enemy = new(position, velocity, angle, angularVelocity, 6, 400, Assets.Get(Sprite.OverloadBoss));
            enemy.AddBehaviour(enemy.Overload());
            return enemy;
        }
        public static Enemy NewOverloadShield(Enemy parent, float distance, float theta)
        {
            Enemy enemy = new(parent.position, parent.velocity, parent.angle, parent.angularVelocity, 0, 25, Assets.Get(Sprite.OverloadShield));
            enemy.AddBehaviour(enemy.Shield(parent, distance, theta));
            return enemy;
        }
    }
}
