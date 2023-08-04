using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;
using System;

namespace Space_Wars.Content.Main.UI_Elements
{
    public class Item : Entity, IFunctional
    {
        internal int id;
        internal string name;
        public ItemSlot parent;

        public Item(Texture2D _texture, string _name, int _id, Vector2 _position, float _angularVelocity)
        {
            texture = _texture;
            name = _name;
            id = _id;
            position = _position;
            angle = 0;
            angularVelocity = _angularVelocity;
            velocity = Vector2.Zero;
            parent = null;
        }

        public override void Update()
        {
            if (!EntityManager.player.leashedMaterials.Contains(this))
            {
                if (EntityManager.DistanceSqr(EntityManager.player, this) < 1250 && EntityManager.player.leashedMaterials.Count < 1 && EntityManager.player.canGatherResources == true)
                {
                    EntityManager.player.leashedMaterials.Add(this);
                    if (EntityManager.player.leashedMaterials.Count < 2)
                    {
                        Engine.PlaySound(Assets.SoundFX["Interact"], position);
                    }
                    else
                    {
                        Engine.PlaySound(Assets.SoundFX["Full"], position);
                    }
                }
            }
            else
            {
                Vector2 playerVelocity = EntityManager.player.velocity;
                Vector2 leashPosition = EntityManager.player.position - Engine.ToUnitVector(EntityManager.player.angle) * 25;
                float distance = EntityManager.DistanceSqr(position, leashPosition);
                if (distance > 16)
                {
                    velocity += Vector2.Normalize(leashPosition - position) * Engine.deltaSeconds * distance;
                }
                else
                {
                    velocity += (playerVelocity - velocity) / 2;
                }
                ClampVelocity(MathF.Sqrt(playerVelocity.X * playerVelocity.X + playerVelocity.Y * playerVelocity.Y) + 1);
            }
            position += velocity * Engine.deltaSeconds * 60;
            angle += angularVelocity * Engine.deltaSeconds * 60;
        }
        public void Interact(Vector2 _position)
        {

        }
        public void ContinuousInteract(Vector2 _position)
        {

        }
        public void AddBehaviour(DelegateMethod _func)
        {
            
        }

        public void ApplyBehaviours()
        {
            
        }
        public override void Collide(int _damage)
        {
            isExpired = true;
        }
    }
    public static class ItemFactory
    {
        public static Item NewScrap(Vector2 _position, float _angle)
        {
            return new Item(Assets.Sprites["Metal Scrap"], "Metal Salvage", 1, _position, _angle);
        }
        public static Item NewSentry(Vector2 _position, float _angle)
        {
            return new Item(Assets.Sprites["Metal Scrap"], "Metal Salvage", 1, _position, _angle);
        }
    }
}
