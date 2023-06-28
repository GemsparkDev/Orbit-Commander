using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Space_Wars.Content.Main.UI_Elements
{
    internal class Button : Widget, IFunctional
    {
        private List<DelegateMethod> behaviours = new();
        public Button(Vector2 offset, Texture2D texture)
        {
            _Size = new Vector2(texture.Width, texture.Height);
            Offset = offset - Size/2;
            Texture = texture;
            Text = null;
            TextColor = Color.White;
        }

        public Button(Vector2 offset, Texture2D texture, string text, Color textColor)
        {
            _Size = new Vector2(texture.Width, texture.Height);
            Offset = offset - Size/2;
            Texture = texture;
            Text = text;
            TextColor = textColor;
        }
        public Button(Vector2 offset, string text, Color textColor)
        {
            _Size = new Vector2(text.Length * 4, 12);
            Offset = offset - Size/2;
            Texture = null;
            Text = text;
            TextColor = textColor;
        }

        public void Interact()
        {
            for (int i = 0; i < behaviours.Count; i++)
            {
                ApplyBehaviours();
            }
        }
        public void AddBehaviour(DelegateMethod func)
        {
            behaviours.Add(func);
        }
        public void ApplyBehaviours()
        {
            for (int i = 0; i < behaviours.Count; i++)
            {
                DelegateMethod func = behaviours[i];
                func();
            }
        }
        public override void Initialize()
        {
            
        }
        public override void Draw(SpriteBatch spriteBatch, Vector2 parentPosition)
        {
            if (Text != null)
            {
                Vector2 textMiddlePoint = Assets.textFont.MeasureString(Text) / 2;
                Vector2 textPosition = parentPosition + Offset + Size/2;
                spriteBatch.DrawString(Assets.textFont, Text, textPosition, TextColor, 0, textMiddlePoint, Engine.UIScale, SpriteEffects.None, 0.45f);
            }
        }
    }
}
