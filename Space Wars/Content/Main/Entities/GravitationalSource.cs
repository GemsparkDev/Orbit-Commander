using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Particles;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Space_Wars.Content.Main.Entities;

public class GravitationalSource
{
    public Vector2 position;
    public Vector2 velocity;
    public float mass;
    public float radius;
    public List<GravitationalSource> moons = new();
    private ParticleEmitter surface;
    private ParticleEmitter trajectory;
    public bool isImmovable;
    private bool hasRing;
    private Color color;
    private float ringAngle = 0;
    public GravitationalSource(Vector2 _position, Vector2 _velocity, float _mass, float _radius, bool _isImmovable, Color _color, bool _hasRing = false)
    {
        position = _position;
        velocity = _velocity;
        mass = _mass;
        radius = _radius * 50;
        isImmovable = _isImmovable;
        color = _color;
        surface = new ParticleEmitter(Assets.Get(Sprite.Dot), position, radius, 1, _color);
        trajectory = new ParticleEmitter(Assets.Get(Sprite.Dot), 10, position, 0, 0, 0, 0, 1f, 1, true, _color * 0.1f, _color, EmitterType.EmissionOverDistance);
        hasRing = _hasRing;
    }
    public void RenderSurface()
    {
        ParticleManager.Add(surface);
        ParticleManager.Add(trajectory);
        foreach(GravitationalSource moon in moons)
        {
            moon.RenderSurface();
        }
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
    public static float GetOrbitalVelocity(Vector2 _position, Vector2 _planetPosition, float _planetMass)
    {
        float distance = (_position - _planetPosition).Length();
        return MathF.Sqrt(_planetMass / distance);
    }
    public Vector2 AttractObject(Entity _entity)
    {
        Vector2 relativePosition = _entity.position - position;
        if(relativePosition == Vector2.Zero)
        {
            relativePosition = Vector2.One;
        }
        if (relativePosition.Length() >= radius + _entity.ColliderRadius)
        {
            Vector2 acceleration = Vector2.Normalize(-relativePosition) * mass / relativePosition.LengthSquared() * Engine.DeltaSeconds * 60;
            foreach(GravitationalSource moon in moons)
            {
                moon.AttractObject(_entity);
            }
            _entity.velocity += acceleration;
            return acceleration;
        }
        else
        {
            int collisionForce = 0;
            Vector2 normalVector = Vector2.Normalize(relativePosition);
            Vector2 frictionVector = new (normalVector.Y, -normalVector.X);
            Vector2 relativeVelocity = velocity - _entity.velocity;
            if (Math.Floor((relativeVelocity).Length() / 2) > 5)
            {
                collisionForce = (int)Math.Floor((relativeVelocity).Length() / 2);
            }
            _entity.Collide(collisionForce);
            _entity.velocity += normalVector * Math.Max(0, Vector2.Dot(relativeVelocity, normalVector)) + frictionVector * Vector2.Dot(relativeVelocity, frictionVector) * 0.1f;
            _entity.position += normalVector * (radius + _entity.ColliderRadius - Vector2.Distance(position, _entity.position));
            return Vector2.Zero;
        }

    }
    public void CalculateTrajectory(Entity _entity)
    {
        Vector2 futureVelocity = _entity.velocity;
        Vector2 futurePosition = _entity.position;
        Vector2[] futureMoonVelocity = new Vector2[moons.Count];
        Vector2[] futureMoonPosition = new Vector2[moons.Count];
        for (int i = 0; i < moons.Count; i++)
        {
            futureMoonPosition[i] = Vector2.Zero;
            futureMoonVelocity[i] = Vector2.Zero;
        }

        int iterations = 5000;
        bool drawPixel = true;
        for (int i = 0; i < iterations;)
        {
            Vector2 futureAcceleration = GetAcceleration(futurePosition);
            for(int m = 0; m < moons.Count; m++)
            {
                GravitationalSource moon = moons[m];
                if (futureMoonPosition[m] == Vector2.Zero)
                {
                    futureMoonPosition[m] = moon.position;
                    futureMoonVelocity[m] = moon.velocity;
                }
                Vector2 relativePosition = futurePosition - futureMoonPosition[m];
                futureMoonVelocity[m] += GetAcceleration(futureMoonPosition[m]);
                futureMoonPosition[m] += futureMoonVelocity[m];
                if ((futurePosition - futureMoonPosition[m]).Length() <= moon.radius + _entity.ColliderRadius)
                {
                    if (Engine.patchedConics == true)
                    {
                        ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), -(futureMoonPosition[m] - futurePosition) + moon.position, 0, 1, Color.Crimson));
                    }
                    else
                    {
                        ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), futurePosition, 0, 1, Color.Crimson));
                    }
                    return;
                }
                futureAcceleration += Vector2.Normalize(-(relativePosition)) * moon.mass / relativePosition.LengthSquared();

                if (i % 3 == 0 && (futurePosition - futureMoonPosition[m]).Length() < moon.radius * 3 && Engine.patchedConics == true)
                {
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), -(futureMoonPosition[m] - futurePosition) + moon.position, 0, 1, Color.DarkCyan));
                    iterations -= 10;
                    drawPixel = false;
                }

            }
            futureVelocity += futureAcceleration;
            futurePosition += futureVelocity;
            if((futurePosition - position).Length() <= radius + _entity.ColliderRadius)
            {
                ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), futurePosition, 0, 1, Color.Crimson));
                return;
            }
            if (i % 3 == 0 && drawPixel == true)
            {
                ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), futurePosition, 0, 1, Color.DarkCyan));
            }
            drawPixel = true;
            i++;
            iterations -= (int)futureVelocity.Length();
        }
    }
    public Vector2 AttractObject(GravitationalSource _celestialBody)
    {
        if (_celestialBody.isImmovable)
        {
            return Vector2.Zero;
        }
        Vector2 relativePosition = _celestialBody.position - position;
        if (relativePosition.Length() - _celestialBody.radius >= radius)
        {
            Vector2 acceleration = Vector2.Normalize(-relativePosition) * mass / relativePosition.LengthSquared();
            _celestialBody.velocity += acceleration * Engine.DeltaSeconds * 60;
            return acceleration;
        }
        else
        {
            _celestialBody.position = Vector2.Normalize(relativePosition) * radius + Vector2.Normalize(relativePosition) * _celestialBody.radius + position;
            Vector2 tempVelocity = _celestialBody.velocity;
            _celestialBody.velocity += (velocity - _celestialBody.velocity) / _celestialBody.mass * mass / 1.25f;
            velocity += (tempVelocity - velocity) / mass * _celestialBody.mass / 1.25f;
            return Vector2.Zero;
        }
    }
    public void AddMoon(float _distance, float _mass, float _radius, bool _isImmovable)
    {
        Vector2 _position = new Vector2(_distance, 0) + position;
        Vector2 _velocity = new(0, GetOrbitalVelocity(_position));
        moons.Add(new(_position, _velocity, _mass, _radius, _isImmovable, Color.Cyan));
    }
    public void Update()
    {
        if(isImmovable == false)
        {
            position += velocity * Engine.DeltaSeconds * 60;
        }
        foreach(GravitationalSource moon in moons)
        {
            moon.Update();
            AttractObject(moon);
            moon.AttractObject(this);
        }
        if (hasRing)
        {
            ringAngle += Engine.DeltaSeconds;
            int randomAngle = 3;
            for (float i = 0; i < 2 * radius; i++)
            {
                float j = i - radius;
                float distance = radius * 1.25f + j*j*j/(radius*radius * 2) + radius / 2 + i/2;
                float speed = MathF.Sqrt(mass / distance) * 60;
                randomAngle = (randomAngle * 65535 + 997) % 628;
                //Golden Ratio
                float particleAngle = (i + (float)(randomAngle)/628 + ringAngle * speed / distance) % MathF.Tau;
                Vector2 particlePosition = new Vector2(MathF.Cos(particleAngle), MathF.Sin(particleAngle) * 0.25f) * distance;
                if (particlePosition.LengthSquared() > radius * radius || particlePosition.Y > 0)
                {
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), position + particlePosition, particleAngle, 0.75f, color));
                }
            }
        }
        surface.position = position;
        trajectory.position = position;
    }
    public bool IsColliding(Vector2 _position)
    {
        if (Vector2.DistanceSquared(_position, position) < radius * radius)
        {
            return true;
        }
        foreach (var moon in moons)
        {
            if (moon.IsColliding(_position))
            {
                return true;
            }
        }
        return false;
    }
    public void Draw(SpriteBatch _spriteBatch)
    {
        foreach(GravitationalSource moon in moons)
        {
            moon.Draw(_spriteBatch);
        }
        if(Engine.DebugMode == true)
        {
            //Draws a line in the direction of motion for X
            _spriteBatch.Draw(Engine.Line, position - Engine.mousePositionOffset, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.White,
                MathF.Atan2(0, velocity.X), Vector2.Zero, new Vector2(MathF.Abs(velocity.X), 1), SpriteEffects.None, 0.4f);
            //Draws a line in the direction of motion for Y
            _spriteBatch.Draw(Engine.Line, position - Engine.mousePositionOffset, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.White,
                MathF.Atan2(velocity.Y, 0), Vector2.Zero, new Vector2(MathF.Abs(velocity.Y), 1), SpriteEffects.None, 0.4f);
        }
    }
    public GravitationalSource Copy()
    {
        GravitationalSource planet = new(position, velocity, mass, radius/50, isImmovable, color, hasRing);
        foreach (var moon in moons)
        {
            planet.moons.Add(moon.Copy());
        }
        return planet;
    }
}
