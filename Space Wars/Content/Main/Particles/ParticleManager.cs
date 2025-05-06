using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Space_Wars.Content.Main.Entities;
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

        public static void Add(Particle particle)
        {
            if (!isUpdating)
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
        public static void Initialize()
        {
            particles.Clear();
        }
        public static void Update()
        {
            isUpdating = true;

            foreach (var particle in particles)
            {
                particle.Update();
            }

            if (particles.Count >= 50000)
            {
                for(int i = 0; i < particles.Count - 50000; i++)
                {
                    particles[i].isExpired = true;
                }
            }

            //Clears all expired entities from the entity lists
            particles = particles.Where(x => !x.isExpired).ToList();

            isUpdating = false;

            foreach (var particle in addedParticles)
            {
                Add(particle);
            }
            addedParticles.Clear();
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
