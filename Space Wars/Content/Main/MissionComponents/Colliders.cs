using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Wars.Content.Main.MissionComponents;
public class Colliders(Func<ICollider[]> _colliders) : IMissionComponent
{
    public ICollider[] GetColliders { get; private set; }
    public void Draw(SpriteBatch _spriteBatch)
    {
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
            foreach (var particle in ParticleManager.Particles)
            {
                //collider.Collide(particle);
            }
        }
    }
}
