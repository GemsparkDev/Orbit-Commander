using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Space_Wars.Content.Main.UI_Elements
{
    public abstract class Container
    {
        protected Vector2 size;
        public Vector2 position;
        //private Vector2 prevMousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        public bool enabled;
        public Texture2D texture;
        public float transparency = 1;
        public Vector2 Size
        {
            get { return size; }
        }
        public abstract void AddWidget(Widget widget, int tab = 0);
        public abstract void AddWidget(IFunctional widget, int tab = 0);
        public abstract Widget GetWidget(int index);
        public abstract IFunctional GetFuncWidget(int index);
        public virtual bool GetMouseOver()
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
        public abstract IFunctional GetWidgetOver();
        public abstract void Draw(SpriteBatch spriteBatch);

        /*
        public virtual void MoveContainer()
        {
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                position += new Vector2(Mouse.GetState().X, Mouse.GetState().Y) - prevMousePosition;
                prevMousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            }
        }
        */

    }
}
