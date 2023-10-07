using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Space_Wars.Content.Main.UI_Elements
{
    public class ItemSlot : Widget, IFunctional
    {
        public Item daughterItem;
        private UIManager UIManager;
        private List<DelegateMethod> behaviours = new();
        public readonly int id;
        public bool moduleOnlySlot;
        public ItemSlot(Vector2 _offset, Texture2D _texture, UIManager _UImanager, int _id, bool _moduleOnlySlot)
        {
            texture = _texture;
            size = new Vector2(texture.Width, texture.Height);
            offset = _offset - Size / 2;
            daughterItem = null;
            UIManager = _UImanager;
            id = _id;
            moduleOnlySlot = _moduleOnlySlot;
        }
        public ItemSlot(Vector2 _offset, Texture2D _texture, Item _daughterIcon, UIManager _UImanager, int _id, bool _moduleOnlySlot)
        {
            texture = _texture;
            size = new Vector2(_texture.Width, _texture.Height);
            offset = _offset - Size / 2;
            daughterItem = _daughterIcon;
            daughterItem.parent = this;
            UIManager = _UImanager;
            id = _id;
            moduleOnlySlot = _moduleOnlySlot;
        }
        private void UpdateDescription()
        { 

        }
        public void Interact(Vector2 parentPosition)
        {
            if(UIManager.selectedIcon == null || (UIManager.selectedIcon is Module && moduleOnlySlot == true) || moduleOnlySlot == false)
            {
                if (UIManager.selectedIcon != null)
                {
                    if (id == UIManager.selectedIcon.id || id == -1)
                    {
                        (daughterItem, UIManager.selectedIcon) = (UIManager.selectedIcon, daughterItem);
                        daughterItem.parent = this;
                    }
                }
                else
                {
                    (daughterItem, UIManager.selectedIcon) = (UIManager.selectedIcon, daughterItem);
                }
                for (int i = 0; i < behaviours.Count; i++)
                {
                    ApplyBehaviours();
                }
            }
            else
            {
                EventHandler.ReturnItemToParent();
                return;
            }
        }
        public void ContinuousInteract(Vector2 parentPosition)
        {

        }
        public void AddBehaviour(DelegateMethod func)
        {
            behaviours.Add(func);
        }
        public void ApplyBehaviours()
        {
            for (int i = 0; i < behaviours.Count; i++)
            {
                DelegateMethod func = behaviours[i];
                func();
            }
        }
        public override void Initialize() { }
        public override void Draw(SpriteBatch _spriteBatch, Vector2 _parentPosition)
        {
            if (daughterItem != null)
            {
                _spriteBatch.Draw(daughterItem.texture, _parentPosition + Offset + Size/2 * Engine.UIScale- daughterItem.Size/2 * Engine.UIScale, null, Color.White, 0, Vector2.Zero, Engine.UIScale, SpriteEffects.None, 0);
            }
        }
    }
}
