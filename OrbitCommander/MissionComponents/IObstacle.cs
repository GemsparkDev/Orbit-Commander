using Microsoft.Xna.Framework;
using OrbitCommander.Entities;
using OrbitCommander.Core;

namespace OrbitCommander.MissionComponents;
public interface IObstacle
{
    public bool Collide(Entity _entity);
    public ICollider IsColliding(Vector2 _position, Vector2 _velocity, float _colliderRadius, bool _override, out float _end);
}
