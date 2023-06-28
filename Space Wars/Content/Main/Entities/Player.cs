using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using Space_Wars.Content.Main.Entities;

using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.Xna.Framework.Audio;
using System.Linq;

namespace Space_Wars.Content.Main.Entities
{
    public class Player : Entity
    {
        private Keys[] pressedKey;
        private Random random = new();
        private KeyboardState oldState;
        public Mothership mothership;
        private float Cooldown = 0;
        private Vector2 TargetVector;
        public bool docked = false;
        public bool canGatherResources;
        public List<Projectile> leashedMaterials = new();
        private Projectile mothershipArrow;
        private float[] healthStats = { 20, 20, 20, 20, 20 };

        public Player(Vector2 position, Vector2 velocity, float angle, float angularVelocity)
        {
            Position = position;
            Velocity = velocity;
            Angle = angle;
            AngularVelocity = angularVelocity;
            Texture = Assets.Sprites["Player"];
            IsFriendly = true;
            mothershipArrow = new MothershipArrow(Position, Velocity, Angle, AngularVelocity, true);
            EntityManager.Add(mothershipArrow);
        }
        public override void Update()
        {
            if (healthStats[4] <= 0)
            {
                IsExpired = true;
                Assets.SoundFX["Death"].Play();
            }
            if (healthStats[4] > 0)
            {
                ControlShip();
            }

            mothershipArrow.Angle = (mothership.Position - Position).ToDirection(0);
            mothershipArrow.Position = Position + mothershipArrow.Angle.ToUnitVector(0) * ColliderRadius * 1.5f;
            for (int i = 0; i < 4; i++)
            {
                UIManager.GetContainer(Containers.PlayerMenu).Children[i].Text = healthStats[i].ToString();
                if (healthStats[i] > 10)
                {
                    UIManager.GetContainer(Containers.PlayerMenu).Children[i].TextColor = Color.White;
                }
                else if (healthStats[i] <= 0)
                {
                    UIManager.GetContainer(Containers.PlayerMenu).Children[i].TextColor = Color.Black;
                }
                else
                {
                    UIManager.GetContainer(Containers.PlayerMenu).Children[i].TextColor = Color.Red;
                }
            }
            UIManager.GetContainer(Containers.PlayerMenu).Children[4].Text = healthStats[4].ToString();
        }
        public override void Collide(int damage)
        {
            if (damage > 0)
            {
                for (int i = 0; i < damage; i++)
                {
                    if (random.NextDouble() <= healthStats[0] / 20)
                    {
                        healthStats[0]--;
                    }
                    else
                    {
                        int randomNumber = random.Next(1, 4);
                        if (healthStats[randomNumber] > 0)
                        {
                            healthStats[randomNumber]--;
                        }
                        else if (healthStats[0] > 0)
                        {
                            healthStats[0]--;
                        }
                        else
                        {
                            healthStats[4]--;
                        }
                    }
                }
                Engine.PlaySound(Assets.SoundFX["Hit"], Position);
            }
        }
        public void ControlShip()
        {
            TargetVector = (new Vector2(Mouse.GetState().X - Engine.screenPosition.X - Position.X, Mouse.GetState().Y - Engine.screenPosition.Y - Position.Y).ToUnitVector(0));
            Angle = TargetVector.ToDirection(0);
            if (Mouse.GetState().LeftButton == ButtonState.Pressed && Cooldown <= 0)
            {
                Shotgun();
            }

            pressedKey = Keyboard.GetState().GetPressedKeys();
            KeyboardState newState = Keyboard.GetState();

            for (int i = 0; i < pressedKey.Length; i++)
            {
                if (docked == false)
                {
                    float playerSpeedScalar = MathF.Sqrt(Velocity.X * Velocity.X + Velocity.Y * Velocity.Y);
                    float speedMultiplier = (1 / (playerSpeedScalar + 0.2f) + 1) * (healthStats[2]/40+0.5f) / (leashedMaterials.Count + 1);
                    switch (pressedKey[i])
                    {
                        case Keys.W:
                            Move(3 * MathF.PI / 2, speedMultiplier);
                            break;
                        case Keys.S:
                            Move(MathF.PI / 2, speedMultiplier);
                            break;
                        case Keys.A:
                            Move(MathF.PI, speedMultiplier);
                            break;
                        case Keys.D:
                            Move(0, speedMultiplier);
                            break;
                        default:
                            break;
                    }
                }
            }

            if (oldState.IsKeyUp(Keys.R) && newState.IsKeyDown(Keys.R))
            {
                Dock();
            }
            if (oldState.IsKeyUp(Keys.Space) && newState.IsKeyDown(Keys.Space))
            {
                canGatherResources = true;
            }
            else
            {
                canGatherResources = false;
            }

            if (oldState.IsKeyUp(Keys.Q) && newState.IsKeyDown(Keys.Q))
            {
                if (docked == false)
                {
                    UIManager.ToggleMenu(Containers.PlayerMenu);
                }
                else
                {
                    UIManager.ToggleMenu(Containers.MothershipMenu);
                }
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

            ClampVelocity(5 * (healthStats[2] / 40 + 0.5f));
        }

        private void Dock()
        {
            if (EntityManager.DistanceSqr(this, mothership) < 1250)
            {
                Position = mothership.Position;
                Velocity = mothership.Velocity;
                AngularVelocity = mothership.AngularVelocity;
                if (docked == true)
                {
                    if (UIManager.GetContainer(Containers.MothershipMenu).Enabled == true)
                    {
                        UIManager.ToggleMenu(UIManager.GetContainer(Containers.MothershipMenu));
                    }
                    Move(mothership.Angle, 5);
                    leashedMaterials.Clear();
                    Engine.PlayGlobalSound(Assets.SoundFX["Undock"]);
                }
                if (docked == false)
                {
                    if (UIManager.GetContainer(Containers.PlayerMenu).Enabled == true)
                    {
                        UIManager.ToggleMenu(UIManager.GetContainer(Containers.PlayerMenu));
                    }
                    mothership.scrap += leashedMaterials.Count();
                    for (int i = 0; i < leashedMaterials.Count; i++)
                    {
                        EntityManager.Collide(leashedMaterials[i], mothership);
                    }
                    leashedMaterials.Clear();
                    Engine.PlayGlobalSound(Assets.SoundFX["Dock"]);
                    UIManager.GetContainer(Containers.MothershipMenu).Children[5].Text = mothership.scrap.ToString();
                }
                docked = !docked;
            }
        }
        private void Move(float angle, float speed)
        {
            Velocity += angle.ToUnitVector(0) * 10 * Engine.deltaSeconds * speed;
        }
        public void RepairShip()
        {
            if (mothership.scrap >= 5)
            {
                Engine.PlayGlobalSound(Assets.SoundFX["Interact"]);
                mothership.scrap -= 5;
                for (int i = 0; i < 3; i++)
                {
                    healthStats[random.Next(0, 5)]++;
                }
                UIManager.GetContainer(Containers.MothershipMenu).Children[5].Text = mothership.scrap.ToString();
            }
        }
        private void Basic()
        {
            EntityManager.Add(new PulseShot(Position, TargetVector * 8, Angle, 0, true));
            Engine.PlaySound(Assets.SoundFX["Fire_1"], Position);
            Cooldown = 0.25f / (healthStats[1]/40 + 0.5f);
        }
        private void Shotgun()
        {
            int randomBulletCount = random.Next(3, 6);
            for (int i = 0; i < randomBulletCount; i++)
            {
                float angleDegrees = (float)(random.NextDouble()-0.5)*10;
                float offsetAngle = angleDegrees * MathF.PI/180;
                Vector2 targetVector = (Angle + offsetAngle).ToUnitVector(0);
                EntityManager.Add(new PulseShot(Position, targetVector * 8, Angle+offsetAngle, 0, true));
            }
            Engine.PlaySound(Assets.SoundFX["Fire_1"], Position);
            Cooldown = 1f / (healthStats[1] / 40 + 0.5f);
        }
    }
}
