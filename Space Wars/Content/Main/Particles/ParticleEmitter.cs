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
        public Vector2 prevPosition;
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
        public bool particlesExperienceGravity = false;
        public float probability = 1;
        public EmitterType EmitterType { get; }
        float cooldown = 0;
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
            particleTimeAlive = float.Epsilon;
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
            if(isEmitterActive && Util.Random.NextSingle() < probability)
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
                    randomAngle = Util.Random.NextSingle() * sprayCone;
                    particleAngle = randomAngle - sprayCone / 2 + sprayAngle;
                    normalVector = Util.ToUnitVector(particleAngle);
                    Particle particle = new(particleTexture, particleTimeAlive, position - normalVector * i / speedOfEmission * particleVelocity, normalVector * particleVelocity + offsetVelocity, 
                        particleAngle, particleAngularVelocity, particleColor, particleFadeToColor) { experienceGravity = particlesExperienceGravity};
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
            Vector2 positionDifference = position - prevPosition;
            var normalVector = Vector2.Normalize(positionDifference);
            float increment = 100 / (speedOfEmission);
            for (cachedDistance += positionDifference.Length(); cachedDistance > increment; cachedDistance -= increment)
            {
                randomAngle = Util.Random.NextSingle() * sprayCone - sprayCone / 2 + sprayAngle - Util.ToAngle(positionDifference);
                ParticleManager.Add(new Particle(particleTexture, particleTimeAlive, position - normalVector * (cachedDistance - increment), Util.ToUnitVector(randomAngle) * particleVelocity + offsetVelocity,
                    randomAngle, particleAngularVelocity, particleColor, particleFadeToColor) { experienceGravity = particlesExperienceGravity });
            }
        }
        private void DrawCircle()
        {
            if(particleVelocity == 0)
            {
                return;
            }
            Vector2 normalVector;
            float increment = MathF.Tau / particleVelocity / speedOfEmission;
            int count = (int)Math.Ceiling(Math.Truncate(sprayCone / increment));
            if(count % 2 == 0 && count != 0)
            {
                for (float angle = increment/2; angle < sprayCone / 2; angle += increment)
                {
                    DrawParticle(angle);
                    DrawParticle(-angle);
                }
            }
            else
            {
                DrawParticle(0);
                for (float angle = increment; angle < sprayCone / 2; angle += increment)
                {
                    DrawParticle(angle);
                    DrawParticle(-angle);
                }
            }
            void DrawParticle(float angle)
            {
                normalVector = Util.ToUnitVector(angle + sprayAngle) * particleVelocity;
                ParticleManager.Add(new Particle(particleTexture, particleTimeAlive, position + normalVector, offsetVelocity, angle, particleAngularVelocity, particleColor, particleFadeToColor) 
                { experienceGravity = particlesExperienceGravity });
            }
            sprayAngle += particleAngularVelocity * Engine.DeltaSeconds * 60;
        }
    }
}
