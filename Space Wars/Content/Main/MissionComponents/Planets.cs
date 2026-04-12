using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Space_Wars.Content.Main.Entities;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Particles;
using Microsoft.Xna.Framework;
using Space_Wars.Content.Main.Components;
namespace Space_Wars.Content.Main.MissionComponents;
internal class Planets(Planet[] _planets) : IMissionComponent, IObstacle
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
        foreach (var planet1 in _planets)
        {
            foreach (var planet2 in _planets)
            {
                if (planet1 == planet2)
                {
                    continue;
                }
                planet1.AttractObject(planet2);
            }
            foreach(var entity in Engine.EntityManager.Entities)
            {
                planet1.AttractObject(entity);
            }
            foreach(var particle in ParticleManager.Particles)
            {
                if(particle.experienceGravity)
                {
                    planet1.AttractObject(particle);
                }
            }
            planet1.AttractObject(Engine.SaveGame.Player);
        }
        foreach (var planet1 in _planets)
        {
            planet1.Update();
        }
        //Prevents players from losing important items
        Entity[] importantEntites = Engine.EntityManager.GetEntity<KeyTag>();
        var planet = _planets[0];
        foreach(var entity in importantEntites)
        {
            if (entity.Position.Length() >= 40 * 50 + planet.radius)
            {
                entity.Velocity *= 0.8f;
                entity.Velocity += Vector2.Normalize(-entity.Position) * Engine.DeltaSeconds * (entity.Position.Length() - (40 * 50 + planet.radius));
            }   
        }
        if (Engine.SaveGame.Player.Position.Length() >= 40 * 50 + planet.radius)
        {
            Engine.SaveGame.Player.Velocity *= 0.8f;
            Engine.SaveGame.Player.Velocity += Vector2.Normalize(-Engine.SaveGame.Player.Position) * Engine.DeltaSeconds * (Engine.SaveGame.Player.Position.Length() - (40 * 50 + planet.radius));
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
        if (Engine.SaveGame.Player.Position.Length() > _planets[0].radius * 2 + 15 * 50)
        {
            _spriteBatch.Draw(Assets.Get(Sprites.Arrow), Engine.SaveGame.Player.Position - Vector2.Normalize(Engine.SaveGame.Player.Position) * 25, null, Engine.SaveGame.Player.Color, -Util.ToAngle(Engine.SaveGame.Player.Position), Assets.DimsOf(Sprites.Arrow) / 2, 1, 0, 0.2f);
            _spriteBatch.DrawString(Assets.TextFont, "Return to planet.", Engine.Camera.Position - new Vector2(Assets.TextFont.MeasureString("Return to planet.").X/2, 225), Color.Crimson);
        }
    }
    public bool Collide(Entity _entity)
    {
        bool IsColliding = false;
        foreach(var planet in _planets)
        {
            IsColliding = IsColliding || planet.Collide(_entity);
        }
        return IsColliding;
    }
    public ICollider IsColliding(Vector2 _position, Vector2 _velocity, float _colliderRadius, bool _override, out float end)
    {
        end = _velocity.Length();
        ICollider returnPlanet = null;
        foreach(var planet in _planets)
        {
            if(planet.IsColliding(_position, _velocity, _colliderRadius, _override, out float _end))
            {
                if(_end < end)
                {
                    end = _end;
                    returnPlanet = planet;
                }
            }
        }
        return returnPlanet;
    }
    //TODO: Check if this clone function has side effects for replaying missions.
    //Should work for now
    public IMissionComponent Clone()
    {
        return new Planets(_planets);
    }
}
