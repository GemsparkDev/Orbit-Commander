using Microsoft.Xna.Framework;
using Space_Wars.Content.Main.UI_Elements;

namespace Space_Wars.Content.Main.Entities
{
    public class Mothership : Entity
    {
        private float health = 100;
        private float maxHealth;
        private float furnaceCooldown = 15;
        public int scrap = 0;
        public Item[,] inventory = new Item[1, 3];
        public Item furnaceItem;
        public bool wasEmpty = true;
        public Mothership(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity)
        {
            position = _position;
            velocity = _velocity;
            angle = _angle;
            angularVelocity = _angularVelocity;
            texture = Assets.Sprites["Mothership"];
            maxHealth = health;
            isFriendly = true;
            color = new Color(0, 255, 0);
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
            if(furnaceItem != null)
            {
                furnaceCooldown -= Engine.deltaSeconds;
                if(furnaceCooldown <= 0)
                {
                    scrap++;
                    furnaceItem = null;
                    Engine.PlaySound(Assets.SoundFX["Interact"], position);
                }
            }
            else
            {
                wasEmpty = true;
                furnaceCooldown = 15;
            }
            EventHandler.UpdateFurnaceUI(15-furnaceCooldown, 15);

            position += velocity;
            angle += angularVelocity;
            angularVelocity = 0;

        }
        public void AddItem(Item _item)
        {
            for (int y = 0; y < inventory.GetLength(1); y++)
            {
                for (int x = 0; x < inventory.GetLength(0); x++)
                {
                    if (inventory[x,y] == null)
                    {
                        inventory[x,y] = _item;
                        EventHandler.UpdateInventoryUI();
                        return;
                    }
                }
            }
        }
        public bool IsFull()
        {
            for (int y = 0; y < inventory.GetLength(1); y++)
            {
                for (int x = 0; x < inventory.GetLength(0); x++)
                {
                    if (inventory[x, y] == null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public override void Collide(int _damage)
        {
            health -= _damage;
            if (_damage > 0)
            {
                Engine.PlaySound(Assets.SoundFX["Hit"], position);
            }
        }
    }
}
