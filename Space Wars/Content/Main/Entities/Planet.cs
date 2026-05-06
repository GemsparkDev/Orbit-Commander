using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Components;
using Space_Wars.Content.Main.Particles;
using System;

namespace Space_Wars.Content.Main.Entities;

public class Planet : Entity, ICollider
{
    public float mass;
    public override float ColliderRadius { get; }
    public bool isImmovable = false;
    public bool EasterEgg { get; set; } = false;
    public override Color Color { get; set; }

    public Planet(Vector2 _position, Vector2 _velocity, float _mass, float _radius, bool _isImmovable, Color _color)
        : base(_position, _velocity, 0, 0)
    {
        mass = _mass;
        ColliderRadius = _radius * 50;
        isImmovable = _isImmovable;
        Color = _color;
        AddComponent(new Temp(this));
        AddComponent(new StationaryEmitter(this) { ParticleEmitter = new ParticleEmitter(Assets.Get(Sprites.Dot), 10, Position, 0, 0, 0, 10f, _color * 0.5f, EmitterType.EmissionOverDistance) { particleFadeToColor = Color.Transparent } });
        AddComponent(new FollowEmitter(this) { ParticleEmitter = new ParticleEmitter(Assets.Get(Sprites.Dot), 0, Position, 0, MathF.Tau, ColliderRadius, 0, Color, EmitterType.Circle) });
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
        if (relativePosition.Length() <= ColliderRadius + _entity.ColliderRadius)
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
    public void AttractObject(Entity _entity)
    {
        Vector2 relativePosition = _entity.Position - Position;
        if (relativePosition == Vector2.Zero)
        {
            _entity.isExpired = true;
        }
        float distance = relativePosition.Length();
        if (distance >= ColliderRadius + _entity.ColliderRadius)
        {
            Vector2 acceleration = Vector2.Normalize(-relativePosition) * mass / relativePosition.LengthSquared() * Engine.DeltaSeconds * 60;
            _entity.Velocity += acceleration;
        }
        else
        {
            Collide(_entity);
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
            if (entity == this)
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
        if (Engine.SaveGame != null)
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
}
