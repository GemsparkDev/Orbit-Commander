using Microsoft.Xna.Framework;

namespace Space_Wars.Content.Main.Entities
{
    public class Mothership : Entity
    {
        private float health;
        private float maxHealth;
        public int scrap;
        public Mothership(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity)
        {
            position = _position;
            velocity = _velocity;
            angle = _angle;
            angularVelocity = _angularVelocity;
            texture = Assets.Sprites["Mothership"];
            health = 100;
            maxHealth = health;
            isFriendly = true;
            entityType = EntityType.None;
        }

        public override void Update()
        {
            if (health <= 0)
            {
                isExpired = true;
                Engine.PlaySound(Assets.SoundFX["Death"], position);
            }
            if (health > maxHealth)
            {
                health = maxHealth;
            }

            position += velocity;
            angle += angularVelocity;
            angularVelocity = 0;
        }
        public override void Collide(int damage)
        {
            health -= damage;
            if (damage > 0)
            {
                Engine.PlaySound(Assets.SoundFX["Hit"], position);
            }
        }
    }
}
