using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OrbitCommander.Components;
using OrbitCommander.MissionComponents;
using OrbitCommander.Particles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UILib.Content;
using OrbitCommander.Core;

namespace OrbitCommander.Entities;

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
        { true, true, true, false },
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

    public Dockable dockedEntity;
    public List<Pickup> leashedMaterials = [];
    private ParticleEmitter smokeParticles = new(Assets.Get(Sprites.Circle), 1f, Vector2.Zero, 0, MathF.PI / 4, 1, 0.5f, Color.Gray, EmitterType.EmissionOverTime) { isEmitterActive = false, particleFadeToColor = new Color(169, 169, 169, 0) };
    private float swapCd, cachedDamageCd = 0;
    public float invincibilityCd, restartCd = 0;
    public int cachedDamage = 0;
    public Vector2 Direction { get; private set; }
    private SoundEffectInstance engineSounds;
    //IsEnabled manages dead file sprite
    public bool IsEnabled { get; set; } = true;
    public bool IsDocked => dockedEntity != null;
    public bool isEngineActive = false;
    public bool canGatherResources = false;
    public Vector2 EngineDirection { get; private set; }
    private Vector2 startLocation = Vector2.Zero;
    public int Progression { get; set; } = 3;
    public override int SensingAbility
    {
        get
        {
            if (modules[ModuleType.Sensors] == null)
            {
                return -1;
            }
            int sensing = 1 + Statuses.SensingChange;
            foreach(var module in modules)
            {
                sensing += module.Value.SensingChange();
            }
            float x = (float)CountFuses(ModuleType.Sensors) - 2;
            //Fuse modifiers: 0 = -2, 1 = -1, 2 or 3 = 0, 4 = +1
            return sensing + (int)Math.Floor(x * x * x / 5);
        }
    }
    public override int StealthAbility
    {
        get
        {
            if (RevealDuration > 0)
            {
                return -99;
            }
            int stealth = Statuses.StealthChange;
            foreach (var module in modules)
            {
                stealth += module.Value.StealthChange();
            }
            if (isEngineActive)
            {
                stealth -= 1;
            }
            return stealth;
        }
    }
    public Player(Vector2 _position, Vector2 _velocity, float _angle)
        : base(_position, _velocity, _angle, 0)
    {
        AddComponent(new Stealth(this));
        AddComponent(new Temp());
        AddComponent(new Statuses(this));
        AddComponent(new Friendly(this) { Team = Team.Friendly });
        AddComponent(new Sprite(this, SaveGame.ColorScheme.TeamColors[Team]) { Texture = Assets.Get(Sprites.Player) });
        smokeParticles.isEmitterActive = false;
        engineSounds = Assets.Get(Sound.FireEngines).CreateInstance();
        engineSounds.IsLooped = true;
        var textures = new Texture2D[modules.Count];
        for (int i = 0; i < modules.Count; i++)
        {
            textures[i] = (modules[(ModuleType)i] as IData).Texture;
        }
        AddComponent(new Collide(this, PlayerCollide));
        Events.SetFuseModuleDecals(textures);
        Events.UpdateFuseUI(moduleFuses, spareFuses);
    }
    public Player(string _serialization, LoadLogger _logger)
        : base(Vector2.One, Vector2.One, 0, 0)
    {
        var serialization = SaveGame.Disassemble(_serialization);
        AddComponent(new Stealth(this));
        AddComponent(new Temp());
        AddComponent(new Statuses(this));
        AddComponent(new Friendly(this) { Team = Team.Friendly });
        AddComponent(new Sprite(this, SaveGame.ColorScheme.TeamColors[Team]) { Texture = Assets.Get(Sprites.Player) });
        smokeParticles.isEmitterActive = false;
        engineSounds = Assets.Get(Sound.FireEngines).CreateInstance();
        engineSounds.IsLooped = true;
        AddComponent(new Collide(this, PlayerCollide));
        Int32.TryParse(serialization[0], out int _fuses);
        spareFuses = _fuses;
        bool.TryParse(serialization[1], out bool _assist);
        aimAssist = _assist;
        var fuses = SaveGame.Disassemble(serialization[2]);
        for(int i = 0; i < 5; i++)
        {
            for(int j = 0; j < 4; j++)
            {
                if(bool.TryParse(fuses[i * 4 + j], out bool _result))
                {
                    moduleFuses[i, j] = _result;
                }
                else
                {
                    throw new ArgumentException("Player loading failed: Invalid fuses");
                }
            }
        }
        for(int i = 0; i < 5; i++)
        {
            modules[(ModuleType)i] = (Module)ItemFactory.TryDeserialize(serialization[i + 3], _logger);
        }
        SecondaryWeapon = (Module)ItemFactory.TryDeserialize(serialization[8], _logger);
        var textures = new Texture2D[modules.Count];
        for (int i = 0; i < modules.Count; i++)
        {
            textures[i] = (modules[(ModuleType)i] as IData).Texture;
        }
        Events.SetFuseModuleDecals(textures);
        Events.UpdateFuseUI(moduleFuses, spareFuses);
    }
    public override void Update()
    {
        GetComponent<Sprite>().TargetColor = SaveGame.ColorScheme.TeamColors[Team];
        if (modules[ModuleType.Core].Health <= 0)
        {
            isExpired = true;
            engineSounds.Stop();
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Death));
            return;
        }
        leashedMaterials = [.. leashedMaterials.Where(x => !x.isExpired)];
        if(restartCd <= 0 && Events.AcknowledgeMessage(Message.RestartModules) && modules.Any(x => x.Value.isFailed))
        {
            restartCd = 1.5f;
        }
        else if(restartCd > 0)
        {
            restartCd -= Engine.DeltaSeconds;
            if(restartCd <= 0)
            {
                var module = modules.Values.Last(x => x.isFailed);
                module.isFailed = false;
                //Small bonus to module health to keep players alive in firefights
                module.Health = Math.Min(module.MaxHealth, module.Health + Util.Random.Next(1, 4));
                if (modules.Any(x => x.Value.isFailed))
                {
                    restartCd = 1.5f;
                    SoundManager.PlaySound(Assets.Get(Sound.Interact), Position);
                }
                else
                {
                    SoundManager.PlaySound(Assets.Get(Sound.Full), Position);
                }
            }
        }
        if (invincibilityCd > 0)
        {
            invincibilityCd -= Engine.DeltaSeconds;
        }
        if(swapCd > 0)
        {
            swapCd -= Engine.DeltaSeconds;
            if(swapCd <= 0)
            {
                SoundManager.PlayGlobalSound(Assets.Get(Sound.Click));
                (modules[ModuleType.Guns], SecondaryWeapon) = (SecondaryWeapon, modules[ModuleType.Guns]);
                Events.UpdateModulesUI();
                swapCd = 0;
            }
        }
        if(cachedDamageCd > 0)
        {
            cachedDamageCd -= Engine.DeltaSeconds;
        }
        else if(cachedDamage > 0)
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
            cachedDamageCd = 0.05f;
            //Guaranteed death sped along
            if (cachedDamage > 100)
            {
                cachedDamageCd = 0.0166f;
            }
        }

        float currentHealth = modules.Values.Sum(x => x.Health);
        float maxHealth = modules.Values.Sum(x => x.MaxHealth);
        UI.PlayerHealth.SetInterval(currentHealth - cachedDamage, maxHealth, 0);
        UI.PlayerHealth.SetInterval(currentHealth + cachedDamageCd / 0.05f, maxHealth, 1);

        float lerp = (MathF.Sin(Engine.Time) + 1f) / 2;
        Vector3 colorVec = new Vector3(1, 0, 0) * lerp + new Vector3(1, 0.2f, 0.2f) * (1f - lerp);
        UI.PlayerHealth.Colors[0] = new Color(colorVec.X, colorVec.Y, colorVec.Z);

        //Only displays if the player has abilities unlocked
        if (Progression > 1 || SaveGame.DebugMode)
        {
            colorVec = new Vector3(0, 1, 1) * lerp + new Vector3(0.2f, 1, 0.8f) * (1f - lerp);
            UI.PlayerAbility.Colors[0] = new Color(colorVec.X, colorVec.Y, colorVec.Z);
            UI.PlayerAbility.Colors[1] = Color.DarkGray;
        }
        else
        {
            UI.PlayerAbility.Colors[0] = Color.Transparent;
            UI.PlayerAbility.Colors[1] = Color.Transparent;
        }
        var temp = Temperature;
        if(temp > 0)
        {
            UI.Thermometer.Colors[1] = Color.Orange * temp;
            UI.Thermometer.SetInterval(0.5f, 1, 0);
            UI.Thermometer.SetInterval(temp/2 + 0.5f, 1, 1);
        }
        else
        {
            UI.Thermometer.Colors[1] = Color.Cyan * -temp;
            UI.Thermometer.SetInterval((1+temp)/2, 1, 0);
            UI.Thermometer.SetInterval(0.5f, 1, 1);
        }
        LowerCooldown();
        if (currentHealth > 50)
        {
            smokeParticles.isEmitterActive = false;
        }
        else
        {
            smokeParticles.isEmitterActive = true;
            smokeParticles.speedOfEmission = 25f - currentHealth / 4;
        }
        base.Update();
        if (dockedEntity != null)
        {
            if (IsDocked && !dockedEntity.Entity.isExpired)
            {
                Position = dockedEntity.Entity.GetComponent<Transform>().Position;
                Velocity = dockedEntity.Entity.GetComponent<Transform>().Velocity;
            }
            else
            {
                dockedEntity = null;
            }
        }

        if (Progression < 0)
        {
            isEngineActive = false;
        }
        var mousePos = new Vector2(Input.NewMouseState.X, Input.NewMouseState.Y);
        //Ensures that target vector performs identically in all resolutions
        Vector2 mouseCamPos = Engine.Camera.Position + mousePos - Engine.BackBuffer / 2 + Engine.MousePositionOffset;
        //Testing
        //ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), mouseCamPos, 0, Color.White));
        //ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), position, 0, Color.White));
        Direction = Vector2.Normalize(mouseCamPos - Position);
    }
    public void LowerCooldown()
    {
        if (SecondaryWeapon != null && Util.Random.NextSingle() < 0.25f)
        {
            SecondaryWeapon.OnUpdate(1);
        }
        for (int i = 0; i < modules.Count; i++)
        {
            var module = modules[(ModuleType)i];
            //Square root of the ratio reduces impact with additional fuse (especially with weapon dps)
            float fuseRatio = MathF.Sqrt((float)CountFuses((ModuleType)i) / 3);
            module.OnUpdate(fuseRatio);
        }
    }
    public void OnEnemyHit(Entity _entity, int _damage)
    {
        foreach(var module in modules)
        {
            module.Value.OnEnemyHit(_entity, _damage);
        }
    }
    public Entity ModifyProjectile(Entity _projectile)
    {
        _projectile.Temperature = Temperature;
        return _projectile;
    }
    public void RestrictedActions()
    {
        if (SaveGame.DebugMode)
        {
            Vector2 mousePos = new Vector2(Input.NewMouseState.X, Input.NewMouseState.Y) + Engine.Camera.Position - Engine.BackBuffer / 2;
            mousePos = new Vector2(MathF.Round(mousePos.X / 25), MathF.Round(mousePos.Y / 25)) * 25;
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), mousePos, 0, Color.Red));
            if (startLocation != Vector2.Zero)
            {
                float f = 1;
                if (Input.NewState.IsKeyDown(Keys.LeftControl))
                {
                    f = 0.5f;
                }
                float angle = MathF.Atan2(startLocation.Y - mousePos.Y, startLocation.X - mousePos.X) - MathF.PI / 2;
                Vector2 dir = Util.ToUnitVector(angle);
                for (float d = 0; d < (startLocation - mousePos).Length() / 4; d += 2)
                {
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), startLocation + dir * d * 4, angle, Color.White * f));
                }
            }
            var comp = Engine.SaveGame.CurrentMission.GetComponent<Colliders>();
            if (Input.IsDown(Binding.WarpBackward) && comp.GetColliders.Length > 0)
            {
                Vector2 newPos = new Vector2(Input.NewMouseState.X, Input.NewMouseState.Y) + Engine.Camera.Position - Engine.BackBuffer / 2;
                Vector2 prevPos = new Vector2(Input.OldMouseState.X, Input.OldMouseState.Y) + Engine.Camera.Position - Engine.BackBuffer / 2;
                comp.GetColliders = [.. comp.GetColliders.Where(x => !x.IsColliding(prevPos, newPos - prevPos, 10, true, out float _))];
            }
            if (Input.NewState.IsKeyDown(Keys.F) && Input.OldState.IsKeyUp(Keys.F))
            {
                if (startLocation == Vector2.Zero)
                {
                    startLocation = mousePos;
                }
                else
                {
                    if (comp != null)
                    {
                        comp.GetColliders =
                        [
                            .. comp.GetColliders,
                            new LineCollider(startLocation, mousePos,Input.NewState.IsKeyDown(Keys.LeftControl)),
                        ];
                    }
                    else
                    {
                        comp = new Colliders(delegate () { return [new LineCollider(startLocation, mousePos)]; });
                        Engine.SaveGame.CurrentMission.Add(comp);
                    }
                    startLocation = Vector2.Zero;
                }
            }
            if (Input.NewState.IsKeyDown(Keys.Tab) && Input.OldState.IsKeyUp(Keys.Tab))
            {
                if (comp != null)
                {
                    foreach (var collider in comp.GetColliders)
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
                Events.ToggleDockingMenus();
            }
            if (Input.WasJustPressed(Binding.SwapPrimary))
            {
                if (SecondaryWeapon != null)
                {
                    swapCd = 0.5f;
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
                        var constructs = new List<(string description, Texture2D sprite)>()
                    {
                        ("Req. 1 scrap, blocks enemy fire. 20 integrity.", Assets.Get(Sprites.Barricade)),
                        ("Req. 1 scrap, attacks enemies. 8 integrity.", Assets.Get(Sprites.Trap)),
                        ("Req. 1 scrap, 100 dmg to all in radius when destroyed. 3 integrity.", Assets.Get(Sprites.Bomb)),
                        ("Req. 1 scrap, smelts all scrap within it", Assets.Get(Sprites.Furnace)),
                        ("Req. 1 scrap, throw at enemies to do damage.", Assets.Get(Sprites.Explosive))
                        };
                        if (Progression > 3)
                        {
                            constructs.Add(("Req. 3 scrap, deployable garage. Use metal to upgrade.", Assets.Get(Sprites.Mothership)));
                        }
                        if (Mission.missions[Engine.SaveGame.CurrentMissionIndex].data.Name == "???")
                        {
                            constructs.Add(("1 scrap to construct. Be ready.", Assets.Get(Sprites.QuantumResonator)));
                        }
                        float angle = 0;
                        Color color;
                        for (float i = 0; i < constructs.Count; i++)
                        {
                            Vector2 dir = Util.ToUnitVector(angle);
                            Vector2 mouseDir = Direction;
                            if (dir.X * mouseDir.X + dir.Y * mouseDir.Y > 1f - 0.9f / constructs.Count && dist > 300)
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
                            "Furnace",
                            "Mace"
                        };
                        if (Progression > 3)
                        {
                            types.Add("Mothership");
                        }
                        if (Mission.missions[Engine.SaveGame.CurrentMissionIndex].data.Name == "???")
                        {
                            types.Add("Resonator");
                        }
                        for (int i = 0; i < types.Count; i++)
                        {
                            Vector2 dir = Util.ToUnitVector(angle);
                            if (dir.X * Direction.X + dir.Y * Direction.Y > 1f - 0.9f / types.Count && dist > 300 && firstScrap != null)
                            {
                                switch (types[i])
                                {
                                    case "Barricade":
                                        firstScrap.isExpired = true;
                                        var barricade = Pickup.NewBarricade(firstScrap.Position, firstScrap.Velocity, 0, 0);
                                        if (modules[ModuleType.Engines] is WorkEngine)
                                        {
                                            barricade.AddTag(Tags.IsImmune);
                                        }
                                        leashedMaterials.Add(barricade);
                                        Engine.SaveGame.CurrentMission.Add(barricade);
                                        break;
                                    case "Trap":
                                        firstScrap.isExpired = true;
                                        var trap = Pickup.NewTrap(firstScrap.Position, firstScrap.Velocity, 0, 0);
                                        if (modules[ModuleType.Engines] is WorkEngine)
                                        {
                                            trap.AddTag(Tags.IsImmune);
                                        }
                                        leashedMaterials.Add(trap);
                                        Engine.SaveGame.CurrentMission.Add(trap);
                                        break;
                                    case "Bomb":
                                        firstScrap.isExpired = true;
                                        var bomb = Pickup.NewBomb(firstScrap.Position, firstScrap.Velocity, 0, 0);
                                        if (modules[ModuleType.Engines] is WorkEngine)
                                        {
                                            bomb.AddTag(Tags.IsImmune);
                                        }
                                        leashedMaterials.Add(bomb);
                                        Engine.SaveGame.CurrentMission.Add(bomb);
                                        break;
                                    case "Furnace":
                                        firstScrap.isExpired = true;
                                        var furnace = Pickup.NewFurnace(firstScrap.Position, firstScrap.Velocity, 0, 0);
                                        if (modules[ModuleType.Engines] is WorkEngine)
                                        {
                                            furnace.AddTag(Tags.IsImmune);
                                        }
                                        leashedMaterials.Add(furnace);
                                        Engine.SaveGame.CurrentMission.Add(furnace);
                                        break;
                                    case "Mothership":
                                        if (scrapCount >= 3)
                                        {
                                            foreach (var pickup in leashedMaterials)
                                            {
                                                pickup.isExpired = true;
                                            }
                                            leashedMaterials.Clear();
                                            Engine.SaveGame.CurrentMission.Add(NewMakeshiftMothership(Position, Velocity, 0));
                                        }
                                        break;
                                    case "Resonator":
                                        firstScrap.isExpired = true;
                                        Engine.SaveGame.CurrentMission.Add(NewQuantumResonator(Position));
                                        break;
                                    case "Mace":
                                        firstScrap.isExpired = true;
                                        var mace = Pickup.NewMace(firstScrap.Position, firstScrap.Velocity, 0, 0);
                                        if (modules[ModuleType.Engines] is WorkEngine)
                                        {
                                            mace.AddTag(Tags.IsImmune);
                                        }
                                        leashedMaterials.Add(mace);
                                        Engine.SaveGame.CurrentMission.Add(mace);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            angle += MathF.PI * 2 / types.Count;
                        }
                    }
                }
                if (Input.NewMouseState.RightButton == ButtonState.Pressed)
                {
                    Vector2 targetDir = Direction;
                    if (aimAssist)
                    {
                        Entity nearestEnemy = Util.Nearest(Position, Engine.SaveGame.CurrentMission.GetEntities<Health>());
                        if (nearestEnemy != null && nearestEnemy.Health <= 0)
                        {
                            var relativePos = Vector2.Normalize(nearestEnemy.Position - Position);
                            if (Vector2.Dot(relativePos, Direction) > 0.9f)
                            {
                                targetDir = relativePos;
                            }
                        }
                    }
                    List<Entity> miningEnemies = Engine.SaveGame.CurrentMission.Hitscan(Position, targetDir, 120, false, out Vector2 _end);
                    foreach (var entity in miningEnemies)
                    {
                        entity.Mine();
                    }
                    for (float i = 0; i < (_end - Position - Direction * 8).Length() / 2; i++)
                    {
                        float lerp = i / 60;
                        Vector3 color = new Vector3(1, 1, 0) * (1 - lerp) + new Vector3(1, 0, 0) * lerp;
                        ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), targetDir * (i + 4f) * 2 + Position + new Vector2(targetDir.Y, -targetDir.X) * MathF.Sin(i / 2 - Engine.Time * 5) / 2, Util.ToAngle(targetDir), new Color(color.X, color.Y, color.Z) * (1 - lerp)));
                    }
                    if (Input.OldMouseState.RightButton == ButtonState.Released)
                    {
                        canGatherResources = true;
                        SoundManager.PlayGlobalSound(Assets.Get(Sound.OpenMenu));
                    }
                }
                if (Input.NewMouseState.RightButton == ButtonState.Released && Input.OldMouseState.RightButton == ButtonState.Pressed)
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
                EngineDirection = Vector2.Zero;
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
                        EngineDirection += pair.Value;
                    }
                }
                isEngineActive = EngineDirection.X != 0 || EngineDirection.Y != 0;
                if (isEngineActive)
                {
                    foreach (var module in modules)
                    {
                        module.Value.OnEngine();
                    }
                }
                if (isEngineActive)
                {
                    Angle = Angle * 0.5f + Util.ToAngle(Direction) * 0.5f; //Better shield aiming
                }
                else
                {
                    Angle = Angle * 0.5f + Util.ToAngle(Direction) * 0.5f;
                }
                if (Input.NewMouseState.LeftButton == ButtonState.Pressed && swapCd <= 0)
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
            smokeParticles.position = Position;
            smokeParticles.offsetVelocity = Velocity;
            if (Util.Random.Next(0, 2) == 0)
            {
                smokeParticles.Update();
            }
        }
        if (Events.AcknowledgeMessage(Message.ToggleTerminal))
        {
            if (dockedEntity != null)
            {
                if (dockedEntity.Menu != null)
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
            Vector2 acc = Engine.SaveGame.CurrentMission.GetNormalizedAcceleration(Position + Direction * _speed * 3) * 180 / _speed;
            Vector2 vel = Velocity - acc;
            float b = Direction.X * vel.X + Direction.Y * vel.Y;
            float c = vel.X * vel.X + vel.Y * vel.Y - _speed * _speed;
            float disc = b * b - c;
            if (disc >= 0)
            {
                float t = b + MathF.Sqrt(disc);
                if (t > 0)
                {
                    return Direction * t + acc;
                }
            }
        }
        return Direction * _speed + Velocity;
    }
    public bool Dock(bool _withVelocity = true, bool _silent = false)
    {
        Dockable dockableEntity = Engine.SaveGame.CurrentMission.NearestDockableEntity(this);
        if (dockableEntity == null || Vector2.DistanceSquared(Position, dockableEntity.Entity.Position) > 1250)
        {
            return false;
        }
        Events.DisableDockingMenus();
        if (dockedEntity != null)
        {
            dockedEntity = null;
            isEngineActive = false;
            if (_withVelocity)
            {
                Velocity += new Vector2(0, -2);
            }
            if(!_silent)
            {
                SoundManager.PlayGlobalSound(Assets.Get(Sound.Undock));
            }
            if (Engine.UIManager.selectedIcon is Pickup pickup)
            {
                Engine.UIManager.selectedIcon = null;
                Events.UpdateInventoryUI();
                pickup.isExpired = false;
                pickup.Position = dockableEntity.Entity.Position;
                leashedMaterials.Add(pickup);
                Engine.SaveGame.CurrentMission.Add(pickup);
            }
        }
        else
        {
            dockedEntity = dockableEntity;
            isEngineActive = false;
            if(!_silent)
            {
                Events.ToggleDockingMenus();
                SoundManager.PlayGlobalSound(Assets.Get(Sound.Dock));
            }
            if (dockedEntity.HasInventory)
            {
                for (int i = 0; i < leashedMaterials.Count; i++)
                {
                    //Launches the leashed material away if the docking module cannot store it
                    leashedMaterials[i].Velocity += Engine.SaveGame.CurrentMission.GetNormalizedAcceleration(leashedMaterials[i].Position) * 15;
                    bool isFull = true;
                    for (int j = 0; j < Engine.SaveGame.Inventory.Length; j++)
                    {
                        if (Engine.SaveGame.Inventory[j] == null)
                        {
                            isFull = false;
                            break;
                        }
                    }
                    if (isFull)
                    {
                        continue;
                    }
                    Dockable.AddItem(leashedMaterials[i]);
                    leashedMaterials[i].isExpired = true;
                }
                leashedMaterials.Clear();
                Events.UpdateScrapText();
            }
        }
        Engine.ShakeScreen(0.35f);
        return true;
    }
    public int PlayerCollide(int _damage, bool _ignoreImmunity = false)
    {
        if (dockedEntity != null)
        {
            //Note: applied statuses will NOT apply to the docked entity
            dockedEntity.Collide(_damage);
            return 0;
        }
        if (!_ignoreImmunity)
        {
            foreach (var module in modules)
            {
                _damage = module.Value.OnCollide(_damage);
            }
        }
        _damage = Statuses.ModifyDamage(_damage);
        if(restartCd > 0)
        {
            _damage = (int)(_damage*0.5f);
        }
        if (_damage > 0 && (invincibilityCd <= 0 || _ignoreImmunity))
        {
            ApplyWork(_damage);
            Flash(Color.White);
            Engine.ShakeScreen(0.08f * _damage);
            //Helps to cushion huge hits
            //Player will never be one shot when at high health
            cachedDamage += Math.Min(50, _damage);
            SoundManager.PlaySound(Assets.Get(Sound.Hit), Position);
            if (!_ignoreImmunity)
            {
                invincibilityCd = 1;
            }
            ParticleManager.Add(new Particle(null, 1, Position + new Vector2(0, -1), new Vector2(0, -1.5f), 0, 0, Color.Red, Color.Transparent) { drawText = $"{_damage}" });
            //Part and Fuse Failure
            if (Progression > 0 && restartCd <= 0)
            {
                //If a module is failed, further collisions damage fuses
                var targetFuse = new Vector2(Util.Random.Next(0, moduleFuses.GetLength(0)), Util.Random.Next(0, moduleFuses.GetLength(1)));
                var failedPart = (ModuleType)Util.Random.Next(0, 4);
                float threshold = 1 - 1 / _damage;
                if (Util.Random.NextSingle() < threshold && modules[(ModuleType)targetFuse.X].isFailed && moduleFuses[(int)targetFuse.X, (int)targetFuse.Y])
                {
                    moduleFuses[(int)targetFuse.X, (int)targetFuse.Y] = false;
                    ParticleManager.Add(new Particle(null, 2, Position + new Vector2(0, -3), new Vector2(0, -0.75f), 0, 0, Color.Red, Color.Transparent) { drawText = "Fuse damaged!" });
                    SoundManager.PlaySound(Assets.Get(Sound.Beep), Position);
                    Events.UpdateFuseUI(moduleFuses, spareFuses);
                }
                else if (Util.Random.Next(0, 12) == 0)
                {
                    if (modules[failedPart].isFailed)
                    {
                        failedPart = ModuleType.Core;
                        if (modules[ModuleType.Core].isFailed)
                        {
                            return _damage;
                        }
                    }
                    modules[failedPart].isFailed = true;
                    ParticleManager.Add(new Particle(null, 2, Position + new Vector2(0, -3), new Vector2(0, -0.75f), 0, 0, Color.Red, Color.Transparent) { drawText = $"{failedPart} has failed!" });
                    SoundManager.PlaySound(Assets.Get(Sound.Beep), Position);
                    Events.UpdateModulesStatus();
                }
            }
            return _damage;
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
        }
        return 0;
    }
    public void ToggleFuse(int x, int y)
    {
        bool fuse = moduleFuses[x, y];
        if (UI.Fuses[y, x].daughterItem == null != fuse)
        {
            return;
        }
        moduleFuses[x, y] = !fuse;
        Events.UpdateFuseUI(moduleFuses, spareFuses);
    }
    public void AddFuse()
    {
        spareFuses++;
        Events.UpdateFuseUI(moduleFuses, spareFuses);
    }
    public void UpdateSpares()
    {
        spareFuses = UI.FuseCounter.Count;
    }
    public int CountFuses(ModuleType _module)
    {
        int count = Statuses.FuseBonus;
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
                    count += fuse && moduleFuses[(int)ModuleType.Core, i] ? 1 : 0;
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
        if (dockedEntity != null)
        {
            return;
        }
        if(swapCd > 0)
        {
            for(float i = 0; i < (0.5f - swapCd) * 200; i++)
            {
                float angle = i / 100 * MathF.Tau;
                _spriteBatch.Draw(Assets.Get(Sprites.Dot), Util.ToUnitVector(angle) * 30 + Position, null, Color.Green, angle, Assets.DimsOf(Sprites.Dot), 1, 0, 0);
            }
        }
        if (Engine.SaveGame.CurrentMission.GetAtmospherePressure(this) > 1f)
        {
            ParticleManager.Add(new Particle(null, Position + new Vector2(0, 50), 0, Color.Red) { drawText = "Danger: Pressure Alert!" });
        }
        Statuses.Draw(_spriteBatch, this);
        for (int i = 0; i < leashedMaterials.Count; i++)
        {
            var prev = i == 0 ? this as Entity : leashedMaterials[i - 1];
            float d = Vector2.Distance(prev.Position, leashedMaterials[i].Position);
            Vector2 relativePosition = leashedMaterials[i].Position - prev.Position;
            Vector2 relativeVelocity = leashedMaterials[i].Velocity - prev.Velocity;
            for (float f = prev.ColliderRadius / 2; f < d - leashedMaterials[i].ColliderRadius / 2; f += 6)
            {
                float lerp = f / d;
                ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), prev.Position + relativePosition * lerp + MathF.Sqrt(d) * relativeVelocity * (1 - (2 * lerp - 1) * (2 * lerp - 1)), 0, Color.Cyan * 0.5f));
            }
        }
        base.Draw(_spriteBatch);
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