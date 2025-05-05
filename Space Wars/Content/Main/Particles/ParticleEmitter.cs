using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Space_Wars.Content.Main.Particles
{
    public enum EmitterType
    {
        EmissionOverTime = 0,
        EmissionOverDistance = 1
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
        public float cachedDistance = 0;
        public int sprayCone;
        public float particleVelocity;
        public float particleAngularVelocity;
        public float speedOfEmission;
        public float particleTransparency;
        public float radius;
        public bool particleFadesOut;
        public bool isEmitterExpired = false;
        public bool isEmitterActive = true;
        public float probability = 1;
        float cooldown = 1;
        private Random random = new();
        private Action emitterFunction;
        public ParticleEmitter(Texture2D _particleTexture, float _particleTimeAlive, Vector2 _position, float _sprayAngle, int _sprayCone, float _particleVelocity, float _particleAngularVelocity, float _speedOfEmission, float _particleTransparency, bool _particleFadesOut, Color _particleColor, Color _particleFadeToColor, EmitterType _emitterType)
        {
            particleTexture = _particleTexture;
            particleTimeAlive = _particleTimeAlive;
            position = _position;
            sprayAngle = _sprayAngle;
            sprayCone = _sprayCone;
            particleVelocity = _particleVelocity;
            particleAngularVelocity = _particleAngularVelocity;
            speedOfEmission = _speedOfEmission;
            cooldown = 1/_speedOfEmission;
            particleTransparency = _particleTransparency;
            particleFadesOut = _particleFadesOut;
            particleColor = _particleColor;
            particleFadeToColor = _particleFadeToColor;
            if(_emitterType == EmitterType.EmissionOverTime)
            {
                emitterFunction = EmissionOverTime;
            }
            else if(_emitterType == EmitterType.EmissionOverDistance)
            {
                emitterFunction = EmissionOverDistance;
                prevPosition = _position;
            }
        }
        public ParticleEmitter(Texture2D _particleTexture, Vector2 _position, float _radius, float _particleTransparency, Color _particleColor)
        {
            particleTexture = _particleTexture;
            particleTimeAlive = Engine.DeltaSeconds;
            position = _position;
            radius = _radius;
            particleTransparency = _particleTransparency;
            particleColor = _particleColor;
            emitterFunction = DrawCircle;
            particleFadesOut = false;
        }
        public void Update()
        {
            if(isEmitterActive && random.NextSingle() < probability)
            {
                emitterFunction();
            }
            prevPosition = position;
        }
        public void EmissionOverTime()
        {
            if (cooldown <= 0)
            {
                float randomAngle;
                float particleAngle;
                float particleAngleRadians;
                Vector2 normalVector;
                int iterations = 1;
                if (cooldown / (1/ -speedOfEmission) > 1)
                {
                    iterations = (int)(cooldown / (1 / -speedOfEmission));
                }
                for (int i = 0; i < iterations; i++)
                {
                    randomAngle = random.Next(0, sprayCone);
                    particleAngle = randomAngle - sprayCone / 2 + sprayAngle;
                    particleAngleRadians = particleAngle * MathF.PI / 180;
                    normalVector = new(MathF.Sin(particleAngleRadians), -MathF.Cos(particleAngleRadians));
                    Particle particle = new(particleTexture, particleTimeAlive, position - normalVector * i / speedOfEmission * particleVelocity, normalVector * particleVelocity + offsetVelocity, particleAngleRadians, particleAngularVelocity, particleTransparency, particleFadesOut, particleColor, particleFadeToColor);
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
            float particleAngleRadians;
            Vector2 normalVector;
            Vector2 positionDifference = (position - prevPosition);
            float iterations = (Vector2.Distance(position, prevPosition) * speedOfEmission + cachedDistance);
            for (float i = 1; i < iterations; i++)
            {
                randomAngle = random.Next(0, sprayCone);
                particleAngle = randomAngle - sprayCone / 2 + sprayAngle;
                particleAngleRadians = particleAngle * MathF.PI / 180 - MathF.Atan2(positionDifference.X, positionDifference.Y);
                normalVector = new(MathF.Sin(particleAngleRadians), -MathF.Cos(particleAngleRadians));
                Particle particle = new(particleTexture, particleTimeAlive, prevPosition * (1 - i / iterations) + position * (i / iterations), normalVector * particleVelocity + offsetVelocity, particleAngleRadians, particleAngularVelocity, particleTransparency, particleFadesOut, particleColor, particleFadeToColor);
                ParticleManager.Add(particle);
            }
            cachedDistance = iterations - (MathF.Truncate(iterations));
        }
        private void DrawCircle()
        {
            Vector2 normalVector;
            float angle;
            for (int i = 0; i < radius; i++)
            {
                angle = (MathF.Tau / radius) * i;
                normalVector = new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * radius;
                ParticleManager.Add(new Particle(particleTexture, position + normalVector, angle, particleTransparency, particleColor));
            }
        }
    }
}
