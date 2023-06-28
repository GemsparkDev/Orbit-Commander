using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Wars.Content.Main.UI_Elements
{
    public class Decal : Widget
    {
        public Decal(Vector2 offset, Texture2D texture, string text, Color textColor)
        {
            Offset = offset;
            Texture = texture;
            Text = text;
            _Size = new Vector2(texture.Width, texture.Height);
            TextColor = textColor;
        }
        public Decal(Vector2 offset, Texture2D texture)
        {
            Offset = offset;
            Texture = texture;
            Text = null;
            _Size = new Vector2(texture.Width, texture.Height);
            TextColor = Color.White;
        }
        public Decal(Vector2 offset, string text, Color textColor)
        {
            Offset = offset;
            Texture = null;
            Text = text;
            _Size = new Vector2(text.Length*4, 12);
            TextColor = textColor;
        }
        public override void Initialize()
        {
            
        }
        public override void Draw(SpriteBatch spriteBatch, Vector2 parentPosition)
        {
            if (Text != null)
            {
                Vector2 textMiddlePoint = Assets.textFont.MeasureString(Text) / 2;
                Vector2 textPosition = parentPosition + Offset;
                spriteBatch.DrawString(Assets.textFont, Text, textPosition, TextColor, 0, textMiddlePoint, Engine.UIScale, SpriteEffects.None, 0.45f);
            }
        }
    }
}
