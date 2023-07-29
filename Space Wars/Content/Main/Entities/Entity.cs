using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Space_Wars.Content.Main.Entities
{
    public enum EntityType { Projectile, Enemy, None }
    public abstract class Entity
    {
        protected Texture2D texture;
        protected Color color = Color.White;
        public Vector2 position;
        public Vector2 velocity;
        public float angularVelocity;
        public bool isExpired;
        public bool isFriendly;
        public float angle;
        public EntityType entityType;
        public int damage;
        public float ColliderRadius
        {
            get { return (texture.Height + texture.Width) / 2; }

        }
        public Vector2 Size
        {
            get { return texture == null ? Vector2.Zero : new Vector2(texture.Width, texture.Height); }
        }

        public abstract void Update();
        public abstract void Collide(int damage);

        public void ClampVelocity(float speed)
        {
            if (velocity.X > speed)
            {
                velocity.X = speed;
            }
            if (velocity.X < -speed)
            {
                velocity.X = -speed;
            }
            if (velocity.Y > speed)
            {
                velocity.Y = speed;
            }
            if (velocity.Y < -speed)
            {
                velocity.Y = -speed;
            }
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            //Draws itself on the given spritebatch, position is offset by the screen position offset
            spriteBatch.Draw(texture, position + Engine.screenPosition - Engine.mousePositionOffset, null, color, angle, Size / 2, 1, 0, 0.2f);

            if (Engine.debugMode == true)
            {
                //Draws a line in the direction of motion for X
                spriteBatch.Draw(Engine.line, position + Engine.screenPosition - Engine.mousePositionOffset, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.White,
                    MathF.Atan2(0, velocity.X), Vector2.Zero, new Vector2(MathF.Abs(velocity.X), 1), SpriteEffects.None, 0.4f);
                //Draws a line in the direction of motion for Y
                spriteBatch.Draw(Engine.line, position + Engine.screenPosition - Engine.mousePositionOffset, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.White,
                    MathF.Atan2(velocity.Y, 0), Vector2.Zero, new Vector2(MathF.Abs(velocity.Y), 1), SpriteEffects.None, 0.4f);
                //Draws a line in the direction the entity is pointing
                spriteBatch.Draw(Engine.line, position + Engine.screenPosition - Engine.mousePositionOffset, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.Red,
                    angle - MathF.PI / 2, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.4f);
            }
        }
    }
}
