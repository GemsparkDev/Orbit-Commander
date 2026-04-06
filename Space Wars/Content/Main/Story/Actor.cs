using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Space_Wars.Content.Main.Story;
public class Actor(Texture2D _texture, Vector2 _position, Color _color, float _angle) : IActor
{
    public Vector2 Position { get; set; } = _position;
    public float Angle { get; set; } = _angle;
    public float Scale { get; set; } = 1f;
    public Color Color { get; set; } = _color;

    public void Draw(SpriteBatch _spriteBatch)
    {
        _spriteBatch.Draw(_texture, Position, null, Color, Angle, new Vector2(_texture.Width, _texture.Height) / 2, Scale, 0, 0);
    }
}
public class TextActor(Vector2 _position, string _text) : IActor
{
    public Vector2 Position { get; set; } = _position;
    public int Index { get; set; } = 0;
    public string Text { get; } = _text;
    public Color TextColor { get; set; } = Color.White;
    public float TextSize { get; set; } = 1;
    public void Draw(SpriteBatch _spriteBatch)
    {
        _spriteBatch.DrawString(Assets.TextFont, Text[0..Index], Position, TextColor, 0, Vector2.Zero, TextSize, 0, 0);
    }
}
public interface IActor
{
    public void Draw(SpriteBatch _spriteBatch);
}
