using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace Space_Wars.Content.Main.UI_Elements
{
    public abstract class Container
    {

        public List<Widget> Children;
        public List<IFunctional> FunctionalChildren;
        protected Vector2 _Size;
        public Vector2 Size
        {
            get { return _Size * Engine.UIScale; }
        }
        public Vector2 Position;
        public bool Enabled;
        public Texture2D Texture;
        public void AddWidget(Widget widget)
        {
            Children.Add(widget);
        }
        public void AddWidget(IFunctional widget)
        {
            FunctionalChildren.Add(widget);
        }
        public bool GetMouseOver()
        {
            Vector2 mousePosition = new(Mouse.GetState().X, Mouse.GetState().Y);
            if (Position.X <= mousePosition.X && mousePosition.X <= Position.X + Size.X && Position.Y <= mousePosition.Y && mousePosition.Y <= Position.Y + Size.Y)
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
