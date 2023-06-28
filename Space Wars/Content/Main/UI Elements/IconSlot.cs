using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Space_Wars.Content.Main.UI_Elements
{
    public class IconSlot : Widget, IFunctional
    {
        private DraggableIcon DaughterIcon;
        private UIManager _UIManager;
        public IconSlot(Vector2 offset, Texture2D texture, UIManager UImanager)
        {
            _Size = new Vector2(texture.Width, texture.Height);
            Offset = offset - Size / 2;
            Texture = texture;
            DaughterIcon = null;
            _UIManager = UImanager;
        }
        public IconSlot(Vector2 offset, Texture2D texture, DraggableIcon daughterIcon, UIManager UImanager)
        {
            _Size = new Vector2(texture.Width, texture.Height);
            Offset = offset - Size / 2;
            Texture = texture;
            DaughterIcon = daughterIcon;
            _UIManager = UImanager;
        }
        public void Interact() 
        {
            (DaughterIcon, _UIManager.selectedIcon) = (_UIManager.selectedIcon, DaughterIcon);
        }
        public void AddBehaviour(DelegateMethod func) { }
        public void ApplyBehaviours() { }
        public override void Initialize() { }
        public override void Draw(SpriteBatch spriteBatch, Vector2 parentPositon)
        {
            if(DaughterIcon != null)
            {
                spriteBatch.Draw(DaughterIcon.Texture, parentPositon+Offset, null, Color.White, 0, Vector2.One / 2, Engine.UIScale, SpriteEffects.None, 0.35f);
            }
        }
    }
}
