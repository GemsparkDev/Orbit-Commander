using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Space_Wars.Content.Main.UI_Elements
{
    public abstract class Container
    {

        public List<Widget> children;
        public List<IFunctional> functionalChildren;
        protected Vector2 size;
        public Vector2 position;
        public bool enabled;
        public Texture2D texture;
        public Vector2 Size
        {
            get { return size * Engine.UIScale; }
        }
        public void AddWidget(Widget widget)
        {
            children.Add(widget);
        }
        public void AddWidget(IFunctional widget)
        {
            functionalChildren.Add(widget);
        }
        public bool GetMouseOver()
        {
            Vector2 mousePosition = new(Mouse.GetState().X, Mouse.GetState().Y);
            if (position.X <= mousePosition.X && mousePosition.X <= position.X + Size.X && position.Y <= mousePosition.Y && mousePosition.Y <= position.Y + Size.Y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public abstract IFunctional GetWidgetOver();
        public abstract void Draw(SpriteBatch spriteBatch);
    }
}
