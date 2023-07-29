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
        public bool enabled;
        public Texture2D texture;
        public Vector2 Size
        {
            get { return size * Engine.UIScale; }
        }
        public abstract void AddWidget(Widget widget, int tab = 0);
        public abstract void AddWidget(IFunctional widget, int tab = 0);
        public abstract Widget GetWidget(int index);
        public abstract IFunctional GetFuncWidget(int index);
        public abstract bool GetMouseOver();
        public abstract IFunctional GetWidgetOver();
        public abstract void Draw(SpriteBatch spriteBatch);
    }
}
