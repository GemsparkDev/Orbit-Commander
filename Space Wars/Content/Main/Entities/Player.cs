using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Space_Wars.Content.Main.UI_Elements;
using Space_Wars.Content.Main;
using Space_Wars.Content.Main.Particles;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.Diagnostics;

namespace Space_Wars.Content.Main.Entities
{
    public class Player : Entity
    {
        private Keys[] pressedKey;
        private Random random = new();
        private KeyboardState oldState;
        private MouseState oldMouseState;
        public Mothership mothership;
        private ParticleEmitter engineParticles = new(Assets.Get(Sprite.Circle), 0.15f, Vector2.Zero, 0, 45, 2, 0, 450f, 1, true, Color.Cyan, Color.DarkSlateBlue, EmitterType.EmissionOverTime);
        //private ParticleEmitter engineParticles = new(Assets.Sprites["Circle"], 0.15f, Vector2.Zero, 0, 45, 2, 0, 450f, 1, true, Color.Orange, Color.Crimson, EmitterType.EmissionOverTime);
        private ParticleEmitter smokeParticles = new(Assets.Get(Sprite.Circle), 1f, Vector2.Zero, 0, 45, 1, 0, 0.25f, 1, true, Color.Gray, Color.DarkGray, EmitterType.EmissionOverTime);
        private SoundEffectInstance engineSounds;
        private float cooldown = 0;
        private float invincibilityCooldown = 0;
        private float engineCooldown = 0;
        private float energyCooldown = 0;
        private float cachedDamage = 0;
        private float gunAngle = 0;
        private float energy = 100;
        private float maxEnergy = 100;
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
            texture = Assets.Get(Sprite.Player);
            isFriendly = true;
            color = new Color(0, 255, 0);
            damage = 5;
            ParticleManager.Add(engineParticles);
            ParticleManager.Add(smokeParticles);
            engineSounds = Assets.Get(Sound.FireEngines).CreateInstance();
            engineSounds.IsLooped = true;
            SoundManager.AddSound(engineSounds);
            moduleFunctions = new PlayerDelegateMethod[3, 5]
            {
               { Hull, Basic, Dash, None, None },
               { Shield, Spiral, None, None, None },
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
                        moduleFunctions[modules[0].ability, 0]();
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
                Assets.Get(Sound.Death).Play();
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
            if(energyCooldown > 0)
            {
                energyCooldown -= Engine.deltaSeconds;
            }
            else
            {
                if(energy < maxEnergy)
                {
                    energy += Engine.deltaSeconds * 60;
                }
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
            if(_damage > 0 && invincibilityCooldown <= 0)
            {
                cachedDamage += _damage;
                SoundManager.PlaySound(Assets.Get(Sound.Hit), position);
                invincibilityCooldown = 1;
            }
        }
        public void ControlShip()
        {
            if (position.Length() >= 2500)
            {
                velocity *= 0.8f;
                velocity += Vector2.Normalize(-position) * Engine.deltaSeconds * 15;
            }
            position += velocity * Engine.deltaSeconds * 60;

            if (isDocked == true)
            {
                position = mothership.position;
                velocity = mothership.velocity;
            }
            MouseState newMouseState = Mouse.GetState();
            targetVector = Vector2.Normalize(new Vector2(Mouse.GetState().X, Mouse.GetState().Y) - Engine.screenSize/2);
            gunAngle = MathF.Atan2(targetVector.X, -targetVector.Y) - Engine.camera.Rotation;
            if (newMouseState.LeftButton == ButtonState.Pressed && cooldown <= 0 && UIManager.lockMouseInput == false && isDocked == false)
            {
                moduleFunctions[modules[1].ability, 1]();
            }
            if (newMouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released && UIManager.lockMouseInput == false && isDocked == false)
            {
                canGatherResources = true;
                SoundManager.PlayGlobalSound(Assets.Get(Sound.OpenMenu));
            }
            else if(newMouseState.RightButton == ButtonState.Released && oldMouseState.RightButton == ButtonState.Pressed && UIManager.lockMouseInput == false && isDocked == false)
            {
                SoundManager.PlayGlobalSound(Assets.Get(Sound.CloseMenu));
                canGatherResources = false;
            }
            oldMouseState = newMouseState;

            pressedKey = Keyboard.GetState().GetPressedKeys();
            KeyboardState newState = Keyboard.GetState();
            Vector2 direction = Vector2.Zero;
            if (isDocked == false)
            {
                for (int i = 0; i < pressedKey.Length; i++)
                {
                    float speed = 0.2f;
                    isEngineActive = true;
                    switch (pressedKey[i])
                    {
                        case Keys.W:
                            engineParticles.sprayAngle = 180;
                            engineParticles.offsetVelocity = velocity;
                            direction += new Vector2(0, -1);
                            break;
                        case Keys.A:
                            engineParticles.sprayAngle = 90;
                            engineParticles.offsetVelocity = velocity;
                            direction += new Vector2(-1, 0);
                            break;
                        case Keys.S:
                            engineParticles.sprayAngle = 0;
                            engineParticles.offsetVelocity = velocity;
                            direction += new Vector2(0, 1);
                            break;
                        case Keys.D:
                            engineParticles.sprayAngle = 270;
                            engineParticles.offsetVelocity = velocity;
                            direction += new Vector2(1, 0);
                            break;
                        default:
                            speed = 0f;
                            isEngineActive = false;
                            break;
                    }
                    angle = MathF.Atan2(direction.X, -direction.Y);
                    Move(angle - Engine.camera.Rotation, speed);
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
                    velocity *= Engine.deltaSeconds * 60 * 0.9f;
                }
            }

            if (isEngineActive == true)
            {
                engineParticles.isEmitterActive = true;
                SoundManager.PlaySound(engineSounds);
            }
            else
            {
                engineParticles.isEmitterActive = false;
                SoundManager.PauseSound(engineSounds);
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
                    SoundManager.PlayGlobalSound(Assets.Get(Sound.Undock));
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
                    SoundManager.PlayGlobalSound(Assets.Get(Sound.Dock));
                    UIManager.GetContainer(Containers.GarageMenu).GetWidget(0).text = mothership.scrap.ToString();
                }
                isDocked = !isDocked;
            }
        }
        private bool UseEnergy(float _energy)
        {
            if(energy > _energy)
            {
                energy -= _energy;
                energyCooldown = 0.5f;
                return true;
            }
            return false;
        }
        public static void None()
        {
            return;
        }
        private void Move(float _angle, float _speed)
        {
            velocity += Engine.ToUnitVector(_angle) * 60 * Engine.deltaSeconds * _speed;
        }
        private void Hull()
        {
            if(random.Next(0, 2) > 0)
            {
                modules[0].health--;
            }
        }
        private void Shield()
        {
            for(int i = 0; i < 2; i++)
            {
                if (UseEnergy(4) == false && modules[0].health > 0)
                {
                    modules[0].health--;
                }
            }
        }
        private void Basic()
        {
            if(UseEnergy(1) == false)
            {
                return;
            }
            EntityManager.Add(new PulseShot(position, velocity + targetVector * 9, gunAngle, 0, true, 3, true));
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
            cooldown = 0.3f / (modules[1].health / 80 + 0.75f);
        }
        private void Spiral()
        {
            if (UseEnergy(10) == false)
            {
                return;
            }
            EntityManager.Add(new SpiralShot(position, velocity + targetVector * 6, gunAngle, 0, true, 8, false));
            EntityManager.Add(new SpiralShot(position, velocity + targetVector * 6, gunAngle, 0, true, 8, true));
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
            cooldown = 1.3f - (modules[1].health / 40);
            //0.8s to 1.3s
        }
        private void Shotgun()
        {
            if (UseEnergy(8) == false)
            {
                return;
            }
            int randomBulletCount = random.Next(4, 6);
            for (int i = 0; i < randomBulletCount; i++)
            {
                float angleDegrees = (float)(random.NextDouble() - 0.5) * 5;
                float offsetAngle = angleDegrees * MathF.PI / 180;
                Vector2 targetVector = Engine.ToUnitVector(gunAngle + offsetAngle);
                EntityManager.Add(new PulseShot(position, velocity +targetVector * 6, gunAngle + offsetAngle, 0, true, 1));
            }
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
            cooldown = 1f - (modules[1].health / 40);
            // 1s to 1.5s
        }
        private void Dash()
        {
            if(engineCooldown <= 0)
            {
                if(UseEnergy(25) == false)
                {
                    return;
                }
                invincibilityCooldown = 0.5f;
                Vector2 normalVector = new(MathF.Sin(gunAngle), -MathF.Cos(gunAngle));
                for (int i = 0; i < 200; i++)
                {
                    float timeLeft = ((float)i / 200);
                    ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), timeLeft, position + normalVector*i, velocity * timeLeft, gunAngle, 0, 1, true, Color.Cyan, Color.SlateBlue));
                }
                position += normalVector * 200;
                engineCooldown =  3 - (modules[2].health / 13.33f);
            }
        }
        public override void Draw(SpriteBatch _spriteBatch)
        {
            if(isDocked == false)
            {
                base.Draw(_spriteBatch);
                //_spriteBatch.Draw(gunTexture, position - Engine.mousePositionOffset - new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * 3, null, Color.White, gunAngle, new Vector2(gunTexture.Width, 3 * gunTexture.Height/2) / 2, 1, SpriteEffects.None, 0);
                _spriteBatch.Draw(Engine.line, position - Engine.mousePositionOffset + new Vector2(-texture.Width * 2, texture.Height * 1.5f) / 2, new Rectangle(0, 0, texture.Width * 2, 2),
                    Color.DarkGray, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                _spriteBatch.Draw(Engine.line, position - Engine.mousePositionOffset + new Vector2(-texture.Width * 2, texture.Height * 1.5f) / 2, new Rectangle(0, 0, (int)(texture.Width * 2 * (1-engineCooldown / (3 - (modules[2].health / 13.33f)))), 2),
                    Color.Cyan, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                _spriteBatch.Draw(Engine.line, position - Engine.mousePositionOffset + new Vector2(-texture.Width, texture.Height), new Rectangle(0, 0, texture.Width * 2, 2),
                    Color.DarkGray, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                _spriteBatch.Draw(Engine.line, position - Engine.mousePositionOffset + new Vector2(-texture.Width, texture.Height), new Rectangle(0, 0, (int)(texture.Width * 2 * (energy / maxEnergy)), 2),
                    Color.Yellow, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            }
            if ((position.X*position.X)/(Engine.screenSize.X * Engine.screenSize.X / 4) + (position.Y * position.Y) / (Engine.screenSize.Y * Engine.screenSize.Y / 4) >= 5)
            {
                _spriteBatch.Draw(Assets.Get(Sprite.Arrow), position - Engine.mousePositionOffset - Vector2.Normalize(position) * 25, null, color, MathF.Atan2(-position.X, position.Y), Assets.DimsOf(Sprite.Arrow)/2, 1, 0, 0.2f);
                _spriteBatch.DrawString(Assets.textFont, "Return to mothership.", Engine.camera.Position - new Vector2(105, 225), Color.Crimson);
            }
        }
    }
}
