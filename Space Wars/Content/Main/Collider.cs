using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main;
public class LineCollider(Vector2 _start, Vector2 _end)
{
    public Vector2 Collide(Entity _entity)
    {
        Vector2 length = _end - _start;
        Vector2 distance = _start - _entity.position;
        float circleDistance = (length.X * distance.Y - length.Y * distance.X) / length.Length();
        return Vector2.Normalize(distance) * circleDistance;
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
        for (float d = 0; d < (_end - _start).Length(); d += 2)
        {
            _spriteBatch.Draw(Assets.Get(Sprite.Dot), _start + dir * d, null, Color.White, angle, Assets.DimsOf(Sprite.Dot), Vector2.One, 0, 0);
        }
    }
}
