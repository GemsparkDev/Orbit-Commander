using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
            position = Vector2.Zero;
            texture = null;
            enabled = false;
        }
        public override IFunctional GetWidgetOver()
        {
            return new DummyWidget();
        }
        public override void Draw(SpriteBatch _spriteBatch) { }
    }
}
