using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Space_Wars.Content.Main.UI_Elements
{
    public abstract class Widget
    {
        protected Vector2 size;
        protected Vector2 offset;
        public string text;
        public Color textColor;
        public Texture2D texture;
        public Vector2 Offset
        {
            get { return offset * Engine.UIScale; }
        }
        public Vector2 Size
        {
            get { return size; }
        }
        public Widget()
        {
            offset = Vector2.Zero;
            texture = null;
        }
        public abstract void Initialize();
        public abstract void Draw(SpriteBatch _spriteBatch, Vector2 _parentPositon);
    }
}
