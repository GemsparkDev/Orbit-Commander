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
using System.Text;

namespace Space_Wars.Content.Main.Entities;

public class Player : Entity
{
    //Serialized variables
    private int spareFuses = 1;
    private bool aimAssist = true;
    private bool[,] moduleFuses = new bool[5, 4]
    {
        { true, true, true, false },
        { true, true, true, false },
        { true, true, true, false },
        { true, true, true, false },
        { true, true, true, false }
    };
    public Dictionary<ModuleType, Module> modules = new()
    {
        { ModuleType.Hull, new Module(Modules.Shield) },
        { ModuleType.Guns, new Module(Modules.Spiral) },
        { ModuleType.Engines, new Module(Modules.Plasma) },
        { ModuleType.Sensors, new Module(Modules.Sensors) },
        { ModuleType.Core, new Module(Modules.Dash) }
    };

    public DockableComponent dockedEntity;
    public List<Pickup> leashedMaterials = [];
    private Entity abilityEntity;
    private Entity gunAngle;
    private ParticleEmitter engineParticles = new(Assets.Get(Sprite.Circle), 0.15f, Vector2.Zero, 0, MathF.PI/4, 2, 450f, Color.Cyan, EmitterType.EmissionOverTime) { isEmitterActive = false, particleFadeToColor = new Color(72, 61, 139, 0) };
    private ParticleEmitter smokeParticles = new(Assets.Get(Sprite.Circle), 1f, Vector2.Zero, 0, MathF.PI/4, 1, 0.5f, Color.Gray, EmitterType.EmissionOverTime) { isEmitterActive = false, particleFadeToColor = new Color(169, 169, 169, 0) };
    private ParticleEmitter shieldEffect;
    private SoundEffectInstance engineSounds;
    private float invincibilityCooldown = 0;
    private float cachedDamage = 0;
    private float restartCooldown = 0;
    private float abilityMaxCooldown = 1f;
    private bool isRestarting = false;
    public bool isEngineActive = false;
    public bool canGatherResources = false;
    private Vector2 targetVector;
    private Vector2 direction;
    private float time = 0;
    private float engineTime = 0;
    public int Progression { get; set; } = 3;
    public override int SensingAbility
    {
        get
        {
            int sensing = 1;
            if (modules[ModuleType.Sensors] == null)
            {
                return -1;
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
            if (revealDuration > 0)
            {
                return -10;
            }
            int stealth = 0;
            if (isEngineActive)
            {
                stealth -= 1;
                if (modules[ModuleType.Engines].Type == Modules.Plasma)
                {
                    stealth -= 1;
                }
            }
            if (modules[ModuleType.Guns].cooldown > 0)
            {
                stealth -= 1;
            }
            if (modules[ModuleType.Hull].Type == Modules.Stealth)
            {
                stealth += 1;
            }
            return stealth;
        }
    }

    public Player(Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity)
        : base(Assets.Get(Sprite.Player), _position, _velocity, _angle, _angularVelocity, 5, true)
    {
        gunAngle = Enemy.NewDummyEnemy(position, true);
        color = new Color(0, 255, 0);
        smokeParticles.isEmitterActive = false;
        engineParticles.isEmitterActive = false;
        engineSounds = Assets.Get(Sound.FireEngines).CreateInstance();
        engineSounds.IsLooped = true;
        var textures = new Texture2D[modules.Count];
        for(int i = 0; i < modules.Count; i++)
        {
            textures[i] = modules[(ModuleType)i].Texture;
        }
        EventHandler.SetFuseModuleDecals(textures);
        EventHandler.UpdateFuseUI(moduleFuses, spareFuses);
        shieldEffect = new(Assets.Get(Sprite.Dot), position, 10, Color.Violet) { particleAngularVelocity = 0.1f };
        //ApplyStatus(new Bomb());
        ApplyStatus(new Fire(10));
    }
    public override void Update()
    {
        time += Engine.DeltaSeconds;
        if (Progression > 0 && Input.OldState.IsKeyUp(Keys.F) && Input.NewState.IsKeyDown(Keys.F))
        {
            SoundManager.SetAllSounds(false);
            CurrentGameState.SwitchState(new InShip());
            UI.FuseMenu.enabled = true;
        }
        if (modules[ModuleType.Core].Health <= 0)
        {
            isExpired = true;
            engineSounds.Stop();
            Assets.Get(Sound.Death).Play();
            SoundManager.SFXVolume = UI.SFXSlider.sliderInterval;
            SoundManager.MusicVolume = UI.MusicSlider.sliderInterval;
            return;
        }
        engineParticles.position = position - new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * 8;
        smokeParticles.position = position;
        leashedMaterials = leashedMaterials.Where(x => !x.isExpired).ToList();
        float restart = 1.5f;
        if (EventHandler.AcknowledgeMessage(Message.RestartModules))
        {
            foreach (var module in modules.Values)
            {
                if (module.isFailed)
                {
                    restartCooldown = restart;
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
            //Reverse order prioritizes most important modules first
            for(int i = modules.Count-1; i >= 0; i--)
            {
                var module = modules[(ModuleType)(i)];
                if (restartedModules && module.isFailed)
                {
                    restartCooldown = restart;
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
                        SoundManager.SFXVolume = UI.SFXSlider.sliderInterval;
                        SoundManager.MusicVolume = UI.MusicSlider.sliderInterval;
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
            EventHandler.UpdateRestartSlider(restart - restartCooldown, restart);
        }
        if (invincibilityCooldown > 0)
        {
            invincibilityCooldown -= Engine.DeltaSeconds;
            color = invincibilityCooldown > 0 ? new Color(0, 255, 0) * (MathF.Cos(invincibilityCooldown * 30) / 2 + 0.5f) : new Color(0, 255, 0);
        }
        if (cachedDamage > 0)
        {
            modules[ModuleType.Hull].ModuleFunction();
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
        float maxHealth = modules[ModuleType.Hull].MaxHealth + modules[ModuleType.Guns].MaxHealth + modules[ModuleType.Engines].MaxHealth + modules[ModuleType.Sensors].MaxHealth + modules[ModuleType.Core].MaxHealth;

        UI.PlayerHealth.SetInterval(currentHealth, maxHealth);
        Vector3 colorVec;
        float val = (MathF.Sin(time) + 1f) / 2;
        shieldEffect.position = position;
        if (modules[ModuleType.Hull].Type == Modules.Shield && modules[ModuleType.Hull].cooldown <= 0)
        {
            UI.PlayerHealth.enabledColor = Color.Violet;
            if (dockedEntity == null) 
            {
                shieldEffect.offsetVelocity = velocity;
                shieldEffect.Update();
            }
        }
        else
        {
            colorVec = new Vector3(1, 0, 0) * val + new Vector3(1, 0.2f, 0.2f) * (1f - val);
            UI.PlayerHealth.enabledColor = new Color(colorVec.X, colorVec.Y, colorVec.Z);
        }

        //Only displays if the player has abilities unlocked
        if (Progression > 1) 
        {
            UI.PlayerAbility.SetInterval(1 - (modules[ModuleType.Core].cooldown / abilityMaxCooldown), 1);
            colorVec = new Vector3(0, 1, 1) * val + new Vector3(0.2f, 1, 0.8f) * (1f - val);
            UI.PlayerAbility.enabledColor = new Color(colorVec.X, colorVec.Y, colorVec.Z);
            UI.PlayerAbility.disabledColor = Color.DarkGray;
        }
        else
        {
            UI.PlayerAbility.enabledColor = Color.Transparent;
            UI.PlayerAbility.disabledColor = Color.Transparent;
        }

        if (currentHealth > 50)
        {
            smokeParticles.isEmitterActive = false;
        }
        else
        {
            smokeParticles.isEmitterActive = true;
            smokeParticles.speedOfEmission = 25f - currentHealth/4;
        }
        var planet = Engine.SaveGame.CurrentMission.Planet;
        if (position.Length() >= 50 * 50 + planet.radius)
        {
            velocity *= (1 - Engine.DeltaSeconds);
            velocity += Vector2.Normalize(-position) * (position.Length() - (50 * 50 + planet.radius)) * Engine.DeltaSeconds / 10;
        }
        base.Update();
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
            if(fuseRatio - 1 > float.Epsilon)
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
        if (Progression < 0)
        {
            isEngineActive = false;
        }
        gunAngle.position = position;
    }
    public void RestrictedActions()
    {
        //Prevents undocking when in the garage menu
        if (Progression > -1)
        {
            if (Input.OldState.IsKeyUp(Keys.I) && Input.NewState.IsKeyDown(Keys.I))
            {
                EventHandler.ToggleDockingMenus();
            }
            if (Progression > 1 && Input.OldState.IsKeyUp(Keys.LeftControl) && Input.NewState.IsKeyDown(Keys.LeftControl))
            {
                aimAssist = !aimAssist;
                SoundEffectInstance sound = Assets.Get(Sound.Click).CreateInstance();
                if (aimAssist)
                {
                    sound.Pitch = 0.5f;
                }
                SoundManager.PlayGlobalSound(sound);
            }
            modules[ModuleType.Sensors].ModuleFunction();
            if (dockedEntity == null)
            {
                if (Progression > 2)
                {
                    if (Input.NewState.IsKeyDown(Keys.C))
                    {
                        float dist = (new Vector2(Input.NewMouseState.X, Input.NewMouseState.Y) - Engine.BackBuffer / 2).Length();
                        var constructs = new List<(String description, Texture2D sprite)>()
                    {
                        ("Req. 1 scrap, blocks enemy fire. 20 integrity.", Assets.Get(Sprite.Barricade)),
                        ("Req. 1 scrap, attacks enemies. 8 integrity.", Assets.Get(Sprite.Trap)),
                        ("Req. 1 scrap, 100 dmg to all in radius when destroyed. 3 integrity.", Assets.Get(Sprite.Bomb)),
                    };
                        if (Progression > 3)
                        {
                            constructs.Add(("Req. 3 scrap, deployable garage. Use metal to upgrade.", Assets.Get(Sprite.Mothership)));
                        }
                        if (Engine.SaveGame.CurrentMission.Name == "???")
                        {
                            constructs.Add(("1 scrap to construct. Be ready.", Assets.Get(Sprite.QuantumResonator)));
                        }
                        float angle = 0;
                        Color color;
                        for (float i = 0; i < constructs.Count; i++)
                        {
                            Vector2 dir = Engine.ToUnitVector(angle);
                            Vector2 mouseDir = Engine.ToUnitVector(gunAngle.angle);
                            if (dir.X * mouseDir.X + dir.Y * mouseDir.Y > (0.2f * constructs.Count) && dist > 500)
                            {
                                color = Color.White;
                                ParticleManager.Add(new Particle(null, new Vector2(0, -100) + position, 0, Color.White) { drawText = constructs[(int)i].description });
                            }
                            else
                            {
                                color = Color.Cyan;
                            }
                            ParticleManager.Add(new Particle(constructs[(int)i].sprite, dir * 45 + position, 0, color));
                            angle += MathF.PI * 2 / constructs.Count;
                        }
                    }
                    else if (Input.OldState.IsKeyDown(Keys.C) && Input.NewState.IsKeyUp(Keys.C))
                    {
                        float dist = (new Vector2(Input.NewMouseState.X, Input.NewMouseState.Y) - Engine.BackBuffer / 2).Length();
                        int scrapCount = 0;
                        Entity firstScrap = null;
                        foreach (var pickup in leashedMaterials)
                        {
                            if (pickup is not Module and not Construct)
                            {
                                scrapCount++;
                                firstScrap ??= pickup;
                            }
                        }
                        float angle = 0;
                        var types = new List<string>
                    {
                        "Barricade",
                        "Trap",
                        "Bomb"
                    };
                        if (Progression > 3)
                        {
                            types.Add("Mothership");
                        }
                        if (Engine.SaveGame.CurrentMission.Name == "???")
                        {
                            types.Add("Resonator");
                        }
                        for (int i = 0; i < types.Count; i++)
                        {
                            Vector2 dir = Engine.ToUnitVector(angle);
                            Vector2 mouseDir = Engine.ToUnitVector(gunAngle.angle);
                            if (dir.X * mouseDir.X + dir.Y * mouseDir.Y > (0.2f * types.Count) && dist > 500)
                            {
                                switch (types[i])
                                {
                                    case "Barricade":
                                        firstScrap.isExpired = true;
                                        var barricade = new Construct(Constructs.Barricade, firstScrap.position, firstScrap.velocity, 0, 0);
                                        leashedMaterials.Add(barricade);
                                        Engine.EntityManager.Add(barricade);
                                        break;
                                    case "Trap":
                                        firstScrap.isExpired = true;
                                        var trap = new Construct(Constructs.Trap, firstScrap.position, firstScrap.velocity, 0, 0);
                                        leashedMaterials.Add(trap);
                                        Engine.EntityManager.Add(trap);
                                        break;
                                    case "Bomb":
                                        firstScrap.isExpired = true;
                                        var bomb = new Construct(Constructs.Bomb, firstScrap.position, firstScrap.velocity, 0, 0);
                                        leashedMaterials.Add(bomb);
                                        Engine.EntityManager.Add(bomb);
                                        break;
                                    case "Mothership":
                                        if (scrapCount >= 3)
                                        {
                                            foreach (var pickup in leashedMaterials)
                                            {
                                                pickup.isExpired = true;
                                            }
                                            leashedMaterials.Clear();
                                            Engine.EntityManager.Add(Enemy.NewMakeshiftMothership(position, velocity, 0));
                                        }
                                        break;
                                    case "Resonator":
                                        //firstScrap.isExpired = true;
                                        Engine.EntityManager.Add(Enemy.NewQuantumResonator(position));
                                        break;
                                }
                            }
                            angle += MathF.PI * 2 / (float)(types.Count);
                        }
                    }
                }
                //Ensures that target vector performs identically in all resolutions
                Vector2 ratio = Engine.ScreenSize / Engine.BackBuffer;
                targetVector = Vector2.Normalize(new Vector2(Input.NewMouseState.X * ratio.X, Input.NewMouseState.Y * ratio.Y) - Engine.ScreenSize / 2 - position + Engine.Camera.Position + Engine.MousePositionOffset);
                gunAngle.angle = MathF.Atan2(targetVector.X, -targetVector.Y);
                if (!UIManager.LockMouseInput && Input.NewMouseState.RightButton == ButtonState.Pressed)
                {
                    Vector2 dir = Engine.ToUnitVector(gunAngle.angle);
                    List<Entity> miningEnemies = Engine.EntityManager.Hitscan(position, dir, 120, false, out Vector2 _end);
                    if (miningEnemies.Count > 0 && miningEnemies[0] as Enemy != null)
                    {
                        (miningEnemies[0] as Enemy).Mine();
                    }
                    for (float i = 0; i < (_end - position - dir * 8).Length() / 2; i++)
                    {
                        float lerp = i / 60;
                        Vector3 color = new Vector3(1, 1, 0) * (1 - lerp) + new Vector3(1, 0, 0) * (lerp);
                        ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), dir * (i + 4f) * 2 + position + new Vector2(dir.Y, -dir.X) * MathF.Sin(i / 2 - time * 5) / 2, gunAngle.angle, new Color(color.X, color.Y, color.Z) * (1 - (lerp))));
                    }
                    if (Input.OldMouseState.RightButton == ButtonState.Released)
                    {
                        canGatherResources = true;
                        SoundManager.PlayGlobalSound(Assets.Get(Sound.OpenMenu));
                    }
                }
                if (Input.NewMouseState.RightButton == ButtonState.Released && Input.OldMouseState.RightButton == ButtonState.Pressed && !UIManager.LockMouseInput)
                {
                    SoundManager.PlayGlobalSound(Assets.Get(Sound.CloseMenu));
                    canGatherResources = false;
                }
                if (Input.OldState.IsKeyUp(Keys.Z) && Input.NewState.IsKeyDown(Keys.Z))
                {
                    leashedMaterials = [];
                }
                if (Progression > 1 && Input.OldState.IsKeyUp(Keys.Q) && Input.NewState.IsKeyDown(Keys.Q))
                {
                    modules[ModuleType.Core].ModuleFunction();
                }
                Keys[] pressedKey = Input.NewState.GetPressedKeys();
                direction = Vector2.Zero;
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
                modules[ModuleType.Engines].ModuleFunction();
                engineParticles.offsetVelocity = velocity;
                angle = angle * 0.5f + MathF.Atan2(direction.X, -direction.Y) * 0.5f;
                if (Input.NewMouseState.LeftButton == ButtonState.Pressed && !UIManager.LockMouseInput)
                {
                    if (!isEngineActive)
                    {
                        angle = angle * 0.5f + gunAngle.angle * 0.5f;
                    }
                    modules[ModuleType.Guns].ModuleFunction();
                }
            }
            if (Input.OldState.IsKeyUp(Keys.Space) && Input.NewState.IsKeyDown(Keys.Space))
            {
                Dock();
            }
            engineParticles.Update();
            smokeParticles.offsetVelocity = velocity;
            if (Engine.Random.Next(0, 2) == 0)
            {
                smokeParticles.Update();
            }
        }
        //Prevents unusual interations between various game states
        if (EventHandler.AcknowledgeMessage(Message.ToggleTerminal))
        {
            if (dockedEntity != null)
            {
                dockedEntity.Menu.enabled = !dockedEntity.Menu.enabled;
            }
            else
            {
                UI.PlayerMenu.enabled = !UI.PlayerMenu.enabled;
            }
        }
        engineParticles.isEmitterActive = isEngineActive;
        if (isEngineActive)
        {
            SoundManager.PlayLoopedSound(engineSounds);
        }
    }
    private Vector2 IdealSpeedWithVelocity(float _speed)
    {
        //Derivation
        //Assume target vector is normalized and is ideal bullet velocity
        //Thus, velocity should be any point on the line (x, y) = t * (targetVector.X, targetVector.Y)
        //Using the circle formula, speed^2 = (x pos of point on line - current x velocity)^2 + (y pos on line - y velocity)^2
        //Substitute and rearrange: t^2(targetVector.X^2 + targetVector.Y^2) - 2t(targetVector.X * velocity.X + targetVector.Y * velocity.Y) + (velocity.Y^2 + velocity.X^2 - speed^2)
        //Then, use quadratic formula and solve for t, then multiply by targetVector to get the best possible velocity for the bullet

        if (aimAssist)
        {
            Vector2 acc = Engine.SaveGame.CurrentMission.GetNormalizedAcceleration(position + targetVector * _speed * 3) * 180 / _speed;
            Vector2 vel = velocity - acc;
            float b = targetVector.X * vel.X + targetVector.Y * vel.Y;
            float c = vel.X * vel.X + vel.Y * vel.Y - _speed * _speed;
            float disc = b * b - c;
            if (disc >= 0)
            {
                float t = b + MathF.Sqrt(disc);
                if (t > 0)
                {
                    return targetVector * t + acc;
                }
            }
        }
        return targetVector * _speed + velocity;
    }
    public void Dock()
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
                    if (abilityEntity != null)
                    {
                        abilityEntity.isExpired = true;
                    }
                }
            }
        }
        else if (dockedEntity.Dock(this))
        {
            dockedEntity = null;
            isEngineActive = false;
        }
    }
    public override void Collide(int _damage, bool _ignoreImmunity = false)
    {
        if (dockedEntity != null)
        {
            dockedEntity.Collide(_damage);
            return;
        }
        if (_damage > 0 && (invincibilityCooldown <= 0 || _ignoreImmunity))
        {
            Engine.ShakeScreen(0.08f * _damage);
            //Helps to cushion huge hits
            //Player will never be one shot (unless they deserve it)
            cachedDamage += Math.Min(50, _damage);
            SoundManager.PlaySound(Assets.Get(Sound.Hit), position);
            if (!_ignoreImmunity) 
            {
                invincibilityCooldown = 1;
            }
            ParticleManager.Add(new Particle(null, 1, position + new Vector2(0, -1), new Vector2(0, -1.5f), 0, 0, Color.Red, Color.Transparent) { drawText = $"{_damage}" });
            //Part and Fuse Failure
            if (Progression > 0)
            {
                //If a module is failed, further collisions damage fuses
                var targetFuse = new Vector2(Engine.Random.Next(0, moduleFuses.GetLength(0)), Engine.Random.Next(0, moduleFuses.GetLength(1)));
                var failedPart = (ModuleType)Engine.Random.Next(0, 4);
                float threshold = 1 - 1 / (_damage - 1);
                if (Engine.Random.NextSingle() < threshold && modules[(ModuleType)targetFuse.X].isFailed && moduleFuses[(int)targetFuse.X, (int)targetFuse.Y])
                {
                    moduleFuses[(int)targetFuse.X, (int)targetFuse.Y] = false;
                    ParticleManager.Add(new Particle(null, 2, position + new Vector2(0, -3), new Vector2(0, -0.75f), 0, 0, Color.Red, Color.Transparent) { drawText = "Fuse damaged!" });
                    SoundManager.PlaySound(Assets.Get(Sound.Beep), position);
                    EventHandler.UpdateFuseUI(moduleFuses, spareFuses);
                }
                else if (Engine.Random.Next(0, 5) == 0 && modules[failedPart].Health < modules[failedPart].MaxHealth / 2)
                {
                    if (modules[failedPart].isFailed)
                    {
                        failedPart = ModuleType.Core;
                        if (modules[ModuleType.Core].isFailed)
                        {
                            return;
                        }
                    }
                    modules[failedPart].isFailed = true;
                    ParticleManager.Add(new Particle(null, 2, position + new Vector2(0, -3), new Vector2(0, -0.75f), 0, 0, Color.Red, Color.Transparent) { drawText = $"{failedPart} has failed!" });
                    SoundManager.PlaySound(Assets.Get(Sound.Beep), position);
                    EventHandler.UpdateModulesStatus();
                }
            }
        }
        else if (_damage < 0)
        {
            int healed = 0;
            while (_damage < 0)
            {
                Module module = modules[(ModuleType)Engine.Random.Next(0, 5)];
                if (module.Health < module.MaxHealth)
                {
                    module.Health++;
                    healed++;
                }
                _damage += 1;
            }
            SoundManager.PlaySound(Assets.Get(Sound.Full), position);
            ParticleManager.Add(new Particle(null, 1, position + new Vector2(0, -1), new Vector2(0, -1.5f), 0, 0, Color.Green, Color.Transparent) { drawText = $"{healed}" });
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
        if (modules[_module].isFailed)
        {
            count--;
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
        modules[0].Health = (int)MathF.Max(0, modules[0].Health - cachedDamage / 2);
    }
    public void Stealth()
    {
        cachedDamage /= 3;
    }
    public void Reflective()
    {
        if (Engine.Random.Next(0, 3) == 0)
        {
            cachedDamage = 0;
            for (float angle = 0; angle < MathF.Tau; angle += MathF.PI / 3)
            {
                Engine.EntityManager.Add(new AssassinShot(position, Engine.ToUnitVector(angle) * 8, angle, 0, isFriendly, 6, 1));
            }
        }
    }
    public void StandardEngine()
    {
        if (isEngineActive)
        {
            engineTime = Math.Clamp(engineTime + Engine.DeltaSeconds, 0, 1);
            float engineTimeModifier = 1 - (1 - engineTime) * (1 - engineTime);
            engineParticles.sprayAngle = angle + MathF.PI;
            float fuseRatio = (float)(CountFuses(ModuleType.Engines)) / 3;
            engineParticles.speedOfEmission = 450f * fuseRatio * engineTimeModifier;
            engineParticles.particleColor = Color.Cyan;
            engineParticles.particleFadeToColor = new Color(72, 61, 139, 0);
            if (direction != Vector2.Zero)
            {
                velocity += Vector2.Normalize(direction) * 24 * Engine.DeltaSeconds * engineTimeModifier * fuseRatio / (leashedMaterials.Count + 2);
            }
        }
        else
        {
            engineTime = Math.Clamp(engineTime - Engine.DeltaSeconds * 2, 0, 1);
        }
    }
    public void PlasmaEngine()
    {
        if (isEngineActive)
        {
            engineTime = Math.Clamp(engineTime + Engine.DeltaSeconds * 2, 0, 1);
            float engineTimeModifier = 1 - (1 - engineTime) * (1 - engineTime);
            float fuseRatio = (float)(CountFuses(ModuleType.Engines)) / 3;
            /*
            engineParticles.sprayAngle = angle * 180 / MathF.PI + 180;
            engineParticles.speedOfEmission = 450f * fuseRatio * engineTimeModifier;
            engineParticles.particleColor = Color.Cyan;
            engineParticles.particleFadeToColor = new Color(1f, 0.5f, 0, 0);
            */
            engineParticles.speedOfEmission = 0;
            Vector2 dir = -Engine.ToUnitVector(angle + Engine.OneToNegOne() / 20);
            for (float i = 0; i < 5 * fuseRatio * engineTimeModifier; i++)
            {
                float lerp = i / (5 * fuseRatio * engineTimeModifier);
                Vector3 color = new Vector3(0, 1, 1) * (1 - lerp) + new Vector3(1, 0.5f, 0) * (lerp);
                ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), position + dir * (i + 2.5f) * 4, angle, new Color(color.X, color.Y, color.Z) * (1 - lerp)));
            }
            if (direction != Vector2.Zero)
            {
                velocity += Vector2.Normalize(direction) * 20 * Engine.DeltaSeconds * engineTimeModifier * fuseRatio / (leashedMaterials.Count + 1);
            }
        }
        else
        {
            engineTime = Math.Clamp(engineTime - Engine.DeltaSeconds, 0, 1);
        }
    }
    public void WorkEngine()
    {
        if (isEngineActive)
        {
            engineTime = Math.Clamp(engineTime + Engine.DeltaSeconds / 3, 0, 1);
            float engineTimeModifier = 1 - (1 - engineTime) * (1 - engineTime);
            engineParticles.sprayAngle = angle + MathF.PI;
            float fuseRatio = (float)(CountFuses(ModuleType.Engines)) / 3;
            engineParticles.speedOfEmission = 450f * fuseRatio * engineTimeModifier;
            engineParticles.particleColor = Color.Orange;
            engineParticles.particleFadeToColor = new Color(1f, 0.1f, 0, 0);
            if (direction != Vector2.Zero)
            {
                velocity += Vector2.Normalize(direction) * 14 * Engine.DeltaSeconds * engineTimeModifier * fuseRatio;
            }
        }
        else
        {
            engineTime = Math.Clamp(engineTime - Engine.DeltaSeconds, 0, 1);
        }
    }
    public void Basic()
    {
        if (!modules[ModuleType.Guns].IsCooldownReady())
        {
            return;
        }
        Engine.EntityManager.Add(new PulseShot(position, IdealSpeedWithVelocity(9), gunAngle.angle, 0, true, 3, true));
        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
        modules[ModuleType.Guns].cooldown = 0.25f;
        Engine.ShakeScreen(0.2f);
        velocity -= targetVector / 4;
    }
    public void Sniper()
    {
        if (!modules[ModuleType.Guns].IsCooldownReady())
        {
            return;
        }
        Engine.EntityManager.Add(new AssassinShot(position, IdealSpeedWithVelocity(20), gunAngle.angle, 0, true, 20) { texture = Assets.Get(Sprite.Arrow) });
        SoundManager.PlaySound(Assets.Get(Sound.SniperFire), position);
        modules[ModuleType.Guns].cooldown = 2f;
        Engine.ShakeScreen(0.3f);
        velocity -= targetVector / 2;
    }
    public void Antimaterial()
    {
        if (!modules[ModuleType.Guns].IsCooldownReady())
        {
            return;
        }
        List<Entity> entities = Engine.EntityManager.Hitscan(position, targetVector, 3000, true, out Vector2 _);
        foreach (var entity in entities)
        {
            entity.Collide(30);
        }
        SoundManager.PlaySound(Assets.Get(Sound.SniperFire), position);
        modules[ModuleType.Guns].cooldown = 4f;
        Engine.ShakeScreen(0.5f);
        velocity -= targetVector * 3;
        for (int i = 0; i < 300; i++)
        {
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), 2, position + targetVector * 4 * i, Vector2.Zero, gunAngle.angle, 0, Color.Red, Color.Transparent));
        }
    }
    public void Spiral()
    {
        if (!modules[ModuleType.Guns].IsCooldownReady())
        {
            return;
        }
        Vector2 speed = IdealSpeedWithVelocity(12);
        Engine.EntityManager.Add(new SpiralShot(position, speed, gunAngle.angle, 0, true, 5, false, 1));
        Engine.EntityManager.Add(new SpiralShot(position, speed, gunAngle.angle, 0, true, 5, true, 1));
        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
        modules[ModuleType.Guns].cooldown = 0.7f;
        Engine.ShakeScreen(0.2f);
    }
    public void Shotgun()
    {
        if (!modules[ModuleType.Guns].IsCooldownReady())
        {
            return;
        }
        int randomBulletCount = Engine.Random.Next(4, 6);
        for (int i = 0; i < randomBulletCount; i++)
        {
            float angleDegrees = (Engine.Random.NextSingle() - 0.5f) * 5;
            float offsetAngle = angleDegrees * MathF.PI / 180;
            Vector2 targetVector = IdealSpeedWithVelocity(8) + new Vector2(Engine.OneToNegOne(), Engine.OneToNegOne());
            Vector2 positionOffset = Engine.ToUnitVector(gunAngle.angle + MathF.PI/2) * offsetAngle * 100;
            Engine.EntityManager.Add(new PulseShot(position + positionOffset, targetVector, gunAngle.angle + offsetAngle, 0, true, 2));
        }
        SoundManager.PlaySound(Assets.Get(Sound.ShotgunFire), position);
        velocity -= targetVector / 2;
        modules[ModuleType.Guns].cooldown = 1f;
        Engine.ShakeScreen(0.4f);
    }
    public void Missile()
    {
        if (!modules[ModuleType.Guns].IsCooldownReady())
        {
            return;
        }
        Engine.EntityManager.Add(Enemy.NewMissile(position + Engine.ToUnitVector(gunAngle.angle + MathF.PI / 2) * 6, IdealSpeedWithVelocity(9), gunAngle.angle, true));
        SoundManager.PlaySound(Assets.Get(Sound.MissileFire), position);
        modules[ModuleType.Guns].cooldown = 1.5f;
        velocity -= targetVector / 4;
        Engine.ShakeScreen(0.3f);
    }
    public void LMG()
    {
        if (!modules[ModuleType.Guns].IsCooldownReady())
        {
            return;
        }
        Vector2 offset = Engine.ToUnitVector(gunAngle.angle + MathF.PI / 2) * Engine.Random.Next(-2, 3);
        Texture2D dot = Assets.Get(Sprite.Microshot);
        Projectile shot = new PulseShot(position + offset, IdealSpeedWithVelocity(8) + offset / 4, gunAngle.angle, 0, true, 2)
        {
            texture = dot,
            timeLeft = 3
        };
        Engine.EntityManager.Add(shot);
        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), position);
        Engine.ShakeScreen(0.01f);
        velocity -= targetVector / 8;
        modules[ModuleType.Guns].cooldown = 0.15f;
    }
    public void Silenced()
    {
        if (!modules[ModuleType.Guns].IsCooldownReady())
        {
            return;
        }
        Vector2 offset = Engine.ToUnitVector(gunAngle.angle + MathF.PI / 2) * Engine.Random.Next(-2, 3);
        Texture2D dot = Assets.Get(Sprite.CrossbowShot);
        Projectile shot = new PulseShot(position + offset, IdealSpeedWithVelocity(8) + offset / 4, gunAngle.angle, 0, true, 8, false, 1)
        {
            texture = dot,
        };
        Engine.EntityManager.Add(shot);
        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), position);
        Engine.ShakeScreen(0.2f);
        velocity -= targetVector / 4;
        modules[ModuleType.Guns].cooldown = 0.5f;
    }
    public void Flamethrower()
    {
        if (!modules[ModuleType.Guns].IsCooldownReady())
        {
            return;
        }
        Engine.EntityManager.Add(new FlameBolt(position, IdealSpeedWithVelocity(5) + new Vector2(Engine.OneToNegOne(), Engine.OneToNegOne()) / 4, true, 1));
        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), position);
        modules[ModuleType.Guns].cooldown = 0.08f;
        Engine.ShakeScreen(0.02f);
    }
    public void Fireball()
    {
        if (!modules[ModuleType.Guns].IsCooldownReady())
        {
            return;
        }
        Engine.EntityManager.Add(new FlameBolt(position, IdealSpeedWithVelocity(8) + new Vector2(Engine.OneToNegOne(), Engine.OneToNegOne()) / 2, true, 8, 4, 0.5f));
        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), position);
        modules[ModuleType.Guns].cooldown = 0.8f;
        Engine.ShakeScreen(0.1f);
    }
    public void GrenadeLauncher()
    {
        if (!modules[ModuleType.Guns].IsCooldownReady())
        {
            return;
        }
        Engine.EntityManager.Add(new Explosive(position, IdealSpeedWithVelocity(8) + new Vector2(Engine.OneToNegOne(), Engine.OneToNegOne()), gunAngle.angle, Engine.OneToNegOne() / 8, true, 16, 40, 1));
        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
        modules[ModuleType.Guns].cooldown = 1f;
        Engine.ShakeScreen(0.3f);
        velocity -= targetVector / 2;
    }
    public void Spewer()
    {
        if (!modules[ModuleType.Guns].IsCooldownReady())
        {
            return;
        }
        Engine.EntityManager.Add(new Spewer(position, IdealSpeedWithVelocity(4) + new Vector2(Engine.OneToNegOne(), Engine.OneToNegOne()) / 2, gunAngle.angle, Engine.OneToNegOne() / 8, true, 2));
        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
        modules[ModuleType.Guns].cooldown = 1f;
        Engine.ShakeScreen(0.3f);
        velocity -= targetVector / 2;
    }
    public void Triangle()
    {
        if (!modules[ModuleType.Guns].IsCooldownReady())
        {
            return;
        }
        Vector2 vel = IdealSpeedWithVelocity(8) - velocity;
        var dir = Vector2.Normalize(vel);
        velocity += dir;
        var offset = new Vector2(dir.Y, -dir.X);
        Engine.EntityManager.Add(new PulseShot(position, vel + velocity, gunAngle.angle, 0, true, 6));
        Engine.EntityManager.Add(new PulseShot(position, -vel + offset * 5 + velocity, gunAngle.angle, 0, true, 10) { texture = Assets.Get(Sprite.Explosive) });
        Engine.EntityManager.Add(new PulseShot(position, -vel - offset * 5 + velocity, gunAngle.angle, 0, true, 10) { texture = Assets.Get(Sprite.Explosive) });
        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
        modules[ModuleType.Guns].cooldown = 0.5f;
        Engine.ShakeScreen(0.3f);
    }
    public void PrismArray()
    {
        Vector2 dir = Engine.ToUnitVector(gunAngle.angle);
        List<Entity> enemies = Engine.EntityManager.Hitscan(position, dir, 250, true, out Vector2 _end);
        for (float i = 0; i < (_end - position - dir * 10).Length() / 5; i++)
        {
            float lerp = i / 50;
            Vector3 color = new Vector3(0, 1, 1) * (1 - lerp) + new Vector3(1, 1, 0) * (lerp);
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), dir * (i + 2f) * 5 + position + new Vector2(dir.Y, -dir.X) * MathF.Sin(i / 2 - time * 5) / 2, gunAngle.angle, new Color(color.X, color.Y, color.Z) * (1 - (lerp))));
        }
        if (!modules[ModuleType.Guns].IsCooldownReady())
        {
            return;
        }
        modules[ModuleType.Guns].cooldown = 0.1f;
        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), position);
        foreach (var enemy in enemies)
        {
            enemy.Collide(1);
        }
    }
    public void MatrixLauncher()
    {
        if (!modules[ModuleType.Guns].IsCooldownReady())
        {
            return;
        }
        Vector2 vel = IdealSpeedWithVelocity(15);
        Engine.EntityManager.Add(new FlameBolt(position, vel + new Vector2(Engine.OneToNegOne(), Engine.OneToNegOne()) / 2, true, 10,
            new ParticleEmitter(Assets.Get(Sprite.Circle), position, 0, Color.Cyan) { sprayCone = MathF.PI * 2 / 3, sprayAngle = Engine.ToAngle(vel - velocity), speedOfEmission = 0.5f }, 4));
        SoundManager.PlaySound(Assets.Get(Sound.SniperFire), position);
        modules[ModuleType.Guns].cooldown = 1.5f;
        Engine.ShakeScreen(0.5f);
    }
    public void Dash()
    {
        if (!modules[ModuleType.Core].IsCooldownReady())
        {
            return;
        }
        invincibilityCooldown = 0.5f;
        Vector2 normalVector = new(MathF.Sin(gunAngle.angle), -MathF.Cos(gunAngle.angle));
        for (int i = 0; i < 200; i++)
        {
            float timeLeft = ((float)i / 200);
            var col = Color.SlateBlue;
            col.A = 0;
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), timeLeft, position + normalVector*i, velocity * timeLeft, gunAngle.angle, 0, Color.Cyan, col));
        }
        position += normalVector * 200;
        float c = 2f;
        modules[ModuleType.Core].cooldown = c;
        abilityMaxCooldown = c;
    }
    public void SummonShield()
    {
        if (!modules[ModuleType.Core].IsCooldownReady())
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
        float c = 5f;
        modules[ModuleType.Core].cooldown = c;
        abilityMaxCooldown = c;
    }
    public void SummonGrapplingHook()
    {
        if ((abilityEntity == null || abilityEntity.isExpired) && modules[ModuleType.Core].IsCooldownReady())
        {
            abilityEntity = new GrapplingHook(position, IdealSpeedWithVelocity(50), gunAngle.angle, this);
            SoundManager.PlaySound(Assets.Get(Sound.Click), position);
            Engine.ShakeScreen(0.3f);
            velocity -= targetVector / 2;
            Engine.EntityManager.Add(abilityEntity);
            float c = 5f;
            modules[ModuleType.Core].cooldown = c;
            abilityMaxCooldown = c;
        }
        else if(abilityEntity != null && !abilityEntity.isExpired)
        {
            abilityEntity.isExpired = true;
            modules[ModuleType.Core].cooldown /= 2;
        }
    }
    public void Nanomachines()
    {
        if (modules[ModuleType.Core].IsCooldownReady())
        {
            foreach (var pickup in leashedMaterials)
            {
                if (pickup is not Module and not Construct)
                {
                    pickup.isExpired = true;
                    Collide(-10);
                    var c = 30;
                    modules[ModuleType.Core].cooldown = c;
                    abilityMaxCooldown = c;
                    return;
                }
            }
        }
    }
    public void CreateFighter()
    {
        if (modules[ModuleType.Core].IsCooldownReady())
        {
            foreach (var pickup in leashedMaterials)
            {
                if (pickup is not Module and not Construct)
                {
                    pickup.isExpired = true;
                    Engine.EntityManager.Add(Enemy.NewAdvancedFighter(position, velocity, angle, isFriendly));
                    var c = 60;
                    modules[ModuleType.Core].cooldown = c;
                    abilityMaxCooldown = c;
                    return;
                }
            }
        }
    }
    public void Lidar()
    {
        if (modules[ModuleType.Sensors].IsCooldownReady())
        {
            Vector2 dir = targetVector + new Vector2(Engine.OneToNegOne(), Engine.OneToNegOne()) / 5;
            Engine.EntityManager.Hitscan(position, dir, 1000, false, out Vector2 end);
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), 1f, end, Vector2.Zero, 0, 0, Color.White, Color.Transparent));
        }
    }
    public void Radar()
    {
        if (modules[ModuleType.Sensors].IsCooldownReady())
        {
            int fuses = CountFuses(ModuleType.Sensors);
            Vector2 dir = Engine.ToUnitVector(time * (float)(fuses) / 3);
            List<Entity> revealedEntities = Engine.EntityManager.Hitscan(position, dir, 2000, true, out Vector2 end);
            foreach (var entity in revealedEntities)
            {
                entity.Reveal(2f);
            }
            if (revealedEntities.Count > 0)
            {
                SoundManager.PlaySound(Assets.Get(Sound.Beep), position);
            }
            for (int i = 0; i < 10; i++)
            {
                ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), position + dir * 10 * i, 0, Color.Green * (1 - (float)(i) / 10)));
            }
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), end, 0, Color.White));
        }
    }
    public void PulseEmitter()
    {
        if (modules[ModuleType.Sensors].IsCooldownReady())
        {
            Engine.EntityManager.NearestEnemy(this)?.Reveal(1);
            Engine.EntityManager.NearestProjectile(this, isFriendly)?.Reveal(1);
            modules[ModuleType.Sensors].cooldown = 2;
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Beep));
        }
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        if (Progression <= -1)
        {
            return;
        }
        if (position.Length() > Engine.SaveGame.CurrentMission.Planet.radius + 25 * 50)
        {
            _spriteBatch.Draw(Assets.Get(Sprite.Arrow), position - Vector2.Normalize(position) * 25, null, color, MathF.Atan2(-position.X, position.Y), Assets.DimsOf(Sprite.Arrow) / 2, 1, 0, 0.2f);
            _spriteBatch.DrawString(Assets.TextFont, "Return to planet.", Engine.Camera.Position - new Vector2(Assets.TextFont.MeasureString("Return to planet.").X/2, 225), Color.Crimson);
        }
        if(dockedEntity != null)
        {
            return;
        }
        base.Draw(_spriteBatch);
        Vector2 linePosition = position + new Vector2(-texture.Width * 2, texture.Height * 1.5f) / 2;
        Rectangle sourceRectangle = new (0, 0, texture.Width * 2, 2);

        if (modules[ModuleType.Hull].ID == 1)
        {
            Engine.DrawFilledLine(_spriteBatch, linePosition, sourceRectangle, (1 - modules[0].cooldown / 8), Color.DarkGray, Color.Yellow);
        }
    }
    public Player(string _serialization, LoadLogger _logger)
    : base(Assets.Get(Sprite.Player), Vector2.One, Vector2.One, 0, 0, 5, true)
    {
        List<string> disassembly = SaveGame.Disassemble(_serialization);

        spareFuses = Int32.TryParse(disassembly[0], out int spares) ? spares : 1;
        aimAssist = !Boolean.TryParse(disassembly[1], out bool assist) || assist;
        var fuses = SaveGame.Disassemble(disassembly[2]);
        for (int i = 0; i < moduleFuses.LongLength; i++)
        {
            if (Boolean.TryParse(fuses[i], out bool fuse))
            {
                moduleFuses[i / 4, i % 4] = fuse;
            }
        }
        _logger.Try(delegate { modules[ModuleType.Hull] = (Module)(ItemFactory.TryDeserialize(disassembly[3], _logger)); }, 3);
        _logger.Try(delegate { modules[ModuleType.Guns] = (Module)(ItemFactory.TryDeserialize(disassembly[4], _logger)); }, 4);
        _logger.Try(delegate { modules[ModuleType.Engines] = (Module)(ItemFactory.TryDeserialize(disassembly[5], _logger)); }, 5);
        _logger.Try(delegate { modules[ModuleType.Sensors] = (Module)(ItemFactory.TryDeserialize(disassembly[6], _logger)); }, 6);
        _logger.Try(delegate { modules[ModuleType.Core] = (Module)(ItemFactory.TryDeserialize(disassembly[7], _logger)); }, 7);

        gunAngle = Enemy.NewDummyEnemy(position, true);
        color = new Color(0, 255, 0);
        smokeParticles.isEmitterActive = false;
        engineParticles.isEmitterActive = false;
        engineSounds = Assets.Get(Sound.FireEngines).CreateInstance();
        engineSounds.IsLooped = true;
        var textures = new Texture2D[modules.Count];
        for (int i = 0; i < modules.Count; i++)
        {
            textures[i] = modules[(ModuleType)i].Texture;
        }
        EventHandler.SetFuseModuleDecals(textures);
        EventHandler.UpdateFuseUI(moduleFuses, spareFuses);
    }
    public string Serialize()
    {
        var fuses = new StringBuilder();
        foreach (var fuse in moduleFuses)
        {
            fuses.Append($"{fuse},");
        }
        fuses.Remove(fuses.Length - 1, 1);
        var modules = new StringBuilder();
        foreach (var module in this.modules)
        {
            modules.Append($"{module.Value.Serialize()},");
        }
        modules.Remove(modules.Length - 1, 1);
        return $"{{{spareFuses},{aimAssist},{{{fuses}}},{modules}}}";
    }
}