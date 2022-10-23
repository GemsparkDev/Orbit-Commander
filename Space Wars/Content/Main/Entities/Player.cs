using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using Space_Wars.Content.Main.Entities;

using System.Diagnostics;

namespace Space_Wars.Content.Main.Entities
{
    public class Player : Entity
    {
        private Keys[] pressedKey;
        private KeyboardState oldState;
        public Mothership mothership;
        public int Health;
        public int MaxHealth;
        private float Cooldown = 0;
        private Vector2 TargetVector;
        public bool docked = false;
        public Projectile leashedMaterial;
        private Projectile velocityArrow;

        //Player constructor
        public Player(Vector2 position, Vector2 velocity, float angle, float angularVelocity)
        {
            Position = position;
            Velocity = velocity;
            Angle = angle;
            AngularVelocity = angularVelocity;
            Texture = Assets.Sprites["Player"];
            Health = 100;
            MaxHealth = Health;
            IsFriendly = true;
            velocityArrow = new VelocityArrow(Position, Velocity, Angle, AngularVelocity, true);
            EntityManager.Add(velocityArrow);
        }
        public override void Update()
        {
            if (Health <= 0)
            {
                IsExpired = true;
                Assets.SoundFX["Death"].Play();
            }
            if (Health > MaxHealth)
            {
                Health = MaxHealth;
            }
            if (Health > 0)
            {
                ControlShip();
            }

            velocityArrow.Angle = Velocity.ToDirection(0);
            velocityArrow.Position = Position + velocityArrow.Angle.ToUnitVector(0) * ColliderRadius * 1.5f;
        }
        public override void Collide(int damage)
        {
            Health -= damage;
            if (damage > 0)
            {
                Assets.SoundFX["Hit"].Play();
            }
        }
        public void ControlShip()
        {
            if (Mouse.GetState().LeftButton == ButtonState.Pressed && Cooldown <= 0)
            {
                TargetVector = (new Vector2(Mouse.GetState().X - Engine.screenPosition.X - Position.X, Mouse.GetState().Y - Engine.screenPosition.Y - Position.Y).ToUnitVector(0));
                EntityManager.Add(new PulseShot(Position, TargetVector * 8, TargetVector.ToDirection(0), 0, true));
                Assets.SoundFX["Fire_1"].Play();
                Cooldown = 0.25f;
            }

            pressedKey = Keyboard.GetState().GetPressedKeys();
            KeyboardState newState = Keyboard.GetState();

            //Checks for player input and moves the character using said input
            for (int i = 0; i < pressedKey.Length; i++)
            {
                switch (pressedKey[i])
                {
                    case Keys.W:
                        Move(3*MathF.PI/2, 10);
                        break;
                    case Keys.S:
                        Move(MathF.PI/2, 10);
                        break;
                    case Keys.A:
                        Move(MathF.PI, 10);
                        break;
                    case Keys.D:
                        Move(0, 10);
                        break;
                    case Keys.Q:
                        Rotate(-0.1f);
                        break;
                    case Keys.E:
                        Rotate(0.1f);
                        break;
                    default:
                        break;
                }
            }

            if (oldState.IsKeyUp(Keys.R) && newState.IsKeyDown(Keys.R))
            {
                Dock();
            }

            if (oldState.IsKeyUp(Keys.OemTilde) && newState.IsKeyDown(Keys.OemTilde))
            {
                Engine.debugMode = !Engine.debugMode;
            }

            oldState = newState;

            //Moves the player, but offsets the screen to keep the player in the middle
            Position += Velocity;
            Angle += AngularVelocity;
            AngularVelocity = 0;

            if (Cooldown > 0)
            {
                Cooldown -= Engine.deltaSeconds;
            }

            ClampVelocity(5);
        }

        private void Dock()
        {
            if (EntityManager.DistanceSqr(this, mothership) < 1250)
            {
                docked = !docked;
                Position = mothership.Position;
                Velocity = mothership.Velocity;
                AngularVelocity = mothership.AngularVelocity;
                if (docked == false)
                {
                    Move(mothership.Angle, 1);
                    mothership.Dock();
                }
                if (docked == true)
                {
                    Health = MaxHealth;
                    if (leashedMaterial != null)
                    {
                        leashedMaterial.Position = mothership.Position;
                        EntityManager.Collide(leashedMaterial, mothership);
                        leashedMaterial = null;
                    }
                    mothership.Dock();
                }
            }
        }
        private void Move(float angle, float speed)
        {
            if (docked == false)
            {
                Velocity += angle.ToUnitVector(0) / speed;
            }
            if (docked == true)
            {
                mothership.Velocity += angle.ToUnitVector(0) / speed / 2;
                Velocity += angle.ToUnitVector(0) / speed / 2;
            }
        }
        private void Rotate(float speed)
        {
            if(docked == false)
            {
                AngularVelocity += speed;
            }
            if (docked == true)
            {
                mothership.AngularVelocity += speed / 2;
                AngularVelocity += speed / 2;
            }
        }
    }
}
