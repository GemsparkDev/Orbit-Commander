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

namespace Space_Wars.Content.Main.Entities;

public class Player : Entity
{
    public DockableComponent dockedEntity;
    private Entity abilityEntity;
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
    private int spareFuses = 0;
    public override int SensingAbility
    {
        get
        {
            int sensing = 1;
            if (modules[ModuleType.Sensors] == null)
            {
                return -1;
            }
            if (modules[ModuleType.Sensors].isFailed)
            {
                sensing = 0;
            }
            float x = (float)(CountFuses(ModuleType.Sensors)) - 2;
            //Fuse modifiers: 0 = -2, 1 = -1, 2 or 3 = 0, 4 = +1
            sensing += (int)Math.Floor(x * x * x / 5);
            return sensing;
        }
    }
    public override int StealthAbility
    {
        get
        {
            int stealth = 0;
            if (isEngineActive)
            {
                stealth -= 1;
            }
            if (modules[ModuleType.Guns].cooldown > 0)
            {
                stealth -= 1;
            }
            return stealth;
        }
    }
    public List<Pickup> leashedMaterials = [];
    public Dictionary<ModuleType, Module> modules = new()
    {
        { ModuleType.Hull, ItemFactory.GetItem(ModuleType.Hull) },
        { ModuleType.Guns, ItemFactory.GetItem(ModuleType.Sniper) },
        { ModuleType.Engines, ItemFactory.GetItem(ModuleType.GrapplingHook) },
        { ModuleType.Sensors, ItemFactory.GetItem(ModuleType.Sensors) },
        { ModuleType.Core, ItemFactory.GetItem(ModuleType.Core) }
    };
    private bool[,] moduleFuses = new bool[5, 4]
    {
        { true, true, true, false },
        { true, true, true, false },
        { true, true, true, false },
        { true, true, true, false },
        { true, true, true, false }
    };

    public Player(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity)
        : base(Assets.Get(Sprite.Player), _position, _velocity, _angle, _angularVelocity, 5, true)
    {
        gunAngle = Enemy.NewDummyEnemy(position, true);
        abilityEntity = Enemy.NewShield(gunAngle, 10, 1, 0, 1, true);
        abilityEntity.isExpired = true;
        color = new Color(0, 255, 0);
        smokeParticles.isEmitterActive = false;
        engineParticles.isEmitterActive = false;
        engineSounds = Assets.Get(Sound.FireEngines).CreateInstance();
        engineSounds.IsLooped = true;
        SoundManager.AddSound(engineSounds);
        Texture2D[] textures = new Texture2D[modules.Count];
        for(int i = 0; i < modules.Count; i++)
        {
            textures[i] = modules[(ModuleType)i].itemData.RealSprite;
        }
        EventHandler.SetFuseModuleDecals(textures);
        EventHandler.UpdateFuseUI(moduleFuses, spareFuses);
    }
    public override void Update()
    {
        if (modules[ModuleType.Core].Health <= 0)
        {
            isExpired = true;
            SoundManager.PauseSound(engineSounds);
            Assets.Get(Sound.Death).Play();
            SoundManager.SFXVolume = (Engine.UIManager.GetFuncWidget(0, 3) as Slider).sliderInterval;
            SoundManager.MusicVolume = (Engine.UIManager.GetFuncWidget(0, 4) as Slider).sliderInterval;
            return;
        }
        engineParticles.position = position - new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * 8;
        engineParticles.Update();
        smokeParticles.position = position;
        smokeParticles.Update();
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
                    //Small bonus to module health to keep players alive in firefights
                    module.Health = Math.Min(module.MaxHealth, module.Health + Engine.Random.Next(1, 4));
                    restartedModules = true;
                    EventHandler.UpdateModulesStatus();
                    if (modules[ModuleType.Core] == module)
                    {
                        SoundManager.SFXVolume = (Engine.UIManager.GetFuncWidget(0, 3) as Slider).sliderInterval;
                        SoundManager.MusicVolume = (Engine.UIManager.GetFuncWidget(0, 4) as Slider).sliderInterval;
                    }
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
        if (cachedDamage > 0)
        {
            if (!modules[ModuleType.Hull].isFailed)
            {
                modules[ModuleType.Hull].ModuleFunction();
            }
            for (int i = 0; i < cachedDamage; i++)
            {
                int randomNumber = Engine.Random.Next(1, 4);
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
            cachedDamage = 0;
        }
        float currentHealth = modules[ModuleType.Hull].Health + modules[ModuleType.Guns].Health + modules[ModuleType.Engines].Health + modules[ModuleType.Sensors].Health + modules[ModuleType.Core].Health;
        if (currentHealth > 50)
        {
            smokeParticles.isEmitterActive = false;
        }
        else
        {
            smokeParticles.isEmitterActive = true;
            smokeParticles.speedOfEmission = 25f - currentHealth/4;
        }
        var planet = Engine.EntityManager.CurrentMission.Planet;
        if (position.Length() >= 40 * 50 + planet.radius)
        {
            velocity *= 0.8f;
            velocity += Vector2.Normalize(-position) * Engine.DeltaSeconds * (position.Length() - (40 * 50 + planet.radius));
        }
        position += velocity * Engine.DeltaSeconds * 60;
        if (dockedEntity != null)
        {
            if (dockedEntity.IsValid)
            {
                position = dockedEntity.Position;
                velocity = dockedEntity.Velocity;
            }
            else
            {
                dockedEntity = null;
            }
        }
        for (int i = 0; i < modules.Count; i++)
        {
            var module = modules[(ModuleType)i];
            //Square root of the ratio reduces balancing impact with an additional fuse (especially with the gun dps)
            //Note: Do not have any active abilities that are based on the cooldown, as the player could remove all fuses and get infinite of the ability
            float fuseRatio = MathF.Sqrt((float)CountFuses((ModuleType)i)/3);
            if(fuseRatio > 1.01)
            {
                //Bonus for 4 fuses
                module.UpdateCooldown();
                //Allows for easy random check in all cases
                fuseRatio -= 1f;
            }
            if (Engine.Random.NextSingle() < fuseRatio)
            {
                module.UpdateCooldown();
            }
        }
        gunAngle.position = position;
        base.Update();
    }
    public void RestrictedActions()
    {
        //Prevents undocking when in the garage menu
        if (!modules[ModuleType.Core].isFailed)
        {
            if (Input.OldState.IsKeyUp(Keys.I) && Input.NewState.IsKeyDown(Keys.I))
            {
                EventHandler.ToggleDockingMenus();
            }
            if (dockedEntity == null)
            {
                targetVector = Vector2.Normalize(new Vector2(Mouse.GetState().X, Mouse.GetState().Y) - Engine.BackBuffer / 2 - position + Engine.Camera.Position);
                gunAngle.angle = MathF.Atan2(targetVector.X, -targetVector.Y) - Engine.Camera.Rotation;
                if (Input.NewMouseState.LeftButton == ButtonState.Pressed && modules[ModuleType.Guns].IsCooldownReady() && !UIManager.LockMouseInput && !modules[ModuleType.Guns].isFailed)
                {
                    modules[ModuleType.Guns].ModuleFunction();
                }
                if (Input.NewMouseState.RightButton == ButtonState.Pressed && Input.OldMouseState.RightButton == ButtonState.Released && !UIManager.LockMouseInput)
                {
                    canGatherResources = true;
                    SoundManager.PlayGlobalSound(Assets.Get(Sound.OpenMenu));
                }
                else if (Input.NewMouseState.RightButton == ButtonState.Released && Input.OldMouseState.RightButton == ButtonState.Pressed && !UIManager.LockMouseInput)
                {
                    SoundManager.PlayGlobalSound(Assets.Get(Sound.CloseMenu));
                    canGatherResources = false;
                }
                if (Input.OldState.IsKeyUp(Keys.Z) && Input.NewState.IsKeyDown(Keys.Z))
                {
                    leashedMaterials = [];
                }
                if (Input.OldState.IsKeyUp(Keys.Q) && Input.NewState.IsKeyDown(Keys.Q) && !modules[ModuleType.Engines].isFailed)
                {
                    modules[ModuleType.Engines].ModuleFunction();
                }
                Keys[] pressedKey = Input.NewState.GetPressedKeys();
                direction = Vector2.Zero;
                if (!modules[ModuleType.Engines].isFailed)
                {
                    isEngineActive = false;
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
                    if (isEngineActive)
                    {
                        engineParticles.offsetVelocity = velocity;
                        angle = angle * 0.5f + MathF.Atan2(direction.X, -direction.Y) * 0.5f;
                        engineParticles.sprayAngle = angle * 180 / MathF.PI + 180;
                        float fuseRatio = (float)(CountFuses(ModuleType.Engines)) / 3;
                        engineParticles.speedOfEmission = 450f * fuseRatio;
                        engineSounds.Volume = Math.Clamp(fuseRatio, 0, 1);
                        float speed = 0.2f;
                        velocity += Engine.ToUnitVector(angle) * 60 * Engine.DeltaSeconds * speed * 2 * fuseRatio / (leashedMaterials.Count + 2);
                    }
                }
            }
            if (Input.OldState.IsKeyUp(Keys.Space) && Input.NewState.IsKeyDown(Keys.Space))
            {
                if (dockedEntity == null)
                {
                    DockableComponent dockableEntity = Engine.EntityManager.NearestDockableEntity(this);
                    if (dockableEntity != null)
                    {
                        if (dockableEntity.Dock(this))
                        {
                            dockedEntity = dockableEntity;
                            isEngineActive = false;
                        }
                    }
                }
                else if (dockedEntity.Dock(this))
                {
                    dockedEntity = null;
                    isEngineActive = false;
                }
            }
        }
        //Prevents unusual interations between various game states
        if (EventHandler.AcknowledgeMessage(Message.ToggleTerminal))
        {
            if (dockedEntity != null)
            {
                Engine.UIManager.ToggleMenu((int)dockedEntity.Menu);
            }
            else
            {
                Engine.UIManager.ToggleMenu((int)Containers.PlayerMenu);
            }
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
    }
    public override void Collide(int _damage)
    {
        if (_damage > 0 && invincibilityCooldown <= 0)
        {
            Engine.ShakeScreen(0.08f * _damage);
            cachedDamage += _damage;
            SoundManager.PlaySound(Assets.Get(Sound.Hit), position);
            invincibilityCooldown = 1;
            ParticleManager.Add(new Particle(null, 1, position + new Vector2(0, -1), new Vector2(0, -1.5f), 0, 0, 1, true, Color.Red, Color.Red) { drawText = $"{_damage}" });
            //Part Failure
            if (Engine.Random.Next(0, 1) == 0)
            {
                ModuleType failedPart = (ModuleType)Engine.Random.Next(0, 4);
                if (modules[failedPart].Health < modules[failedPart].MaxHealth / 2)
                {
                    if (modules[failedPart].isFailed)
                    {
                        failedPart = ModuleType.Core;
                    }
                    if (modules[failedPart].isFailed && modules[ModuleType.Core].isFailed)
                    {
                        return;
                    }
                    modules[failedPart].isFailed = true;
                    string text = $"{failedPart} has failed!";
                    //Picks a random fuse to burn out if a module fails
                    int burntOutFuse = Engine.Random.Next(0, 3);
                    if (moduleFuses[(int)failedPart, burntOutFuse])
                    {
                        moduleFuses[(int)failedPart, burntOutFuse] = false;
                        SoundManager.PlayGlobalSound(Assets.Get(Sound.FireEngines));
                        text += " Fuse damaged!";
                        EventHandler.UpdateFuseUI(moduleFuses, spareFuses);
                    }
                    ParticleManager.Add(new Particle(null, 2, position + new Vector2(0, -3), new Vector2(0, -0.75f), 0, 0, 1, true, Color.Red, Color.Red) { drawText = text });
                    SoundManager.PlaySound(Assets.Get(Sound.Beep), position);
                    EventHandler.UpdateModulesStatus();
                    if (failedPart == ModuleType.Core)
                    {
                        SoundManager.SFXVolume = 0;
                        SoundManager.MusicVolume = 0;
                    }
                }
            }
        }
    }
    public void ToggleFuse(int x, int y)
    {
        bool fuse = moduleFuses[x, y];
        if (!fuse && spareFuses <= 0)
        {
            return;
        }
        spareFuses += fuse ? 1 : -1;
        moduleFuses[x, y] = !moduleFuses[x, y];
        EventHandler.UpdateFuseUI(moduleFuses, spareFuses);
    }
    public void AddFuse()
    {
        spareFuses++;
        EventHandler.UpdateFuseUI(moduleFuses, spareFuses);
    }
    public int CountFuses(ModuleType _module)
    {
        int count = 0;
        //Fuses only count if the corresponding core fuse is also active
        for (int i = 0; i < 4; i++)
        {
            switch (_module)
            {
                case ModuleType.Core:
                    count += moduleFuses[(int)ModuleType.Core, i] ? 1 : 0;
                    break;
                default:
                    bool fuse = moduleFuses[(int)_module, i];
                    count += (fuse && moduleFuses[(int)ModuleType.Core, i]) ? 1 : 0;
                    break;
            }
        }
        return count;
    }
    public void Hull()
    {
        cachedDamage /= 2;
    }
    public void Shield()
    {
        if(modules[0].cooldown <= 0)
        {
            modules[0].cooldown = 8;
            cachedDamage = 0;
            return;
        }
        modules[0].Health = MathF.Max(0, modules[0].Health - cachedDamage / 2);
    }
    public void Basic()
    {
        Engine.EntityManager.Add(new PulseShot(position, targetVector * 9 + velocity, gunAngle.angle, 0, true, 3, true));
        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
        modules[ModuleType.Guns].cooldown = 0.25f;
        Engine.ShakeScreen(0.2f);
        velocity -= targetVector / 4;
    }
    public void Sniper()
    {
        Engine.EntityManager.Add(new AssassinShot(position, targetVector * 100, gunAngle.angle, 0, true, 10));
        SoundManager.PlaySound(Assets.Get(Sound.SniperFire), position);
        modules[ModuleType.Guns].cooldown = 2f;
        Engine.ShakeScreen(0.3f);
        velocity -= targetVector / 2;
    }
    public void Spiral()
    {
        Engine.EntityManager.Add(new SpiralShot(position, targetVector * 8 + velocity, gunAngle.angle, 0, true, 5, false));
        Engine.EntityManager.Add(new SpiralShot(position, targetVector * 8 + velocity, gunAngle.angle, 0, true, 5, true));
        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
        modules[ModuleType.Guns].cooldown = 0.7f;
        Engine.ShakeScreen(0.2f);
    }
    public void Shotgun()
    {
        int randomBulletCount = Engine.Random.Next(4, 6);
        for (int i = 0; i < randomBulletCount; i++)
        {
            float angleDegrees = (Engine.Random.NextSingle() - 0.5f) * 5;
            float offsetAngle = angleDegrees * MathF.PI / 180;
            Vector2 targetVector = Engine.ToUnitVector(gunAngle.angle + offsetAngle);
            Vector2 positionOffset = Engine.ToUnitVector(gunAngle.angle + MathF.PI/2) * offsetAngle * 100;
            Engine.EntityManager.Add(new PulseShot(position + positionOffset, targetVector * 8 + velocity , gunAngle.angle + offsetAngle, 0, true, 2));
        }
        SoundManager.PlaySound(Assets.Get(Sound.ShotgunFire), position);
        velocity -= targetVector / 2;
        modules[ModuleType.Guns].cooldown = 1f;
        Engine.ShakeScreen(0.4f);
    }
    public void Missile()
    {
        Engine.EntityManager.Add(Enemy.NewMissile(position + Engine.ToUnitVector(gunAngle.angle + MathF.PI / 2) * 6, targetVector * 9 + velocity, gunAngle.angle, true));
        SoundManager.PlaySound(Assets.Get(Sound.MissileFire), position);
        modules[ModuleType.Guns].cooldown = 0.15f;
        velocity -= targetVector / 4;
        Engine.ShakeScreen(0.3f);
    }
    public void LMG()
    {
        Vector2 offset = Engine.ToUnitVector(gunAngle.angle + MathF.PI / 2) * Engine.Random.Next(-2, 3);
        Texture2D dot = Assets.Get(Sprite.Microshot);
        Projectile shot = new PulseShot(position + offset, targetVector * 8 + velocity + offset / 4, gunAngle.angle, 0, true, 1)
        {
            texture = dot,
            timeLeft = 3
        };
        Engine.EntityManager.Add(shot);
        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), position);
        Engine.ShakeScreen(0.15f);
        velocity -= targetVector / 16;
        modules[ModuleType.Guns].cooldown = 0.09f;
    }
    public void Silenced()
    {
        Vector2 offset = Engine.ToUnitVector(gunAngle.angle + MathF.PI / 2) * Engine.Random.Next(-2, 3);
        Texture2D dot = Assets.Get(Sprite.Microshot);
        Projectile shot = new PulseShot(position + offset, targetVector * 12 + velocity + offset / 4, gunAngle.angle, 0, true, 5, false, 1)
        {
            texture = dot,
            timeLeft = 3
        };
        Engine.EntityManager.Add(shot);
        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), position);
        Engine.ShakeScreen(0.15f);
        velocity -= targetVector / 16;
        modules[ModuleType.Guns].cooldown = 0.2f;
    }
    public void Dash()
    {
        if (!modules[ModuleType.Engines].IsCooldownReady())
        {
            return;
        }
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
    public void SummonShield()
    {
        if (!modules[ModuleType.Engines].IsCooldownReady())
        {
            return;
        }
        if (!abilityEntity.isExpired)
        {
            return;
        }
        (abilityEntity as Enemy).health = 1;
        abilityEntity.isExpired = false;
        Engine.EntityManager.Add(abilityEntity);
        modules[ModuleType.Engines].cooldown = 5f;
    }
    public void SummonGrapplingHook()
    {
        if ((abilityEntity == null || abilityEntity.isExpired) && modules[ModuleType.Engines].IsCooldownReady())
        {
            abilityEntity = new GrapplingHook(position, targetVector * 50, gunAngle.angle, this);
            SoundManager.PlaySound(Assets.Get(Sound.Click), position);
            Engine.ShakeScreen(0.3f);
            velocity -= targetVector / 2;
            Engine.EntityManager.Add(abilityEntity);
            modules[ModuleType.Engines].cooldown = 5f;
        }
        else if(abilityEntity != null && !abilityEntity.isExpired)
        {
            abilityEntity.isExpired = true;
            modules[ModuleType.Engines].cooldown /= 2;
        }
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        if (position.Length() > Engine.EntityManager.CurrentMission.Planet.radius + 25 * 50)
        {
            _spriteBatch.Draw(Assets.Get(Sprite.Arrow), position - Vector2.Normalize(position) * 25, null, color, MathF.Atan2(-position.X, position.Y), Assets.DimsOf(Sprite.Arrow) / 2, 1, 0, 0.2f);
            _spriteBatch.DrawString(Assets.TextFont, "Return to planet.", Engine.Camera.Position - new Vector2(105, 225), Color.Crimson);
        }
        if(dockedEntity != null)
        {
            return;
        }
        base.Draw(_spriteBatch);
        Vector2 linePosition = position + new Vector2(-texture.Width * 2, texture.Height * 1.5f) / 2;
        Rectangle sourceRectangle = new (0, 0, texture.Width * 2, 2);

        Engine.DrawFilledLine(_spriteBatch, linePosition, sourceRectangle, (1 - modules[ModuleType.Engines].cooldown / 2), Color.DarkGray, Color.Cyan);
        if (modules[ModuleType.Hull].ID == 1)
        {
            Engine.DrawFilledLine(_spriteBatch, linePosition + new Vector2(0, texture.Height / 4), sourceRectangle, (1 - modules[0].cooldown / 8), Color.DarkGray, Color.Yellow);
        }
    }
}
