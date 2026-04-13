using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Space_Wars.Content.Main.Components;
using Space_Wars.Content.Main.MissionComponents;
using Space_Wars.Content.Main.Particles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UILib.Content.Main;

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
        { ModuleType.Hull, ItemFactory.moduleData[UI.setModules[0]].Retrieve()},
        { ModuleType.Guns, ItemFactory.moduleData[UI.setModules[1]].Retrieve() },
        { ModuleType.Engines, ItemFactory.moduleData[UI.setModules[2]].Retrieve() },
        { ModuleType.Sensors, ItemFactory.moduleData[UI.setModules[3]].Retrieve() },
        { ModuleType.Core, ItemFactory.moduleData[UI.setModules[4]].Retrieve() }
    };
    public Module SecondaryWeapon { get; set; } = null;
    private Vector2 startLocation = Vector2.Zero;
    public Vector2 Direction => targetVector;
    public DockableComponent dockedEntity;
    public List<Pickup> leashedMaterials = [];
    private ParticleEmitter smokeParticles = new(Assets.Get(Sprites.Circle), 1f, Vector2.Zero, 0, MathF.PI/4, 1, 0.5f, Color.Gray, EmitterType.EmissionOverTime) { isEmitterActive = false, particleFadeToColor = new Color(169, 169, 169, 0) };
    private SoundEffectInstance engineSounds;
    public float invincibilityCooldown = 0;
    public float cachedDamage = 0;
    private float restartCooldown = 0;
    public bool IsRestarting { get; private set; } = false;
    public bool isEngineActive = false;
    public bool canGatherResources = false;
    private Vector2 targetVector;
    public Vector2 direction;
    private float cachedDamageCooldown = 0;
    public int Progression { get; set; } = 3;
    public bool IsEnabled { get; set; } = true;
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
            if (RevealDuration > 0)
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
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), Position, 0, Color.Red));
                }
            }
            if (modules[ModuleType.Guns].Cooldown > 0)
            {
                stealth -= 1;
            }
            if (modules[ModuleType.Hull].Type is Modules.Stealth)
            {
                stealth += 2;
            }
            if (modules[ModuleType.Hull].Type is Modules.Turtle)
            {
                stealth += 1;
            }
            return stealth;
        }
    }
    public Player(Vector2 _position, Vector2 _velocity, float _angle)
        : base(_position, _velocity, _angle, 0)
    {
        AddComponent(new StatusHolder(this));
        AddComponent(new Sprite(this) { Texture = Assets.Get(Sprites.Player) });
        AddComponent(new Friendly(this) { Team = Team.Friendly });
        UpdateColor();
        smokeParticles.isEmitterActive = false;
        engineSounds = Assets.Get(Sound.FireEngines).CreateInstance();
        engineSounds.IsLooped = true;
        var textures = new Texture2D[modules.Count];
        for(int i = 0; i < modules.Count; i++)
        {
            textures[i] = modules[(ModuleType)i].Texture;
        }
        AddComponent(new Collide(this, PlayerCollide));
        EventHandler.SetFuseModuleDecals(textures);
        EventHandler.UpdateFuseUI(moduleFuses, spareFuses);
    }
    public static Player NewPlayer(Vector2 _position, Vector2 _velocity, float _angle)
    {
        return new Player(_position, _velocity, _angle);
    }
    public override void UpdateColor()
    {
        Color = SaveGame.ColorScheme.TeamColors[Team];
    }
    public override void Update()
    {
        Color c = SaveGame.ColorScheme.TeamColors[Team];
        if (Color != c)
        {
            float l = Util.FIED(0.025f);
            Color = new Color((byte)(Color.R * l + c.R * (1f - l)), (byte)(Color.G * l + c.G * (1f - l)), (byte)(Color.B * l + c.B * (1f - l)));
        }
        if (modules[ModuleType.Core].Health <= 0)
        {
            isExpired = true;
            engineSounds.Stop();
            Assets.Get(Sound.Death).Play();
            return;
        }
        smokeParticles.position = Position;
        leashedMaterials = [.. leashedMaterials.Where(x => !x.isExpired)];
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
                IsRestarting = true;
                SoundManager.PlaySound(Assets.Get(Sound.Interact), Position);
            }
        }
        if (IsRestarting && restartCooldown <= 0)
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
                }
            }
            if (restartedModules)
            {
                IsRestarting = false;
                SoundManager.PlaySound(Assets.Get(Sound.Full), Position);
            }
        }
        else if (IsRestarting)
        {
            restartCooldown -= Engine.DeltaSeconds;
        }
        if (invincibilityCooldown > 0)
        {
            invincibilityCooldown -= Engine.DeltaSeconds;
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
                //Guaranteed death sped along
                if (cachedDamage > 100)
                {
                    cachedDamageCooldown = 0;
                }
            }
        }
        else
        {
            cachedDamageCooldown -= Engine.DeltaSeconds;
        }
        float currentHealth = modules[ModuleType.Hull].Health + modules[ModuleType.Guns].Health + modules[ModuleType.Engines].Health + modules[ModuleType.Sensors].Health + modules[ModuleType.Core].Health;
        float maxHealth = modules[ModuleType.Hull].MaxHealth + modules[ModuleType.Guns].MaxHealth + modules[ModuleType.Engines].MaxHealth + modules[ModuleType.Sensors].MaxHealth + modules[ModuleType.Core].MaxHealth;

        UI.PlayerHealth.SetInterval(currentHealth - cachedDamage, maxHealth, 0);
        UI.PlayerHealth.SetInterval(currentHealth + cachedDamageCooldown / 0.05f, maxHealth, 1);
        Vector3 colorVec;
        float val = (MathF.Sin(Engine.Time) + 1f) / 2;
        colorVec = new Vector3(1, 0, 0) * val + new Vector3(1, 0.2f, 0.2f) * (1f - val);
        UI.PlayerHealth.Colors[0] = new Color(colorVec.X, colorVec.Y, colorVec.Z);

        //Only displays if the player has abilities unlocked
        if (Progression > 1 || SaveGame.DebugMode) 
        {
            colorVec = new Vector3(0, 1, 1) * val + new Vector3(0.2f, 1, 0.8f) * (1f - val);
            UI.PlayerAbility.Colors[0] = new Color(colorVec.X, colorVec.Y, colorVec.Z);
            UI.PlayerAbility.Colors[1] = Color.DarkGray;
        }
        else
        {
            UI.PlayerAbility.Colors[0] = Color.Transparent;
            UI.PlayerAbility.Colors[1] = Color.Transparent;
        }
        LowerCooldown();
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
        if (Position.Length() >= 40 * 50 + planet.radius * 2)
        {
            Velocity *= Util.FIED(0.1f);
            Velocity += Vector2.Normalize(-Position) * (Position.Length() - (40 * 50 + planet.radius*2)) * Engine.DeltaSeconds / 30;
        }
        base.Update();
        if (dockedEntity != null)
        {
            if (!(dockedEntity == null) && !dockedEntity.Entity.isExpired)
            {
                Position = dockedEntity.Entity.GetComponent<Transform>().Position;
                Velocity = dockedEntity.Entity.GetComponent<Transform>().Velocity;
            }
            else
            {
                dockedEntity = null;
            }
        }
        UI.PlayerSpecialHealth.Colors[0] = Color.Transparent;
        UI.PlayerSpecialHealth.Colors[1] = Color.Transparent;

        if (Progression < 0)
        {
            isEngineActive = false;
        }
        var mousePos = new Vector2(Input.NewMouseState.X, Input.NewMouseState.Y);
        //Ensures that target vector performs identically in all resolutions
        Vector2 mouseCamPos = Engine.Camera.Position + mousePos - Engine.BackBuffer/2 + Engine.MousePositionOffset;
        //Testing
        //ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), mouseCamPos, 0, Color.White));
        //ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), position, 0, Color.White));
        targetVector = Vector2.Normalize(mouseCamPos - Position);
    }
    public void LowerCooldown()
    {
        if (SecondaryWeapon != null && Util.Random.NextSingle() < 0.25f)
        {
            SecondaryWeapon.OnUpdate();
        }
        for (int i = 0; i < modules.Count; i++)
        {
            var module = modules[(ModuleType)i];
            //Square root of the ratio reduces impact with additional fuse (especially with weapon dps)
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
        if(SaveGame.DebugMode)
        {
            Vector2 mousePos = new Vector2(Input.NewMouseState.X, Input.NewMouseState.Y) + Engine.Camera.Position - Engine.BackBuffer/2;
            mousePos = new Vector2(MathF.Round(mousePos.X/25), MathF.Round(mousePos.Y/25))*25;
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), mousePos, 0, Color.Red));
            if (startLocation != Vector2.Zero)
            {
                float f = 1;
                if(Input.NewState.IsKeyDown(Keys.LeftControl))
                {
                    f = 0.5f;
                }
                float angle = MathF.Atan2((startLocation.Y - mousePos.Y), (startLocation.X - mousePos.X)) - MathF.PI / 2;
                Vector2 dir = Util.ToUnitVector(angle);
                for (float d = 0; d < (startLocation - mousePos).Length() / 4; d += 2)
                {
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), startLocation + dir * d * 4, angle, Color.White*f));
                }
            }
            var comp = Engine.SaveGame.CurrentMission.GetComponent<Colliders>();
            if(Input.IsDown(Binding.WarpBackward) && comp.GetColliders.Length > 0)
            {
                Vector2 newPos = new Vector2(Input.NewMouseState.X, Input.NewMouseState.Y) + Engine.Camera.Position - Engine.BackBuffer / 2;
                Vector2 prevPos = new Vector2(Input.OldMouseState.X, Input.OldMouseState.Y) + Engine.Camera.Position - Engine.BackBuffer / 2;
                comp.GetColliders = [.. comp.GetColliders.Where(x => !x.IsColliding(prevPos, newPos - prevPos, 10, true, out float _))];   
            }
            if(Input.NewState.IsKeyDown(Keys.F) && Input.OldState.IsKeyUp(Keys.F))
            {
                if(startLocation == Vector2.Zero)
                {
                    startLocation = mousePos;
                }
                else
                {
                    if(comp != null)
                    {
                        comp.GetColliders =
                        [
                            .. comp.GetColliders,
                            new LineCollider(startLocation, mousePos,Input.NewState.IsKeyDown(Keys.LeftControl)),
                        ];   
                    }
                    else
                    {
                        comp = new Colliders(delegate() { return [new LineCollider(startLocation, mousePos)];});
                        Engine.SaveGame.CurrentMission.AddComponent(comp);
                    }
                    startLocation = Vector2.Zero;
                }
            }
            if (Input.NewState.IsKeyDown(Keys.Tab) && Input.OldState.IsKeyUp(Keys.Tab))
            {
                if(comp != null)
                {
                    foreach(var collider in comp.GetColliders)
                    {
                        Debug.WriteLine(collider.Print());
                    }   
                }
            }
        }
        //Prevents undocking when in the garage menu
        if (Progression > -1 && IsEnabled)
        {
            if (Input.WasJustPressed(Binding.OpenPanel))
            {
                EventHandler.ToggleDockingMenus();
            }
            if (Input.WasJustPressed(Binding.SwapPrimary))
            {
                if (SecondaryWeapon != null)
                {
                    SoundManager.PlayGlobalSound(Assets.Get(Sound.Click));
                    (modules[ModuleType.Guns], SecondaryWeapon) = (SecondaryWeapon, modules[ModuleType.Guns]);
                    EventHandler.UpdateModulesUI();
                }
                else
                {
                    SoundManager.PlayGlobalSound(Assets.Get(Sound.Fail));
                }
            }
            if (Progression > 1 && Input.WasJustPressed(Binding.ToggleAimAssist))
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
                    if (Input.IsDown(Binding.Construct))
                    {
                        float dist = (new Vector2(Input.NewMouseState.X, Input.NewMouseState.Y) - Engine.BackBuffer / 2).Length();
                        var constructs = new List<(String description, Texture2D sprite)>()
                    {
                        ("Req. 1 scrap, blocks enemy fire. 20 integrity.", Assets.Get(Sprites.Barricade)),
                        ("Req. 1 scrap, attacks enemies. 8 integrity.", Assets.Get(Sprites.Trap)),
                        ("Req. 1 scrap, 100 dmg to all in radius when destroyed. 3 integrity.", Assets.Get(Sprites.Bomb)),
                        ("Req. 1 scrap, smelts all scrap within it", Assets.Get(Sprites.Furnace))                    
                    };
                        if (Progression > 3)
                        {
                            constructs.Add(("Req. 3 scrap, deployable garage. Use metal to upgrade.", Assets.Get(Sprites.Mothership)));
                        }
                        if (Engine.SaveGame.CurrentMission.Name == "???")
                        {
                            constructs.Add(("1 scrap to construct. Be ready.", Assets.Get(Sprites.QuantumResonator)));
                        }
                        float angle = 0;
                        Color color;
                        for (float i = 0; i < constructs.Count; i++)
                        {
                            Vector2 dir = Util.ToUnitVector(angle);
                            Vector2 mouseDir = targetVector;
                            if (dir.X * mouseDir.X + dir.Y * mouseDir.Y > (1f - 0.9f / constructs.Count) && dist > 300)
                            {
                                color = Color.White;
                                ParticleManager.Add(new Particle(null, new Vector2(0, -100) + Position, 0, Color.White) { drawText = constructs[(int)i].description });
                            }
                            else
                            {
                                color = Color.Cyan;
                            }
                            ParticleManager.Add(new Particle(constructs[(int)i].sprite, dir * 45 + Position, 0, color));
                            angle += MathF.Tau / constructs.Count;
                        }
                    }
                    else if (Input.WasJustReleased(Binding.Construct))
                    {
                        float dist = (new Vector2(Input.NewMouseState.X, Input.NewMouseState.Y) - Engine.BackBuffer / 2).Length();
                        int scrapCount = 0;
                        Entity firstScrap = null;
                        foreach (var pickup in leashedMaterials)
                        {
                            if (pickup is not Module)
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
                            "Bomb",
                            "Furnace"
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
                            if (dir.X * mouseDir.X + dir.Y * mouseDir.Y > (1f - 0.9f / types.Count) && dist > 300 && firstScrap != null)
                            {
                                switch (types[i])
                                {
                                    case "Barricade":
                                        firstScrap.isExpired = true;
                                        var barricade = Pickup.NewBarricade(firstScrap.Position, firstScrap.Velocity, 0, 0);
                                        leashedMaterials.Add(barricade);
                                        Engine.EntityManager.Add(barricade);
                                        break;
                                    case "Trap":
                                        firstScrap.isExpired = true;
                                        var trap = Pickup.NewTrap(firstScrap.Position, firstScrap.Velocity, 0, 0);
                                        leashedMaterials.Add(trap);
                                        Engine.EntityManager.Add(trap);
                                        break;
                                    case "Bomb":
                                        firstScrap.isExpired = true;
                                        var bomb = Pickup.NewBomb(firstScrap.Position, firstScrap.Velocity, 0, 0);
                                        leashedMaterials.Add(bomb);
                                        Engine.EntityManager.Add(bomb);
                                        break;
                                    case "Furnace":
                                        firstScrap.isExpired = true;
                                        var furnace = Pickup.NewFurnace(firstScrap.Position, firstScrap.Velocity, 0, 0);
                                        leashedMaterials.Add(furnace);
                                        Engine.EntityManager.Add(furnace);
                                        break;
                                    case "Mothership":
                                        if (scrapCount >= 3)
                                        {
                                            foreach (var pickup in leashedMaterials)
                                            {
                                                pickup.isExpired = true;
                                            }
                                            leashedMaterials.Clear();
                                            Engine.EntityManager.Add(Enemy.NewMakeshiftMothership(Position, Velocity, 0));
                                        }
                                        break;
                                    case "Resonator":
                                        //firstScrap.isExpired = true;
                                        Engine.EntityManager.Add(Enemy.NewQuantumResonator(Position));
                                        break;
                                }
                            }
                            angle += MathF.PI * 2 / (float)(types.Count);
                        }
                    }
                }
                if (!UIManager.LockMouseInput && Input.NewMouseState.RightButton == ButtonState.Pressed)
                {
                    Vector2 targetDir = targetVector;
                    if(aimAssist)
                    {
                        Entity nearestEnemy = Engine.EntityManager.NearestEnemy(this, true);
                        if(nearestEnemy != null && (nearestEnemy as Enemy).Health <= 0)
                        {
                            var relativePos = Vector2.Normalize(nearestEnemy.Position - Position);
                            if (Vector2.Dot(relativePos, targetVector) > 0.9f)
                            {
                                targetDir = relativePos;
                            }
                        }
                    }
                    List<Entity> miningEnemies = Engine.EntityManager.Hitscan(Position, targetDir, 120, false, out Vector2 _end);
                    Engine.WriteLine(_end.Length());
                    foreach(var entity in miningEnemies)
                    {
                        if(entity as Enemy != null)
                        {
                            (entity as Enemy).Mine();
                        }
                    }
                    for (float i = 0; i < (_end - Position - targetVector * 8).Length() / 2; i++)
                    {
                        float lerp = i / 60;
                        Vector3 color = new Vector3(1, 1, 0) * (1 - lerp) + new Vector3(1, 0, 0) * (lerp);
                        ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), targetDir * (i + 4f) * 2 + Position + new Vector2(targetDir.Y, -targetDir.X) * MathF.Sin(i / 2 - Engine.Time * 5) / 2, Util.ToAngle(targetDir), new Color(color.X, color.Y, color.Z) * (1 - (lerp))));
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
                if (Input.WasJustPressed(Binding.DropScrap))
                {
                    leashedMaterials = [];
                }
                if ((Progression > 1 || SaveGame.DebugMode) && Input.WasJustPressed(Binding.Ability))
                {
                    foreach (var module in modules)
                    {
                        module.Value.OnAbility();
                    }
                }
                Keys[] pressedKey = Input.NewState.GetPressedKeys();
                direction = Vector2.Zero;
                isEngineActive = false;
                var directions = new Dictionary<Binding, Vector2>
                {
                    { Binding.Up, new Vector2(0, -1) },
                    { Binding.Left, new Vector2(-1, 0) },
                    { Binding.Down, new Vector2(0, 1) },
                    { Binding.Right, new Vector2(1, 0) }
                };
                foreach (var pair in directions)
                {
                    if (Input.IsDown(pair.Key))
                    {
                        direction += pair.Value;
                    }
                }
                isEngineActive = (direction.X != 0 || direction.Y != 0);
                if (isEngineActive)
                {
                    foreach (var module in modules)
                    {
                        module.Value.OnEngine();
                    }
                }
                if (isEngineActive)
                {
                    Angle = Angle * 0.5f + Util.ToAngle(targetVector) * 0.5f; //Better shield aiming
                }
                else
                {
                    Angle = Angle * 0.5f + Util.ToAngle(targetVector) * 0.5f;
                }
                if (Input.NewMouseState.LeftButton == ButtonState.Pressed && !UIManager.LockMouseInput)
                {
                    foreach (var module in modules)
                    {
                        module.Value.OnShoot();
                    }
                }
            }
            if (Input.WasJustPressed(Binding.Dock))
            {
                Dock();
            }
            smokeParticles.offsetVelocity = Velocity;
            if (Util.Random.Next(0, 2) == 0)
            {
                smokeParticles.Update();
            }
        }
        if (EventHandler.AcknowledgeMessage(Message.ToggleTerminal))
        {
            if (dockedEntity != null)
            {
                if(dockedEntity.Menu != null)
                {
                    dockedEntity.Menu.enabled = !dockedEntity.Menu.enabled;
                }
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
            Vector2 acc = Engine.SaveGame.CurrentMission.GetNormalizedAcceleration(Position + targetVector * _speed * 3) * 180 / _speed;
            Vector2 vel = Velocity - acc;
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
        return targetVector * _speed + Velocity;
    }
    public void Dock(bool _withVelocity = true)
    {
        if (dockedEntity == null)
        {
            DockableComponent dockableEntity = Engine.EntityManager.NearestDockableEntity(this);
            if (dockableEntity != null)
            {
                if (dockableEntity.Dock(this, _withVelocity))
                {
                    dockedEntity = dockableEntity;
                    isEngineActive = false;
                }
            }
        }
        else if (dockedEntity.Dock(this, _withVelocity))
        {
            dockedEntity = null;
            isEngineActive = false;
        }
    }
    public bool PlayerCollide(int _damage, bool _ignoreImmunity = false)
    {
        if (dockedEntity != null)
        {
            //Note: applied statuses will NOT apply to the docked entity
            dockedEntity.Collide(_damage);
            return false;
        }
        if (!_ignoreImmunity)
        {
            foreach (var module in modules)
            {
                _damage = module.Value.OnCollide(_damage);
            }
        }
        _damage = StatusHolder.ModifyDamage(_damage);
        if (_damage > 0 && (invincibilityCooldown <= 0 || _ignoreImmunity))
        {
            Flash(Color.White);
            Engine.ShakeScreen(0.08f * (float)(_damage));
            //Helps to cushion huge hits
            //Player will never be one shot (unless they deserve it)
            cachedDamage += Math.Min(50, _damage);
            SoundManager.PlaySound(Assets.Get(Sound.Hit), Position);
            if (!_ignoreImmunity) 
            {
                invincibilityCooldown = 1;
            }
            ParticleManager.Add(new Particle(null, 1, Position + new Vector2(0, -1), new Vector2(0, -1.5f), 0, 0, Color.Red, Color.Transparent) { drawText = $"{_damage}" });
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
                    ParticleManager.Add(new Particle(null, 2, Position + new Vector2(0, -3), new Vector2(0, -0.75f), 0, 0, Color.Red, Color.Transparent) { drawText = "Fuse damaged!" });
                    SoundManager.PlaySound(Assets.Get(Sound.Beep), Position);
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
                    ParticleManager.Add(new Particle(null, 2, Position + new Vector2(0, -3), new Vector2(0, -0.75f), 0, 0, Color.Red, Color.Transparent) { drawText = $"{failedPart} has failed!" });
                    SoundManager.PlaySound(Assets.Get(Sound.Beep), Position);
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
            SoundManager.PlaySound(Assets.Get(Sound.Full), Position);
            ParticleManager.Add(new Particle(null, 1, Position + new Vector2(0, -1), new Vector2(0, -1.5f), 0, 0, Color.Green, Color.Transparent) { drawText = $"{healed}" });
            return true;
        }
        return false;
    }
    public void ToggleFuse(int x, int y)
    {
        bool fuse = moduleFuses[x, y];
        if ((UI.Fuses[y, x].daughterItem == null) != fuse)
        {
            return;
        }
        moduleFuses[x, y] = !fuse;
        EventHandler.UpdateFuseUI(moduleFuses, spareFuses);
    }
    public void AddFuse()
    {
        spareFuses++;
        EventHandler.UpdateFuseUI(moduleFuses, spareFuses);
    }
    public void UpdateSpares()
    {
        spareFuses = UI.FuseCounter.Count;
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
        if(dockedEntity != null)
        {
            return;
        }
        StatusHolder.Draw(_spriteBatch, this);
        base.Draw(_spriteBatch);
    }
    public Player(string _serialization, LoadLogger _logger)
    : base(Vector2.One, Vector2.One, 0, 0)
    {
        throw new NotImplementedException();
        AddComponent(new StatusHolder(this));
        AddComponent(new Sprite(this) { Texture = Assets.Get(Sprites.Player)});
        AddComponent(new Friendly(this) { Team = Team.Friendly });
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
        _logger.Try(delegate 
        {
            if (disassembly[8] != "null")
            {
                SecondaryWeapon = (Module)(ItemFactory.TryDeserialize(disassembly[8], _logger));
            }
            else
            {
                SecondaryWeapon = null;
            }
        }, 8);

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
        if (SecondaryWeapon != null)
        {
            modules.Append($"{SecondaryWeapon.Serialize()}");
        }
        else
        {
            modules.Append("null");
        }
        return $"{{{spareFuses},{aimAssist},{{{fuses}}},{modules}}}";
    }
}