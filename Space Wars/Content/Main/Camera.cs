using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Space_Wars.Content.Main;

public class Camera
{
    private Matrix _viewMatrix = Matrix.Identity;
    public Matrix ViewMatrix
    {
        get
        {
            if (updateMatricies)
            {
                UpdateMatricies();
            }
            return _viewMatrix;
        }
    }
    private Matrix _inverseMatrix = Matrix.Identity;
    public Matrix InverseMatrix 
    {
        get 
        { 
            if(updateMatricies)
            {
                UpdateMatricies();
            }
            return _inverseMatrix;
        } 
    }
    private Vector2 _position = Vector2.Zero;
    public Vector2 Position
    {
        get { return _position; }
        set
        {
            if (value == _position)
            {
                return;
            }
            _position = value;
            updateMatricies = true;
        }
    }
    private Vector2 _zoom = Vector2.One;
    public Vector2 Zoom
    {
        get { return _zoom; }
        set
        {
            if (value == _zoom)
            {
                return;
            }
            _zoom = value;
            updateMatricies = true;
        }
    }
    private Vector2 _origin = Vector2.Zero;
    public Vector2 Origin
    {
        get { return _origin; }
        set
        {
            if (value == _origin)
            {
                return;
            }
            _origin = value;
            updateMatricies = true;
        }
    }
    private float _rotation = 0;
    public float Rotation
    {
        get { return _rotation; }
        set
        {
            if (value == _rotation)
            {
                return;
            }
            _rotation = value;
            updateMatricies = true;
        }
    }
    public bool updateMatricies;
    public Viewport viewport = new();
    public void UpdateMatricies()
    {
        Matrix scaleMatrix = Matrix.CreateScale(new Vector3()
        {
            X = _zoom.X,
            Y = _zoom.Y,
            Z = 1
        });
        Matrix positionMatrix = Matrix.CreateTranslation(new Vector3()
        {
            X = -_position.X,
            Y = -_position.Y,
            Z = 0
        });
        Matrix originMatrix = Matrix.CreateTranslation(new Vector3()
        {
            X = _origin.X,
            Y = _origin.Y,
            Z = 0
        });
        Matrix rotationMatrix = Matrix.CreateRotationZ(_rotation);

        _viewMatrix = positionMatrix * rotationMatrix * scaleMatrix * originMatrix;
        _inverseMatrix = Matrix.Invert(_viewMatrix);

        updateMatricies = false;
    }
    public Vector2 ScreenToWorld(Vector2 _position)
    {
        return Vector2.Transform(_position, InverseMatrix);
    }
    public Vector2 WorldToScreen(Vector2 _position)
    {
        return Vector2.Transform(_position, ViewMatrix);
    }
    public Camera(Viewport _viewPort)
    {
        viewport = _viewPort;
    }
    public Camera(Vector2 _viewportSize)
    {
        viewport.Width = (int)_viewportSize.X;
        viewport.Height = (int)_viewportSize.Y;
    }
}
