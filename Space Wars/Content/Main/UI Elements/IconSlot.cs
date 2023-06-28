using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Space_Wars.Content.Main.UI_Elements
{
    public class IconSlot : Widget, IFunctional
    {
        private DraggableIcon daughterIcon;
        private UIManager UIManager;
        public IconSlot(Vector2 _offset, Texture2D _texture, UIManager _UImanager)
        {
            size = new Vector2(texture.Width, texture.Height);
            offset = _offset - Size / 2;
            texture = _texture;
            daughterIcon = null;
            UIManager = _UImanager;
        }
        public IconSlot(Vector2 _offset, Texture2D _texture, DraggableIcon _daughterIcon, UIManager _UImanager)
        {
            size = new Vector2(_texture.Width, _texture.Height);
            offset = _offset - Size / 2;
            texture = _texture;
            daughterIcon = _daughterIcon;
            UIManager = _UImanager;
        }
        public void Interact()
        {
            (daughterIcon, UIManager.selectedIcon) = (UIManager.selectedIcon, daughterIcon);
        }
        public void AddBehaviour(DelegateMethod func) { }
        public void ApplyBehaviours() { }
        public override void Initialize() { }
        public override void Draw(SpriteBatch spriteBatch, Vector2 parentPositon)
        {
            if (daughterIcon != null)
            {
                spriteBatch.Draw(daughterIcon.texture, parentPositon + offset, null, Color.White, 0, Vector2.One / 2, Engine.UIScale, SpriteEffects.None, 0.35f);
            }
        }
    }
}
