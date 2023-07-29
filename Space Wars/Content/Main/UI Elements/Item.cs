using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Space_Wars.Content.Main.UI_Elements
{
    public class Item
    {
        internal int id;
        internal string name;
        internal Texture2D texture;
        public ItemSlot parent;

        public Item(Texture2D _texture, string _name, int _id)
        {
            texture = _texture;
            name = _name;
            id = _id;
            parent = null;
        }
    }
    public static class ItemFactory
    {
        public static Item NewScrap()
        {
            return new Item(Assets.Sprites["Metal Scrap"], "Metal Salvage", 1);
        }
    }
}
