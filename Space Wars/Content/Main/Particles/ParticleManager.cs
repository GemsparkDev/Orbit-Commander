using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Space_Wars.Content.Main.Particles
{
    public static class ParticleManager
    {
        private static bool isUpdating;
        public static List<Particle> Particles { get; private set; } = [];
        private static List<Particle> addedParticles = [];

        public static void Add(Particle particle)
        {
            if (!isUpdating)
            {
                //Checks the entity type, and adds it to the corresponding list for each type
                Particles.Add(particle);
            }
            else
            {
                addedParticles.Add(particle);
            }

            //Moves entities to the inactive list to prevent modifying a list while iterating
        }
        public static void Initialize()
        {
            Particles.Clear();
        }
        public static void Update()
        {
            isUpdating = true;

            foreach (var particle in Particles)
            {
                particle.Update();
            }
            if (Particles.Count >= 50000)
            {
                for (int i = 0; i < Particles.Count - 50000; i++)
                {
                    Particles[i].isExpired = true;
                }
            }
            Particles = [.. Particles.Where(x => !x.isExpired)];

            isUpdating = false;

            foreach (var particle in addedParticles)
            {
                Add(particle);
            }
            addedParticles.Clear();
        }

        public static void Draw(SpriteBatch _spriteBatch)
        {
            foreach (var particle in Particles)
            {
                particle.Draw(_spriteBatch);
            }
        }
    }
}
