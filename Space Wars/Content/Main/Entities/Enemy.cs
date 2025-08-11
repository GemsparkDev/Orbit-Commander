using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Space_Wars.Content.Main.Components;
using Space_Wars.Content.Main.Particles;
using System;
using System.Collections.Generic;
using UILib.Content.Main;

namespace Space_Wars.Content.Main.Entities;

public class Enemy : Entity
{
    private List<IEnumerator<int>> behaviours = [];
    private List<IEnumerable<int>> copyBehaviors = [];
    private float targetAngle = 0;
    private float cooldown;
    public int health;
    private int maxHealth;
    private bool deleteOnCollide = false;
    public bool ChildEnemy { get; private set; }
    private bool wasHit = false;
    public override int StealthAbility 
    { 
        get => base.StealthAbility + ((revealDuration > 0) ? -5 : 0); 
        protected set => base.StealthAbility = value; 
    }
    public override float ColliderRadius => ((texture != null) ? Engine.EnemyHitboxModifier * ((texture.Height + texture.Width) / 4 + 1) : 0);
    private Vector2 targetVector;
    public ParticleEmitter enemyRange = new(Assets.Get(Sprite.Dot), Vector2.Zero, 0, Color.Red * 0.75f);
    public Enemy(Vector2 _position, Vector2 _velocity, float _angle, int _damage, int _health, Texture2D _texture, bool _isFriendly = false)
        : base(_texture, _position, _velocity, _angle, 0, _damage, _isFriendly)
    {
        entityType = EntityType.Enemy;
        color = _isFriendly ? new Color(0, 255, 0) : Color.Red;
        health = _health;
        maxHealth = health;
        cooldown = 0.5f;
        enemyRange.position = position;
        hitSound = Assets.Get(Sound.Hit);
    }
    public override void Update()
    {
        enemyRange.isEmitterActive = Engine.DebugMode;
        enemyRange.position = position;
        enemyRange.Update();

        if (angle - targetAngle >= Math.PI)
        {
            angle -= MathF.PI * 2;
        }
        if (angle - targetAngle <= -Math.PI)
        {
            angle += MathF.PI * 2;
        }
        position += velocity * Engine.DeltaSeconds * 60;
        angle += angularVelocity * Engine.DeltaSeconds * 60;

        ApplyBehaviours();
        wasHit = false;
        if (health > maxHealth)
        {
            health = maxHealth;
        }
        base.Update();
    }
    public void LowerCooldown()
    {
        if (cooldown > 0)
        {
            cooldown -= Engine.DeltaSeconds;
        }
    }
    private Vector2 GetNormalizedAcceleration()
    {
        return Engine.SaveGame.CurrentMission.GetNormalizedAcceleration(position);
    }
    public void RotateTowards(float _angle, float _maxSpeed = 0.05f)
    {
        if (angle > _angle && angularVelocity > -_maxSpeed)
        {
            angle -= _maxSpeed;
        }
        if (angle < _angle && angularVelocity < _maxSpeed)
        {
            angle += _maxSpeed;
        }
    }
    public void GoToPosition(Vector2 _position, float speed)
    {
        targetVector = Vector2.Normalize(Vector2.Normalize(_position - position) + GetNormalizedAcceleration() * 10f);
        velocity += targetVector * speed * Engine.DeltaSeconds * 10;
    }

    private void AddBehaviour(IEnumerable<int> behaviour)
    {
        copyBehaviors.Add(behaviour);
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
        if (deleteOnCollide && damage >= 0)
        {
            wasHit = true; ;
            health = 0;
            SoundManager.PlaySound(hitSound, position);
            return;
        }
        if (damage > 0)
        {
            wasHit = true;
            SoundManager.PlaySound(hitSound, position);
            health -= damage;
            Engine.ShakeScreen(10 / ((position - Engine.Camera.Position).Length() + 200) * damage);
            ParticleManager.Add(new Particle(null, 1, position + new Vector2(0,-1), new Vector2(0,-1.5f), 0, 0, Color.Orange, new Color(255, 0, 0, 0)) { drawText = $"{damage}" });
            revealDuration = Math.Max(revealDuration, 0.3f * MathF.Sqrt(damage));
        }
        else if (damage < 0)
        {
            SoundManager.PlaySound(Assets.Get(Sound.Full), position);
            health -= damage;
            ParticleManager.Add(new Particle(null, 1, position + new Vector2(0, -1), new Vector2(0, -1.5f), 0, 0, Color.Orange, new Color(0, 255, 0, 0)) { drawText = $"{-damage}" });
        }
    }
    public void Explode(int _damage, float _radius)
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
        Engine.EntityManager.Explode(_damage, _radius, position);
        Engine.ShakeScreen(150 / ((position - Engine.Camera.Position).Length()+300));
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        if(!ChildEnemy)
        {
            float val = (Engine.SaveGame.Player.SensingAbility > base.StealthAbility ? 1 : 0);
            if (Engine.SaveGame.Player.SensingAbility <= base.StealthAbility)
            {
                val = Math.Clamp(val + revealDuration, 0, 1);
            }
            //Health bar
            Vector2 barPosition = position + new Vector2(-texture.Width * 2, texture.Height) / 2;
            Rectangle sourceRectangle = new (0, 0, texture.Width * 2, 2);
            Engine.DrawFilledLine(_spriteBatch, barPosition, sourceRectangle, (float)(health) / (float)(maxHealth), new Color(0, 50, 25) * val, Color.Green * val);
        }
        Vector2 halfSize = Engine.BackBuffer / 2;
        if (!ChildEnemy && !deleteOnCollide && isFriendly &&
           (position.X - Engine.Camera.Position.X + Size.X / 2 < -halfSize.X || position.Y - Engine.Camera.Position.Y + Size.Y / 2 < -halfSize.Y
         || position.X - Engine.Camera.Position.X - Size.X / 2 >  halfSize.X || position.Y - Engine.Camera.Position.Y - Size.Y / 2 >  halfSize.Y))
        {
            var pos = position - Engine.SaveGame.Player.position;
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), pos / 50 + Engine.SaveGame.Player.position, 0, color * 0.75f));
        }
        base.Draw(_spriteBatch);
    }
    public override Entity Clone()
    {
        Enemy enemy = new(position, velocity, angle, damage, maxHealth, texture, isFriendly);
        foreach (var behavior in copyBehaviors)
        {
            enemy.AddBehaviour(behavior);
        }
        return enemy;
    }
    #region Useful Behaviors
    IEnumerable<int> EnemyDeath(float _rarity)
    {
        while (true)
        {
            if (health <= 0)
            {
                Explode(4, ColliderRadius);
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), position);
                if (EntityManager.RandomWithKarma(Engine.SaveGame.CurrentMission.EnemiesSpawned * _rarity))
                {
                    Engine.EntityManager.Add(ItemFactory.NewScrap(position, GetNormalizedAcceleration() * 10, angularVelocity));
                }
            }
            yield return 0;
        }
    }
    IEnumerable<int> AvoidNearbyAllies()
    {
        while (true)
        {
            Entity nearestAlly = Engine.EntityManager.NearestAlly(this);
            if (nearestAlly != null)
            {
                if ((position - nearestAlly.position).LengthSquared() < 0.001f)
                {
                    position += new Vector2(0, 0.1f);
                }
                Vector2 relativePosition = nearestAlly.position - position;
                position -= Size.Length() * Vector2.Normalize(relativePosition) / (MathF.Sqrt(relativePosition.Length())) / 10;
            }
            yield return 0;
        }
    }
#endregion
    #region Bosses
    IEnumerable<int> Symmetry()
    {
        enemyRange.particleVelocity = 500;
        float missileCooldown = 10;
        float missileCount = 0;
        float bulletCount = 0;
        bool hasLaunchedAllies = false;
        while (true)
        {
            if (missileCooldown > 0)
            {
                missileCooldown -= Engine.DeltaSeconds;
            }
            float speed = (Player.position - position).Length() / 75;
            float timeToHit;
            float prevTimeToHit = 0;
            Vector2 playerIterativePosition = Player.position;
            for (int i = 0; i < 5; i++)
            {
                timeToHit = MathF.Sqrt(EntityManager.DistanceSqr(position, playerIterativePosition)) / (8);
                playerIterativePosition += (Player.velocity - velocity) * (timeToHit - prevTimeToHit);
                prevTimeToHit = timeToHit;
            }
            targetVector = playerIterativePosition - position;
            targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
            Vector2 gravityForce = GetNormalizedAcceleration();
            float theta = MathF.Atan2(gravityForce.Y, gravityForce.X);

            GoToPosition(Player.position + 100 * new Vector2(MathF.Cos(theta), MathF.Sin(theta)), speed);
            if (!hasLaunchedAllies && ((float)health / (float)maxHealth < 0.5f))
            {
                hasLaunchedAllies = !hasLaunchedAllies;
                Engine.EntityManager.Add(NewShield(this, 12, 25, 0, 0));
                SoundManager.PlaySound(Assets.Get(Sound.MissileFire), position);
            }
            if (EntityManager.DistanceSqr(this, Player) < 500 * 500)
            {
                velocity += gravityForce * Engine.DeltaSeconds * 60;
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
                    Engine.EntityManager.Add(new PulseShot(position, velocity + direction * 8, angle, 0, false, damage, true));
                    SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
                }
                else if (missileCooldown < 0)
                {
                    if (missileCount < 3 - (int)(3 * health / maxHealth))
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
                    Engine.EntityManager.Add(NewMissile(position, direction * 5 + velocity, angle, isFriendly));
                    SoundManager.PlaySound(Assets.Get(Sound.MissileFire), position);
                }
            }
            RotateTowards(targetAngle);
            LowerCooldown();

            if (health <= 0)
            {
                Explode(6, ColliderRadius);
                int particles = Engine.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Engine.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Engine.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.5f, position, particleVelocity + velocity, angle, 0, Color.Yellow, new Color(255, 0, 0, 0)));
                }
                particles = Engine.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Engine.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Engine.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.5f, position, particleVelocity + velocity, angle, 0, Color.DarkSlateGray, Color.Transparent));
                }
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), position);
                if (Engine.SaveGame.GiveWeapon)
                {
                    Engine.EntityManager.Add(new Module(Modules.Missile, position, GetNormalizedAcceleration() * 10, angularVelocity));
                }
                else
                {
                    Engine.EntityManager.Add(new Module(Modules.SummonShield, position, GetNormalizedAcceleration() * 10, angularVelocity));
                }
            }
            velocity *= 0.8f;
            yield return 0;
        }
    }
    IEnumerable<int> Overload()
    {
        enemyRange.particleVelocity = 10;
        float octoshotCooldown = 10;
        int octoshots = 0;
        float maxShieldCooldown = 3.33f;
        float chargeCooldown = 5;
        float chargingWindup = 1;
        float shieldCooldown = maxShieldCooldown;
        Vector2 chargeLocation = Vector2.Zero;
        Enemy[] shields =
        [
            NewShield(this, 14, 8, 0, 1),
            NewShield(this, 14, 8, MathF.PI, 1),
            NewShield(this, 14, 8, MathF.PI/2, 1),
            NewShield(this, 14, 8, MathF.PI*3/2, 1)
        ];
        foreach (Enemy shield in shields)
        {
            Engine.EntityManager.Add(shield);
        }
        while (true)
        {
            if (shields[0].isExpired && shields[1].isExpired && shields[2].isExpired && shields[3].isExpired)
            {
                if (chargeCooldown < 5)
                {
                    chargeCooldown = 15;
                }
                velocity *= 0.985f;
                if (shieldCooldown > 0)
                {
                    shieldCooldown -= Engine.DeltaSeconds;
                }
                else
                {
                    shieldCooldown = maxShieldCooldown;
                    for (int i = 0; i < 4; i++)
                    {
                        shields[i].isExpired = false;
                        shields[i].health = shields[i].maxHealth;
                        shields[i].position = position;
                        Engine.EntityManager.Add(shields[i]);
                    }
                }
            }
            else if (octoshotCooldown < 0 && octoshots <= 16)
            {
                velocity *= 0.9f;
                float speed = velocity.Length();
                if (speed < 0.5f)
                {
                    velocity *= 0.75f;
                }
                if (speed < 0.5f)
                {
                    velocity *= 0.25f;
                }
                if (speed < 0.05f)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 shotDirection = new(MathF.Sin(targetAngle), -MathF.Cos(targetAngle));
                        Engine.EntityManager.Add(new PulseShot(position, velocity + shotDirection * 8, targetAngle, 0, false, 5));
                        //EntityManager.Add(NewMissile(position, velocity + shotDirection * 8, shotAngle, 0, false));
                        //Golden Ratio
                        targetAngle += 1.6180339887f;
                    }
                    Assets.Get(Sound.PulseFire).Play();
                    octoshots++;
                    if (octoshots > 16)
                    {
                        octoshots = 0;
                        octoshotCooldown = 15;
                        targetAngle = 0;
                    }
                }
                RotateTowards(targetAngle);
            }
            else if (chargeCooldown < 0)
            {
                velocity *= 0.9f;
                Vector2 gravityForce = GetNormalizedAcceleration();
                float theta = MathF.Atan2(gravityForce.X, -gravityForce.Y);
                if (chargingWindup == 1)
                {
                    chargeLocation = Player.position + 50 * new Vector2(MathF.Cos(theta), MathF.Sin(theta));
                }
                GoToPosition(chargeLocation, 1 + 2 * MathF.Sqrt((chargeLocation - position).Length()));
                if ((chargeLocation - position).LengthSquared() < 200)
                {
                    if (chargingWindup <= 0)
                    {
                        Vector2 relativePosition = position - Player.position;
                        velocity -= Vector2.Normalize(relativePosition) * 15;
                        chargeCooldown = 15;
                        chargingWindup = 1;
                    }
                    else
                    {
                        float sum = 2 - chargingWindup;
                        angle = sum * sum * sum * sum;
                        chargingWindup -= Engine.DeltaSeconds;
                    }
                }
            }
            else
            {
                octoshotCooldown -= Engine.DeltaSeconds;
                chargeCooldown -= Engine.DeltaSeconds;

                targetVector = Vector2.Normalize(Player.position - position);
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                var normalAcceleration = Vector2.Normalize(new Vector2(velocity.Y - 0.001f, -velocity.X));
                float closingVelocity = Vector2.Dot(targetVector, velocity);
                var futureTargetVector = (Player.position + Player.velocity) - (position + velocity);
                float angleRateOfChange = (targetAngle - MathF.Atan2(futureTargetVector.X, -futureTargetVector.Y)) / Engine.DeltaSeconds;
                var accelerationVector = normalAcceleration * closingVelocity * angleRateOfChange * Engine.DeltaSeconds * 2;
                var normalAccelerationVector = Vector2.Normalize(accelerationVector);
                if (accelerationVector.LengthSquared() > 0.75f)
                {
                    accelerationVector = normalAccelerationVector * 0.75f;
                }
                velocity += accelerationVector + GetNormalizedAcceleration() * 2;
                angularVelocity = velocity.Length() / 100;
                if (MathF.Abs(angleRateOfChange) < 0.5f && Vector2.Dot(targetVector, velocity) < 10)
                {
                    Vector2 thrustForce = targetVector * 8;
                    velocity += thrustForce * Engine.DeltaSeconds;
                }
                if (EntityManager.DistanceSqr(this, Player) < 10 * 10)
                {
                    Player.Collide(damage);
                    velocity = Player.velocity - velocity / 2;
                }
            }
            if (shieldCooldown >= maxShieldCooldown)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = (i * 2 * MathF.PI) / 6;
                    Particle particle = new(Assets.Get(Sprite.Dot), 0.08f, position - velocity, velocity + new Vector2(MathF.Cos(angle), MathF.Sin(angle)), angle, 0, new Color(255, 0, 0), Color.Transparent);
                    ParticleManager.Add(particle);
                }
            }
            if (health <= 0)
            {
                Explode(6, ColliderRadius);
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), position);
                if (Engine.SaveGame.GiveWeapon)
                {
                    Engine.EntityManager.Add(new Module(Modules.Shotgun, position, GetNormalizedAcceleration() * 10, angularVelocity));
                }
                else
                {
                    Engine.EntityManager.Add(new Module(Modules.Shield, position, GetNormalizedAcceleration() * 10, angularVelocity));
                }
            }
            yield return 0;
        }
    }
    IEnumerable<int> Wyvern(Enemy _parent)
    {
        enemyRange.particleVelocity = 500;
        Enemy parent = _parent;
        Enemy tail1 = null;
        Enemy tail2 = null;
        bool hasExploded = false;
        if (_parent == null)
        {
            tail1 = new(position, velocity, angle, 15, 50, Assets.Get(Sprite.WyvernBoss), false);
            tail2 = new(position, velocity, angle, 4, 75, Assets.Get(Sprite.WyvernBoss), false);
            tail1.AddBehaviour(tail1.Wyvern(this));
            tail2.AddBehaviour(tail2.Wyvern(tail1));
            Engine.EntityManager.Add(tail1);
            Engine.EntityManager.Add(tail2);
        }
        while (true)
        {
            velocity *= 0.8f;
            ChildEnemy = !(parent == null);
            Vector2 normalizedAcceleration = GetNormalizedAcceleration();
            if (health <= 0)
            {
                if (wasHit && Engine.Random.Next(0, 10) == 0)
                {
                    for (float angle = 0; angle < MathF.Tau; angle += MathF.PI / 3)
                    {
                        Engine.EntityManager.Add(new AssassinShot(position, Engine.ToUnitVector(angle) * 8, angle, 0, isFriendly, 6, 1));
                    }
                }
                if (!hasExploded)
                {
                    Explode(6, ColliderRadius);
                    SoundManager.PlaySound(Assets.Get(Sound.Death), position);
                    if (tail1 == null && tail2 == null)
                    {
                        isExpired = true;
                    }
                    else
                    {
                        hasExploded = true;
                        position = new Vector2(10000, 10000);
                        if (Engine.SaveGame.GiveWeapon)
                        {
                            Engine.EntityManager.Add(new Module(Modules.LMG, position, normalizedAcceleration * 10, angularVelocity));
                        }
                        else
                        {
                            Engine.EntityManager.Add(new Module(Modules.Reflective, position, normalizedAcceleration * 10, angularVelocity));
                        }
                    }
                }
                else
                {
                    if (tail1 != null && tail2 != null)
                    {
                        isExpired = (tail1.isExpired && tail2.isExpired);
                    }
                }
            }
            if (parent != null)
            {
                if (parent.health <= 0)
                {
                    parent = null;
                }
                else
                {
                    Vector2 p1 = parent.position - new Vector2(MathF.Sin(parent.angle), -MathF.Cos(parent.angle)) * 5;
                    Vector2 relativep1 = p1 - position;
                    if (relativep1.LengthSquared() > 0.01f)
                    {
                        angle = MathF.Atan2(relativep1.Y, relativep1.X) + MathF.PI / 2;
                    }
                    position = p1 - new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * 5;
                }
            }
            else
            {
                if (!hasExploded)
                {
                    Entity nearestEnemy = Player;
                    float theta = MathF.Atan2((nearestEnemy.position - position).X, -(nearestEnemy.position - position).Y) - MathF.PI / 2;
                    targetVector = nearestEnemy.position - new Vector2(MathF.Cos(theta + MathF.PI / 8), MathF.Sin(theta + MathF.PI / 8)) * 200;
                    targetAngle = MathF.Atan2(velocity.X, -velocity.Y);
                    if (tail1 != null && tail2 != null)
                    {
                        if (tail1.isExpired)
                        {
                            theta = MathF.Atan2((nearestEnemy.position - tail2.position).X, -(nearestEnemy.position - tail2.position).Y) - MathF.PI / 2;
                            targetVector = nearestEnemy.position + new Vector2(MathF.Cos(theta + MathF.PI / 8), MathF.Sin(theta + MathF.PI / 8)) * 200;
                        }
                    }
                    angle = targetAngle;
                    LowerCooldown();
                    float speed = 5 + Math.Abs((nearestEnemy.velocity - velocity).Length() - 5) + (targetVector - position).LengthSquared() / 10000;
                    GoToPosition(targetVector, speed);
                    if (tail1 != null)
                    {
                        if (!tail1.isExpired && health < maxHealth / 2)
                        {
                            tail1.health = 0;
                            health = maxHealth;
                        }
                    }
                    if ((nearestEnemy.position - position).Length() < 500)
                    {
                        if (cooldown <= 0)
                        {
                            if ((tail1 == null && tail2 == null))
                            {
                                var direction = Vector2.Normalize(nearestEnemy.position - position);
                                Engine.EntityManager.Add(new PulseShot(position, velocity + new Vector2(direction.X, -direction.Y) * 2, theta, 0, isFriendly, damage, true) { texture = Assets.Get(Sprite.Microshot) });
                                Engine.EntityManager.Add(new PulseShot(position, velocity - new Vector2(direction.X, -direction.Y) * 2, theta + MathF.PI, 0, isFriendly, damage, true) { texture = Assets.Get(Sprite.Microshot) });
                                SoundManager.PlaySound(Assets.Get(Sound.LMGFire), position);
                                cooldown = 0.5f;
                            }
                            else
                            {
                                Engine.EntityManager.Add(new PulseShot(position, Vector2.Normalize(nearestEnemy.position - position) * 6 + nearestEnemy.velocity, angle, 0, isFriendly, damage, tail2.isExpired));
                                SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
                                cooldown = 0.75f;
                            }
                        }
                    }
                }
            }
            yield return 0;
        }
    }
    IEnumerable<int> Excursion()
    {
        enemyRange.particleVelocity = 500;
        int currentWave = 0;
        int currentEnemy = 0;
        float waveTimer = 0;
        Entity nearestPickup;
        int bulletOffset = 8;
        float laserCooldown = 0;
        float laserWindup = 3;
        Func<Vector2, Vector2, float, bool, Enemy>[][] waves =
        [
            [ NewFighter, NewFighter, ],
            [ NewCarrier, NewFighter, NewFighter, NewFighter, NewFighter ],
            [ NewSniper, NewSniper, NewSniper, NewFighter, NewFighter, ],
            [ NewSniper, NewSniper, NewFighter, NewFighter, NewShotgunner, NewCarrier, ],
        ];
        while (true)
        {
            nearestPickup = Engine.EntityManager.NearestItem(this, true);
            Vector2 relativePosition = Player.position + Player.velocity - position - velocity;
            Vector2 normalizedAcceleration = GetNormalizedAcceleration();
            targetAngle = MathF.Atan2(relativePosition.X, -relativePosition.Y);
            if (health >= maxHealth / 2 || laserCooldown > 0)
            {
                if (nearestPickup != null)
                {
                    GoToPosition(nearestPickup.position, 5);
                    if (Vector2.Distance(nearestPickup.position, position) < 100)
                    {
                        velocity -= normalizedAcceleration * 8 * 60 * Engine.DeltaSeconds;
                    }
                    if (EntityManager.DistanceSqr(this, nearestPickup) < 25 * 25)
                    {
                        nearestPickup.isExpired = true;
                        Collide(-15);
                    }
                }
                else
                {
                    Vector2 targetLocation = Player.position + Vector2.Normalize(Player.velocity) * 250 + Engine.SaveGame.CurrentMission.GetNormalizedAcceleration(Player.position) * 50;
                    float speed = 8 - 5 / ((targetLocation - position).Length() / 75 + 1);
                    GoToPosition(targetLocation, speed);
                }
            }
            if (laserCooldown > 0)
            {
                laserCooldown -= Engine.DeltaSeconds;
            }
            if (laserCooldown <= 0 && health < maxHealth / 2)
            {
                velocity *= 0.9f;
                float timeToHit;
                Vector2 playerIterativePosition = Player.position;
                float prevTimeToHit = 0;
                for (int i = 0; i < 1; i++)
                {
                    timeToHit = MathF.Sqrt(EntityManager.DistanceSqr(position, playerIterativePosition)) / 100;
                    playerIterativePosition += Player.velocity * (timeToHit - prevTimeToHit);
                    prevTimeToHit = timeToHit;
                }
                targetVector = (playerIterativePosition - position);
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                if (laserWindup > 0)
                {
                    if (laserWindup - Math.Truncate(laserWindup) < Engine.DeltaSeconds && laserWindup > 1)
                    {
                        Assets.Get(Sound.OpenMenu).Play();
                    }
                    laserWindup -= Engine.DeltaSeconds;
                }
                else
                {
                    Vector2 direction = Engine.ToUnitVector(angle);
                    Projectile shot = new AssassinShot(position, direction * 100, angle, 0, isFriendly, 15);
                    Engine.EntityManager.Add(shot);
                    SoundManager.PlaySound(Assets.Get(Sound.SniperFire), position);
                    velocity -= direction * 15;
                    laserWindup = 3;
                    laserCooldown = 10;
                }
            }
            else if (EntityManager.DistanceSqr(Player, this) < 300 * 300)
            {
                if (cooldown <= 0)
                {
                    Vector2 normalOffset = Engine.ToUnitVector(angle + MathF.PI / 2);
                    Vector2 offset = normalOffset * Engine.Random.Next(-2, 3);
                    Texture2D dot = Assets.Get(Sprite.Microshot);
                    Projectile shot = new AssassinShot(position + normalOffset * bulletOffset, Engine.ToUnitVector(angle) * 10 + offset / 4, angle, 0, isFriendly, 3)
                    {
                        texture = dot,
                        timeLeft = 3
                    };
                    Engine.EntityManager.Add(shot);
                    SoundManager.PlaySound(Assets.Get(Sound.LMGFire), position);
                    cooldown = 0.25f;
                    bulletOffset = -bulletOffset;
                }
                else
                {
                    cooldown -= Engine.DeltaSeconds;
                }
            }
            RotateTowards(targetAngle);
            LowerCooldown();
            if ((float)health / (float)maxHealth < 0.8f - currentWave * 0.2f && currentWave < waves.Length)
            {
                if (currentEnemy == 0)
                {
                    SoundManager.PlaySound(Assets.Get(Sound.Undock), position);
                }
                Func<Vector2, Vector2, float, bool, Entity>[] wave = waves[currentWave];
                waveTimer -= Engine.DeltaSeconds;
                if (waveTimer <= 0)
                {
                    if (currentEnemy >= wave.Length)
                    {
                        currentWave++;
                        currentEnemy = 0;
                        waveTimer = 0;

                    }
                    else
                    {
                        float spawnAngle = MathF.Tau * currentEnemy / wave.Length + angle;
                        Entity enemy = wave[currentEnemy](position + new Vector2(MathF.Cos(spawnAngle), MathF.Sin(spawnAngle)) * 40, velocity, angle, isFriendly);
                        Engine.EntityManager.Add(enemy);
                        waveTimer = 0.1f;
                        currentEnemy++;
                    }
                }
            }
            if (health <= 0)
            {
                Explode(6, ColliderRadius);
                int particles = Engine.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Engine.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Engine.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.5f, position, particleVelocity + velocity, angle, 0, Color.Yellow, new Color(255, 0, 0, 0)));
                }
                particles = Engine.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Engine.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Engine.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.5f, position, particleVelocity + velocity, angle, 0, Color.DarkSlateGray, Color.Transparent));
                }
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), position);
                if (Engine.SaveGame.GiveWeapon)
                {
                    Engine.EntityManager.Add(new Module(Modules.Sniper, position, normalizedAcceleration * 10, angularVelocity));
                }
                else
                {
                    Engine.EntityManager.Add(new Module(Modules.Nanomachines, position, normalizedAcceleration * 10, angularVelocity));
                }
            }
            velocity *= 0.8f;
            yield return 0;
        }
    }
    IEnumerable<int> Exodus()
    {
        enemyRange.particleVelocity = 250;
        float cooldown1 = 0;
        float cooldown2 = 2.5f;
        float missileGap = 0;
        var col = Color.DarkRed;
        col.A = 0;
        var engineParticles = new ParticleEmitter(Assets.Get(Sprite.Circle), 0.1f, Vector2.Zero, 0, MathF.PI/2, 2, 
            200f, Color.Yellow, EmitterType.EmissionOverTime) { particleFadeToColor = col };
        while (true)
        {
            targetVector = Vector2.Normalize(Player.position - position);
            targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
            var normalAcceleration = Vector2.Normalize(new Vector2(velocity.Y, -velocity.X));
            if (velocity.Length() <= 0.01f)
            {
                normalAcceleration = Vector2.Zero;
            }
            float closingVelocity = Vector2.Dot(targetVector, velocity);
            var relativePosition = Player.position - position;
            var relativeVelocity = Player.velocity - velocity;
            var futureTargetVector = relativePosition + relativeVelocity;
            var direction = Vector2.Normalize(relativePosition);
            Entity nearestEnemy = Engine.EntityManager.NearestEnemy(NewDummyEnemy(position - direction * 10, isFriendly));
            Entity nearestProjectile = Engine.EntityManager.NearestProjectile(NewDummyEnemy(position - direction * 10, isFriendly), isFriendly);
            float playerAngle = MathF.Atan2(relativePosition.Y, relativePosition.X) + MathF.PI / 2;
            if (missileGap <= 0 && MathF.Abs(playerAngle - angle) < 0.15f && relativePosition.Length() < 250)
            {
                bool fire = false;
                direction = Engine.ToUnitVector(angle);
                int sign = 0;
                if (cooldown1 <= 0)
                {
                    cooldown1 = 5;
                    fire = true;
                    sign = 1;
                }
                else if (cooldown2 <= 0)
                {
                    cooldown2 = 5;
                    fire = true;
                    sign = -1;
                }
                if (fire)
                {
                    missileGap = 0.5f;
                    Engine.EntityManager.Add(NewMissile(position + new Vector2(direction.Y, -direction.X) * 5 * sign, direction * 15 + velocity, angle, isFriendly));
                    SoundManager.PlaySound(Assets.Get(Sound.MissileFire), position);
                }
            }
            if (nearestProjectile != null)
            {
                var pos = Vector2.Normalize(position - nearestProjectile.position);
                var vel = Vector2.Normalize(velocity - nearestProjectile.velocity);
                if ((pos.X * vel.X + pos.Y * vel.Y) < -0.5f)
                {
                    int sign = Math.Sign(pos.X * vel.Y - vel.X * pos.Y);
                    if (sign == 0)
                    {
                        sign = 1;
                    }
                    targetAngle += MathF.PI / 2 * sign;
                }
            }
            if (nearestEnemy as Enemy != null && (nearestEnemy as Enemy).deleteOnCollide && (position - nearestEnemy.position).Length() < 250)
            {
                Vector2 playerIterativePosition = nearestEnemy.position;
                float timeToHit = MathF.Sqrt(EntityManager.DistanceSqr(position, playerIterativePosition)) / 20;
                playerIterativePosition += nearestEnemy.velocity * timeToHit;
                Vector2 targetVector = playerIterativePosition - position;
                float angle = MathF.Atan2(targetVector.X, -targetVector.Y);
                if (cooldown <= 0)
                {
                    Engine.EntityManager.Add(new PulseShot(position, Engine.ToUnitVector(angle) * 15, angle, 0, isFriendly, damage, true, 1) { texture = Assets.Get(Sprite.CrossbowShot) });
                    SoundManager.PlaySound(Assets.Get(Sound.LMGFire), position);
                    cooldown = 0.2f;
                }
            }
            float angleRateOfChange = (targetAngle - MathF.Atan2(futureTargetVector.X, -futureTargetVector.Y)) / Engine.DeltaSeconds;
            //Note: Add bonus to speed if moving very fast toward planet
            Vector2 accelerationVector = (normalAcceleration * closingVelocity * angleRateOfChange * 2 ) * Engine.DeltaSeconds + GetNormalizedAcceleration() * (2);
            if (accelerationVector.LengthSquared() > 1f)
            {
                accelerationVector = Vector2.Normalize(accelerationVector);
            }
            velocity += accelerationVector;
            if (MathF.Abs(angleRateOfChange) < 0.5f && relativeVelocity.X * direction.X + relativeVelocity.Y * direction.Y > -8f)
            {
                Vector2 thrustForce = targetVector * 12;
                velocity += thrustForce * Engine.DeltaSeconds;
                accelerationVector += thrustForce;
            }
            if (accelerationVector.LengthSquared() < 0.05f)
            {
                RotateTowards(MathF.Atan2(velocity.Y, velocity.X) + MathF.PI / 2, 0.2f);
            }
            else
            {
                RotateTowards(MathF.Atan2(accelerationVector.X, -accelerationVector.Y), 0.2f);
            }
            LowerCooldown();
            if (cooldown1 > 0)
            {
                cooldown1 -= Engine.DeltaSeconds;
            }
            if (cooldown2 > 0)
            {
                cooldown2 -= Engine.DeltaSeconds;
            }
            if (missileGap > 0)
            {
                missileGap -= Engine.DeltaSeconds;
            }
            if (health <= 0)
            {
                Explode(6, ColliderRadius);
                int particles = Engine.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Engine.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Engine.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.5f, position, particleVelocity + velocity, angle, 0, Color.Yellow, new Color(255, 0, 0, 0)));
                }
                particles = Engine.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Engine.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Engine.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.5f, position, particleVelocity + velocity, angle, 0, Color.DarkSlateGray, Color.Transparent));
                }
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), position);
                if (Engine.SaveGame.GiveWeapon)
                {
                    Engine.EntityManager.Add(new Module(Modules.Crossbow, position, GetNormalizedAcceleration() * 10, angularVelocity));
                }
                else
                {
                    Engine.EntityManager.Add(new Module(Modules.Plasma, position, GetNormalizedAcceleration() * 10, angularVelocity));
                }
            }
            engineParticles.sprayAngle = (angle + MathF.PI) * 180 / MathF.PI;
            engineParticles.speedOfEmission = accelerationVector.Length() * 50 + 200;
            engineParticles.offsetVelocity = velocity;
            engineParticles.Update();
            engineParticles.position = position + new Vector2(-MathF.Sin(angle), MathF.Cos(angle)) * 11;

            yield return 0;
        }
    }
    IEnumerable<int> VeilBoss()
    {
        StealthAbility = 2;
        SensingAbility = -1;
        float detectionCooldown = 0;
        enemyRange.particleVelocity = 450;
        Entity detectedEntity = null;
        while (true)
        {
            if (detectionCooldown <= 0)
            {
                detectionCooldown = 1;
                if (Engine.Random.Next(0, 2) == 0)
                {
                    detectedEntity = Engine.EntityManager.NearestEnemy(this);
                }
            }
            else
            {
                detectionCooldown -= Engine.DeltaSeconds;
            }
            if (detectedEntity != null)
            {
                velocity *= 0.8f;
                Vector2 relativePosition = detectedEntity.position - position;
                targetVector = Vector2.Normalize(relativePosition);
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                Entity nearestProjectile = Engine.EntityManager.NearestProjectile(NewDummyEnemy(position + targetVector * 10, isFriendly), isFriendly);
                if (nearestProjectile != null)
                {
                    var pos = Vector2.Normalize(position - nearestProjectile.position);
                    var vel = Vector2.Normalize(velocity - nearestProjectile.velocity);
                    if ((pos.X * vel.X + pos.Y * vel.Y) < -0.5f)
                    {
                        int sign = Math.Sign(pos.X * vel.Y - vel.X * pos.Y);
                        if (sign == 0)
                        {
                            sign = 1;
                        }
                        velocity += new Vector2(-pos.Y, pos.X) * sign * Engine.DeltaSeconds * 500;
                    }
                }
                Vector2 relativeVelocity = velocity - Player.velocity - targetVector * 4;
                GoToPosition(detectedEntity.position, (-Math.Min(0, relativeVelocity.X * targetVector.X + relativeVelocity.Y * targetVector.Y) * 1.75f));
                RotateTowards(targetAngle);
                if (cooldown <= 0)
                {
                    cooldown = 0.25f;
                    revealDuration += 0.2f;
                    Engine.EntityManager.Add(new Explosive(position, velocity + targetVector * 10 + new Vector2(Engine.OneToNegOne(), Engine.OneToNegOne()) * 5, Engine.OneToNegOne() * MathF.PI, Engine.OneToNegOne(), isFriendly, damage / 2, 40, 1));
                    SoundManager.PlaySound(Assets.Get(Sound.LMGFire), position);
                }
                if (relativePosition.Length() < 100)
                {
                    velocity -= targetVector * 180 * Engine.DeltaSeconds;
                }
            }
            else
            {
                var planet = Engine.SaveGame.CurrentMission.Planet;
                var relativePosition = position - planet.position;
                var dir = Vector2.Normalize(relativePosition);
                velocity -= Vector2.Normalize(dir * (relativePosition.Length() - planet.radius * 1.5f)) * Engine.DeltaSeconds * 4;
                Vector2 targetVelocity = new Vector2(dir.Y, -dir.X) * planet.GetOrbitalVelocity((dir * planet.radius * 1.5f));
                Vector2 diffVelocity = targetVelocity - velocity;
                velocity += diffVelocity * Engine.DeltaSeconds;
                targetAngle = MathF.Atan2(velocity.Y, velocity.X) + MathF.PI / 2;
                RotateTowards(targetAngle);
                if (cooldown <= 0)
                {
                    cooldown = 1;
                    revealDuration += 0.5f;
                    Engine.EntityManager.Add(new Explosive(position, velocity + GetNormalizedAcceleration() * 160, Engine.OneToNegOne() * MathF.PI, Engine.OneToNegOne(), isFriendly, damage, 200, 1) { timeLeft = 10 });
                    SoundManager.PlaySound(Assets.Get(Sound.LMGFire), position);
                }
            }
            LowerCooldown();
            if (health <= 0)
            {
                Explode(6, ColliderRadius);
                int particles = Engine.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Engine.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Engine.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.5f, position, particleVelocity + velocity, angle, 0, Color.Yellow, new Color(255, 0, 0, 0)));
                }
                particles = Engine.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Engine.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Engine.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.5f, position, particleVelocity + velocity, angle, 0, Color.DarkSlateGray, Color.Transparent));
                }
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), position);
                if (Engine.SaveGame.GiveWeapon)
                {
                    Engine.EntityManager.Add(new Module(Modules.GrenadeLauncher, position, GetNormalizedAcceleration() * 10, angularVelocity));
                }
                else
                {
                    Engine.EntityManager.Add(new Module(Modules.Stealth, position, GetNormalizedAcceleration() * 10, angularVelocity));
                }
            }
            Engine.WriteLine(position);
            yield return 0;
        }
    }
    IEnumerable<int> Inferno()
    {
        Enemy flare = NewFlareBoss(position - new Vector2(2000, 0), velocity, 0, this);
        Engine.EntityManager.Add(flare);
        cooldown = 1.5f;
        float rangeFactor = 1;
        float rotationSpeed = 0.03f;
        float sign = 1;
        float swapCooldown = 10;
        float time = 0;
        bool isDamaged = false;
        while (true)
        {
            LowerCooldown();
            if (swapCooldown > 0)
            {
                swapCooldown -= Engine.DeltaSeconds;
            }
            else
            {
                swapCooldown = 10;
                sign *= -1;
            }
            //If the first to die, reduce attack power
            //If the second to die, increase attack power
            if (isDamaged)
            {
                rangeFactor = 0.75f;
            }
            else if (flare.health <= 0)
            {
                rangeFactor = 1.25f;
                rotationSpeed = 0.05f;
            }
            enemyRange.particleVelocity = 400 * rangeFactor;
            time += Engine.DeltaSeconds;
            Vector2 normalizedAcceleration = GetNormalizedAcceleration();
            Vector2 relativeVelocity = Player.velocity - velocity;
            Vector2 relativePosition = Player.position - position;
            var playerAcceleration = Vector2.Normalize(Engine.SaveGame.CurrentMission.GetNormalizedAcceleration(Player.position));
            Vector2 relativeTargetPosition = relativePosition + new Vector2(playerAcceleration.Y, -playerAcceleration.X) * 100 * sign + normalizedAcceleration * 20;
            Vector2 playerIterativePosition = Player.position;
            float timeToHit = MathF.Sqrt(EntityManager.DistanceSqr(position, playerIterativePosition)) / 8;
            playerIterativePosition += relativeVelocity * timeToHit;
            targetVector = (playerIterativePosition - position);
            targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
            RotateTowards(targetAngle + MathF.Sin(time * 3) / 3, rotationSpeed);

            Vector2 velocityChange = (relativeVelocity + relativeTargetPosition / 12) - velocity;
            if (velocityChange.Length() > 60)
            {
                velocityChange = Vector2.Normalize(velocityChange) * 60;
            }
            velocity += velocityChange * Engine.DeltaSeconds;
            if (cooldown <= 0 && Vector2.Distance(Player.position, position) < 400 * rangeFactor)
            {
                Engine.EntityManager.Add(new FlameBolt(position, velocity + Engine.ToUnitVector(angle) * 12 * rangeFactor + new Vector2(Engine.OneToNegOne(), Engine.OneToNegOne()) / 4, false, damage, 0.3f * rangeFactor, 2f));
                SoundManager.PlaySound(Assets.Get(Sound.LMGFire), position);
                cooldown = 0.08f;
            }
            if (health <= 0 && !isDamaged)
            {
                isDamaged = true;
                SoundManager.PlaySound(Assets.Get(Sound.ShieldHit), position);
                Explode(0, 0);
            }
            if (health <= 0 && flare.health <= 0)
            {
                Explode(6, ColliderRadius);
                int particles = Engine.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Engine.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Engine.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.5f, position, particleVelocity + velocity, angle, 0, Color.Yellow, new Color(255, 0, 0, 0)));
                }
                particles = Engine.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Engine.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Engine.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.5f, position, particleVelocity + velocity, angle, 0, Color.DarkSlateGray, Color.Transparent));
                }
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), position);
                Engine.EntityManager.Add(new Module(Modules.GrenadeLauncher, position, GetNormalizedAcceleration() * 10, angularVelocity));
            }
            yield return 0;
        }
    }
    IEnumerable<int> Flare(Enemy _inferno)
    {
        enemyRange.particleVelocity = 1000;
        cooldown = 1.5f;
        float octoshot = 15;
        float shotCooldown = 0.75f;
        int shotCount = 0;
        bool isDamaged = false;
        while (true)
        {
            if (!isDamaged || Engine.Random.Next(0, 2) == 0)
            {
                LowerCooldown();
                if (octoshot > 0)
                {
                    octoshot -= Engine.DeltaSeconds;
                }
            }
            if (_inferno.health <= 0 && Engine.Random.Next(0, 2) == 0)
            {
                LowerCooldown();
                if (octoshot > 0)
                {
                    octoshot -= Engine.DeltaSeconds;
                }
            }
            Vector2 relativeVelocity = Player.velocity - velocity;
            Vector2 relativePosition = Player.position - position;
            Vector2 relativeTargetPosition = relativePosition + Vector2.Normalize(Engine.SaveGame.CurrentMission.GetNormalizedAcceleration(Player.position)) * 250;
            Vector2 playerIterativePosition = Player.position;
            float timeToHit = MathF.Sqrt(EntityManager.DistanceSqr(position, playerIterativePosition)) / 15;
            playerIterativePosition += relativeVelocity * timeToHit;
            targetVector = Vector2.Normalize(playerIterativePosition - position);
            targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
            RotateTowards(targetAngle);

            Vector2 velocityChange = (relativeVelocity + relativeTargetPosition / 15) - velocity;
            if (velocityChange.Length() > 60)
            {
                velocityChange = Vector2.Normalize(velocityChange) * 60;
            }
            velocity += velocityChange * Engine.DeltaSeconds;
            if (octoshot <= 0)
            {
                if (shotCooldown > 0)
                {
                    shotCooldown -= Engine.DeltaSeconds;
                }
                else
                {
                    if (shotCount < 8)
                    {
                        shotCooldown = 0.05f;
                        float speed = 4 * ((shotCount % 2 == 0) ? 1.5f : 1);
                        Engine.EntityManager.Add(new FlameBolt(position, velocity + Engine.ToUnitVector((float)(shotCount) * MathF.Tau / 8) * speed + new Vector2(Engine.OneToNegOne(), Engine.OneToNegOne()) / 2, false, damage, 30, 1f));
                        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), position);
                        shotCount++;
                    }
                    else
                    {
                        shotCount = 0;
                        octoshot = 15f;
                        cooldown = 0.2f;
                        shotCooldown = 0.75f;
                    }
                }
                float cd = (shotCount == 0) ? shotCooldown : 0;
                for (int i = 0; i < 8; i++)
                {
                    DrawLine((float)(i) * MathF.Tau / 8, cd + 0.1f, 0.85f);
                }
            }
            else
            {
                if (cooldown <= 0 && Vector2.Distance(Player.position, position) < 1000 && shotCount == 0)
                {
                    Engine.EntityManager.Add(new FlameBolt(position, velocity + targetVector * 15 + new Vector2(Engine.OneToNegOne(), Engine.OneToNegOne()) / 2, false, damage, 4, 1f));
                    SoundManager.PlaySound(Assets.Get(Sound.LMGFire), position);
                    cooldown = 0.8f;
                }
                DrawLine(angle, cooldown, 0.8f);
            }
            if (health <= 0 && !isDamaged)
            {
                isDamaged = true;
                SoundManager.PlaySound(Assets.Get(Sound.ShieldHit), position);
                Explode(0, 0);
            }
            if (health <= 0 && _inferno.health <= 0)
            {
                Explode(6, ColliderRadius);
                int particles = Engine.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Engine.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Engine.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.5f, position, particleVelocity + velocity, angle, 0, Color.Yellow, new Color(255, 0, 0, 0)));
                }
                particles = Engine.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Engine.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Engine.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.5f, position, particleVelocity + velocity, angle, 0, Color.DarkSlateGray, Color.Transparent));
                }
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), position);
                Engine.EntityManager.Add(new Module(Modules.Fireball, position, GetNormalizedAcceleration() * 10, angularVelocity));
            }
            void DrawLine(float _angle, float _cooldown, float _maxCooldown)
            {
                Vector2 dir = Engine.ToUnitVector(_angle);
                float cd = (1 - _cooldown / _maxCooldown) * (1 - _cooldown / _maxCooldown);
                for (int i = 0; i < 500; i++)
                {
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), position + dir * (i * 2 + 8), _angle, Color.Red * (1 - (float)(i) / 500f) * cd));
                }
            }
            yield return 0;
        }
    }
#endregion
    #region Enemies
    IEnumerable<int> Fighter()
    {
        enemyRange.particleVelocity = 250;
        float speed = 3;
        if (isFriendly)
        {
            speed = 7;
        }
        while (true)
        {
            velocity *= 0.8f;
            Vector2 normalizedAcceleration = GetNormalizedAcceleration();
            Entity nearestEnemy = Engine.EntityManager.NearestEnemy(this);
            if(nearestEnemy == null)
            {
                if((Player.position - position).LengthSquared() > 1000)
                {
                    GoToPosition(Player.position,(speed + (Player.position - position).Length() / 100));
                }
                targetVector = Player.position - position + (Player.velocity - velocity) * 8;
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                yield return 0;
                continue;
            }
            targetVector = nearestEnemy.position - position + (nearestEnemy.velocity - velocity) * 8;
            targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
            RotateTowards(targetAngle);
            LowerCooldown();
            if (EntityManager.DistanceSqr(this, nearestEnemy) > 250 * 250)
            {
                GoToPosition(nearestEnemy.position, speed);
            }
            else
            {
                velocity += normalizedAcceleration * Engine.DeltaSeconds * 60;
                if (cooldown <= 0)
                {
                    Engine.EntityManager.Add(new PulseShot(position, Engine.ToUnitVector(angle) * 8, angle, 0, isFriendly, damage, false));
                    SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
                    cooldown = 1;
                }
            }
            yield return 0;
        }
    }
    IEnumerable<int> Carrier()
    {
        enemyRange.particleVelocity = 500;
        while (true)
        {
            targetVector = Player.position - position + (Player.velocity - velocity);
            targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
            Vector2 gravityForce = GetNormalizedAcceleration();
            if (EntityManager.DistanceSqr(this, Player) > 500 * 500)
            {
                GoToPosition(Player.position, 1.5f);
            }
            else if (EntityManager.DistanceSqr(this, Player) < 75*75)
            {
                GoToPosition(Player.position, -1);
                if (cooldown <= 0)
                {
                    Engine.EntityManager.Add(new PulseShot(position, Engine.ToUnitVector(angle) * 8, angle, 0, false, damage));
                    SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
                    cooldown = 0.25f;
                }
            }
            else
            {
                velocity += gravityForce * Engine.DeltaSeconds * 60 * 2;
                if (cooldown <= 0)
                {
                    Engine.EntityManager.Add(NewMissile(position, Engine.ToUnitVector(angle) * 2, angle, isFriendly));
                    SoundManager.PlaySound(Assets.Get(Sound.MissileFire), position);
                    cooldown = 5;
                }
            }
            RotateTowards(targetAngle);
            LowerCooldown();
            velocity *= 0.8f;
            yield return 0;
        }
    }
    IEnumerable<int> Sniper()
    {
        enemyRange.particleVelocity = 400;
        while (true)
        {
            float timeToHit;
            float prevTimeToHit = 0;
            Vector2 playerIterativePosition = Player.position;
            Vector2 gravityForce = GetNormalizedAcceleration();
            for (int i = 0; i < 1; i++)
            {
                timeToHit = MathF.Sqrt(EntityManager.DistanceSqr(position, playerIterativePosition)) / 15;
                playerIterativePosition += Player.velocity * (timeToHit - prevTimeToHit);
                prevTimeToHit = timeToHit;
            }
            targetVector = (playerIterativePosition - position);
            targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
            if (EntityManager.DistanceSqr(this, Player) > 400 * 400)
            {
                GoToPosition(Player.position, 2);
            }
            else
            {
                velocity += gravityForce * Engine.DeltaSeconds * 60 * 2;
                if (cooldown <= 0 && MathF.Abs(targetAngle - angle) < 0.1f)
                {
                    Engine.EntityManager.Add(new AssassinShot(position, Engine.ToUnitVector(angle) * 15, angle, 0, false, damage));
                    SoundManager.PlaySound(Assets.Get(Sound.SniperFire), position);
                    cooldown = 2.5f;
                }
            }
            RotateTowards(targetAngle);
            LowerCooldown();
            velocity *= 0.8f;
            yield return 0;
        }
    }
    IEnumerable<int> Missile()
    {
        enemyRange.particleVelocity = 10;
        entityType = EntityType.Projectile;
        float fuel = 45;
        float deathCooldown = 2;
        deleteOnCollide = true;
        var col = Color.DarkRed;
        col.A = 0;
        ParticleEmitter engineParticles = new(Assets.Get(Sprite.Circle), 0.1f, Vector2.Zero, 0, MathF.PI/4, 2, 
            200f, Color.Yellow, EmitterType.EmissionOverTime) { isEmitterActive = false, particleFadeToColor = col };
        while (true)
        {
            if (fuel <= 0)
            {
                if (deathCooldown > 0)
                {
                    deathCooldown -= Engine.DeltaSeconds;
                }
                else
                {
                    health = 0;
                }
            }
            if (health <= 0)
            {
                Explode(8, 12);
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), position);
            }
            Entity nearestEnemy = Engine.EntityManager.NearestEnemy(this);
            nearestEnemy ??= NewDummyEnemy(position + 100 * new Vector2(MathF.Cos(angle- MathF.PI / 2), MathF.Sin(angle - MathF.PI/2)));

            targetVector = Vector2.Normalize(nearestEnemy.position - position);
            targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
            var normalAcceleration = Vector2.Normalize(new Vector2(velocity.Y, -velocity.X));
            if (velocity.Length() <= 0.01f)
            {
                normalAcceleration = Vector2.Zero;
            }
            float closingVelocity = Vector2.Dot(targetVector, velocity);
            Vector2 futureTargetVector = nearestEnemy.position + nearestEnemy.velocity - position - velocity;
            float angleRateOfChange = (targetAngle - MathF.Atan2(futureTargetVector.X, -futureTargetVector.Y)) / Engine.DeltaSeconds;
            Vector2 accelerationVector = normalAcceleration * closingVelocity * angleRateOfChange * Engine.DeltaSeconds * 2;
            if(accelerationVector.LengthSquared() > 0.75f)
            {
                accelerationVector = Vector2.Normalize(accelerationVector) * 0.75f;
            }
            engineParticles.isEmitterActive = false;
            if (fuel > 0)
            {
                velocity += accelerationVector;
                if(MathF.Abs(angleRateOfChange) < 0.5f)
                {
                    Vector2 thrustForce = targetVector * Engine.DeltaSeconds * 8;
                    velocity += thrustForce;
                    accelerationVector += thrustForce;
                }
                float fuelUsage = accelerationVector.Length();
                fuel -= fuelUsage;
                if (accelerationVector.LengthSquared() < 0.05f)
                {
                    angle = MathF.Atan2(velocity.Y, velocity.X) + MathF.PI/2;
                }
                else
                {
                    angle = MathF.Atan2(accelerationVector.X, -accelerationVector.Y);
                }

                engineParticles.isEmitterActive = true;
            }
            engineParticles.sprayAngle = (angle + MathF.PI) * 180 / MathF.PI;
            engineParticles.offsetVelocity = velocity;
            engineParticles.Update();
            engineParticles.position = position + new Vector2(-MathF.Sin(angle), MathF.Cos(angle)) * 7;
            if (EntityManager.DistanceSqr(this, nearestEnemy) < 10 * 10)
            {
                Explode(8, 12);
                SoundManager.PlaySound(Assets.Get(Sound.Explosion), position);
                isExpired = true;
            }

            yield return 0;
        }
    }
    IEnumerable<int> Shotgunner()
    {
        Random random = new();
        Enemy shield = NewShield(this, 3, 25, 0, 0);
        Engine.EntityManager.Add(shield);
        enemyRange.particleVelocity = 200;
        while (true)
        {
            targetVector = Player.position - position + (Player.velocity - velocity) * 8;
            targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
            if (EntityManager.DistanceSqr(this, Player) > 200 * 200)
            {
                GoToPosition(Player.position, 3.5f);
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
                        Engine.EntityManager.Add(new PulseShot(position, targetVector * 6, angle + offsetAngle, 0, false, damage, true));
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
            velocity *= 0.8f;
            yield return 0;
        }
    }
    IEnumerable<int> Shield(Entity parent, float distance, float theta)
    {
        color = Color.Orange;
        ChildEnemy = true;
        hitSound = Assets.Get(Sound.ShieldHit);
        while (true)
        {
            angle = parent.angle + theta;
            position = parent.position + new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * distance;
            velocity = parent.velocity;
            if(parent.isExpired)
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
    IEnumerable<int> Hovercraft()
    {
        float thrust;
        float cooldown = 1;
        int shots = 0;
        float weaponCooldown = 2;
        Vector2 randomPos = Vector2.Zero;
        enemyRange.particleVelocity = 250;
        var col = Color.DarkRed;
        col.A = 0;
        ParticleEmitter engineParticles = new(Assets.Get(Sprite.Circle), 0.15f, Vector2.Zero, 0, MathF.PI/4, 2, 200f, Color.Yellow, EmitterType.EmissionOverTime) { particleFadeToColor = col };
        while (true)
        {
            Entity nearestEnemy = Engine.EntityManager.NearestEnemy(this);
            Vector2 relativePosition = position - nearestEnemy.position;
            Vector2 normalizedAcceleration = GetNormalizedAcceleration() * 2;
            Vector2 Offset = Vector2.Normalize(Engine.SaveGame.CurrentMission.GetNormalizedAcceleration(nearestEnemy.position)) * 100;
            Vector2 targetLocation = relativePosition - Offset;
            Vector2 targetAcceleration;
            if (Vector2.Distance(nearestEnemy.position + Offset, position) < 50)
            {
                if (cooldown > 0)
                {
                    cooldown -= Engine.DeltaSeconds;
                }
                else
                {
                    cooldown = 1;
                    randomPos = new Vector2(Engine.Random.NextSingle() * 50 - 25, Engine.Random.NextSingle() * 50 - 25);
                }
                targetAcceleration = velocity - nearestEnemy.velocity - normalizedAcceleration * 10 + (randomPos + targetLocation) * Engine.DeltaSeconds;
            }
            else
            {
                targetAcceleration = velocity - nearestEnemy.velocity + targetLocation * Engine.DeltaSeconds - normalizedAcceleration * normalizedAcceleration.Length() * targetLocation.Length() / 10;
            }
            targetAngle = MathF.Atan2(targetAcceleration.Y, targetAcceleration.X) - MathF.PI / 2;
            thrust = MathF.Min(3 + normalizedAcceleration.Length() * 10, (1 - MathF.Abs(targetAngle - angle) / MathF.PI) * (targetAcceleration.Length()));
            Entity nearestProjectile = Engine.EntityManager.NearestProjectile(this, isFriendly);
            if (nearestProjectile != null)
            {
                var pos = Vector2.Normalize(position - nearestProjectile.position);
                var vel = Vector2.Normalize(velocity - nearestProjectile.velocity);
                if ((pos.X * vel.X + pos.Y * vel.Y) < -0.75f)
                {
                    int sign = Math.Sign(pos.X * vel.Y - vel.X * pos.Y);
                    if (sign == 0)
                    {
                        sign = 1;
                    }
                    targetAngle += MathF.PI / 2 * sign;
                    thrust = 20;
                }
            }
            if (Vector2.Distance(position, nearestEnemy.position) < enemyRange.particleVelocity)
            {
                if (weaponCooldown > 0)
                {
                    weaponCooldown -= Engine.DeltaSeconds;
                }
                else
                {
                    Engine.EntityManager.Add(new PulseShot(position, -Vector2.Normalize(relativePosition) * 8 + nearestEnemy.velocity, MathF.Atan2(relativePosition.Y, relativePosition.X) - MathF.PI/2, 0, isFriendly, damage, false));
                    SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
                    shots++;
                    if (shots == 3) 
                    {
                        shots = 0;
                        weaponCooldown = 2;
                    }
                    else
                    {
                        weaponCooldown = 0.1f;
                    }
                    
                }
            }
            RotateTowards(targetAngle, 0.1f);
            LowerCooldown();
            velocity += (new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * thrust) * Engine.DeltaSeconds;
            engineParticles.offsetVelocity = velocity;
            engineParticles.sprayAngle = angle * 180 / MathF.PI + 180;
            engineParticles.speedOfEmission = thrust * 75;
            engineParticles.particleVelocity = 3 - 3 / (thrust + 1);
            engineParticles.Update();
            engineParticles.position = position + new Vector2(-MathF.Sin(angle), MathF.Cos(angle)) * 8;
            yield return 0;
        }
    }
    IEnumerable<int> AdvancedFighter()
    {
        enemyRange.particleVelocity = 500;
        float tripleCooldown = 0;
        int shotCount = 0;
        while (true)
        {
            velocity *= 0.8f;
            Vector2 normalizedAcceleration = GetNormalizedAcceleration();
            float speed = 8 + Math.Max((Player.position - position).Length() - 500, 0) / 500;

            Entity nearestProjectile = Engine.EntityManager.NearestProjectile(NewDummyEnemy(position + Vector2.Normalize(Player.position - position) * 30, isFriendly), isFriendly);
            if (nearestProjectile != null)
            {
                var pos = Vector2.Normalize(position - nearestProjectile.position);
                var vel = Vector2.Normalize(velocity - nearestProjectile.velocity);
                if ((pos.X * vel.X + pos.Y * vel.Y) < -0.5f)
                {
                    int sign = Math.Sign(pos.X * vel.Y - vel.X * pos.Y);
                    if (sign == 0)
                    {
                        sign = 1;
                    }
                    velocity += new Vector2(-pos.Y, pos.X) * sign;
                }
            }

            Entity nearestEnemy = Engine.EntityManager.NearestEnemy(this);
            float playerDist = Vector2.Distance(Player.position, position);
            if ((nearestEnemy == null || Vector2.Distance(nearestEnemy.position, position) > playerDist))
            {
                if (playerDist > 200)
                {
                    GoToPosition(Player.position, (speed + playerDist / 100));
                }
                if (nearestEnemy == null)
                {
                    targetVector = (position - Player.position);
                }
                else
                {
                    targetVector = position - nearestEnemy.position;
                }
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                yield return 0;
            }

            float timeToHit;
            Vector2 playerIterativePosition = nearestEnemy.position;
            timeToHit = MathF.Sqrt(EntityManager.DistanceSqr(position, playerIterativePosition)) / 8;
            playerIterativePosition += Player.velocity * timeToHit;
            targetVector = (playerIterativePosition - position);
            targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
            RotateTowards(targetAngle, 0.1f);
            LowerCooldown();
            if (EntityManager.DistanceSqr(this, nearestEnemy) > 500 * 500)
            {
                GoToPosition(nearestEnemy.position, speed);
            }
            else
            {
                velocity += normalizedAcceleration * Engine.DeltaSeconds * 60;
                if (cooldown <= 0)
                {
                    if (tripleCooldown <= 0)
                    {
                        Texture2D tex = Assets.Get(Sprite.SpiralShot);
                        if (shotCount == 0)
                        {
                            SoundManager.PlaySound(Assets.Get(Sound.LMGFire), position);
                            Engine.EntityManager.Add(new PulseShot(position, Engine.ToUnitVector(angle) * 8, angle, 0, isFriendly, damage, false) { texture = tex, timeLeft = 3 } );
                        }
                        else
                        {
                            Engine.EntityManager.Add(new SpiralShot(position, Engine.ToUnitVector(angle) * 8, angle, 0, isFriendly, damage, false) { texture = tex, timeLeft = 3 });
                            Engine.EntityManager.Add(new SpiralShot(position, Engine.ToUnitVector(angle) * 8, angle, 0, isFriendly, damage, true) { texture = tex, timeLeft = 3 });
                        }
                        if (shotCount < 1)
                        {
                            shotCount++;
                            tripleCooldown = 0.02f;
                        }
                        else
                        {
                            shotCount = 0;
                            cooldown = 1.5f;
                        }
                    }
                }
            }
            if (tripleCooldown > 0)
            {
                tripleCooldown -= Engine.DeltaSeconds;
            }
            yield return 0;
        }
    }
    IEnumerable<int> StealthFighter()
    {
        enemyRange.particleVelocity = 500;
        SensingAbility = -1;
        StealthAbility = 0;
        Entity target = null;
        float trackTime = 0;
        Vector2 rand = position;
        while (true)
        {
            velocity += GetNormalizedAcceleration() * Engine.DeltaSeconds * 60;
            velocity *= 0.8f;
            if (cooldown > 0)
            {
                cooldown -= Engine.DeltaSeconds;
            }
            if (trackTime > 0) 
            { 
                trackTime -= Engine.DeltaSeconds;
            }
            else
            {
                target = null;
            }
            Entity nearestEnemy = Engine.EntityManager.NearestEnemy(this);
            if (nearestEnemy != null || target != null)
            {
                if (nearestEnemy != null || target == null)
                {
                    target = nearestEnemy;
                }
                if (nearestEnemy != null)
                {
                    trackTime = 3;
                }
                nearestEnemy = target;
                targetVector = nearestEnemy.position - position;
                targetAngle = MathF.Atan2(targetVector.Y, targetVector.X) + MathF.PI/2;
                float diff = MathF.Abs(angle - targetAngle);
                RotateTowards(targetAngle, diff / 10);
                if (targetVector.Length() > 200)
                {
                    GoToPosition(nearestEnemy.position, 15);
                }
                if (diff < 0.2f && cooldown <= 0)
                {
                    Engine.EntityManager.Add(new AssassinShot(position, Vector2.Normalize(targetVector) * 300, angle, 0, isFriendly, damage) { timeLeft = 0.2f });
                    cooldown = 1;
                }
            }
            else
            {
                if (Vector2.Distance(rand, position) < 500)
                {
                    float radius = Engine.SaveGame.CurrentMission.Planet.radius;
                    do
                    {
                        rand = new Vector2((Engine.Random.NextSingle() * 2 - 1) * radius * 3, (Engine.Random.NextSingle()* 2 - 1) * radius * 3);
                    }
                    while (rand.Length() < radius);
                }
                GoToPosition(rand, 5);
                targetVector = velocity;
                targetAngle = MathF.Atan2(targetVector.Y, targetVector.X) + MathF.PI / 2;
                float diff = MathF.Abs(angle - targetAngle);
                RotateTowards(targetAngle, diff / 10);
            }
            yield return 0;
        }
    }
    IEnumerable<int> Hunter()
    {
        enemyRange.particleVelocity = 300;
        SensingAbility = 0;
        StealthAbility = 1;
        Entity target = null;
        GrapplingHook grapplingHook = null;
        float trackTime = 0;
        float hookCooldown = 0;
        Vector2 rand = position;
        while (true)
        {
            velocity += GetNormalizedAcceleration() * Engine.DeltaSeconds * 60;
            velocity *= 0.8f;
            if (cooldown > 0)
            {
                cooldown -= Engine.DeltaSeconds;
            }
            if (hookCooldown > 0)
            {
                hookCooldown -= Engine.DeltaSeconds;
            }
            if (grapplingHook != null)
            {
                if (grapplingHook.isExpired)
                {
                    grapplingHook = null;
                }
                else if(grapplingHook.IsHooked && target != null)
                {
                    if (Vector2.Distance(grapplingHook.position, target.position) < 50)
                    {
                        target.Reveal(3f);
                    }
                    else
                    {
                        grapplingHook.isExpired = true;
                    }
                }
            }
            if (trackTime > 0)
            {
                trackTime -= Engine.DeltaSeconds;
            }
            else
            {
                target = null;
            }
            if (health <= 0 && grapplingHook != null)
            {
                grapplingHook.isExpired = true;
            }
            var nearestEnemy = Engine.EntityManager.NearestEnemy(this);
            if (target != null)
            {
                if (target.isExpired)
                {
                    target = nearestEnemy;
                }
            }
            else
            {
                target = nearestEnemy;
            }
            if (target != null)
            {
                if (target == nearestEnemy)
                {
                    trackTime = 3;
                }
                targetVector = target.position - position;
                targetAngle = MathF.Atan2(targetVector.Y, targetVector.X) + MathF.PI / 2;
                float diff = MathF.Abs(angle - targetAngle);
                RotateTowards(targetAngle, diff / 10);
                if (targetVector.Length() > 150)
                {
                    GoToPosition(target.position, 15);
                }
                else if (diff < 0.1f && hookCooldown <= 0 && grapplingHook == null)
                {
                    grapplingHook = new GrapplingHook(position, Vector2.Normalize(targetVector) * 30, angle, this, isFriendly);
                    Engine.EntityManager.Add(grapplingHook);
                    SoundManager.PlaySound(Assets.Get(Sound.Click), position);
                    Engine.ShakeScreen(0.2f);
                    hookCooldown = 10;
                }
                if (diff < 0.2f && targetVector.Length() < 300 && cooldown <= 0)
                {
                    Engine.EntityManager.Add(new PulseShot(position, Vector2.Normalize(targetVector) * 10, angle, 0, isFriendly, 5, false, 1) { texture = Assets.Get(Sprite.Microshot), timeLeft = 2f });
                    SoundManager.PlaySound(Assets.Get(Sound.LMGFire), position);
                    Engine.ShakeScreen(0.1f);
                    cooldown = 0.5f;
                }
            }
            else
            {
                if (Vector2.Distance(rand, position) < 500)
                {
                    float radius = Engine.SaveGame.CurrentMission.Planet.radius;
                    do
                    {
                        rand = new Vector2((Engine.Random.NextSingle() * 2 - 1) * radius * 3, (Engine.Random.NextSingle() * 2 - 1) * radius * 3);
                    }
                    while (rand.Length() < radius);
                }
                GoToPosition(rand, 5);
                targetVector = velocity;
                targetAngle = MathF.Atan2(targetVector.Y, targetVector.X) + MathF.PI / 2;
                float diff = MathF.Abs(angle - targetAngle);
                RotateTowards(targetAngle, diff / 10);
            }
            yield return 0;
        }
    }
    IEnumerable<int> Healer()
    {
        enemyRange.particleVelocity = 300;
        float weaponCooldown = 0;
        while(true)
        {
            velocity *= 0.8f;
            Entity nearestAlly = Engine.EntityManager.NearestAlly(this);
            Entity nearestEnemy = Engine.EntityManager.NearestEnemy(this);
            Entity nearestProjectile = Engine.EntityManager.NearestProjectile(this, isFriendly);
            if (nearestAlly != null)
            {
                if (Vector2.Distance(nearestAlly.position, position) > 300)
                {
                    GoToPosition(nearestAlly.position, 9);
                }
                else if(cooldown <= 0)
                {
                    cooldown = 5;
                    nearestAlly.Collide(-3);
                    for (float i = 0; i < 16; i++)
                    {
                        float angle = MathF.PI / 8 * i;
                        ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), 1, position, Engine.ToUnitVector(angle), angle, 0, Color.Green, Color.Transparent));
                    }
                    SoundManager.PlaySound(Assets.Get(Sound.Interact), position);
                }
            }
            else if(nearestEnemy != null && Vector2.Distance(nearestEnemy.position, position) < 500)
            {
                var dir = Vector2.Normalize(position - nearestEnemy.position);
                GoToPosition(position + dir * 10, 5);
            }   
            if (nearestEnemy != null && weaponCooldown <= 0 && Vector2.Distance(nearestEnemy.position, position) < 300)
            {
                var dir = Vector2.Normalize(nearestEnemy.position - position);
                weaponCooldown = 1;
                Engine.EntityManager.Add(new PulseShot(position, dir * 10, MathF.Atan2(dir.Y, dir.X) + MathF.PI / 2, 0, isFriendly, damage));
                SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
            }
            if (nearestProjectile != null)
            {
                var pos = Vector2.Normalize(position - nearestProjectile.position);
                var vel = Vector2.Normalize(velocity - nearestProjectile.velocity);
                if ((pos.X * vel.X + pos.Y * vel.Y) < -0.75f)
                {
                    int sign = Math.Sign(pos.X * vel.Y - vel.X * pos.Y);
                    if (sign == 0)
                    {
                        sign = 1;
                    }
                    GoToPosition(position + new Vector2(-pos.Y, pos.X) * sign * 10, 4);
                }
            }
            if (weaponCooldown > 0)
            {
                weaponCooldown -= Engine.DeltaSeconds;
            }
            LowerCooldown();
            RotateTowards(MathF.Atan2(velocity.Y, velocity.X) + MathF.PI / 2, 0.15f);
            yield return 0;
        }
    }
#endregion
    #region Infrastructure
    IEnumerable<int> Mothership()
    {
        enemyRange.particleVelocity = 300;
        float furnaceCooldown = 15;
        float craftingCooldown = 12;
        int requiredCraftsLeft = 20;
        Pickup furnaceItem = null;
        bool currentlyCrafting = false;
        while (true)
        {
            velocity = Vector2.Zero;
            if (EventHandler.AcknowledgeMessage(Message.MothershipCraftItem))
            {
                currentlyCrafting = true;
            }
            if (EventHandler.AcknowledgeMessage(Message.MothershipUpdateFurnace))
            {
                furnaceItem = UI.FurnaceSlot.daughterItem;
            }
            if (health <= 0)
            {
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), position);
            }
            if (furnaceItem != null)
            {
                furnaceCooldown -= Engine.DeltaSeconds;
                if (furnaceCooldown <= 0)
                {
                    if (furnaceItem is Module)
                    {
                        Engine.SaveGame.Scrap += 3;
                    }
                    else
                    {
                        Engine.SaveGame.Scrap++;
                    }
                    furnaceItem = null;
                    SoundManager.PlaySound(Assets.Get(Sound.Interact), position);
                }
            }
            else
            {
                furnaceCooldown = 15;
            }
            if (currentlyCrafting)
            {
                craftingCooldown -= Engine.DeltaSeconds;
                if (craftingCooldown <= 0)
                {
                    craftingCooldown = 12;
                    requiredCraftsLeft -= 1;
                    Collide(-100);
                    currentlyCrafting = false;
                }
            }

            EventHandler.UpdateFurnaceUI(15 - furnaceCooldown, 15, furnaceItem);
            EventHandler.UpdateCraftingUI(12 - craftingCooldown, 12, requiredCraftsLeft);
            if (requiredCraftsLeft <= 5)
            {
                if (cooldown > 0)
                {
                    cooldown -= Engine.DeltaSeconds;
                }
                else
                {
                    Entity nearestEnemy = Engine.EntityManager.NearestEnemy(this);
                    if (nearestEnemy != null)
                    {
                        Vector2 relativePosition = position - nearestEnemy.position;
                        if (relativePosition.Length() < enemyRange.particleVelocity)
                        {
                            Engine.EntityManager.Add(new AssassinShot(position, -Vector2.Normalize(relativePosition) * 100 + nearestEnemy.velocity, MathF.Atan2(relativePosition.Y, relativePosition.X) - MathF.PI / 2, 0, isFriendly, damage));
                            SoundManager.PlaySound(Assets.Get(Sound.MissileFire), position);
                            cooldown = 3;
                        }
                    }
                }
            }
            if (requiredCraftsLeft <= 0)
            {
                Engine.SaveGame.CurrentMission.CompleteCustomRule(this);
            }
            yield return 0;
        }
    }
    IEnumerable<int> Turret()
    {
        Enemy turretCannon = NewTurretCannon(this);
        Engine.EntityManager.Add(turretCannon);
        while (true)
        {
            velocity *= 0;
            turretCannon.position = position + new Vector2(8 * MathF.Sin(angle), -8 * MathF.Cos(angle));
            Entity nearestPickup = Engine.EntityManager.NearestItem(this, false);
            if (nearestPickup != null)
            {
                if (EntityManager.DistanceSqr(this, nearestPickup) < 2500 && health <= maxHealth - 15)
                {
                    Collide(-15);
                    nearestPickup.isExpired = true;
                    SoundManager.PlaySound(Assets.Get(Sound.Dock), position);
                    turretCannon.health = health;
                }
            }
            if (health <= 0)
            {
                isExpired = true;
                turretCannon.isExpired = true;
                Explode(20, ColliderRadius);
            }
            if (turretCannon.health != health)
            {
                health = Math.Min(turretCannon.health, health);
                turretCannon.health = health;
            }
            yield return 0;
        }
    }
    IEnumerable<int> TurretCannon(float _angle)
    {
        float bulletOffset = 4;
        enemyRange.particleVelocity = 400;
        ChildEnemy = true;
        while (true)
        {
            velocity *= 0;
            var dir = new Vector2(-MathF.Sin(_angle), MathF.Cos(_angle));
            var gunDir = new Vector2(-MathF.Sin(angle), MathF.Cos(angle));
            Vector2 offset = dir * (Size.Y / 2 + 150);
            Entity nearestEnemy = Engine.EntityManager.NearestEnemy(NewDummyEnemy(position + offset, isFriendly));
            enemyRange.position = position + offset;
            if (nearestEnemy != null)
            {
                var relPos = Vector2.Normalize(nearestEnemy.position - position + Engine.SaveGame.CurrentMission.GetNormalizedAcceleration(nearestEnemy.position) * 1000);
                float distance = (nearestEnemy.position - position - dir * (Size / 2)).Length();
                float dot = relPos.X * dir.X + relPos.Y * dir.Y;
                float cross = relPos.X * gunDir.Y - gunDir.X * relPos.Y;
                if (cross < -0.1f)
                {
                    angle -= 10 * Engine.DeltaSeconds;
                }
                else if (cross > 0.1f)
                {
                    angle += 10 * Engine.DeltaSeconds;
                }
                else if (distance < 400 && cooldown <= 0 && dot < 0.5f)
                {
                    var rotatedOffset = gunDir * Size.Y / 2;
                    Engine.EntityManager.Add(NewMissile(position - rotatedOffset + new Vector2(-dir.Y, dir.X) * bulletOffset, -gunDir * 8, angle, isFriendly));
                    Assets.Get(Sound.MissileFire).Play();
                    cooldown = 0.9f;
                    bulletOffset *= -1;
                }
            }
            if (cooldown > 0)
            {
                cooldown -= Engine.DeltaSeconds;
            }
            yield return 0;
        }
    }
    IEnumerable<int> Orbiter()
    {
        float furnaceCooldown = 15;
        float craftingCooldown = 12;
        Pickup furnaceItem = null;
        bool currentlyCrafting = false;
        while (true)
        {
            if (EventHandler.AcknowledgeMessage(Message.MothershipCraftItem))
            {
                currentlyCrafting = true;
            }
            if (EventHandler.AcknowledgeMessage(Message.MothershipUpdateFurnace))
            {
                furnaceItem = UI.FurnaceSlot.daughterItem;
            }
            if (health <= 0)
            {
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), position);
            }
            if (furnaceItem != null)
            {
                furnaceCooldown -= Engine.DeltaSeconds;
                if (furnaceCooldown <= 0)
                {
                    Engine.SaveGame.Scrap++;
                    furnaceItem = null;
                    SoundManager.PlaySound(Assets.Get(Sound.Interact), position);
                }
            }
            else
            {
                furnaceCooldown = 15;
            }
            if (currentlyCrafting)
            {
                craftingCooldown -= Engine.DeltaSeconds;
                if (craftingCooldown <= 0)
                {
                    craftingCooldown = 12;
                    Collide(-100);
                    currentlyCrafting = false;
                }
            }

            EventHandler.UpdateFurnaceUI(15 - furnaceCooldown, 15, furnaceItem);
            EventHandler.UpdateCraftingUI(12 - craftingCooldown, 12, health);
            yield return 0;
        }
    }
    IEnumerable<int> PickupDrone()
    {
        bool currentlyLeaving = false;
        while (true)
        {
            if (EventHandler.AcknowledgeMessage(Message.EscapeDroneLeave))
            {
                currentlyLeaving = true;
                Engine.SaveGame.CurrentMission.CompleteCustomRule(this);
            }
            if (health <= 0)
            {
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), position);
            }
            if (currentlyLeaving)
            {
                velocity += new Vector2(Engine.DeltaSeconds * 10, -Engine.DeltaSeconds * 5);
            }
            else
            {
                velocity = Vector2.Zero;
                position = position * (1f - Engine.DeltaSeconds) - new Vector2(0, Engine.SaveGame.CurrentMission.Planet.radius * 1.5f) * Engine.DeltaSeconds;
            }
            yield return 0;
        }
    }
    IEnumerable<int> Miner()
    {
        ParticleEmitter miningDebris = new(Assets.Get(Sprite.Circle), 0.1f, position, angle, MathF.PI/2, 2, 500, Color.Cyan, EmitterType.EmissionOverTime) 
        { particleFadeToColor = Color.Transparent, particleAngularVelocity = Engine.OneToNegOne() / 2 };
        float healTimer = 30;
        while (true)
        {
            velocity *= 0;
            if (health <= 0)
            {
                Explode(4, ColliderRadius);
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), position);
            }
            if (health < maxHealth)
            {
                if (healTimer > 0)
                {
                    healTimer -= Engine.DeltaSeconds;
                }
                else
                {
                    healTimer = 30;
                    Collide(-15);
                }
            }
            else
            {
                healTimer = 30;
            }
            Entity nearestPickup = Engine.EntityManager.NearestItem(this, false);
            if (nearestPickup != null)
            {
                if (EntityManager.DistanceSqr(this, nearestPickup) < 2500 && health <= maxHealth - 15)
                {
                    Collide(-15);
                    nearestPickup.isExpired = true;
                    SoundManager.PlaySound(Assets.Get(Sound.Dock), position);
                }
            }
            miningDebris.position = position + new Vector2(-MathF.Sin(angle), MathF.Cos(angle)) * texture.Height / 2;
            miningDebris.Update();
            yield return 0;
        }
    }
    IEnumerable<int> MakeshiftMothership()
    {
        float furnaceCooldown = 15;
        float craftingCooldown = 12;
        Pickup furnaceItem = null;
        bool currentlyCrafting = false;
        int tier = 1;
        int untilNextTier = 1;
        while (true)
        {
            float tierBonus = 1 / MathF.Sqrt(tier);
            if (EventHandler.AcknowledgeMessage(Message.MothershipCraftItem))
            {
                currentlyCrafting = true;
            }
            if (EventHandler.AcknowledgeMessage(Message.MothershipUpdateFurnace))
            {
                furnaceItem = UI.FurnaceSlot.daughterItem;
            }
            var dockableComponent = (Components.GetComponent(ComponentType.DockableComponent));
            if (furnaceItem != null)
            {
                furnaceCooldown -= Engine.DeltaSeconds;
                if (furnaceCooldown <= 0)
                {
                    if (furnaceItem is Module)
                    {
                        Engine.SaveGame.Scrap += 3;
                    }
                    else
                    {
                        Engine.SaveGame.Scrap++;
                    }
                    furnaceItem = null;
                    SoundManager.PlaySound(Assets.Get(Sound.Interact), position);
                }
            }
            else
            {
                furnaceCooldown = 15 * tierBonus;
            }
            if (currentlyCrafting)
            {
                craftingCooldown -= Engine.DeltaSeconds;
                if (craftingCooldown <= 0)
                {
                    craftingCooldown = 12 * tierBonus;
                    untilNextTier -= 1;
                    if (untilNextTier <= 0)
                    {
                        tier++;
                        untilNextTier = tier;
                        maxHealth = 400 + (int)(100 * MathF.Sqrt(tier));
                    }
                    Collide(-100);
                    currentlyCrafting = false;
                }
            }

            EventHandler.UpdateFurnaceUI(15f * tierBonus - furnaceCooldown, 15f * tierBonus, furnaceItem);
            EventHandler.UpdateCraftingUI(12f * tierBonus - craftingCooldown, 12f * tierBonus, untilNextTier);
            LowerCooldown();
            if (dockableComponent.IsValid && (dockableComponent as DockableComponent).IsDocked)
            {
                if (tier > 1 && Input.NewMouseState.LeftButton == ButtonState.Pressed && cooldown <= 0 && !UIManager.LockMouseInput)
                {
                    targetVector = Vector2.Normalize(new Vector2(Mouse.GetState().X, Mouse.GetState().Y) - Engine.BackBuffer / 2 - position + Engine.Camera.Position);
                    targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                    Engine.EntityManager.Add(new PulseShot(position, targetVector * 9 + velocity, targetAngle, 0, true, damage, true));
                    SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
                    cooldown = 0.75f;
                    if (tier > 3)
                    {
                        cooldown = 0.5f;
                    }
                    Engine.ShakeScreen(0.2f);
                    velocity -= targetVector / 4;
                }
                Keys[] pressedKey = Input.NewState.GetPressedKeys();
                Vector2 direction = Vector2.Zero;
                bool isEngineActive = false;
                for (int i = 0; i < pressedKey.Length; i++)
                {
                    switch (pressedKey[i])
                    {
                        case Keys.W:
                            direction += new Vector2(0, -1);
                            isEngineActive = true;
                            break;
                        case Keys.A:
                            direction += new Vector2(-1, 0);
                            isEngineActive = true;
                            break;
                        case Keys.S:
                            direction += new Vector2(0, 1);
                            isEngineActive = true;
                            break;
                        case Keys.D:
                            direction += new Vector2(1, 0);
                            isEngineActive = true;
                            break;
                        default:
                            break;
                    }
                }
                if (isEngineActive)
                {
                    angle = angle * 0.5f + MathF.Atan2(direction.X, -direction.Y) * 0.5f;
                    velocity += Engine.ToUnitVector(angle) * 60 * Engine.DeltaSeconds * 0.1f;
                }
            }
            Engine.SaveGame.CurrentMission.CalculateTrajectory(position, velocity, ColliderRadius);
            //Prevents the player from losing it accidentally
            var planet = Engine.SaveGame.CurrentMission.Planet;
            if (position.Length() >= 40 * 50 + planet.radius)
            {
                velocity *= 0.8f;
                velocity += Vector2.Normalize(-position) * Engine.DeltaSeconds * (position.Length() - (40 * 50 + planet.radius));
            }
            yield return 0;
        }
    }
    IEnumerable<int> LargeMiner()
    {
        var arms = new List<Enemy>()
        {
            NewLargeMinerArm(position - Engine.ToUnitVector(angle + MathF.PI/2) * texture.Width / 2 + new Vector2(2, 2), velocity, angle, isFriendly, 0, this),
            NewLargeMinerArm(position + Engine.ToUnitVector(angle + MathF.PI/2) * texture.Width / 2 + new Vector2(-2, 2), velocity, angle, isFriendly, 2.5f, this),
        };
        float sparkCooldown = 0;
        foreach (var arm in arms)
        {
            Engine.EntityManager.Add(arm);
        }
        while (true)
        {
            velocity = Vector2.Zero;
            if (sparkCooldown > 0)
            {
                sparkCooldown -= Engine.DeltaSeconds;
            }
            LowerCooldown();
            for(int i = 0; i < 2; i++)
            {
                var entity = arms[i];
                if (!entity.isExpired)
                {

                }
                else
                {
                    if (Engine.Random.NextSingle() > cooldown)
                    {
                        cooldown = 1.5f;
                        if (arms[0].isExpired && arms[1].isExpired)
                        {
                            cooldown = 1.1f;
                        }
                        float sign = (i * 2 - 1);
                        var dir = Engine.ToUnitVector(this.angle + MathF.PI / 2);
                        var offset = new Vector2(Engine.Random.NextSingle(), Engine.Random.NextSingle() * 2 - 1);
                        float angle = MathF.Atan2(dir.Y, dir.X) + MathF.PI / 2;
                        Engine.EntityManager.Add(new PulseShot(position + dir * sign * (texture.Width / 2 - 4), dir * sign * 5 + offset * 2 * sign, angle * sign, 0, isFriendly, damage) { texture = Assets.Get(Sprite.Microshot), timeLeft = 3f });
                        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), position);
                    }
                    if (Engine.Random.NextSingle() > sparkCooldown)
                    {
                        sparkCooldown = 1f;
                        if (arms[0].isExpired && arms[1].isExpired)
                        {
                            sparkCooldown = 0.8f;
                        }
                        float sign = (i * 2 - 1);
                        var dir = Engine.ToUnitVector(this.angle + MathF.PI / 2);
                        var offset = new Vector2(Engine.Random.NextSingle(), Engine.Random.NextSingle() * 2 - 1);
                        ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), 0.5f, position + dir * sign * (texture.Width / 2 - 4), dir * sign * 3 + offset * sign, 0, 0, Color.Yellow, new Color(255, 0, 0, 0)));
                    }
                }
            }
            var nearestPickup = Engine.EntityManager.NearestItem(this, false);
            if (nearestPickup != null)
            {
                float dist = Vector2.Distance(nearestPickup.position, position);
                if (dist is < 1000 and > 50)
                {
                    Texture2D texture = Assets.Get(Sprite.Dot);
                    var direction = Vector2.Normalize(position - nearestPickup.position);
                    float angle = MathF.Atan2(direction.Y, direction.X);
                    float distance = Vector2.Distance(position, nearestPickup.position);
                    for (float i = 0; i < distance / texture.Height / 2; i++)
                    {
                        ParticleManager.Add(new Particle(texture, position - direction * i * texture.Height * 2, angle, color * ((1000f - distance) / 1000f)));
                    }
                    nearestPickup.velocity += direction * Engine.DeltaSeconds * 4;
                }
                else if (dist < 50)
                {
                    nearestPickup.isExpired = true;
                    Collide(-20);
                }
            }
            if (health != maxHealth && (!arms[0].isExpired || !arms[1].isExpired))
            {
                var diff = maxHealth - health;
                foreach (var entity in arms)
                {
                    entity.health -= diff / 2;
                }
                health = maxHealth;
            }
            if (health <= 0)
            {
                isExpired = true;
                Explode(10, ColliderRadius);
                SoundManager.PlaySound(Assets.Get(Sound.Explosion), position);
                Engine.EntityManager.Add(new Module(Modules.GrapplingHook, position, GetNormalizedAcceleration() * 10, angularVelocity));
            }
            yield return 0;
        }
    }
    IEnumerable<int> LargeMinerArm(float _pos, Entity _parent)
    {
        float pos = _pos;
        Vector2 initialPos = position - _parent.position;
        Vector2 dir = Engine.ToUnitVector(angle);
        bool createSparks = true;
        while(true)
        {
            velocity = Vector2.Zero;
            pos += Engine.DeltaSeconds;
            if (pos < 3.75f)
            {
                position = initialPos + _parent.position + new Vector2(Engine.Random.NextSingle() * 2 - 1, Engine.Random.NextSingle() * 2 - 1) + dir * 5 * pos;
            }
            else if (pos < 4)
            {
                position = initialPos + _parent.position + dir * 20 - dir * 20 * ((pos - 3.75f) * (pos - 3.75f) * 16);
            }
            else
            {
                if (createSparks)
                {
                    for (int i = Engine.Random.Next(7, 10); i > 0; i--)
                    {
                        ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.5f, position + new Vector2(0, this.texture.Height / 2), dir + new Vector2(Engine.Random.NextSingle() * 2 - 1, Engine.Random.NextSingle() * 2 - 1) / 2, 0, 0, Color.Cyan, Color.Transparent));
                        Engine.ShakeScreen(0.1f);
                        SoundManager.PlaySound(Assets.Get(Sound.ShieldHit), position);
                    }
                    createSparks = false;
                }
                position = initialPos + _parent.position + new Vector2(Engine.Random.NextSingle() * 2 - 1, Engine.Random.NextSingle() * 2 - 1) * (5f - pos) * (5f - pos) * 5;
            }
            if (pos > 5)
            {
                pos -= 5;
                createSparks = true;
            }
            if (health <= 0)
            {
                isExpired = true;
                Explode(0, 0);
                SoundManager.PlaySound(Assets.Get(Sound.Explosion), position);
            }
            yield return 0;
        }
    }
    IEnumerable<int> WarpGate()
    {
        float time = -3;
        ChildEnemy = true;
        bool isThrough = false;
        int dir = 0;
        while (true)
        {
            if (dir == 0)
            {
                if (Engine.SaveGame.System == 0)
                {
                    dir = 1;
                }
                else if (Engine.SaveGame.System == 2)
                {
                    dir = -1;
                }
                else if (Input.NewState.IsKeyDown(Keys.LeftShift))
                {
                    dir = -1;
                }
                else if (Input.NewState.IsKeyDown(Keys.RightShift))
                {
                    dir = 1;
                }
            }
            else
            {
                time += Engine.DeltaSeconds * (isThrough ? -5 : 1);
                float count = Math.Clamp(time, 0, 10);
                angularVelocity = count / 350 * dir;
                float angle = this.angle;
                for (float i = 0; i < count * count * 20 && !isThrough; i++)
                {
                    float maxCount = 2000;
                    float ratio = 1 - (i / maxCount) * (i / maxCount);
                    //Vector3 col = (new Vector3(126, 118, 230) * (1 - ratio) + new Vector3(72, 61, 139) * (ratio)) * (MathF.Sin(angle * 10 + ratio * 10 + time * 3)/3 + 0.67f);
                    Vector3 col = (new Vector3(0, 0, 0) * (1 - ratio) + new Vector3(72, 61, 139) * (ratio)) * (MathF.Sin(angle * 10 + ratio * 10 + time * 3) / 3 + 0.67f);
                    angle += 1.61803398875f;
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), Engine.ToUnitVector(angle) * (texture.Height / 2f) * ratio + position, angle, new Color(col.X / 255, col.Y / 255, col.Z / 255)));
                }
                if (Engine.Random.NextSingle() > 1f - Engine.DeltaSeconds * count / 5)
                {
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), 10f, Engine.ToUnitVector(Engine.Random.NextSingle() * MathF.Tau) * (150 + Engine.Random.NextSingle() * 300) + position, new Vector2(Engine.Random.NextSingle() - 0.5f, Engine.Random.NextSingle() - 0.5f),
                        Engine.Random.NextSingle() * MathF.Tau, Engine.Random.NextSingle() - 0.5f, Color.SlateBlue * 0.5f, Color.Transparent));
                }
                if (!isThrough && Vector2.Distance(position, Engine.SaveGame.Player.position) < (texture.Height / 2f) && count >= 10)
                {
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Explosion), 1f, position, Vector2.Zero, 0, 0, Color.White, Color.Transparent));
                    SoundManager.PlayGlobalSound(Assets.Get(Sound.Full));
                    isThrough = true;
                    time = 10;
                    Engine.SaveGame.System += dir;
                    Engine.SaveGame.Player.Progression = -1;
                    Engine.SaveGame.CurrentMission.CompleteCustomRule(this);
                }
                if (isThrough)
                {
                    Engine.SaveGame.Player.position = position;
                    Engine.SaveGame.Player.velocity = velocity;
                }
            }
            yield return 0;
        }
    }
    IEnumerable<int> QuantumResonator()
    {
        cooldown = 5;
        angularVelocity = 0.01f;
        int waveCount = 0;
        while (true)
        {
            velocity = Vector2.Zero;
            if (Engine.SaveGame.CurrentMission.Name == "???")
            {
                if (cooldown <= 0)
                {
                    if (waveCount < 3)
                    {
                        for (float angle = MathF.PI / 30; angle < MathF.Tau; angle += MathF.PI/30)
                        {
                            Vector2 dir = Engine.ToUnitVector(angle);
                            ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), 0.5f, position, dir * 2, angle, 0, Color.Cyan, Color.Transparent));
                        }
                        SoundManager.PlaySound(Assets.Get(Sound.Interact), position);
                        cooldown = 0.75f;
                    }
                    else if (waveCount == 3)
                    {
                        cooldown = 4;
                    }
                    else
                    {
                        isExpired = true;
                        Explode(0, 0);
                        SoundManager.PlaySound(Assets.Get(Sound.Explosion), position);
                        Enemy inferno = NewInfernoBoss(Engine.SaveGame.Player.position + new Vector2(1000, 0), Vector2.Zero, 0);
                        Engine.EntityManager.Add(inferno);
                        SoundManager.ChangeTrack(Assets.Get(Sound.secretBoss));
                    }
                    waveCount++; 
                }
                LowerCooldown();
            }
            yield return 0;
        }
    }
    IEnumerable<int> Communicator()
    {
        float cooldown = 5;
        while (true)
        {
            if (wasHit)
            {
                var dir = Vector2.Normalize(new Vector2(Engine.OneToNegOne(), Engine.OneToNegOne()));
                SoundManager.PlaySound(Assets.Get(Sound.LMGFire), position);
                Engine.EntityManager.Add(new AssassinShot(position, dir * 10, MathF.Atan2(dir.Y, dir.X), 0, isFriendly, 5, 1));
            }
            if (cooldown > 0)
            {
                cooldown -= Engine.DeltaSeconds;
            }
            else
            {
                cooldown = 5;
                for (float angle = MathF.PI / 30; angle < MathF.Tau; angle += MathF.PI / 30)
                {
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), 0.25f, position, Engine.ToUnitVector(angle) * 2, angle, 0, Color.Cyan, Color.Transparent));
                }
            }
            yield return 0;
        }
    }
    IEnumerable<int> Trader()
    {
        while(true)
        {

        }
    }
    #endregion
    public static Enemy NewDummyEnemy(Vector2 _position, bool _isFriendly = false)
    {
        return new(_position, Vector2.Zero, 0, 0, 0, Assets.Get(Sprite.Fighter), _isFriendly);
    }
    public static Enemy NewFighter(Vector2 position, Vector2 velocity, float angle, bool _isFriendly = false)
    {
        Enemy enemy = new(position, velocity, angle, 5, 8, Assets.Get(Sprite.Fighter), _isFriendly);
        enemy.AddBehaviour(enemy.Fighter());
        enemy.AddBehaviour(enemy.AvoidNearbyAllies());
        enemy.AddBehaviour(enemy.EnemyDeath(2));
        return enemy;
    }
    public static Enemy NewCarrier(Vector2 position, Vector2 velocity, float angle, bool _isFriendly = false)
    {
        Enemy enemy = new(position, velocity, angle, 5, 15, Assets.Get(Sprite.Cruiser), _isFriendly);
        enemy.AddBehaviour(enemy.Carrier());
        enemy.AddBehaviour(enemy.AvoidNearbyAllies());
        enemy.AddBehaviour(enemy.EnemyDeath(1));
        return enemy;
    }
    public static Enemy NewSniper(Vector2 position, Vector2 velocity, float angle, bool _isFriendly = false)
    {
        Enemy enemy = new(position, velocity, angle, 8, 5, Assets.Get(Sprite.Sniper), _isFriendly);
        enemy.AddBehaviour(enemy.Sniper());
        enemy.AddBehaviour(enemy.AvoidNearbyAllies());
        enemy.AddBehaviour(enemy.EnemyDeath(1));
        return enemy;
    }
    public static Enemy NewMissile(Vector2 position, Vector2 velocity, float angle, bool _isFriendly = false)
    {
        Enemy enemy = new(position, velocity, angle, 8, 10, Assets.Get(Sprite.Missile), _isFriendly);
        enemy.AddBehaviour(enemy.Missile());
        enemy.AddBehaviour(enemy.AvoidNearbyAllies());
        return enemy;
    }
    public static Enemy NewShotgunner(Vector2 position, Vector2 velocity, float angle, bool _isFriendly = false)
    {
        Enemy enemy = new(position, velocity, angle, 5, 10, Assets.Get(Sprite.Shotgunner), _isFriendly);
        enemy.AddBehaviour(enemy.Shotgunner());
        enemy.AddBehaviour(enemy.AvoidNearbyAllies());
        enemy.AddBehaviour(enemy.EnemyDeath(1));
        return enemy;
    }
    public static Enemy NewShield(Entity parent, float distance, int health, float theta, int size, bool _isFriendly = false)
    {
        Sprite shieldSprite;
        if(size == 0)
        {
            shieldSprite = Sprite.ShotgunShield;
        }
        else
        {
            shieldSprite = Sprite.OverloadShield;
        }
        Enemy enemy = new(parent.position, parent.velocity, parent.angle, 0, health, Assets.Get(shieldSprite), _isFriendly);
        enemy.AddBehaviour(enemy.Shield(parent, distance, theta));
        return enemy;
    }
    public static Enemy NewSymmetryBoss(Vector2 position, Vector2 velocity, float angle, bool _isFriendly = false)
    {
        Enemy enemy = new(position, velocity, angle, 6, 100, Assets.Get(Sprite.SymmetryBoss), _isFriendly);
        enemy.AddBehaviour(enemy.Symmetry());
        return enemy;
    }
    public static Enemy NewOverloadBoss(Vector2 position, Vector2 velocity, float angle, bool _isFriendly = false)
    {
        Enemy enemy = new(position, velocity, angle, 10, 120, Assets.Get(Sprite.OverloadBoss), _isFriendly);
        enemy.AddBehaviour(enemy.Overload());
        return enemy;
    }
    public static Enemy NewMothership(Vector2 position, Vector2 velocity, float angle)
    {
        Enemy enemy = new(position, velocity, angle, 8, 1000, Assets.Get(Sprite.Mothership), true);
        enemy.AddBehaviour(enemy.Mothership());
        enemy.Components.Add(new DockableComponent(enemy, UI.MothershipMenu));
        return enemy;
    }
    public static Enemy NewTurret(Vector2 position, Vector2 velocity, float angle, bool _isFriendly)
    {
        Enemy enemy = new(position, velocity, angle, 0, 400, Assets.Get(Sprite.TurretBase), _isFriendly);
        enemy.AddBehaviour(enemy.Turret());
        return enemy;
    }
    public static Enemy NewTurret(Vector2 position, Vector2 velocity, float angle)
    {
        return NewTurret(position, velocity, angle, true);
    }
    public static Enemy NewTurretCannon(Enemy parent)
    {
        Enemy enemy = new(parent.position, parent.velocity, parent.angle, 6, 800, Assets.Get(Sprite.TurretHead), parent.isFriendly);
        enemy.AddBehaviour(enemy.TurretCannon(parent.angle));
        return enemy;
    }
    public static Enemy NewMiner(Vector2 position, Vector2 velocity, float angle, bool _isFriendly)
    {
        Enemy enemy = new(position, velocity, angle, 0, 600, Assets.Get(Sprite.Miner), _isFriendly);
        enemy.AddBehaviour(enemy.Miner());
        return enemy;
    }
    public static Enemy NewMiner(Vector2 position, Vector2 velocity, float angle)
    {
        return NewMiner(position, velocity, angle, true);
    }
    public static Enemy NewHovercraft(Vector2 position, Vector2 velocity, float angle, bool isFriendly = false)
    {
        Enemy enemy = new(position, velocity, angle, 2, 12, Assets.Get(Sprite.Hovercraft), isFriendly);
        enemy.AddBehaviour(enemy.Hovercraft());
        enemy.AddBehaviour(enemy.EnemyDeath(2));
        return enemy;
    }
    public static Enemy NewExcursionBoss(Vector2 position, Vector2 velocity, float angle, bool isFriendly = false)
    {
        Enemy enemy = new(position, velocity, angle, 10, 200, Assets.Get(Sprite.ExcursionBoss), isFriendly);
        enemy.AddBehaviour(enemy.Excursion());
        return enemy;
    }
    public static Enemy NewExcursionBoss(Vector2 position, Vector2 velocity, float angle)
    {
        return NewExcursionBoss(position, velocity, angle, false);
    }
    public static Enemy NewWyvernBoss(Vector2 position, Vector2 velocity, float angle, bool isFriendly = false)
    {
        Enemy enemy = new(position, velocity, angle, 8, 100, Assets.Get(Sprite.WyvernBoss), isFriendly);
        enemy.AddBehaviour(enemy.Wyvern(null));
        return enemy;
    }
    public static Enemy NewAdvancedFighter(Vector2 position, Vector2 velocity, float angle, bool _isFriendly = false)
    {
        Enemy enemy = new(position, velocity, angle, 3, 15, Assets.Get(Sprite.AdvancedFighter), _isFriendly);
        enemy.AddBehaviour(enemy.AdvancedFighter());
        enemy.AddBehaviour(enemy.AvoidNearbyAllies());
        enemy.AddBehaviour(enemy.EnemyDeath(1.5f));
        return enemy;
    }
    public static Enemy NewOrbiter(Vector2 position, Vector2 velocity, float angle)
    {
        Enemy enemy = new(position, velocity, angle, 10, 300, Assets.Get(Sprite.Orbiter), true);
        enemy.AddBehaviour(enemy.Orbiter());
        enemy.Components.Add(new DockableComponent(enemy, UI.MothershipMenu));
        enemy.angularVelocity = -0.01f;
        return enemy;
    }
    public static Enemy NewPickupDrone(Vector2 position, Vector2 velocity, float angle)
    {
        Enemy enemy = new(position, velocity, angle, 0, 250, Assets.Get(Sprite.PickupDrone), true);
        enemy.AddBehaviour(enemy.PickupDrone());
        enemy.Components.Add(new DockableComponent(enemy, UI.PickupDroneMenu));
        return enemy;
    }
    public static Enemy NewStealthFighter(Vector2 position, Vector2 velocity, float angle, bool _isFriendly = false)
    {
        Enemy enemy = new(position, velocity, angle, 10, 8, Assets.Get(Sprite.Fighter), _isFriendly);
        enemy.AddBehaviour(enemy.StealthFighter());
        enemy.AddBehaviour(enemy.AvoidNearbyAllies());
        enemy.AddBehaviour(enemy.EnemyDeath(2));
        return enemy;
    }
    public static Enemy NewHunter(Vector2 position, Vector2 velocity, float angle, bool _isFriendly = false) 
    {
        Enemy enemy = new(position, velocity, angle, 8, 15, Assets.Get(Sprite.Fighter), _isFriendly);
        enemy.AddBehaviour(enemy.Hunter());
        enemy.AddBehaviour(enemy.AvoidNearbyAllies());
        enemy.AddBehaviour(enemy.EnemyDeath(1));
        return enemy;
    }
    public static Enemy NewMakeshiftMothership(Vector2 position, Vector2 velocity, float angle, bool _isFriendly = true)
    {
        Enemy enemy = new(position, velocity, angle, 8, 500, Assets.Get(Sprite.Mothership), _isFriendly);
        enemy.AddBehaviour(enemy.MakeshiftMothership());
        enemy.AddBehaviour(enemy.EnemyDeath(0.01f));
        enemy.Components.Add(new DockableComponent(enemy, UI.MothershipMenu));
        return enemy;
    }
    public static Enemy NewExodus(Vector2 position, Vector2 velocity, float angle, bool _isFriendly = false)
    {
        Enemy enemy = new(position, velocity, angle, 8, 80, Assets.Get(Sprite.ExodusBoss), _isFriendly);
        enemy.AddBehaviour(enemy.Exodus());
        return enemy;
    }
    public static Enemy NewHealer(Vector2 position, Vector2 velocity, float angle, bool _isFriendly = false)
    {
        Enemy enemy = new(position, velocity, angle, 4, 10, Assets.Get(Sprite.Healer), _isFriendly);
        enemy.AddBehaviour(enemy.Healer());
        return enemy;
    }
    public static Enemy NewLargeMiner(Vector2 position, Vector2 velocity, float angle)
    {
        Enemy enemy = new(position, velocity, angle, 5, 500, Assets.Get(Sprite.LargeMiner), false);
        enemy.AddBehaviour(enemy.LargeMiner());
        return enemy;
    }
    public static Enemy NewLargeMinerArm(Vector2 position, Vector2 velocity, float angle, bool _isFriendly, float _pos, Entity _parent)
    {
        Enemy enemy = new(position, velocity, angle, 5, 200, Assets.Get(Sprite.LargeMinerArm), _isFriendly);
        enemy.AddBehaviour(enemy.LargeMinerArm(_pos, _parent));
        return enemy;
    }
    public static Enemy NewWarpGate(Vector2 position, Vector2 velocity, float angle)
    {
        Enemy enemy = new(position, velocity, angle, 0, 1000, Assets.Get(Sprite.WarpGate), true);
        enemy.AddBehaviour(enemy.WarpGate());
        return enemy;
    }
    public static Enemy NewVeilBoss(Vector2 position, Vector2 velocity, float angle)
    {
        Enemy enemy = new(position, velocity, angle, 12, 150, Assets.Get(Sprite.VeilBoss), false);
        enemy.AddBehaviour(enemy.VeilBoss());
        return enemy;
    }
    public static Enemy NewQuantumResonator(Vector2 _position)
    {
        var enemy = new Enemy(_position, Vector2.Zero, 0, 0, 10, Assets.Get(Sprite.QuantumResonator), true);
        enemy.AddBehaviour(enemy.QuantumResonator());
        return enemy;
    }
    public static Enemy NewInfernoBoss(Vector2 _position, Vector2 _velocity, float _angle)
    {
        var enemy = new Enemy(_position, _velocity, _angle, 6, 175, Assets.Get(Sprite.InfernoBoss));
        enemy.AddBehaviour(enemy.Inferno());
        return enemy;
    }
    public static Enemy NewFlareBoss(Vector2 _position, Vector2 _velocity, float _angle, Enemy _inferno)
    {
        var enemy = new Enemy(_position, _velocity, _angle, 15, 125, Assets.Get(Sprite.FlareBoss));
        enemy.AddBehaviour(enemy.Flare(_inferno));
        return enemy;
    }
    public static Enemy NewCommunicator(Vector2 _position, Vector2 _velocity, float _angle, bool _isFriendly = true) 
    {
        var enemy = new Enemy(_position, _velocity, _angle, 6, 400, Assets.Get(Sprite.Communicator), _isFriendly);
        enemy.AddBehaviour(enemy.Communicator());
        enemy.AddBehaviour(enemy.EnemyDeath(1));
        return enemy;
    }
    public static Enemy NewTrader(Vector2 _position, Vector2 _velocity, float _angle)
    {
        var enemy = new Enemy(_position, _velocity, _angle, 999, 400, Assets.Get(Sprite.Communicator), true);
        enemy.AddBehaviour(enemy.Trader());
        enemy.Components.Add(new DockableComponent(enemy, UI.UpgradeMenu, false));
        return enemy;
    }
}
