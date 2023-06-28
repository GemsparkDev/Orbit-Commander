using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Space_Wars.Content.Main.UI_Elements
{
    public class DummyWidget : Widget, IFunctional
    {
        public new Vector2 Size
        {
            get { return Vector2.Zero; }
        }
        public DummyWidget()
        {
            offset = Vector2.Zero;
            texture = null;
        }
        public void Interact() { }
        public void AddBehaviour(DelegateMethod func) { }
        public void ApplyBehaviours() { }
        public override void Initialize() { }
        public override void Draw(SpriteBatch spriteBatch, Vector2 parentPositon) { }
    }
}
