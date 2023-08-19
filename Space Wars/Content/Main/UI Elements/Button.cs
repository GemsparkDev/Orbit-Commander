using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Space_Wars.Content.Main.UI_Elements
{
    public class Button : Widget, IFunctional
    {
        private List<DelegateMethod> behaviours = new();
        public Button(Vector2 _offset, Texture2D _texture)
        {
            size = new Vector2(_texture.Width, _texture.Height);
            offset = _offset - Size / 2;
            texture = _texture;
            text = null;
            textColor = Color.White;
        }

        public Button(Vector2 _offset, Texture2D _texture, string _text, Color _textColor)
        {
            size = new Vector2(_texture.Width, _texture.Height);
            offset = _offset - Size / 2;
            texture = _texture;
            text = _text;
            textColor = _textColor;
        }
        public Button(Vector2 _offset, string _text, Color _textColor)
        {
            size = new Vector2(_text.Length * 4, 12);
            offset = _offset - Size / 2;
            texture = null;
            text = _text;
            textColor = _textColor;
        }

        public void Interact(Vector2 parentPosition)
        {
            for (int i = 0; i < behaviours.Count; i++)
            {
                ApplyBehaviours();
            }
        }
        public void ContinuousInteract(Vector2 parentPosition)
        {

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
            if (text != null)
            {
                Vector2 textMiddlePoint = Assets.textFont.MeasureString(text) / 2;
                Vector2 textPosition = (parentPosition + Offset + Size);
                spriteBatch.DrawString(Assets.textFont, text, textPosition, textColor, 0, textMiddlePoint, Engine.UIScale, SpriteEffects.None, 0.45f);
            }
        }
    }
}
