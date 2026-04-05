using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Particles;
using System;
using System.Linq;
using Space_Wars.Content.Main.Components;

namespace Space_Wars.Content.Main.Entities;

public class Planet
{
    public Vector2 position;
    public Vector2 velocity;
    public float mass;
    public float radius;
    private ParticleEmitter trajectory;
    private Texture2D atmosphere;
    public bool isImmovable;
    private bool hasRing;
    private bool isSun = false;
    public bool EasterEgg { get; set; } = false;
    private Color color;
    private float atmosphereStrength = 0;
    public float Temperature { get; set; } = 0;
    public float RingOffset { get; set; } = 0;

    //Remember to update the clone function too!
    public Planet(Vector2 _position, Vector2 _velocity, float _mass, float _radius, bool _isImmovable, Color _color, bool _hasRing = false, float _atmosphereStrength = 0, float _ringOffset = 0, bool _isSun = false)
    {
        position = _position;
        velocity = _velocity;
        mass = _mass;
        radius = _radius * 50;
        isImmovable = _isImmovable;
        color = _color;
        trajectory = new ParticleEmitter(Assets.Get(Sprites.Dot), 10, position, 0, 0, 0, 10f, _color * 0.5f, EmitterType.EmissionOverDistance) { particleFadeToColor = Color.Transparent };
        hasRing = _hasRing;
        atmosphereStrength = _atmosphereStrength;
        isSun = _isSun;
        if(_atmosphereStrength > 0)
        {
            atmosphere = Engine.Self.RenderAtmosphere(AtmosphereRadius(), _atmosphereStrength, radius, color, this, _isSun);
        }
        RingOffset = _ringOffset;
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
        Vector2 relativePosition = _entity.Position - position;
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
                Vector2 relativeVelocity = (velocity - _entity.Velocity);
                Vector2 drag = relativeVelocity * strength / 40;
                acceleration += drag;
                float q = (drag * relativeVelocity * relativeVelocity).Length() / 15;
                for(float i = 0; i < 5 * 60 * Engine.DeltaSeconds; i++)
                {
                    if (Util.Random.NextSingle() < q * q / 2)
                    {
                        Vector2 pos = Util.ToUnitVector(Util.Random.NextSingle() * MathF.Tau) * Util.Random.NextSingle() * 8;
                        ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), 0.5f + Util.Random.NextSingle() / 5, _entity.Position - _entity.Velocity + pos,
                            (_entity.Velocity + velocity) / 2, 0, 0, Color.Yellow * 0.5f, new Color(1f, 0.5f, 0f, 0f)){ experienceGravity = true });
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
                    if((_entity as Enemy) != null && (_entity as Enemy).Health <= 0)
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
            var normalVector = Vector2.Normalize(relativePosition);
            var frictionVector = new Vector2(normalVector.Y, -normalVector.X);
            var relativeVelocity = velocity - _entity.Velocity;
            int collisionForce = (int)Math.Floor((relativeVelocity).Length() / 2);
            if (_entity as Pickup == null && (collisionForce > 5 || _entity.GetComponent<Damager>() != null))
            {
                _entity.Collide(collisionForce);
            }
            float verticalVelocity = Math.Max(0, Vector2.Dot(relativeVelocity, normalVector));
            _entity.Velocity += normalVector * verticalVelocity + frictionVector * Vector2.Dot(relativeVelocity, frictionVector) * 0.1f;
            _entity.Position += normalVector * (radius + _entity.ColliderRadius - Vector2.Distance(position, _entity.Position));
            float val = (int)MathF.Sqrt(collisionForce);
            if (verticalVelocity > 1)
            {
                for (int i = 0; i < val * 1.5f; i++)
                {
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 10, normalVector * (radius + 2) + position, normalVector * val + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) * val / 2, 0, 0, color * 0.75f, Color.Transparent) { experienceGravity = true });
                }
            }
            _entity.ConductHeat(Temperature, 5);
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
            if (Util.Random.NextSingle() > Util.FIED(0.0002f))
            {
                _particle.Velocity = velocity;
            }
            return Vector2.Zero;
        }
    }
    public void Update()
    {
        if (!isImmovable)
        {
            position += velocity * Engine.DeltaSeconds * 60;
        }
        if (EasterEgg)
        {
            color = new Color((MathF.Cos(Engine.Time) + 1) / 2, (MathF.Cos(Engine.Time + MathF.PI * 2 / 3) + 1) / 2, (MathF.Cos(Engine.Time - MathF.PI * 2 / 3) + 1) / 2);
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
        if (SaveGame.DebugMode)
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
            _spriteBatch.Draw(Assets.Get(Sprites.Dot), position + Util.ToUnitVector(angle) * radius, null, color, angle, Assets.DimsOf(Sprites.Dot), 1, 0, 0);
        }
        if(atmosphereStrength > 0)
        {
            _spriteBatch.Draw(atmosphere, position, null, Color.White, 0, new Vector2(atmosphere.Width / 2, atmosphere.Height / 2), 1, 0, 0);
        }
        if (hasRing)
        {
            int randomAngle = 3;
            float r = radius + RingOffset;
            for (float i = 0; i < 2 * r; i++)
            {
                float j = i - r;
                float distance = r * 1.25f + j * j * j / (r * r * 2) + r / 2 + i / 2;
                float speed = MathF.Sqrt(mass / distance) * 60;
                //Simple deterministic random number generator
                randomAngle = (randomAngle * 65535 + 997) % 628;
                float particleAngle = (i + (float)(randomAngle) / 628 + Engine.Time * speed / distance) % MathF.Tau;
                Vector2 particlePosition = new Vector2(MathF.Cos(particleAngle), MathF.Sin(particleAngle) * 0.25f) * distance;
                if (particlePosition.LengthSquared() > radius * radius || particlePosition.Y > 0)
                {
                    _spriteBatch.Draw(Assets.Get(Sprites.Dot), position + particlePosition, null, color * 0.75f, particleAngle, Assets.DimsOf(Sprites.Dot), 1, 0, 0);
                }
            }
        }
    }
    public float GetAtmosphereDensity(Entity _entity)
    {
        float distance = Vector2.Distance(_entity.Position, position);
        if(distance > AtmosphereRadius())
        {
            return 0;
        }
        return GetAtmosphereDensity(distance);
    }
    public float GetAtmosphereDensity(float r)
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
        return new Planet(position, velocity, mass, radius / 50, isImmovable, color, hasRing, atmosphereStrength, RingOffset)
        { isSun = this.isSun, Temperature = this.Temperature };
    }
}
