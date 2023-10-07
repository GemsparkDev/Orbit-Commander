using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main;
using Space_Wars.Content.Main.Entities;
using System;
using System.Collections.Generic;

namespace Space_Wars.Content.Main.UI_Elements
{
    public class Item : Entity
    {
        internal int id;
        internal string name;
        public ItemSlot parent;
        public Enemy daughterEnemy;
        public Item(Texture2D _texture, string _name, int _id, Vector2 _position, Vector2 _velocity, float _angularVelocity, Color _color)
        {
            texture = _texture;
            name = _name;
            id = _id;
            position = _position;
            angle = 0;
            angularVelocity = _angularVelocity;
            velocity = _velocity;
            color = _color;
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
                        SoundManager.PlaySound(Assets.SoundFX["Interact"], position);
                    }
                    else
                    {
                        SoundManager.PlaySound(Assets.SoundFX["Full"], position);
                    }
                }
                velocity /= 2*Engine.deltaSeconds + 1;
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

            if(daughterEnemy != null)
            {
                if (daughterEnemy.health <= 0)
                {
                    isExpired = true;
                }
                else
                {
                    daughterEnemy.position = position;
                }
            }
        }
        public override void Collide(int _damage)
        {
            if (!EntityManager.player.leashedMaterials.Contains(this))
            {
                isExpired = true;
                if (daughterEnemy != null)
                {
                    daughterEnemy.isExpired = true;
                }
            }
        }
    }
    public static class ItemFactory
    {
        //Items
        public static Item NewScrap(Vector2 _position, Vector2 _velocity, float _angle)
        {
            return new Item(Assets.Sprites["Metal Scrap"], "Metal Salvage", 1, _position, _velocity, _angle, Color.Cyan);
        }
        //Modules
        public static Module NewBasicHullModule(Vector2 _position, Vector2 _velocity, float _angle)
        {
            return new(20, new float[] { 1 }, Assets.Sprites["Hull Module"], "Basic Hull", ModuleType.Hull, 0, _position, _velocity, _angle, Color.White);
        }
        public static Module NewBasicGunModule(Vector2 _position, Vector2 _velocity, float _angle)
        {
            return new(20, new float[] { 1 }, Assets.Sprites["Gun Module"], "Basic Guns", ModuleType.Guns, 2, _position, _velocity, _angle, Color.White);
        }
        public static Module NewBasicEngineModule(Vector2 _position, Vector2 _velocity, float _angle)
        {
            return new(20, new float[] { 1 }, Assets.Sprites["Engine Module"], "Basic Engines", ModuleType.Engines, 0, _position, _velocity, _angle, Color.White);
        }
        public static Module NewBasicSensorModule(Vector2 _position, Vector2 _velocity, float _angle)
        {
            return new(20, new float[] { 1 }, Assets.Sprites["Sensor Module"], "Basic Sensors", ModuleType.Sensors, 0, _position, _velocity, _angle, Color.White);
        }
        public static Module NewBasicCoreModule(Vector2 _position, Vector2 _velocity, float _angle)
        {
            Module module = new(20, new float[] { 1 }, Assets.Sprites["Core Module"], "Basic Core", ModuleType.Core, 0, _position, _velocity, _angle, Color.White);
            return module;
        }
    }
}
