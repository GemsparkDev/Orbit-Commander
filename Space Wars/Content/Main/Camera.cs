using Microsoft.Xna.Framework;

namespace Space_Wars.Content.Main;

public class Camera(Vector2 _position, Vector2 _origin, float _zoom, float _rotation)
{
    private Matrix? transform;
    public Matrix Transform
    {
        get
        {
            return transform ??=
                Matrix.CreateTranslation(-_position.X, -_position.Y, 0) *
                Matrix.CreateRotationZ(_rotation) * Matrix.CreateScale(_zoom) *
                Matrix.CreateTranslation(_origin.X, _origin.Y, 0);
        }
    }
    public Vector2 Position
    {
        get { return _position; }
        set { _position = value; transform = null; }
    }
    public float Zoom
    {
        get { return _zoom; }
        set { _zoom = value; transform = null; }
    }
    public Vector2 Origin
    {
        get { return _origin; }
        set { _origin = value; transform = null; }
    }
    public float Rotation
    {
        get { return _rotation; }
        set { _rotation = value; transform = null; }
    }
}
