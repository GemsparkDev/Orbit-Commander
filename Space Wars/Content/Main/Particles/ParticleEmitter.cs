using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public bool particleFadesOut;
        public bool isEmitterExpired = false;
        public bool isEmitterActive = true;
        float cooldown = 1;
        Random random = new();
        private DelegateMethod emitterFunction;
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
            }
        }
        public void Update()
        {
            if(isEmitterActive == true)
            {
                emitterFunction();
            }
            prevPosition = position;
        }
        private void EmissionOverTime()
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
                cooldown -= Engine.deltaSeconds;
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
            Engine.WriteLine(positionDifference.X);
            int iterations = (int)(MathF.Sqrt(EntityManager.DistanceSqr(position, prevPosition)) * speedOfEmission + cachedDistance);
            for (int i = 0; i < iterations; i++)
            {
                randomAngle = random.Next(0, sprayCone);
                particleAngle = randomAngle - sprayCone / 2 + sprayAngle;
                particleAngleRadians = particleAngle * MathF.PI / 180 - MathF.Atan2(positionDifference.X, positionDifference.Y);
                normalVector = new(MathF.Sin(particleAngleRadians), -MathF.Cos(particleAngleRadians));
                Particle particle = new(particleTexture, particleTimeAlive, position, normalVector * particleVelocity + offsetVelocity, particleAngleRadians, particleAngularVelocity, particleTransparency, particleFadesOut, particleColor, particleFadeToColor);
                ParticleManager.Add(particle);
                cachedDistance = 0;
            }
            if (iterations < 1)
            {
                cachedDistance += (MathF.Sqrt(EntityManager.DistanceSqr(position, prevPosition)) * speedOfEmission);
            }
        }
    }
}
