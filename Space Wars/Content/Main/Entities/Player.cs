using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Space_Wars.Content.Main.Entities
{
    public class Player : Entity
    {
        private Keys[] pressedKey;
        private Random random = new();
        private KeyboardState oldState;
        public Mothership mothership;
        private float cooldown = 0;
        private Vector2 targetVector;
        public bool docked = false;
        public bool canGatherResources;
        public List<Projectile> leashedMaterials = new();
        private Projectile mothershipArrow;
        private float[] healthStats = { 20, 20, 20, 20, 20 };

        public Player(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity)
        {
            position = _position;
            velocity = _velocity;
            angle = _angle;
            angularVelocity = _angularVelocity;
            texture = Assets.Sprites["Player"];
            isFriendly = true;
            mothershipArrow = new MothershipArrow(position, velocity, angle, angularVelocity, true);
            EntityManager.Add(mothershipArrow);
        }
        public override void Update()
        {
            if (healthStats[4] <= 0)
            {
                isExpired = true;
                Assets.SoundFX["Death"].Play();
            }
            if (healthStats[4] > 0)
            {
                ControlShip();
            }

            mothershipArrow.angle = MathF.Atan2(mothership.position.X - position.X, -(mothership.position.Y - position.Y));
            mothershipArrow.position = position + Engine.ToUnitVector(mothershipArrow.angle) * ColliderRadius * 1.5f;
            for (int i = 0; i < 4; i++)
            {
                UIManager.GetContainer(Containers.PlayerMenu).children[i].text = healthStats[i].ToString();
                if (healthStats[i] > 10)
                {
                    UIManager.GetContainer(Containers.PlayerMenu).children[i].textColor = Color.White;
                }
                else if (healthStats[i] <= 0)
                {
                    UIManager.GetContainer(Containers.PlayerMenu).children[i].textColor = Color.Black;
                }
                else
                {
                    UIManager.GetContainer(Containers.PlayerMenu).children[i].textColor = Color.Red;
                }
            }
            UIManager.GetContainer(Containers.PlayerMenu).children[4].text = healthStats[4].ToString();
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
                Engine.PlaySound(Assets.SoundFX["Hit"], position);
            }
        }
        public void ControlShip()
        {
            targetVector = Vector2.Normalize(new Vector2(Mouse.GetState().X, Mouse.GetState().Y) - Engine.screenPosition - position);
            angle = MathF.Atan2(targetVector.X, -targetVector.Y);
            if (Mouse.GetState().LeftButton == ButtonState.Pressed && cooldown <= 0)
            {
                Shotgun();
            }

            pressedKey = Keyboard.GetState().GetPressedKeys();
            KeyboardState newState = Keyboard.GetState();

            for (int i = 0; i < pressedKey.Length; i++)
            {
                if (docked == false)
                {
                    float playerSpeedScalar = MathF.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y);
                    float speedMultiplier = (1 / (playerSpeedScalar + 0.2f) + 1) * (healthStats[2] / 40 + 0.5f) / (leashedMaterials.Count + 1);
                    switch (pressedKey[i])
                    {
                        case Keys.W:
                            Move(0, speedMultiplier);
                            break;
                        case Keys.S:
                            Move(MathF.PI, speedMultiplier);
                            break;
                        case Keys.A:
                            Move(3 * MathF.PI / 2, speedMultiplier);
                            break;
                        case Keys.D:
                            Move(MathF.PI / 2, speedMultiplier);
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
                Engine.PlayGlobalSound(Assets.SoundFX["Open Menu"]);
            }
            else if (oldState.IsKeyDown(Keys.Space) && newState.IsKeyUp(Keys.Space))
            {
                Engine.PlayGlobalSound(Assets.SoundFX["Close Menu"]);
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
            position += velocity;
            angle += angularVelocity;
            angularVelocity = 0;

            if (cooldown > 0)
            {
                cooldown -= Engine.deltaSeconds;
            }

            ClampVelocity(5 * (healthStats[2] / 40 + 0.5f));
        }

        private void Dock()
        {
            if (EntityManager.DistanceSqr(this, mothership) < 1250)
            {
                position = mothership.position;
                velocity = mothership.velocity;
                angularVelocity = mothership.angularVelocity;
                if (docked == true)
                {
                    if (UIManager.GetContainer(Containers.MothershipMenu).enabled == true)
                    {
                        UIManager.ToggleMenu(UIManager.GetContainer(Containers.MothershipMenu));
                    }
                    Move(mothership.angle, 5);
                    leashedMaterials.Clear();
                    Engine.PlayGlobalSound(Assets.SoundFX["Undock"]);
                }
                if (docked == false)
                {
                    if (UIManager.GetContainer(Containers.PlayerMenu).enabled == true)
                    {
                        UIManager.ToggleMenu(UIManager.GetContainer(Containers.PlayerMenu));
                    }
                    mothership.scrap += leashedMaterials.Count;
                    for (int i = 0; i < leashedMaterials.Count; i++)
                    {
                        EntityManager.Collide(leashedMaterials[i], mothership);
                    }
                    leashedMaterials.Clear();
                    Engine.PlayGlobalSound(Assets.SoundFX["Dock"]);
                    UIManager.GetContainer(Containers.MothershipMenu).children[5].text = mothership.scrap.ToString();
                }
                docked = !docked;
            }
        }
        private void Move(float _angle, float _speed)
        {
            velocity += Engine.ToUnitVector(_angle) * 10 * Engine.deltaSeconds * _speed;
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
                UIManager.GetContainer(Containers.MothershipMenu).children[5].text = mothership.scrap.ToString();
            }
        }
        private void Basic()
        {
            EntityManager.Add(new PulseShot(position, targetVector * 8, angle, 0, true));
            Engine.PlaySound(Assets.SoundFX["Fire_1"], position);
            cooldown = 0.25f / (healthStats[1] / 40 + 0.5f);
        }
        private void Shotgun()
        {
            int randomBulletCount = random.Next(3, 6);
            for (int i = 0; i < randomBulletCount; i++)
            {
                float angleDegrees = (float)(random.NextDouble() - 0.5) * 10;
                float offsetAngle = angleDegrees * MathF.PI / 180;
                Vector2 targetVector = Engine.ToUnitVector(angle + offsetAngle);
                EntityManager.Add(new PulseShot(position, targetVector * 8, angle + offsetAngle, 0, true));
            }
            Engine.PlaySound(Assets.SoundFX["Fire_1"], position);
            cooldown = 1f / (healthStats[1] / 40 + 0.5f);
        }
    }
}
