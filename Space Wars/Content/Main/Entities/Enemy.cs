using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Space_Wars.Content.Main.Components;
using Space_Wars.Content.Main.Particles;
using Space_Wars.Content.Main.Story;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UILib.Content.Main;
using Space_Wars.Content.Main.MissionComponents;

namespace Space_Wars.Content.Main.Entities;

public class Enemy : Entity
{
    private float targetAngle = 0;
    private int prevHealth;
    private float healthCD = 0;
    private float mineTime = 0;
    private SoundEffect hitSound;
    public bool ChildEnemy { get; private set; }
    public override int StealthAbility
    {
        get => (base.StealthAbility + (((RevealDuration > 0 || Health <= 0) ? -5 : 0) + StatusHolder.StealthChange));
        protected set => base.StealthAbility = value;
    }
    public override int SensingAbility
    {
        get => (base.SensingAbility + StatusHolder.SensingChange);
        protected set => base.SensingAbility = value;
    }
    public Vector2 NewGoToLocation()
    {
        Vector2 rand;
        do
        {
            rand = new Vector2((Util.Random.NextSingle() * 2 - 1) * 8 * 50 * 3, (Util.Random.NextSingle() * 2 - 1) * 8 * 50 * 3);
        }
        while (Engine.SaveGame.CurrentMission.IsColliding(rand, Vector2.Zero, ColliderRadius, false, out float _) == null);
        return rand;
    }
    public ParticleEmitter EnemyRange { get{ return GetComponent<FollowEmitter>().ParticleEmitter; } }
    public Enemy(Vector2 _position, Vector2 _velocity, float _angle, int _health, Texture2D _texture, Team _team = Team.Hostile)
        : base(_position, _velocity, _angle, 0)
    {
        AddComponent(new Stealth(this));
        AddComponent(new Temp(this));
        AddComponent(new Statuses(this));
        AddComponent(new Sprite(this) { Texture = _texture });
        AddComponent(new Friendly(this) { Team = _team });
        AddComponent(new Health(this) { CurrentHealth = _health, MaxHealth = _health });
        prevHealth = _health;
        hitSound = Assets.Get(Sound.Hit);
        AddComponent(new Collide(this, delegate(int damage, bool _ignoreImmunity)
        {
            damage = StatusHolder.ModifyDamage(damage);
            if (damage > 0)
            {
                healthCD = 0.5f;
                if(Health > 0)
                {
                    Flash(Color.White);
                }
                else
                {
                    Velocity += new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) + Vector2.Normalize(Position - Player.Position) * 0.3f * (float)damage;
                    AngularVelocity += Util.OneToNegOne()*0.05f * (float)damage;
                }
                ApplyWork(damage);
                SoundManager.PlaySound(hitSound, Position);
                Health -= damage;
                Engine.ShakeScreen(10 / ((Position - Engine.Camera.Position).Length() + 200) * damage);
                ParticleManager.Add(new Particle(null, 1, Position + new Vector2(0, -1), new Vector2(0, -1.5f), 0, 0, Color.Orange, new Color(255, 0, 0, 0)) { drawText = $"{damage}" });
                ParticleManager.Add(new Particle(Assets.Get(Sprites.Glow), 0.33f, Position, Vector2.Zero, 0, 0, Color.Wheat, Color.Transparent));
                RevealDuration = Math.Max(RevealDuration, 0.3f * MathF.Sqrt(damage));
                return true;
            }
            else if (damage < 0)
            {
                SoundManager.PlaySound(Assets.Get(Sound.Full), Position);
                Health -= damage;
                ParticleManager.Add(new Particle(null, 1, Position + new Vector2(0, -1), new Vector2(0, -1.5f), 0, 0, Color.Orange, new Color(0, 255, 0, 0)) { drawText = $"{-damage}" });
            }
            return false;
        }));
        AddComponent(new FollowEmitter(this) { ParticleEmitter = new(Assets.Get(Sprites.Dot), Position, 0, Color.Red * 0.75f) });
        AddComponent(new Cooldown(this));
        UpdateColor();
    }
    public override void Update()
    {
        if(Health > 0)
        {
            EnemyRange.isEmitterActive = SaveGame.DebugMode;
        }
        if (Angle - targetAngle >= Math.PI)
        {
            Angle -= MathF.PI * 2;
        }
        if (Angle - targetAngle <= -Math.PI)
        {
            Angle += MathF.PI * 2;
        }
        if(float.IsNaN(Velocity.X + Velocity.Y))
        {
            Velocity = Vector2.Zero;
            Engine.WriteLine("Velocity was NaN");
        }
        base.Update();
        if(healthCD <= 0)
        {
            if(prevHealth > Health)
            {
                prevHealth -= 1 + MaxHealth / 20;
                healthCD = 0.05f;
            }
            else
            {
                prevHealth = Health;
            }
        }
        else
        {
            healthCD -= Engine.DeltaSeconds;
        }
        float d = 1f;
        if(Health <= 0)
        {
            Team = Team.Dead;
            d = 0.67f;
        }
        Color c = SaveGame.ColorScheme.TeamColors[GetComponent<Friendly>().Team] * d; //Sets color based on friendlyness
        if (Color != c)
        {
            float l = Util.FIED(0.025f);
            Color = new Color((byte)(Color.R * l + c.R * (1f - l)), (byte)(Color.G * l + c.G * (1f - l)), (byte)(Color.B * l + c.B * (1f - l)), (byte)(c.A)); //Lerp towards ideal color
        }
    }
    public override void UpdateColor()
    {
        Color = SaveGame.ColorScheme.TeamColors[Team];
    }
    private Vector2 GetNormalizedAcceleration()
    {
        return Engine.SaveGame.CurrentMission.GetNormalizedAcceleration(Position);
    }
    public void RotateTowards(float _angle, float _maxSpeed = 0.05f)
    {
        float angleDiff = Math.Abs(_angle - Angle);
        if (Angle > _angle && AngularVelocity > -_maxSpeed)
        {
            Angle -= _maxSpeed * (angleDiff + 0.25f);
        }
        if (Angle < _angle && AngularVelocity < _maxSpeed)
        {
            Angle += _maxSpeed * (angleDiff + 0.25f);
        }
    }
    public void GoToPosition(Vector2 _position, float speed)
    {
        var collider = Engine.SaveGame.CurrentMission.IsColliding(Position, _position - Position, ColliderRadius, false, out float _);
        //TODO: Implement A# algorithm
        if(collider != null)
        {
            return;
        }
        var targetVector = Vector2.Normalize(Vector2.Normalize(_position - Position) + GetNormalizedAcceleration() * 20f);
        Velocity += targetVector * speed * Engine.DeltaSeconds * 10;
    }
    private void DrawLine(float _angle, float _cooldown, float _maxCooldown)
    {
        Vector2 dir = Util.ToUnitVector(_angle);
        float cd = (1 - _cooldown / _maxCooldown) * (1 - _cooldown / _maxCooldown);
        for (int i = 0; i < 500; i++)
        {
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), Position + dir * (i * 2 + 8), _angle, Color.Red * (1 - (float)(i) / 500f) * cd));
        }
    }
    public void Mine()
    {
        if (Health <= 0)
        {
            mineTime += Engine.DeltaSeconds;
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 0.5f, Position, new Vector2(Util.OneToNegOne(), Util.OneToNegOne()), Util.OneToNegOne() * MathF.PI, Util.OneToNegOne() / 2, Color.Yellow, Color.Transparent));
        }
    }
    public void Explode(int _damage, float _radius)
    {
        Util.Explode(Position, Velocity, _damage, _radius);
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        if (!ChildEnemy)
        {
            float val = (Engine.SaveGame.Player.SensingAbility > base.StealthAbility ? 1 : 0);
            if (Engine.SaveGame.Player.SensingAbility <= base.StealthAbility)
            {
                val = Math.Clamp(val + RevealDuration, 0, 1);
            }
            if (GetComponent<Health>() != null && Health > 0)
            {
                //Health bar
                Vector2 barPosition = Position + new Vector2(-Texture.Width * 2, Texture.Height) / 2;
                Rectangle sourceRectangle = new(0, 0, Texture.Width * 2, 2);
                _spriteBatch.Draw(Engine.Line, barPosition, sourceRectangle, new Color(0, 50, 25) * val);
                _spriteBatch.Draw(Engine.Line, barPosition, new Rectangle(sourceRectangle.Location, new Point((int)(sourceRectangle.Width * (float)(prevHealth) / (float)(MaxHealth)), sourceRectangle.Height)), Color.White * val);
                _spriteBatch.Draw(Engine.Line, barPosition, new Rectangle(sourceRectangle.Location, new Point((int)(sourceRectangle.Width * (float)(Health) / (float)(MaxHealth)), sourceRectangle.Height)), Color.Green * val);
            }
        }
        Vector2 halfSize = Engine.BackBuffer / 2;
        if (!ChildEnemy && IsFriendly(Engine.SaveGame.Player) &&
           (Position.X - Engine.Camera.Position.X + Size.X / 2 < -halfSize.X || Position.Y - Engine.Camera.Position.Y + Size.Y / 2 < -halfSize.Y
         || Position.X - Engine.Camera.Position.X - Size.X / 2 > halfSize.X || Position.Y - Engine.Camera.Position.Y - Size.Y / 2 > halfSize.Y))
        {
            var pos = Position - Engine.SaveGame.Player.Position;
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), pos / 50 + Engine.SaveGame.Player.Position, 0, Color * 0.67f));
        }
        base.Draw(_spriteBatch);
    }
    #region Useful Behaviors
    IEnumerable<int> EnemyDeath()
    {
        bool hasExploded = false;
        while (true)
        {
            if (Health <= 0)
            {
                if (!hasExploded)
                {
                    Explode(4, ColliderRadius);
                    hasExploded = true;
                    SoundManager.PlaySound(Assets.Get(Sound.Death), Position);
                }
                Velocity *= Util.FIED(0.05f);
                AngularVelocity *= Util.FIED(0.1f);
            }
            if (mineTime > MathF.Sqrt((float)(MaxHealth) / 8))
            {
                isExpired = true;
                for (float angle = 0; angle < MathF.Tau; angle += MathF.PI / 4)
                {
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 0.5f, Position, Util.ToUnitVector(angle) * 2, angle, 0, Color.Yellow, Color.Transparent));
                }
            }
            yield return 0;
        }
    }
    public IEnumerable<int> DropItem(Func<Vector2, Vector2, float, Pickup> _action)
    {
        while(true)
        {
            if (isExpired)
            {
                Engine.EntityManager.Add(_action(Position, GetNormalizedAcceleration() * 10, AngularVelocity));
            }
            yield return 0;
        }
    }
    IEnumerable<int> AvoidNearbyAllies()
    {
        while (Health > 0)
        {
            Entity nearestAlly = Engine.EntityManager.NearestAlly(this);
            if (nearestAlly != null)
            {
                if ((Position - nearestAlly.Position).LengthSquared() < 0.001f)
                {
                    Position += new Vector2(0, 0.1f);
                }
                Vector2 relativePosition = nearestAlly.Position - Position;
                Position -= Size.Length() * Vector2.Normalize(relativePosition) / (MathF.Sqrt(relativePosition.Length())) / 10;
            }
            yield return 0;
        }
    }
    public IEnumerable<int> AvoidProjectiles(float _strength)
    {
        while (Health > 0)
        {
            Entity nearestProjectile = Engine.EntityManager.NearestProjectile(Position + Vector2.Normalize(Player.Position - Position) * 30, SensingAbility, Team);
            if (nearestProjectile != null)
            {
                var pos = Vector2.Normalize(Position - nearestProjectile.Position);
                var vel = Vector2.Normalize(Velocity - nearestProjectile.Velocity);
                //Enemies can only see projectiles 180 degrees ahead of them
                if ((pos.X * vel.X + pos.Y * vel.Y) < -0.5f && Vector2.Dot(-pos, Util.ToUnitVector(Angle)) > 0)
                {
                    int sign = Math.Sign(pos.X * vel.Y - vel.X * pos.Y);
                    if (sign == 0)
                    {
                        sign = 1;
                    }
                    Velocity += new Vector2(-pos.Y, pos.X) * sign * _strength;
                }
            }
            yield return 0;
        }
    }
    //Simulate worm based enemies
    //Use the SpawnWorm behavior on the head for proper functioning
    IEnumerable<int> FollowNextSegment(Enemy parent)
    {
        ChildEnemy = true;
        while (Health > 0 && parent != null)
        {
            Velocity = Vector2.Zero;
            if (parent != null)
            {
                if (parent.Health <= 0)
                {
                    parent = null;
                    ChildEnemy = false;
                }
                else
                {
                    Vector2 p1 = parent.Position - new Vector2(MathF.Sin(parent.Angle), -MathF.Cos(parent.Angle)) * parent.Texture.Height / 2;
                    Vector2 relativep1 = p1 - Position;
                    if (relativep1.LengthSquared() > float.Epsilon)
                    {
                        Angle = Util.ToAngle(relativep1);
                    }
                    Position = p1 - new Vector2(MathF.Sin(Angle), -MathF.Cos(Angle)) * Texture.Height / 2;
                }
            }
            yield return 0;
        }
    }
    static IEnumerable<int> SpawnWorm(List<Enemy> segments)
    {
        foreach (var enemy in segments)
        {
            Engine.EntityManager.Add(enemy);
        }
        yield return 0;
    }
    #endregion
    #region Bosses
    IEnumerable<int> SymmetryBoss()
    {
        int damage = 6;
        EnemyRange.particleVelocity = 500;
        CD =
        [
            0, //Default
            10, //Missile cd
        ];
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
            var nearestEnemy = Engine.EntityManager.NearestEnemy(this);
            if(nearestEnemy != null)
            {
                float speed = (nearestEnemy.Position - Position).Length() / 75;
                float timeToHit;
                float prevTimeToHit = 0;
                Vector2 playerIterativePosition = nearestEnemy.Position;
                for (int i = 0; i < 5; i++)
                {
                    timeToHit = MathF.Sqrt(EntityManager.DistanceSqr(Position, playerIterativePosition)) / (8);
                    playerIterativePosition += (nearestEnemy.Velocity - Velocity) * (timeToHit - prevTimeToHit);
                    prevTimeToHit = timeToHit;
                }
                Vector2 targetVector = playerIterativePosition - Position;
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                Vector2 gravityForce = GetNormalizedAcceleration();
                float theta = MathF.Atan2(gravityForce.Y, gravityForce.X);

                GoToPosition(nearestEnemy.Position + 100 * new Vector2(MathF.Cos(theta), MathF.Sin(theta)), speed);
                if (!hasLaunchedAllies && ((float)Health / (float)MaxHealth < 0.5f))
                {
                    hasLaunchedAllies = !hasLaunchedAllies;
                    Engine.EntityManager.Add(NewShield(this, 12, 25, 0, 0, Team));
                    SoundManager.PlaySound(Assets.Get(Sound.MissileFire), Position);
                }
                if (EntityManager.DistanceSqr(this, nearestEnemy) < 500 * 500)
                {
                    Velocity += gravityForce * Engine.DeltaSeconds * 60;
                    if (CD[0] <= 0 && missileCooldown > 0)
                    {
                        if (bulletCount < 2)
                        {
                            CD[0] = 0.1f;
                            bulletCount += 1;
                        }
                        else
                        {
                            CD[0] = (float)MaxHealth / (MaxHealth * 2 - Health);
                            bulletCount = 0;
                        }
                        Vector2 direction = Util.ToUnitVector(Angle);
                        Engine.EntityManager.Add(NewPulseShot(Position, Velocity + direction * 8, Angle, 0, Team, damage, true));
                        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Position);
                    }
                    else if (missileCooldown < 0)
                    {
                        if (missileCount < 3 - (int)(3 * Health / MaxHealth))
                        {
                            CD[0] += 0.25f;
                            missileCount += 1;
                            missileCooldown = 0.25f;
                        }
                        else
                        {
                            CD[0] += 2f;
                            missileCount = 0;
                            missileCooldown = 10;
                        }
                        Vector2 direction = Util.ToUnitVector(Angle + MathF.PI / 2 + MathF.PI * missileCount);
                        Engine.EntityManager.Add(NewMissile(Position, direction * 2 + Velocity, Angle, Team));
                        SoundManager.PlaySound(Assets.Get(Sound.MissileFire), Position);
                    }
                }
                RotateTowards(targetAngle);
            }

            if (Health <= 0)
            {
                Explode(6, ColliderRadius);
                int particles = Util.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Util.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Util.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), 0.5f, Position, particleVelocity + Velocity, angle, 0, Color.Yellow, new Color(255, 0, 0, 0)));
                }
                particles = Util.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Util.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Util.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), 0.5f, Position, particleVelocity + Velocity, angle, 0, Color.DarkSlateGray, Color.Transparent));
                }
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), Position);
                if(Team == Team.Hostile)
                {
                    if (Engine.SaveGame.GiveWeapon)
                    {
                        Engine.EntityManager.Add(new Missile() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 10, AngularVelocity = this.AngularVelocity });
                    }
                    else
                    {
                        Engine.EntityManager.Add(new SummonShield() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 10, AngularVelocity = this.AngularVelocity });
                    }
                }
            }
            Velocity *= 0.8f;
            yield return 0;
        }
    }
    public static Enemy NewSymmetryBoss(Vector2 position, Vector2 velocity, float angle, Team _team = Team.Hostile)
    {
        Enemy enemy = new(position, velocity, angle, 100, Assets.Get(Sprites.SymmetryBoss), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.SymmetryBoss()));
        return enemy;
    }
    IEnumerable<int> OverloadBoss()
    {
        int damage = 10;
        EnemyRange.particleVelocity = 10;
        float octoshotCooldown = 10;
        int octoshots = 0;
        float maxShieldCooldown = 3.33f;
        float chargeCooldown = 5;
        float chargingWindup = 1;
        float shieldCooldown = maxShieldCooldown;
        Vector2 chargeLocation = Vector2.Zero;
        Enemy[] shields =
        [
            NewShield(this, 14, 8, 0, 1, Team),
            NewShield(this, 14, 8, MathF.PI, 1, Team),
            NewShield(this, 14, 8, MathF.PI/2, 1, Team),
            NewShield(this, 14, 8, MathF.PI*3/2, 1, Team)
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
                Velocity *= 0.985f;
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
                        shields[i].Health = shields[i].MaxHealth;
                        shields[i].Position = Position;
                        Engine.EntityManager.Add(shields[i]);
                    }
                }
            }
            else if (octoshotCooldown < 0 && octoshots <= 16)
            {
                Velocity *= 0.9f;
                float speed = Velocity.Length();
                if (speed < 0.5f)
                {
                    Velocity *= 0.75f;
                }
                if (speed < 0.5f)
                {
                    Velocity *= 0.25f;
                }
                if (speed < 0.05f)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 shotDirection = new(MathF.Sin(targetAngle), -MathF.Cos(targetAngle));
                        Engine.EntityManager.Add(NewPulseShot(Position, Velocity + shotDirection * 8, targetAngle, 0, Team, 5));
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
                Velocity *= 0.9f;
                Vector2 gravityForce = GetNormalizedAcceleration();
                float theta = MathF.Atan2(gravityForce.X, -gravityForce.Y);
                if (chargingWindup == 1)
                {
                    chargeLocation = Player.Position + 50 * new Vector2(MathF.Cos(theta), MathF.Sin(theta));
                }
                GoToPosition(chargeLocation, 1 + 2 * MathF.Sqrt((chargeLocation - Position).Length()));
                if ((chargeLocation - Position).LengthSquared() < 200)
                {
                    if (chargingWindup <= 0)
                    {
                        Vector2 relativePosition = Position - Player.Position;
                        Velocity -= Vector2.Normalize(relativePosition) * 15;
                        chargeCooldown = 15;
                        chargingWindup = 1;
                    }
                    else
                    {
                        float sum = 2 - chargingWindup;
                        Angle = sum * sum * sum * sum;
                        chargingWindup -= Engine.DeltaSeconds;
                    }
                }
            }
            else
            {
                octoshotCooldown -= Engine.DeltaSeconds;
                chargeCooldown -= Engine.DeltaSeconds;

                var targetVector = Vector2.Normalize(Player.Position - Position);
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                var normalAcceleration = Vector2.Normalize(new Vector2(Velocity.Y - 0.001f, -Velocity.X));
                float closingVelocity = Vector2.Dot(targetVector, Velocity);
                var futureTargetVector = (Player.Position + Player.Velocity) - (Position + Velocity);
                float angleRateOfChange = (targetAngle - MathF.Atan2(futureTargetVector.X, -futureTargetVector.Y)) / Engine.DeltaSeconds;
                var accelerationVector = normalAcceleration * closingVelocity * angleRateOfChange * Engine.DeltaSeconds * 2;
                var normalAccelerationVector = Vector2.Normalize(accelerationVector);
                if (accelerationVector.LengthSquared() > 0.75f)
                {
                    accelerationVector = normalAccelerationVector * 0.75f;
                }
                Velocity += accelerationVector + GetNormalizedAcceleration() * 2;
                AngularVelocity = Velocity.Length() / 100;
                if (MathF.Abs(angleRateOfChange) < 0.5f && Vector2.Dot(targetVector, Velocity) < 10)
                {
                    Vector2 thrustForce = targetVector * 8;
                    Velocity += thrustForce * Engine.DeltaSeconds;
                }
                if (EntityManager.DistanceSqr(this, Player) < 10 * 10)
                {
                    Player.Collide(damage);
                    Velocity = Player.Velocity - Velocity / 2;
                }
            }
            if (shieldCooldown >= maxShieldCooldown)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = (i * 2 * MathF.PI) / 6;
                    Particle particle = new(Assets.Get(Sprites.Dot), 0.08f, Position - Velocity, Velocity + new Vector2(MathF.Cos(angle), MathF.Sin(angle)), angle, 0, new Color(255, 0, 0), Color.Transparent);
                    ParticleManager.Add(particle);
                }
            }
            if (Health <= 0)
            {
                Explode(6, ColliderRadius);
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), Position);
                if (Engine.SaveGame.GiveWeapon)
                {
                    Engine.EntityManager.Add(new Shotgun() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 10, AngularVelocity = this.AngularVelocity });
                }
                else
                {
                    Engine.EntityManager.Add(new SummonShield() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 10, AngularVelocity = this.AngularVelocity });
                }
            }
            yield return 0;
        }
    }
    public static Enemy NewOverloadBoss(Vector2 position, Vector2 velocity, float angle, Team _team = Team.Hostile)
    {
        Enemy enemy = new(position, velocity, angle, 120, Assets.Get(Sprites.OverloadBoss), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.OverloadBoss()));
        return enemy;
    }
    IEnumerable<int> WyvernBoss(Enemy _parent, int damage)
    {
        EnemyRange.particleVelocity = 500;
        Enemy parent = _parent;
        Enemy tail1 = null;
        Enemy tail2 = null;
        bool hasExploded = false;
        if (_parent == null)
        {
            tail1 = new(Position, Velocity, Angle, 50, Assets.Get(Sprites.WyvernBoss), _parent.Team);
            tail2 = new(Position, Velocity, Angle, 75, Assets.Get(Sprites.WyvernBoss), _parent.Team);
            tail1.AddComponent(new Behaviour(tail1).AddBehaviour(tail1.WyvernBoss(this, 15)));
            tail2.AddComponent(new Behaviour(tail2).AddBehaviour(tail2.WyvernBoss(tail1, 4)));
            Engine.EntityManager.Add(tail1);
            Engine.EntityManager.Add(tail2);
        }
        CD = [0];
        while (true)
        {
            Velocity *= 0.8f;
            ChildEnemy = !(parent == null);
            Vector2 normalizedAcceleration = GetNormalizedAcceleration();
            if (Health <= 0)
            {
                if (GetComponent<Collide>().WasHit && Util.Random.Next(0, 10) == 0)
                {
                    for (float angle = 0; angle < MathF.Tau; angle += MathF.PI / 3)
                    {
                        Engine.EntityManager.Add(NewAssassinShot(Position, Util.ToUnitVector(angle) * 8, angle, 0, Team, 6, 1));
                    }
                }
                if (!hasExploded)
                {
                    Explode(6, ColliderRadius);
                    SoundManager.PlaySound(Assets.Get(Sound.Death), Position);
                    if (tail1 == null && tail2 == null)
                    {
                        isExpired = true;
                    }
                    else
                    {
                        hasExploded = true;
                        Position = new Vector2(10000, 10000);
                        if (Engine.SaveGame.GiveWeapon)
                        {
                            Engine.EntityManager.Add(new LMG() { Position = this.Position, Velocity = normalizedAcceleration * 10, AngularVelocity = this.AngularVelocity });
                        }
                        else
                        {
                            Engine.EntityManager.Add(new Reflective() { Position = this.Position, Velocity = normalizedAcceleration * 10, AngularVelocity = this.AngularVelocity });
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
                if (parent.Health <= 0)
                {
                    parent = null;
                }
                else
                {
                    Vector2 p1 = parent.Position - new Vector2(MathF.Sin(parent.Angle), -MathF.Cos(parent.Angle)) * 5;
                    Vector2 relativep1 = p1 - Position;
                    if (relativep1.LengthSquared() > 0.01f)
                    {
                        Angle = MathF.Atan2(relativep1.Y, relativep1.X) + MathF.PI / 2;
                    }
                    Position = p1 - new Vector2(MathF.Sin(Angle), -MathF.Cos(Angle)) * 5;
                }
            }
            else
            {
                if (!hasExploded)
                {
                    Entity nearestEnemy = Player;
                    float theta = MathF.Atan2((nearestEnemy.Position - Position).X, -(nearestEnemy.Position - Position).Y) - MathF.PI / 2;
                    Vector2 targetVector = nearestEnemy.Position - new Vector2(MathF.Cos(theta + MathF.PI / 8), MathF.Sin(theta + MathF.PI / 8)) * 200;
                    targetAngle = MathF.Atan2(Velocity.X, -Velocity.Y);
                    if (tail1 != null && tail2 != null)
                    {
                        if (tail1.isExpired)
                        {
                            theta = MathF.Atan2((nearestEnemy.Position - tail2.Position).X, -(nearestEnemy.Position - tail2.Position).Y) - MathF.PI / 2;
                            targetVector = nearestEnemy.Position + new Vector2(MathF.Cos(theta + MathF.PI / 8), MathF.Sin(theta + MathF.PI / 8)) * 200;
                        }
                    }
                    Angle = targetAngle;
                    float speed = 5 + Math.Abs((nearestEnemy.Velocity - Velocity).Length() - 5) + (targetVector - Position).LengthSquared() / 10000;
                    GoToPosition(targetVector, speed);
                    if (tail1 != null)
                    {
                        if (!tail1.isExpired && Health < MaxHealth / 2)
                        {
                            tail1.Health = 0;
                            Health = MaxHealth;
                        }
                    }
                    if ((nearestEnemy.Position - Position).Length() < 500)
                    {
                        if (CD[0] <= 0)
                        {
                            if ((tail1 == null && tail2 == null))
                            {
                                var direction = Vector2.Normalize(nearestEnemy.Position - Position);
                                var p1 = NewPulseShot(Position, Velocity + new Vector2(direction.X, -direction.Y) * 2, theta, 0, Team, damage, true);
                                p1.Texture = Assets.Get(Sprites.Microshot);
                                Engine.EntityManager.Add(p1);
                                p1 = NewPulseShot(Position, Velocity - new Vector2(direction.X, -direction.Y) * 2, theta + MathF.PI, 0, Team, damage, true);
                                p1.Texture = Assets.Get(Sprites.Microshot);
                                Engine.EntityManager.Add(p1);
                                SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Position);
                                CD[0] = 0.5f;
                            }
                            else
                            {
                                Engine.EntityManager.Add(NewPulseShot(Position, Vector2.Normalize(nearestEnemy.Position - Position) * 6 + nearestEnemy.Velocity, Angle, 0, Team, damage, tail2.isExpired));
                                SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Position);
                                CD[0] = 0.75f;
                            }
                        }
                    }
                }
            }
            yield return 0;
        }
    }
    public static Enemy NewWyvernBoss(Vector2 position, Vector2 velocity, float angle, Team _team = Team.Hostile)
    {
        Enemy enemy = new(position, velocity, angle, 100, Assets.Get(Sprites.WyvernBoss), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.WyvernBoss(null, 8)));
        return enemy;
    }
    IEnumerable<int> ExcursionBoss()
    {
        CD =
        [
            0, //Default
            0, //Laser cooldown
        ];
        EnemyRange.particleVelocity = 500;
        int currentWave = 0;
        int currentEnemy = 0;
        float waveTimer = 0;
        Entity nearestPickup;
        int bulletOffset = 8;
        float laserWindup = 3;
        Func<Vector2, Vector2, float, Team, Enemy>[][] waves =
        [
            [ NewFighter, NewFighter, ],
            [ NewCarrier, NewFighter, NewFighter, NewFighter, NewFighter ],
            [ NewSniper, NewSniper, NewSniper, NewFighter, NewFighter, ],
            [ NewSniper, NewSniper, NewFighter, NewFighter, NewShotgunner, NewCarrier, ],
        ];
        while (true)
        {
            nearestPickup = Engine.EntityManager.NearestItem(this, true);
            Vector2 relativePosition = Player.Position + Player.Velocity - Position - Velocity;
            Vector2 normalizedAcceleration = GetNormalizedAcceleration();
            targetAngle = MathF.Atan2(relativePosition.X, -relativePosition.Y);
            if (Health >= MaxHealth / 2 || CD[1] > 0)
            {
                if (Engine.EntityManager.NearestAlly(this) is Enemy nearestExpiredAlly)
                {
                    float distance = Vector2.Distance(Position, nearestExpiredAlly.Position);
                    if (nearestExpiredAlly.Health <= 0 && distance < 300)
                    {
                        nearestExpiredAlly.Mine();
                        var dir = Vector2.Normalize(nearestExpiredAlly.Position - Position);
                        for (float i = 0; i < distance / 2; i++)
                        {
                            Vector3 color = new Vector3(1, 1, 0) * (1 - i / 150) + new Vector3(1, 0, 0) * (i / 150);
                            ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), dir * (i + 4f) * 2 + Position, Util.ToAngle(dir), new Color(color.X, color.Y, color.Z) * (1 - (i / 150))));
                        }
                    }
                }
                if (nearestPickup != null)
                {
                    GoToPosition(nearestPickup.Position, 5);
                    if (Vector2.Distance(nearestPickup.Position, Position) < 100)
                    {
                        Velocity -= normalizedAcceleration * 8 * 60 * Engine.DeltaSeconds;
                    }
                    if (EntityManager.DistanceSqr(this, nearestPickup) < 25 * 25)
                    {
                        nearestPickup.isExpired = true;
                        Collide(-15);
                    }
                }
                else
                {
                    Vector2 targetLocation = Player.Position + Vector2.Normalize(Player.Velocity) * 250 + Engine.SaveGame.CurrentMission.GetNormalizedAcceleration(Player.Position) * 50;
                    float speed = 8 - 5 / ((targetLocation - Position).Length() / 75 + 1);
                    GoToPosition(targetLocation, speed);
                }
            }
            if (CD[1] <= 0 && Health < MaxHealth / 2)
            {
                Velocity *= 0.9f;
                float timeToHit;
                Vector2 playerIterativePosition = Player.Position;
                float prevTimeToHit = 0;
                for (int i = 0; i < 1; i++)
                {
                    timeToHit = MathF.Sqrt(EntityManager.DistanceSqr(Position, playerIterativePosition)) / 100;
                    playerIterativePosition += Player.Velocity * (timeToHit - prevTimeToHit);
                    prevTimeToHit = timeToHit;
                }
                Vector2 targetVector = (playerIterativePosition - Position);
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
                    Vector2 direction = Util.ToUnitVector(Angle);
                    var shot = NewAssassinShot(Position, direction * 100, Angle, 0, Team, 15);
                    Engine.EntityManager.Add(shot);
                    SoundManager.PlaySound(Assets.Get(Sound.SniperFire), Position);
                    Velocity -= direction * 15;
                    laserWindup = 3;
                    CD[1] = 10;
                }
            }
            else if (EntityManager.DistanceSqr(Player, this) < 300 * 300)
            {
                if (CD[0] <= 0)
                {
                    Vector2 normalOffset = Util.ToUnitVector(Angle + MathF.PI / 2);
                    Vector2 offset = normalOffset * Util.Random.Next(-2, 3);
                    Texture2D dot = Assets.Get(Sprites.Microshot);
                    var shot = NewAssassinShot(Position + normalOffset * bulletOffset, Util.ToUnitVector(Angle) * 10 + offset / 4, Angle, 0, Team, 3);
                    shot.Texture = dot;
                    shot.TimeLeft = 3;
                    Engine.EntityManager.Add(shot);
                    SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Position);
                    CD[0] = 0.25f;
                    bulletOffset = -bulletOffset;
                }
            }
            RotateTowards(targetAngle);
            if ((float)Health / (float)MaxHealth < 0.8f - currentWave * 0.2f && currentWave < waves.Length)
            {
                if (currentEnemy == 0)
                {
                    SoundManager.PlaySound(Assets.Get(Sound.Undock), Position);
                }
                Func<Vector2, Vector2, float, Team, Entity>[] wave = waves[currentWave];
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
                        float spawnAngle = MathF.Tau * currentEnemy / wave.Length + Angle;
                        var enemy = wave[currentEnemy](Position + new Vector2(MathF.Cos(spawnAngle), MathF.Sin(spawnAngle)) * 40, Velocity, Angle, Team) as Enemy;
                        if(Util.Random.Next(0, 5) == 0)
                        {
                            enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.DropItem(ItemFactory.NewScrap)));
                        }
                        Engine.EntityManager.Add(enemy);
                        waveTimer = 0.1f;
                        currentEnemy++;
                    }
                }
            }
            if (Health <= 0)
            {
                Explode(6, ColliderRadius);
                int particles = Util.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Util.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Util.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), 0.5f, Position, particleVelocity + Velocity, angle, 0, Color.Yellow, new Color(255, 0, 0, 0)));
                }
                particles = Util.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Util.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Util.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), 0.5f, Position, particleVelocity + Velocity, angle, 0, Color.DarkSlateGray, Color.Transparent));
                }
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), Position);
                if (Engine.SaveGame.GiveWeapon)
                {
                    Engine.EntityManager.Add(new Antimaterial() { Position = this.Position, Velocity = normalizedAcceleration * 10, AngularVelocity = this.AngularVelocity });
                }
                else
                {
                    Engine.EntityManager.Add(new Nanomachines() { Position = this.Position, Velocity = normalizedAcceleration * 10, AngularVelocity = this.AngularVelocity });
                }
            }
            Velocity *= 0.8f;
            yield return 0;
        }
    }
    public static Enemy NewExcursionBoss(Vector2 position, Vector2 velocity, float angle, Team _team = Team.Hostile)
    {
        Enemy enemy = new(position, velocity, angle, 200, Assets.Get(Sprites.ExcursionBoss), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.ExcursionBoss()));
        return enemy;
    }
    public static Enemy NewExcursionBoss(Vector2 position, Vector2 velocity, float angle)
    {
        return NewExcursionBoss(position, velocity, angle, Team.Hostile);
    }
    IEnumerable<int> ExodusBoss(bool isWeak = false)
    {
        int damage = 8;
        CD = [0, 0, 2.5f];
        EnemyRange.particleVelocity = 250;
        float missileGap = 0;
        var col = Color.DarkRed;
        col.A = 0;
        var engineParticles = new ParticleEmitter(Assets.Get(Sprites.Circle), 0.1f, Vector2.Zero, 0, MathF.PI / 2, 2,
            200f, Color.Yellow, EmitterType.EmissionOverTime) { particleFadeToColor = col };
        while (true)
        {
            var targetVector = Vector2.Normalize(Player.Position - Position);
            targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
            var normalAcceleration = Vector2.Normalize(new Vector2(Velocity.Y, -Velocity.X));
            if (Velocity.Length() <= 0.01f)
            {
                normalAcceleration = Vector2.Zero;
            }
            float closingVelocity = Vector2.Dot(targetVector, Velocity);
            var relativePosition = Player.Position - Position;
            var relativeVelocity = Player.Velocity - Velocity;
            var futureTargetVector = relativePosition + relativeVelocity;
            var direction = Vector2.Normalize(relativePosition);
            Entity nearestEnemy = Engine.EntityManager.NearestEnemy(new Enemy(Position - direction * 10, Vector2.Zero, 0, 0, null, Team));
            Entity nearestProjectile = Engine.EntityManager.NearestProjectile(Position - direction * 10, SensingAbility, Team);
            float playerAngle = MathF.Atan2(relativePosition.Y, relativePosition.X) + MathF.PI / 2;
            if (missileGap <= 0 && MathF.Abs(playerAngle - Angle) < 0.15f && relativePosition.Length() < 250)
            {
                bool fire = false;
                direction = Util.ToUnitVector(Angle);
                int sign = 0;
                if (CD[1] <= 0)
                {
                    CD[1] = 5;
                    fire = true;
                    sign = 1;
                }
                else if (CD[2] <= 0)
                {
                    CD[2] = 5;
                    fire = true;
                    sign = -1;
                }
                if (fire)
                {
                    missileGap = 0.5f;
                    Engine.EntityManager.Add(NewMissile(Position + new Vector2(direction.Y, -direction.X) * 5 * sign, direction * 15 + Velocity, Angle, Team));
                    SoundManager.PlaySound(Assets.Get(Sound.MissileFire), Position);
                }
            }
            if (nearestProjectile != null)
            {
                var pos = Vector2.Normalize(Position - nearestProjectile.Position);
                var vel = Vector2.Normalize(Velocity - nearestProjectile.Velocity);
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
            //Missile targetting
            if (nearestEnemy.GetComponent<MissileTag>() != null && (Position - nearestEnemy.Position).Length() < 250)
            {
                Vector2 playerIterativePosition = nearestEnemy.Position;
                float timeToHit = MathF.Sqrt(EntityManager.DistanceSqr(Position, playerIterativePosition)) / 20;
                playerIterativePosition += nearestEnemy.Velocity * timeToHit;
                targetVector = playerIterativePosition - Position;
                float angle = MathF.Atan2(targetVector.X, -targetVector.Y);
                if (CD[0] <= 0)
                {
                    var p1 = NewPulseShot(Position, Util.ToUnitVector(angle) * 15, angle, 0, Team, damage, true, 1);
                    p1.Texture = Assets.Get(Sprites.CrossbowShot);
                    Engine.EntityManager.Add(p1);
                    SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Position);
                    CD[0] = 0.2f;
                }
            }
            float angleRateOfChange = (targetAngle - MathF.Atan2(futureTargetVector.X, -futureTargetVector.Y)) / Engine.DeltaSeconds;
            //Note: Add bonus to speed if moving very fast toward planet
            Vector2 accelerationVector = (normalAcceleration * closingVelocity * angleRateOfChange * 2) * Engine.DeltaSeconds + GetNormalizedAcceleration() * (2);
            if (accelerationVector.LengthSquared() > 1f)
            {
                accelerationVector = Vector2.Normalize(accelerationVector);
            }
            Velocity += accelerationVector;
            if (MathF.Abs(angleRateOfChange) < 0.5f && relativeVelocity.X * direction.X + relativeVelocity.Y * direction.Y > -8f)
            {
                Vector2 thrustForce = targetVector * 12;
                Velocity += thrustForce * Engine.DeltaSeconds;
                accelerationVector += thrustForce;
            }
            if (accelerationVector.LengthSquared() < 0.05f)
            {
                RotateTowards(MathF.Atan2(Velocity.Y, Velocity.X) + MathF.PI / 2, 0.2f);
            }
            else
            {
                RotateTowards(MathF.Atan2(accelerationVector.X, -accelerationVector.Y), 0.2f);
            }
            if (missileGap > 0)
            {
                missileGap -= Engine.DeltaSeconds;
            }
            if (Health <= 0)
            {
                Explode(6, ColliderRadius);
                int particles = Util.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Util.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Util.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), 0.5f, Position, particleVelocity + Velocity, angle, 0, Color.Yellow, new Color(255, 0, 0, 0)));
                }
                particles = Util.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Util.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Util.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), 0.5f, Position, particleVelocity + Velocity, angle, 0, Color.DarkSlateGray, Color.Transparent));
                }
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), Position);
                if (!isWeak)
                {
                    if (Engine.SaveGame.GiveWeapon)
                    {
                        //TODO: Move crossbow drop to better fitting boss 
                        Engine.EntityManager.Add(new Crossbow() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 10, AngularVelocity = this.AngularVelocity });
                    }
                    else
                    {
                        Engine.EntityManager.Add(new PlasmaEngine() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 10, AngularVelocity = this.AngularVelocity });
                    }
                }
                else
                {
                    Engine.EntityManager.Add(ItemFactory.NewScrap(Position, GetNormalizedAcceleration() * 10, AngularVelocity));
                }
            }
            engineParticles.sprayAngle = (Angle + MathF.PI) * 180 / MathF.PI;
            engineParticles.speedOfEmission = accelerationVector.Length() * 50 + 200;
            engineParticles.offsetVelocity = Velocity;
            engineParticles.Update();
            engineParticles.position = Position + new Vector2(-MathF.Sin(Angle), MathF.Cos(Angle)) * 11;
            yield return 0;
        }
    }
    public static Enemy NewExodusBoss(Vector2 position, Vector2 velocity, float angle, Team _team = Team.Hostile)
    {
        Enemy enemy = new(position, velocity, angle, 80, Assets.Get(Sprites.ExodusBoss), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.ExodusBoss()));
        return enemy;
    }
    IEnumerable<int> VeilBoss()
    {
        int damage = 12;
        CD = [0];
        StealthAbility = 2;
        SensingAbility = -1;
        float detectionCooldown = 0;
        EnemyRange.particleVelocity = 450;
        Entity detectedEntity = null;
        while (true)
        {
            if (detectionCooldown <= 0)
            {
                detectionCooldown = 1;
                if (Util.Random.Next(0, 2) == 0)
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
                Velocity *= 0.8f;
                Vector2 relativePosition = detectedEntity.Position - Position;
                var targetVector = Vector2.Normalize(relativePosition);
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                Entity nearestProjectile = Engine.EntityManager.NearestProjectile(Position + targetVector * 10, SensingAbility, Team);
                if (nearestProjectile != null)
                {
                    var pos = Vector2.Normalize(Position - nearestProjectile.Position);
                    var vel = Vector2.Normalize(Velocity - nearestProjectile.Velocity);
                    if (float.IsNaN(vel.X))
                    {
                        vel = Vector2.Zero;
                    }
                    if ((pos.X * vel.X + pos.Y * vel.Y) < -0.5f)
                    {
                        int sign = Math.Sign(pos.X * vel.Y - vel.X * pos.Y);
                        if (sign == 0)
                        {
                            sign = 1;
                        }
                        Velocity += new Vector2(-pos.Y, pos.X) * sign * Engine.DeltaSeconds * 500;
                    }
                }
                Vector2 relativeVelocity = Velocity - Player.Velocity - targetVector * 4;
                GoToPosition(detectedEntity.Position, (-Math.Min(0, relativeVelocity.X * targetVector.X + relativeVelocity.Y * targetVector.Y) * 1.75f));
                RotateTowards(targetAngle);
                if (CD[0] <= 0)
                {
                    CD[0] = 0.25f;
                    RevealDuration += 0.2f;
                    Engine.EntityManager.Add(NewExplosive(Position, Velocity + targetVector * 10 + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) * 5, Util.OneToNegOne() * MathF.PI, Util.OneToNegOne(), Team, damage / 2, 40, 1));
                    SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Position);
                }
                if (relativePosition.Length() < 100)
                {
                    Velocity -= targetVector * 180 * Engine.DeltaSeconds;
                }
            }
            else
            {
                var planet = Engine.SaveGame.CurrentMission.Planet;
                var relativePosition = Position - planet.Position;
                var dir = Vector2.Normalize(relativePosition);
                Velocity -= Vector2.Normalize(dir * (relativePosition.Length() - planet.ColliderRadius * 1.5f)) * Engine.DeltaSeconds * 4;
                Vector2 targetVelocity = new Vector2(dir.Y, -dir.X) * planet.GetOrbitalVelocity((dir * planet.ColliderRadius * 1.5f));
                Vector2 diffVelocity = targetVelocity - Velocity;
                Velocity += diffVelocity * Engine.DeltaSeconds;
                targetAngle = MathF.Atan2(Velocity.Y, Velocity.X) + MathF.PI / 2;
                RotateTowards(targetAngle);
                if (CD[0] <= 0)
                {
                    CD[0] = 1;
                    RevealDuration += 0.5f;
                    var p1 = NewExplosive(Position, Velocity + GetNormalizedAcceleration() * 160, Util.OneToNegOne() * MathF.PI, Util.OneToNegOne(), Team, damage, 200, 1);
                    p1.TimeLeft = 10;
                    Engine.EntityManager.Add(p1);
                    SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Position);
                }
            }
            if (Health <= 0)
            {
                Explode(6, ColliderRadius);
                int particles = Util.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Util.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Util.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), 0.5f, Position, particleVelocity + Velocity, angle, 0, Color.Yellow, new Color(255, 0, 0, 0)));
                }
                particles = Util.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Util.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Util.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), 0.5f, Position, particleVelocity + Velocity, angle, 0, Color.DarkSlateGray, Color.Transparent));
                }
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), Position);
                if (Engine.SaveGame.GiveWeapon)
                {
                    Engine.EntityManager.Add(new GrenadeLauncher() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 10, AngularVelocity = this.AngularVelocity });
                }
                else
                {
                    Engine.EntityManager.Add(new StealthHull() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 10, AngularVelocity = this.AngularVelocity });
                }
            }
            yield return 0;
        }
    }
    public static Enemy NewVeilBoss(Vector2 position, Vector2 velocity, float angle)
    {
        Enemy enemy = new(position, velocity, angle, 150, Assets.Get(Sprites.VeilBoss), Team.Hostile);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.VeilBoss()));
        return enemy;
    }
    IEnumerable<int> InfernoBoss()
    {
        int damage = 3;
        Enemy flare = NewFlareBoss(Position - new Vector2(2000, 0), Velocity, 0, this);
        Engine.EntityManager.Add(flare);
        CD = [1.5f];
        float rangeFactor = 1;
        float rotationSpeed = 0.03f;
        float sign = 1;
        float swapCooldown = 10;
        float time = 0;
        bool isDamaged = false;
        while (true)
        {
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
            else if (flare.Health <= 0)
            {
                rangeFactor = 1.25f;
                rotationSpeed = 0.05f;
            }
            EnemyRange.particleVelocity = 400 * rangeFactor;
            time += Engine.DeltaSeconds;
            Vector2 normalizedAcceleration = GetNormalizedAcceleration();
            Vector2 relativeVelocity = Player.Velocity - Velocity;
            Vector2 relativePosition = Player.Position - Position;
            var playerAcceleration = Vector2.Normalize(Engine.SaveGame.CurrentMission.GetNormalizedAcceleration(Player.Position));
            Vector2 relativeTargetPosition = relativePosition + new Vector2(playerAcceleration.Y, -playerAcceleration.X) * 100 * sign + normalizedAcceleration * 20;
            Vector2 playerIterativePosition = Player.Position;
            float timeToHit = MathF.Sqrt(EntityManager.DistanceSqr(Position, playerIterativePosition)) / 8;
            playerIterativePosition += relativeVelocity * timeToHit;
            Vector2 targetVector = (playerIterativePosition - Position);
            targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
            RotateTowards(targetAngle + MathF.Sin(time * 3) / 3, rotationSpeed);

            Vector2 velocityChange = (relativeVelocity + relativeTargetPosition / 12) - Velocity;
            if (velocityChange.Length() > 60)
            {
                velocityChange = Vector2.Normalize(velocityChange) * 60;
            }
            Velocity += velocityChange * Engine.DeltaSeconds;
            if (CD[0] <= 0 && Vector2.Distance(Player.Position, Position) < 400 * rangeFactor)
            {
                Engine.EntityManager.Add(new FlameBolt(Position, Velocity + Util.ToUnitVector(Angle) * 12 * rangeFactor + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 4, Team, damage, 0.3f * rangeFactor, 2f));
                SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Position);
                CD[0] = 0.08f;
            }
            if (Health <= 0 && !isDamaged)
            {
                isDamaged = true;
                SoundManager.PlaySound(Assets.Get(Sound.ShieldHit), Position);
                Explode(0, 0);
            }
            if (Health <= 0 && flare.Health <= 0)
            {
                Explode(6, ColliderRadius);
                int particles = Util.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Util.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Util.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), 0.5f, Position, particleVelocity + Velocity, angle, 0, Color.Yellow, new Color(255, 0, 0, 0)));
                }
                particles = Util.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Util.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Util.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), 0.5f, Position, particleVelocity + Velocity, angle, 0, Color.DarkSlateGray, Color.Transparent));
                }
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), Position);
                Engine.EntityManager.Add(new Flamethrower() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 10, AngularVelocity = this.AngularVelocity });
            }
            yield return 0;
        }
    }
    public static Enemy NewInfernoBoss(Vector2 _position, Vector2 _velocity, float _angle)
    {
        var enemy = new Enemy(_position, _velocity, _angle, 175, Assets.Get(Sprites.InfernoBoss));
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.InfernoBoss()));
        return enemy;
    }
    IEnumerable<int> FlareBoss(Enemy _inferno)
    {
        int damage = 7;
        EnemyRange.particleVelocity = 1000;
        CD = [
            1.5f, //Primary attack
            15, //Octoshot
        ];
        float shotCooldown = 0.75f;
        int shotCount = 0;
        bool isDamaged = false;
        while (true)
        {
            float cdMod = ((isDamaged) ? 2 : 1) * ((_inferno.Health <= 0) ? 0.5f : 1);

            Vector2 relativeVelocity = Player.Velocity - Velocity;
            Vector2 relativePosition = Player.Position - Position;
            Vector2 relativeTargetPosition = relativePosition + Vector2.Normalize(Engine.SaveGame.CurrentMission.GetNormalizedAcceleration(Player.Position)) * 250;
            Vector2 playerIterativePosition = Player.Position;
            float timeToHit = MathF.Sqrt(EntityManager.DistanceSqr(Position, playerIterativePosition)) / 15;
            playerIterativePosition += relativeVelocity * timeToHit;
            var targetVector = Vector2.Normalize(playerIterativePosition - Position);
            targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
            RotateTowards(targetAngle);

            Vector2 velocityChange = (relativeVelocity + relativeTargetPosition / 15) - Velocity;
            if (velocityChange.Length() > 60)
            {
                velocityChange = Vector2.Normalize(velocityChange) * 60;
            }
            Velocity += velocityChange * Engine.DeltaSeconds;
            if (CD[1] <= 0)
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
                        Engine.EntityManager.Add(new FlameBolt(Position, Velocity + Util.ToUnitVector((float)(shotCount) * MathF.Tau / 8) * speed + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 2, Team, damage, 30, 1f));
                        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Position);
                        shotCount++;
                    }
                    else
                    {
                        shotCount = 0;
                        CD[1] = 15f * cdMod;
                        CD[0] = 0.2f * cdMod;
                        shotCooldown = 0.75f * cdMod;
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
                if (CD[0] <= 0 && Vector2.Distance(Player.Position, Position) < 1000 && shotCount == 0)
                {
                    Engine.EntityManager.Add(new FlameBolt(Position, Velocity + targetVector * 15 + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 2, Team, damage, 4, 1f));
                    SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Position);
                    CD[0] = 0.8f * cdMod;
                }
                DrawLine(Angle, CD[0], 0.8f);
            }
            if (Health <= 0 && !isDamaged)
            {
                isDamaged = true;
                SoundManager.PlaySound(Assets.Get(Sound.ShieldHit), Position);
                Explode(0, 0);
            }
            if (Health <= 0 && _inferno.Health <= 0)
            {
                Explode(6, ColliderRadius);
                int particles = Util.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Util.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Util.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), 0.5f, Position, particleVelocity + Velocity, angle, 0, Color.Yellow, new Color(255, 0, 0, 0)));
                }
                particles = Util.Random.Next(3, 5);
                for (int i = 0; i < particles; i++)
                {
                    float angle = Util.Random.NextSingle() * MathF.PI * 2;
                    Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Util.Random.NextSingle() * 2 + 2) / 2;
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), 0.5f, Position, particleVelocity + Velocity, angle, 0, Color.DarkSlateGray, Color.Transparent));
                }
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), Position);
                Engine.EntityManager.Add(new Fireball() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 10, AngularVelocity = this.AngularVelocity });
            }
            yield return 0;
        }
    }
    public static Enemy NewFlareBoss(Vector2 _position, Vector2 _velocity, float _angle, Enemy _inferno)
    {
        var enemy = new Enemy(_position, _velocity, _angle, 100, Assets.Get(Sprites.FlareBoss));
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.FlareBoss(_inferno)));
        return enemy;
    }
    IEnumerable<int> SurgeBoss()
    {
        int damage = 6;
        CD = [0];
        List<Enemy> children = [];
        for (float angle = 0; angle < MathF.Tau; angle += 1.61803398875f / 6)
        {
            Vector2 dir = Util.ToUnitVector(angle);
            var enemy = NewSurgeChild(Position + dir * angle * 10, Velocity, angle, this, children);
            children.Add(enemy);
            Engine.EntityManager.Add(enemy);
        }
        while (true)
        {
            if (children.Count <= 0)
            {
                if (Health <= 0)
                {
                    isExpired = true;
                    Explode(0, 0);
                    if (Engine.SaveGame.GiveWeapon)
                    {
                        Engine.EntityManager.Add(new Spiral() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 10, AngularVelocity = this.AngularVelocity });
                    }
                    else
                    {
                        Engine.EntityManager.Add(new CreateFighter() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 10, AngularVelocity = this.AngularVelocity });
                    }
                }
                else
                {
                    Velocity += (Engine.SaveGame.Player.Position - Position) * 30 * Engine.DeltaSeconds;
                    if (Velocity.Length() > 12)
                    {
                        Velocity = Vector2.Normalize(Velocity) * 12;
                    }
                    if (Velocity.Length() < 1)
                    {
                        Velocity += Util.ToUnitVector(Angle) * 30 * Engine.DeltaSeconds;
                    }
                    if (CD[0] <= 0 && Vector2.Distance(Engine.SaveGame.Player.Position, Position) < 450 && Vector2.Dot(Velocity, (Engine.SaveGame.Player.Position - Position)) / (Velocity.Length() * (Engine.SaveGame.Player.Position - Position).Length()) > 0.75f)
                    {
                        CD[0] = 0.8f;
                        Vector2 dir = Util.ToUnitVector(Angle) * 12;
                        Engine.EntityManager.Add(NewSpiralShot(Position, dir, Angle, 0, Team, damage, 0));
                        Engine.EntityManager.Add(NewSpiralShot(Position, dir, Angle, 0, Team, damage, MathF.PI));
                    }
                    Angle = Util.ToAngle(Velocity);
                }
            }
            else
            {
                if (Health > 0)
                {
                    Velocity += (Engine.SaveGame.Player.Position - Position * 12) * Engine.DeltaSeconds;
                    if (Velocity.Length() > 10)
                    {
                        Velocity = Vector2.Normalize(Velocity) * 10;
                    }
                    if (Velocity.Length() < 1)
                    {
                        Velocity += Util.ToUnitVector(Angle) * 300 * Engine.DeltaSeconds;
                    }
                    if (CD[0] <= 0 && Vector2.Distance(Engine.SaveGame.Player.Position, Position) < 300 && Vector2.Dot(Velocity, (Engine.SaveGame.Player.Position - Position)) / (Velocity.Length() * (Engine.SaveGame.Player.Position - Position).Length()) > 0.75f)
                    {
                        CD[0] = 1f;
                        Engine.EntityManager.Add(NewSpiralShot(Position, Util.ToUnitVector(Angle) * 8, Angle, 0, Team, damage, 0));
                    }
                    Angle = Util.ToAngle(Velocity);
                }
            }
            children = [.. children.Where(child => !child.isExpired)];

            yield return 0;
        }
    }
    public static Enemy NewSurgeBoss(Vector2 position, Vector2 velocity, float angle, Team _team = Team.Hostile)
    {
        Enemy enemy = new(position, velocity, angle, 50, Assets.Get(Sprites.SurgeBoss), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.SurgeBoss()));
        return enemy;
    }
    IEnumerable<int> SurgeChild(Entity _parent, List<Enemy> _allies)
    {
        int vision = 75;
        CD = [Util.Random.NextSingle() * 3, 0];
        var nearestEnemy = Engine.EntityManager.NearestEnemy(this) as Enemy;
        while (true)
        {
            Velocity *= 0.95f;
            if (Health <= 0)
            {
                isExpired = true;
                Explode(2, 1f);
            }
            Vector2 acceleration = Vector2.Zero;
            bool goToParent = false;
            if (_parent is not Enemy _enemy || (_enemy != null && !(_enemy.Health <= 0)))
            {
                goToParent = CD[0] > 0;
            }
            if (goToParent)
            {
                acceleration += (_parent.Position - Position);
                acceleration += (_parent.Velocity - Velocity) * 3;
            }
            Vector2 velocitySum = Vector2.Zero;
            float totalBirds = 0;
            foreach (var ally in _allies)
            {
                if (ally == this)
                {
                    continue;
                }
                float distance = Vector2.Distance(ally.Position, Position);
                if (distance < vision)
                {
                    if (distance < 25)
                    {
                        acceleration += Vector2.Normalize(Position - ally.Position) * 12;
                    }
                    velocitySum += ally.Velocity;
                    totalBirds++;
                }
            }
            if (totalBirds > 0)
            {
                acceleration += (velocitySum / totalBirds - Velocity) / 4;
            }
            if (nearestEnemy != null)
            {
                acceleration += Vector2.Normalize(nearestEnemy.Position - Position) * 5 * (!goToParent ? 10 : 1);
            }
            float speed = (Velocity - _parent.Velocity).Length();
            if (speed < 1)
            {
                acceleration += Util.ToUnitVector(Angle) * 30;
            }
            if (speed > 6)
            {
                acceleration -= Util.ToUnitVector(Angle) * 30;
            }
            Vector2 acc = GetNormalizedAcceleration();
            acceleration += acc * 30;
            if (Vector2.Dot(-acc, Velocity) / acc.Length() > 4)
            {
                acceleration += Vector2.Normalize(acc) * 100;
            }
            if (acceleration.Length() > 25)
            {
                acceleration = Vector2.Normalize(acceleration) * 25;
            }
            Velocity += acceleration * Engine.DeltaSeconds;
            Angle = Util.ToAngle(acceleration);
            if (nearestEnemy != null)
            {
                var relPos = nearestEnemy.Position - Position;
                float bulletSpeed = 10;
                var relativePosition = Vector2.Normalize(relPos + (nearestEnemy.Velocity - Velocity) * relPos.Length() / bulletSpeed);
                if (CD[0] <= 0 && relPos.Length() < 250 && Vector2.Dot(Util.ToUnitVector(Angle), relativePosition) > 0.85f)
                {
                    Engine.EntityManager.Add(NewPulseShot(Position, Util.ToUnitVector(Angle) * bulletSpeed + Velocity, Angle, 0, Team, 3));
                    SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Position);
                    CD[0] = Util.Random.NextSingle() * 8 + 5;
                }
            }
            if(IsFriendly(Engine.SaveGame.Player) && Health < MaxHealth && CD[1] <= 0)
            {
                CD[1] = 1;
                Health++;
            }
            yield return 0;
        }
    }
    public static Enemy NewSurgeChild(Vector2 position, Vector2 velocity, float angle, Entity _parent, List<Enemy> _allies)
    {
        Enemy enemy = new(position, velocity, angle, 12, Assets.Get(Sprites.SurgeChild), _parent.Team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.SurgeChild(_parent, _allies)).AddBehaviour(enemy.AvoidProjectiles(0.33f)));
        return enemy;
    }
    IEnumerable<int> BloomBoss(List<Enemy> segments, int damage)
    {
        CD =
        [
            0, //Default
            0, //Tail destruction
            10, //Charge cooldown
        ];
        hitSound = Assets.Get(Sound.ShieldHit);
        ChildEnemy = true;
        Enemy tail = segments[^1];
        bool hasSet = false;
        int moveTowards = 1;
        int segmentKillCount = 0;
        Vector2 targetVector = Velocity;
        foreach (var enemy in segments)
        {
            Engine.EntityManager.Add(enemy);
        }
        while (true)
        {
            Vector2 relativePosition = Engine.SaveGame.Player.Position - Position;
            float distSqr = relativePosition.LengthSquared();
            float speed = 2;
            if (distSqr > 500 * 500)
            {
                speed = (Engine.SaveGame.Player.Velocity - Velocity).Length() / 60 + 2;
            }
            if (tail.Health > 0)
            {
                Health = MaxHealth;
                tail.ChildEnemy = false;
                GoToPosition(Engine.SaveGame.Player.Position + new Vector2(relativePosition.Y, -relativePosition.X), speed);
            }
            else
            {
                if (moveTowards == 1)
                {
                    Velocity += Vector2.Normalize(relativePosition) * 30 * Engine.DeltaSeconds * moveTowards;
                }
                else
                {
                    Velocity += (targetVector - Velocity) * 15 * Engine.DeltaSeconds;
                }
                if (distSqr < 4000)
                {
                    targetVector = Velocity;
                    moveTowards = -1;
                }
                if (distSqr > 500 * 500)
                {
                    moveTowards = 1;
                }
                if (CD[0] <= 0)
                {
                    CD[0] = 12;
                    Engine.EntityManager.Add(NewSpewer(Position, Velocity, 0, 0, Team, damage));
                    SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Position);
                }

                if (!hasSet)
                {
                    hasSet = true;
                    ChildEnemy = false;
                    hitSound = Assets.Get(Sound.Hit);
                    CD[2] = 2;
                }
                if (segments.Count > 0 && CD[1] <= 0 && (MaxHealth - Health) / 6 >= segmentKillCount)
                {
                    CD[1] = 1f + Util.Random.NextSingle();
                    var seg = segments[^1];
                    seg.isExpired = true;
                    seg.Explode(0, 0);
                    SoundManager.PlaySound(Assets.Get(Sound.Explosion), seg.Position);
                    segments.RemoveAt(segments.Count - 1);
                    segmentKillCount++;
                }
            }
            if (Health <= 0)
            {
                Explode(5, ColliderRadius);
                SoundManager.PlaySound(Assets.Get(Sound.Explosion), Position);
                isExpired = true;
                if (Engine.SaveGame.GiveWeapon)
                {
                    Engine.EntityManager.Add(new SpewerModule() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 10, AngularVelocity = this.AngularVelocity });
                }
                else
                {
                    Engine.EntityManager.Add(new Reflective() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 10, AngularVelocity = this.AngularVelocity });
                }
            }
            Velocity *= Util.FIED(0.1f);
            if (CD[2] is > 0 and < 2)
            {
                DrawLine(Util.ToAngle(relativePosition), CD[2], 2);
            }
            if (CD[2] <= 0)
            {
                Velocity = Engine.SaveGame.Player.Velocity + Vector2.Normalize(relativePosition) * 25;
                CD[2] = 10;
                if (tail.Health <= 0)
                {
                    CD[2] = 3 + 0.5f * segments.Count;
                }
            }
            if (distSqr < (Engine.SaveGame.Player.ColliderRadius + ColliderRadius + 25) * (Engine.SaveGame.Player.ColliderRadius + ColliderRadius + 10))
            {
                if (Engine.SaveGame.Player.Collide(damage))
                {
                    for (float angle = MathF.PI / 30; angle < MathF.Tau; angle += MathF.PI / 30)
                    {
                        Vector2 dir = Util.ToUnitVector(angle);
                        ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 0.5f, Position, Velocity + dir * 3, angle, 0, Color.Red, Color.Transparent));
                    }
                }
            }
            targetAngle = Util.ToAngle(Velocity);
            RotateTowards(targetAngle, 0.2f);
            yield return 0;
        }
    }
    public static Enemy NewBloomBoss(Vector2 position, Vector2 velocity, float angle)
    {
        List<Enemy> segments = [];
        //Head
        var enemy = new Enemy(position, velocity, angle, 75, Assets.Get(Sprites.BloomHead), Team.Hostile);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.BloomBoss(segments, 10)));
        Enemy head = enemy;

        //Segments
        for (int i = 0; i < 8; i++)
        {
            var _enemy = new Enemy(position, velocity, angle, 100, Assets.Get(Sprites.BloomBody), Team.Hostile);
            _enemy.AddComponent(new Behaviour(_enemy).AddBehaviour(_enemy.Rope()).AddBehaviour(_enemy.FollowNextSegment(enemy)));
            segments.Add(_enemy);
            enemy = _enemy;
        }
        var _tail = new Enemy(position, velocity, angle, 20, Assets.Get(Sprites.BloomTail), Team.Hostile);
        _tail.AddComponent(new Behaviour(_tail).AddBehaviour(_tail.FollowNextSegment(enemy)));
        segments.Add(_tail);

        return head;
    }
    IEnumerable<int> Rope()
    {
        int damage = 8;
        CD = [Util.Random.NextSingle() * 5 + 1];
        while (true)
        {
            if (Health != MaxHealth)
            {
                bool reflect = false;
                for (int i = 0; i < (MaxHealth - Health); i++)
                {
                    if (Util.Random.Next(0, 8) == 0)
                    {
                        reflect = true;
                    }
                }
                if (reflect)
                {
                    float offset = Util.Random.NextSingle() * MathF.Tau;
                    for (float angle = 0; angle < MathF.Tau; angle += MathF.PI / 3)
                    {
                        Engine.EntityManager.Add(NewAssassinShot(Position, Util.ToUnitVector(angle + offset) * 8, angle + offset, 0, Team, damage, 1));
                    }
                }
                Health = MaxHealth;
            }
            if (CD[0] <= 0)
            {
                Vector2 dir = Vector2.Normalize(Engine.SaveGame.Player.Position - Position) * 10;
                Engine.EntityManager.Add(NewPulseShot(Position, dir, Util.ToAngle(dir), 0, Team, damage));
                SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Position);
                CD[0] = Util.Random.NextSingle() * 5 + 5;
            }
            yield return 0;
        }
    }
    IEnumerable<int> PursuerBoss(bool isWeak)
    {
        int damage = 8;
        Enemy holo = null;
        Entity nearestEnemy = null;
        Entity targettingProjectile = null;
        Vector2 randomPosition = Vector2.Zero;
        int mode = 0;
        int shotCount = 0;
        StealthAbility = 2;
        SensingAbility = 1;
        CD =
        [
            0, //Default
            0, //Enemy tracking
            0, //Weapons
        ];
        while (true)
        {
            var enemy = Engine.EntityManager.NearestEnemy(this);
            if (enemy == nearestEnemy)
            {
                CD[1] = 10;
            }
            else
            {
                if (enemy == null)
                {
                    if (CD[1] <= 0)
                    {
                        nearestEnemy = null;
                    }
                }
                else
                {
                    nearestEnemy = enemy;
                    CD[1] = 10;
                }
            }
            if (holo != null && holo.isExpired)
            {
                holo = null;
                Engine.SaveGame.Player.RevealDuration = 5;
            }
            if (CD[0] <= 0)
            {
                Engine.EntityManager.Add(NewDecoy(Position, Vector2.Zero, Angle, Sprites.Engineer, Team)); //Make sure the sprite matches the bosses sprite
                CD[0] = 25;
                if (holo != null)
                {
                    holo.isExpired = true;
                    holo = null;
                }
            }
            if (nearestEnemy != null)
            {
                if (mode == 0) //Hunting
                {
                    randomPosition = nearestEnemy.Position + Vector2.Normalize(nearestEnemy.Velocity) * 500;
                    GoToPosition(randomPosition, 5);
                    Velocity -= (nearestEnemy.Position - Position) * (nearestEnemy.Position - Position).Length() * Engine.DeltaSeconds / 20000;
                    if (CD[2] <= 0)
                    {
                        var vel = Vector2.Normalize(randomPosition - Position);
                        float angle = Util.ToAngle(vel);
                        Engine.EntityManager.Add(NewPulseShot(Position, vel * 5 + Player.Velocity, angle, 0, Team, damage, true, 1));
                        Engine.EntityManager.Add(NewExplosive(Position, vel * 5 + new Vector2(vel.Y, -vel.X) * 5 + Player.Velocity, angle, 0, Team, 8, 25, 1));
                        Engine.EntityManager.Add(NewExplosive(Position, vel * 5 - new Vector2(vel.Y, -vel.X) * 5 + Player.Velocity, angle, 0, Team, 8, 25, 1));
                        CD[2] = 1;
                        shotCount++;
                        if (shotCount >= 10)
                        {
                            shotCount = 0;
                            mode = 1;
                            CD[2] = 0;
                        }
                    }
                }
                else //Targetting
                {
                    GoToPosition(nearestEnemy.Position - Vector2.Normalize(nearestEnemy.Position - Position) * 200, 5);
                    if(targettingProjectile == null || targettingProjectile.isExpired)
                    {
                        targettingProjectile = NewPulseShot(Position, Velocity, Angle, 0, Team, damage);
                        targettingProjectile.Texture = Assets.Get(Sprites.Glow);
                        Engine.EntityManager.Add(targettingProjectile);
                    }
                    else
                    {
                        targettingProjectile.Velocity += (Engine.SaveGame.Player.Position - targettingProjectile.Position) * Engine.DeltaSeconds / 10;
                        targettingProjectile.Angle = Util.ToAngle(targettingProjectile.Velocity - Player.Velocity);
                    }
                    if (CD[2] <= 0)
                    {
                        Vector2 dir = Util.PredictEnemy(nearestEnemy, this, 12);
                        Engine.EntityManager.Add(NewPulseShot(Position, dir, Util.ToAngle(dir), 0, Team, damage, true, 1));
                        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Player.Position);
                        CD[2] = 0.25f;
                        RevealDuration += 0.25f;
                        shotCount++;
                        if (shotCount >= 25)
                        {
                            shotCount = 0;
                            mode = 0;
                            CD[2] = 0;
                        }
                    }
                }
            }
            else
            {
                if (Vector2.DistanceSquared(Position, randomPosition) < 1000)
                {
                    randomPosition = Util.ToUnitVector(Util.Random.NextSingle() * MathF.Tau) * (Engine.SaveGame.CurrentMission.Planet.ColliderRadius) * (1 + Util.Random.NextSingle() / 2);
                }
                GoToPosition(randomPosition, 1);
            }
            if (Health <= 0)
            {
                isExpired = true;
                Explode(10, ColliderRadius);
                SoundManager.PlaySound(Assets.Get(Sound.Explosion), Position);
                if (!isWeak)
                {
                    if (Engine.SaveGame.GiveWeapon)
                    {
                        Engine.EntityManager.Add(new GuidedRound() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 10, AngularVelocity = this.AngularVelocity });
                    }
                    else
                    {
                        Engine.EntityManager.Add(new Decoy() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 10, AngularVelocity = this.AngularVelocity });
                    }
                }
                else
                {
                    Engine.EntityManager.Add(ItemFactory.NewScrap(Position, GetNormalizedAcceleration() * 10, AngularVelocity));
                }
            }
            Velocity *= Util.FIED(0.15f);
            targetAngle = Util.ToAngle(Velocity);
            RotateTowards(targetAngle);
            yield return 0;
        }
    }
    public static Enemy NewPursuerBoss(Vector2 position, Vector2 velocity, float angle, Team _team = Team.Hostile)
    {
        Enemy enemy = new(position, velocity, angle, 100, Assets.Get(Sprites.Engineer), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.PursuerBoss(false)).AddBehaviour(enemy.AvoidProjectiles(1)));
        return enemy;
    }
    IEnumerable<int> StreamlineBoss(Enemy _leftWing, Enemy _rightWing)
    {
        int damage = 12;
        CD = [0];
        Engine.EntityManager.Add(_leftWing);
        Engine.EntityManager.Add(_rightWing);
        float offset = 1;
        while (true)
        {
            var nearestEnemy = Engine.EntityManager.NearestEnemy(this);
            if (nearestEnemy != null &&
                Vector2.DistanceSquared(nearestEnemy.Position, Position) < (nearestEnemy.ColliderRadius + ColliderRadius) * (nearestEnemy.ColliderRadius + ColliderRadius))
            {
                nearestEnemy.Collide(damage);
            }
            var tangent = new Vector2(Velocity.Y, -Velocity.X);
            Vector2 relPos = Engine.SaveGame.Player.Position - Position;
            float cross = Math.Sign(relPos.X * Velocity.Y - relPos.Y * Velocity.X);
            if (_leftWing.isExpired && _rightWing.isExpired)
            {
                if (relPos.Length() > 75)
                {
                    Velocity += tangent * Engine.DeltaSeconds * cross * 5;
                }
                if (CD[0] <= 0)
                {
                    Engine.EntityManager.Add(NewPulseShot(Position, Velocity + Vector2.Normalize(Engine.SaveGame.Player.Position - Position) * 10, Angle, 0, Team, damage, true, 1));
                    CD[0] = 1;
                    offset *= -1;
                }
            }
            else
            {
                if (relPos.Length() > 200)
                {
                    Velocity += tangent * Engine.DeltaSeconds * cross * 3;
                }
            }
            if (Velocity.Length() < float.Epsilon)
            {
                Velocity = Vector2.One;
            }
            if (relPos.Length() > 500)
            {
                Velocity += Vector2.Normalize(relPos) * 10 * Engine.DeltaSeconds;
            }
            float dv = Util.FIED(0.15f);
            if ((Engine.SaveGame.Player.Velocity - Velocity).Length() < 10)
            {
                Velocity += Vector2.Normalize(Velocity) * Engine.DeltaSeconds * 20;
            }
            Velocity = Velocity * dv + Engine.SaveGame.Player.Velocity * (1 - dv);
            if (Health <= 0)
            {
                isExpired = true;
                Explode(10, ColliderRadius);
                SoundManager.PlaySound(Assets.Get(Sound.Explosion), Position);
                if (Engine.SaveGame.GiveWeapon)
                {
                    Engine.EntityManager.Add(new SplitterModule() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 10, AngularVelocity = this.AngularVelocity });
                }
                else
                {
                    Engine.EntityManager.Add(new Ablative() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 10, AngularVelocity = this.AngularVelocity });
                }
            }
            targetAngle = Util.ToAngle(Velocity);
            RotateTowards(targetAngle, 0.1f);
            yield return 0;
        }
    }
    public static Enemy NewStreamlineBoss(Vector2 position, Vector2 velocity, float angle, Team _team = Team.Hostile)
    {
        Enemy boss = new(position, velocity, angle, 50, Assets.Get(Sprites.StreamlineBoss), _team);
        Enemy leftWing = new(position, velocity, angle, 30, Assets.Get(Sprites.StreamlineLeftWing), _team);
        leftWing.AddComponent(new Behaviour(leftWing).AddBehaviour(leftWing.Wing(boss, -5)));
        Enemy rightWing = new(position, velocity, angle, 30, Assets.Get(Sprites.StreamlineRightWing), _team);
        rightWing.AddComponent(new Behaviour(rightWing).AddBehaviour(rightWing.Wing(boss, 5)));
        boss.AddComponent(new Behaviour(boss).AddBehaviour(boss.StreamlineBoss(leftWing, rightWing)).AddBehaviour(boss.AvoidProjectiles(1)));
        return boss;
    }
    IEnumerable<int> Wing(Entity _parent, float _offset)
    {
        int damage = 8;
        ChildEnemy = true;
        CD =
        [
            (_offset > 0) ? 0 : 0.2f, //Weapon
            0, //Ablative shield
        ];
        float buffer = 10;
        int newMax = MaxHealth;
        while (true)
        {
            Position = _parent.Position + Util.ToUnitVector(_parent.Angle + MathF.PI / 2) * _offset;
            Velocity = _parent.Velocity;
            Angle = _parent.Angle;
            var nearestEnemy = Engine.EntityManager.NearestEnemy(this);
            if (nearestEnemy != null)
            {
                Vector2 relativePosition = nearestEnemy.Position - Position;
                Vector2 dir = Util.ToUnitVector(Angle);
                if (Vector2.Dot(dir, relativePosition) > relativePosition.Length() * 0.85f)
                {
                    if (CD[0] <= 0)
                    {
                        List<Entity> bullets = [];
                        for (int i = 0; i < 3; i++)
                        {
                            bullets.Add(NewPulseShot(Vector2.Zero, Vector2.Zero, 0, 0, Team, damage, true, 0));
                        }
                        Engine.EntityManager.Add(NewSplitter(Position, Velocity + dir * 10 + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) * 2, Angle, Team, damage, bullets, 0.25f));
                        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Position);
                        CD[0] = 0.4f;
                    }
                }
                else
                {
                    CD[0] += Engine.DeltaSeconds;
                }
            }
            if (Health <= 0)
            {
                isExpired = true;
                Explode(0, 0);
                SoundManager.PlaySound(Assets.Get(Sound.Explosion), Position);
            }
            if (Health < MaxHealth)
            {
                int diff = Math.Min(newMax - Health, (int)buffer);
                buffer -= diff;
                CD[1] = 1;
                if (diff < (newMax - Health))
                {
                    newMax = Health;
                }
                Health = newMax;
            }
            if (CD[1] <= 0 && buffer < 10)
            {
                buffer += Engine.DeltaSeconds;
            }
            if (_parent.isExpired)
            {
                isExpired = true;
            }
            yield return 0;
        }
    }
    IEnumerable<int> DeadeyeBoss()
    {
        int damage = 8;
        CD =
        [
            0, //Gun 
            10 //Ability
        ];
        float bullets = 0;
        while (true)
        {
            Vector2 targetVector;
            var nearestEnemy = Engine.EntityManager.NearestEnemy(this);
            if (nearestEnemy != null)
            {
                Vector2 relativeVelocity = nearestEnemy.Velocity - Velocity;
                Velocity += relativeVelocity * Engine.DeltaSeconds * 2;
                targetVector = nearestEnemy.Position - Position;
                Vector2 relativePosition = nearestEnemy.Position - Position;
                float distance = relativePosition.Length();
                Vector2 velocityOffset = relativePosition * (distance - 150) / distance;
                Velocity += velocityOffset * Engine.DeltaSeconds + GetNormalizedAcceleration() * 2;
                if (CD[0] <= 0)
                {
                    Engine.EntityManager.Add(NewSplitter(Position, Velocity + Util.ToUnitVector(Angle) * 8, Angle, Team, 6,
                        [NewAssassinShot(default, default, 0, 0, Team, damage)], 0.5f, 0, true));
                    SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Position);
                    CD[0] = 1;
                }
                if (CD[1] <= 0 && bullets < 5)
                {
                    float cooldown = 1.5f - 0.25f * bullets;
                    Engine.EntityManager.Add(NewSplitter(Position, Velocity + Util.ToUnitVector(Angle + 0.1f * bullets) * 8, Angle + 0.1f * bullets, Team, 6,
                        [NewAssassinShot(default, default, 0, 0, Team, damage)], cooldown, 0, true));
                    Engine.EntityManager.Add(NewSplitter(Position, Velocity + Util.ToUnitVector(Angle - 0.1f * bullets) * 8, Angle - 0.1f * bullets, Team, 6,
                        [NewAssassinShot(default, default, 0, 0, Team, damage)], cooldown, 0, true));
                    SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Position);
                    CD[1] = 0.25f;
                    CD[0] = 1;
                    bullets++;
                    if (bullets >= 5)
                    {
                        bullets = 0;
                        CD[1] = 5f * (Health / MaxHealth + 1);
                    }
                }
            }
            else
            {
                targetVector = Velocity;
            }
            if (Health <= 0)
            {
                isExpired = true;
                Explode(10, ColliderRadius);
                if (Engine.SaveGame.GiveWeapon)
                {
                    Engine.EntityManager.Add(new CrackShot() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 10, AngularVelocity = this.AngularVelocity });
                }
                else
                {
                    Engine.EntityManager.Add(new Assault() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 10, AngularVelocity = this.AngularVelocity });
                }
            }
            targetAngle = Util.ToAngle(targetVector);
            RotateTowards(targetAngle);
            yield return 0;
        }
    }
    public static Enemy NewDeadeyeBoss(Vector2 position, Vector2 velocity, float angle, Team _team = Team.Hostile)
    {
        Enemy boss = new(position, velocity, angle, 80, Assets.Get(Sprites.DeadeyeBoss), _team);
        boss.AddComponent(new Behaviour(boss).AddBehaviour(boss.DeadeyeBoss()));
        return boss;
    }
    IEnumerable<int> ContinuumBoss(bool isWeak = false)
    {
        int damage = 6;
        CD = 
        [
            0, //Weapon
            0, //Enemy loss timer
            1, //Direction change timer
            5, //Ability timer
            0, //Ability offset timer
            0, //Burnout timer
        ];
        int prevHealth = Health;
        float direction = 1;
        int abilityShots = 0;
        Entity nearestEnemy = null;
        StealthAbility = 2;
        SensingAbility = 1;
        while(true)
        {
            if (Health <= 0)
            {
                isExpired = true;
                Explode(10, ColliderRadius);
                if(!isWeak)
                {
                    if (Engine.SaveGame.GiveWeapon)
                    {
                        Engine.EntityManager.Add(new Fractal() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 10, AngularVelocity = this.AngularVelocity });
                    }
                    else
                    {
                        Engine.EntityManager.Add(new Turtle() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 10, AngularVelocity = this.AngularVelocity });
                    }
                }
            }
            if (prevHealth != Health && RevealDuration <= 0)
            {
                Health = prevHealth;
            }
            var enemy = Engine.EntityManager.NearestEnemy(this);
            if (enemy == nearestEnemy)
            {
                CD[1] = 10;
            }
            else
            {
                if (enemy == null)
                {
                    if (CD[1] <= 0)
                    {
                        nearestEnemy = null;
                    }
                }
                else
                {
                    nearestEnemy = enemy;
                    CD[1] = 10;
                }
            }
            if (CD[5] <= 0)
            {
                if (nearestEnemy != null)
                {
                    Vector2 relativeVelocity = nearestEnemy.Velocity - Velocity;
                    Velocity += relativeVelocity * Engine.DeltaSeconds * 5;
                    Vector2 relativePosition = nearestEnemy.Position - Position;
                    float distance = relativePosition.Length();
                    Vector2 velocityOffset = relativePosition * (distance - 150) / distance;
                    Velocity += (velocityOffset + GetNormalizedAcceleration() * 60 + Vector2.Normalize(new Vector2(relativePosition.Y, -relativePosition.X)) * 25 * direction) * Engine.DeltaSeconds;
                    if (CD[0] <= 0)
                    {
                        List<Entity> splitters = [];
                        for (int i = 0; i < 3; i++)
                        {
                            List<Entity> finalBullets = [];
                            for (int j = 0; j < 3; j++)
                            {
                                finalBullets.Add(NewPulseShot(Position, Velocity, 0, 0, Team, damage, false, 1));
                            }
                            splitters.Add(NewSplitter(Position, Velocity, 0, Player.Team, (int)(damage * 1.5f), finalBullets, 0.05f, 1));
                        }
                        var dir = Vector2.Normalize(relativePosition);
                        Engine.EntityManager.Add(NewSplitter(Position, nearestEnemy.Velocity + dir * 8, Util.ToAngle(dir), Team, damage * 2, splitters, 0.05f));
                        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Position);
                        RevealDuration += 1.5f;
                        CD[0] = 1.5f;
                    }
                    if (CD[3] <= 0 && CD[4] <= 0f)
                    {
                        List<Entity> splitters = [];
                        for (int i = 0; i < 3; i++)
                        {
                            List<Entity> finalBullets = [];
                            for (int j = 0; j < 3; j++)
                            {
                                finalBullets.Add(NewAssassinShot(Position, Velocity, 0, 0, Team, damage));
                            }
                            splitters.Add(NewSplitter(Position, Velocity, 0, Player.Team, (int)(damage * 1.5f), finalBullets, 0.2f, 1, true));
                        }
                        var dir = Vector2.Normalize(relativePosition);
                        Engine.EntityManager.Add(NewSplitter(Position, nearestEnemy.Velocity + dir * 4, Util.ToAngle(dir), Team, damage * 2, splitters, 0.2f));
                        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Position);
                        RevealDuration += 1.5f;
                        abilityShots++;
                        CD[0] = 1.5f;
                        CD[4] = 0.5f;
                        if (abilityShots >= 3)
                        {
                            CD[3] = 20;
                            abilityShots = 0;
                            CD[5] = 0.5f;
                            RevealDuration = 1f;
                        }
                    }
                }
                else
                {
                    Vector2 relativePosition = Engine.SaveGame.CurrentMission.Planet.Position - Position;
                    float distance = relativePosition.Length();
                    Vector2 velocityOffset = relativePosition * (distance - Engine.SaveGame.CurrentMission.Planet.ColliderRadius) / distance;
                    Velocity += (velocityOffset + Vector2.Normalize(new Vector2(relativePosition.Y, -relativePosition.X)) * 25 * direction) * Engine.DeltaSeconds;
                }
                if (CD[2] <= 0)
                {
                    if (Util.Random.Next(0, 5) == 0)
                    {
                        direction *= -1;
                    }
                    CD[2] = 1;
                }
                targetAngle = Util.ToAngle(Velocity);
                RotateTowards(targetAngle);
            }
            else
            {
                Velocity *= Util.FIED(0.1f);
                yield return 0;
            }
            prevHealth = Health;
            yield return 0;
        }
    }
    public static Enemy NewContinuumBoss(Vector2 position, Vector2 velocity, float angle, Team _team = Team.Hostile)
    {
        Enemy boss = new(position, velocity, angle, 120, Assets.Get(Sprites.ContinuumBoss), _team);
        boss.AddComponent(new Behaviour(boss).AddBehaviour(boss.ContinuumBoss()));
        return boss;
    }
    IEnumerable<int> ClockworkBoss(Enemy _cog)
    {
        int damage = 8;
        Engine.EntityManager.Add(_cog);
        hitSound = Assets.Get(Sound.ShieldHit);
        CD = 
        [
            0, //Cog timer
            0, //Ability timer
        ];
        ChildEnemy = true;
        int unitsPerShot = 0;
        int unitsPerAbility = 0;
        bool isFirst = true;
        while (true)
        {
            float frac;
            bool isPhase1 = _cog.Health > 0;
            if (isPhase1)
            {
                Health = MaxHealth;
                frac = 0.5f + (float)(_cog.Health) / (float)(_cog.MaxHealth) / 2;
            }
            else
            {
                if (isFirst)
                {
                    ChildEnemy = false;
                    hitSound = Assets.Get(Sound.Hit);
                    isFirst = false;
                }
                frac = 0.25f;
            }
            var nearestEnemy = Engine.EntityManager.NearestEnemy(this);
            if (nearestEnemy != null)
            {
                if (CD[0] <= 0)
                {
                    CD[0] = frac;
                    var relativeVel = nearestEnemy.Velocity - Velocity + (nearestEnemy.Position - Position) / 100;
                    float relativeSpeed = relativeVel.Length();
                    if (relativeSpeed > 0.5f)
                    {
                        Util.Explode(Position - relativeVel / relativeSpeed * 25, Velocity, 0, 8);
                        SoundManager.PlaySound(Assets.Get(Sound.Explosion), Position);
                        Velocity += relativeVel / relativeSpeed * Math.Min(10, relativeSpeed);
                    }

                    unitsPerShot++;
                    unitsPerAbility++;
                    if (nearestEnemy != null)
                    {
                        if (unitsPerShot >= 3)
                        {
                            unitsPerShot = 0;
                            Vector2 vel = Util.PredictEnemy(nearestEnemy, this, 9);
                            Engine.EntityManager.Add(NewPulseShot(Position, vel, Util.ToAngle(vel), 0, Team, damage, true, 1));
                            SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Position);
                        }
                    }
                }
                if (unitsPerAbility >= 10)
                {
                    if (CD[1] <= 0)
                    {
                        for (float angle = 0; angle < MathF.Tau; angle += MathF.PI / 2)
                        {
                            Vector2 dir = Util.ToUnitVector(angle + MathF.PI / 4 * (unitsPerAbility % 2));
                            Engine.EntityManager.Add(NewPulseShot(Position, Velocity + dir * 8, angle + MathF.PI / 4 * (unitsPerAbility % 2), 0, Team, damage, true, 1));
                        }
                        CD[1] = 0.5f * frac;
                        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Position);
                    }
                    if (unitsPerAbility >= 15)
                    {
                        if (!isPhase1)
                        {
                            Vector2 relPos = nearestEnemy.Position - Position;
                            Engine.EntityManager.Add(NewExplosive(Position, Velocity + Vector2.Normalize(relPos) * 12, 0, 0, Team, damage, 20));
                        }
                        unitsPerAbility = 0;
                    }
                }
            }
            if (_cog != null)
            {
                float val = (frac - CD[0]);
                _cog.Angle = Angle + val * val * MathF.PI / 2;
            }
            if (Health <= 0)
            {
                Explode(10, ColliderRadius);
                _cog.isExpired = true;
                isExpired = true;
                Engine.EntityManager.Add(Pickup.NewSpecializedParts(Position, GetNormalizedAcceleration() * 10, Angle, 0.1f));
                Engine.EntityManager.Add(new OrionEngine() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 5, AngularVelocity = this.AngularVelocity });
            }
            targetAngle = Util.ToAngle(Velocity);
            RotateTowards(targetAngle);
            yield return 0;
        }
    }
    public static Enemy NewClockworkBoss(Vector2 position, Vector2 velocity, float angle, Team _team = Team.Hostile)
    {
        Enemy boss = new(position, velocity, angle, 180, Assets.Get(Sprites.ClockworkBoss), _team);
        Enemy cog = new(position, velocity, angle, 180, Assets.Get(Sprites.Cog), _team);
        boss.AddComponent(new Behaviour(boss).AddBehaviour(boss.ClockworkBoss(cog)));
        cog.AddComponent(new Behaviour(cog).AddBehaviour(cog.Cog(boss)));
        return boss;
    }
    IEnumerable<int> Cog(Entity _parent)
    {
        while(true)
        {
            Position = _parent.Position - Util.ToUnitVector(_parent.Angle) * _parent.Texture.Height / 2;
            if (Health <= 0)
            {
                isExpired = true;
                Explode(0, 0);
                SoundManager.PlaySound(Assets.Get(Sound.Explosion), Position);
            }
            yield return 0;
        }
    }
    IEnumerable<int> EpitomeBoss()
    {
        int damage = 8;
        //Starts timer
        Engine.SaveGame.Player.StatusHolder.ApplyStatus(new Bomb());
        List<Enemy> wave = [];
        CD = 
        [
            0,
            0,
            0,
        ];
        int phase = 0;
        float a = 0;
        int bullets = 0;
        Enemy shield = null;
        Vector2 randomPos = Vector2.One;
        SoundManager.ChangeTrack(Assets.Get(Sound.finalBoss));
        while(true)
        {
            int newPhase = 3 - (int)MathF.Ceiling((float)Health * 3 / (float)MaxHealth);
            if (newPhase > phase)
            {
                phase = newPhase;
                switch (phase)
                {
                    case 1:
                        Enemy enemy = new(Position + new Vector2(0, -125), Velocity, Angle, 45, Assets.Get(Sprites.ExodusBoss), Team);
                        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.ExodusBoss(true)));
                        wave.Add(enemy);
                        shield = NewShield(this, 20, 100, 0, 1, Team);
                        wave.Add(shield);
                        Texture = Assets.Get(Sprites.EpitomeTwo);
                        Explode(0, 0);
                        SoundManager.PlaySound(Assets.Get(Sound.Explosion), Position);
                        break;
                    case 2:
                        if (shield != null)
                        {
                            shield.isExpired = true;
                        }
                        Enemy boss = new(Position, Velocity, Angle, 50, Assets.Get(Sprites.Engineer), Team);
                        boss.AddComponent(new Behaviour(boss).AddBehaviour(boss.PursuerBoss(true)).AddBehaviour(boss.AvoidProjectiles(1)));
                        wave.Add(boss);
                        StealthAbility = 2;
                        Texture = Assets.Get(Sprites.EpitomeThree);
                        Explode(0, 0);
                        SoundManager.PlaySound(Assets.Get(Sound.Explosion), Position);
                        break;
                    case 3:
                        CD[0] = 10;
                        CD[1] = 1;
                        break;
                    default:
                        break;
                }
                foreach (var enemy in wave)
                {
                    Engine.EntityManager.Add(enemy);
                }
            }
            if (phase == 0)
            {
                if (CD[0] <= 0 && Angle - targetAngle < 0.1f)
                {
                    CD[0] = Math.Max(0.05f, 0.55f - CD[1] / 2);
                    CD[1] = 1f;
                    SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Position);
                    Engine.EntityManager.Add(NewPulseShot(Position, Util.ToUnitVector(Angle) * 10 + Velocity, Angle, 0, Team, damage, true, 1));
                }
                var targetVector = Util.PredictEnemy(Engine.SaveGame.Player, this, 10);
                targetAngle = Util.ToAngle(targetVector);
                RotateTowards(targetAngle);
                if (Vector2.Distance(Position, Engine.SaveGame.Player.Position) > 200)
                {
                    GoToPosition(Engine.SaveGame.Player.Position, 2);
                }
                Velocity *= Util.FIED(0.06f);
            }
            else if (phase == 1)
            {
                Entity nearestProjectile = Engine.EntityManager.NearestProjectile(Position + Vector2.Normalize(Player.Position - Position) * 30, SensingAbility, Team);
                if (nearestProjectile != null)
                {
                    var pos = Vector2.Normalize(Position - nearestProjectile.Position);
                    var vel = Vector2.Normalize(Velocity - nearestProjectile.Velocity);
                    if ((pos.X * vel.X + pos.Y * vel.Y) < -0.5f)
                    {
                        int sign = Math.Sign(pos.X * vel.Y - vel.X * pos.Y);
                        if (sign == 0)
                        {
                            sign = 1;
                        }
                        Velocity += new Vector2(-pos.Y, pos.X) * sign * Engine.DeltaSeconds * 15;
                    }
                }
                if (Vector2.Distance(randomPos, Position - Engine.SaveGame.Player.Position) < 50)
                {
                    targetAngle = Util.ToAngle(Engine.SaveGame.Player.Position - Position);
                    RotateTowards(targetAngle, 0.1f);
                    if (CD[0] <= 0 && Vector2.Dot(Vector2.Normalize(Engine.SaveGame.Player.Position - Position), Util.ToUnitVector(Angle)) > 0.8f)
                    {
                        if (CD[1] <= 0)
                        {
                            CD[1] = 1.25f;
                            CD[0] = 1f;
                            randomPos = Util.ToUnitVector(Util.Random.NextSingle() * MathF.Tau) * 250;
                        }
                        else
                        {
                            CD[0] = 0.5f;
                        }
                        Engine.EntityManager.Add(NewMissile(Position, Util.ToUnitVector(Angle) * 10 + Velocity, Angle, Team, 2));
                        SoundManager.PlaySound(Assets.Get(Sound.MissileFire), Position);
                    }
                }
                else
                {
                    targetAngle = Util.ToAngle(Velocity);
                    RotateTowards(targetAngle, 0.1f);
                }
                GoToPosition(Engine.SaveGame.Player.Position + randomPos, 3 + (Engine.SaveGame.Player.Position + randomPos - Position).Length() / 100);
                Velocity *= Util.FIED(0.1f);
            }
            else if (phase == 2)
            {
                randomPos = Vector2.Normalize(GetNormalizedAcceleration()) * 500;
                DrawLine(Angle, CD[0], 1.5f);
                if (CD[0] <= 0)
                {
                    var enemies = Engine.EntityManager.Hitscan(Position, Util.ToUnitVector(Angle), 10000, false, out Vector2 _end, null);
                    if (enemies.Count > 0 && enemies[0].IsFriendly != IsFriendly)
                    {
                        enemies[0].Collide(20);
                    }
                    var emitter = new ParticleEmitter(Assets.Get(Sprites.Dot), 0.5f, Position, 0, 0, 0, 50, Color.Red, EmitterType.EmissionOverDistance) { particleFadeToColor = Color.Transparent };
                    emitter.Update();
                    emitter.position = _end;
                    emitter.Update();
                    CD[0] = 1.5f;
                }
                if (CD[0] > 0.05f)
                {
                    targetAngle = Util.ToAngle(Engine.SaveGame.Player.Position - Position);
                }
                RotateTowards(targetAngle, 0.1f);
                GoToPosition(Engine.SaveGame.Player.Position + randomPos, 3);
                Velocity *= Util.FIED(0.15f);
            }
            else
            {
                Velocity += (Engine.SaveGame.Player.Position + Vector2.Normalize(GetNormalizedAcceleration()) * 200 - Position) / 100;
                Velocity *= Util.FIED(0.15f);
                Angle = Util.ToAngle(Engine.SaveGame.Player.Position - Position);
                if (CD[1] <= 0)
                {
                    Util.Explode(Position + Util.ToUnitVector(Util.Random.NextSingle() * MathF.Tau) * 32 * Util.Random.NextSingle(), Velocity, 10, 5);
                    SoundManager.PlaySound(Assets.Get(Sound.Explosion), Position);
                    CD[1] = CD[0] / 10 + 0.1f;
                }
                if (CD[2] <= 0)
                {
                    a += MathF.Tau / 20;
                    if (bullets < 20)
                    {
                        Engine.EntityManager.Add(NewPulseShot(Position, Util.ToUnitVector(a) * 10, a, 0, Team, 10, true, 1));
                        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Position);
                        bullets++;
                        CD[2] = 0.05f;
                    }
                    else
                    {
                        a = Util.Random.NextSingle() * MathF.Tau;
                        Engine.EntityManager.Add(new FlameBolt(Position, Util.ToUnitVector(a) * 10 * Util.Random.NextSingle(), Team, 10));
                        bullets++;
                        if (bullets >= 23)
                        {
                            bullets = 0;
                        }
                        CD[2] = 0.5f;
                    }
                }
                if (CD[0] <= 0)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = Util.Random.NextSingle() * MathF.Tau;
                        Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Util.Random.NextSingle() * 3 + 3);
                        ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), 0.5f, Position, particleVelocity + Velocity, angle, 0, Color.Yellow, new Color(255, 0, 0, 0)));
                    }
                    for (int i = 0; i < 5; i++)
                    {
                        float angle = Util.Random.NextSingle() * MathF.Tau;
                        Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Util.Random.NextSingle() * 3 + 3);
                        ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), 0.5f, Position, particleVelocity + Velocity, angle, 0, Color.DarkSlateGray, Color.Transparent));
                    }
                    Engine.SaveGame.Player.StatusHolder.ApplyStatus(new Bomb());
                    isExpired = true;
                    SoundManager.ChangeTrack(null);
                    Explode(10, 10);
                    SoundManager.PlaySound(Assets.Get(Sound.Explosion), Position);
                }
            }
            wave = [.. wave.Where(x => x.Health > 0)];
            yield return 0;
        }
    }
    public static Enemy NewEpitomeBoss(Vector2 position, Vector2 velocity, float angle)
    {
        Enemy boss = new(position, velocity, angle, 500, Assets.Get(Sprites.EpitomeOne), Team.Hostile);
        boss.AddComponent(new Behaviour(boss).AddBehaviour(boss.EpitomeBoss()));
        return boss;
    }
    IEnumerable<int> Decoy()
    {
        int damage = 20;
        while (true)
        {
            Entity nearestEnemy = Engine.EntityManager.NearestEnemy(this, false);
            if (nearestEnemy != null)
            {
                Vector2 relativePosition = nearestEnemy.Position - Position;
                GoToPosition(nearestEnemy.Position - Vector2.Normalize(relativePosition) * 100, 3f);
                targetAngle = Util.ToAngle(relativePosition);
                RotateTowards(targetAngle, 0.1f);
            }
            Velocity *= Util.FIED(0.1f);
            if (Health <= 0)
            {
                Explode(damage, 200);
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Explosion), Position);
            }
            yield return 0;
        }
    }
    public static Enemy NewDecoy(Vector2 position, Vector2 velocity, float angle, Sprites _sprite, Team _team = Team.Hostile)
    {
        Enemy enemy = new(position, velocity, angle, 20, Assets.Get(_sprite), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.Decoy()));
        return enemy;
    }
    #endregion
    #region Enemies
    IEnumerable<int> Fighter()
    {
        int damage = 5;
        CD = [0];
        EnemyRange.particleVelocity = 250;
        float speed = 3;
        if (IsFriendly(Engine.SaveGame.Player))
        {
            speed = 7;
        }
        while (Health > 0)
        {
            Velocity *= 0.8f;
            Vector2 normalizedAcceleration = GetNormalizedAcceleration();
            Entity nearestEnemy = Engine.EntityManager.NearestEnemy(this);
            if(nearestEnemy == null)
            {
                if((Player.Position - Position).LengthSquared() > 1000)
                {
                    GoToPosition(Player.Position,(speed + (Player.Position - Position).Length() / 100));
                }
                Vector2 targetVector = Player.Position - Position + (Player.Velocity - Velocity) * 8;
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                yield return 0;
                continue;
            }
            else
            {
                Vector2 targetVector = nearestEnemy.Position - Position + (nearestEnemy.Velocity - Velocity) * 8;
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                RotateTowards(targetAngle);
                if (EntityManager.DistanceSqr(this, nearestEnemy) > 250 * 250)
                {
                    GoToPosition(nearestEnemy.Position, speed);
                }
                else
                {
                    Velocity += normalizedAcceleration * Engine.DeltaSeconds * 60;
                    if (CD[0] <= 0)
                    {
                        Engine.EntityManager.Add(NewPulseShot(Position, Util.ToUnitVector(Angle) * 8, Angle, 0, Team, damage, false));
                        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Position);
                        CD[0] = 1;
                    }
                }
            }
            yield return 0;
        }
    }
    public static Enemy NewFighter(Vector2 position, Vector2 velocity, float angle, Team _team = Team.Hostile)
    {
        Enemy enemy = new(position, velocity, angle, 8, Assets.Get(Sprites.Fighter), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.Fighter()).AddBehaviour(enemy.AvoidNearbyAllies()).AddBehaviour(enemy.EnemyDeath()));
        return enemy;
    }
    IEnumerable<int> Carrier()
    {
        int damage = 5;
        CD = [0];
        EnemyRange.particleVelocity = 500;
        while (Health > 0)
        {
            var nearestEnemy = Engine.EntityManager.NearestEnemy(this);
            if(nearestEnemy != null)
            {
                Vector2 targetVector = nearestEnemy.Position - Position + (nearestEnemy.Velocity - Velocity);
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                Vector2 gravityForce = GetNormalizedAcceleration();
                float distSqr = EntityManager.DistanceSqr(this, nearestEnemy);
                if (distSqr > 500 * 500)
                {
                    GoToPosition(nearestEnemy.Position, 2.5f);
                }
                else if (distSqr < 75 * 75)
                {
                    GoToPosition(nearestEnemy.Position, -1);
                    if (CD[0] <= 0)
                    {
                        Engine.EntityManager.Add(NewPulseShot(Position, Util.ToUnitVector(Angle) * 8, Angle, 0, Team, damage));
                        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Position);
                        CD[0] = 0.25f;
                    }
                }
                else
                {
                    Velocity += gravityForce * Engine.DeltaSeconds * 60 * 2;
                    if (CD[0] <= 0)
                    {
                        Engine.EntityManager.Add(NewMissile(Position, Util.ToUnitVector(Angle) * 2, Angle, Team));
                        SoundManager.PlaySound(Assets.Get(Sound.MissileFire), Position);
                        CD[0] = 5;
                    }
                }
                RotateTowards(targetAngle);
            }
            Velocity *= 0.8f;
            yield return 0;
        }
    }
    public static Enemy NewCarrier(Vector2 position, Vector2 velocity, float angle, Team _team = Team.Hostile)
    {
        Enemy enemy = new(position, velocity, angle, 15, Assets.Get(Sprites.Cruiser), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.Carrier()).AddBehaviour(enemy.AvoidNearbyAllies()).AddBehaviour(enemy.EnemyDeath()));
        return enemy;
    }
    IEnumerable<int> Sniper()
    {
        int damage = 8;
        CD = [0];
        EnemyRange.particleVelocity = 400;
        while (Health > 0)
        {
            var nearestEnemy = Engine.EntityManager.NearestEnemy(this);
            if(nearestEnemy != null)
            {
                float timeToHit;
                float prevTimeToHit = 0;
                Vector2 playerIterativePosition = nearestEnemy.Position;
                Vector2 gravityForce = GetNormalizedAcceleration();
                for (int i = 0; i < 1; i++)
                {
                    timeToHit = MathF.Sqrt(EntityManager.DistanceSqr(Position, playerIterativePosition)) / 15;
                    playerIterativePosition += nearestEnemy.Velocity * (timeToHit - prevTimeToHit);
                    prevTimeToHit = timeToHit;
                }
                Vector2 targetVector = (playerIterativePosition - Position);
                targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                if (EntityManager.DistanceSqr(this, nearestEnemy) > 400 * 400)
                {
                    GoToPosition(nearestEnemy.Position, 3);
                }
                else
                {
                    Velocity += gravityForce * Engine.DeltaSeconds * 60 * 2;
                    if (CD[0] <= 0 && MathF.Abs(targetAngle - Angle) < 0.1f)
                    {
                        Engine.EntityManager.Add(NewAssassinShot(Position, Util.ToUnitVector(Angle) * 15, Angle, 0, Team, damage));
                        SoundManager.PlaySound(Assets.Get(Sound.SniperFire), Position);
                        CD[0] = 2.5f;
                    }
                }
                RotateTowards(targetAngle);
            }
            Velocity *= 0.8f;
            yield return 0;
        }
    }
    public static Enemy NewSniper(Vector2 position, Vector2 velocity, float angle, Team _team = Team.Hostile)
    {
        Enemy enemy = new(position, velocity, angle, 5, Assets.Get(Sprites.Sniper), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.Sniper()).AddBehaviour(enemy.AvoidNearbyAllies()).AddBehaviour(enemy.EnemyDeath()));
        return enemy;
    }
    IEnumerable<int> Missile()
    {
        int damage = GetComponent<Attack>().Damage;
        EnemyRange.particleVelocity = 10;
        float fuel = 45;
        float deathCooldown = 2;
        GetComponent<Collide>().OnCollide = delegate(int _damage, bool _ignoreImmunity)
        {
            _damage = StatusHolder.ModifyDamage(_damage);
            if (_damage >= 0)
            {
                Health = 0;
                SoundManager.PlaySound(hitSound, Position);
                return true;
            }
            return false;
        };
        var col = Color.DarkRed;
        col.A = 0;
        ParticleEmitter engineParticles = new(Assets.Get(Sprites.Circle), 0.1f, Position, 0, MathF.PI/4, 2, 
            200f, Color.Yellow, EmitterType.EmissionOverTime) { isEmitterActive = false, particleFadeToColor = col };
        ParticleEmitter smokeParticles = new(Assets.Get(Sprites.Circle), 1, Position, 0, MathF.Tau, 0.25f, 10, new Color(0.33f, 0.33f, 0.33f), EmitterType.EmissionOverDistance) { particleFadeToColor = Color.Transparent };
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
                    Health = 0;
                }
            }
            if (Health <= 0)
            {
                Explode(damage / 2, 12);
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), Position);
            }
            var nearestEnemy = Engine.EntityManager.NearestEnemy(this, false);
            if (nearestEnemy == null || (nearestEnemy as Enemy != null && (nearestEnemy as Enemy).Health <= 0))
            {
                nearestEnemy = new Enemy(Position + 100 * new Vector2(MathF.Cos(Angle - MathF.PI / 2), MathF.Sin(Angle - MathF.PI / 2)),
                Vector2.Zero, 0, 0, null, Team);
            }

            var targetVector = Vector2.Normalize(nearestEnemy.Position - Position);
            targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
            var normalAcceleration = Vector2.Normalize(new Vector2(Velocity.Y, -Velocity.X));
            if (Velocity.Length() <= 0.01f)
            {
                normalAcceleration = Vector2.Zero;
            }
            float closingVelocity = Vector2.Dot(targetVector, Velocity);
            Vector2 futureTargetVector = nearestEnemy.Position + nearestEnemy.Velocity - Position - Velocity;
            float angleRateOfChange = (targetAngle - MathF.Atan2(futureTargetVector.X, -futureTargetVector.Y)) / Engine.DeltaSeconds;
            Vector2 accelerationVector = normalAcceleration * closingVelocity * angleRateOfChange * Engine.DeltaSeconds * 2;
            if(accelerationVector.LengthSquared() > 0.75f)
            {
                accelerationVector = Vector2.Normalize(accelerationVector) * 0.75f;
            }
            engineParticles.isEmitterActive = false;
            smokeParticles.isEmitterActive = false;
            if (fuel > 0)
            {
                Velocity += accelerationVector;
                if(MathF.Abs(angleRateOfChange) < 0.5f)
                {
                    Vector2 thrustForce = targetVector * Engine.DeltaSeconds * 8;
                    Velocity += thrustForce;
                    accelerationVector += thrustForce;
                }
                float fuelUsage = accelerationVector.Length();
                fuel -= fuelUsage;
                if (accelerationVector.LengthSquared() < 0.05f)
                {
                    Angle = MathF.Atan2(Velocity.Y, Velocity.X) + MathF.PI/2;
                }
                else
                {
                    Angle = MathF.Atan2(accelerationVector.X, -accelerationVector.Y);
                }

                engineParticles.isEmitterActive = true;
                smokeParticles.isEmitterActive = true;
            }
            engineParticles.sprayAngle = Angle + MathF.PI;
            engineParticles.offsetVelocity = Velocity;
            engineParticles.Update();
            Vector2 offset = Util.ToUnitVector(Angle) * 7;
            engineParticles.position = Position - offset;

            smokeParticles.Update();
            smokeParticles.position = Position - offset;
            if (EntityManager.DistanceSqr(this, nearestEnemy) < 10 * 10)
            {
                Explode(damage, 12);
                SoundManager.PlaySound(Assets.Get(Sound.Explosion), Position);
                isExpired = true;
            }

            yield return 0;
        }
    }
    public static Enemy NewMissile(Vector2 position, Vector2 velocity, float angle, Team _team, int _sensingAbility, int _damage, int _health)
    {
        Enemy enemy = new(position, velocity, angle, _health, Assets.Get(Sprites.Missile), _team) { SensingAbility = _sensingAbility };
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.Missile()).AddBehaviour(enemy.AvoidNearbyAllies()));
        enemy.AddComponent(new MissileTag(enemy));
        enemy.AddComponent(new Attack(enemy) { Damage = _damage });
        return enemy;
    }
    public static Enemy NewMissile(Vector2 position, Vector2 velocity, float angle, Team _team = Team.Hostile, int _sensingAbility = 1)
    {
        Enemy enemy = new(position, velocity, angle, 10, Assets.Get(Sprites.Missile), _team) { SensingAbility = _sensingAbility };
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.Missile()).AddBehaviour(enemy.AvoidNearbyAllies()));
        enemy.AddComponent(new MissileTag(enemy));
        enemy.AddComponent(new Attack(enemy) { Damage = 8 });
        return enemy;
    }
    IEnumerable<int> Shotgunner()
    {
        int damage = 5;
        Enemy shield = NewShield(this, 3, 25, 0, 0, Team);
        Engine.EntityManager.Add(shield);
        EnemyRange.particleVelocity = 200;
        CD = [0];
        while (Health > 0)
        {
            Vector2 targetVector = Player.Position - Position + (Player.Velocity - Velocity) * 8;
            targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
            if (EntityManager.DistanceSqr(this, Player) > 200 * 200)
            {
                GoToPosition(Player.Position, 3.5f);
            }
            else
            {
                if (CD[0] <= 0)
                {
                    int randomBulletCount = Util.Random.Next(4, 6);
                    for (int i = 0; i < randomBulletCount; i++)
                    {
                        float angleDegrees = (float)(Util.Random.NextDouble() - 0.5) * 30;
                        float offsetAngle = angleDegrees * MathF.PI / 180;
                        targetVector = Util.ToUnitVector(Angle + offsetAngle);
                        Engine.EntityManager.Add(NewPulseShot(Position, targetVector * 6, Angle + offsetAngle, 0, Team, damage, true));
                    }
                    SoundManager.PlaySound(Assets.Get(Sound.ShotgunFire), Position);
                    CD[0] = 1.2f;
                }
            }
            if (Angle > targetAngle && AngularVelocity > -0.02f)
            {
                AngularVelocity -= 0.01f;
            }
            if (Angle < targetAngle && AngularVelocity < 0.02f)
            {
                AngularVelocity += 0.01f;
            }
            Velocity *= 0.8f;
            yield return 0;
        }
    }
    public static Enemy NewShotgunner(Vector2 position, Vector2 velocity, float angle, Team _team = Team.Hostile)
    {
        Enemy enemy = new(position, velocity, angle, 10, Assets.Get(Sprites.Shotgunner), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.Shotgunner()).AddBehaviour(enemy.AvoidNearbyAllies()).AddBehaviour(enemy.EnemyDeath()));
        return enemy;
    }
    IEnumerable<int> Shield(Entity parent, float distance, float theta)
    {
        Color = Color.Orange;
        ChildEnemy = true;
        hitSound = Assets.Get(Sound.ShieldHit);
        while (true)
        {
            Angle = parent.Angle + theta;
            Position = parent.Position + new Vector2(MathF.Sin(Angle), -MathF.Cos(Angle)) * distance;
            Velocity = parent.Velocity;
            if(parent.isExpired || (parent as Enemy != null && (parent as Enemy).Health <= 0))
            {
                isExpired = true;
                parent = null;
            }
            if (Health <= 0)
            {
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), Position);
            }
            yield return 0;
        }
    }
    public static Enemy NewShield(Entity parent, float distance, int health, float theta, int size, Team _team)
    {
        Sprites shieldSprite;
        if (size == 0)
        {
            shieldSprite = Sprites.ShotgunShield;
        }
        else
        {
            shieldSprite = Sprites.OverloadShield;
        }
        Enemy enemy = new(parent.Position, parent.Velocity, parent.Angle, health, Assets.Get(shieldSprite), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.Shield(parent, distance, theta)));
        return enemy;
    }
    IEnumerable<int> Hovercraft()
    {
        int damage = 2;
        float thrust;
        float cooldown = 1;
        int shots = 0;
        float weaponCooldown = 2;
        Vector2 randomPos = Vector2.Zero;
        EnemyRange.particleVelocity = 250;
        var col = Color.DarkRed;
        col.A = 0;
        ParticleEmitter engineParticles = new(Assets.Get(Sprites.Circle), 0.15f, Vector2.Zero, 0, MathF.PI/4, 2, 150f, Color.Yellow, EmitterType.EmissionOverTime) { particleFadeToColor = col };
        while (Health > 0)
        {
            Entity nearestEnemy = Engine.EntityManager.NearestEnemy(this);
            Vector2 relativePosition = Position - nearestEnemy.Position;
            Vector2 normalizedAcceleration = GetNormalizedAcceleration() * 2;
            Vector2 Offset = Vector2.Normalize(Engine.SaveGame.CurrentMission.GetNormalizedAcceleration(nearestEnemy.Position)) * 100;
            Vector2 targetLocation = relativePosition - Offset;
            Vector2 targetAcceleration;
            if (Vector2.Distance(nearestEnemy.Position + Offset, Position) < 50)
            {
                if (cooldown > 0)
                {
                    cooldown -= Engine.DeltaSeconds;
                }
                else
                {
                    cooldown = 1;
                    randomPos = new Vector2(Util.Random.NextSingle() * 50 - 25, Util.Random.NextSingle() * 50 - 25);
                }
                targetAcceleration = Velocity - nearestEnemy.Velocity - normalizedAcceleration * 10 + (randomPos + targetLocation) * Engine.DeltaSeconds;
            }
            else
            {
                targetAcceleration = Velocity - nearestEnemy.Velocity + targetLocation * Engine.DeltaSeconds - normalizedAcceleration * normalizedAcceleration.Length() * targetLocation.Length() / 10;
            }
            targetAcceleration -= (nearestEnemy.Velocity - Velocity) / 10;
            targetAngle = MathF.Atan2(targetAcceleration.Y, targetAcceleration.X) - MathF.PI / 2;
            thrust = MathF.Min(3 + normalizedAcceleration.Length() * 10, (1 - MathF.Abs(targetAngle - Angle) / MathF.PI) * (targetAcceleration.Length()));
            Entity nearestProjectile = Engine.EntityManager.NearestProjectile(Position, SensingAbility, Team);
            if (nearestProjectile != null)
            {
                var pos = Vector2.Normalize(Position - nearestProjectile.Position);
                var vel = Vector2.Normalize(Velocity - nearestProjectile.Velocity);
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
            if (Vector2.Distance(Position, nearestEnemy.Position) < EnemyRange.particleVelocity)
            {
                if (weaponCooldown > 0)
                {
                    weaponCooldown -= Engine.DeltaSeconds;
                }
                else
                {
                    Engine.EntityManager.Add(NewPulseShot(Position, -Vector2.Normalize(relativePosition) * 8 + nearestEnemy.Velocity, MathF.Atan2(relativePosition.Y, relativePosition.X) - MathF.PI/2, 0, Team, damage, false));
                    SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Position);
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
            RotateTowards(targetAngle, 0.05f);
            Velocity += (new Vector2(MathF.Sin(Angle), -MathF.Cos(Angle)) * thrust) * Engine.DeltaSeconds;
            engineParticles.offsetVelocity = Velocity;
            engineParticles.sprayAngle = Angle + MathF.PI;
            engineParticles.speedOfEmission = thrust * 75;
            engineParticles.particleVelocity = 3 - 3 / (thrust + 1);
            engineParticles.Update();
            engineParticles.position = Position + new Vector2(-MathF.Sin(Angle), MathF.Cos(Angle)) * 8;
            yield return 0;
        }
    }
    public static Enemy NewHovercraft(Vector2 position, Vector2 velocity, float angle, Team _team = Team.Hostile)
    {
        Enemy enemy = new(position, velocity, angle, 12, Assets.Get(Sprites.Hovercraft), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.Hovercraft()).AddBehaviour(enemy.EnemyDeath()));
        return enemy;
    }
    IEnumerable<int> AdvancedFighter()
    {
        int damage = 3;
        CD = [0];
        EnemyRange.particleVelocity = 500;
        float tripleCooldown = 0;
        int shotCount = 0;
        Entity target = null;
        float trackTime = 0;
        Vector2 rand = Vector2.Zero;
        while (Health > 0)
        {
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

                float dist = Vector2.Distance(nearestEnemy.Position, Position);
                if (dist > 500)
                {
                    GoToPosition(nearestEnemy.Position, (2 + Math.Max(dist - 500, 0) / 500));
                    targetAngle = Util.ToAngle(Velocity);
                }
                else
                {
                    var targetVector = Util.PredictEnemy(nearestEnemy, this, 8);
                    targetAngle = Util.ToAngle(targetVector);
                    if (CD[0] <= 0 && MathF.Abs(targetAngle - Angle) < 0.05f)
                    {
                        if (tripleCooldown <= 0)
                        {
                            Texture2D tex = Assets.Get(Sprites.SpiralShot);
                            if (shotCount == 0)
                            {
                                SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Position);
                                var p1 = NewPulseShot(Position, Util.ToUnitVector(Angle) * 8, Angle, 0, Team, damage, false);
                                p1.Texture = tex;
                                p1.GetComponent<ExpireTimer>().TimeLeft = 3;
                                Engine.EntityManager.Add(p1);
                            }
                            else
                            {
                                var p1 = NewSpiralShot(Position, Util.ToUnitVector(Angle) * 8, Angle, 0, Team, damage, 0);
                                p1.Texture = tex;
                                p1.TimeLeft = 3;
                                Engine.EntityManager.Add(p1);
                                p1 = NewSpiralShot(Position, Util.ToUnitVector(Angle) * 8, Angle, 0, Team, damage, MathF.PI);
                                p1.Texture = tex;
                                p1.TimeLeft = 3;
                                Engine.EntityManager.Add(p1);
                            }
                            if (shotCount < 1)
                            {
                                shotCount++;
                                tripleCooldown = 0.02f;
                            }
                            else
                            {
                                shotCount = 0;
                                CD[0] = 1.5f;
                            }
                        }
                    }
                }
            }
            else
            {
                if (Vector2.Distance(rand, Position) < 500)
                {
                    rand = NewGoToLocation();
                }
                GoToPosition(rand, 5);
                targetAngle = Util.ToAngle(Velocity);
            }
            RotateTowards(targetAngle, 0.1f);
            Velocity += GetNormalizedAcceleration() * Engine.DeltaSeconds * 60;
            Velocity *= Util.FIED(0.05f);

            if (nearestEnemy == null)
            {
                if (trackTime > 0)
                {
                    trackTime -= Engine.DeltaSeconds;
                }
                else
                {
                    target = null;
                }
            }
            if (tripleCooldown > 0)
            {
                tripleCooldown -= Engine.DeltaSeconds;
            }
            yield return 0;
        }
    }
    public static Enemy NewAdvancedFighter(Vector2 position, Vector2 velocity, float angle, Team _team = Team.Hostile)
    {
        Enemy enemy = new(position, velocity, angle, 15, Assets.Get(Sprites.AdvancedFighter), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.AdvancedFighter()).AddBehaviour(enemy.AvoidNearbyAllies())
            .AddBehaviour(enemy.EnemyDeath()).AddBehaviour(enemy.AvoidProjectiles(0.5f)));
        return enemy;
    }
    IEnumerable<int> StealthFighter()
    {
        int damage = 10;
        EnemyRange.particleVelocity = 500;
        SensingAbility = -1;
        StealthAbility = 0;
        CD = [0];
        Entity target = null;
        float trackTime = 0;
        Vector2 rand = Position;
        while (Health > 0)
        {
            Velocity += GetNormalizedAcceleration() * Engine.DeltaSeconds * 60;
            Velocity *= 0.8f;
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
                Vector2 targetVector = nearestEnemy.Position - Position;
                targetAngle = MathF.Atan2(targetVector.Y, targetVector.X) + MathF.PI/2;
                float diff = MathF.Abs(Angle - targetAngle);
                RotateTowards(targetAngle, diff / 10);
                if (targetVector.Length() > 200)
                {
                    GoToPosition(nearestEnemy.Position, 15);
                }
                if (diff < 0.2f && CD[0] <= 0)
                {
                    var p1 = NewAssassinShot(Position, Vector2.Normalize(targetVector) * 300, Angle, 0, Team, damage);
                    p1.TimeLeft = 0.2f;
                    Engine.EntityManager.Add(p1);
                    CD[0] = 1;
                }
            }
            else
            {
                if (Vector2.Distance(rand, Position) < 500)
                {
                    rand = NewGoToLocation();
                }
                GoToPosition(rand, 5);
                targetAngle = MathF.Atan2(Velocity.Y, Velocity.X) + MathF.PI / 2;
                float diff = MathF.Abs(Angle - targetAngle);
                RotateTowards(targetAngle, diff / 10);
            }
            yield return 0;
        }
    }
    public static Enemy NewStealthFighter(Vector2 position, Vector2 velocity, float angle, Team _team = Team.Hostile)
    {
        Enemy enemy = new(position, velocity, angle, 8, Assets.Get(Sprites.StealthFighter), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.StealthFighter()).AddBehaviour(enemy.AvoidNearbyAllies()).AddBehaviour(enemy.EnemyDeath()));
        return enemy;
    }
    IEnumerable<int> Hunter()
    {
        CD = [0];
        EnemyRange.particleVelocity = 300;
        SensingAbility = 0;
        StealthAbility = 1;
        Entity target = null;
        GrapplingHook grapplingHook = null;
        float trackTime = 0;
        float hookCooldown = 0;
        Vector2 rand = Position;
        while (Health > 0)
        {
            Velocity += GetNormalizedAcceleration() * Engine.DeltaSeconds * 60;
            Velocity *= 0.8f;
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
                    if (Vector2.Distance(grapplingHook.Position, target.Position) < 50)
                    {
                        target.RevealDuration = 3f;
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
            if (Health <= 0 && grapplingHook != null)
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
                Vector2 targetVector = target.Position - Position;
                targetAngle = MathF.Atan2(targetVector.Y, targetVector.X) + MathF.PI / 2;
                float diff = MathF.Abs(Angle - targetAngle);
                RotateTowards(targetAngle, diff / 10);
                if (targetVector.Length() > 150)
                {
                    GoToPosition(target.Position, 15);
                }
                else if (diff < 0.1f && hookCooldown <= 0 && grapplingHook == null)
                {
                    grapplingHook = new GrapplingHook(Position, Vector2.Normalize(targetVector) * 30, Angle, this, Team);
                    Engine.EntityManager.Add(grapplingHook);
                    SoundManager.PlaySound(Assets.Get(Sound.Click), Position);
                    Engine.ShakeScreen(0.2f);
                    hookCooldown = 10;
                }
                if (diff < 0.2f && targetVector.Length() < 300 && CD[0] <= 0)
                {
                    var p1 = NewPulseShot(Position, Vector2.Normalize(targetVector) * 10, Angle, 0, Team, 5, false, 1);
                    p1.Texture = Assets.Get(Sprites.Microshot);
                    p1.GetComponent<ExpireTimer>().TimeLeft = 2;
                    Engine.EntityManager.Add(p1);
                    SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Position);
                    Engine.ShakeScreen(0.1f);
                    CD[0] = 0.5f;
                }
            }
            else
            {
                if (Vector2.Distance(rand, Position) < 500)
                {
                    rand = NewGoToLocation();
                }
                GoToPosition(rand, 5);
                targetAngle = MathF.Atan2(Velocity.Y, Velocity.X) + MathF.PI / 2;
                float diff = MathF.Abs(Angle - targetAngle);
                RotateTowards(targetAngle, diff / 10);
            }
            yield return 0;
        }
    }
    public static Enemy NewHunter(Vector2 position, Vector2 velocity, float angle, Team _team = Team.Hostile)
    {
        Enemy enemy = new(position, velocity, angle, 15, Assets.Get(Sprites.Hunter), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.Hunter()).AddBehaviour(enemy.AvoidNearbyAllies()).AddBehaviour(enemy.EnemyDeath()));
        return enemy;
    }
    IEnumerable<int> Healer()
    {
        int damage = 4;
        CD = [0];
        EnemyRange.particleVelocity = 300;
        float weaponCooldown = 0;
        while(Health > 0)
        {
            Velocity *= 0.8f;
            Entity nearestAlly = Engine.EntityManager.NearestAlly(this);
            Entity nearestEnemy = Engine.EntityManager.NearestEnemy(this);
            Entity nearestProjectile = Engine.EntityManager.NearestProjectile(Position, SensingAbility, Team);
            if (nearestAlly != null)
            {
                if (Vector2.Distance(nearestAlly.Position, Position) > 300)
                {
                    GoToPosition(nearestAlly.Position, 9);
                }
                else if(CD[0] <= 0)
                {
                    CD[0] = 5;
                    nearestAlly.Collide(-3);
                    for (float i = 0; i < 16; i++)
                    {
                        float angle = MathF.PI / 8 * i;
                        ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 1, Position, Util.ToUnitVector(angle), angle, 0, Color.Green, Color.Transparent));
                    }
                    SoundManager.PlaySound(Assets.Get(Sound.Interact), Position);
                }
            }
            else if(nearestEnemy != null && Vector2.Distance(nearestEnemy.Position, Position) < 500)
            {
                var dir = Vector2.Normalize(Position - nearestEnemy.Position);
                GoToPosition(Position + dir * 10, 5);
            }   
            if (nearestEnemy != null && weaponCooldown <= 0 && Vector2.Distance(nearestEnemy.Position, Position) < 300)
            {
                var dir = Vector2.Normalize(nearestEnemy.Position - Position);
                weaponCooldown = 1;
                Engine.EntityManager.Add(NewPulseShot(Position, dir * 10, MathF.Atan2(dir.Y, dir.X) + MathF.PI / 2, 0, Team, damage));
                SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Position);
            }
            if (nearestProjectile != null)
            {
                var pos = Vector2.Normalize(Position - nearestProjectile.Position);
                var vel = Vector2.Normalize(Velocity - nearestProjectile.Velocity);
                if ((pos.X * vel.X + pos.Y * vel.Y) < -0.75f)
                {
                    int sign = Math.Sign(pos.X * vel.Y - vel.X * pos.Y);
                    if (sign == 0)
                    {
                        sign = 1;
                    }
                    GoToPosition(Position + new Vector2(-pos.Y, pos.X) * sign * 10, 4);
                }
            }
            if (weaponCooldown > 0)
            {
                weaponCooldown -= Engine.DeltaSeconds;
            }
            RotateTowards(MathF.Atan2(Velocity.Y, Velocity.X) + MathF.PI / 2, 0.15f);
            yield return 0;
        }
    }
    public static Enemy NewHealer(Vector2 position, Vector2 velocity, float angle, Team _team = Team.Hostile)
    {
        Enemy enemy = new(position, velocity, angle, 10, Assets.Get(Sprites.Healer), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.Healer()));
        return enemy;
    }
    IEnumerable<int> Engineer()
    {
        int damage = 3;
        StealthAbility = 1;
        EnemyRange.particleVelocity = 500;
        CD = 
        [
            0, //Default
            10, //Construct cooldown
        ];
        float constructionTime = 0;
        Vector2 constructLocation = Vector2.Zero;
        bool constructing = false;
        Entity trackedEnemy = null;
        float enemyCooldown = 5;
        while (true)
        {
            var enemy = Engine.EntityManager.NearestEnemy(this);
            if (trackedEnemy == null)
            {
                trackedEnemy = enemy;
            }
            else
            {
                if (enemy == trackedEnemy)
                {
                    enemyCooldown = 5;
                }
                if (enemy == null && enemyCooldown > 0)
                {
                    enemyCooldown -= Engine.DeltaSeconds;
                }
            }
            if (enemyCooldown <= 0)
            {
                trackedEnemy = null;
            }
            Velocity *= 0.9f;
            if (CD[1] <= 0)
            {
                if (!constructing)
                {
                    constructLocation = NewGoToLocation();
                    constructing = true;
                    constructionTime = 10;
                }
                GoToPosition(constructLocation, 5);
            }
            else
            {
                if (trackedEnemy != null)
                {
                    GoToPosition(trackedEnemy.Position, 3);
                    if (Vector2.Distance(trackedEnemy.Position, Position) < 500 && CD[0] <= 0)
                    {
                        CD[0] = 1.5f;
                        Engine.EntityManager.Add(NewSpiralShot(Position, Velocity + Util.ToUnitVector(Angle) * 8, Angle, 0, Team, damage, 0));
                    }
                }
                else
                {
                    GoToPosition(constructLocation, 3);
                }
            }
            if (constructing && Vector2.Distance(Position, constructLocation) < 15)
            {
                if (constructionTime <= 0)
                {
                    Engine.EntityManager.Add(Pickup.NewTrap(Position, Vector2.Zero, 0, 0, 1, Team));
                    constructing = false;
                    CD[1] = 15;
                }
                else
                {
                    constructionTime -= Engine.DeltaSeconds;
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 0.5f, Position, Velocity + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) * 3, 0, Util.OneToNegOne() / 3, Color.Yellow, Color.Transparent));
                }
            }
            if (Velocity.Length() > 0)
            {
                targetAngle = Util.ToAngle(Velocity);
            }
            else
            {
                targetAngle = 0;
            }
            RotateTowards(targetAngle);
            yield return 0;
        }
    }
    public static Enemy NewEngineer(Vector2 position, Vector2 velocity, float angle, Team _team = Team.Hostile)
    {
        Enemy enemy = new(position, velocity, angle, 12, Assets.Get(Sprites.Engineer), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.Engineer()).AddBehaviour(enemy.EnemyDeath()).AddBehaviour(enemy.AvoidProjectiles(1)));
        return enemy;
    }
    IEnumerable<int> Wyrm(Enemy _parent)
    {
        int damage = 8;
        CD = [0];
        StealthAbility = 2;
        Vector2 randomLocation = NewGoToLocation();
        while (Health > 0)
        {
            if (CD[0] <= 0)
            {
                CD[0] = Util.Random.NextSingle() * 3 + 1;
                RevealDuration += 0.5f;
            }
            if (_parent == null || _parent.Health <= 0)
            {
                Entity enemy = Engine.EntityManager.NearestEnemy(this);
                if (enemy as Enemy != null)
                {
                    Vector2 relPos = (enemy.Position - Position);
                    float distance = relPos.Length();
                    Vector2 acceleration = (enemy.Velocity - Velocity) + (relPos) / distance * (distance + 15) / 10;
                    if (acceleration.LengthSquared() > 20 * 20)
                    {
                        acceleration = Vector2.Normalize(acceleration) * 20;
                    }
                    Velocity += acceleration * Engine.DeltaSeconds;
                    if (Vector2.DistanceSquared(enemy.Position, Position) < (enemy.ColliderRadius + ColliderRadius + 25) * (enemy.ColliderRadius + ColliderRadius + 10))
                    {
                        if (enemy.Collide(damage))
                        {
                            for (float angle = MathF.PI / 30; angle < MathF.Tau; angle += MathF.PI / 30)
                            {
                                Vector2 dir = Util.ToUnitVector(angle);
                                ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 0.5f, Position, Velocity + dir * 3, angle, 0, Color.Red, Color.Transparent));
                            }
                        }
                    }
                }
                else
                {
                    GoToPosition(randomLocation, 1);
                    if (Vector2.DistanceSquared(randomLocation, Position) < 100)
                    {
                        randomLocation = NewGoToLocation();
                    }
                }
                Velocity *= Util.FIED(0.1f);
                Angle = Util.ToAngle(Velocity);
            }
            yield return 0;
        }
    }
    public static Enemy NewWyrm(Vector2 position, Vector2 velocity, float angle)
    {
        List<Enemy> segments = [];
        //Head
        var enemy = new Enemy(position, velocity, angle, 8, Assets.Get(Sprites.Wyrm), Team.Hostile);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.Wyrm(null)).AddBehaviour(enemy.EnemyDeath()).AddBehaviour(SpawnWorm(segments)));
        Enemy head = enemy;

        //Segments
        for (int i = 0; i < 5; i++)
        {
            var _enemy = new Enemy(position, velocity, angle, 8, Assets.Get(Sprites.Wyrm), Team.Hostile);
            _enemy.AddComponent(new Behaviour(_enemy).AddBehaviour(_enemy.Wyrm(enemy))
                .AddBehaviour(_enemy.EnemyDeath()).AddBehaviour(_enemy.FollowNextSegment(enemy)));
            segments.Add(_enemy);
            enemy = _enemy;
        }
        return head;
    }
    #endregion
    #region Infrastructure
    IEnumerable<int> Mothership()
    {
        int damage = 8;
        CD = [0];
        EnemyRange.particleVelocity = 300;
        float furnaceCooldown = 15;
        float craftingCooldown = 12;
        int requiredCraftsLeft = 20;
        Pickup furnaceItem = null;
        bool currentlyCrafting = false;
        bool alert = false;
        while (true)
        {
            Velocity = Vector2.Zero;
            if (EventHandler.AcknowledgeMessage(Message.MothershipCraftItem))
            {
                currentlyCrafting = true;
            }
            if (EventHandler.AcknowledgeMessage(Message.MothershipUpdateFurnace))
            {
                furnaceItem = UI.FurnaceSlot.daughterItem;
            }
            if (Health <= 0)
            {
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), Position);
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
                    SoundManager.PlaySound(Assets.Get(Sound.Interact), Position);
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
                if (CD[0] <= 0)
                {
                    Entity nearestEnemy = Engine.EntityManager.NearestEnemy(this);
                    if (nearestEnemy != null)
                    {
                        Vector2 relativePosition = Position - nearestEnemy.Position;
                        if (relativePosition.Length() < EnemyRange.particleVelocity)
                        {
                            Engine.EntityManager.Add(NewAssassinShot(Position, -Vector2.Normalize(relativePosition) * 100 + nearestEnemy.Velocity, MathF.Atan2(relativePosition.Y, relativePosition.X) - MathF.PI / 2, 0, Team, damage));
                            SoundManager.PlaySound(Assets.Get(Sound.MissileFire), Position);
                            CD[0] = 3;
                        }
                    }
                }
            }
            if (requiredCraftsLeft <= 0)
            {
                Engine.SaveGame.CurrentMission.CompleteCustomRule(this);
            }
            if(!alert && requiredCraftsLeft == 19)
            {
                SoundManager.PlaySound(Assets.Get(Sound.Beep), Position);
                ParticleManager.Add(new Particle(null, 5, Position + new Vector2(0, -30), Velocity, Angle, 0, Color.Red, Color.Transparent) { drawText = "Alert: Enemies detected.\nDefend the mothership." });
                var comp = new WaveSpawner(Mission.T1, 1, false);
                Engine.SaveGame.CurrentMission.AddComponent(comp);
                comp.Initialize();
                alert = true;
            }
            yield return 0;
        }
    }
    public static Enemy NewMothership(Vector2 position, Vector2 velocity, float angle)
    {
        Enemy enemy = new(position, velocity, angle, 1000, Assets.Get(Sprites.Mothership), Team.Friendly);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.Mothership()));
        enemy.AddComponent(new Dockable(enemy, UI.MothershipMenu));
        return enemy;
    }
    IEnumerable<int> Turret()
    {
        Enemy turretCannon = NewTurretCannon(this);
        Engine.EntityManager.Add(turretCannon);
        while (true)
        {
            Velocity *= 0;
            turretCannon.Position = Position + new Vector2(8 * MathF.Sin(Angle), -8 * MathF.Cos(Angle));
            Entity nearestPickup = Engine.EntityManager.NearestItem(this, false);
            if (nearestPickup != null)
            {
                if (EntityManager.DistanceSqr(this, nearestPickup) < 2500 && Health <= MaxHealth - 15)
                {
                    Collide(-15);
                    nearestPickup.isExpired = true;
                    SoundManager.PlaySound(Assets.Get(Sound.Dock), Position);
                    turretCannon.Health = Health;
                }
            }
            if (Health <= 0)
            {
                isExpired = true;
                turretCannon.isExpired = true;
                Explode(20, ColliderRadius);
            }
            if (turretCannon.Health != Health)
            {
                Health = Math.Min(turretCannon.Health, Health);
                turretCannon.Health = Health;
            }
            yield return 0;
        }
    }
    public static Enemy NewTurret(Vector2 position, Vector2 velocity, float angle, Team _team)
    {
        Enemy enemy = new(position, velocity, angle, 400, Assets.Get(Sprites.TurretBase), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.Turret()));
        return enemy;
    }
    public static Enemy NewTurret(Vector2 position, Vector2 velocity, float angle)
    {
        return NewTurret(position, velocity, angle, Team.Friendly);
    }
    IEnumerable<int> TurretCannon(float _angle)
    {
        CD = [0];
        float bulletOffset = 4;
        EnemyRange.particleVelocity = 400;
        ChildEnemy = true;
        while (true)
        {
            Velocity *= 0;
            var dir = new Vector2(-MathF.Sin(_angle), MathF.Cos(_angle));
            var gunDir = new Vector2(-MathF.Sin(Angle), MathF.Cos(Angle));
            Vector2 offset = dir * (Size.Y / 2 + 150);
            Entity nearestEnemy = Engine.EntityManager.NearestEnemy(new Enemy(Position + offset, Vector2.Zero, 0, 0, null, Team), false);
            EnemyRange.position = Position + offset;
            if (nearestEnemy != null)
            {
                var relPos = Vector2.Normalize(nearestEnemy.Position - Position + Engine.SaveGame.CurrentMission.GetNormalizedAcceleration(nearestEnemy.Position) * 1000);
                float distance = (nearestEnemy.Position - Position - dir * (Size / 2)).Length();
                float dot = relPos.X * dir.X + relPos.Y * dir.Y;
                float cross = relPos.X * gunDir.Y - gunDir.X * relPos.Y;
                if (cross < -0.1f)
                {
                    Angle -= 10 * Engine.DeltaSeconds;
                }
                else if (cross > 0.1f)
                {
                    Angle += 10 * Engine.DeltaSeconds;
                }
                else if (distance < 400 && CD[0] <= 0 && dot < 0.5f)
                {
                    var rotatedOffset = gunDir * Size.Y / 2;
                    Engine.EntityManager.Add(NewMissile(Position - rotatedOffset + new Vector2(-dir.Y, dir.X) * bulletOffset, -gunDir * 8, Angle, Team));
                    Assets.Get(Sound.MissileFire).Play();
                    CD[0] = 0.9f;
                    bulletOffset *= -1;
                }
            }
            yield return 0;
        }
    }
    public static Enemy NewTurretCannon(Enemy parent)
    {
        Enemy enemy = new(parent.Position, parent.Velocity, parent.Angle, 800, Assets.Get(Sprites.TurretHead), parent.Team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.TurretCannon(parent.Angle)));
        return enemy;
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
            if (Health <= 0)
            {
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), Position);
            }
            if (furnaceItem != null)
            {
                furnaceCooldown -= Engine.DeltaSeconds;
                if (furnaceCooldown <= 0)
                {
                    Engine.SaveGame.Scrap++;
                    furnaceItem = null;
                    SoundManager.PlaySound(Assets.Get(Sound.Interact), Position);
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
            EventHandler.UpdateCraftingUI(12 - craftingCooldown, 12, Health);
            yield return 0;
        }
    }
    public static Enemy NewOrbiter(Vector2 position, Vector2 velocity, float angle)
    {
        Enemy enemy = new(position, velocity, angle, 300, Assets.Get(Sprites.Orbiter), Team.Friendly);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.Orbiter()));
        enemy.AddComponent(new Dockable(enemy, UI.MothershipMenu));
        enemy.AngularVelocity = -0.01f;
        return enemy;
    }
    IEnumerable<int> PickupDrone(float _distance)
    {
        bool currentlyLeaving = false;
        while (true)
        {
            if (EventHandler.AcknowledgeMessage(Message.EscapeDroneLeave))
            {
                currentlyLeaving = true;
                Engine.SaveGame.CurrentMission.CompleteCustomRule(this);
            }
            if (Health <= 0)
            {
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), Position);
            }
            if (currentlyLeaving)
            {
                Velocity += new Vector2(Engine.DeltaSeconds * 10, -Engine.DeltaSeconds * 5);
            }
            else
            {
                Velocity = Vector2.Zero;
                Position = Position * (1f - Engine.DeltaSeconds) - new Vector2(0, _distance) * Engine.DeltaSeconds;
            }
            yield return 0;
        }
    }
    public static Enemy NewPickupDrone(Vector2 position, float _distance)
    {
        Enemy enemy = new(position, Vector2.Zero, 0, 250, Assets.Get(Sprites.PickupDrone), Team.Friendly);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.PickupDrone(_distance)));
        enemy.AddComponent(new Dockable(enemy, UI.PickupDroneMenu));
        return enemy;
    }
    IEnumerable<int> DropPod(float _distance)
    {
        _distance = _distance + Texture.Height / 2 + 1;
        while (true)
        {
            Velocity = new Vector2(0, 15);
            if (Position.Y > -_distance)
            {
                isExpired = true;
                Engine.SaveGame.Player.Velocity = new Vector2(0, -1f);
                Engine.SaveGame.Player.Dock();
                SoundManager.PlaySound(Assets.Get(Sound.Explosion), Position);
            }
            yield return 0;
        }
    }
    public static Enemy NewDropPod(Vector2 position, float _distance)
    {
        Enemy enemy = new(position, Vector2.Zero, 0, 500, Assets.Get(Sprites.DropPod), Team.Friendly);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.DropPod(_distance)));
        enemy.AddComponent(new Dockable(enemy, null));
        enemy.GetComponent<Collide>().OnCollide = delegate (int _damage, bool _override)
        {
            if (_damage > 0)
            {
                enemy.isExpired = true;
            }
            return enemy.isExpired;
        };
        return enemy;
    }
    IEnumerable<int> Glider(float _distance)
    {
        bool isDocked = true;
        float xSpeed = Planet.GetOrbitalVelocity(new Vector2(0, _distance), Engine.SaveGame.CurrentMission.Planet.Position, Engine.SaveGame.CurrentMission.Planet.mass).X;
        while (true)
        {
            Velocity = new Vector2(xSpeed, xSpeed * 2 * (Position.Y - _distance) / Position.X);
            if (Position.X > 0 && isDocked)
            {
                Engine.SaveGame.Player.Dock(false);
                isDocked = false;
            }
            if (Position.X > 1000)
            {
                isExpired = true;
            }
            yield return 0;
        }
    }
    public static Enemy NewGlider(Vector2 position, float _distance)
    {
        Enemy enemy = new(position, Vector2.Zero, 8, 500, Assets.Get(Sprites.PickupDrone), Team.Friendly);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.Glider(_distance)));
        enemy.AddComponent(new Dockable(enemy, null));
        return enemy;
    }
    IEnumerable<int> Miner()
    {
        ParticleEmitter miningDebris = new(Assets.Get(Sprites.Circle), 0.1f, Position, Angle, MathF.PI/2, 2, 500, Color.Cyan, EmitterType.EmissionOverTime) 
        { particleFadeToColor = Color.Transparent, particleAngularVelocity = Util.OneToNegOne() / 2 };
        float healTimer = 30;
        while (true)
        {
            Velocity *= 0;
            if (Health <= 0)
            {
                Explode(4, ColliderRadius);
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), Position);
            }
            if (healTimer > 0)
            {
                healTimer -= Engine.DeltaSeconds;
            }
            else
            {
                if (Health < MaxHealth - 15)
                {
                    Collide(-15);
                }
                else
                {
                    if(IsFriendly(Engine.SaveGame.Player))
                    {
                        Engine.EntityManager.Add(ItemFactory.NewScrap(Position + Util.ToUnitVector(Angle) * 20, GetNormalizedAcceleration() * 15 + new Vector2(Util.OneToNegOne(), -Util.Random.NextSingle()) * 5, AngularVelocity));
                    }
                }
                healTimer = 30;
            }
            Entity nearestPickup = Engine.EntityManager.NearestItem(this, false);
            if (nearestPickup != null)
            {
                if (EntityManager.DistanceSqr(this, nearestPickup) < 2500 && Health <= MaxHealth - 15)
                {
                    Collide(-15);
                    nearestPickup.isExpired = true;
                    SoundManager.PlaySound(Assets.Get(Sound.Dock), Position);
                }
            }
            miningDebris.position = Position + new Vector2(-MathF.Sin(Angle), MathF.Cos(Angle)) * Texture.Height / 2;
            miningDebris.Update();
            yield return 0;
        }
    }
    public static Enemy NewMiner(Vector2 position, Vector2 velocity, float angle, Team _team)
    {
        Enemy enemy = new(position, velocity, angle, 600, Assets.Get(Sprites.Miner), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.Miner()));
        return enemy;
    }
    public static Enemy NewMiner(Vector2 position, Vector2 velocity, float angle)
    {
        return NewMiner(position, velocity, angle, Team.Friendly);
    }
    IEnumerable<int> MakeshiftMothership()
    {
        int damage = 8;
        CD = 
        [
            0, 
        ];
        float furnaceCooldown = 15;
        float craftingCooldown = 12;
        Pickup furnaceItem = null;
        bool currentlyCrafting = false;
        int tier = 1;
        int untilNextTier = 1;
        while (Health > 0)
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
            var dockableComponent = (GetComponent<Dockable>());
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
                    SoundManager.PlaySound(Assets.Get(Sound.Interact), Position);
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
                        MaxHealth = 400 + (int)(100 * MathF.Sqrt(tier));
                    }
                    Collide(-100);
                    currentlyCrafting = false;
                }
            }

            EventHandler.UpdateFurnaceUI(15f * tierBonus - furnaceCooldown, 15f * tierBonus, furnaceItem);
            EventHandler.UpdateCraftingUI(12f * tierBonus - craftingCooldown, 12f * tierBonus, untilNextTier);
            if (!dockableComponent.Entity.isExpired && (dockableComponent as Dockable).IsDocked)
            {
                if (tier > 1 && Input.NewMouseState.LeftButton == ButtonState.Pressed && CD[0] <= 0 && !UIManager.LockMouseInput)
                {
                    var targetVector = Vector2.Normalize(new Vector2(Mouse.GetState().X, Mouse.GetState().Y) - Engine.BackBuffer / 2 - Position + Engine.Camera.Position);
                    targetAngle = MathF.Atan2(targetVector.X, -targetVector.Y);
                    Engine.EntityManager.Add(NewPulseShot(Position, targetVector * 9 + Velocity, targetAngle, 0, Team, damage, true));
                    SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Position);
                    CD[0] = 0.75f;
                    if (tier > 3)
                    {
                        CD[0] = 0.5f;
                    }
                    Engine.ShakeScreen(0.2f);
                    Velocity -= targetVector / 4;
                }
                Vector2 direction = Vector2.Zero;
                bool isEngineActive = false;
                var directions = new Dictionary<Binding, Vector2>
                {
                    { Binding.Up, new Vector2(0, -1) },
                    { Binding.Left, new Vector2(-1, 0) },
                    { Binding.Down, new Vector2(0, 1) },
                    { Binding.Right, new Vector2(1, 0) }
                };
                foreach (var pair in directions)
                {
                    if (Input.IsDown(pair.Key))
                    {
                        direction += pair.Value;
                        isEngineActive = true;
                    }
                }
                if (isEngineActive)
                {
                    Angle = Angle * 0.5f + MathF.Atan2(direction.X, -direction.Y) * 0.5f;
                    Velocity += Util.ToUnitVector(Angle) * 60 * Engine.DeltaSeconds * 0.1f;
                }
            }
            Engine.SaveGame.CurrentMission.CalculateTrajectory(Position, Velocity, ColliderRadius);
            yield return 0;
        }
    }
    public static Enemy NewMakeshiftMothership(Vector2 position, Vector2 velocity, float angle, Team _team = Team.Friendly)
    {
        Enemy enemy = new(position, velocity, angle, 500, Assets.Get(Sprites.Mothership), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.MakeshiftMothership()).AddBehaviour(enemy.EnemyDeath()));
        enemy.AddComponent(new Dockable(enemy, UI.MothershipMenu));
        enemy.AddComponent(new KeyTag(enemy));
        return enemy;
    }
    IEnumerable<int> LargeMiner()
    {
        int damage = 5;
        var arms = new List<Enemy>()
        {
            NewLargeMinerArm(Position - Util.ToUnitVector(Angle + MathF.PI/2) * Texture.Width / 2 + new Vector2(2, 2), Velocity, Angle, Team, 0, this),
            NewLargeMinerArm(Position + Util.ToUnitVector(Angle + MathF.PI/2) * Texture.Width / 2 + new Vector2(-2, 2), Velocity, Angle, Team, 2.5f, this),
        };
        CD = 
        [
            0, //Default
            0 //Spark
        ];
        foreach (var arm in arms)
        {
            Engine.EntityManager.Add(arm);
        }
        while (true)
        {
            Velocity = Vector2.Zero;
            for(int i = 0; i < 2; i++)
            {
                var entity = arms[i];
                if (!entity.isExpired)
                {

                }
                else
                {
                    if (Util.Random.NextSingle() > CD[0])
                    {
                        CD[0] = 1.5f;
                        if (arms[0].isExpired && arms[1].isExpired)
                        {
                            CD[0] = 1.1f;
                        }
                        float sign = (i * 2 - 1);
                        var dir = Util.ToUnitVector(this.Angle + MathF.PI / 2);
                        var offset = new Vector2(Util.Random.NextSingle(), Util.Random.NextSingle() * 2 - 1);
                        float angle = MathF.Atan2(dir.Y, dir.X) + MathF.PI / 2;
                        var p1 = NewPulseShot(Position + dir * sign * (Texture.Width / 2 - 4), dir * sign * 5 + offset * 2 * sign, angle * sign, 0, Team, damage);
                        p1.Texture = Assets.Get(Sprites.Microshot);
                        p1.GetComponent<ExpireTimer>().TimeLeft = 3f;
                        Engine.EntityManager.Add(p1);
                        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Position);
                    }
                    if (Util.Random.NextSingle() > CD[1])
                    {
                        CD[1] = 1f;
                        if (arms[0].isExpired && arms[1].isExpired)
                        {
                            CD[1] = 0.8f;
                        }
                        float sign = (i * 2 - 1);
                        var dir = Util.ToUnitVector(this.Angle + MathF.PI / 2);
                        var offset = new Vector2(Util.Random.NextSingle(), Util.Random.NextSingle() * 2 - 1);
                        ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 0.5f, Position + dir * sign * (Texture.Width / 2 - 4), dir * sign * 3 + offset * sign, 0, 0, Color.Yellow, new Color(255, 0, 0, 0)));
                    }
                }
            }
            var nearestPickup = Engine.EntityManager.NearestItem(this, false);
            if (nearestPickup != null)
            {
                float dist = Vector2.Distance(nearestPickup.Position, Position);
                if (dist is < 1000 and > 50)
                {
                    Texture2D texture = Assets.Get(Sprites.Dot);
                    var direction = Vector2.Normalize(Position - nearestPickup.Position);
                    float angle = MathF.Atan2(direction.Y, direction.X);
                    float distance = Vector2.Distance(Position, nearestPickup.Position);
                    for (float i = 0; i < distance / texture.Height / 2; i++)
                    {
                        ParticleManager.Add(new Particle(texture, Position - direction * i * texture.Height * 2, angle, Color * ((1000f - distance) / 1000f)));
                    }
                    nearestPickup.Velocity += direction * Engine.DeltaSeconds * 4;
                }
                else if (dist < 50)
                {
                    nearestPickup.isExpired = true;
                    Collide(-20);
                }
            }
            if (Health != MaxHealth && (!arms[0].isExpired || !arms[1].isExpired))
            {
                var diff = MaxHealth - Health;
                foreach (var entity in arms)
                {
                    entity.Health -= diff / 2;
                }
                Health = MaxHealth;
            }
            if (Health <= 0)
            {
                isExpired = true;
                Explode(10, ColliderRadius);
                SoundManager.PlaySound(Assets.Get(Sound.Explosion), Position);
                Engine.EntityManager.Add(new SummonGrapplingHook() { Position = this.Position, Velocity = GetNormalizedAcceleration() * 10, AngularVelocity = this.AngularVelocity });
            }
            yield return 0;
        }
    }
    public static Enemy NewLargeMiner(Vector2 position, Vector2 velocity, float angle)
    {
        Enemy enemy = new(position, velocity, angle, 500, Assets.Get(Sprites.LargeMiner), Team.Hostile);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.LargeMiner()));
        return enemy;
    }
    IEnumerable<int> LargeMinerArm(float _pos, Entity _parent)
    {
        float pos = _pos;
        Vector2 initialPos = Position - _parent.Position;
        Vector2 dir = Util.ToUnitVector(Angle);
        bool createSparks = true;
        while(true)
        {
            Velocity = Vector2.Zero;
            pos += Engine.DeltaSeconds;
            if (pos < 3.75f)
            {
                Position = initialPos + _parent.Position + new Vector2(Util.Random.NextSingle() * 2 - 1, Util.Random.NextSingle() * 2 - 1) + dir * 5 * pos;
            }
            else if (pos < 4)
            {
                Position = initialPos + _parent.Position + dir * 20 - dir * 20 * ((pos - 3.75f) * (pos - 3.75f) * 16);
            }
            else
            {
                if (createSparks)
                {
                    for (int i = Util.Random.Next(7, 10); i > 0; i--)
                    {
                        ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), 0.5f, Position + new Vector2(0, this.Texture.Height / 2), dir + new Vector2(Util.Random.NextSingle() * 2 - 1, Util.Random.NextSingle() * 2 - 1) / 2, 0, 0, Color.Cyan, Color.Transparent));
                        Engine.ShakeScreen(0.1f);
                        SoundManager.PlaySound(Assets.Get(Sound.ShieldHit), Position);
                    }
                    createSparks = false;
                }
                Position = initialPos + _parent.Position + new Vector2(Util.Random.NextSingle() * 2 - 1, Util.Random.NextSingle() * 2 - 1) * (5f - pos) * (5f - pos) * 5;
            }
            if (pos > 5)
            {
                pos -= 5;
                createSparks = true;
            }
            if (Health <= 0)
            {
                isExpired = true;
                Explode(0, 0);
                SoundManager.PlaySound(Assets.Get(Sound.Explosion), Position);
            }
            yield return 0;
        }
    }
    public static Enemy NewLargeMinerArm(Vector2 position, Vector2 velocity, float angle, Team _team, float _pos, Entity _parent)
    {
        Enemy enemy = new(position, velocity, angle, 200, Assets.Get(Sprites.LargeMinerArm), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.LargeMinerArm(_pos, _parent)));
        return enemy;
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
                else if (Input.IsDown(Binding.WarpBackward))
                {
                    dir = -1;
                }
                else if (Input.IsDown(Binding.WarpForward))
                {
                    dir = 1;
                }
            }
            else
            {
                time += Engine.DeltaSeconds * (isThrough ? -5 : 1);
                float count = Math.Clamp(time, 0, 10);
                AngularVelocity = count / 350 * dir;
                float angle = this.Angle;
                for (float i = 0; i < count * count * 20 && !isThrough; i++)
                {
                    float maxCount = 2000;
                    float ratio = 1 - (i / maxCount) * (i / maxCount);
                    //Vector3 col = (new Vector3(126, 118, 230) * (1 - ratio) + new Vector3(72, 61, 139) * (ratio)) * (MathF.Sin(angle * 10 + ratio * 10 + time * 3)/3 + 0.67f);
                    Vector3 col = (new Vector3(0, 0, 0) * (1 - ratio) + new Vector3(72, 61, 139) * (ratio)) * (MathF.Sin(angle * 10 + ratio * 10 + time * 3) / 3 + 0.67f);
                    angle += 1.61803398875f;
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), Util.ToUnitVector(angle) * (Texture.Height / 2f) * ratio + Position, angle, new Color(col.X / 255, col.Y / 255, col.Z / 255)));
                }
                if (Util.Random.NextSingle() > 1f - Engine.DeltaSeconds * count / 5)
                {
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 10f, Util.ToUnitVector(Util.Random.NextSingle() * MathF.Tau) * (150 + Util.Random.NextSingle() * 300) + Position, new Vector2(Util.Random.NextSingle() - 0.5f, Util.Random.NextSingle() - 0.5f),
                        Util.Random.NextSingle() * MathF.Tau, Util.Random.NextSingle() - 0.5f, Color.SlateBlue * 0.5f, Color.Transparent));
                }
                if (!isThrough && Vector2.Distance(Position, Engine.SaveGame.Player.Position) < (Texture.Height / 2f) && count >= 10)
                {
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Explosion), 1f, Position, Vector2.Zero, 0, 0, Color.White, Color.Transparent));
                    SoundManager.PlayGlobalSound(Assets.Get(Sound.Full));
                    isThrough = true;
                    time = 10;
                    Engine.SaveGame.System += dir;
                    Engine.SaveGame.Player.Progression = -1;
                    Engine.SaveGame.CurrentMission.CompleteCustomRule(this);
                }
                if (isThrough)
                {
                    Engine.SaveGame.Player.Position = Position;
                    Engine.SaveGame.Player.Velocity = Velocity;
                }
            }
            yield return 0;
        }
    }
    public static Enemy NewWarpGate(Vector2 position, Vector2 velocity, float angle)
    {
        Enemy enemy = new(position, velocity, angle, 1000, Assets.Get(Sprites.WarpGate), Team.Friendly);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.WarpGate()));
        return enemy;
    }
    IEnumerable<int> QuantumResonator()
    {
        CD = [5];
        AngularVelocity = 0.01f;
        int waveCount = 0;
        while (true)
        {
            Velocity = Vector2.Zero;
            if (Engine.SaveGame.CurrentMission.Name == "???")
            {
                if (CD[0] <= 0)
                {
                    if (waveCount < 3)
                    {
                        for (float angle = MathF.PI / 30; angle < MathF.Tau; angle += MathF.PI/30)
                        {
                            Vector2 dir = Util.ToUnitVector(angle);
                            ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 0.5f, Position, dir * 2, angle, 0, Color.Cyan, Color.Transparent));
                        }
                        SoundManager.PlaySound(Assets.Get(Sound.Interact), Position);
                        CD[0] = 0.75f;
                    }
                    else if (waveCount == 3)
                    {
                        CD[0] = 4;
                    }
                    else
                    {
                        isExpired = true;
                        Explode(0, 0);
                        SoundManager.PlaySound(Assets.Get(Sound.Explosion), Position);
                        Enemy inferno = NewInfernoBoss(Engine.SaveGame.Player.Position + new Vector2(1000, 0), Vector2.Zero, 0);
                        Engine.EntityManager.Add(inferno);
                        SoundManager.ChangeTrack(Assets.Get(Sound.secretBoss));
                    }
                    waveCount++; 
                }
            }
            else
            {
                CD[0] = 5;
            }
                yield return 0;
        }
    }
    public static Enemy NewQuantumResonator(Vector2 _position)
    {
        var enemy = new Enemy(_position, Vector2.Zero, 0, 10, Assets.Get(Sprites.QuantumResonator), Team.Friendly);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.QuantumResonator()));
        return enemy;
    }
    IEnumerable<int> Communicator()
    {
        float cooldown = 5;
        while (true)
        {
            if (GetComponent<Collide>().WasHit)
            {
                var dir = Vector2.Normalize(new Vector2(Util.OneToNegOne(), Util.OneToNegOne()));
                SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Position);
                Engine.EntityManager.Add(NewAssassinShot(Position, dir * 10, MathF.Atan2(dir.Y, dir.X), 0, Team, 5, 1));
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
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 0.25f, Position, Util.ToUnitVector(angle) * 2, angle, 0, Color.Cyan, Color.Transparent));
                }
            }
            yield return 0;
        }
    }
    public static Enemy NewCommunicator(Vector2 _position, Vector2 _velocity, float _angle, Team _team = Team.Friendly)
    {
        var enemy = new Enemy(_position, _velocity, _angle, 400, Assets.Get(Sprites.Communicator), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.Communicator()).AddBehaviour(enemy.EnemyDeath()));
        return enemy;
    }
    IEnumerable<int> MassRelay()
    {
        float furnaceCooldown = 15;
        float craftingCooldown = 20;
        int requiredCraftsLeft = 18;
        Pickup furnaceItem = null;
        bool currentlyCrafting = false;
        List<Texture2D> tier =
        [
            Assets.Get(Sprites.MassRelayOne),
            Assets.Get(Sprites.MassRelayTwo),
            Assets.Get(Sprites.MassRelayThree),
            Assets.Get(Sprites.MassRelayFour),
        ];
        while (true)
        {
            Velocity = Vector2.Zero;
            if (EventHandler.AcknowledgeMessage(Message.MothershipCraftItem))
            {
                currentlyCrafting = true;
            }
            if (EventHandler.AcknowledgeMessage(Message.MothershipUpdateFurnace))
            {
                furnaceItem = UI.FurnaceSlot.daughterItem;
            }
            if (Health <= 0)
            {
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), Position);
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
                    SoundManager.PlaySound(Assets.Get(Sound.Interact), Position);
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
                    craftingCooldown = 20 - requiredCraftsLeft;
                    requiredCraftsLeft -= 1;
                    Collide(-100);
                    MaxHealth += 50;
                    Texture = tier[3 - (int)Math.Round((float)requiredCraftsLeft / 6)];
                    currentlyCrafting = false;
                }
            }
            EventHandler.UpdateFurnaceUI(15 - furnaceCooldown, 15, furnaceItem);
            EventHandler.UpdateCraftingUI(20 - craftingCooldown - requiredCraftsLeft, 20 - requiredCraftsLeft, requiredCraftsLeft);

            if (requiredCraftsLeft <= 0)
            {
                Engine.SaveGame.CurrentMission.CompleteCustomRule(this);
            }
            yield return 0;
        }
    }
    public static Enemy MassRelay(Vector2 _position, Vector2 _velocity, float _angle)
    {
        Enemy enemy = new(_position, _velocity, _angle, 200, Assets.Get(Sprites.MassRelayOne), Team.Friendly);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.MassRelay()));
        enemy.AddComponent(new Dockable(enemy, UI.MothershipMenu));
        return enemy;
    }
    IEnumerable<int> MeshNetworkNode()
    {
        float hackCD = 0;
        bool isDone = false;
        float max = 60;
        while(true)
        {
            if (EventHandler.AcknowledgeMessage(Message.Hack) && Vector2.Distance(Player.Position, Position) < 1000 && !IsFriendly(Engine.SaveGame.Player))
            {
                StealthAbility = -3;
                Team = Engine.SaveGame.Player.Team;
                UpdateColor();
                hackCD = max;
            }
            if(hackCD > 0)
            {
                hackCD -= Engine.DeltaSeconds;
                UI.HackTimer.SetInterval(max - hackCD, max);
            }
            else
            {
                StealthAbility = 1;
                if (IsFriendly(Engine.SaveGame.Player) && !isDone)
                {
                    Engine.SaveGame.CurrentMission.CompleteCustomRule(this);
                    isDone = true;
                }
            }
            yield return 0;
        }
    }
    public static Enemy NewMeshNetworkNode(Vector2 _position, Vector2 _velocity, float _angle)
    {
        var enemy = new Enemy(_position, _velocity, _angle, 1000, Assets.Get(Sprites.Mothership), Team.Hostile);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.MeshNetworkNode()));
        enemy.AddComponent(new Dockable(enemy, UI.HackMenu, false));
        return enemy;
    }
    IEnumerable<int> EnemySpawner()
    {
        CD = [5];
        while(true)
        {
            if (CD[0] <= 0)
            {
                int enemyType = Util.Random.Next(1, 4);
                CD[0] = enemyType * 3;
                switch (enemyType)
                {
                    case 1:
                        for(int i = 0; i < 2; i++)
                        {
                            Engine.EntityManager.Add(NewFighter(Position + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) * 10, Velocity, Angle, Team));
                        }
                        break;
                    case 2:
                        Engine.EntityManager.Add(NewSniper(Position, Velocity, Angle, Team));
                        break;
                    case 3:
                        Engine.EntityManager.Add(NewCarrier(Position, Velocity, Angle, Team));
                        break;
                }
            }
            yield return 0;
        }
    }
    public static Enemy NewEnemySpawner(Vector2 _position, Vector2 _velocity, float _angle, Team _team = Team.Hostile)
    {
        var enemy = new Enemy(_position, _velocity, _angle, 500, Assets.Get(Sprites.Orbiter), _team);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.EnemySpawner()));
        return enemy;
    }
    IEnumerable<int> CrashedShip()
    {
        int damage = 5;
        //Shoots at nearby enemies
        //Need to spend scrap to reload ammunition, or to repair the ship
        bool hasLanded = false;
        bool isRepaired = false;
        ParticleEmitter engineParticles = new(Assets.Get(Sprites.Circle), 1f, Position, 0, MathF.PI/2, 1,
         200f, Color.LightGray, EmitterType.EmissionOverTime)
        { particleFadeToColor = Color.Transparent };
        bool currentlyCrafting = false;
        Pickup furnaceItem = null;
        float furnaceCooldown = 15;
        float craftingCooldown = 12;
        int requiredCraftsLeft = 10;
        int ammo = 200;
        CD = [0, 30];
        EnemyRange.particleVelocity = 500;
        while (true)
        {
            //UI handling
            if (EventHandler.AcknowledgeMessage(Message.MothershipCraftItem))
            {
                currentlyCrafting = true;
            }
            if (EventHandler.AcknowledgeMessage(Message.MothershipUpdateFurnace))
            {
                furnaceItem = UI.FurnaceSlot.daughterItem;
            }
            //Fail state
            if (Health <= 0)
            {
                isExpired = true;
                SoundManager.PlaySound(Assets.Get(Sound.Death), Position);
            }
            //Smelting
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
                    SoundManager.PlaySound(Assets.Get(Sound.Interact), Position);
                }
            }
            else
            {
                furnaceCooldown = 15;
            }
            //Repairing
            if (currentlyCrafting)
            {
                craftingCooldown -= Engine.DeltaSeconds;
                if (craftingCooldown <= 0)
                {
                    craftingCooldown = 12;
                    if (ammo > 0)
                    {
                        requiredCraftsLeft -= 1;
                        Collide(-100);
                        ammo = Math.Clamp(ammo + 25, 0, 200);
                    }
                    else
                    {
                        ammo = 200;
                    }
                    currentlyCrafting = false;
                }
            }

            //Updating UI
            EventHandler.UpdateFurnaceUI(15 - furnaceCooldown, 15, furnaceItem);
            EventHandler.UpdateCraftingUI(12 - craftingCooldown, 12, requiredCraftsLeft);

            //Firing at enemies
            if (ammo > 0 && CD[0] <= 0)
            {
                Entity nearestEnemy = Engine.EntityManager.NearestEnemy(this);
                if (nearestEnemy != null)
                {
                    Vector2 relativePosition = nearestEnemy.Position-Position;
                    if (relativePosition.Length() < EnemyRange.particleVelocity)
                    {
                        Engine.EntityManager.Add(NewPulseShot(Position, Vector2.Normalize(relativePosition + nearestEnemy.Velocity * relativePosition.Length() / 10) * 10 + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()), MathF.Atan2(relativePosition.Y, relativePosition.X) + MathF.PI / 2, 0, Team, damage));
                        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Position);
                        CD[0] = 0.1f;
                    }
                }
            }
            //Success state
            if (requiredCraftsLeft <= 0 && !isRepaired)
            {
                isRepaired = true;
                Engine.SaveGame.CurrentMission.CompleteCustomRule(this);
                Engine.DialogueManager.Add(new Dialogue("Repair complete, let's get out of here.", null));
            }
            //Story beats
            engineParticles.position = Position;
            engineParticles.Update();
            if(CD[1] <= 0)
            {
                if(Position.X < 0)
                {
                    Velocity = new Vector2(6, 6);
                }
                else
                {
                    if(!hasLanded)
                    {
                        hasLanded = true;
                        Explode(100, 3*Size.Length());
                        Engine.ShakeScreen(1);
                    }
                    Velocity = Vector2.Zero;
                }   
            }
            if (CD[1] is < 0 and > (-10))
            {
                CD[1] = -30;
                Engine.DialogueManager.Add(new Dialogue("Incoming: Ship going down, I repeat, ship going down!", null));
                Engine.DialogueManager.Add(new Dialogue("All allies, clear immediately!", null));
                Engine.DialogueManager.Add(new Dialogue("We are down! Requesting assistance!", null));
                Engine.DialogueManager.Add(new Dialogue("Dammit, that was our escape route...", null));
                Engine.DialogueManager.Add(new Dialogue("Get that ship online ASAP!", null));
            }
            yield return 0;
        }
    }
    public static Enemy NewCrashedShip(Vector2 _position, Vector2 _velocity, float _angle)
    {
        var enemy = new Enemy(_position, _velocity, _angle, 1000, Assets.Get(Sprites.Mothership), Team.Friendly);
        enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.CrashedShip()));
        enemy.AddComponent(new Dockable(enemy, UI.MothershipMenu, true));
        return enemy;
    }
    #endregion
    public static Enemy NewTrader(Vector2 _position, Vector2 _velocity, float _angle)
    {
        var enemy = new Enemy(_position, _velocity, _angle, 400, Assets.Get(Sprites.Trader), Team.Friendly);
        enemy.AddComponent(new Dockable(enemy, UI.UpgradeMenu, false));
        return enemy;
    }
    public static Enemy NewScrambled(Vector2 _position, Vector2 _velocity, float _angle) //Just for fun!
    {
        var enemy = new Enemy(_position, _velocity, _angle, Util.Random.Next(5,10), Assets.Get((Sprites)(Util.Random.Next((int)Sprites.Fighter, (int)Sprites.Hunter))));
        var b = new Behaviour(enemy);
        enemy.AddComponent(b);
        for(int i = 0; i < Util.Random.Next(2, 4); i++)
        {
            switch(Util.Random.Next(0, 10))
            {
                case 0: b.AddBehaviour(enemy.Fighter()); break;
                case 1: b.AddBehaviour(enemy.AdvancedFighter()); break;
                case 2: b.AddBehaviour(enemy.StealthFighter()); break;
                case 3: b.AddBehaviour(enemy.Sniper()); break;
                case 4: b.AddBehaviour(enemy.Carrier()); break;
                case 5: b.AddBehaviour(enemy.Shotgunner()); break;
                case 6: b.AddBehaviour(enemy.Hovercraft()); break;
                case 7: b.AddBehaviour(enemy.Hunter()); break;
                case 8: b.AddBehaviour(enemy.Healer()); break;
                case 9: b.AddBehaviour(enemy.Engineer()); break;
                default: break;
            }
        }
        if(Util.Random.Next(0, 2) == 0)
        {
            b.AddBehaviour(enemy.AvoidProjectiles(0.5f));
        }
        b.AddBehaviour(enemy.EnemyDeath());
        return enemy;
    }
}
