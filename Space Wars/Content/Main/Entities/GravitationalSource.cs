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
        private float mass;
        public float radius;
        public List<GravitationalSource> moons = new();
        public bool isImmovable;
        public GravitationalSource(Vector2 _position, Vector2 _velocity, float _mass, float _radius, bool _isImmovable = false)
        {
            position = _position;
            velocity = _velocity;
            mass = _mass;
            radius = _radius * 50;
            isImmovable = _isImmovable;
        }
        public Vector2 GetAcceleration(Vector2 _position)
        {
            Vector2 relativePosition = _position - position;
            Vector2 acceleration = Vector2.Normalize(-relativePosition) * mass / relativePosition.LengthSquared();
            return acceleration;
        }

        public Vector2 AttractObject(Entity _entity)
        {
            Vector2 relativePosition = _entity.position - position;
            if (relativePosition.Length() - _entity.ColliderRadius >= radius)
            {
                Vector2 acceleration = Vector2.Normalize(-relativePosition) * mass / relativePosition.LengthSquared();
                foreach(GravitationalSource moon in moons)
                {
                    acceleration += moon.AttractObject(_entity);
                }
                _entity.velocity += acceleration * Engine.deltaSeconds * 60;
                return acceleration;
            }
            else
            {
                int collisionForce = 0;
                if(Math.Floor(_entity.velocity.Length() / 2) > 5)
                {
                    collisionForce = (int)Math.Floor(_entity.velocity.Length() / 2);
                }
                _entity.Collide(collisionForce);
                _entity.position = Vector2.Normalize(relativePosition) * radius + Vector2.Normalize(relativePosition) * _entity.ColliderRadius + position;
                _entity.velocity += (velocity - _entity.velocity) / 2;
                return Vector2.Zero;
            }

        }
        public void CalculateTrajectory(Entity _entity)
        {
            Vector2 initialAcceleration = Vector2.Normalize(-(_entity.position - position)) * mass / (_entity.position - position).LengthSquared();
            Vector2 futurePosition = _entity.position;
            Vector2 futureVelocity = _entity.velocity;
            Vector2 futureAcceleration = initialAcceleration;
            for (int i = 0; i < 2500; i++)
            {
                futurePosition += futureVelocity;
                futureVelocity += futureAcceleration;
                futureAcceleration = Vector2.Normalize(-futurePosition + position + velocity * i) * mass / (futurePosition - position).LengthSquared();
                if((futurePosition - position).Length() <= radius)
                {
                    return;
                }
                foreach (GravitationalSource moon in moons)
                {
                    if ((futurePosition - moon.position).Length() <= moon.radius)
                    {
                        return;
                    }
                    futureAcceleration += moon.GetAcceleration(futurePosition);
                }
                if (i % 3 == 0)
                {
                    ParticleManager.Add(new Particle(Assets.Sprites["Dot"], 0.01f, futurePosition, Vector2.Zero, 0, 0, 1, false, Color.DarkCyan, Color.DarkCyan));
                }
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
                    _celestialBody.velocity = velocity;
                }
            }
            return Vector2.Zero;
        }
        public void CalculateTrajectory(GravitationalSource _celestialBody)
        {
            Vector2 initialAcceleration = GetAcceleration(_celestialBody.position);
            Vector2 futurePosition = _celestialBody.position;
            Vector2 futureVelocity = _celestialBody.velocity;
            Vector2 futureAcceleration = initialAcceleration;
            for (int i = 0; i < 4000; i++)
            {
                futurePosition += futureVelocity;
                futureVelocity += futureAcceleration;
                futureAcceleration = GetAcceleration(futurePosition);
                if(i % 3 == 0)
                {
                    ParticleManager.Add(new Particle(Assets.Sprites["Dot"], 0.01f, futurePosition, Vector2.Zero, 0, 0, 1, false, Color.Cyan, Color.Cyan));
                }
            }
        }
        public void DrawRadius()
        {
            Vector2 normalVector;
            float angle;
            for (int i = 0; i < radius; i++)
            {
                angle = (MathF.Tau / radius) * i;
                normalVector = new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * radius;
                ParticleManager.Add(new Particle(Assets.Sprites["Dot"], 0.01f, position + normalVector, Vector2.Zero, angle, 0, 1, false, Color.Cyan, Color.Cyan));

            }
            foreach(GravitationalSource moon in moons)
            {
                moon.DrawRadius();
            }
        }
        public void Update()
        {
            if(isImmovable == false)
            {
                position += velocity * Engine.deltaSeconds * 60;
            }
            foreach(GravitationalSource moon in moons)
            {
                AttractObject(moon);
                moon.Update();
                moon.AttractObject(this);
            }
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
                _spriteBatch.Draw(Engine.line, position + Engine.screenPosition - Engine.mousePositionOffset, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.White,
                    MathF.Atan2(0, velocity.X), Vector2.Zero, new Vector2(MathF.Abs(velocity.X), 1), SpriteEffects.None, 0.4f);
                //Draws a line in the direction of motion for Y
                _spriteBatch.Draw(Engine.line, position + Engine.screenPosition - Engine.mousePositionOffset, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.White,
                    MathF.Atan2(velocity.Y, 0), Vector2.Zero, new Vector2(MathF.Abs(velocity.Y), 1), SpriteEffects.None, 0.4f);
            }
        }
    }
}
