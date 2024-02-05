using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Space_Wars.Content.Main.UI_Elements;
using System;

namespace Space_Wars.Content.Main.Entities
{
    public class Mothership : Entity
    {
        private float health = 100;
        private float maxHealth;
        private float furnaceCooldown = 15;
        private float craftingCooldown = 60;
        public int requiredCraftsLeft = 25;
        public int scrap = 0;
        public Item[,] inventory = new Item[1, 4];
        public Item furnaceItem;
        public Item craftingItem;
        public bool wasEmpty = true;
        public bool currentlyCrafting = false;
        public Mothership(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity)
        {
            position = _position;
            velocity = _velocity;
            angle = _angle;
            angularVelocity = _angularVelocity;
            texture = Assets.Get(Sprite.Mothership);
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
                SoundManager.PlaySound(Assets.Get(Sound.Death), position);
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
                    SoundManager.PlaySound(Assets.Get(Sound.Interact), position);
                }
            }
            else
            {
                wasEmpty = true;
                furnaceCooldown = 15;
            }
            if(currentlyCrafting == true)
            {
                craftingCooldown -= Engine.deltaSeconds;
                if(craftingCooldown <= 0)
                {
                    craftingCooldown = 60;
                    requiredCraftsLeft -= 5;
                    currentlyCrafting = false;
                }
            }
            EventHandler.UpdateFurnaceUI(15-furnaceCooldown, 15);
            EventHandler.UpdateCraftingUI(60-craftingCooldown, 60);

            position += velocity * Engine.deltaSeconds * 60;
            angle += angularVelocity * Engine.deltaSeconds * 60;
            angularVelocity = 0;

            if(requiredCraftsLeft <= 0)
            {
                CurrentGameState.SwitchState(new Victory());
            }

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
                SoundManager.PlaySound(Assets.Get(Sound.Hit), position);
            }
        }
    }
}
