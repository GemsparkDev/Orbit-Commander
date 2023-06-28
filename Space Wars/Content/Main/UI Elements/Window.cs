using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Space_Wars.Content.Main.UI_Elements
{
    internal class Window : Container
    {
        public Window(Vector2 _position, Texture2D _texture)
        {
            size = new Vector2(_texture.Width, _texture.Height);
            texture = _texture;
            position = _position - Size / 2;
            enabled = true;
            children = new List<Widget>();
            functionalChildren = new List<IFunctional>();
        }
        public override IFunctional GetWidgetOver()
        {
            Vector2 mousePosition = new(Mouse.GetState().X - position.X, Mouse.GetState().Y - position.Y);
            foreach (IFunctional functionalWidget in functionalChildren)
            {
                Widget widget = functionalWidget as Widget ?? new DummyWidget();
                if (widget.offset.X <= mousePosition.X && mousePosition.X <= widget.offset.X + widget.Size.X && widget.offset.Y <= mousePosition.Y && mousePosition.Y <= widget.offset.Y + widget.Size.Y)
                {
                    return functionalWidget;
                }
            }
            return new DummyWidget();
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (var widget in children)
            {
                if (widget.texture != null)
                {
                    spriteBatch.Draw(widget.texture, position + widget.offset, null, Color.White, 0, Vector2.One / 2, Engine.UIScale, SpriteEffects.None, 0.4f);
                    widget.Draw(spriteBatch, position);
                }
                else
                {
                    widget.Draw(spriteBatch, position);
                }
            }
            foreach (var functionalWidget in functionalChildren)
            {
                Widget widget = functionalWidget as Widget;
                if (widget != null)
                {
                    if (widget.texture != null)
                    {
                        spriteBatch.Draw(widget.texture, position + widget.offset, null, Color.White, 0, Vector2.One / 2, Engine.UIScale, SpriteEffects.None, 0.4f);
                        widget.Draw(spriteBatch, position);
                    }
                    else
                    {
                        widget.Draw(spriteBatch, position);
                    }
                }
            }
        }
    }
}
