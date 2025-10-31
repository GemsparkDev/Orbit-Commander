using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Particles;
using System;
using System.Linq;

namespace Space_Wars.Content.Main.Entities;

public class Planet
{
    public Vector2 position;
    public Vector2 velocity;
    public float mass;
    public float radius;
    private ParticleEmitter trajectory;
    public bool isImmovable;
    private bool hasRing;
    public bool isSun = false;
    public bool EasterEgg { get; set; } = false;
    private Color color;
    private float time = 0;
    private float atmosphereStrength = 0;
    public Planet(Vector2 _position, Vector2 _velocity, float _mass, float _radius, bool _isImmovable, Color _color, bool _hasRing = false, float _atmosphereStrength = 0)
    {
        position = _position;
        velocity = _velocity;
        mass = _mass;
        radius = _radius * 50;
        isImmovable = _isImmovable;
        color = _color;
        trajectory = new ParticleEmitter(Assets.Get(Sprite.Dot), 10, position, 0, 0, 0, 10f, _color * 0.5f, EmitterType.EmissionOverDistance) { particleFadeToColor = Color.Transparent };
        hasRing = _hasRing;
        atmosphereStrength = _atmosphereStrength;
    }
    public Vector2 GetAcceleration(Vector2 _position)
    {
        Vector2 relativePosition = _position - position;
        if (relativePosition == Vector2.Zero)
        {
            relativePosition = Vector2.One;
        }
        return Vector2.Normalize(-relativePosition) * mass / relativePosition.LengthSquared();
    }
    public float GetOrbitalVelocity(Vector2 _position)
    {
        float distance = (_position - position).Length();
        return MathF.Sqrt(mass / distance);
    }
    public static Vector2 GetOrbitalVelocity(Vector2 _position, Vector2 _planetPosition, float _planetMass)
    {
        Vector2 relativePosition = (_position - _planetPosition);
        float distance = relativePosition.Length();
        float speed = MathF.Sqrt(_planetMass / distance);
        return new Vector2(-relativePosition.Y, relativePosition.X) / distance * speed;
    }
    public Vector2 AttractObject(Entity _entity)
    {
        Vector2 relativePosition = _entity.position - position;
        if (relativePosition == Vector2.Zero)
        {
            _entity.isExpired = true;
            return Vector2.Zero;
        }
        float distance = relativePosition.Length();
        if (distance >= radius + _entity.ColliderRadius)
        {
            Vector2 acceleration = Vector2.Normalize(-relativePosition) * mass / relativePosition.LengthSquared();
            if (distance < AtmosphereRadius())
            {
                float strength = GetAtmosphereDensity(distance);
                //Drag
                acceleration += (velocity - _entity.velocity) * strength / 40;
                //Buoyancy
                acceleration += relativePosition / distance * strength * mass / distance / distance / 5;
                if (strength > 2)
                {
                    _entity.StatusHolder.ApplyStatus(new Pressure(Color.Red, isSun));
                }
            }
            _entity.velocity += acceleration * Engine.DeltaSeconds * 60;
            return acceleration;
        }
        else
        {
            var normalVector = Vector2.Normalize(relativePosition);
            var frictionVector = new Vector2(normalVector.Y, -normalVector.X);
            var relativeVelocity = velocity - _entity.velocity;
            int collisionForce = (int)Math.Floor((relativeVelocity).Length() / 2);
            if (_entity as Pickup == null && (collisionForce > 5 || _entity is Projectile))
            {
                _entity.Collide(collisionForce);
            }
            float verticalVelocity = Math.Max(0, Vector2.Dot(relativeVelocity, normalVector));
            _entity.velocity += normalVector * verticalVelocity + frictionVector * Vector2.Dot(relativeVelocity, frictionVector) * 0.1f;
            _entity.position += normalVector * (radius + _entity.ColliderRadius - Vector2.Distance(position, _entity.position));
            float val = (int)MathF.Sqrt(collisionForce);
            if (verticalVelocity > 1)
            {
                for (int i = 0; i < val * 1.5f; i++)
                {
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), 10, normalVector * (radius + 2) + position, normalVector * val + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) * val / 2, 0, 0, color * 0.75f, Color.Transparent) { experienceGravity = true });
                }
            }
            return Vector2.Zero;
        }
    }
    public Vector2 AttractObject(Planet _celestialBody)
    {
        if (_celestialBody.isImmovable)
        {
            return Vector2.Zero;
        }
        Vector2 relativePosition = _celestialBody.position - position;
        if (relativePosition.Length() >= radius + _celestialBody.radius)
        {
            Vector2 acceleration = Vector2.Normalize(-relativePosition) * mass / relativePosition.LengthSquared();
            _celestialBody.velocity += acceleration * Engine.DeltaSeconds * 60;
            return acceleration;
        }
        else
        {
            var normalVector = Vector2.Normalize(relativePosition);
            var frictionVector = new Vector2(normalVector.Y, -normalVector.X);
            var relativeVelocity = velocity - _celestialBody.velocity;

            _celestialBody.velocity += normalVector * Math.Max(0, Vector2.Dot(relativeVelocity, normalVector)) + frictionVector * Vector2.Dot(relativeVelocity, frictionVector) * 0.1f;
            _celestialBody.position += normalVector * (radius + _celestialBody.radius - Vector2.Distance(position, _celestialBody.position));
            return Vector2.Zero;
        }
    }
    public Vector2 AttractObject(Particle _particle)
    {
        Vector2 relativePosition = _particle.Position - position;
        if (relativePosition == Vector2.Zero)
        {
            _particle.isExpired = true;
            return Vector2.Zero;
        }
        if (relativePosition.Length() >= radius + (_particle.Size.X + _particle.Size.Y) / 4)
        {
            Vector2 acceleration = Vector2.Normalize(-relativePosition) * mass / relativePosition.LengthSquared() * Engine.DeltaSeconds * 60;
            _particle.Velocity += acceleration;
            return acceleration;
        }
        else
        {
            _particle.isExpired = true;
            return Vector2.Zero;
        }
    }
    public void Update()
    {
        time += Engine.DeltaSeconds;
        if (!isImmovable)
        {
            position += velocity * Engine.DeltaSeconds * 60;
        }
        if (EasterEgg)
        {
            color = new Color((MathF.Cos(time) + 1) / 2, (MathF.Cos(time + MathF.PI * 2 / 3) + 1) / 2, (MathF.Cos(time - MathF.PI * 2 / 3) + 1) / 2);
            trajectory.particleColor = color;
        }
        trajectory.Update();
        trajectory.position = position;
    }
    public bool IsColliding(Vector2 _position)
    {
        return Vector2.DistanceSquared(_position, position) < radius * radius;
    }
    public void Draw(SpriteBatch _spriteBatch)
    {
        if (Engine.DebugMode)
        {
            //Draws a line in the direction of motion for X
            _spriteBatch.Draw(Engine.Line, position, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.White,
                MathF.Atan2(0, velocity.X), Vector2.Zero, new Vector2(MathF.Abs(velocity.X), 1), SpriteEffects.None, 0.4f);
            //Draws a line in the direction of motion for Y
            _spriteBatch.Draw(Engine.Line, position, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.White,
                MathF.Atan2(velocity.Y, 0), Vector2.Zero, new Vector2(MathF.Abs(velocity.Y), 1), SpriteEffects.None, 0.4f);
        }
        float increment = MathF.Tau / radius;
        int count = (int)Math.Truncate(radius);
        if (count == 0)
        {
            return;
        }
        for (float angle = increment / 2; angle < MathF.Tau; angle += increment)
        {
            _spriteBatch.Draw(Assets.Get(Sprite.Dot), position + Util.ToUnitVector(angle) * radius, null, color, angle, Assets.DimsOf(Sprite.Dot), 1, 0, 0);
        }
        if (atmosphereStrength > 0)
        {
            float atmR = AtmosphereRadius();
            float start = radius;
            if (isSun)
            {
                start = 0;
            }
            for (float r = start; r < atmR; r += MathF.Sqrt(36 + 36 / MathF.Pow(GetAtmosphereDensity(r), 2)))
            {
                float iterations = MathF.PI * MathF.PI * r / 9 + 4;
                float offset = 1;
                if (atmosphereStrength > 5)
                {
                    offset = MathF.Sin(r) / 4 + 1;
                }
                for (float t = MathF.Tau / MathF.Ceiling(iterations) / 2; t < MathF.Tau; t += MathF.Tau / MathF.Ceiling(iterations))
                {
                    _spriteBatch.Draw(Assets.Get(Sprite.Circle), position + Util.ToUnitVector(t) * r, null, color * MathF.Tanh(GetAtmosphereDensity(r) / 4f) * offset, t, Assets.DimsOf(Sprite.Circle), 1, 0, 0);
                }
            }
        }
        if (hasRing)
        {
            int randomAngle = 3;
            for (float i = 0; i < 2 * radius; i++)
            {
                float j = i - radius;
                float distance = radius * 1.25f + j * j * j / (radius * radius * 2) + radius / 2 + i / 2;
                float speed = MathF.Sqrt(mass / distance) * 60;
                //Simple deterministic random number generator
                randomAngle = (randomAngle * 65535 + 997) % 628;
                float particleAngle = (i + (float)(randomAngle) / 628 + time * speed / distance) % MathF.Tau;
                Vector2 particlePosition = new Vector2(MathF.Cos(particleAngle), MathF.Sin(particleAngle) * 0.25f) * distance;
                if (particlePosition.LengthSquared() > radius * radius || particlePosition.Y > 0)
                {
                    _spriteBatch.Draw(Assets.Get(Sprite.Dot), position + particlePosition, null, color * 0.75f, particleAngle, Assets.DimsOf(Sprite.Dot), 1, 0, 0);
                }
            }
        }
    }
    private float GetAtmosphereDensity(float r)
    {
        float gravityForce = mass / radius / radius;
        return atmosphereStrength * MathF.Pow(2, -gravityForce * (r - radius) / atmosphereStrength / 4);
    }
    private float AtmosphereRadius()
    {
        float gravityForce = mass / radius / radius;
        return MathF.Log2(0.1f/atmosphereStrength) * 4 * atmosphereStrength / (-gravityForce) + radius;
    }
    public Planet Copy()
    {
        Planet planet = new(position, velocity, mass, radius / 50, isImmovable, color, hasRing, atmosphereStrength) { isSun = this.isSun};
        return planet;
    }
}
