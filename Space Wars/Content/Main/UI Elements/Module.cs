using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace Space_Wars.Content.Main.UI_Elements
{
    public class Module : Item
    {
        public float health;
        public float maxHealth;
        public float[] cost;

        public Module(float _health, float[] _cost, Texture2D _texture, string _name, int _id, Vector2 _position, float _angularVelocity) : base(_texture, _name, _id, _position, _angularVelocity)
        {
            health = _health;
            maxHealth = health;
            cost = _cost;
            texture = _texture;
            name = _name;
            id = _id;
            position = _position;
            angle = 0;
            angularVelocity = _angularVelocity;
            velocity = Vector2.Zero;
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
