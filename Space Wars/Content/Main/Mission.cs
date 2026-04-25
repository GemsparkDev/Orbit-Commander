using Space_Wars.Content.Main.Entities;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using Space_Wars.Content.Main.Particles;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Space_Wars.Content.Main.Story;
using Space_Wars.Content.Main.Components;
using Space_Wars.Content.Main.MissionComponents;
using UILib.Content.Main;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
//using System.Numerics;

namespace Space_Wars.Content.Main;
public class Mission
{
    private static Player Player => Engine.SaveGame.Player;
    //Maximum distance for any detection when sensing = stealth
    public static float StealthRange { get; private set; } = 750;
    //Threshold of detection for enemies
    public static float StealthThreshold { get; private set; } = 0.75f;
    public static readonly List<(MissionData data, Func<Mission> instance)> missions =
    [
        //Enemy planet test
        (new("Enemy Planet Test", "", 200, [], 0, 2000, 0),
        delegate(){Entity p = new Planet(Vector2.Zero, Vector2.Zero, 10000, 8, true, Color.Red, false); 
        return new([
            p.AddComponent(new Health(p) { CurrentHealth = 1000, MaxHealth = 1000 })
             .AddComponent(new Friendly(p) { Team = Team.Hostile })
             .AddComponent(new Stealth(p) { SensingAbility = 1, StealthAbility = 0 })
             .AddComponent(new Collide(p, delegate(int damage, bool _ignoreImmunity)
             {
                 if (damage > 0)
                 {
                    SoundManager.PlaySound(Assets.Get(Sound.ShieldHit), p.Position);
                    p.Health -= damage;
                    Engine.ShakeScreen(10 / ((p.Position - Engine.Camera.Position).Length() + 200) * damage);
                    ParticleManager.Add(new Particle(null, 1, p.Position + new Vector2(0, -1), new Vector2(0, -1.5f), 0, 0, Color.Orange, new Color(255, 0, 0, 0)) { drawText = $"{damage}" });                  }
                 return damage > 0;
            }))],
            new Conditional([new Kill([p])], Mission.SendPickup()),
            new DropSpawner(1500));}),

        (new("Crash Landing","The crash landing site. Objective: Explore the system.",160,[0],0, 2000, 1),
        delegate(){Entity m; return new([
            new Planet(new Vector2(1000, 0), Planet.GetOrbitalVelocity(new Vector2(1000, 0), Vector2.Zero, 10000), 250, 1.5f, false, Color.Cyan),
            new Planet(Vector2.Zero, Vector2.Zero, 10000, 8, true, Color.Cyan, false),
            new IntroCutscene(RestartCutscene),
            new Tip("WASD to move, Space to dock and undock.\nRmb to collect scrap, Lmb to shoot.", new Vector2(0, 9*50)),
            ItemFactory.NewScrap(new Vector2(0, -8*50), new Vector2(10, -10), 0.07f),
            ItemFactory.NewScrap(new Vector2(0, -8*50), new Vector2(-8, -4), -0.03f),
            m = Entity.NewMothership(new Vector2(0, -8*50 - Assets.DimsOf(Sprites.Mothership).Y / 2), Vector2.Zero, 0f),],
            new Conditional([new Protect([m]), new Custom(m)], Mission.Win(DayOneLog)),
            new CustomSpawner(new Vector2(0, -8*50 - Assets.DimsOf(Sprites.Mothership).Y / 2)));}),
        
        //TODO: Add custom "Humanlike" enemies
        (new("Crossfire", "Sensors indicate a group fighting against the same hostiles encountered during our crash landing.\nAiding them could gain us a powerful ally.",
        140, [0], 0, 2000, 1),
        delegate(){Entity e; return new([
            new Planet(Vector2.Zero, Vector2.Zero, 15000, 12, true, Color.White, true),
            Entity.NewEnemySpawner(new Vector2(1200, 0), Planet.GetOrbitalVelocity(new Vector2(1200, 0), Vector2.Zero, 20000), 0, Team.Friendly),
            Entity.NewEnemySpawner(new Vector2(-1200, 0), Planet.GetOrbitalVelocity(new Vector2(-1200, 0), Vector2.Zero, 20000), 0, Team.Friendly),
            Entity.NewCarrier(new Vector2(50, -750), Vector2.Zero, 0, Team.Friendly),
            Entity.NewCarrier(new Vector2(-50, -750), Vector2.Zero, 0, Team.Friendly),
            Entity.NewSymmetryBoss(new Vector2(700, 0), Vector2.Zero, 0, Team.Friendly),
            Entity.NewTurret(new Vector2(MathF.Sin(0.3f), -MathF.Cos(0.3f)) * 12 * 50, Vector2.Zero, 0.3f, Team.Friendly),
            Entity.NewTurret(new Vector2(MathF.Sin(-0.3f), -MathF.Cos(-0.3f)) * 12 * 50, Vector2.Zero, -0.3f, Team.Friendly),
            e = Entity.NewCrashedShip(new Vector2(-6000, -6640), Vector2.Zero, -MathF.PI * 3 / 8),
            new WaveSpawner(Mission.T1, 0.18f, true),
            new IntroCutscene(QueueCrossfireDialogue),
        ], new Conditional([new Protect([e]), new Custom(e)], Mission.SendPickup(RepairCrashedShip)), 
        new DropSpawner(4000));}),

        (new("Sentry Defense", "We've been assigned to defend this small planet.\n *Repair will not be available for this mission*",
        100, [1, 2], 0, 2000, 1),
        delegate(){Entity a,b;return new([
            new Planet(Vector2.Zero, Vector2.Zero, 3500, 4, true, Color.Cyan, true),
            new WaveSpawner(Mission.T1, 0.75f, true),
            new IntroCutscene(SentryDialogue),
            a=Entity.NewTurret(new Vector2(0, -200 - Assets.DimsOf(Sprites.TurretBase).Y / 2), Vector2.Zero, 0),
            b=Entity.NewOrbiter(new Vector2(400, 0), Planet.GetOrbitalVelocity(new Vector2(400, 0), Vector2.Zero, 3500), 0),
        ], new Conditional([new Protect([a,b]), new WaveGoal(30)], Mission.SendPickup()), 
         new DropSpawner(1500));} ),

        (new("Meet the locals", "Local scans indicate a nearby mineral rich planet occupied by enemy forces. \nCapturing this site will aid in future resource gathering.",
        400, [3], 0, 2000, 2),
        delegate(){Entity a;return new([
            new Planet(Vector2.Zero, Vector2.Zero, 15000, 6f, true, Color.Cyan),
            new Planet(new Vector2(0, 800), Planet.GetOrbitalVelocity(new Vector2(0, 800), Vector2.Zero, 15000) * 0.85f, 1000, 1f, false, Color.Cyan),
            new Tip("Press Q to use your special ability.\nCtrl to toggle aim assist.", new Vector2(0, -6*50)),
            new WaveSpawner(Mission.T1, 0.75f, true),
            a=Entity.NewLargeMiner(new Vector2(0, -6*50 - Assets.Get(Sprites.LargeMiner).Height/2), Vector2.Zero, 0)
        ], new Conditional([new Kill([a])],Mission.SendPickup()), 
        new DropSpawner(1500));}),
        
        (new("Showdown", "Our activities appear to have gathered the attention of an advanced drone.\nAttacking now will suprise the enemy before it can develop any reinforcements.",
        50, [3], 0, 2000, 2),
        delegate(){Entity a;return new([
            new Planet(Vector2.Zero, Vector2.Zero, 5000, 3, true, Color.Cyan),
            new Planet(new Vector2(400, 0), Planet.GetOrbitalVelocity(new Vector2(400, 0), Vector2.Zero, 5000), 240, 1f, false, Color.Cyan),
            new Planet(new Vector2(-600, 0), -Planet.GetOrbitalVelocity(new Vector2(-600, 0), Vector2.Zero, 5000) * 1.2f, 120, 0.6f, false, Color.Yellow),
            new WaveSpawner(Mission.T1, 1f, true),
            a=Entity.NewScrambled(new Vector2(0, -6*50), Vector2.Zero, 0),], 
            new Conditional([new Kill([a])], Mission.SendPickup()),
            new DropSpawner(1500));}),

        (new("Gas Giant", "", 10, [], 1, 2000),
        delegate(){return new([
            new Planets([new Planet(Vector2.Zero, Vector2.Zero, 16000, 4, true, new Color(167, 156, 134), true, 6, 500f, true),
            new Planet(new Vector2(900, 0), Planet.GetOrbitalVelocity(new Vector2(900, 0), Vector2.Zero, 16000), 100, 0.5f, false, Color.OldLace),
            new Planet(new Vector2(-1200, 0), Planet.GetOrbitalVelocity(new Vector2(-1200, 0), Vector2.Zero, 16000) * 1.05f, 100, 0.5f, false, Color.OldLace)]),
            new WaveSpawner(Mission.T1, 1f, false),
            ], new Conditional([new WaveGoal(30)], Mission.SendPickup()), new GliderSpawner(new Vector2(-800, -1100), -900));}),

        (new("Warp Gate", "Scans indicate that a large enemy fleet is coming our way after the loss of their prototype.\nRecommended action: Leave the system immediately.", 170, [6], 1, 2000, 3),
        delegate(){Entity a;return new([a=Entity.NewWarpGate(Vector2.Zero, Vector2.Zero, 0)], new Conditional([new Custom(a)], Mission.Win()), new PlayerSpawner(new Vector2(0, 500)), Sound.None);}),

        (new("???", "Sensor data shows that this site has an unusually high temperature.\nInvestigate possible enemy interferance.", 145, [7], 1, 2000, 3, true),
        //Note: The player construct menu and the Quantum Resonator both use the name of this mission for their special behavior. When changing, make sure their name is updated as well.
        delegate(){return new([
            new Planets([new Planet(Vector2.Zero, Vector2.Zero, 50000, 12, true, new Color(255, 219, 0), true, 1.5f) { Temperature = 0.5f } ]),
            ], new Conditional([], Mission.SendPickup()),
             new PlayerSpawner(new Vector2(-2000, -2000)), Sound.None);}),

        (new("Base of Operations", "We have deployed several communication stations to this site.\nProtect the location for future development.", 130, [8], 1, 2000),
        delegate(){Entity a,b,c; return new([
            new Planets([new Planet(Vector2.Zero, Vector2.Zero, 30000, 10f, true, Color.HotPink, true) ]),
            new WaveSpawner(Mission.T2, 1, false),
            a=Entity.NewCommunicator(new Vector2(MathF.Sin(1.02f), -MathF.Cos(1.02f)), Vector2.Zero, 1.02f, Team.Friendly),
            b=Entity.NewCommunicator(new Vector2(MathF.Sin(2.7f), -MathF.Cos(2.7f)), Vector2.Zero, 2.7f, Team.Friendly),
            c=Entity.NewCommunicator(new Vector2(MathF.Sin(5.33f), -MathF.Cos(5.33f)), Vector2.Zero, 5.33f, Team.Friendly),
            ], new Conditional([new Protect([a, b, c]), new WaveGoal(30),
        ], Mission.SendPickup()), new DropSpawner(500));}),

        (new("Assault", "It appears the enemy has improved their fleet, and has pushed the mothership to a non-ideal location.\nDefend the mothership and defeat the fortified miner base on this planet.", 150, [9], 1, 2000),
        delegate(){Entity a,b,c,d,e,f,g;return new([
            new Planet(Vector2.Zero, Vector2.Zero, 20000, 9f, true, Color.Cyan, false, 1.8f),
            new Planet(new Vector2(0, 1800), Planet.GetOrbitalVelocity(new Vector2(0, 1800), Vector2.Zero, 20000), 1500, 2f, false, Color.Cyan),
            a=Entity.NewTurret(new Vector2(MathF.Sin(5.5f), -MathF.Cos(5.5f)) * 9 * 50, Vector2.Zero, 5.5f, Team.Hostile),
            b=Entity.NewTurret(new Vector2(MathF.Sin(3.2f), -MathF.Cos(3.2f)) * 9 * 50, Vector2.Zero, 3.2f, Team.Hostile),
            c=Entity.NewTurret(new Vector2(MathF.Sin(2.6f), -MathF.Cos(2.6f)) * 9 * 50, Vector2.Zero, 2.6f, Team.Hostile),
            d=Entity.NewTurret(new Vector2(MathF.Sin(1.1f), -MathF.Cos(1.1f)) * 9 * 50, Vector2.Zero, 1.1f, Team.Hostile),
            e=Entity.NewMiner(new Vector2(MathF.Sin(3), -MathF.Cos(3)) * 9 * 50, Vector2.Zero, 3, Team.Hostile),
            f=Entity.NewMiner(new Vector2(MathF.Sin(5.2f), -MathF.Cos(5.2f)) * 9 * 50, Vector2.Zero, 5.2f, Team.Hostile),
            g=Entity.NewOrbiter(new Vector2(0, 1650), Planet.GetOrbitalVelocity(new Vector2(0, 1650), new Vector2(0, 1800), 1500)
                + Planet.GetOrbitalVelocity(new Vector2(0, 1800), Vector2.Zero, 20000), 0),
            new WaveSpawner(Mission.T2, 0.75f, true),
            new Tip("Press C to open the construct menu.\nEach construct requires one scrap to craft.", new Vector2(0, -9*50)),
            ], new Conditional([new Kill([a,b,c,d,e,f]), new Protect([g])], Mission.Win()),
             new CustomSpawner(new Vector2(0, 1650)));}),

        (new("Extraction", "Our success has led us to deploying a miner on this deceptively dense planet.\nDefend it from the incoming enemy forces.", 200, [], 2, 2000),
        delegate(){Entity a;return new([
            new Planets([new Planet(Vector2.Zero, Vector2.Zero, 25000, 7f, true, Color.Cyan), new(new Vector2(800, 0), Planet.GetOrbitalVelocity(new Vector2(800, 0), Vector2.Zero, 25000), 150, 0.5f, false, Color.Cyan), ]),
            new WaveSpawner(Mission.T2, 1, true),
            a=Entity.NewMiner(new Vector2(0, -7*50), Vector2.Zero, 0),
            ], new Conditional([new Protect([a]), new WaveGoal(30)], Mission.Win()), 
             new DropSpawner(1500));}),

        (new("Flight of the bumblebee.", "The enemy fleet's fastest fighter appears to have arrived to this planet and is blocking our path.\nDefeating it appears to be the only way forward.", 150, [11], 2, 2000),
        delegate(){Entity a; return new([
            new Planet(Vector2.Zero, Vector2.Zero, 5000, 4.5f, true, Color.Cyan),
            new Planet(new Vector2(600, 0), Planet.GetOrbitalVelocity(new Vector2(600, 0), Vector2.Zero, 5000), 240, 1f, false, Color.Cyan),
            new Planet(new Vector2(-600, 0), Planet.GetOrbitalVelocity(new Vector2(-600, 0), Vector2.Zero, 5000), 240, 1f, false, Color.Cyan),
            new WaveSpawner(Mission.T2, 1.1f, false),
            a=Entity.NewExodusBoss(new Vector2(0, -6*50), Vector2.Zero, 0, Team.Hostile),
            ], new Conditional([new Kill([a])], Mission.SendPickup()),
            new DropSpawner(1500));}),

        (new("Warp Gate", "The enemy fleet is still hot on our tail.", 100, [12], 2, 2000, 3, true),
        delegate(){Entity a;return new([
            new Planets([new Planet(Vector2.Zero, Vector2.Zero, 150, 3, true, Color.OldLace)]),
            new Tip("Press left shift to return to the previous system. Press right shift to enter the next system.", new Vector2(0, 3 * 50)),
            a=Entity.NewWarpGate(new Vector2(0, 450), -Planet.GetOrbitalVelocity(new Vector2(0, 450), Vector2.Zero, 150), 0),
            ], new Conditional([new Custom(a)], Mission.Win()),
            new PlayerSpawner(new Vector2(0, 500)), Sound.None);}),

        (new("Trader", "This friendly trader invites us to upgrade our modules in exchange for resources", 80, [13], 2, 2000, 3, true),
        delegate(){return new([
            new Planet(Vector2.Zero, Vector2.Zero, 6000, 6, true, Color.Cyan, true, 0.8f),
            Entity.NewTrader(new Vector2(0, 500), Planet.GetOrbitalVelocity(new Vector2(0, 500), Vector2.Zero, 6000), 0),], 
            Mission.SendPickup()(),
            new PlayerSpawner(new Vector2(-2000, -2000)));}),

        (new("Clockwork creation", "A strange sentinal appears to be housed on this ancient planet.\nDismantling it may yield unusual resources.", 60, [14], 2, 2000),
        delegate(){Entity a;return new([
            new Planet(Vector2.Zero, Vector2.Zero, 4000, 3, true, Color.Wheat, false, 1.5f),
            new Planet(new Vector2(500, 0), Planet.GetOrbitalVelocity(new Vector2(500, 0), Vector2.Zero, 4000), 300, 1f, false, Color.Wheat),
            new WaveSpawner(Mission.T2, 1f, true),
            a=Entity.NewClockworkBoss(new Vector2(0, -6*50), Vector2.Zero, 0, Team.Hostile)], 
            new Conditional([new Kill([a])], Mission.SendPickup()),
            new DropSpawner(1500));}),

        (new("Ice giant", "The unusual conditions in this system have resulted in unique developments in the enemies technology.\nBe prepared for advanced enemy cloaking.", 0, [15], 2, 2000),
        delegate(){return new([
            new Planet(Vector2.Zero, Vector2.Zero, 10000, 3, true, new Color(41, 144, 181), false, 15f, 0, true) { Temperature = -20 },
            new WaveSpawner(Mission.T2, 1f, false),], 
            new Conditional([new WaveGoal(30)], Mission.SendPickup()),
            new GliderSpawner(new Vector2(-800, -1300), -1000));}),

        (new("Binary system", "It seems plans for a mass relay have been abandoned here.\nConstruct it to recieve some advanced equipment from our previous stations.", 0, [15], 2, 2000),
        delegate(){Entity a;return new([
            new Planets([new(new Vector2(500, 0), new Vector2(0, 1.05f), 10000, 7, false, Color.Cyan) { Temperature = -5},
            new(new Vector2(-1000, 0), new Vector2(0, -2.1f), 5000, 4f, false, Color.Orange) { Temperature = 5 }]),
            new WaveSpawner(Mission.T3, 1, true),
            a=Entity.MassRelay(Vector2.Zero, Vector2.Zero, 0)], 
            new Conditional([new Protect([a]), new Custom(a)], Mission.SendPickup()),
            new DropSpawner(1500));}),

        (new("Veiled", "Sensors indicate that our actions have been spied on by the enemy.\nDestroy it.",  0, [15], 2, 2000),
        delegate(){Entity a;return new([
            new Planet(Vector2.Zero, Vector2.Zero, 4000, 4.5f, true, new Color(0.03f, 0.05f, 0.08f)),
            new Planet(new Vector2(600, 0), Planet.GetOrbitalVelocity(new Vector2(600, 0), Vector2.Zero, 4000) * 1.05f, 500, 1.5f, false, new Color(0.06f, 0.08f, 0.12f)),
            new WaveSpawner(Mission.T3, 1.1f, false),
            a=Entity.NewVeilBoss(new Vector2(0, -6*50), Vector2.Zero, 0),
            ], new Conditional([new Kill([a])], Mission.SendPickup()),
            new DropSpawner(1500));}),

        (new("Hack", "The enemy has set up a mesh node network for storing information.\nHack it to discover the location of their leader.", 0, [15], 2, 2000),
        delegate(){Entity a,b,c;return new([
            new Planet(Vector2.Zero, Vector2.Zero, 18000, 6f, true, Color.Cyan, false, 1.5f) { Temperature = -2 },
            new Planet(new Vector2(1100, 0), Planet.GetOrbitalVelocity(new Vector2(1100, 0), Vector2.Zero, 18000), 1500, 1.5f, false, Color.Cyan) { Temperature = -2 },
            new WaveSpawner(Mission.T3, 0.8f, true),
            a=Entity.NewMeshNetworkNode(new Vector2(0, -1), Vector2.Zero, 0),
            b=Entity.NewMeshNetworkNode(new Vector2(1250, 0),-Planet.GetOrbitalVelocity(new Vector2(150, 0),
                Vector2.Zero, 1500) + Planet.GetOrbitalVelocity(new Vector2(1100, 0), Vector2.Zero, 18000), 0),
            c=Entity.NewMeshNetworkNode(new Vector2(-500, 0), -Planet.GetOrbitalVelocity(new Vector2(-500, 0), Vector2.Zero, 18000), 0)], 
            new Conditional([new Protect([a,b,c]), new Custom(a), new Custom(b), new Custom(c)], Mission.SendPickup()),
            new DropSpawner(1500));}),

        (new("Inferno", "Your intel has led you here. Finish this.",  0, [15], 2, 2000),
        delegate(){Entity a; return new([
            new Planet(Vector2.Zero, Vector2.Zero, 160000, 6, true, new Color(0.9f, 1f, 0.75f), false, 50f, 0, true) { Temperature = 5 },
            new Planet(new Vector2(0, 2000), Planet.GetOrbitalVelocity(new Vector2(0, 2000) * 0.99f, Vector2.Zero, 16-000), 8000, 4, false, new Color(0.95f, 0.2f, 0.1f)),
            a=Entity.NewEpitomeBoss(new Vector2(0, 2800), Vector2.Zero, 0)],
            new Conditional([new Kill([a])], Mission.SendPickup()),
            new GliderSpawner(new Vector2(-1500, -2000), -1500), Sound.None);}),

        (new("BEEG", "", 0, [15], 2, 2000),
        delegate(){return new([
            new Planets([new Planet(Vector2.Zero, Vector2.Zero, 8000000, 100, true, new Color(1f, 0.8f, 0.5f), false, 10f, 0, true) { Temperature = 5 }]),
            new Colliders(SolarStation),
            new WaveSpawner(Mission.T3, 1, false),], 
            new Conditional([new WaveGoal(10)], Mission.SendPickup()),
            new GliderSpawner(new Vector2(-1000, -7000), -7000), Sound.None);}),

        (new("Black Hole", "", 0, [15], 2, 2000),
        delegate(){return new Mission([
            new Planet(new Vector2(0, 500), Vector2.Zero, 5000, 0.1f, true, Color.Black, false, 0),
            
            ItemFactory.NewScrap(new Vector2(0, -8*50), Vector2.Zero, -0.03f),
            Entity.NewFighter(Vector2.Zero, Vector2.Zero, 0, Team.Hostile )], 
            new Conditional([new WaveGoal(10)], Mission.SendPickup()),
            new GliderSpawner(new Vector2(-1000, -700), -1000), Sound.None);}),

        (new("Last Stand", "Survive", 2000, [], 0, 4, 3, true),
        delegate(){return new Mission([
            new Planet(Vector2.Zero, Vector2.Zero, 20000, 9, true, Color.OrangeRed, true, 1.5f),
            new Planet(new Vector2(1200, 0), Planet.GetOrbitalVelocity(new Vector2(1200, 0), Vector2.Zero, 20000), 750, 2f, false, Color.Red),
            new Tip("You can now construct the makeshift mothership in the construct menu.\n Requires 3 scrap.", new Vector2(0, -10 * 50)),
            new WaveSpawner(Mission.All, 0.25f, true)], 
            new Conditional([new WaveGoal(1000)], Mission.SendPickup()),
            new DropSpawner(1500)); })
    ];
    private bool isUpdating = false;
    public List<Entity> Entities { get; private set; } = [];
    private List<Entity> addedEntities = [];
    private List<Entity> enemies = [];
    private List<Entity> projectiles = [];
    public int Wave { get; set; } = 0;
    //TODO: Add behavior for when there is no planet
    public Planet Planet { get { return GetComponent<Planets>().GetPlanets[0]; }}
    public Mission(List<IMissionComponent> _components, Conditional _objective, IPlayerSpawner _spawner, Sound _music = Sound.main)
    {
        components = _components;
        foreach(var comp in _components)
        {
            if(comp is IObstacle)
            {
                obstacles.Add(comp as IObstacle);
            }
            var entity = comp as Entity;
            if(entity != null)
            {
                Entities.Add(entity);
                if (entity.GetComponent<Attack>() != null)
                {
                    projectiles.Add(entity);
                }
                if (entity.GetComponent<Health>() != null || entity is Player)
                {
                    enemies.Add(entity);
                }   
            }
        }
        spawner = _spawner;
        objective = _objective;
        music = _music;
    }
    private Sound music;
    private Conditional objective;
    private IPlayerSpawner spawner;
    private List<IMissionComponent> components = [];
    private List<IObstacle> obstacles = [];
    public void Add(Entity entity)
    {
        if (!isUpdating)
        {
            components.Add(entity);
            //Checks the entity type, and adds it to the corresponding list for each type
            Entities.Add(entity);
            if (entity.GetComponent<Attack>() != null)
            {
                projectiles.Add(entity);
            }
            if (entity.GetComponent<Health>() != null || entity is Player)
            {
                enemies.Add(entity);
            }
        }
        else
        {
            //Moves entities to the inactive list to prevent modifying a list while iterating
            addedEntities.Add(entity);
        }
    }
    public void AddComponent(IMissionComponent component)
    {
        components.Add(component);
        if(component is IObstacle)
        {
            obstacles.Add(component as IObstacle);
        }
        if(component is Entity)
        {
            addedEntities.Add(component as Entity);
        }
    }
    public T GetComponent<T>() where T : class, IMissionComponent
    {
        T comp;
        foreach (IMissionComponent component in components)
        {
            comp = component as T;
            if (comp != null)
            {
                return comp;
            }
        }
        return null;
    }
    public void Initialize()
    {
        CurrentGameState.SwitchState(new PlayingGame());
        Engine.SaveGame.Player.dockedEntity = null;
        foreach(var comp in components)
        {
            comp.Initialize();
        }
        objective.Initialize();
        spawner.Spawn();
        SoundManager.ChangeTrack(Assets.Get(music));
    }
    public void IngameUpdate()
    {
        Player.Update();
        if (Player.dockedEntity == null && Player.Progression > -1)
        {
            Engine.SaveGame.CurrentMission.CalculateTrajectory(Player.Position, Player.Velocity, Player.ColliderRadius);
        }
        Engine.MousePositionOffset = new Vector2(Mouse.GetState().X, Mouse.GetState().Y) / 10 - Engine.BackBuffer / 20
        + Engine.ScreenShakeFactor * Engine.ScreenShakeFactor * new Vector2(Util.Random.NextSingle() - 0.5f, Util.Random.NextSingle() - 0.5f) * 50;
        Engine.Camera.Rotation = Engine.ScreenShakeFactor * Engine.ScreenShakeFactor * (Util.Random.NextSingle() - 0.5f) * 0.15f;
        //If the player is further from the camera, put more weight on the player
        //Tanh prevents frac from going above 1
        float frac = MathF.Tanh(Vector2.Distance(Player.Position, Engine.Camera.Position) / 750);
        Engine.Camera.Position = Player.Position * frac + Engine.Camera.Position * (1 - frac);
        var time = Engine.IngameTime;
        time.Duration += Engine.DeltaSeconds;
        Engine.IngameTime = time;
        if (Player.isExpired)
        {
            foreach (var module in Player.modules)
            {
                //module.Value.Health = module.Value.MaxHealth;
                module.Value.Health = 20;
                module.Value.isFailed = false;
            }
            Player.isExpired = false;
            EventHandler.UpdateModulesStatus();
            EventHandler.MissionSelectTrigger(new MissionSelect());
        }
        Update();
    }
    public void Update()
    {
        isUpdating = true;
        foreach(var comp in components)
        {
            comp.Update();
        }
        if(objective != null)
        {
            objective = objective.Update();    
        }
        //Prevents players from losing important items
        Entity[] importantEntites = Engine.SaveGame.CurrentMission.GetEntity<KeyTag>();
        float r = missions[Engine.SaveGame.CurrentMissionIndex].data.EdgeRadius;
        foreach(var entity in importantEntites)
        {
            if (entity.Position.Length() >= r)
            {
                entity.Velocity *= 0.8f;
                entity.Velocity += Vector2.Normalize(-entity.Position) * Engine.DeltaSeconds * (entity.Position.Length() - r);
            }   
        }
        if (Engine.SaveGame.Player.Position.Length() >= r)
        {
            Engine.SaveGame.Player.Velocity *= 0.8f;
            Engine.SaveGame.Player.Velocity += Vector2.Normalize(-Engine.SaveGame.Player.Position) * Engine.DeltaSeconds * (Engine.SaveGame.Player.Position.Length() - r);
        }
        if (projectiles.Count >= 150)
        {
            for (int i = 0; i < projectiles.Count - 150; i++)
            {
                projectiles[i].isExpired = true;
            }
        }
        components = [.. components.Where(x => ((x as Entity) == null) || !(x as Entity).isExpired)];
        Entities = [.. Entities.Where(x => !x.isExpired)];
        projectiles = [.. projectiles.Where(x => !x.isExpired)];
        enemies = [.. enemies.Where(x => !x.isExpired)];

        isUpdating = false;

        //Moves all newly created entities to the main list
        foreach (var entity in addedEntities)
        {
            Add(entity);
        }
        addedEntities.Clear();
    }
    public void Explode(int _damage, float _radius, Vector2 _position)
    {
        float dist;
        foreach (var entity in Entities)
        {
            dist = Vector2.Distance(_position, entity.Position);
            if (dist < _radius + entity.ColliderRadius && dist > float.Epsilon)
            {
                entity.Collide(_damage);
            }
        }
        dist = Vector2.Distance(_position, Player.Position);
        if (dist < _radius && dist > 0.001f)
        {
            Player.Collide(_damage);
        }
    }
    public void DecayPickups()
    {
        foreach (var pickup in Entities)
        {
            if (pickup is Pickup && Util.Random.NextSingle() > (0.4f + Engine.SaveGame.CurrentMission.GetAtmospherePressure(pickup))) //Update changelog when available
            {
                pickup.Collide(1);
            }
        }
    }
    public Dockable NearestDockableEntity(Entity _entity)
    {
        float nearestDistance = float.MaxValue;
        Dockable returnEntity = null;
        foreach (Entity entity in Entities)
        {
            var component = entity.GetComponent<Dockable>();
            if (!(component == null) && !component.Entity.isExpired)
            {
                float distanceSqr = Vector2.DistanceSquared(entity.Position, _entity.Position);
                if (distanceSqr < nearestDistance)
                {
                    nearestDistance = distanceSqr;
                    returnEntity = component as Dockable;
                }
            }
        }
        return returnEntity;
    }
    public Entity NearestEnemy(Entity entity, bool _getDeadEnemies = false)
    {
        float maxDistSqr = StealthRange * StealthRange * StealthThreshold * StealthThreshold;
        float nearestDistance = float.MaxValue;
        Entity returnEnemy = null;
        foreach (Entity targetEnemy in enemies)
        {
            var comp = targetEnemy.GetComponent<Stealth>();
            if (entity.IsFriendly(targetEnemy) || ((comp == null) ? comp.StealthAbility : 0) > entity.SensingAbility)
            {
                continue;
            }
            if(!_getDeadEnemies && targetEnemy.GetComponent<Health>() != null && targetEnemy.Health <= 0)
            {
                continue;
            }
            float distance = Vector2.DistanceSquared(entity.Position, targetEnemy.Position);
            if (distance < nearestDistance && (targetEnemy.StealthAbility < entity.SensingAbility || (distance < maxDistSqr)))
            {
                nearestDistance = distance;
                returnEnemy = targetEnemy;
            }
        }
        if (!entity.IsFriendly(Engine.SaveGame.Player))
        {
            //Enemies will prioritize the player
            float distance = Vector2.DistanceSquared(entity.Position, Player.Position) / 1.5f;
            if (Player.StealthAbility > entity.SensingAbility)
            {
                return returnEnemy;
            }
            if (distance < nearestDistance && (Player.StealthAbility < entity.SensingAbility || (distance < maxDistSqr)))
            {
                returnEnemy = Player;
            }
        }
        return returnEnemy;
    }
    public Entity NearestAlly(Entity entity)
    {
        float nearestDistance = float.MaxValue;
        Entity returnEnemy = null;
        foreach (Entity targetEnemy in enemies)
        {
            if (!entity.IsFriendly(targetEnemy) || targetEnemy == entity)
            {
                continue;
            }
            if (targetEnemy.GetComponent<ChildEnemyTag>()?.ChildEnemy ?? false)
            {
                continue;
            }
            float distance = Vector2.DistanceSquared(entity.Position, targetEnemy.Position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                returnEnemy = targetEnemy;
            }
        }
        if (entity.IsFriendly(Engine.SaveGame.Player))
        {
            float distance = Vector2.DistanceSquared(entity.Position, Player.Position);
            if (distance < nearestDistance)
            {
                returnEnemy = Player;
            }
        }
        return returnEnemy;
    }
    public Entity NearestItem(Entity entity, bool _findAll)
    {
        float nearestDistance = float.MaxValue;
        Entity returnItem = null;
        foreach (Entity targetEntity in Entities)
        {
            if (targetEntity is not Pickup || targetEntity == entity)
            {
                continue;
            }
            if (!_findAll && (targetEntity is Module || targetEntity.GetComponent<SpecializedTag>() != null || targetEntity.GetComponent<Behaviour> != null))
            {
                continue;
            }
            float distance = Vector2.DistanceSquared(entity.Position, targetEntity.Position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                returnItem = targetEntity;
            }
        }
        return returnItem;
    }
    public Entity NearestProjectile(Vector2 _position, int _sensingAbility, Team _team)
    {
        float nearestDistance = float.MaxValue;
        Entity returnProjectile = null;
        foreach (var targetProjectile in projectiles)
        {
            float distance = Vector2.DistanceSquared(targetProjectile.Position, _position);
            if (distance < nearestDistance && _team != targetProjectile.Team
                && (targetProjectile.StealthAbility < _sensingAbility || (targetProjectile.StealthAbility == _sensingAbility && distance < StealthRange * StealthThreshold)))
            {
                nearestDistance = distance;
                returnProjectile = targetProjectile;
            }
        }
        return returnProjectile;
    }
    public Entity[] GetEntity<T>() where T : Component
    {
        List<Entity> selectedEntities = [];
        foreach(var entity in Entities)
        {
            if(entity.GetComponent<T>() != null)
            {
                selectedEntities.Add(entity);
            }
        }
        return [.. selectedEntities];
    }
    public List<Entity> Hitscan(Vector2 _pos, Vector2 _dir, float _maxLength, bool _getAll, out Vector2 _end, Team[] _whitelist = null, bool _getProjectiles = false)
    {
        var dir = Vector2.Normalize(_dir);
        var list = new List<Entity>();
        float dist = _maxLength;
        Entity nearestEnemy = null;
        Engine.SaveGame.CurrentMission.IsColliding(_pos, dir * dist, 1, false, out float maxDist);
        foreach (var entity in Entities)
        {
            if(_getProjectiles || entity.GetComponent<Health>() != null)
            {
                CalculateIntersection(entity);
            }
        }
        CalculateIntersection(Engine.SaveGame.Player);
        if (maxDist > dist)
        {
            maxDist = dist;
        }
        if (!_getAll && nearestEnemy != null)
        {
            list.Add(nearestEnemy);
        }
        _end = _pos + dir * maxDist;
        return list;
        void CalculateIntersection(Entity _entity)
        {
            if (_whitelist != null && !_whitelist.Contains(_entity.Team))
            {
                return;
            }
            Vector2 relativePos = _entity.Position - _pos;
            float closestLength = (relativePos.X * dir.X + relativePos.Y * dir.Y);
            float closestDistance = Vector2.Distance((dir * closestLength + _pos), _entity.Position);
            if (closestLength > 0 && closestLength < maxDist && closestDistance < _entity.ColliderRadius)
            {
                if (_getAll)
                {
                    list.Add(_entity);
                }
                else
                {
                    float discriminant = MathF.Sqrt(_entity.ColliderRadius * _entity.ColliderRadius - closestDistance * closestDistance);
                    if (dist > closestLength - discriminant)
                    {
                        dist = closestLength - discriminant;
                        nearestEnemy = _entity;
                    }
                }
            }
        }
    }
    private static Cutscene RestartCutscene()
    {
        List<string> text =
        [
            "Kernel Ship-Master ver 3.1.1 - Copyright(C) In-Tech 2059",
            "Detected x86 P5 Pentium @ 250MHz, 4MB available",
            "Booting with parameters -v -f",
            "Error: Retrying",
            "Error: Retrying",
            "Error: Retrying",
            "Fatal Error: Boot sector missing or corrupted.",
            "Please insert disk image.",
            "Image Detected, booting from disk.",
            "Loading core.bin...",
            "Loading navnet.bin...",
            "Loading music_player.bin...",
            "Load complete, initiating system check.",
            "Hull:",
            "Guns:",
            "Engn:",
            "Snsr:",
            "Core:",
            "Please restart modules to restore functionality.",
            "                           @@@@@@                ",
            "                       @@@@@@@@@@@@@@            ",
            "                   @@@@@@@@@@@@@@@@@@@@@@        ",
            "               @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@    ",
            "           =      @@@@@@@@@      @@@@@@@@@       ",
            "           ====      @@@    %%%%    @@@      ####",
            "           =======       %%%%%%%%%%       #######",
            "           ==========  %%%%%%%%%%%%%%  ##########",
            "           =========  %%%%%%%%%%%%%%%%  #########",
            "                    Welcome aboard pilot         ",
            "           =========  %%%%%%%%%%%%%%%%  #########",
            "           ==========  %%%%%%%%%%%%%%  ##########",
            "           ============  %%%%%%%%%%   ###########",
            "           =============    %%%%    #############",
            "           ================      ################",
            "               =============    #############    ",
            "                   =========    #########        ",
            "                       =====    #####            ",
        ];
        Cutscene scene = null;
        Vector2 screen = Engine.BackBuffer / 2;
        var t5 = new TextActor(new Vector2(80, 260) * 3 - screen, "Failed\nFailed\nFailed\nFailed\nFailed\n")
        {
            TextSize = 1.45f * UIManager.UIScale,
            TextColor = Color.Red
        };
        var floppy = new Actor(Assets.Get(Sprites.Floppy), new Vector2(Engine.BackBuffer.X * 4 / 5, Engine.BackBuffer.Y), Color.Gray, MathF.PI / 8) { Scale = UIManager.UIScale };
        var floppyFlat = new Actor(Assets.Get(Sprites.FloppyFlat), new Vector2(Engine.BackBuffer.X * 4 / 5, Engine.BackBuffer.Y), Color.White, 0) { Scale = UIManager.UIScale };
        var floppyVel = Vector2.Zero;
        var ledGlow = new Actor(Assets.Get(Sprites.LEDGlow), UI.FloppyTerminal.position + (new Vector2(72.5f, 94.5f) * UIManager.UIScale - Assets.DimsOf(Sprites.Terminal) / 2) * UIManager.UIScale, Color.Red, 0) { Scale = UIManager.UIScale };
        float floppyAngVel = Util.OneToNegOne();
        List<IActor> actors = [];
        for (int i = 0; i < text.Count; i++)
        {
            actors.Add(new TextActor(new Vector2(60, 20 * 3 * i) - screen, text[i]) { TextSize = 1.5f * UIManager.UIScale });
        }
        actors.Add(t5);
        float ts = 0.2f;
        float trueTime = 0;
        SoundEffectInstance computerSounds = null;
        //Ensure planets still orbit and render
        List<IEvent> events =
        [
            new TriggerEvent(0, delegate(float time)
            {
                Engine.UIManager.ScreenWindow = UI.CutsceneGlobalMenu;
                for(ModuleType i = 0; i < (ModuleType)5; i++)
                {
                    Engine.SaveGame.Player.modules[i].isFailed = true;
                }
            }),
            new EndlessEvent(delegate(float time)
            {
                trueTime += Engine.DeltaSeconds;
                if(trueTime / 28 - Math.Truncate(trueTime / 28) <= Engine.DeltaSeconds / 28 + float.Epsilon)
                {
                    computerSounds = Assets.Get(Sound.ComputerSounds).CreateInstance();
                    computerSounds.Play();
                }
            }),
            new Event(0, ts * 3, delegate (float time)
            {
                var a = actors[(int)(time/ts)] as TextActor;
                a.Index = a.Text.Length;
            }),
            new Event(ts*3, 8f + Engine.DeltaSeconds, delegate (float time)
            {
                var a = actors[(int)(time/2) + 2] as TextActor;
                a.Index = a.Text.Length;
            }),
            new Event(8 + ts * 4, Engine.DeltaSeconds, delegate(float time)
            {
                var a = actors[7] as TextActor;
                a.Index = a.Text.Length;
                scene.IsPaused = true;
                //Check this line for differing UI scales
                if(Input.OldMouseState.LeftButton == ButtonState.Pressed && MathF.Abs((UI.FloppyTerminal.position.X - floppy.Position.X + 200)) < 200 && MathF.Abs(UI.FloppyTerminal.position.Y + 175 - floppy.Position.Y) < 75)
                {
                    floppy.Color = Color.White * (MathF.Sin(Engine.Time * 4) / 8 + 0.875f);
                    floppy.Angle = MathF.Sin(Engine.Time * 5) / 20;
                    if(Input.NewMouseState.LeftButton == ButtonState.Released)
                    {
                        scene.IsPaused = false;
                    }
                }
                else
                {
                    floppy.Color = Color.White;
                }
            }),
            new Event(0, 8 + ts * 4 + Engine.DeltaSeconds, delegate(float time) //Render floppy overtop of inserter
            {
                Engine.Self.QueueShaderException(floppy);
                var mousePos = new Vector2(Input.OldMouseState.X, Input.OldMouseState.Y);
                if(Input.NewMouseState.LeftButton == ButtonState.Pressed && Vector2.Distance(floppy.Position, mousePos) < 100 * UIManager.UIScale)
                {
                    var newPos = new Vector2(Input.NewMouseState.X, Input.NewMouseState.Y);
                    floppyVel = newPos - mousePos;
                    floppy.Position = newPos;
                    floppy.Angle *= Util.FIED(0.02f);
                    floppyAngVel = Util.OneToNegOne();
                    return;
                }
                if(floppy.Position.X < 0 || floppy.Position.X > Engine.BackBuffer.X)
                {
                    floppy.Position = new Vector2(Math.Clamp(floppy.Position.X, 0, Engine.BackBuffer.X), floppy.Position.Y);
                    floppyVel.X *= -0.2f;
                    floppyVel.Y *= Util.FIED(0.03f);
                    floppyAngVel = 0;
                }
                if(floppy.Position.Y < 0 || floppy.Position.Y > Engine.BackBuffer.Y)
                {
                    floppy.Position = new Vector2(floppy.Position.X, Math.Clamp(floppy.Position.Y, 0, Engine.BackBuffer.Y));
                    floppyVel.Y *= -0.2f;
                    floppyVel.X *= Util.FIED(0.03f);
                    floppyAngVel = 0;
                }
                floppyVel += new Vector2(0,18) * Engine.DeltaSeconds;
                floppy.Position += floppyVel;
                floppy.Angle += floppyAngVel * Engine.DeltaSeconds;
                if(EventHandler.AcknowledgeMessage(Message.ToggleTerminal))
                {
                    UI.FloppyTerminal.enabled = !UI.FloppyTerminal.enabled;
                }
            }),
            new Event(8 + ts * 4 + Engine.DeltaSeconds,2,delegate(float time)
            {
                floppyFlat.Position = UI.FloppyTerminal.position + (new Vector2(107, 94.5f) * UIManager.UIScale - Assets.DimsOf(Sprites.Terminal) / 2) * UIManager.UIScale;
                floppyFlat.Color = Color.White * ((2f - time)/2f);
                Engine.Self.QueueShaderException(floppyFlat);
                Engine.Self.QueueShaderException(ledGlow);
            }),
            new TriggerEvent(10 + ts * 4, delegate(float time)
            {
                UI.FloppyTerminal.enabled = false;
            }),
            new Event(10 + ts * 5, ts * 10, delegate (float time)
            {
                if(time/ts - Math.Truncate(time/ts) <= Engine.DeltaSeconds/ts && time > ts * 5)
                {
                    PushTextUp();
                }
                var a = actors[(int)(time/ts) + 8] as TextActor;
                a.Index = a.Text.Length;
            }),
            new Event(10 + ts * 11, 6, delegate (float time)
            {
                if(time - Math.Truncate(time) <= Engine.DeltaSeconds && time > 0.5f)
                {
                    t5.Index += 7;
                    SoundManager.PlayGlobalSound(Assets.Get(Sound.Beep));
                }
            }),
            new TriggerEvent(16 + ts * 12, delegate(float time)
            {
                scene.IsPaused = true;
                PushTextUp();
                var a = actors[18] as TextActor;
                a.Index = a.Text.Length;
            }),
            new Event(16 + ts * 12, Engine.DeltaSeconds, delegate(float time)
            {
                bool notReady = UI.RestartSwitch.Intervals[0] < 0.95f;
                foreach(var module in Engine.SaveGame.Player.modules)
                {
                    notReady = module.Value.isFailed || notReady;
                }
                if(!notReady)
                {
                    scene.IsPaused = false;
                    UI.FuseMenu.enabled = false;
                    for(int i = 0; i < 13; i++)
                    {
                        PushTextUp();
                    }
                }
            }),
            new Event(16 + ts * 12, Engine.DeltaSeconds, delegate(float time)
            {
                Engine.SaveGame.Player.Update();
            }),
            new Event(16 + ts * 12 + Engine.DeltaSeconds, 2f, delegate (float time)
            {
                var a = actors[(int)(time*9) + 19] as TextActor;
                a.Index = a.Text.Length;
            }),
            new TriggerEvent(21 + ts * 12 + Engine.DeltaSeconds, delegate(float time)
            {
                foreach(var module in Engine.SaveGame.Player.modules)
                {
                    module.Value.isFailed = false;
                }
                computerSounds.Pause();
                Engine.UIManager.ScreenWindow = UI.GlobalMenu;
                UI.FloppyTerminal.enabled = false;
                UI.FuseMenu.enabled = false;
                EventHandler.AcknowledgeMessage(Message.ToggleTerminal);
            })
        ];
        scene = new Cutscene(events, actors, new PlayingGame());
        return scene;
        void PushTextUp()
        {
            foreach(var actor in actors)
            {
                TextActor text;
                if((text = (actor as TextActor)) != null)
                {
                    text.Position += new Vector2(0, -20) * 3;
                }
            }
        }
    }
    private static Cutscene DayOneLog()
    {
        Cutscene scene;
        List<string> text =
        [
            "Day one log:",
            "System diagnostics indicate full memory corruption.",
            "Original mission parameters lost.",
            "Encounter with hostile force suggests wanted status.",
            "Recommended actions:",
            "Investigate nearby planets.",
            "Discover original mission.",
            "Survive.", 
        ];
        List<IActor> actors = [];
        List<IEvent> events = [];
        Vector2 screen = Engine.BackBuffer / 2;
        float sum = 0;
        float ts = 0.65f;
        for (int i = 0; i < text.Count; i++)
        {
            var actor = new TextActor(new Vector2(60, 20 * 3 * i) - screen, text[i]) { TextSize = 1.5f * UIManager.UIScale };
            actors.Add(actor);
            events.Add(new Event(sum + i, text[i].Length * ts, delegate(float time)
            {
                float index = time / ts;
                actor.Index = (int)(index) + 1;
                if(index - MathF.Floor(index) <= Engine.DeltaSeconds / ts)
                {
                    SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
                }
            }));
            sum += text[i].Length * ts;
        }
        events.Add(new Event(sum + text.Count, 1, delegate (float time) { }));
        scene = new Cutscene(events, actors, new MissionSelect());
        return scene;
    }
    private static Cutscene QueueCrossfireDialogue()
    {
        List<IEvent> events = 
        [
            new TriggerEvent(0, delegate(float time)
            {
                Engine.DialogueManager.Add(new Dialogue(
                    [
                        "Incoming: Sir, an unknown ship appears to be approaching!",
                        "They seem to be hailing us... They're on our side!",
                        "Pilot! Aid us in this fight and we'll help you however we can!",
                        ], null));
            }),
        ];
        return new Cutscene(events, [], new PlayingGame());
    }
    private static Cutscene RepairCrashedShip()
    {
        var ship = new Actor(Assets.Get(Sprites.Mothership), Vector2.Zero, Color.White, 0);
        List<IActor> actors = [ship];
        List<IEvent> events =
        [
            new TriggerEvent(0, delegate(float time)
            {
                Engine.DialogueManager.Add(new Dialogue(
                    [
                        "All teams, rendezvous at the ship if you value your life!",
                        "Get this ship in the air!",
                    ], null));
            }),
            new Event(6, 3, delegate(float time)
            {
                ship.Position += new Vector2(MathF.Sin(MathF.Atan2(time, 1)), MathF.Cos(MathF.Atan2(time, 1))) * time;
            }),
            new TriggerEvent(9, delegate(float time)
            {
                Engine.DialogueManager.Add(new Dialogue(
                    [
                        "We barely got out of there.",
                        "Pilot, I applaud your courage. We're in your debt.",
                        "Your ship... I've never seen anything like it before.",
                        "Lets regroup at the base. We might have some information that can aid you on your journey."
                    ], null));
            }),
        ];
        Vector2 screen = Engine.BackBuffer / 2;
        float sum = 0;
        float ts = 0.65f;
        List<string> text =
        [
            "Day two log",
            "Cross referencing at insurgent group indicates connection between group and original mission parameters.",
            "Data indicates possible conflict of interests between group and mission.",
            "Recommended course of action:",
            "Gain trust within the group.",
            "Search for more information about original creators.",
        ];
        for (int i = 0; i < text.Count; i++)
        {
            var actor = new TextActor(new Vector2(60, 20 * 3 * i) - screen, text[i]) { TextSize = 1.5f * UIManager.UIScale };
            actors.Add(actor);
            events.Add(new Event(sum + i, text[i].Length * ts, delegate (float time)
            {
                float index = time / ts;
                actor.Index = (int)(index) + 1;
                if (index - MathF.Floor(index) <= Engine.DeltaSeconds / ts)
                {
                    SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
                }
            }));
            sum += text[i].Length * ts;
        }
        events.Add(new Event(sum + text.Count, 1, delegate (float time) { }));
        return new Cutscene(events, actors, new MissionSelect());
    }
    private static Cutscene SentryDialogue()
    {
        List<IEvent> events = 
        [
            new TriggerEvent(0, delegate(float _time)
            {
                Engine.DialogueManager.Add(new Dialogue(
                    [
                        "*incoming* Oye, you recievin' this?", 
                        "Good, now listen up.",
                        "You've been deployed on one of them there planets that we've scoped out.",
                        "See that last battle beat us pretty badly, so we need materials!",
                        "We've deployed a turret as you can see, and we want you to defend it until we say you can leave.",
                        "We can't spare equipment to fix damage you sustain, so try to avoid getting hit.",
                        "Hey, we might be able to scrounge up some intel if you help us with this though.",
                        "See you soon.",
                    ], null));
            }),
        ];
        return new Cutscene(events, [], new PlayingGame());
    }
    private static ICollider[] SolarStation() => [
            new LineCollider(new Vector2(-1000, -5800), new Vector2(1000, -5800)), 
    ];
    public ICollider IsColliding(Vector2 _position, Vector2 _velocity, float _colliderRadius, bool _override, out float end)
    {
        end = _velocity.Length();
        ICollider returnObstacle = null;
        foreach(var entity in Engine.SaveGame.CurrentMission.Entities)
        {
            if(entity is not ICollider)
            {
                continue;
            }
            var collided = (entity as ICollider).IsColliding(_position, _velocity, _colliderRadius, _override, out float _end);
            if(collided)
            {
                if(_end < end)
                {
                    end = _end;
                    returnObstacle = entity as ICollider;
                }
            }
        }
        return returnObstacle;
    }
    public float GetAtmospherePressure(Entity _entity)
    {
        float sum = 0;
        foreach(var entity in Engine.SaveGame.CurrentMission.Entities)
        {
            if(entity is Planet)
            {
                sum += (entity as Planet).GetAtmosphereDensity(_entity);
            }
        }
        return sum;
    }
    public void FailMission()
    {
        throw new NotImplementedException();
    }
    public void CompleteCustomRule(Entity _target)
    {
        objective.CompleteCustomRule(_target);
    }
    public void CalculateTrajectory(Vector2 _startPosition, Vector2 _startVelocity, float _radius)
    {
        Entity[] planets = Engine.SaveGame.CurrentMission.Entities.Where(x => x is Planet).ToArray();
        ICollider[] Colliders = [];
        if (GetComponent<Colliders>() != null)
        {
            Colliders = GetComponent<Colliders>().GetColliders;
        }
        Vector2 futurePosition = _startPosition;
        Vector2 futureVelocity = _startVelocity;
        Vector2[] futurePlanetPositions = [.. planets.Select(planet => planet.Position)];
        Vector2[] futurePlanetVelocities = [.. planets.Select(planet => planet.Velocity)];
        int currentPlanet = 0;
        bool hasChanged = false;
        var emitter = new ParticleEmitter(Assets.Get(Sprites.Dot), Engine.DeltaSeconds, _startPosition, 0, 0, 0, 5f, Color.Cyan, EmitterType.EmissionOverDistance);
        bool exit = false;

        for (int n = 0; n < 1000; n++)
        {
            foreach(var collider in Colliders)
            {
                if(collider.IsColliding(futurePosition, futureVelocity, Engine.SaveGame.Player.ColliderRadius, false, out float _))
                {
                    exit = true;
                    break;
                }
            }
            for (int i = 0; i < planets.Length; i++)
            {
                var planet = planets[i] as Planet;
                if (planet.ColliderRadius + _radius > Vector2.Distance(futurePlanetPositions[i], futurePosition))
                {
                    exit = true;
                    break;
                }
                for (int j = 0; j < planets.Length; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    Vector2 relativePlanetPosition = futurePlanetPositions[i] - futurePlanetPositions[j];
                    if (relativePlanetPosition == Vector2.Zero)
                    {
                        relativePlanetPosition = Vector2.One;
                    }
                    if (!planet.isImmovable)
                    {
                        futurePlanetVelocities[i] += Vector2.Normalize(-relativePlanetPosition) * (planets[j] as Planet).mass / relativePlanetPosition.LengthSquared();
                    }
                }
                Vector2 relativePosition = futurePosition - futurePlanetPositions[i];
                futureVelocity += Vector2.Normalize(-relativePosition) * planet.mass / relativePosition.LengthSquared();
            }
            for (int i = 0; i < planets.Length; i++)
            {
                futurePlanetPositions[i] += futurePlanetVelocities[i];
            }
            futurePosition += futureVelocity;
            Vector2 particlePos = futurePosition;
            if (SaveGame.PatchedConics)
            {
                hasChanged = false;
                float smallestSOI = 999999;
                for (int i = 0; i < futurePlanetPositions.Length; i++)
                {
                    var p = (planets[i] as Planet);
                    float sphereOfInfluence = p.mass + p.ColliderRadius;
                    if (Vector2.Distance(futurePosition, futurePlanetPositions[i]) < sphereOfInfluence && sphereOfInfluence < smallestSOI)
                    {
                        smallestSOI = sphereOfInfluence;
                        particlePos += planets[i].Position - futurePlanetPositions[i];
                        if (currentPlanet != i)
                        {
                            currentPlanet = i;
                            hasChanged = true;
                        }
                        break;
                    }
                }
            }
            if (!hasChanged)
            {
                emitter.position = particlePos;
                emitter.Update();
            }
            else
            {
                emitter.position = particlePos;
                emitter.prevPosition = particlePos;
            }
            if (exit)
            {
                ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), particlePos, 0, Color.Red * 0.75f));
                return;
            }
        }
    }
    public Vector2 GetNormalizedAcceleration(Vector2 _position)
    {
        var comp = GetComponent<Planets>();
        if(comp != null)
        {
            Vector2 acceleration = Vector2.Zero;
            foreach (var planet in comp.GetPlanets)
            {
                acceleration -= planet.GetAcceleration(_position);
            }
            return acceleration;
        }
        return Vector2.Zero;
    }
    public Vector2 NewSpawnLocation()
    {
        Vector2 spawnLocation;
        do
        {
            float angle = (Util.Random.NextSingle() - 0.5f) * MathF.Tau;
            float distanceMultiplier = 1 + (Util.Random.NextSingle() - 0.5f) / 4;
            float distance = (Engine.ScreenSize.X + Engine.ScreenSize.Y) * distanceMultiplier / 3;
            spawnLocation = Util.ToUnitVector(angle) * distance + Engine.SaveGame.Player.Position;
        }
        while(IsColliding(spawnLocation, Vector2.Zero, 10, false, out float _) != null);
        return spawnLocation;
    }
    public void Draw(SpriteBatch _spriteBatch)
    {
        Player.Draw(_spriteBatch);
        foreach(var comp in components)
        {
            comp.Draw(_spriteBatch);
        }
    }
    public static List<(int, Func<Vector2, Vector2, float, Team, Entity>)> TierOneEnemies()
    {
        return
        [
            (1, Entity.NewFighter),
            (3, Entity.NewSniper),
            (4, Entity.NewShotgunner),
            (4, Entity.NewCarrier),
        ];
    }
    public static List<Func<Vector2, Vector2, float, Team, Entity>> TierOneBosses()
    {
        return
        [
            Entity.NewSymmetryBoss,
            Entity.NewWyvernBoss,
            Entity.NewDeadeyeBoss,
        ];
    }
    public static List<(int, Func<Vector2, Vector2, float, Team, Entity>)> TierTwoEnemies()
    {
        return
        [
            (1, Entity.NewAdvancedFighter),
            (2, Entity.NewHovercraft),
            (2, Entity.NewHealer),
        ];
    }
    public static List<Func<Vector2, Vector2, float, Team, Entity>> TierTwoBosses()
    {
        return
        [
            Entity.NewOverloadBoss,
            Entity.NewSurgeBoss,
            Entity.NewStreamlineBoss
        ];
    }
    public static List<(int, Func<Vector2, Vector2, float, Team, Entity>)> TierThreeEnemies()
    {
        return
        [
            (1, Entity.NewStealthFighter),
            (2, Entity.NewHunter),
            (3, Entity.NewEngineer),
        ];
    }
    public static List<Func<Vector2, Vector2, float, Team, Entity>> TierThreeBosses()
    {
        return
        [
            Entity.NewPursuerBoss,
            Entity.NewContinuumBoss,
        ];
    }
    public static List<(int, Func<Vector2, Vector2, float, Team, Entity>)> AllEnemies()
    {
        return [.. TierOneEnemies(), .. TierTwoEnemies(), .. TierThreeEnemies()];
    }
    public static List<Func<Vector2, Vector2, float, Team, Entity>> AllBosses()
    {
        return [.. TierOneBosses(), .. TierTwoBosses(), .. TierThreeBosses()];
    }
    public static (List<(int, Func<Vector2, Vector2, float, Team, Entity>)>, 
    List<Func<Vector2, Vector2, float, Team, Entity>>) T1()
    {
        return (TierOneEnemies(), TierOneBosses());
    }
        public static (List<(int, Func<Vector2, Vector2, float, Team, Entity>)>, 
    List<Func<Vector2, Vector2, float, Team, Entity>>) T2()
    {
        return (TierTwoEnemies(), TierTwoBosses());
    }
        public static (List<(int, Func<Vector2, Vector2, float, Team, Entity>)>, 
    List<Func<Vector2, Vector2, float, Team, Entity>>) T3()
    {
        return (TierThreeEnemies(), TierThreeBosses());
    }
        public static (List<(int, Func<Vector2, Vector2, float, Team, Entity>)>, 
    List<Func<Vector2, Vector2, float, Team, Entity>>) All()
    {
        return (AllEnemies(), AllBosses());
    }
    //TODO: Find a better way to do this
    //Delegate stacking is messy
    public static Func<Conditional> SendPickup(Func<GameState> _scene = null)
    {
        return delegate{
        return new Conditional([new Custom(Entity.NewPickupDrone(new Vector2(-2000, 2000), Engine.SaveGame.CurrentMission.Planet.ColliderRadius * 1.25f))], 
        Win(_scene));};
    }
    public static Func<Conditional> Win(Func<GameState> _scene = null)
    {
        return Begin(_scene ?? delegate{ return new MissionSelect(); }, delegate{Engine.SaveGame.CompleteMission(Engine.SaveGame.CurrentMission.Wave); return null;});
    }
    public static Func<Conditional> Begin(Func<GameState> _state, Func<Conditional> _nextConditional)
    {
        return delegate
        {
            CurrentGameState.SwitchState(_state());
            return _nextConditional();
        };
    }
}
public class MissionData(string _name, string _description, float _distance, 
    int[] _prerequisites, int _system, float _radius, int _playerProgression = 3, bool _isRelaunchable = false)
{
    public string Name => _name;
    public string Description => _description;
    public float Distance => _distance;
    public int[] Prerequisites => _prerequisites;
    public int System => _system;
    public int PlayerProgression { get { if (SaveGame.DebugMode) { return 99; } else { return _playerProgression; } } }
    public bool IsRelaunchable => _isRelaunchable;
    public float EdgeRadius => _radius;
}
public interface ICondition 
{ 
    public bool IsComplete();
    public virtual void Initialize() { } 
}
public class Kill(Entity[] _entity) : ICondition
{
    public bool IsComplete()
    {
        bool isComplete = true;
        foreach(var entity in _entity)
        {
            isComplete &= entity.isExpired;   
        }
        return isComplete;
    }
}
public class Protect(Entity[] _entity) : ICondition
{
    public bool IsComplete()
    {
        bool isComplete = true;
        foreach(var entity in _entity)
        {
            isComplete &= !entity.isExpired;   
        }
        if(!isComplete)
        {
            Engine.SaveGame.CurrentMission.FailMission();
        }
        return isComplete;
    }
}
public class Custom(Entity _entity) : ICondition
{
    private bool isDone;
    public bool IsComplete()
    {
        return isDone;
    }
    public void CustomCompleteRule(Entity _targetEntity)
    {
        if(_entity == _targetEntity)
        {
            isDone = true;
        }
    }
}
public class WaveGoal(int _targetWave) : ICondition
{
    public bool IsComplete() 
    {
        return Engine.SaveGame.CurrentMission.Wave > _targetWave;
    }
}
public class DialogueCondition(Dialogue[] _dialogues) : ICondition
{
    private bool isComplete = false;
    public void Initialize() 
    {
        foreach(var dialogue in _dialogues)
        {
            Engine.DialogueManager.Add(dialogue);
        }
        isComplete = true;
    }
    public bool IsComplete()
    {
        return isComplete;
    }
}