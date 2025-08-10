using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Particles;
using System;

namespace Space_Wars.Content.Main.Entities;

public class GravitationalSource
{
    public Vector2 position;
    public Vector2 velocity;
    public float mass;
    public float radius;
    private ParticleEmitter surface;
    private ParticleEmitter trajectory;
    public bool isImmovable;
    private bool hasRing;
    public bool EasterEgg { get; set; } = false;
    private Color color;
    private float time = 0;
    public GravitationalSource(Vector2 _position, Vector2 _velocity, float _mass, float _radius, bool _isImmovable, Color _color, bool _hasRing = false)
    {
        position = _position;
        velocity = _velocity;
        mass = _mass;
        radius = _radius * 50;
        isImmovable = _isImmovable;
        color = _color;
        surface = new ParticleEmitter(Assets.Get(Sprite.Dot), position, radius, _color);
        trajectory = new ParticleEmitter(Assets.Get(Sprite.Dot), 10, position, 0, 0, 0, 10f, _color * 0.5f, EmitterType.EmissionOverDistance) { particleFadeToColor = Color.Transparent};
        hasRing = _hasRing;
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
        if(relativePosition == Vector2.Zero)
        {
            _entity.isExpired = true;
            return Vector2.Zero;
        }
        if (relativePosition.Length() >= radius + _entity.ColliderRadius)
        {
            Vector2 acceleration = Vector2.Normalize(-relativePosition) * mass / relativePosition.LengthSquared() * Engine.DeltaSeconds * 60;
            _entity.velocity += acceleration;
            return acceleration;
        }
        else
        {
            var normalVector = Vector2.Normalize(relativePosition);
            var frictionVector = new Vector2(normalVector.Y, -normalVector.X);
            var relativeVelocity = velocity - _entity.velocity;
            int collisionForce = (int)Math.Floor((relativeVelocity).Length() / 2);
            if (_entity as Pickup == null && collisionForce > 5)
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
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), 10, normalVector * (radius + 2), normalVector * val + new Vector2(Engine.OneToNegOne(), Engine.OneToNegOne()) * val / 2, 0, 0, color * 0.75f, Color.Transparent) { experienceGravity = true });
                }
            }
            return Vector2.Zero;
        }
    }
    public Vector2 AttractObject(GravitationalSource _celestialBody)
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
            surface.particleColor = color;
            trajectory.particleColor = color;
        }
        if (hasRing)
        {
            int randomAngle = 3;
            for (float i = 0; i < 2 * radius; i++)
            {
                float j = i - radius;
                float distance = radius * 1.25f + j*j*j/(radius*radius * 2) + radius / 2 + i/2;
                float speed = MathF.Sqrt(mass / distance) * 60;
                randomAngle = (randomAngle * 65535 + 997) % 628;
                //Golden Ratio
                float particleAngle = (i + (float)(randomAngle)/628 + time * speed / distance) % MathF.Tau;
                Vector2 particlePosition = new Vector2(MathF.Cos(particleAngle), MathF.Sin(particleAngle) * 0.25f) * distance;
                if (particlePosition.LengthSquared() > radius * radius || particlePosition.Y > 0)
                {
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), position + particlePosition, particleAngle, color * 0.75f));
                }
            }
        }
        surface.position = position;
        surface.Update();
        trajectory.position = position;
        trajectory.Update();
    }
    public bool IsColliding(Vector2 _position)
    {
        return Vector2.DistanceSquared(_position, position) < radius * radius;
    }
    public void Draw(SpriteBatch _spriteBatch)
    {
        if(Engine.DebugMode)
        {
            //Draws a line in the direction of motion for X
            _spriteBatch.Draw(Engine.Line, position, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.White,
                MathF.Atan2(0, velocity.X), Vector2.Zero, new Vector2(MathF.Abs(velocity.X), 1), SpriteEffects.None, 0.4f);
            //Draws a line in the direction of motion for Y
            _spriteBatch.Draw(Engine.Line, position, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.White,
                MathF.Atan2(velocity.Y, 0), Vector2.Zero, new Vector2(MathF.Abs(velocity.Y), 1), SpriteEffects.None, 0.4f);
        }
    }
    public GravitationalSource Copy()
    {
        GravitationalSource planet = new(position, velocity, mass, radius/50, isImmovable, color, hasRing);
        return planet;
    }
}
