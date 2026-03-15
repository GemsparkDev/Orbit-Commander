using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Space_Wars.Content.Main;
public interface ICollider
{
    public abstract Vector2 Collide(Vector2 _point);
    public abstract void Render(SpriteBatch _spriteBatch);
    public abstract void Move(Vector2 _offset);
}
public class LineCollider(Vector2 _start, Vector2 _end) : ICollider
{
    public Vector2 Collide(Vector2 _point)
    {
        throw new NotImplementedException();
    }
    public void Move(Vector2 _offset)
    {
        _start += _offset;
        _end += _offset;
    }
    public void Rotate(float _rad)
    {
        throw new NotImplementedException();
    }
    public void Render(SpriteBatch _spriteBatch)
    {
        throw new NotImplementedException();
    }
}
public class CircleCollider(Vector2 _position, float _radius) : ICollider
{
    public Vector2 Collide(Vector2 _point)
    {
        throw new NotImplementedException();
    }
    public void Move(Vector2 _offset)
    {
        _position += _offset;
    }
    public void Render(SpriteBatch _spriteBatch)
    {
        throw new NotImplementedException();
    }
}
