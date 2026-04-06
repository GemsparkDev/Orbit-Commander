using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Space_Wars.Content.Main.MissionComponents;
public interface IObstacle
{
    public bool Collide(Entity _entity);
    public ICollider IsColliding(Vector2 _position, Vector2 _velocity, float _colliderRadius, bool _override);
}
