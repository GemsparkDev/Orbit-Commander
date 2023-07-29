using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Space_Wars.Content.Main.UI_Elements;
using System.Runtime.InteropServices;
using Space_Wars.Content.Main;

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
        //private Projectile mothershipArrow;
        public Module[] modules = {
            new Module(20, new float[] { 1 }, Assets.Sprites["Hull Module"], "Hull", 1), 
            new Module(20, new float[] { 1 }, Assets.Sprites["Gun Module"], "Guns", 2), 
            new Module(20, new float[] { 1 }, Assets.Sprites["Engine Module"], "Engines", 3), 
            new Module(20, new float[] { 1 }, Assets.Sprites["Sensor Module"], "Sensors", 4), 
            new Module(20, new float[] { 1 }, Assets.Sprites["Core Module"], "Core", 5) 
        };

        public Player(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity)
        {
            position = _position;
            velocity = _velocity;
            angle = _angle;
            angularVelocity = _angularVelocity;
            texture = Assets.Sprites["Player"];
            isFriendly = true;
            color = new Color(0, 255, 0);
            //mothershipArrow = new MothershipArrow(position, velocity, angle, angularVelocity, true);
            //EntityManager.Add(mothershipArrow);
            damage = 5;
        }
        public override void Update()
        {
            if (modules[4].health <= 0)
            {
                isExpired = true;
                Assets.SoundFX["Death"].Play();
            }
            if (modules[4].health > 0)
            {
                ControlShip();
            }

            //mothershipArrow.angle = MathF.Atan2(mothership.position.X - position.X, -(mothership.position.Y - position.Y));
            //mothershipArrow.position = position + Engine.ToUnitVector(mothershipArrow.angle) * ColliderRadius * 1.5f;
        }
        public override void Collide(int damage)
        {
            if (damage > 0)
            {
                for (int i = 0; i < damage; i++)
                {
                    if (random.NextDouble() <= modules[0].health / 20)
                    {
                        modules[0].health--;
                    }
                    else
                    {
                        int randomNumber = random.Next(1, 4);
                        if (modules[randomNumber].health > 0)
                        {
                            modules[randomNumber].health--;
                        }
                        else if (modules[0].health > 0)
                        {
                            modules[0].health--;
                        }
                        else
                        {
                            modules[4].health--;
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
            if (Mouse.GetState().LeftButton == ButtonState.Pressed && cooldown <= 0 && UIManager.lockMouseInput == false)
            {
                Basic();
            }

            pressedKey = Keyboard.GetState().GetPressedKeys();
            KeyboardState newState = Keyboard.GetState();

            for (int i = 0; i < pressedKey.Length; i++)
            {
                if (docked == false)
                {
                    float playerSpeedScalar = MathF.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y);
                    float speedMultiplier = (1 / (playerSpeedScalar + 0.2f) + 1) * (modules[2].health / 40 + 0.5f) / (leashedMaterials.Count + 1);
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
            position += velocity * Engine.deltaSeconds * 60;
            angle += angularVelocity * Engine.deltaSeconds * 60 ;
            angularVelocity = 0;

            if (cooldown > 0)
            {
                cooldown -= Engine.deltaSeconds;
            }

            ClampVelocity(5 * (modules[2].health / 40 + 0.5f));
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
                    Move(mothership.angle, 5 / (Engine.deltaSeconds*60));
                    leashedMaterials.Clear();
                    Engine.PlayGlobalSound(Assets.SoundFX["Undock"]);
                }
                if (docked == false)
                {
                    if (UIManager.GetContainer(Containers.PlayerMenu).enabled == true)
                    {
                        UIManager.ToggleMenu(UIManager.GetContainer(Containers.PlayerMenu));
                    }
                    for (int i = 0; i < leashedMaterials.Count; i++)
                    {
                        if (mothership.IsFull() == false)
                        {
                            mothership.AddItem(ItemFactory.NewScrap());
                            EntityManager.Collide(leashedMaterials[i], mothership);
                        }
                        else
                        {
                            
                        }
                    }
                    leashedMaterials.Clear();
                    Engine.PlayGlobalSound(Assets.SoundFX["Dock"]);
                    UIManager.GetContainer(Containers.MothershipMenu).GetWidget(0).text = mothership.scrap.ToString();
                }
                docked = !docked;
            }
        }
        private void Move(float _angle, float _speed)
        {
            velocity += Engine.ToUnitVector(_angle) * 10 * Engine.deltaSeconds * _speed;
        }
        private void Basic()
        {
            EntityManager.Add(new PulseShot(position, targetVector * 4, angle, 0, true, damage));
            Engine.PlaySound(Assets.SoundFX["Fire_1"], position);
            cooldown = 0.5f / (modules[1].health / 40 + 0.5f);
        }
        private void Spiral()
        {
            EntityManager.Add(new SpiralShot(position, targetVector * 4, angle, 0, true, damage * 2, false));
            EntityManager.Add(new SpiralShot(position, targetVector * 4, angle, 0, true, damage * 2, true));
            Engine.PlaySound(Assets.SoundFX["Fire_1"], position);
            cooldown = 0.8f / (modules[1].health / 40 + 0.5f);
        }
        private void Shotgun()
        {
            int randomBulletCount = random.Next(3, 6);
            for (int i = 0; i < randomBulletCount; i++)
            {
                float angleDegrees = (float)(random.NextDouble() - 0.5) * 10;
                float offsetAngle = angleDegrees * MathF.PI / 180;
                Vector2 targetVector = Engine.ToUnitVector(angle + offsetAngle);
                EntityManager.Add(new PulseShot(position, targetVector * 8, angle + offsetAngle, 0, true, damage));
            }
            Engine.PlaySound(Assets.SoundFX["Fire_1"], position);
            cooldown = 1f / (modules[1].health / 40 + 0.5f);
        }
    }
}
