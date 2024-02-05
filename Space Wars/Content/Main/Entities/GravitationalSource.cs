using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Particles;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace Space_Wars.Content.Main.Entities
{
    public class GravitationalSource
    {
        public Vector2 position;
        public Vector2 velocity;
        public float mass;
        public float radius;
        public List<GravitationalSource> moons = new();
        private ParticleEmitter surface;
        public bool isImmovable;
        public GravitationalSource(Vector2 _position, Vector2 _velocity, float _mass, float _radius, bool _isImmovable, Color _color)
        {
            position = _position;
            velocity = _velocity;
            mass = _mass;
            radius = _radius * 50;
            isImmovable = _isImmovable;
            surface = new ParticleEmitter(Assets.Get(Sprite.Dot), position, radius, 1, _color);
            ParticleManager.Add(surface);
        }
        public Vector2 GetAcceleration(Vector2 _position)
        {
            Vector2 relativePosition = _position - position;
            if (relativePosition == Vector2.Zero)
            {
                relativePosition = Vector2.One;
            }
            Vector2 acceleration = Vector2.Normalize(-relativePosition) * mass / relativePosition.LengthSquared();
            return acceleration;
        }
        public float GetOrbitalVelocity(Vector2 _position)
        {
            float distance = (_position - position).Length();
            return MathF.Sqrt(mass / distance);
        }

        public Vector2 AttractObject(Entity _entity)
        {
            Vector2 relativePosition = _entity.position - position;
            if(relativePosition == Vector2.Zero)
            {
                relativePosition = Vector2.One;
            }
            if (relativePosition.Length() - _entity.ColliderRadius >= radius)
            {
                Vector2 acceleration = Vector2.Normalize(-relativePosition) * mass / relativePosition.LengthSquared() * Engine.deltaSeconds * 60;
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
                if(Math.Floor((_entity.velocity - velocity).Length() / 2) > 5)
                {
                    collisionForce = (int)Math.Floor((_entity.velocity - velocity).Length() / 2);
                }
                _entity.Collide(collisionForce);
                _entity.velocity += normalVector * (1-((relativePosition.Length() - _entity.ColliderRadius)/radius)) * 10000 * Engine.deltaSeconds;
                _entity.velocity += (-_entity.velocity / 2 + velocity / 2) * Engine.deltaSeconds * 60;
                return Vector2.Zero;
            }

        }
        public void CalculateTrajectory(Entity _entity)
        {
            Vector2 futureVelocity = _entity.velocity;
            Vector2 futurePosition = _entity.position;
            Vector2 futureMoonVelocity = Vector2.Zero;
            Vector2 futureMoonPosition = Vector2.Zero;
            int iterations = 5000;
            bool drawPixel = true;
            for (int i = 0; i < iterations;)
            {
                Vector2 futureAcceleration = GetAcceleration(futurePosition);
                foreach (GravitationalSource moon in moons)
                {
                    if (futureMoonPosition == Vector2.Zero)
                    {
                        futureMoonPosition = moon.position;
                        futureMoonVelocity = moon.velocity;
                    }
                    Vector2 futureMoonAcceleration = GetAcceleration(futureMoonPosition);
                    futureMoonVelocity += futureMoonAcceleration;
                    futureMoonPosition += futureMoonVelocity;
                    Vector2 relativePosition = futurePosition - futureMoonPosition;
                    if ((futurePosition - futureMoonPosition).Length() <= moon.radius + _entity.ColliderRadius)
                    {
                        if (Engine.patchedConics == true)
                        {
                            ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), -(futureMoonPosition - futurePosition) + moon.position, 0, 1, Color.Crimson));
                        }
                        else
                        {
                            ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), futurePosition, 0, 1, Color.Crimson));
                        }
                        return;
                    }
                    futureAcceleration += Vector2.Normalize(-(relativePosition)) * moon.mass / relativePosition.LengthSquared();

                    if (i % 3 == 0 && (futurePosition - futureMoonPosition).Length() < moon.radius * 3 && Engine.patchedConics == true)
                    {
                        ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), -(futureMoonPosition - futurePosition) + moon.position, 0, 1, Color.DarkCyan));
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
            if(_celestialBody.isImmovable == false)
            {
                Vector2 relativePosition = _celestialBody.position - position;
                if (relativePosition.Length() - _celestialBody.radius >= radius)
                {
                    Vector2 acceleration = Vector2.Normalize(-relativePosition) * mass / relativePosition.LengthSquared();
                    _celestialBody.velocity += acceleration * Engine.deltaSeconds * 60;
                    return acceleration;
                }
                else
                {
                    _celestialBody.position = Vector2.Normalize(relativePosition) * radius + Vector2.Normalize(relativePosition) * _celestialBody.radius + position;
                    Vector2 tempVelocity = _celestialBody.velocity;
                    _celestialBody.velocity += (velocity-_celestialBody.velocity) / _celestialBody.mass * mass / 1.25f;
                    velocity += (tempVelocity-velocity) / mass * _celestialBody.mass / 1.25f;
                }
            }
            return Vector2.Zero;
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
                position += velocity * Engine.deltaSeconds * 60;
            }
            foreach(GravitationalSource moon in moons)
            {
                moon.Update();
                AttractObject(moon);
                moon.AttractObject(this);
                foreach(GravitationalSource secondMoon in moons)
                {
                    if(moon != secondMoon)
                    {
                        moon.AttractObject(secondMoon);
                    }
                }
            }
            surface.position = position;
        }
        public void Draw(SpriteBatch _spriteBatch)
        {
            foreach(GravitationalSource moon in moons)
            {
                moon.Draw(_spriteBatch);
            }
            if(Engine.debugMode == true)
            {
                //Draws a line in the direction of motion for X
                _spriteBatch.Draw(Engine.line, position - Engine.mousePositionOffset, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.White,
                    MathF.Atan2(0, velocity.X), Vector2.Zero, new Vector2(MathF.Abs(velocity.X), 1), SpriteEffects.None, 0.4f);
                //Draws a line in the direction of motion for Y
                _spriteBatch.Draw(Engine.line, position - Engine.mousePositionOffset, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.White,
                    MathF.Atan2(velocity.Y, 0), Vector2.Zero, new Vector2(MathF.Abs(velocity.Y), 1), SpriteEffects.None, 0.4f);
            }
        }
    }
}
