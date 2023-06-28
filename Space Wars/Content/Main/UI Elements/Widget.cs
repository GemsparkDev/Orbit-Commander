using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Wars.Content.Main.UI_Elements
{
    public abstract class Widget
    {
        protected Vector2 _Size;
        public Vector2 Offset;
        public string Text;
        public Color TextColor;
        public Vector2 Size
        {
            get { return _Size * Engine.UIScale; }
        }
        public Texture2D Texture;
        public Widget()
        {
            Offset = Vector2.Zero;
            Texture = null;
        }
        public abstract void Initialize();
        public abstract void Draw(SpriteBatch spriteBatch, Vector2 parentPositon);
    }
}
