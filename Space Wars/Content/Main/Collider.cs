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
    public Vector2 Collide(Entity _entity)
    {
        Vector2 length = _start - _end;
        Vector2 distance = _start - _entity.position;
        float distanceBetween = MathF.Abs(distance.Length() * Vector2.Dot(distance, length) / distance.Length() / length.Length());
        if(distanceBetween < 0)
        {
            if(Vector2.Distance(_entity.position, _start) < _entity.ColliderRadius)
            {
                _entity.velocity = Vector2.Zero;
                return Vector2.Normalize(-distance) * (_entity.ColliderRadius - distance.Length());
            }
            return Vector2.Zero;
        }
        if (distanceBetween > length.Length())
        {
            if (Vector2.Distance(_entity.position, _end) < _entity.ColliderRadius)
            {
                _entity.velocity = Vector2.Zero;
                return Vector2.Normalize(_entity.position - _end) * (_entity.ColliderRadius - (_entity.position - _end).Length());
            }
            return Vector2.Zero;
        }
        Vector2 closestPoint = _start-Vector2.Normalize(length) * distanceBetween;
        if(Vector2.Distance(closestPoint, _entity.position) > _entity.ColliderRadius)
        {
            return Vector2.Zero;
        }
        _entity.velocity = Vector2.Zero;
        return Vector2.Normalize(_entity.position - closestPoint) * (_entity.ColliderRadius - Vector2.Distance(closestPoint, _entity.position));
    }
    public void Move(Vector2 _offset)
    {
        _start += _offset;
        _end += _offset;
    }
    public void Rotate(float _rad)
    {
        float angle = MathF.Atan2((_end.Y - _start.Y),(_end.X - _start.X));
        Vector2 com = (_end + _start) / 2;
        float length = (_end - com).Length();
        Vector2 dir = Util.ToUnitVector(angle + _rad);
        _end = com + dir * length;
        _start = com - dir * length;
    }
    public void Draw(SpriteBatch _spriteBatch)
    {
        float angle = MathF.Atan2((_end.Y - _start.Y), (_end.X - _start.X));
        Vector2 dir = Util.ToUnitVector(angle);
        for (float d = 0; d < (_end - _start).Length() / 2; d += 2)
        {
            _spriteBatch.Draw(Assets.Get(Sprite.Dot), _start + dir * d * 2, null, Color.White, angle, Assets.DimsOf(Sprite.Dot), Vector2.One, 0, 0);
        }
    }
}
