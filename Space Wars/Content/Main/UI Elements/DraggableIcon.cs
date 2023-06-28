using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Space_Wars.Content.Main.UI_Elements
{
    public class DraggableIcon : Widget
    {
        public DraggableIcon(Texture2D _texture)
        {
            offset = Vector2.Zero;
            texture = _texture;
        }
        public override void Initialize() { }
        public override void Draw(SpriteBatch _spriteBatch, Vector2 _parentPositon) { }
    }
}
