using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Space_Wars.Content.Main.UI_Elements
{
    public class Decal : Widget
    {
        public Decal(Vector2 _offset, Texture2D _texture, string _text, Color _textColor)
        {
            offset = _offset;
            texture = _texture;
            text = _text;
            size = new Vector2(_texture.Width, _texture.Height);
            textColor = _textColor;
        }
        public Decal(Vector2 _offset, Texture2D _texture)
        {
            offset = _offset;
            texture = _texture;
            text = null;
            size = new Vector2(_texture.Width, _texture.Height);
            textColor = Color.White;
        }
        public Decal(Vector2 _offset, string _text, Color _textColor)
        {
            offset = _offset;
            texture = null;
            text = _text;
            size = new Vector2(_text.Length * 4, 12);
            textColor = _textColor;
        }
        public override void Initialize()
        {

        }
        public override void Draw(SpriteBatch spriteBatch, Vector2 parentPosition)
        {
            if (text != null)
            {
                Vector2 textMiddlePoint = Assets.textFont.MeasureString(text) / 2;
                Vector2 textPosition = parentPosition + Offset;
                spriteBatch.DrawString(Assets.textFont, text, textPosition, textColor, 0, textMiddlePoint, Engine.UIScale, SpriteEffects.None, 0.45f);
            }
        }
    }
}
