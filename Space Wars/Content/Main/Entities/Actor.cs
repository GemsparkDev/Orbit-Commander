using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Space_Wars.Content.Main.Entities;
public class Actor
{
    private Texture2D texture;
    public Vector2 Position { get; set; }
    private Color color;
    public float Angle { get; set; }
    public Actor(Texture2D _texture, Vector2 _position, Color _color, float _angle)
    {
        texture = _texture;
        Position = _position;
        color = _color;
        Angle = _angle;
    }

    public void Draw(SpriteBatch _spriteBatch)
    {
        _spriteBatch.Draw(texture, Position, null, color, Angle, new Vector2(texture.Width, texture.Height) / 2, 1, 0, 0);
    }
}
