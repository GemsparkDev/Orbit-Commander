using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Space_Wars.Content.Main.UI_Elements
{
    internal class Window : Container
    {
        private List<Widget> children = new();
        private List<IFunctional> functionalChildren = new();
        public Window(Vector2 _position, Texture2D _texture, float _transparency = 1)
        {
            size = new Vector2(_texture.Width, _texture.Height);
            texture = _texture;
            position = _position - Size / 2 * Engine.UIScale;
            transparency = _transparency;
            enabled = true;
            children = new List<Widget>();
            functionalChildren = new List<IFunctional>();
        }
        public override IFunctional GetWidgetOver()
        {
            Vector2 mousePosition = new(Mouse.GetState().X - position.X, Mouse.GetState().Y - position.Y);
            float bestDistance = float.MaxValue;
            float currentDistance;
            IFunctional bestWidget = new DummyWidget();
            foreach (IFunctional functionalWidget in functionalChildren)
            {
                Widget widget = functionalWidget as Widget ?? new DummyWidget();
                if (widget.Offset.X <= mousePosition.X && mousePosition.X <= widget.Offset.X + widget.Size.X * Engine.UIScale && widget.Offset.Y <= mousePosition.Y && mousePosition.Y <= widget.Offset.Y + widget.Size.Y * Engine.UIScale)
                {
                    currentDistance = EntityManager.DistanceSqr(widget.Size/2 * Engine.UIScale + widget.Offset, mousePosition);
                    if (currentDistance < bestDistance)
                    {
                        bestDistance = currentDistance;
                        bestWidget = functionalWidget;
                    }
                }
            }
            return bestWidget;
        }
        public override bool GetMouseOver()
        {
            Vector2 mousePosition = new(Mouse.GetState().X, Mouse.GetState().Y);
            if (position.X <= mousePosition.X && mousePosition.X <= position.X + Size.X * Engine.UIScale && position.Y <= mousePosition.Y && mousePosition.Y <= position.Y + Size.Y * Engine.UIScale)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (var widget in children)
            {
                if (widget.texture != null)
                {
                    spriteBatch.Draw(widget.texture, position + widget.Offset, null, Color.White, 0, Vector2.Zero, Engine.UIScale, SpriteEffects.None, 0.4f);
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
                        spriteBatch.Draw(widget.texture, position + widget.Offset, null, Color.White * transparency, 0, Vector2.Zero, Engine.UIScale, SpriteEffects.None, 0.4f);
                        widget.Draw(spriteBatch, position);
                    }
                    else
                    {
                        widget.Draw(spriteBatch, position);
                    }
                }
            }
        }
        public override void AddWidget(Widget widget, int index = 0)
        {
            children.Add(widget);
        }
        public override void AddWidget(IFunctional widget, int index = 0)
        {
            functionalChildren.Add(widget);
        }
        public override Widget GetWidget(int index = 0)
        {
            return children[index];
        }
        public override IFunctional GetFuncWidget(int index = 0)
        {
            return functionalChildren[index];
        }
    }
}
