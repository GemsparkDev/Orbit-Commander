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
        { ModuleType.Hull, new Hull() },
        { ModuleType.Guns, new CrackShot() },
        { ModuleType.Engines, new PlasmaEngine() },
        { ModuleType.Sensors, new Sensors() },
        { ModuleType.Core, new SummonGrapplingHook() }
    };

    public Vector2 Direction => targetVector;
    public DockableComponent dockedEntity;
    public List<Pickup> leashedMaterials = [];
    private ParticleEmitter smokeParticles = new(Assets.Get(Sprite.Circle), 1f, Vector2.Zero, 0, MathF.PI/4, 1, 0.5f, Color.Gray, EmitterType.EmissionOverTime) { isEmitterActive = false, particleFadeToColor = new Color(169, 169, 169, 0) };
    private SoundEffectInstance engineSounds;
    public float invincibilityCooldown = 0;
    public float cachedDamage = 0;
    private float restartCooldown = 0;
    private bool isRestarting = false;
    public bool isEngineActive = false;
    public bool canGatherResources = false;
    private Vector2 targetVector;
    public Vector2 direction;
    private float time = 0;
    private float cachedDamageCooldown = 0;
    public int Progression { get; set; } = 3;
    public override int SensingAbility
    {
        get
        {
            int sensing = 1 + StatusHolder.SensingChange;
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
            int stealth = StatusHolder.StealthChange;
            if (isEngineActive)
            {
                stealth -= 1;
                if (modules[ModuleType.Engines].Type == Modules.Plasma)
                {
                    stealth -= 1;
                }
            }
            if (modules[ModuleType.Guns].Cooldown > 0)
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
        UpdateColor();
        smokeParticles.isEmitterActive = false;
        engineSounds = Assets.Get(Sound.FireEngines).CreateInstance();
        engineSounds.IsLooped = true;
        var textures = new Texture2D[modules.Count];
        for(int i = 0; i < modules.Count; i++)
        {
            textures[i] = modules[(ModuleType)i].Texture;
        }
        EventHandler.SetFuseModuleDecals(textures);
        EventHandler.UpdateFuseUI(moduleFuses, spareFuses);
    }
    public override void UpdateColor()
    {
        color = Engine.ColorScheme.FriendlyEnemy();
    }
    public override void Update()
    {
        UI.PlayerSpecialHealth.enabledColor = Color.Transparent;
        UI.PlayerSpecialHealth.disabledColor = Color.Transparent;
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
            return;
        }
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
                    module.Health = Math.Min(module.MaxHealth, module.Health + Util.Random.Next(1, 4));
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
            EventHandler.UpdateRestartSlider(restart - restartCooldown, restart);
        }
        if (invincibilityCooldown > 0)
        {
            invincibilityCooldown -= Engine.DeltaSeconds;
            color = Engine.ColorScheme.FriendlyEnemy() * (MathF.Cos(invincibilityCooldown * 30) / 2 + 0.5f);
        }
        if (cachedDamageCooldown <= 0)
        {
            if (cachedDamage > 0)
            {
                int randomNumber = Util.Random.Next(1, 4);
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
                cachedDamage--;
                cachedDamageCooldown = 0.05f;
            }
        }
        else
        {
            cachedDamageCooldown -= Engine.DeltaSeconds;
        }
        float currentHealth = modules[ModuleType.Hull].Health + modules[ModuleType.Guns].Health + modules[ModuleType.Engines].Health + modules[ModuleType.Sensors].Health + modules[ModuleType.Core].Health;
        float maxHealth = modules[ModuleType.Hull].MaxHealth + modules[ModuleType.Guns].MaxHealth + modules[ModuleType.Engines].MaxHealth + modules[ModuleType.Sensors].MaxHealth + modules[ModuleType.Core].MaxHealth;

        UI.PlayerHealth.SetInterval(currentHealth + cachedDamageCooldown / 0.05f, maxHealth);
        Vector3 colorVec;
        float val = (MathF.Sin(time) + 1f) / 2;
        colorVec = new Vector3(1, 0, 0) * val + new Vector3(1, 0.2f, 0.2f) * (1f - val);
        UI.PlayerHealth.enabledColor = new Color(colorVec.X, colorVec.Y, colorVec.Z);

        //Only displays if the player has abilities unlocked
        if (Progression > 1) 
        {
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
        LowerCooldown();
        if (Progression < 0)
        {
            isEngineActive = false;
        }
        //Ensures that target vector performs identically in all resolutions
        Vector2 ratio = Engine.ScreenSize / Engine.BackBuffer;
        targetVector = Vector2.Normalize(new Vector2(Input.NewMouseState.X * ratio.X, Input.NewMouseState.Y * ratio.Y) - Engine.ScreenSize / 2 - position + Engine.Camera.Position + Engine.MousePositionOffset);
    }
    public override void LowerCooldown()
    {
        for (int i = 0; i < modules.Count; i++)
        {
            var module = modules[(ModuleType)i];
            //Square root of the ratio reduces balancing impact with an additional fuse (especially with the gun dps)
            //Note: Do not have any active abilities that are based on the cooldown, as the player could remove all fuses and get infinite of the ability
            float fuseRatio = MathF.Sqrt((float)CountFuses((ModuleType)i) / 3);
            if (fuseRatio - 1 > float.Epsilon)
            {
                //Bonus for 4 fuses
                module.OnUpdate();
                //Allows for easy random check in all cases
                fuseRatio -= 1f;
            }
            if (Util.Random.NextSingle() < fuseRatio)
            {
                module.OnUpdate();
            }
        }
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
                            Vector2 dir = Util.ToUnitVector(angle);
                            Vector2 mouseDir = targetVector;
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
                            Vector2 dir = Util.ToUnitVector(angle);
                            Vector2 mouseDir = targetVector;
                            if (dir.X * mouseDir.X + dir.Y * mouseDir.Y > (0.2f * types.Count) && dist > 500 && firstScrap != null)
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
                                        firstScrap.isExpired = true;
                                        Engine.EntityManager.Add(Enemy.NewQuantumResonator(position));
                                        break;
                                }
                            }
                            angle += MathF.PI * 2 / (float)(types.Count);
                        }
                    }
                }
                if (!UIManager.LockMouseInput && Input.NewMouseState.RightButton == ButtonState.Pressed)
                {
                    List<Entity> miningEnemies = Engine.EntityManager.Hitscan(position, targetVector, 120, false, out Vector2 _end);
                    if (miningEnemies.Count > 0 && miningEnemies[0] as Enemy != null)
                    {
                        (miningEnemies[0] as Enemy).Mine();
                    }
                    for (float i = 0; i < (_end - position - targetVector * 8).Length() / 2; i++)
                    {
                        float lerp = i / 60;
                        Vector3 color = new Vector3(1, 1, 0) * (1 - lerp) + new Vector3(1, 0, 0) * (lerp);
                        ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), targetVector * (i + 4f) * 2 + position + new Vector2(targetVector.Y, -targetVector.X) * MathF.Sin(i / 2 - time * 5) / 2, Util.ToAngle(targetVector), new Color(color.X, color.Y, color.Z) * (1 - (lerp))));
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
                    foreach (var module in modules)
                    {
                        module.Value.OnAbility();
                    }
                }
                Keys[] pressedKey = Input.NewState.GetPressedKeys();
                direction = Vector2.Zero;
                isEngineActive = false;
                var directions = new Dictionary<Keys, Vector2>
                {
                    { Keys.W, new Vector2(0, -1) },
                    { Keys.A, new Vector2(-1, 0) },
                    { Keys.S, new Vector2(0, 1) },
                    { Keys.D, new Vector2(1, 0) }
                };
                foreach (var key in pressedKey)
                {
                    if (directions.TryGetValue(key, out Vector2 value))
                    {
                        direction += value;
                        isEngineActive = true;
                    }
                }
                if (isEngineActive)
                {
                    foreach (var module in modules)
                    {
                        module.Value.OnEngine();
                    }
                }
                angle = angle * 0.5f + MathF.Atan2(direction.X, -direction.Y) * 0.5f;
                if (Input.NewMouseState.LeftButton == ButtonState.Pressed && !UIManager.LockMouseInput)
                {
                    if (!isEngineActive)
                    {
                        angle = angle * 0.5f + Util.ToAngle(targetVector) * 0.5f;
                    }
                    foreach (var module in modules)
                    {
                        module.Value.OnShoot();
                    }
                }
            }
            if (Input.OldState.IsKeyUp(Keys.Space) && Input.NewState.IsKeyDown(Keys.Space))
            {
                Dock();
            }
            smokeParticles.offsetVelocity = velocity;
            if (Util.Random.Next(0, 2) == 0)
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
        if (isEngineActive)
        {
            SoundManager.PlayLoopedSound(engineSounds);
        }
    }
    public Vector2 IdealSpeedWithVelocity(float _speed)
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
                }
            }
        }
        else if (dockedEntity.Dock(this))
        {
            dockedEntity = null;
            isEngineActive = false;
        }
    }
    public override bool Collide(int _damage, bool _ignoreImmunity = false)
    {
        if (dockedEntity != null)
        {
            //Note: applied statuses will NOT apply to the docked entity
            dockedEntity.Collide(_damage);
            return false;
        }
        foreach (var module in modules)
        {
            _damage = module.Value.OnCollide(_damage);
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
                var targetFuse = new Vector2(Util.Random.Next(0, moduleFuses.GetLength(0)), Util.Random.Next(0, moduleFuses.GetLength(1)));
                var failedPart = (ModuleType)Util.Random.Next(0, 4);
                float threshold = 1 - 1 / (_damage);
                if (Util.Random.NextSingle() < threshold && modules[(ModuleType)targetFuse.X].isFailed && moduleFuses[(int)targetFuse.X, (int)targetFuse.Y])
                {
                    moduleFuses[(int)targetFuse.X, (int)targetFuse.Y] = false;
                    ParticleManager.Add(new Particle(null, 2, position + new Vector2(0, -3), new Vector2(0, -0.75f), 0, 0, Color.Red, Color.Transparent) { drawText = "Fuse damaged!" });
                    SoundManager.PlaySound(Assets.Get(Sound.Beep), position);
                    EventHandler.UpdateFuseUI(moduleFuses, spareFuses);
                }
                else if (Util.Random.Next(0, 5) == 0 && modules[failedPart].Health < modules[failedPart].MaxHealth / 2)
                {
                    if (modules[failedPart].isFailed)
                    {
                        failedPart = ModuleType.Core;
                        if (modules[ModuleType.Core].isFailed)
                        {
                            return true;
                        }
                    }
                    modules[failedPart].isFailed = true;
                    ParticleManager.Add(new Particle(null, 2, position + new Vector2(0, -3), new Vector2(0, -0.75f), 0, 0, Color.Red, Color.Transparent) { drawText = $"{failedPart} has failed!" });
                    SoundManager.PlaySound(Assets.Get(Sound.Beep), position);
                    EventHandler.UpdateModulesStatus();
                }
            }
            return true;
        }
        else if (_damage < 0)
        {
            int healed = 0;
            while (_damage < 0)
            {
                Module module = modules[(ModuleType)Util.Random.Next(0, 5)];
                if (module.Health < module.MaxHealth)
                {
                    module.Health++;
                    healed++;
                }
                _damage += 1;
            }
            SoundManager.PlaySound(Assets.Get(Sound.Full), position);
            ParticleManager.Add(new Particle(null, 1, position + new Vector2(0, -1), new Vector2(0, -1.5f), 0, 0, Color.Green, Color.Transparent) { drawText = $"{healed}" });
            return true;
        }
        return false;
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
        StatusHolder.Draw(_spriteBatch, this);
        base.Draw(_spriteBatch);
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

        UpdateColor();
        smokeParticles.isEmitterActive = false;
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