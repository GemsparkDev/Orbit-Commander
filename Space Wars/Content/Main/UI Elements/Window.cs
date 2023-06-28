using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Space_Wars.Content.Main.UI_Elements
{
    internal class Window : Container
    {
        public Window(Vector2 position, Texture2D texture)
        {
            _Size = new Vector2(texture.Width, texture.Height);
            Texture = texture;
            Position = position - Size/2;
            Enabled = true;
            Children = new List<Widget>();
            FunctionalChildren = new List<IFunctional>();
        }
        public override IFunctional GetWidgetOver()
        {
            Vector2 mousePosition = new(Mouse.GetState().X - Position.X, Mouse.GetState().Y - Position.Y);
            foreach (IFunctional functionalWidget in FunctionalChildren)
            {
                Widget widget = functionalWidget as Widget ?? new DummyWidget();
                if (widget.Offset.X <= mousePosition.X && mousePosition.X <= widget.Offset.X + widget.Size.X && widget.Offset.Y <= mousePosition.Y && mousePosition.Y <= widget.Offset.Y + widget.Size.Y)
                {
                    return functionalWidget;
                }
            }
            return new DummyWidget();
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (var widget in Children)
            {
                if (widget.Texture != null)
                {
                    spriteBatch.Draw(widget.Texture, Position + widget.Offset, null, Color.White, 0, Vector2.One / 2, Engine.UIScale, SpriteEffects.None, 0.4f);
                    widget.Draw(spriteBatch, Position);
                }
                else
                {
                    widget.Draw(spriteBatch, Position);
                }
            }
            foreach (var functionalWidget in FunctionalChildren)
            {
                Widget widget = functionalWidget as Widget;
                if (widget != null)
                {
                    if (widget.Texture != null)
                    {
                        spriteBatch.Draw(widget.Texture, Position + widget.Offset, null, Color.White, 0, Vector2.One / 2, Engine.UIScale, SpriteEffects.None, 0.4f);
                        widget.Draw(spriteBatch, Position);
                    }
                    else
                    {
                        widget.Draw(spriteBatch, Position);
                    }
                }
            }
        }
    }
}
