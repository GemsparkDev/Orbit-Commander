using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;

namespace Space_Wars.Content.Main;
public class LineCollider(Vector2 _start, Vector2 _end)
{
    public bool Collide(Entity _entity)
    {
        Vector2 length = _start - _end;
        float distanceBetween = Math.Clamp(Vector2.Dot((_start - _entity.position), length) / length.LengthSquared(), 0, 1);
        Vector2 closestPoint = _start-length * distanceBetween;
        float distance = Vector2.Distance(closestPoint, _entity.position);
        if (distance < _entity.ColliderRadius)
        {
            var frictionVector = Vector2.Normalize(-length);
            var normalVector = (_entity.position - closestPoint) / distance;
            var relativeVelocity = - _entity.velocity; //Include velocity if colliders are moving in the future.
            int collisionForce = (int)Math.Floor((relativeVelocity).Length() / 2);
            if (_entity as Pickup == null && (collisionForce > 5 || _entity is Projectile))
            {
                _entity.Collide(collisionForce);
            }
            float verticalVelocity = Vector2.Dot(relativeVelocity, normalVector);
            _entity.velocity += normalVector * verticalVelocity + frictionVector * Vector2.Dot(relativeVelocity, frictionVector) * 0.1f;
            _entity.position += Vector2.Normalize(_entity.position - closestPoint) * (_entity.ColliderRadius - distance);
        }
        return distance < _entity.ColliderRadius;
    }
    public bool IsColliding(Vector2 _position, float _radius)
    {
        Vector2 length = _start - _end;
        float distanceBetween = Math.Clamp(Vector2.Dot((_start - _position), length) / length.LengthSquared(), 0, 1);
        Vector2 closestPoint = _start - length * distanceBetween;
        float distance = Vector2.Distance(closestPoint, _position);
        return distance < _radius;
    }
    public void Move(Vector2 _offset)
    {
        _start += _offset;
        _end += _offset;
    }
    public void Rotate(float _rad)
    {
        float angle = MathF.Atan2((_end.Y - _start.Y),(_end.X - _start.X)) + MathF.PI/2;
        Vector2 com = (_end + _start) / 2;
        float length = (_end - com).Length();
        Vector2 dir = Util.ToUnitVector(angle + _rad);
        _end = com + dir * length;
        _start = com - dir * length;
    }
    public void Draw(SpriteBatch _spriteBatch)
    {
        float angle = MathF.Atan2((_start.Y - _end.Y), (_start.X - _end.X)) - MathF.PI/2;
        Vector2 dir = Util.ToUnitVector(angle);
        for (float d = 0; d < (_end - _start).Length() / 2; d += 2)
        {
            _spriteBatch.Draw(Assets.Get(Sprite.Dot), _start + dir * d * 2, null, Color.White, angle, Assets.DimsOf(Sprite.Dot), Vector2.One, 0, 0);
        }
    }
}
