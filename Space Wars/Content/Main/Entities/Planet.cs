using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Components;
using Space_Wars.Content.Main.MissionComponents;
using Space_Wars.Content.Main.Particles;
using System;
using System.Linq;

namespace Space_Wars.Content.Main.Entities;

public class Planet : Entity, ICollider
{
    public float mass;
    public override float ColliderRadius { get; }
    private Texture2D atmosphere;
    public bool isImmovable = false;
    private bool hasRing;
    private bool isSun = false;
    public bool EasterEgg { get; set; } = false;
    public override Color Color { get; set; }
    private float atmosphereStrength = 0;
    public float RingOffset { get; set; } = 0;

    //Remember to update the clone function too!
    public Planet(Vector2 _position, Vector2 _velocity, float _mass, float _radius, bool _isImmovable, Color _color, bool _hasRing = false, float _atmosphereStrength = 0, float _ringOffset = 0, bool _isSun = false)
        : base(_position, _velocity, 0, 0)
    {
        AddComponent(new Temp(this));
        AddComponent(new StationaryEmitter(this) { ParticleEmitter = new ParticleEmitter(Assets.Get(Sprites.Dot), 10, Position, 0, 0, 0, 10f, _color * 0.5f, EmitterType.EmissionOverDistance) { particleFadeToColor = Color.Transparent } });
        mass = _mass;
        ColliderRadius = _radius * 50;
        isImmovable = _isImmovable;
        Color = _color;
        hasRing = _hasRing;
        atmosphereStrength = _atmosphereStrength;
        isSun = _isSun;
        if(_atmosphereStrength > 0)
        {
            atmosphere = Engine.Self.RenderAtmosphere(AtmosphereRadius(), _atmosphereStrength, ColliderRadius, Color, this, _isSun);
        }
        RingOffset = _ringOffset;
    }
    public Vector2 GetAcceleration(Vector2 _position)
    {
        Vector2 relativePosition = _position - Position;
        if (relativePosition == Vector2.Zero)
        {
            relativePosition = Vector2.One;
        }
        return Vector2.Normalize(-relativePosition) * mass / relativePosition.LengthSquared();
    }
    public float GetOrbitalVelocity(Vector2 _position)
    {
        float distance = (_position - Position).Length();
        return MathF.Sqrt(mass / distance);
    }
    public static Vector2 GetOrbitalVelocity(Vector2 _position, Vector2 _planetPosition, float _planetMass)
    {
        Vector2 relativePosition = (_position - _planetPosition);
        float distance = relativePosition.Length();
        float speed = MathF.Sqrt(_planetMass / distance);
        return new Vector2(-relativePosition.Y, relativePosition.X) / distance * speed;
    }
    public bool Collide(Entity _entity)
    {
        Vector2 relativePosition = _entity.Position - Position;
        if(false)
        {
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), _entity.Position + _entity.Velocity * Engine.DeltaSeconds, 0, Color.Red));
            var dir = Vector2.Normalize(_entity.Velocity);
            float closestLength = -(relativePosition.X * dir.X + relativePosition.Y * dir.Y);
            float closestDistance = Vector2.Distance((dir * closestLength + _entity.Position), _entity.Position);
            if (closestLength > 0 && closestLength < _entity.Velocity.Length() && closestDistance < _entity.ColliderRadius)
            {
                float discriminant = MathF.Sqrt(_entity.ColliderRadius * _entity.ColliderRadius - closestDistance * closestDistance);
                _entity.Position += dir * (closestLength - discriminant);
                _entity.Velocity = Vector2.Zero;
            } 
        }
        else if(relativePosition.Length() <= ColliderRadius + _entity.ColliderRadius)
        {
            var normalVector = Vector2.Normalize(relativePosition);
            var frictionVector = new Vector2(normalVector.Y, -normalVector.X);
            var relativeVelocity = Velocity - _entity.Velocity;
            int collisionForce = (int)Math.Floor((relativeVelocity).Length() / 2);
            if (_entity as Pickup == null && (collisionForce > 5 || _entity.GetComponent<Attack>() != null))
            {
                _entity.Collide(collisionForce);
            }
            float verticalVelocity = Math.Max(0, Vector2.Dot(relativeVelocity, normalVector));
            _entity.Velocity += normalVector * verticalVelocity + frictionVector * Vector2.Dot(relativeVelocity, frictionVector) * 0.1f;
            _entity.Position += normalVector * (ColliderRadius + _entity.ColliderRadius - Vector2.Distance(Position, _entity.Position));
            float val = (int)MathF.Sqrt(collisionForce);
            if (verticalVelocity > 1)
            {
                for (int i = 0; i < val * 1.5f; i++)
                {
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 10, normalVector * (ColliderRadius + 2) + Position, normalVector * val + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) * val / 2 + Velocity, 0, 0, Color * 0.75f, Color.Transparent) { experienceGravity = true });
                }
            }
            _entity.ConductHeat(Temperature, 5);
            return true;
        }
        return false;
    }
    public bool IsColliding(Vector2 _position, Vector2 _velocity, float _colliderRadius, bool _override, out float _end)
    {
        _end = _velocity.Length();
        Vector2 relativePosition = Position - _position;
        var dir = Vector2.Normalize(_velocity);
        float closestLength = (relativePosition.X * dir.X + relativePosition.Y * dir.Y);
        float closestDistance = (Vector2.Distance((dir * closestLength + _position), Position));
        float discriminant = MathF.Sqrt(ColliderRadius * ColliderRadius - closestDistance * closestDistance);
        if (closestLength - discriminant < _end && closestLength - discriminant > 0)
        {
            _end = closestLength - discriminant;
            return true;
        }
        return false;
    }
    public string Print() { return ""; }
    public Vector2 AttractObject(Entity _entity)
    {
        Vector2 relativePosition = _entity.Position - Position;
        if (relativePosition == Vector2.Zero)
        {
            _entity.isExpired = true;
            return Vector2.Zero;
        }
        float distance = relativePosition.Length();
        if (distance >= ColliderRadius + _entity.ColliderRadius)
        {
            Vector2 acceleration = Vector2.Normalize(-relativePosition) * mass / relativePosition.LengthSquared();
            if (distance < AtmosphereRadius())
            {
                float strength = GetAtmosphereDensity(distance);
                //Drag
                Vector2 relativeVelocity = (Velocity - _entity.Velocity);
                Vector2 drag = relativeVelocity * strength / 40;
                acceleration += drag;
                float q = (drag * relativeVelocity * relativeVelocity).Length() / 15;
                for(float i = 0; i < 5 * 60 * Engine.DeltaSeconds; i++)
                {
                    if (Util.Random.NextSingle() < q * q / 2)
                    {
                        Vector2 pos = Util.ToUnitVector(Util.Random.NextSingle() * MathF.Tau) * Util.Random.NextSingle() * 8;
                        ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), 0.5f + Util.Random.NextSingle() / 5, _entity.Position - _entity.Velocity + pos,
                            (_entity.Velocity + Velocity) / 2, 0, 0, Color.Yellow * 0.5f, new Color(1f, 0.5f, 0f, 0f)){ experienceGravity = true });
                    }
                }
                _entity.ApplyWork(q);
                if(isSun)
                {
                    _entity.ConductHeat(Temperature * strength, MathF.Tanh(strength));
                }
                else
                {
                    _entity.ConductHeat(Temperature, MathF.Tanh(strength));
                }
                    //Buoyancy
                acceleration += relativePosition / distance * strength * mass / distance / distance / 5;
                if (strength > 2)
                {
                    _entity.StatusHolder.ApplyStatus(new Pressure(Color.Red, isSun));
                    if(_entity.GetComponent<Health>() != null && _entity.Health <= 0)
                    {
                        _entity.isExpired = true;
                    }
                }
            }
            _entity.Velocity += acceleration * Engine.DeltaSeconds * 60;
            return acceleration;
        }
        else
        {
            Collide(_entity);
            return Vector2.Zero;
        }
    }
    public Vector2 AttractObject(Planet _celestialBody)
    {
        if (_celestialBody.isImmovable)
        {
            return Vector2.Zero;
        }
        Vector2 relativePosition = _celestialBody.Position - Position;
        if (relativePosition.Length() >= ColliderRadius + _celestialBody.ColliderRadius)
        {
            Vector2 acceleration = Vector2.Normalize(-relativePosition) * mass / relativePosition.LengthSquared();
            _celestialBody.Velocity += acceleration * Engine.DeltaSeconds * 60;
            return acceleration;
        }
        else
        {
            var normalVector = Vector2.Normalize(relativePosition);
            var frictionVector = new Vector2(normalVector.Y, -normalVector.X);
            var relativeVelocity = Velocity - _celestialBody.Velocity;

            _celestialBody.Velocity += normalVector * Math.Max(0, Vector2.Dot(relativeVelocity, normalVector)) + frictionVector * Vector2.Dot(relativeVelocity, frictionVector) * 0.1f;
            _celestialBody.Position += normalVector * (ColliderRadius + _celestialBody.ColliderRadius - Vector2.Distance(Position, _celestialBody.Position));
            return Vector2.Zero;
        }
    }
    public Vector2 AttractObject(Particle _particle)
    {
        Vector2 relativePosition = _particle.Position - Position;
        if (relativePosition == Vector2.Zero)
        {
            _particle.isExpired = true;
            return Vector2.Zero;
        }
        if (relativePosition.Length() >= ColliderRadius + (_particle.Size.X + _particle.Size.Y) / 4)
        {
            Vector2 acceleration = Vector2.Normalize(-relativePosition) * mass / relativePosition.LengthSquared() * Engine.DeltaSeconds * 60;
            _particle.Velocity += acceleration;
            return acceleration;
        }
        else
        {
            if (Util.Random.NextSingle() > Util.FIED(0.0002f))
            {
                _particle.Velocity = Velocity;
            }
            return Vector2.Zero;
        }
    }
    public override void Update()
    {
        foreach (var entity in Engine.SaveGame.CurrentMission.Entities)
        {
            if(entity == this)
            {
                continue;
            }
            AttractObject(entity);
        }
        foreach (var particle in ParticleManager.Particles)
        {
            if (particle.experienceGravity)
            {
                AttractObject(particle);
            }
        }
        if(Engine.SaveGame != null)
        {
            AttractObject(Engine.SaveGame.Player);
        }
        if (isImmovable)
        {
            Velocity = Vector2.Zero;
        }
        if (EasterEgg)
        {
            Color = new Color((MathF.Cos(Engine.Time) + 1) / 2, (MathF.Cos(Engine.Time + MathF.PI * 2 / 3) + 1) / 2, (MathF.Cos(Engine.Time - MathF.PI * 2 / 3) + 1) / 2);
            GetComponent<FollowEmitter>().ParticleEmitter.particleColor = Color;
        }
        base.Update();
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        float increment = MathF.Tau / ColliderRadius;
        int count = (int)Math.Truncate(ColliderRadius);
        if (count == 0)
        {
            return;
        }
        for (float angle = increment / 2; angle < MathF.Tau; angle += increment)
        {
            _spriteBatch.Draw(Assets.Get(Sprites.Dot), Position + Util.ToUnitVector(angle) * ColliderRadius, null, Color, angle, Assets.DimsOf(Sprites.Dot), 1, 0, 0);
        }
        if(atmosphereStrength > 0)
        {
            _spriteBatch.Draw(atmosphere, Position, null, Color.White, 0, new Vector2(atmosphere.Width / 2, atmosphere.Height / 2), 1, 0, 0);
        }
        if (hasRing)
        {
            int randomAngle = 3;
            float r = ColliderRadius + RingOffset;
            for (float i = 0; i < 2 * r; i++)
            {
                float j = i - r;
                float distance = r * 1.25f + j * j * j / (r * r * 2) + r / 2 + i / 2;
                float speed = MathF.Sqrt(mass / distance) * 60;
                //Simple deterministic random number generator
                randomAngle = (randomAngle * 65535 + 997) % 628;
                float particleAngle = (i + (float)(randomAngle) / 628 + Engine.Time * speed / distance) % MathF.Tau;
                Vector2 particlePosition = new Vector2(MathF.Cos(particleAngle), MathF.Sin(particleAngle) * 0.25f) * distance;
                if (particlePosition.LengthSquared() > ColliderRadius * ColliderRadius || particlePosition.Y > 0)
                {
                    _spriteBatch.Draw(Assets.Get(Sprites.Dot), Position + particlePosition, null, Color * 0.75f, particleAngle, Assets.DimsOf(Sprites.Dot), 1, 0, 0);
                }
            }
        }
        base.Draw(_spriteBatch);
    }
    public float GetAtmosphereDensity(Entity _entity)
    {
        float distance = Vector2.Distance(_entity.Position, Position);
        if(distance > AtmosphereRadius())
        {
            return 0;
        }
        return GetAtmosphereDensity(distance);
    }
    public float GetAtmosphereDensity(float r)
    {
        float gravityForce = mass / ColliderRadius / ColliderRadius;
        return atmosphereStrength * MathF.Pow(2, -gravityForce * (r - ColliderRadius) / atmosphereStrength / 4);
    }
    private float AtmosphereRadius()
    {
        float gravityForce = mass / ColliderRadius / ColliderRadius;
        return MathF.Log2(0.1f/atmosphereStrength) * 4 * atmosphereStrength / (-gravityForce) + ColliderRadius;
    }
    public Planet Copy()
    {
        return new Planet(Position, Velocity, mass, ColliderRadius / 50, isImmovable, Color, hasRing, atmosphereStrength, RingOffset)
        { isSun = this.isSun, Temperature = this.Temperature };
    }
}
