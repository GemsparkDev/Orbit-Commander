using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Space_Wars.Content.Main.UI_Elements;
using Space_Wars.Content.Main;
using Space_Wars.Content.Main.Particles;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Space_Wars.Content.Main.Entities
{
    public class Player : Entity
    {
        private Keys[] pressedKey;
        private Random random = new();
        private KeyboardState oldState;
        public Mothership mothership;
        private ParticleEmitter engineParticles = new(Assets.Sprites["Circle"], 0.15f, Vector2.Zero, 0, 45, 2, 0, 450f, 1, true, Color.Cyan, Color.DarkSlateBlue, EmitterType.EmissionOverTime);
        private ParticleEmitter smokeParticles = new(Assets.Sprites["Circle"], 1f, Vector2.Zero, 0, 45, 1, 0, 0.25f, 1, true, Color.Gray, Color.DarkGray, EmitterType.EmissionOverTime);
        private SoundEffectInstance engineSounds;
        private float cooldown = 0;
        private float invincibilityCooldown = 0;
        private float engineCooldown = 0;
        private float cachedDamage = 0;
        private Vector2 targetVector;
        public bool isDocked = true;
        public bool isEngineActive = false;
        public bool canGatherResources;
        public List<Item> leashedMaterials = new();
        delegate void PlayerDelegateMethod();
        PlayerDelegateMethod[,] moduleFunctions;
        public Module[] modules =
        {
            ItemFactory.NewBasicHullModule(Vector2.Zero, Vector2.Zero, 0),
            ItemFactory.NewBasicGunModule(Vector2.Zero, Vector2.Zero, 0),
            ItemFactory.NewBasicEngineModule(Vector2.Zero, Vector2.Zero, 0),
            ItemFactory.NewBasicSensorModule(Vector2.Zero, Vector2.Zero, 0),
            ItemFactory.NewBasicCoreModule(Vector2.Zero, Vector2.Zero, 0)
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
            damage = 5;
            ParticleManager.Add(engineParticles);
            ParticleManager.Add(smokeParticles);
            engineSounds = Assets.SoundFX["Fire Engines"].CreateInstance();
            engineSounds.IsLooped = true;
            SoundManager.AddSound(engineSounds);
            moduleFunctions = new PlayerDelegateMethod[3, 5]
            {
               { None, Basic, Dash, None, None },
               { None, Spiral, None, None, None },
               { None, Shotgun, None, None, None },
            };
        }
        public override void Update()
        {
            if (cachedDamage > 0)
            {
                for (int i = 0; i < cachedDamage; i++)
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
                cachedDamage = 0;
            }
            isEngineActive = false;
            float currentHealth = modules[0].health + modules[1].health + modules[2].health + modules[3].health + modules[4].health;
            if (currentHealth > 50)
            {
                smokeParticles.isEmitterActive = false;
            }
            else
            {
                smokeParticles.isEmitterActive = true;
                smokeParticles.speedOfEmission = (-currentHealth + 100) / 4;
            }
            if (modules[4].health <= 0)
            {
                isExpired = true;
                SoundManager.PauseSound(engineSounds);
                Assets.SoundFX["Death"].Play();
            }
            else if (modules[4].health > 0)
            {
                ControlShip();
            }
            engineParticles.position = position - new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * 8;
            smokeParticles.position = position;

            if (cooldown > 0)
            {
                cooldown -= Engine.deltaSeconds;
            }
            if (engineCooldown > 0)
            {
                engineCooldown -= Engine.deltaSeconds;
            }
            if (invincibilityCooldown > 0)
            {
                invincibilityCooldown -= Engine.deltaSeconds;
                color = new Color(0, 255, 0) * (MathF.Cos(invincibilityCooldown * 30) / 2 + 0.5f);
            }
            else
            {
                color = new Color(0, 255, 0);
            }
        }
        public override void Collide(int _damage)
        {
            if(_damage > 0)
            {
                if(invincibilityCooldown <= 0)
                {
                    cachedDamage += _damage;
                    SoundManager.PlaySound(Assets.SoundFX["Hit"], position);
                    invincibilityCooldown = 1;
                }
            }
        }
        public void ControlShip()
        {
            targetVector = Vector2.Normalize(new Vector2(Mouse.GetState().X, Mouse.GetState().Y) - Engine.screenSize/2);
            angle = MathF.Atan2(targetVector.X, -targetVector.Y) - Engine.camera.Rotation;
            if (Mouse.GetState().LeftButton == ButtonState.Pressed && cooldown <= 0 && UIManager.lockMouseInput == false && isDocked == false)
            {
                moduleFunctions[modules[1].ability, 1]();
            }

            pressedKey = Keyboard.GetState().GetPressedKeys();
            KeyboardState newState = Keyboard.GetState();

            if (isDocked == false)
            {
                for (int i = 0; i < pressedKey.Length; i++)
                {
                    float speedMultiplier = (1 / (velocity.Length() + 0.2f) + 1.5f) * (modules[2].health / 40 + 0.5f) / (leashedMaterials.Count + 1) / 10;
                    //float speedMultiplier = 0.05f;
                    switch (pressedKey[i])
                    {
                        case Keys.W:
                            isEngineActive = true;
                            engineParticles.sprayAngle = 180;
                            engineParticles.offsetVelocity = velocity;
                            Move(-Engine.camera.Rotation, speedMultiplier);
                            break;
                        case Keys.A:
                            isEngineActive = true;
                            engineParticles.sprayAngle = 90;
                            engineParticles.offsetVelocity = velocity;
                            Move(3 * MathF.PI / 2 - Engine.camera.Rotation, speedMultiplier);
                            break;
                        case Keys.S:
                            isEngineActive = true;
                            engineParticles.sprayAngle = 0;
                            engineParticles.offsetVelocity = velocity;
                            Move(MathF.PI - Engine.camera.Rotation, speedMultiplier);
                            break;
                        case Keys.D:
                            isEngineActive = true;
                            engineParticles.sprayAngle = 270;
                            engineParticles.offsetVelocity = velocity;
                            Move(MathF.PI / 2 - Engine.camera.Rotation, speedMultiplier);
                            break;
                        default:
                            break;
                    }
                }
                if (oldState.IsKeyUp(Keys.E) && newState.IsKeyDown(Keys.E))
                {
                    canGatherResources = true;
                    SoundManager.PlayGlobalSound(Assets.SoundFX["Open Menu"]);
                }
                else if (oldState.IsKeyDown(Keys.E) && newState.IsKeyUp(Keys.E))
                {
                    SoundManager.PlayGlobalSound(Assets.SoundFX["Close Menu"]);
                    canGatherResources = false;
                }
                if (oldState.IsKeyUp(Keys.Z) && newState.IsKeyDown(Keys.Z))
                {
                    leashedMaterials = new();
                }
                if (oldState.IsKeyUp(Keys.Q) && newState.IsKeyDown(Keys.Q))
                {
                    moduleFunctions[modules[2].ability, 2]();
                }
                if (newState.IsKeyDown(Keys.F))
                {
                    velocity *= (0.9f * Engine.deltaSeconds * 60);
                }
            }

            if (oldState.IsKeyUp(Keys.Space) && newState.IsKeyDown(Keys.Space))
            {
                Dock();
            }

            if (oldState.IsKeyUp(Keys.I) && newState.IsKeyDown(Keys.I))
            {
                if (isDocked == false)
                {
                    UIManager.ToggleMenu(Containers.PlayerMenu);
                }
                else
                {
                    UIManager.ToggleMenu(Containers.MothershipMenu);
                }
            }

            oldState = newState;

            if(isEngineActive == true)
            {
                engineParticles.isEmitterActive = true;
                SoundManager.PlaySound(engineSounds);
            }
            else
            {
                engineParticles.isEmitterActive = false;
                SoundManager.PauseSound(engineSounds);
            }
            if(position.Length() >= 2500)
            {
                velocity = Vector2.Normalize(-position);
                position = Vector2.Normalize(position) * 2500;
            }
            //Moves the player, but offsets the screen to keep the player in the middle
            position += velocity * Engine.deltaSeconds * 60;
            angle += angularVelocity * Engine.deltaSeconds * 60 ;
            angularVelocity = 0;

            if(isDocked == true)
            {
                position = mothership.position;
                velocity = mothership.velocity;
            }
        }

        private void Dock()
        {
            if (EntityManager.DistanceSqr(this, mothership) < 1250)
            {
                position = mothership.position;
                velocity = mothership.velocity;
                if (isDocked == true)
                {
                    if (UIManager.GetContainer(Containers.MothershipMenu).enabled == true)
                    {
                        UIManager.ToggleMenu(UIManager.GetContainer(Containers.MothershipMenu));
                    }
                    velocity += new Vector2(0, -2);
                    leashedMaterials.Clear();
                    SoundManager.PlayGlobalSound(Assets.SoundFX["Undock"]);
                }
                else if (isDocked == false)
                {
                    if (UIManager.GetContainer(Containers.PlayerMenu).enabled == true)
                    {
                        UIManager.ToggleMenu(UIManager.GetContainer(Containers.PlayerMenu));
                    }
                    for (int i = 0; i < leashedMaterials.Count; i++)
                    {
                        if (mothership.IsFull() == false)
                        {
                            mothership.AddItem(leashedMaterials[i]);
                            leashedMaterials[i].isExpired = true;
                        }
                        else
                        {
                            
                        }
                    }
                    leashedMaterials.Clear();
                    SoundManager.PlayGlobalSound(Assets.SoundFX["Dock"]);
                    UIManager.GetContainer(Containers.GarageMenu).GetWidget(0).text = mothership.scrap.ToString();
                }
                isDocked = !isDocked;
            }
        }
        public static void None()
        {

        }
        private void Move(float _angle, float _speed)
        {
            velocity += Engine.ToUnitVector(_angle) * 60 * Engine.deltaSeconds * _speed;
        }
        private void Basic()
        {
            EntityManager.Add(new PulseShot(position, targetVector * 4, angle, 0, true, damage));
            SoundManager.PlaySound(Assets.SoundFX["Fire_1"], position);
            cooldown = 0.5f / (modules[1].health / 40 + 0.5f);
        }
        private void Spiral()
        {
            EntityManager.Add(new SpiralShot(position, targetVector * 4, angle, 0, true, damage * 2, false));
            EntityManager.Add(new SpiralShot(position, targetVector * 4, angle, 0, true, damage * 2, true));
            SoundManager.PlaySound(Assets.SoundFX["Fire_1"], position);
            cooldown = 0.8f / (modules[1].health / 40 + 0.5f);
        }
        private void Shotgun()
        {
            int randomBulletCount = random.Next(3, 6);
            for (int i = 0; i < randomBulletCount; i++)
            {
                float angleDegrees = (float)(random.NextDouble() - 0.5) * 5;
                float offsetAngle = angleDegrees * MathF.PI / 180;
                Vector2 targetVector = Engine.ToUnitVector(angle + offsetAngle);
                EntityManager.Add(new PulseShot(position, targetVector * 6, angle + offsetAngle, 0, true, damage/2));
            }
            SoundManager.PlaySound(Assets.SoundFX["Fire_1"], position);
            cooldown = 1f / (modules[1].health / 40 + 0.5f);
        }
        private void Dash()
        {
            if(engineCooldown <= 0)
            {
                invincibilityCooldown = 0.5f;
                Vector2 normalVector = new(MathF.Sin(angle), -MathF.Cos(angle));
                position += normalVector * 400;
                engineCooldown = (-modules[2].health/2) + 30;
            }
        }
        public override void Draw(SpriteBatch _spriteBatch)
        {
            if(isDocked == false)
            {
                base.Draw(_spriteBatch);
                _spriteBatch.Draw(Engine.line, position - Engine.mousePositionOffset + new Vector2(-texture.Width * 2, texture.Height * 1.5f) / 2, new Rectangle(0, 0, texture.Width * 2, 2),
                    Color.DarkGray, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                _spriteBatch.Draw(Engine.line, position - Engine.mousePositionOffset + new Vector2(-texture.Width * 2, texture.Height * 1.5f) / 2, new Rectangle(0, 0, (int)(texture.Width * 2 * (1-engineCooldown/((-modules[2].health / 2) + 30))), 2),
                    Color.Yellow, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            }
            if ((position.X*position.X)/(Engine.screenSize.X * Engine.screenSize.X / 4) + (position.Y * position.Y) / (Engine.screenSize.Y * Engine.screenSize.Y / 4) >= 5)
            {
                _spriteBatch.Draw(Assets.Sprites["Arrow"], position - Engine.mousePositionOffset - Vector2.Normalize(position) * 25, null, color, MathF.Atan2(-position.X, position.Y), new Vector2(Assets.Sprites["Arrow"].Width, Assets.Sprites["Arrow"].Height)/2, 1, 0, 0.2f);
                _spriteBatch.DrawString(Assets.textFont, "Return to mothership.", Engine.camera.Position - new Vector2(105, 225), Color.Crimson);
            }
        }
    }
}
