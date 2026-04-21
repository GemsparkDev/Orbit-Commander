using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Space_Wars.Content.Main.MissionComponents;
public class Colliders(Func<ICollider[]> _colliders) : IMissionComponent, IObstacle
{
    public ICollider[] GetColliders { get; set; }
    public void Draw(SpriteBatch _spriteBatch)
    {
        foreach(var collider in GetColliders)
        {
            collider.Draw(_spriteBatch);
        }
    }

    public void Initialize()
    {
        GetColliders = _colliders();
    }

    public void Update()
    {
        foreach(var collider in GetColliders)
        {
            foreach (var entity in Engine.EntityManager.Entities)
            {
                collider.Collide(entity);
            }
            //foreach (var particle in ParticleManager.Particles)
            //{
                //collider.Collide(particle);
            //}
        }
    }
    public bool Collide(Entity _entity)
    {
        bool IsColliding = false;
        foreach(var collider in GetColliders)
        {
            IsColliding = IsColliding || collider.Collide(_entity);
        }
        return IsColliding;
    }
    public ICollider IsColliding(Vector2 _position, Vector2 _velocity, float _colliderRadius, bool _override, out float end)
    {
        end = _velocity.Length();
        ICollider returnCollider = null;
        foreach(var collider in GetColliders)
        {
            if(collider.IsColliding(_position, _velocity, _colliderRadius, _override, out float _end))
            {
                if(_end < end)
                {
                    end = _end;
                    returnCollider = collider;
                }
            }
        }
        return returnCollider;
    }
    public IMissionComponent Clone()
    {
        return new Colliders(_colliders);
    }
}
