using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.UI_Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Space_Wars.Content.Main.Particles
{
    public static class ParticleManager
    {
        private static bool isUpdating;
        private static List<Particle> particles = new();
        private static List<Particle> addedParticles = new();
        private static List<ParticleEmitter> particleEmitters = new();
        private static List<ParticleEmitter> addedEmitters = new();

        public static void Add(Particle particle)
        {
            if (isUpdating == false)
            {
                //Checks the entity type, and adds it to the corresponding list for each type
                particles.Add(particle);
            }
            else
            {
                addedParticles.Add(particle);
            }

            //Moves entities to the inactive list to prevent modifying a list while iterating
        }
        public static void Add(ParticleEmitter particleEmitter)
        {
            if (isUpdating == false)
            {
                particleEmitters.Add(particleEmitter);
            }
            else
            {
                addedEmitters.Add(particleEmitter);
            }
        }
        public static void Initialize()
        {
            particles = new();
            particleEmitters = new();
        }
        public static void Update()
        {
            foreach (var particle in addedParticles)
            {
                Add(particle);
            }
            foreach (ParticleEmitter particleEmitter in addedEmitters)
            {
                Add(particleEmitter);
            }
            addedParticles.Clear();
            addedEmitters.Clear();

            isUpdating = true;

            foreach (var particle in particles)
            {
                particle.Update();
            }
            foreach (ParticleEmitter particleEmitter in particleEmitters)
            {
                particleEmitter.Update();
            }

            if (particles.Count >= 50000)
            {
                particles[0].isExpired = true;
            }

            //Clears all expired entities from the entity lists
            particles = particles.Where(x => !x.isExpired).ToList();
            particleEmitters = particleEmitters.Where(x => !x.isEmitterExpired).ToList();

            isUpdating = false;
        }

        public static void Draw(SpriteBatch _spriteBatch)
        {
            foreach (var particle in particles)
            {
                particle.Draw(_spriteBatch);
            }
        }
    }
}
