using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Space_Wars.Content.Main.Particles;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.Linq;
using UILib.Content.Main;
using Space_Wars.Content.Main.Components;
using System.Diagnostics;
using System.ComponentModel;

namespace Space_Wars.Content.Main.Entities;

public class Player : Entity
{
    private Keys[] pressedKey;
    private Random random = new();
    private MouseState oldMouseState;
    public DockableComponent dockedEntity;
    private Enemy shield;
    private ParticleEmitter engineParticles = new(Assets.Get(Sprite.Circle), 0.15f, Vector2.Zero, 0, 45, 2, 0, 450f, 1, true, Color.Cyan, Color.DarkSlateBlue, EmitterType.EmissionOverTime) { isEmitterActive = false };
    //private ParticleEmitter engineParticles = new(Assets.Sprites["Circle"], 0.15f, Vector2.Zero, 0, 45, 2, 0, 450f, 1, true, Color.Orange, Color.Crimson, EmitterType.EmissionOverTime);
    private ParticleEmitter smokeParticles = new(Assets.Get(Sprite.Circle), 1f, Vector2.Zero, 0, 45, 1, 0, 0.25f, 1, true, Color.Gray, Color.DarkGray, EmitterType.EmissionOverTime) { isEmitterActive = false };
    private SoundEffectInstance engineSounds;
    private float invincibilityCooldown = 0;
    private float cachedDamage = 0;
    private float restartCooldown = 0;
    private bool isRestarting = false;
    private Entity gunAngle;
    private Vector2 targetVector;
    private Vector2 direction;
    public bool isEngineActive = false;
    public bool canGatherResources = false;
    public override int SensingAbility { get { return GetSensingAbility(); } }
    public override int StealthAbility { get { return GetStealthAbility(); } }
    public List<Pickup> leashedMaterials = new();
    Action[][] moduleFunctions;
    public Dictionary<ModuleType, Module> modules;

    public Player(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity)
    {
        position = _position;
        velocity = _velocity;
        gunAngle = Enemy.NewDummyEnemy(position, true);
        shield = Enemy.NewShield(gunAngle, 10, 1, 0, 1, true);
        shield.isExpired = true;
        angle = _angle;
        angularVelocity = _angularVelocity;
        texture = Assets.Get(Sprite.Player);
        isFriendly = true;
        color = new Color(0, 255, 0);
        damage = 5;
        smokeParticles.isEmitterActive = false; 
        engineParticles.isEmitterActive = false;
        ParticleManager.Add(engineParticles);
        ParticleManager.Add(smokeParticles);
        engineSounds = Assets.Get(Sound.FireEngines).CreateInstance();
        engineSounds.IsLooped = true;
        SoundManager.AddSound(engineSounds);
        moduleFunctions = new Action[3][]
        {
           new Action[2] { Hull, Shield },
           new Action[7] { Basic, Spiral, Shotgun, Missile, LMG, Silenced, Sniper },
           new Action[2] { Dash, SummonShield },
        };

        modules = new()
        {
            { ModuleType.Hull, ItemFactory.GetItem(ModuleType.Hull) },
            { ModuleType.Guns, ItemFactory.GetItem(ModuleType.Sniper) },
            { ModuleType.Engines, ItemFactory.GetItem(ModuleType.Engines) },
            { ModuleType.Sensors, ItemFactory.GetItem(ModuleType.Sensors) },
            { ModuleType.Core, ItemFactory.GetItem(ModuleType.Core) }
        };
    }
    public override void Update()
    {
        leashedMaterials = leashedMaterials.Where(x => !x.isExpired).ToList();
        if (EventHandler.AcknowledgeMessage(Message.RestartModules))
        {
            foreach (var module in modules.Values)
            {
                if (module.isFailed)
                {
                    restartCooldown += 1f;
                    break;
                }
            }
            if (restartCooldown > 0)
            {
                isRestarting = true;
                SoundManager.PlaySound(Assets.Get(Sound.Interact), position);
            }
        }
        if (isRestarting && restartCooldown <= 0)
        {
            bool restartedModules = false;
            foreach (var module in modules.Values)
            {
                if (restartedModules && module.isFailed)
                {
                    restartCooldown = 1;
                    restartedModules = false;
                    break;
                }
                else if (module.isFailed)
                {
                    module.isFailed = false;
                    restartedModules = true;
                    EventHandler.UpdateModulesStatus();
                }
            }
            if (restartedModules)
            {
                isRestarting = false;
                SoundManager.PlaySound(Assets.Get(Sound.Full), position);
                EventHandler.UpdateModulesStatus();
            }
        }
        else if (isRestarting)
        {
            restartCooldown -= Engine.DeltaSeconds;
            EventHandler.UpdateRestartSlider(1 - restartCooldown, 1f);
        }
        if (invincibilityCooldown > 0)
        {
            invincibilityCooldown -= Engine.DeltaSeconds;
            color = invincibilityCooldown > 0 ? new Color(0, 255, 0) * (MathF.Cos(invincibilityCooldown * 30) / 2 + 0.5f) : new Color(0, 255, 0);
        }
        //if cachedDamage > 0
        if (false)
        {
            if (!modules[ModuleType.Hull].isFailed)
            {
                moduleFunctions[0][modules[ModuleType.Hull].AbilityID]();
            }
            for (int i = 0; i < cachedDamage; i++)
            {
                int randomNumber = random.Next(1, 4);
                if (modules[ModuleType.Hull].Health > 0)
                {
                    modules[ModuleType.Hull].Health--;
                }
                else if (modules.ElementAt(randomNumber).Value.Health > 0)
                {
                    modules.ElementAt(randomNumber).Value.Health--;
                }
                else
                {
                    modules[ModuleType.Core].Health--;
                }
            }
            foreach (var module in modules)
            {
                module.Value.UpdateHealth();
            }
            cachedDamage = 0;
        }
        isEngineActive = false;
        float currentHealth = modules[ModuleType.Hull].Health + modules[ModuleType.Guns].Health + modules[ModuleType.Engines].Health + modules[ModuleType.Sensors].Health + modules[ModuleType.Core].Health;
        if (currentHealth > 50)
        {
            smokeParticles.isEmitterActive = false;
        }
        else
        {
            smokeParticles.isEmitterActive = true;
            smokeParticles.speedOfEmission = (-currentHealth + 100) / 4;
        }
        isEngineActive = false;
        if (modules[ModuleType.Core].Health <= 0)
        {
            isExpired = true;
            SoundManager.PauseSound(engineSounds);
            Assets.Get(Sound.Death).Play();
        }
        else if (modules[ModuleType.Core].Health > 0)
        {
            if (position.Length() >= 40*50 + EntityManager.CurrentMission.Planet.radius)
            {
                velocity *= 0.8f;
                velocity += Vector2.Normalize(-position) * Engine.DeltaSeconds * (position.Length() - (40 * 50 + EntityManager.CurrentMission.Planet.radius));
            }
            position += velocity * Engine.DeltaSeconds * 60;
            ControlShip();
        }
        engineParticles.position = position - new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * 8;
        smokeParticles.position = position;
        foreach(var module in modules.Values)
        {
            module.UpdateCooldown();
        }
        if (isEngineActive)
        {
            engineParticles.isEmitterActive = true;
            SoundManager.PlaySound(engineSounds);
        }
        else
        {
            engineParticles.isEmitterActive = false;
            SoundManager.PauseSound(engineSounds);
        }
        gunAngle.position = position;
    }
    public override void Collide(int _damage)
    {
        if(_damage > 0 && invincibilityCooldown <= 0)
        {
            Engine.ShakeScreen(0.08f * _damage);
            cachedDamage += _damage;
            SoundManager.PlaySound(Assets.Get(Sound.Hit), position);
            invincibilityCooldown = 1;
            ParticleManager.Add(new Particle(null, 1, position + new Vector2(0, -1), new Vector2(0, -1.5f), 0, 0, 1, true, Color.Red, Color.Red) { drawText = $"{_damage}" });
            //Part Failure
            if (random.Next(0, 5) == 0)
            {
                ModuleType failedPart = (ModuleType)random.Next(0, 4);
                if (modules[failedPart].Health < modules[failedPart].MaxHealth / 2 && !modules[failedPart].isFailed)
                {
                    modules[failedPart].isFailed = true;
                    ParticleManager.Add(new Particle(null, 2, position + new Vector2(0, -3), new Vector2(0, -0.75f), 0, 0, 1, true, Color.Red, Color.Red) { drawText = $"{failedPart} has failed!" });
                    SoundManager.PlaySound(Assets.Get(Sound.Beep), position);
                    EventHandler.UpdateModulesStatus();
                }
            }
        }
    }
    public void ControlShip()
    {
        pressedKey = Keyboard.GetState().GetPressedKeys();
        if (Input.OldState.IsKeyUp(Keys.Space) && Input.NewState.IsKeyDown(Keys.Space))
        {
            if (dockedEntity == null)
            {
                DockableComponent dockableEntity = EntityManager.NearestDockableEntity(this);
                if (dockableEntity != null)
                {
                    if (dockableEntity.Dock(this))
                    {
                        dockedEntity = dockableEntity;
                    }
                }
            }
            else if(dockedEntity.Dock(this))
            {
                dockedEntity = null;
            }
        }
        if (Input.OldState.IsKeyUp(Keys.I) && Input.NewState.IsKeyDown(Keys.I))
        {
            EventHandler.ToggleDockingMenus();
        }
        if(EventHandler.AcknowledgeMessage(Message.ToggleTerminal))
        {
            if (dockedEntity != null)
            {
                Engine.UIManager.ToggleMenu((int)Containers.MothershipMenu);
            }
            else
            {
                Engine.UIManager.ToggleMenu((int)Containers.PlayerMenu);
            }
        }
        if (dockedEntity != null)
        {
            position = dockedEntity.Position;
            velocity = dockedEntity.Velocity;
            return;
        }
        MouseState newMouseState = Mouse.GetState();
        targetVector = Vector2.Normalize(new Vector2(Mouse.GetState().X, Mouse.GetState().Y) - Engine.ScreenSize / 2 - position + Engine.Camera.Position);
        gunAngle.angle = MathF.Atan2(targetVector.X, -targetVector.Y) - Engine.Camera.Rotation;
        if (newMouseState.LeftButton == ButtonState.Pressed && modules[ModuleType.Guns].IsCooldownReady() && UIManager.LockMouseInput == false && !modules[ModuleType.Guns].isFailed)
        {
            moduleFunctions[1][modules[ModuleType.Guns].AbilityID]();
        }
        if (newMouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released && UIManager.LockMouseInput == false)
        {
            canGatherResources = true;
            SoundManager.PlayGlobalSound(Assets.Get(Sound.OpenMenu));
        }
        else if (newMouseState.RightButton == ButtonState.Released && oldMouseState.RightButton == ButtonState.Pressed && UIManager.LockMouseInput == false)
        {
            SoundManager.PlayGlobalSound(Assets.Get(Sound.CloseMenu));
            canGatherResources = false;
        }
        oldMouseState = newMouseState;

        direction = Vector2.Zero;
        if (!modules[ModuleType.Engines].isFailed)
        {
            for (int i = 0; i < pressedKey.Length; i++)
            {
                switch (pressedKey[i])
                {
                    case Keys.W:
                        direction += new Vector2(0, -1);
                        isEngineActive = true;
                        break;
                    case Keys.A:
                        direction += new Vector2(-1, 0);
                        isEngineActive = true;
                        break;
                    case Keys.S:
                        direction += new Vector2(0, 1);
                        isEngineActive = true;
                        break;
                    case Keys.D:
                        direction += new Vector2(1, 0);
                        isEngineActive = true;
                        break;
                    default:
                        break;
                }
            }
            if (direction != Vector2.Zero)
            {
                float speed = 0.2f;
                engineParticles.offsetVelocity = velocity;
                angle = (angle * 0.5f + MathF.Atan2(direction.X, -direction.Y) * 0.5f);
                engineParticles.sprayAngle = angle * 180 / MathF.PI + 180;
                velocity += Engine.ToUnitVector(angle) * 60 * Engine.DeltaSeconds * speed * 2 / (leashedMaterials.Count + 2);
            }
        }
        if (Input.OldState.IsKeyUp(Keys.Z) && Input.NewState.IsKeyDown(Keys.Z))
        {
            leashedMaterials = new();
        }
        if (Input.OldState.IsKeyUp(Keys.Q) && Input.NewState.IsKeyDown(Keys.Q) && modules[ModuleType.Engines].IsCooldownReady() && !modules[ModuleType.Engines].isFailed)
        {
            moduleFunctions[2][modules[ModuleType.Engines].AbilityID]();
        }
    }
    private void Hull()
    {
        cachedDamage /= 2;
    }
    private void Shield()
    {
        if(modules[0].cooldown <= 0)
        {
            modules[0].cooldown = 8;
            cachedDamage = 0;
            return;
        }
        modules[0].Health = MathF.Max(0, modules[0].Health - cachedDamage / 2);
    }
    private void Basic()
    {
        EntityManager.Add(new PulseShot(position, targetVector * 9 + velocity, gunAngle.angle, 0, true, 3, true));
        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
        modules[ModuleType.Guns].cooldown = 0.25f;
        Engine.ShakeScreen(0.2f);
        velocity -= targetVector / 4;
    }
    private void Sniper()
    {
        EntityManager.Add(new AssassinShot(position, targetVector * 100, gunAngle.angle, 0, true, 10));
        SoundManager.PlaySound(Assets.Get(Sound.SniperFire), position);
        modules[ModuleType.Guns].cooldown = 2f;
        Engine.ShakeScreen(0.3f);
        velocity -= targetVector / 2;
    }
    private void Spiral()
    {
        EntityManager.Add(new SpiralShot(position, targetVector * 8 + velocity, gunAngle.angle, 0, true, 5, false));
        EntityManager.Add(new SpiralShot(position, targetVector * 8 + velocity, gunAngle.angle, 0, true, 5, true));
        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
        modules[ModuleType.Guns].cooldown = 0.7f;
        Engine.ShakeScreen(0.2f);
    }
    private void Shotgun()
    {
        int randomBulletCount = random.Next(4, 6);
        for (int i = 0; i < randomBulletCount; i++)
        {
            float angleDegrees = (float)(random.NextDouble() - 0.5) * 5;
            float offsetAngle = angleDegrees * MathF.PI / 180;
            Vector2 targetVector = Engine.ToUnitVector(gunAngle.angle + offsetAngle);
            Vector2 positionOffset = Engine.ToUnitVector(gunAngle.angle + MathF.PI/2) * offsetAngle * 100;
            EntityManager.Add(new PulseShot(position + positionOffset, targetVector * 8 + velocity , gunAngle.angle + offsetAngle, 0, true, 2));
        }
        SoundManager.PlaySound(Assets.Get(Sound.ShotgunFire), position);
        velocity -= targetVector / 2;
        modules[ModuleType.Guns].cooldown = 1f;
        Engine.ShakeScreen(0.4f);
    }
    private void Missile()
    {
        EntityManager.Add(Enemy.NewMissile(position + Engine.ToUnitVector(gunAngle.angle + MathF.PI / 2) * 6, targetVector * 9 + velocity, gunAngle.angle, true));
        SoundManager.PlaySound(Assets.Get(Sound.MissileFire), position);
        modules[ModuleType.Guns].cooldown = 0.15f;
        velocity -= targetVector / 4;
        Engine.ShakeScreen(0.3f);
    }
    private void LMG()
    {
        Vector2 offset = Engine.ToUnitVector(gunAngle.angle + MathF.PI / 2) * random.Next(-2, 3);
        Texture2D dot = Assets.Get(Sprite.Microshot);
        Projectile shot = new PulseShot(position + offset, targetVector * 8 + velocity + offset / 4, gunAngle.angle, 0, true, 1)
        {
            texture = dot,
            timeLeft = 3
        };
        EntityManager.Add(shot);
        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), position);
        Engine.ShakeScreen(0.15f);
        velocity -= targetVector / 16;
        modules[ModuleType.Guns].cooldown = 0.09f;
    }
    private void Silenced()
    {
        Vector2 offset = Engine.ToUnitVector(gunAngle.angle + MathF.PI / 2) * random.Next(-2, 3);
        Texture2D dot = Assets.Get(Sprite.Microshot);
        Projectile shot = new PulseShot(position + offset, targetVector * 12 + velocity + offset / 4, gunAngle.angle, 0, true, 5, false, 1)
        {
            texture = dot,
            timeLeft = 3
        };
        EntityManager.Add(shot);
        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), position);
        Engine.ShakeScreen(0.15f);
        velocity -= targetVector / 16;
        modules[ModuleType.Guns].cooldown = 0.2f;
    }
    private void Dash()
    {
        invincibilityCooldown = 0.5f;
        Vector2 normalVector = new(MathF.Sin(gunAngle.angle), -MathF.Cos(gunAngle.angle));
        for (int i = 0; i < 200; i++)
        {
            float timeLeft = ((float)i / 200);
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), timeLeft, position + normalVector*i, velocity * timeLeft, gunAngle.angle, 0, 1, true, Color.Cyan, Color.SlateBlue));
        }
        position += normalVector * 200;
        modules[ModuleType.Engines].cooldown = 2f;
    }
    private void SummonShield()
    {
        if (!shield.isExpired)
        {
            return;
        }
        shield.health = 1;
        shield.isExpired = false;
        EntityManager.Add(shield);
        modules[ModuleType.Engines].cooldown = 5f;
    }
    public int GetSensingAbility()
    {
        int sensing = 1;
        if (modules[ModuleType.Sensors].isFailed)
        {
            sensing = 0;
        }
        return sensing;
    }
    public int GetStealthAbility()
    {
        int stealth = 0;
        if (direction != Vector2.Zero)
        {
            stealth -= 1;
        }
        if (modules[ModuleType.Guns].cooldown > 0)
        {
            stealth -= 1;
        }
        return stealth;
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        if (position.Length() > EntityManager.CurrentMission.Planet.radius + 25 * 50)
        {
            _spriteBatch.Draw(Assets.Get(Sprite.Arrow), position - Engine.mousePositionOffset - Vector2.Normalize(position) * 25, null, color, MathF.Atan2(-position.X, position.Y), Assets.DimsOf(Sprite.Arrow) / 2, 1, 0, 0.2f);
            _spriteBatch.DrawString(Assets.TextFont, "Return to planet.", Engine.Camera.Position - new Vector2(105, 225), Color.Crimson);
        }
        if(dockedEntity != null)
        {
            return;
        }
        base.Draw(_spriteBatch);
        Vector2 linePosition = position - Engine.mousePositionOffset + new Vector2(-texture.Width * 2, texture.Height * 1.5f) / 2;
        Rectangle sourceRectangle = new (0, 0, texture.Width * 2, 2);

        Engine.DrawFilledLine(_spriteBatch, linePosition, sourceRectangle, (1 - modules[ModuleType.Engines].cooldown / 2), Color.DarkGray, Color.Cyan);
        if (modules[ModuleType.Hull].ID == 1)
        {
            Engine.DrawFilledLine(_spriteBatch, linePosition + new Vector2(0, texture.Height / 4), sourceRectangle, (1 - modules[0].cooldown / 8), Color.DarkGray, Color.Yellow);
        }
    }
}
