using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Space_Wars.Content.Main.Entities;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Particles;

namespace Space_Wars.Content.Main.MissionComponents;
internal class Planets(Planet[] _planets) : IMissionComponent
{
    public Planet[] GetPlanets { get { return _planets; } }
    public void Initialize()
    {
        if (Engine.SaveGame.CurrentMissionCompleted && Util.Random.Next(0, 10000) == 0)
        {
            foreach (var planet in _planets)
            {
                planet.EasterEgg = true;
            }
        }
    }
    public void Update()
    {
        foreach (var planet in _planets)
        {
            foreach (var planet2 in _planets)
            {
                if (planet == planet2)
                {
                    continue;
                }
                planet.AttractObject(planet2);
            }
            foreach(var entity in Engine.EntityManager.Entities)
            {
                planet.AttractObject(entity);
            }
            foreach(var particle in ParticleManager.Particles)
            {
                if(particle.experienceGravity)
                {
                    planet.AttractObject(particle);
                }
            }
        }
        foreach (var planet in _planets)
        {
            planet.Update();
        }
    }
    public float GetAtmospherePressure(Entity _entity)
    {
        float sum = 0;
        foreach (var planet in _planets)
        {
            sum += planet.GetAtmosphereDensity(_entity);
        }
        return sum;
    }
    public void Draw(SpriteBatch _spriteBatch)
    {
        foreach (var planet in _planets)
        {
            planet.Draw(_spriteBatch);
        }
    }
}
