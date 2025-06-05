using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Components;
using Space_Wars.Content.Main.Particles;
using System;
using System.Collections.Generic;
using System.Dynamic;
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
    public override float ColliderRadius => Engine.EnemyHitboxModifier * ((texture.Height + texture.Width) / 4 + 1);
    private Vector2 targetVector;
    public ParticleEmitter enemyRange = new(Assets.Get(Sprite.Dot), Vector2.Zero, 0, 0.75f, Color.Red);
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
        position += velocity;
        angle += angularVelocity;

        ApplyBehaviours();
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
        return Engine.EntityManager.CurrentMission.GetNormalizedAcceleration(position);
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
        Vector2 gravityForce = GetNormalizedAcceleration();
        targetVector = Vector2.Normalize(Vector2.Normalize(_position - position) + gravityForce * 1.25f);
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
            health = 0;
            SoundManager.PlaySound(hitSound, position);
            return;
        }
        if (damage > 0)
        {
            SoundManager.PlaySound(hitSound, position);
            health -= damage;
            Engine.ShakeScreen(10 / ((position - Engine.Camera.Position).Length() + 200) * damage);
            ParticleManager.Add(new Particle(null, 1, position + new Vector2(0,-1), new Vector2(0,-1.5f), 0, 0, 1, true, Color.Orange, Color.Red) { drawText = $"{damage}" });
        }
        else if (damage < 0)
        {
            SoundManager.PlaySound(Assets.Get(Sound.Full), position);
            health -= damage;
            ParticleManager.Add(new Particle(null, 1, position + new Vector2(0, -1), new Vector2(0, -1.5f), 0, 0, 1, true, Color.Orange, Color.Green) { drawText = $"{-damage}" });
        }
    }
    public void Explode()
    {
        int particles = Engine.Random.Next(15, 25);
        for (int i = 0; i < particles; i++)
        {
            float angle = Engine.Random.NextSingle() * MathF.PI * 2;
            Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Engine.Random.NextSingle() * 2 + 2);
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), 0.25f, position, particleVelocity + velocity, angle, 0, 1, true, Color.Yellow, Color.Red));
        }
        particles = Engine.Random.Next(8, 16);
        for (int i = 0; i < particles; i++)
        {
            float angle = Engine.Random.NextSingle() * MathF.PI * 2;
            Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Engine.Random.NextSingle() * 2 + 2);
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.25f, position, particleVelocity + velocity, angle, 0, 1, true, Color.DarkSlateGray, Color.Black));
        }
        Engine.ShakeScreen(150 / ((position - Engine.Camera.Position).Length()+300));
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        if(!ChildEnemy && Player.SensingAbility > StealthAbility)
        {
            //Health bar
            Vector2 barPosition = position + new Vector2(-texture.Width * 2, texture.Height) / 2;
            Rectangle sourceRectangle = new (0, 0, texture.Width * 2, 2);
            Engine.DrawFilledLine(_spriteBatch, barPosition, sourceRectangle, (float)(health) / (float)(maxHealth), new Color(0, 50, 25), Color.Green);
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
    IEnumerable<int> Fighter()
    {
        enemyRange.radius = 250;
        float speed = 3;
        if (isFriendly)
        {
            speed = 7;
        }
        while (true)
        {
            velocity *= 0.8f;
            Vector2 normalizedAcceleration = GetNormalizedAcceleration();
            if (health <= 0)
            {
                Explode();
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), position);
                if (EntityManager.RandomWithKarma(Engine.EntityManager.CurrentMission.EnemiesSpawned * 2))
                {
                    Engine.EntityManager.Add(ItemFactory.NewScrap(position, normalizedAcceleration * 10, angularVelocity));
                }
            }
            Entity nearestAlly = Engine.EntityManager.NearestAlly(this);
            if (nearestAlly != null)
            {
                if((position - nearestAlly.position).LengthSquared() < 0.001f)
                {
                    position += new Vector2(0,0.1f);
                }
                Vector2 relativePosition = nearestAlly.position - position;
                position -= Size.Length() * Vector2.Normalize(relativePosition) / (MathF.Sqrt(relativePosition.Length())) / 10;

            }
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
                velocity += normalizedAcceleration / 2 * Engine.DeltaSeconds * 60;
                if (cooldown <= 0)
                {
                    Engine.EntityManager.Add(new PulseShot(position, Engine.ToUnitVector(angle) * 8, angle, 0, isFriendly, damage, true));
                    SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
                    cooldown = 1;
                }
            }
            yield return 0;
        }
    }
    IEnumerable<int> Carrier()
    {
        enemyRange.radius = 500;
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
                velocity += gravityForce / 2 * Engine.DeltaSeconds * 60;
                if (cooldown <= 0)
                {
                    Engine.EntityManager.Add(NewMissile(position, Engine.ToUnitVector(angle) * 2, angle, isFriendly));
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
                if (EntityManager.RandomWithKarma(Engine.EntityManager.CurrentMission.EnemiesSpawned))
                {
                    Engine.EntityManager.Add(ItemFactory.NewScrap(position, GetNormalizedAcceleration() * 10, angularVelocity));
                }
            }
            velocity *= 0.8f;
            yield return 0;
        }
    }
    IEnumerable<int> Sniper()
    {
        enemyRange.radius = 400;
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
                velocity += gravityForce / 2 * Engine.DeltaSeconds * 60;
                if (cooldown <= 0 && MathF.Abs(targetAngle - angle) < 0.1f)
                {
                    Engine.EntityManager.Add(new AssassinShot(position, Engine.ToUnitVector(angle) * 15, angle, 0, false, damage));
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
                if (EntityManager.RandomWithKarma(Engine.EntityManager.CurrentMission.EnemiesSpawned))
                {
                    Engine.EntityManager.Add(ItemFactory.NewScrap(position, gravityForce * 10, angularVelocity));
                }
            }
            velocity *= 0.8f;
            yield return 0;
        }
    }
    IEnumerable<int> Missile()
    {
        enemyRange.radius = 10;
        entityType = EntityType.Projectile;
        float fuel = 45;
        float deathCooldown = 2;
        deleteOnCollide = true;
        ParticleEmitter engineParticles = new(Assets.Get(Sprite.Dot), 0.15f, Vector2.Zero, 0, 45, 2, 0, 
            450f, 1, true, Color.Yellow, Color.DarkRed, EmitterType.EmissionOverTime) { isEmitterActive = false };
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
                Explode();
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), position);
            }
            Entity nearestEnemy = Engine.EntityManager.NearestEnemy(this);
            nearestEnemy ??= NewDummyEnemy(position + 100 * new Vector2(MathF.Cos(angle- MathF.PI / 2), MathF.Sin(angle - MathF.PI/2)));

            targetVector = Vector2.Normalize(nearestEnemy.position - position);
            targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
            Vector2 normalAcceleration = Vector2.Normalize(new Vector2(velocity.Y, -velocity.X));
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
            engineParticles.sprayAngle = angle + MathF.PI;
            engineParticles.position = position + new Vector2(-MathF.Sin(angle), MathF.Cos(angle)) * 4;
            engineParticles.Update();
            if (EntityManager.DistanceSqr(this, nearestEnemy) < 10 * 10)
            {
                nearestEnemy.Collide(8);
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
        enemyRange.radius = 200;
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

            if (health <= 0)
            {
                Explode();
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), position);
                if (EntityManager.RandomWithKarma(Engine.EntityManager.CurrentMission.EnemiesSpawned * 2))
                {
                    Engine.EntityManager.Add(ItemFactory.NewScrap(position, GetNormalizedAcceleration() * 10, angularVelocity));
                }
            }
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
    IEnumerable<int> Symmetry()
    {
        enemyRange.radius = 500;
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
                playerIterativePosition += (Player.velocity-velocity) * (timeToHit - prevTimeToHit);
                prevTimeToHit = timeToHit;
            }
            targetVector = playerIterativePosition - position;
            targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
            Vector2 gravityForce = GetNormalizedAcceleration();
            float theta = MathF.Atan2(gravityForce.Y, gravityForce.X);
            
            GoToPosition(Player.position + 100 * new Vector2(MathF.Cos(theta), MathF.Sin(theta)), speed);
            if(!hasLaunchedAllies && ((float)health / (float)maxHealth < 0.5f))
            {
                hasLaunchedAllies = !hasLaunchedAllies;
                Engine.EntityManager.Add(NewShield(this, 12, 25, 0, 0));
                SoundManager.PlaySound(Assets.Get(Sound.MissileFire), position);
            }
            if(EntityManager.DistanceSqr(this, Player) < 500 * 500)
            {
                velocity += GetNormalizedAcceleration() * Engine.DeltaSeconds * 10;
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
                else if(missileCooldown < 0)
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
                Explode();
                int particles = Engine.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Engine.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Engine.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.5f, position, particleVelocity + velocity, angle, 0, 1, true, Color.Yellow, Color.Red));
                }
                particles = Engine.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Engine.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Engine.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.5f, position, particleVelocity + velocity, angle, 0, 1, true, Color.DarkSlateGray, Color.Black));
                }
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), position);
                Engine.EntityManager.Add(ItemFactory.NewScrap(position, Engine.ToUnitVector(angle) * 5, angularVelocity));
                Engine.EntityManager.Add(ItemFactory.NewScrap(position, Engine.ToUnitVector(angle + MathF.PI/6) * 5, angularVelocity));
                Engine.EntityManager.Add(ItemFactory.NewScrap(position, Engine.ToUnitVector(angle - MathF.PI / 6) * 5, angularVelocity));
            }
            velocity *= 0.8f;
            yield return 0;
        }
    }
    IEnumerable<int> Overload()
    {
        enemyRange.radius = 10;
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
        foreach(Enemy shield in shields)
        {
            Engine.EntityManager.Add(shield);
        }
        while(true)
        {
            if (shields[0].isExpired && shields[1].isExpired && shields[2].isExpired && shields[3].isExpired)
            {
                if(chargeCooldown < 5)
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
            else if(chargeCooldown < 0)
            {
                velocity *= 0.9f;
                Vector2 gravityForce = GetNormalizedAcceleration();
                float theta = MathF.Atan2(gravityForce.X, -gravityForce.Y);
                if(chargingWindup == 1)
                {
                    chargeLocation = Player.position + 50 * new Vector2(MathF.Cos(theta), MathF.Sin(theta));
                }
                GoToPosition(chargeLocation, 1 + 2 * MathF.Sqrt((chargeLocation - position).Length()));
                if((chargeLocation - position).LengthSquared() < 200)
                {
                    if(chargingWindup <= 0)
                    {
                        Vector2 relativePosition = position - Player.position;
                        velocity -= Vector2.Normalize(relativePosition) * 15;
                        chargeCooldown = 15;
                        chargingWindup = 1;
                    }
                    else
                    {
                        float sum = 2 - chargingWindup;
                        angle = sum*sum*sum*sum;
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
                Vector2 normalAcceleration = Vector2.Normalize(new Vector2(velocity.Y - 0.001f, -velocity.X));
                float closingVelocity = Vector2.Dot(targetVector, velocity);
                Vector2 futureTargetVector = (Player.position + Player.velocity) - (position + velocity);
                float angleRateOfChange = (targetAngle - MathF.Atan2(futureTargetVector.X, -futureTargetVector.Y)) / Engine.DeltaSeconds;
                Vector2 accelerationVector = normalAcceleration * closingVelocity * angleRateOfChange * Engine.DeltaSeconds * 2;
                Vector2 normalAccelerationVector = Vector2.Normalize(accelerationVector);
                if (accelerationVector.LengthSquared() > 0.75f)
                {
                    accelerationVector = normalAccelerationVector * 0.75f;
                }
                velocity += accelerationVector + GetNormalizedAcceleration() / 5;
                angularVelocity = velocity.Length() / 100;
                if (MathF.Abs(angleRateOfChange) < 0.5f && Vector2.Dot(targetVector, velocity) < 10)
                {
                    Vector2 thrustForce = targetVector * 8;
                    velocity += thrustForce * Engine.DeltaSeconds;
                }
                if (EntityManager.DistanceSqr(this, Player) < 10 * 10)
                {
                    Player.Collide(damage);
                    velocity = Player.velocity - velocity/2;
                }
            }
            if(shieldCooldown >= maxShieldCooldown)
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
                Engine.EntityManager.Add(ItemFactory.NewScrap(position, Engine.ToUnitVector(angle) * 5, angularVelocity));
                Engine.EntityManager.Add(ItemFactory.NewScrap(position, Engine.ToUnitVector(angle + MathF.PI / 6) * 5, angularVelocity));
                Engine.EntityManager.Add(ItemFactory.NewScrap(position, Engine.ToUnitVector(angle - MathF.PI / 6) * 5, angularVelocity));
                SoundManager.PlaySound(Assets.Get(Sound.Death), position);
            }
            yield return 0;
        }
    }
    IEnumerable<int> Mothership()
    {
        enemyRange.radius = 300;
        float furnaceCooldown = 15;
        float craftingCooldown = 12;
        int requiredCraftsLeft = 20;
        float cooldown = 3;
        Pickup furnaceItem = null;
        bool currentlyCrafting = false;
        while (true)
        {
            velocity = Vector2.Zero;
            if (EventHandler.AcknowledgeMessage(Message.MothershipCraftItem))
            {
                currentlyCrafting = true;
            }
            if(EventHandler.AcknowledgeMessage(Message.MothershipUpdateFurnace))
            {
                furnaceItem = ((ItemSlot<Pickup>)Engine.UIManager.GetFuncWidget((int)Containers.MothershipMenu, 1)).daughterItem;
            }
            if(EventHandler.AcknowledgeMessage(Message.MothershipUpdateInventory))
            {
                var dockableComponent = Components.GetComponent(ComponentType.DockableComponent);
                if (dockableComponent.IsValid) { (dockableComponent as DockableComponent).SetInventory(Engine.InventorySlots); }
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
                        if (relativePosition.Length() < enemyRange.radius)
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
                Engine.EntityManager.CurrentMission.CompleteCustomRule(this);
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
            turretCannon.position = position + new Vector2(0, -8);
            Entity nearestPickup = Engine.EntityManager.NearestItem(this);
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
            }
            if (turretCannon.health != health)
            {
                health = Math.Min(turretCannon.health, health);
                turretCannon.health = health;
            }
            yield return 0;
        }
    }
    IEnumerable<int> TurretCannon()
    {
        float targetingAngle = 0;
        float turretAngle = 0;
        float bulletOffset = 3;
        enemyRange.radius = 400;
        ChildEnemy = true;
        while (true)
        {
            velocity *= 0;
            targetingAngle += Engine.DeltaSeconds;
            Entity nearestEnemy = Engine.EntityManager.NearestEnemy(NewDummyEnemy(position - new Vector2(0, Size.Y / 2 + 150), isFriendly));
            enemyRange.position = position + new Vector2(0, Size.Y / 2);
            if (nearestEnemy != null)
            {

                float distance = (nearestEnemy.position - position - new Vector2(0, Size.Y / 2)).Length();
                Vector2 relativePosition = nearestEnemy.position - position - new Vector2(0, Size.Y / 2) + nearestEnemy.velocity * distance / 8;
                float targetAngle = MathF.Atan2(relativePosition.Y, relativePosition.X) + MathF.PI / 2;
                if (turretAngle - targetAngle > 0.1f && targetAngle > -1.7f)
                {
                    turretAngle -= 10 * Engine.DeltaSeconds;
                }
                else if (turretAngle - targetAngle < -0.1f && targetAngle < 1.7f)
                {
                    turretAngle += 10 * Engine.DeltaSeconds;
                }
                if (distance < 400 && cooldown <= 0 && MathF.Abs(turretAngle - targetAngle) < 0.1f)
                {
                    Vector2 normalVector = Vector2.Normalize(relativePosition);
                    var rotatedOffset = new Vector2(-Size.Y / 2 * MathF.Sin(angle), Size.Y / 2 * MathF.Cos(angle));
                    //EntityManager.Add(new PulseShot(position - new Vector2(0, Size.Y / 2) + new Vector2(normalVector.Y, -normalVector.X) * bulletOffset * 2, normalVector * 8, turretAngle, 0, isFriendly, 5));
                    Engine.EntityManager.Add(NewMissile(position - rotatedOffset + new Vector2(normalVector.Y, -normalVector.X) * bulletOffset, normalVector * 8, turretAngle, isFriendly));
                    Assets.Get(Sound.MissileFire).Play();
                    //Assets.Get(Sound.PulseFire).Play();
                    cooldown = 0.9f;
                    bulletOffset *= -1;
                }
            }
            if (cooldown > 0)
            {
                cooldown -= Engine.DeltaSeconds;
            }
            angle = turretAngle;
            yield return 0;
        }
    }
    IEnumerable<int> Miner()
    {
        ParticleEmitter miningDebris = new(Assets.Get(Sprite.Dot), 0.15f, position + new Vector2(0, Assets.Get(Sprite.Miner).Height / 2), 0, 90, 2,
            Engine.Random.NextSingle() - 0.5f, 1000, 0, true, Color.Cyan, Color.Black, EmitterType.EmissionOverTime);
        while (true)
        {
            velocity *= 0;
            if (health <= 0)
            {
                Explode();
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), position);
            }
            Entity nearestPickup = Engine.EntityManager.NearestItem(this);
            if (nearestPickup != null)
            {
                if (EntityManager.DistanceSqr(this, nearestPickup) < 2500 && health <= maxHealth -15)
                {
                    Collide(-15);
                    nearestPickup.isExpired = true;
                    SoundManager.PlaySound(Assets.Get(Sound.Dock), position);
                }
            }
            miningDebris.Update();
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
        enemyRange.radius = 250;
        ParticleEmitter engineParticles = new(Assets.Get(Sprite.Dot), 0.15f, Vector2.Zero, 0, 45, 2, 0,
            450f, 1, true, Color.Yellow, Color.DarkRed, EmitterType.EmissionOverTime);
        while (true)
        {
            Entity nearestEnemy = Engine.EntityManager.NearestEnemy(this);
            Vector2 relativePosition = position - nearestEnemy.position;
            Vector2 normalizedAcceleration = GetNormalizedAcceleration();
            Vector2 Offset = Vector2.Normalize(Engine.EntityManager.CurrentMission.GetNormalizedAcceleration(nearestEnemy.position))*100;
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
            if (health <= 0)
            {
                Explode();
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), position);
                if (EntityManager.RandomWithKarma(Engine.EntityManager.CurrentMission.EnemiesSpawned * 2))
                {
                    Engine.EntityManager.Add(ItemFactory.NewScrap(position, normalizedAcceleration * 10, angularVelocity));
                }
            }
            if (Vector2.Distance(position, nearestEnemy.position) < enemyRange.radius)
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
            RotateTowards(targetAngle);
            LowerCooldown();
            velocity += (new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * thrust) * Engine.DeltaSeconds;
            engineParticles.position = position;
            engineParticles.offsetVelocity = velocity;
            engineParticles.sprayAngle = angle * 180 / MathF.PI + 180;
            engineParticles.speedOfEmission = thrust * 100;
            engineParticles.particleVelocity = 3 - 3 / (thrust + 1);
            engineParticles.Update();
            yield return 0;
        }
    }
    IEnumerable<int> Excursion()
    {
        enemyRange.radius = 500;
        int currentWave = 0;
        int currentEnemy = 0;
        float waveTimer = 0;
        Entity nearestPickup;
        int bulletOffset = 8;
        float laserCooldown = 0;
        float laserWindup = 3;
        Func<Vector2, Vector2, float, bool, Enemy>[][] waves = 
        [
            [ NewHovercraft, NewHovercraft, ],
            [ NewCarrier, NewHovercraft, NewHovercraft, NewHovercraft, NewHovercraft ],
            [ NewCarrier, NewSniper, NewSniper, NewSniper, NewHovercraft, NewHovercraft, NewHovercraft, ],
            [ NewCarrier, NewSniper, NewSniper, NewHovercraft, NewHovercraft, NewHovercraft, NewShotgunner, NewCarrier, ],
        ];
        while (true)
        {
            nearestPickup = Engine.EntityManager.NearestItem(this);
            Vector2 relativePosition = Player.position + Player.velocity - position - velocity;
            targetAngle = MathF.Atan2(relativePosition.X, -relativePosition.Y);
            if (health >= maxHealth / 2 || laserCooldown > 0)
            {
                if (nearestPickup != null)
                {
                    GoToPosition(nearestPickup.position, 5);
                    if (EntityManager.DistanceSqr(this, nearestPickup) < 25 * 25)
                    {
                        nearestPickup.isExpired = true;
                        Collide(-15);
                    }
                }
                else
                {
                    Vector2 targetLocation = Player.position + Vector2.Normalize(Player.velocity) * 250 + Engine.EntityManager.CurrentMission.GetNormalizedAcceleration(Player.position) * 50;
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
            else if (EntityManager.DistanceSqr(Player, this) < 300*300)
            {
                if(cooldown <= 0)
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
            if ((float)health / (float)maxHealth < 0.8f - currentWave*0.2f && currentWave < waves.Length)
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
                Explode();
                Engine.EntityManager.Add(ItemFactory.NewScrap(position, Engine.ToUnitVector(angle) * 5, angularVelocity));
                Engine.EntityManager.Add(ItemFactory.NewScrap(position, Engine.ToUnitVector(angle + MathF.PI / 6) * 5, angularVelocity));
                Engine.EntityManager.Add(ItemFactory.NewScrap(position, Engine.ToUnitVector(angle - MathF.PI / 6) * 5, angularVelocity));
                int particles = Engine.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Engine.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Engine.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.5f, position, particleVelocity + velocity, angle, 0, 1, true, Color.Yellow, Color.Red));
                }
                particles = Engine.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Engine.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Engine.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.5f, position, particleVelocity + velocity, angle, 0, 1, true, Color.DarkSlateGray, Color.Black));
                }
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), position);
            }
            velocity *= 0.8f;
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
                furnaceItem = ((ItemSlot<Pickup>)Engine.UIManager.GetFuncWidget((int)Containers.MothershipMenu, 1)).daughterItem;
            }
            if (EventHandler.AcknowledgeMessage(Message.MothershipUpdateInventory))
            {
                var dockableComponent = Components.GetComponent(ComponentType.DockableComponent);
                if (dockableComponent.IsValid)
                {
                    (dockableComponent as DockableComponent).SetInventory(Engine.InventorySlots);
                }
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
    IEnumerable<int> Wyvern(Enemy _parent)
    {
        enemyRange.radius = 500;
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
                if (!hasExploded) 
                {
                    Explode();
                    SoundManager.PlaySound(Assets.Get(Sound.Death), position);
                    if (tail1 == null && tail2 == null)
                    {
                        isExpired = true;
                    }
                    else
                    {
                        hasExploded = true;
                        Engine.EntityManager.Add(ItemFactory.GetItem(ModuleType.LMG, position, normalizedAcceleration * 10, angularVelocity));
                        position = new Vector2(10000, 10000);
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
                        angle = MathF.Atan2(relativep1.Y, relativep1.X) + MathF.PI/2;
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
                    if((nearestEnemy.position - position).Length() < 500)
                    {
                        if (cooldown <= 0)
                        {
                            if ((tail1 == null && tail2 == null))
                            {
                                Vector2 direction = Vector2.Normalize(nearestEnemy.position - position);
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
    IEnumerable<int> AdvancedFighter()
    {
        enemyRange.radius = 500;
        float tripleCooldown = 0;
        int shotCount = 0;
        while (true)
        {
            velocity *= 0.8f;
            Vector2 normalizedAcceleration = GetNormalizedAcceleration();
            if (health <= 0)
            {
                Explode();
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), position);
                if (EntityManager.RandomWithKarma(Engine.EntityManager.CurrentMission.EnemiesSpawned * 1.5f))
                {
                    Engine.EntityManager.Add(ItemFactory.NewScrap(position, normalizedAcceleration * 10, angularVelocity));
                }
            }
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
            float speed = 8 + Math.Max((Player.position - position).Length() - 500, 0) / 500;
            Entity nearestEnemy = Engine.EntityManager.NearestEnemy(this);
            if (nearestEnemy == null)
            {
                if ((Player.position - position).LengthSquared() > 1000)
                {
                    GoToPosition(Player.position, (speed + (Player.position - position).Length() / 100));
                }
                targetVector = Player.position - position + (Player.velocity - velocity) * 8;
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                yield return 0;
                continue;
            }

            Entity nearestProjectile = Engine.EntityManager.NearestProjectile(NewDummyEnemy(position + Vector2.Normalize(Player.position - position) * 30, isFriendly));
            if (nearestProjectile != null)
            {
                Vector2 relativePos = position - nearestProjectile.position;
                float relativeAngle = MathF.Atan2(relativePos.Y, relativePos.X) - MathF.Atan2(nearestProjectile.velocity.Y, nearestProjectile.velocity.X) % MathF.PI;
                //-15 to 15 degrees
                if (MathF.Abs(relativeAngle) < MathF.PI/12)
                {
                    Vector2 direction = Vector2.Normalize(relativePos);
                    velocity += new Vector2(direction.Y, -direction.X) * Math.Sign(-relativeAngle);
                }
            }
            float timeToHit;
            float prevTimeToHit = 0;
            Vector2 playerIterativePosition = nearestEnemy.position;
            for (int i = 0; i < 1; i++)
            {
                timeToHit = MathF.Sqrt(EntityManager.DistanceSqr(position, playerIterativePosition)) / 8;
                playerIterativePosition += Player.velocity * (timeToHit - prevTimeToHit);
                prevTimeToHit = timeToHit;
            }
            targetVector = (playerIterativePosition - position);
            //targetVector = nearestEnemy.position - position + (nearestEnemy.velocity - velocity) * 8;
            targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
            RotateTowards(targetAngle, 0.1f);
            LowerCooldown();
            if (EntityManager.DistanceSqr(this, nearestEnemy) > 500 * 500)
            {
                GoToPosition(nearestEnemy.position, speed);
            }
            else
            {
                velocity += normalizedAcceleration / 2 * Engine.DeltaSeconds * 60;
                if (cooldown <= 0)
                {
                    if (tripleCooldown <= 0)
                    {
                        Texture2D tex = Assets.Get(Sprite.Microshot);
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
    IEnumerable<int> PickupDrone()
    {
        bool currentlyLeaving = false;
        while (true)
        {
            if (EventHandler.AcknowledgeMessage(Message.EscapeDroneLeave))
            {
                currentlyLeaving = true;
                Engine.EntityManager.CurrentMission.CompleteCustomRule(this);
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
                position = position * (1f - Engine.DeltaSeconds) - new Vector2(0, Engine.EntityManager.CurrentMission.Planet.radius * 1.5f) * Engine.DeltaSeconds;
            }
            yield return 0;
        }
    }
    IEnumerable<int> StealthFighter()
    {
        enemyRange.radius = 500;
        SensingAbility = -1;
        StealthAbility = 0;
        Entity target = null;
        float trackTime = 0;
        Vector2 rand = position;
        while (true)
        {
            velocity += GetNormalizedAcceleration() * Engine.DeltaSeconds / 2 * 60;
            velocity *= 0.8f;
            if (cooldown > 0)
            {
                cooldown -= Engine.DeltaSeconds;
            }
            if (trackTime > 0) 
            { 
                trackTime -= Engine.DeltaSeconds;
                if (trackTime <= 0)
                {
                    target = null;
                }
            }
            if (health <= 0)
            {
                isExpired = true;
                Explode();
                if (EntityManager.RandomWithKarma(Engine.EntityManager.CurrentMission.EnemiesSpawned))
                {
                    Engine.EntityManager.Add(ItemFactory.NewScrap(position, velocity, angularVelocity));
                }
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
                    float radius = Engine.EntityManager.CurrentMission.Planet.radius;
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
    public static Enemy NewDummyEnemy(Vector2 _position, bool _isFriendly = false)
    {
        return new(_position, Vector2.Zero, 0, 0, 0, Assets.Get(Sprite.Fighter), _isFriendly);
    }
    public static Enemy NewFighter(Vector2 position, Vector2 velocity, float angle, bool _isFriendly = false)
    {
        Enemy enemy = new(position, velocity, angle, 5, 8, Assets.Get(Sprite.Fighter), _isFriendly);
        enemy.AddBehaviour(enemy.Fighter());
        return enemy;
    }
    public static Enemy NewCarrier(Vector2 position, Vector2 velocity, float angle, bool _isFriendly = false)
    {
        Enemy enemy = new(position, velocity, angle, 5, 15, Assets.Get(Sprite.Cruiser), _isFriendly);
        enemy.AddBehaviour(enemy.Carrier());
        return enemy;
    }
    public static Enemy NewSniper(Vector2 position, Vector2 velocity, float angle, bool _isFriendly = false)
    {
        Enemy enemy = new(position, velocity, angle, 8, 5, Assets.Get(Sprite.Sniper), _isFriendly);
        enemy.AddBehaviour(enemy.Sniper());
        return enemy;
    }
    public static Enemy NewMissile(Vector2 position, Vector2 velocity, float angle, bool _isFriendly = false)
    {
        Enemy enemy = new(position, velocity, angle, 8, 10, Assets.Get(Sprite.Missile), _isFriendly);
        enemy.AddBehaviour(enemy.Missile());
        return enemy;
    }
    public static Enemy NewShotgunner(Vector2 position, Vector2 velocity, float angle, bool _isFriendly = false)
    {
        Enemy enemy = new(position, velocity, angle, 5, 10, Assets.Get(Sprite.Shotgunner), _isFriendly);
        enemy.AddBehaviour(enemy.Shotgunner());
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
        enemy.Components.Add(new DockableComponent(enemy, Containers.MothershipMenu));
        return enemy;
    }
    public static Enemy NewTurret(Vector2 position, Vector2 velocity, float angle)
    {
        Enemy enemy = new(position, velocity, angle, 0, 400, Assets.Get(Sprite.TurretBase), true);
        enemy.AddBehaviour(enemy.Turret());
        return enemy;
    }
    public static Enemy NewTurretCannon(Enemy parent)
    {
        Enemy enemy = new(parent.position, parent.velocity, parent.angle, 6, 400, Assets.Get(Sprite.TurretHead), parent.isFriendly);
        enemy.AddBehaviour(enemy.TurretCannon());
        return enemy;
    }
    public static Enemy NewMiner(Vector2 position, Vector2 velocity, float angle)
    {
        Enemy enemy = new(position, velocity, angle, 0, 1000, Assets.Get(Sprite.Miner), true);
        enemy.AddBehaviour(enemy.Miner());
        return enemy;
    }
    public static Enemy NewHovercraft(Vector2 position, Vector2 velocity, float angle, bool isFriendly = false)
    {
        Enemy enemy = new(position, velocity, angle, 2, 12, Assets.Get(Sprite.Hovercraft), isFriendly);
        enemy.AddBehaviour(enemy.Hovercraft());
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
        return enemy;
    }
    public static Enemy NewOrbiter(Vector2 position, Vector2 velocity, float angle)
    {
        Enemy enemy = new(position, velocity, angle, 10, 300, Assets.Get(Sprite.Orbiter), true);
        enemy.AddBehaviour(enemy.Orbiter());
        enemy.Components.Add(new DockableComponent(enemy, Containers.MothershipMenu));
        enemy.angularVelocity = -0.01f;
        return enemy;
    }
    public static Enemy NewPickupDrone(Vector2 position, Vector2 velocity, float angle)
    {
        Enemy enemy = new(position, velocity, angle, 0, 250, Assets.Get(Sprite.PickupDrone), true);
        enemy.AddBehaviour(enemy.PickupDrone());
        enemy.Components.Add(new DockableComponent(enemy, Containers.PickupDroneMenu));
        return enemy;
    }
    public static Enemy NewStealthFighter(Vector2 position, Vector2 velocity, float angle, bool _isFriendly = false)
    {
        Enemy enemy = new(position, velocity, angle, 10, 8, Assets.Get(Sprite.Fighter), _isFriendly);
        enemy.AddBehaviour(enemy.StealthFighter());
        return enemy;
    }
}
