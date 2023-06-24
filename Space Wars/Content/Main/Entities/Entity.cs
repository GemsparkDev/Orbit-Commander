using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Space_Wars.Content.Main.Entities
{
    public enum EntityType { Projectile, Enemy, None } 
    public abstract class Entity
    {
        protected Texture2D Texture;
        protected Color Color = Color.White;
        public Vector2 Position;
        public Vector2 Velocity;
        public float AngularVelocity;
        public bool IsExpired;
        public bool IsFriendly;
        private float _Angle;
        public EntityType entityType;
        public int damage;
        public float ColliderRadius
        {
            get { return (Texture.Height + Texture.Width) / 2; }

        }
        public float Angle
        {
            get { return _Angle - MathF.PI / 2; }
            set { _Angle = value + MathF.PI / 2; }
        }
        public Vector2 Size
        {
            get { return Texture == null ? Vector2.Zero : new Vector2(Texture.Width, Texture.Height); }
        }

        public abstract void Update();
        public abstract void Collide(int damage);

        public void ClampVelocity(float speed)
        {
            if(Velocity.X > speed)
            {
                Velocity.X = speed;
            }
            if (Velocity.X < -speed)
            {
                Velocity.X = -speed;
            }
            if (Velocity.Y > speed)
            {
                Velocity.Y = speed;
            }
            if (Velocity.Y < -speed)
            {
                Velocity.Y = -speed;
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            //Draws itself on the given spritebatch, position is offset by the screen position offset
            spriteBatch.Draw(Texture, Position + Engine.screenPosition, null, Color, _Angle, (Size / 2), 1, 0, 0.1f);

            if (Engine.debugMode == true)
            {
                //Draws a line in the direction of motion for X
                spriteBatch.Draw(Engine.line, Position + Engine.screenPosition, new Rectangle((int)Position.X, (int)Position.Y, 10, 1), Color.White,
                    new Vector2(Velocity.X, 0).ToDirection(0), Vector2.Zero, new Vector2(MathF.Abs(Velocity.X), 1), SpriteEffects.None, 0.2f);
                //Draws a line in the direction of motion for Y
                spriteBatch.Draw(Engine.line, Position + Engine.screenPosition, new Rectangle((int)Position.X, (int)Position.Y, 10, 1), Color.White,
                    new Vector2(0, Velocity.Y).ToDirection(0), Vector2.Zero, new Vector2(MathF.Abs(Velocity.Y), 1), SpriteEffects.None, 0.2f);
                //Draws a line in the direction the entity is pointing
                spriteBatch.Draw(Engine.line, Position + Engine.screenPosition, new Rectangle((int)Position.X, (int)Position.Y, 10, 1), Color.Red,
                    _Angle - MathF.PI / 2, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.2f);
            }
        }
    }
}
