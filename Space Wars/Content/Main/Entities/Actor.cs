using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Space_Wars.Content.Main.Entities;
public class Actor(Texture2D _texture, Vector2 _position, Color _color, float _angle) : IActor
{
    public Vector2 Position { get; set; } = _position;
    public float Angle { get; set; } = _angle;

    public void Draw(SpriteBatch _spriteBatch)
    {
        _spriteBatch.Draw(_texture, Position, null, _color, Angle, new Vector2(_texture.Width, _texture.Height) / 2, 1, 0, 0);
    }
}
public class TextActor(Vector2 _position, string _text) : IActor
{
    public Vector2 Position { get; set; } = _position;
    public int Index { get; set; } = 0;
    public string Text { get; } = _text;
    public Color TextColor { get; set; } = Color.White;
    public void Draw(SpriteBatch _spriteBatch)
    {
        _spriteBatch.DrawString(Assets.TextFont, Text[0..Index], Position, TextColor);
    }
}
public interface IActor
{
    public void Draw(SpriteBatch _spriteBatch);
}
