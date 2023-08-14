using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main.UI_Elements
{
    public enum ModuleType
    {
        Hull = 1,
        Guns = 2,
        Engines = 3,
        Sensors = 4,
        Core = 5,
    }
    public class Module : Item
    {
        public float health;
        public float maxHealth;
        public float[] cost;
        public int ability;

        public Module(float _health, float[] _cost, Texture2D _texture, string _name, ModuleType _moduleType, int _weaponID, Vector2 _position, Vector2 _velocity, float _angularVelocity, Color _color) : base(_texture, _name, _weaponID, _position, _velocity, _angularVelocity, _color)
        {
            health = _health;
            maxHealth = health;
            cost = _cost;
            texture = _texture;
            name = _name;
            id = (int)_moduleType;
            ability = _weaponID;
            position = _position;
            velocity = _velocity;
            angle = 0;
            angularVelocity = _angularVelocity;
            velocity = Vector2.Zero;
            color = _color;
            parent = null;
        }

        //public override void Draw(SpriteBatch _spriteBatch, Vector2 _parentPosition) 
        //{
        //    Color color = Color.White;
        //    if (health > maxHealth / 2) { color = Color.Green; }
        //    if (maxHealth/2 >= health  && health > maxHealth/4) { color = Color.Yellow; }
        //    if (maxHealth/4 >= health && health > 0) { color = Color.Orange; }
        //    if (health <= 0){ color = Color.Red; }
        //    _spriteBatch.Draw(Engine.line, _parentPosition + new Vector2(-2, texture.Height / 4), new Rectangle(0, 0, 4, 4),
        //        color, 0, Vector2.Zero, 1, SpriteEffects.None, 0); ;
        //}
    }
}
