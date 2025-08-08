using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Space_Wars.Content.Main.Particles
{
    public enum EmitterType
    {
        EmissionOverTime = 0,
        EmissionOverDistance = 1,
        Circle = 2,
    }
    public class ParticleEmitter
    {
        public Vector2 position;
        private Vector2 prevPosition;
        public Vector2 offsetVelocity = Vector2.Zero;
        public Color particleColor;
        public Color particleFadeToColor;
        public Texture2D particleTexture;
        public float particleTimeAlive;
        public float sprayAngle;
        private float cachedDistance = 0;
        public float sprayCone;
        public float particleVelocity;
        public float particleAngularVelocity = 0;
        public float speedOfEmission;
        public bool isEmitterActive = true;
        public float probability = 1;
        public EmitterType EmitterType { get; }
        float cooldown = 1;
        private Action emitterFunction;
        public ParticleEmitter(Texture2D _particleTexture, float _particleTimeAlive, Vector2 _position, float _sprayAngle, float _sprayCone, float _particleVelocity, float _speedOfEmission, Color _particleColor, EmitterType _emitterType)
        {
            particleTexture = _particleTexture;
            particleTimeAlive = _particleTimeAlive;
            position = _position;
            sprayAngle = _sprayAngle;
            sprayCone = _sprayCone;
            particleVelocity = _particleVelocity;
            speedOfEmission = _speedOfEmission;
            cooldown = 1/_speedOfEmission;
            particleColor = _particleColor;
            particleFadeToColor = particleColor;
            EmitterType = _emitterType;
            if(_emitterType == EmitterType.EmissionOverTime)
            {
                emitterFunction = EmissionOverTime;
            }
            else if(_emitterType == EmitterType.EmissionOverDistance)
            {
                emitterFunction = EmissionOverDistance;
                prevPosition = _position;
            }
            else if(_emitterType == EmitterType.Circle)
            {
                emitterFunction = DrawCircle;
            }
        }
        public ParticleEmitter(Texture2D _particleTexture, Vector2 _position, float _radius, Color _particleColor)
        {
            particleTexture = _particleTexture;
            particleTimeAlive = 1;
            sprayCone = MathF.Tau;
            speedOfEmission = 1;
            position = _position;
            particleVelocity = _radius;
            particleColor = _particleColor;
            emitterFunction = DrawCircle;
            EmitterType = EmitterType.Circle;
        }
        public void Update()
        {
            if(isEmitterActive && Engine.Random.NextSingle() < probability)
            {
                emitterFunction();
            }
            prevPosition = position;
        }
        public void EmissionOverTime()
        {
            if (speedOfEmission <= 0)
            {
                return;
            }
            if (cooldown <= 0)
            {
                float randomAngle;
                float particleAngle;
                Vector2 normalVector;
                int iterations = 1;
                if (cooldown * -speedOfEmission > 1)
                {
                    iterations = (int)(cooldown / (1 / -speedOfEmission));
                }
                for (int i = 0; i < iterations; i++)
                {
                    randomAngle = Engine.Random.NextSingle() * sprayCone;
                    particleAngle = randomAngle - sprayCone / 2 + sprayAngle;
                    normalVector = Engine.ToUnitVector(particleAngle);
                    Particle particle = new(particleTexture, particleTimeAlive, position - normalVector * i / speedOfEmission * particleVelocity, normalVector * particleVelocity + offsetVelocity, particleAngle, particleAngularVelocity, particleColor, particleFadeToColor);
                    ParticleManager.Add(particle);
                }
                cooldown = 1/speedOfEmission;
            }
            else
            {
                cooldown -= Engine.DeltaSeconds;
            }
        }
        private void EmissionOverDistance()
        {
            if (prevPosition == position)
            {
                return;
            }
            float randomAngle;
            float particleAngle;
            Vector2 normalVector;
            Vector2 positionDifference = position - prevPosition;
            float differenceMagnitude = positionDifference.Length();
            float iterations = (differenceMagnitude * speedOfEmission);
            for (float i = 0; i < iterations; i++)
            {
                randomAngle = Engine.Random.NextSingle() * sprayCone;
                particleAngle = randomAngle - sprayCone / 2 + sprayAngle - MathF.Atan2(positionDifference.X, positionDifference.Y);
                normalVector = Engine.ToUnitVector(particleAngle);
                float lerp = i / iterations;
                ParticleManager.Add(new Particle(particleTexture, particleTimeAlive, prevPosition * (1 - lerp) + position * lerp + normalVector * Math.Min(10, cachedDistance), 
                normalVector * particleVelocity + offsetVelocity, particleAngle, particleAngularVelocity, particleColor, particleFadeToColor));
            }
            float val = 1 - (iterations - MathF.Truncate(iterations)) / iterations;
            cachedDistance = (prevPosition * (1 - val) + position * val).Length();
        }
        private void DrawCircle()
        {
            Vector2 normalVector;
            for (float angle = 0; angle < sprayCone; angle += MathF.Tau / particleVelocity / particleTimeAlive / speedOfEmission)
            {
                normalVector = Engine.ToUnitVector(angle + sprayAngle - sprayCone/2) * particleVelocity * particleTimeAlive;
                ParticleManager.Add(new Particle(particleTexture, position + normalVector + offsetVelocity, angle, particleColor));
            }
            sprayAngle += particleAngularVelocity * Engine.DeltaSeconds * 60;
        }
    }
}
