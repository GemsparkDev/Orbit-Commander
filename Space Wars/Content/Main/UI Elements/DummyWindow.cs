using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static System.Net.Mime.MediaTypeNames;

namespace Space_Wars.Content.Main.UI_Elements
{
    public class DummyWindow : Container
    {
        public new Vector2 Size
        {
            get { return Vector2.Zero; }
        }
        public DummyWindow()
        {
            Position = Vector2.Zero;
            Texture = null;
            Enabled = false;
        }
        public override IFunctional GetWidgetOver() 
        {
            return new DummyWidget();
        }
        public override void Draw(SpriteBatch spriteBatch) {}
    }
}
