using Microsoft.Xna.Framework;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main.MissionComponents;
public interface IObstacle
{
    public bool Collide(Entity _entity);
    public ICollider IsColliding(Vector2 _position, Vector2 _velocity, float _colliderRadius, bool _override, out float _end);
}
