using Microsoft.Xna.Framework.Graphics;

namespace Space_Wars.Content.Main
{
    internal struct Module
    {
        public float health;
        public readonly float[] cost;
        public readonly Texture2D texture;
        public readonly string name;

        public Module(float _health, float[] _cost, Texture2D _texture, string _name)
        {
            health = _health;
            cost = _cost;
            texture = _texture;
            name = _name;
        }
    }
}
