using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

namespace Space_Wars.Content.Main.UI_Elements
{
    public class DraggableIcon : Widget
    {
        public DraggableIcon(Texture2D texture)
        {
            Offset = Vector2.Zero;
            Texture = texture;
        }
        public override void Initialize() { }
        public override void Draw(SpriteBatch spriteBatch, Vector2 parentPositon) { }
    }
}
