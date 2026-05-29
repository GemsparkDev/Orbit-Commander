using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OrbitCommander.Story;
using OrbitCommander.Particles;
using OrbitCommander.MissionComponents;
using OrbitCommander.Entities;
using OrbitCommander.Components;
using System.Diagnostics;
using static OrbitCommander.Core.Util;
//using System.Numerics;

namespace OrbitCommander.Core;
public class Mission
{
    private static Player Player => Engine.SaveGame.Player;
    //Maximum distance for any detection when sensing = stealth
    public static float StealthRange { get; private set; } = 750;
    //Threshold of detection for enemies
    public static float StealthThreshold { get; private set; } = 0.75f;
    public static readonly List<(MissionData data, Func<Mission> instance)> missions =
    [
        //T1

        (new MissionData("Crash Landing","The crash landing site. Objective: Explore the system.",160, [], 0, 2000, 1),
        delegate(){Entity m; 
        return new Mission([
            new Planet(new Vector2(1000, 0), Planet.GetOrbitalVelocity(new Vector2(1000, 0), Vector2.Zero, 10000), 250, 1.5f, false, Color.Cyan),
            new Planet(Vector2.Zero, Vector2.Zero, 10000, 8, true, Color.Cyan),
            new IntroCutscene(RestartCutscene),
            new Tip("WASD to move, Space to dock and undock.\nRmb to collect scrap, Lmb to shoot.", new Vector2(0, 9*50)),
            ItemFactory.NewScrap(new Vector2(0, -8*50), new Vector2(10, -10), 0.07f),
            ItemFactory.NewScrap(new Vector2(0, -8*50), new Vector2(-8, -4), -0.03f),
            m = Entity.NewMothership(new Vector2(0, -8*50 - Assets.DimsOf(Sprites.Mothership).Y / 2), Vector2.Zero, 0f),],
            new Conditional([new Protect([m]), new Custom(m)], Win(DayOneLog)),
            new CustomSpawner(new Vector2(0, -8*50 - Assets.DimsOf(Sprites.Mothership).Y / 2)));}),

        //TODO: Add custom "Humanlike" enemies
        (new MissionData("Crossfire", "Sensors indicate a group fighting against the same hostiles encountered during our crash landing.\nAiding them could gain us a powerful ally.", 140, [0], 0, 2000, 1),
        delegate(){var p = new Planet(Vector2.Zero, Vector2.Zero, 15000, 12, true, Color.White); Entity e; return new([
            p.AddComponent(new Ring(p) { Mass = 15000 }),
            Entity.NewEnemySpawner(new Vector2(1200, 0), Planet.GetOrbitalVelocity(new Vector2(1200, 0), Vector2.Zero, 20000), 0, Team.Friendly),
            Entity.NewEnemySpawner(new Vector2(-1200, 0), Planet.GetOrbitalVelocity(new Vector2(-1200, 0), Vector2.Zero, 20000), 0, Team.Friendly),
            Entity.NewCarrier(new Vector2(50, -750), Vector2.Zero, 0, Team.Friendly),
            Entity.NewCarrier(new Vector2(-50, -750), Vector2.Zero, 0, Team.Friendly),
            Entity.NewSymmetryBoss(new Vector2(700, 0), Vector2.Zero, 0, Team.Friendly),
            Entity.NewTurret(new Vector2(MathF.Sin(0.3f), -MathF.Cos(0.3f)) * 12 * 50, Vector2.Zero, 0.3f, Team.Friendly),
            Entity.NewTurret(new Vector2(MathF.Sin(-0.3f), -MathF.Cos(-0.3f)) * 12 * 50, Vector2.Zero, -0.3f, Team.Friendly),
            e = Entity.NewCrashedShip(new Vector2(-6000, -6640), Vector2.Zero, -MathF.PI * 3 / 8),
            new WaveSpawner(T1, 0.18f, true),
            new IntroCutscene(QueueCrossfireDialogue),
        ], new Conditional([new Protect([e]), new Custom(e)], SendPickup(2000, RepairCrashedShip)),
        new DropSpawner(4000));}),

        (new("Showdown", "Our activities appear to have gathered the attention of an advanced drone.\nDefeat it to move to the next system.",
        50, [1], 0, 2000, 2),
        delegate(){Entity a;return new([
            new Planet(Vector2.Zero, Vector2.Zero, 5000, 3, true, Color.Cyan),
            new Planet(new Vector2(400, 0), Planet.GetOrbitalVelocity(new Vector2(400, 0), Vector2.Zero, 5000), 240, 1f, false, Color.Cyan),
            new Planet(new Vector2(-600, 0), -Planet.GetOrbitalVelocity(new Vector2(-600, 0), Vector2.Zero, 5000) * 1.2f, 120, 0.6f, false, Color.Yellow),
            new WaveSpawner(T1, 1f, true),
            a=Entity.NewScrambled(new Vector2(0, -6*50), Vector2.Zero, 0),],
            new Conditional([new Kill([a])], SendPickup(2000)),
            new DropSpawner(1500));}),

        (new("Warp Gate", "Scans indicate that a large enemy fleet is coming our way after the loss of their prototype.\nRecommended action: Leave the system immediately.", 170, [2], 0, 2000, 3),
        delegate(){Entity a;return new([a=Entity.NewWarpGate(Vector2.Zero, Vector2.Zero, 0)], new Conditional([new Custom(a)], Win()), new PlayerSpawner(new Vector2(0, 500)), Sound.None);}),

        //T2

        (new("Assault", "It appears the enemy has improved their fleet, and has pushed the mothership to a non-ideal location.\nDefend the mothership and defeat the fortified miner base on this planet.", 150, [], 1, 2000),
        delegate(){var p = new Planet(Vector2.Zero, Vector2.Zero, 20000, 9f, true, Color.Cyan);
            Entity a,b,c,d,e,f,g;return new([
            p.AddComponent(new Atmosphere(p, 1.8f, 20000)),
            new Planet(new Vector2(0, 1800), Planet.GetOrbitalVelocity(new Vector2(0, 1800), Vector2.Zero, 20000), 1500, 2f, false, Color.Cyan),
            a=Entity.NewTurret(new Vector2(MathF.Sin(5.5f), -MathF.Cos(5.5f)) * 9 * 50, Vector2.Zero, 5.5f, Team.Hostile),
            b=Entity.NewTurret(new Vector2(MathF.Sin(3.2f), -MathF.Cos(3.2f)) * 9 * 50, Vector2.Zero, 3.2f, Team.Hostile),
            c=Entity.NewTurret(new Vector2(MathF.Sin(2.6f), -MathF.Cos(2.6f)) * 9 * 50, Vector2.Zero, 2.6f, Team.Hostile),
            d=Entity.NewTurret(new Vector2(MathF.Sin(1.1f), -MathF.Cos(1.1f)) * 9 * 50, Vector2.Zero, 1.1f, Team.Hostile),
            e=Entity.NewMiner(new Vector2(MathF.Sin(3), -MathF.Cos(3)) * 9 * 50, Vector2.Zero, 3, Team.Hostile),
            f=Entity.NewMiner(new Vector2(MathF.Sin(5.2f), -MathF.Cos(5.2f)) * 9 * 50, Vector2.Zero, 5.2f, Team.Hostile),
            g=Entity.NewOrbiter(new Vector2(0, 1650), Planet.GetOrbitalVelocity(new Vector2(0, 1650), new Vector2(0, 1800), 1500)
                + Planet.GetOrbitalVelocity(new Vector2(0, 1800), Vector2.Zero, 20000), 0),
            new WaveSpawner(T2, 0.75f, true),
            new Tip("Press C to open the construct menu.\nEach construct requires one scrap to craft.", new Vector2(0, -9*50)),
            ], new Conditional([new Kill([a,b,c,d,e,f]), new Protect([g])], Win()),
             new CustomSpawner(new Vector2(0, 1650)));}),

        (new("Gas Giant", "", 10, [4], 1, 2000),
        delegate(){var p = new Planet(Vector2.Zero, Vector2.Zero, 16000, 4, true, new Color(167, 156, 134));
            return new([
            p.AddComponent(new Atmosphere(p, 6, 16000) { IsSun = true })
             .AddComponent(new Ring(p) { Offset = 500, Mass = 16000 }),
            new Planet(new Vector2(900, 0), Planet.GetOrbitalVelocity(new Vector2(900, 0), Vector2.Zero, 16000), 100, 0.5f, false, Color.OldLace),
            new Planet(new Vector2(-1200, 0), Planet.GetOrbitalVelocity(new Vector2(-1200, 0), Vector2.Zero, 16000) * 1.05f, 100, 0.5f, false, Color.OldLace),
            new WaveSpawner(T1, 1f, false),
            ], new Conditional([new WaveGoal(30)], SendPickup(2000)), new GliderSpawner(new Vector2(-800, -1100), -900));}),

        (new("Flight of the bumblebee.", "The enemy fleet's fastest fighter appears to have arrived to this planet and is blocking our path.\nDefeating it appears to be the only way forward.", 150, [5], 1, 2000),
        delegate(){Entity a; return new([
            new Planet(Vector2.Zero, Vector2.Zero, 5000, 4.5f, true, Color.Cyan),
            new Planet(new Vector2(600, 0), Planet.GetOrbitalVelocity(new Vector2(600, 0), Vector2.Zero, 5000), 240, 1f, false, Color.Cyan),
            new Planet(new Vector2(-600, 0), Planet.GetOrbitalVelocity(new Vector2(-600, 0), Vector2.Zero, 5000), 240, 1f, false, Color.Cyan),
            new WaveSpawner(T2, 1.1f, false),
            a=Entity.NewExodusBoss(new Vector2(0, -6*50), Vector2.Zero, 0, Team.Hostile),
            ], new Conditional([new Kill([a])], SendPickup(2000)),
            new DropSpawner(1500));}),

        (new("Warp Gate", "The enemy fleet is still hot on our tail.", 100, [6], 1, 2000, 3, true),
        delegate(){Entity a;return new([
            new Planet(Vector2.Zero, Vector2.Zero, 150, 3, true, Color.OldLace),
            new Tip("Press left shift to return to the previous system. Press right shift to enter the next system.", new Vector2(0, 3 * 50)),
            a=Entity.NewWarpGate(new Vector2(0, 450), -Planet.GetOrbitalVelocity(new Vector2(0, 450), Vector2.Zero, 150), 0),
            ], new Conditional([new Custom(a)], Win()),
            new PlayerSpawner(new Vector2(0, 500)), Sound.None);}),

        //T3

        (new("Black Hole", "", 0, [], 2, 2000),
        delegate(){return new Mission([
            new Planet(new Vector2(0, 500), Vector2.Zero, 5000, 0.1f, true, Color.Black),
            ItemFactory.NewScrap(new Vector2(0, -8*50), Vector2.Zero, -0.03f),
            Entity.NewFighter(Vector2.Zero, Vector2.Zero, 0, Team.Hostile )],
            new Conditional([new WaveGoal(10)], SendPickup(2000)),
            new GliderSpawner(new Vector2(-1000, -700), -1000), Sound.None);}),

        (new("Binary system", "It seems plans for a mass relay have been abandoned here.\nConstruct it to recieve some advanced equipment from our previous stations.", 0, [8], 2, 2000),
        delegate(){Entity a;return new([
            new Planet(new Vector2(500, 0), new Vector2(0, 1.05f), 10000, 7, false, Color.Cyan) { Temperature = -5},
            new Planet(new Vector2(-1000, 0), new Vector2(0, -2.1f), 5000, 4f, false, Color.Orange) { Temperature = 5 },
            new WaveSpawner(T3, 1, true),
            a=Entity.MassRelay(Vector2.Zero, Vector2.Zero, 0)],
            new Conditional([new Protect([a]), new Custom(a)], SendPickup(2000)),
            new DropSpawner(1500));}),

        (new("Veiled", "Sensors indicate that our actions have been spied on by the enemy.\nDestroy it.",  0, [9], 2, 2000),
        delegate(){Entity a;return new([
            new Planet(Vector2.Zero, Vector2.Zero, 4000, 4.5f, true, new Color(0.03f, 0.05f, 0.08f)),
            new Planet(new Vector2(600, 0), Planet.GetOrbitalVelocity(new Vector2(600, 0), Vector2.Zero, 4000) * 1.05f, 500, 1.5f, false, new Color(0.06f, 0.08f, 0.12f)),
            new WaveSpawner(T3, 1.1f, false),
            a=Entity.NewVeilBoss(new Vector2(0, -6*50), Vector2.Zero, 0),
            ], new Conditional([new Kill([a])], SendPickup(2000)),
            new DropSpawner(1500));}),

        (new("Inferno", "Your intel has led you here. Finish this.",  0, [10], 2, 2000),
        delegate(){var p = new Planet(Vector2.Zero, Vector2.Zero, 160000, 6, true, new Color(0.9f, 1f, 0.75f)) { Temperature = 5 };
            Entity a; return new([
            p.AddComponent(new Atmosphere(p, 50, 160000) { IsSun = true }),
            new Planet(new Vector2(0, 2000), Planet.GetOrbitalVelocity(new Vector2(0, 2000) * 0.99f, Vector2.Zero, 16-000), 8000, 4, false, new Color(0.95f, 0.2f, 0.1f)),
            a=Entity.NewEpitomeBoss(new Vector2(0, 2800), Vector2.Zero, 0)],
            new Conditional([new Kill([a])], SendPickup(2000)),
            new GliderSpawner(new Vector2(-1500, -2000), -1500), Sound.None);}),

        //Epilogue

        (new("Sentry Defense", "We've been assigned to defend this small planet.\n *Repair will not be available for this mission*",
        100, [], 3, 2000, 1),
        delegate(){var p = new Planet(Vector2.Zero, Vector2.Zero, 3500, 4, true, Color.Cyan); Entity a,b;return new([
            p.AddComponent(new Ring(p) { Mass = 3500 }),
            new WaveSpawner(T1, 0.75f, true),
            new IntroCutscene(SentryDialogue),
            a=Entity.NewTurret(new Vector2(0, -200 - Assets.DimsOf(Sprites.TurretBase).Y / 2), Vector2.Zero, 0),
            b=Entity.NewOrbiter(new Vector2(400, 0), Planet.GetOrbitalVelocity(new Vector2(400, 0), Vector2.Zero, 3500), 0),
        ], new Conditional([new Protect([a,b]), new WaveGoal(30)], SendPickup(2000)), 
         new DropSpawner(1500));} ),

        (new("Meet the locals", "Local scans indicate a nearby mineral rich planet occupied by enemy forces. \nCapturing this site will aid in future resource gathering.",
        400, [], 3, 2000, 2),
        delegate(){Entity a;return new([
            new Planet(Vector2.Zero, Vector2.Zero, 15000, 6f, true, Color.Cyan),
            new Planet(new Vector2(0, 800), Planet.GetOrbitalVelocity(new Vector2(0, 800), Vector2.Zero, 15000) * 0.85f, 1000, 1f, false, Color.Cyan),
            new Tip("Press Q to use your special ability.\nCtrl to toggle aim assist.", new Vector2(0, -6*50)),
            new WaveSpawner(T1, 0.75f, true),
            a=Entity.NewLargeMiner(new Vector2(0, -6*50 - Assets.Get(Sprites.LargeMiner).Height/2), Vector2.Zero, 0)
        ], new Conditional([new Kill([a])],SendPickup(2000)), 
        new DropSpawner(1500));}),

        (new("???", "Sensor data shows that this site has an unusually high temperature.\nInvestigate possible enemy interferance.", 145, [], 3, 2000, 3, true),
        //Note: The player construct menu and the Quantum Resonator both use the name of this mission for their special behavior. When changing, make sure their name is updated as well.
        delegate(){var p = new Planet(Vector2.Zero, Vector2.Zero, 50000, 12, true, new Color(255, 219, 0)) { Temperature = 0.5f };
            return new([
            p.AddComponent(new Atmosphere(p, 1.5f, 50000))
            .AddComponent(new Ring(p) { Mass = 50000 }),], 
            new Conditional([], SendPickup(2000)),
            new PlayerSpawner(new Vector2(-2000, -2000)), Sound.None);}),

        (new("Base of Operations", "We have deployed several communication stations to this site.\nProtect the location for future development.", 130, [], 3, 2000),
        delegate(){var p = new Planet(Vector2.Zero, Vector2.Zero, 30000, 10f, true, Color.HotPink); Entity a,b,c; return new([
            p.AddComponent(new Ring(p) { Mass = 30000 }),
            new WaveSpawner(T2, 1, false),
            a=Entity.NewCommunicator(new Vector2(MathF.Sin(1.02f), -MathF.Cos(1.02f)), Vector2.Zero, 1.02f, Team.Friendly),
            b=Entity.NewCommunicator(new Vector2(MathF.Sin(2.7f), -MathF.Cos(2.7f)), Vector2.Zero, 2.7f, Team.Friendly),
            c=Entity.NewCommunicator(new Vector2(MathF.Sin(5.33f), -MathF.Cos(5.33f)), Vector2.Zero, 5.33f, Team.Friendly),
            ], new Conditional([new Protect([a, b, c]), new WaveGoal(30),
        ], SendPickup(2000)), new DropSpawner(500));}),

        (new("Extraction", "Our success has led us to deploying a miner on this deceptively dense planet.\nDefend it from the incoming enemy forces.", 200, [], 3, 2000),
        delegate(){Entity a;return new([
            new Planet(Vector2.Zero, Vector2.Zero, 25000, 7f, true, Color.Cyan), 
            new Planet(new Vector2(800, 0), Planet.GetOrbitalVelocity(new Vector2(800, 0), Vector2.Zero, 25000), 150, 0.5f, false, Color.Cyan),
            new WaveSpawner(T2, 1, true),
            a=Entity.NewMiner(new Vector2(0, -7*50), Vector2.Zero, 0),
            ], new Conditional([new Protect([a]), new WaveGoal(30)], Win()), 
             new DropSpawner(1500));}),

        (new("Trader", "This friendly trader invites us to upgrade our modules in exchange for resources", 80, [], 3, 2000, 3, true),
        delegate(){var p = new Planet(Vector2.Zero, Vector2.Zero, 6000, 6, true, Color.Cyan);
            return new([
            p.AddComponent(new Atmosphere(p, 0.8f, 6000))
             .AddComponent(new Ring(p) { Mass = 6000 }),
            Entity.NewTrader(new Vector2(0, 500), Planet.GetOrbitalVelocity(new Vector2(0, 500), Vector2.Zero, 6000), 0),],
            SendPickup(2000)(),
            new PlayerSpawner(new Vector2(-2000, -2000)));}),

        (new("Clockwork creation", "A strange sentinal appears to be housed on this ancient planet.\nDismantling it may yield unusual resources.", 60, [], 3, 2000),
        delegate(){var p = new Planet(Vector2.Zero, Vector2.Zero, 4000, 3, true, Color.Wheat);
            Entity a;return new([
            p.AddComponent(new Atmosphere(p, 1.5f, 4000)),
            new Planet(new Vector2(500, 0), Planet.GetOrbitalVelocity(new Vector2(500, 0), Vector2.Zero, 4000), 300, 1f, false, Color.Wheat),
            new WaveSpawner(T2, 1f, true),
            a=Entity.NewClockworkBoss(new Vector2(0, -6*50), Vector2.Zero, 0, Team.Hostile)], 
            new Conditional([new Kill([a])], SendPickup(2000)),
            new DropSpawner(1500));}),

        (new("Ice giant", "The unusual conditions in this system have resulted in unique developments in the enemies technology.\nBe prepared for advanced enemy cloaking.", 0, [], 3, 2000),
        delegate(){var p = new Planet(Vector2.Zero, Vector2.Zero, 10000, 3, true, new Color(41, 144, 181)) { Temperature = -20 };
            return new([
            p.AddComponent(new Atmosphere(p, 15, 10000) { IsSun = true }),
            new WaveSpawner(T2, 1f, false),], 
            new Conditional([new WaveGoal(30)], SendPickup(2000)),
            new GliderSpawner(new Vector2(-800, -1300), -1000));}),

        (new("Hack", "The enemy has set up a mesh node network for storing information.\nHack it to discover the location of their leader.", 0, [], 3, 2000),
        delegate(){var p = new Planet(Vector2.Zero, Vector2.Zero, 18000, 6f, true, Color.Cyan) { Temperature = -2 };
            Entity a,b,c;return new([
            p.AddComponent(new Atmosphere(p, 1.5f, 18000)),
            new Planet(new Vector2(1100, 0), Planet.GetOrbitalVelocity(new Vector2(1100, 0), Vector2.Zero, 18000), 1500, 1.5f, false, Color.Cyan) { Temperature = -2 },
            new WaveSpawner(T3, 0.8f, true),
            a=Entity.NewMeshNetworkNode(new Vector2(0, -1), Vector2.Zero, 0),
            b=Entity.NewMeshNetworkNode(new Vector2(1250, 0),-Planet.GetOrbitalVelocity(new Vector2(150, 0),
                Vector2.Zero, 1500) + Planet.GetOrbitalVelocity(new Vector2(1100, 0), Vector2.Zero, 18000), 0),
            c=Entity.NewMeshNetworkNode(new Vector2(-500, 0), -Planet.GetOrbitalVelocity(new Vector2(-500, 0), Vector2.Zero, 18000), 0)], 
            new Conditional([new Protect([a,b,c]), new Custom(a), new Custom(b), new Custom(c)], SendPickup(2000)),
            new DropSpawner(1500));}),

        (new("BEEG", "", 0, [], 3, 2000),
        delegate(){var p = new Planet(Vector2.Zero, Vector2.Zero, 8000000, 100, true, new Color(1f, 0.8f, 0.5f)) { Temperature = 5 };
            return new([
            p.AddComponent(new Atmosphere(p, 10, 8000000) { IsSun = true }),
            new Colliders(SolarStation),
            new WaveSpawner(T3, 1, false),], 
            new Conditional([new WaveGoal(10)], SendPickup(2000)),
            new GliderSpawner(new Vector2(-1000, -7000), -7000), Sound.None);}),

        (new("Last Stand", "Survive", 2000, [], 3, 4, 3, true),
        delegate(){var p = new Planet(Vector2.Zero, Vector2.Zero, 20000, 9, true, Color.OrangeRed);  return new Mission([
            p.AddComponent(new Ring(p) { Offset = 1.5f, Mass = 20000 }),
            new Planet(new Vector2(1200, 0), Planet.GetOrbitalVelocity(new Vector2(1200, 0), Vector2.Zero, 20000), 750, 2f, false, Color.Red),
            new Tip("You can now construct the makeshift mothership in the construct menu.\n Requires 3 scrap.", new Vector2(0, -10 * 50)),
            new WaveSpawner(All, 0.25f, true)], 
            new Conditional([new WaveGoal(1000)], SendPickup(2000)),
            new DropSpawner(1500)); }),

        //Enemy planet test
        (new("Enemy Planet Test", "", 200, [], 3, 2000, 0),
        delegate(){
            var p = new Planet(Vector2.Zero, Vector2.Zero, 10000, 8, true, Color.Red);
            var l = new Entity(new Vector2(0, 1000), Vector2.Zero, 0, 0);
            var k = ItemFactory.NewScrap(new Vector2(0, -1000), Vector2.Zero, 0);
        return new([
            k, l.AddComponent(new Sprite(l, Color.White) { Texture = Assets.Get(Sprites.MetalScrap) })
            .AddComponent(new Lock(l) { Key = k }),
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
            new Conditional([new Kill([p])], SendPickup(2000)),
            new DropSpawner(1500)); }),
    ];
    public List<Entity> Entities { get; private set; } = [];
    private List<Entity> enemies = [];
    private List<Entity> projectiles = [];
    public int Wave { get; set; } = 0;
    public Mission(List<IMissionComponent> _components, Conditional _objective, IPlayerSpawner _spawner, Sound _music = Sound.main)
    {
        Events.SetModules();
        components = _components;
        foreach(var comp in _components)
        {
            if(comp is IObstacle)
            {
                obstacles.Add(comp as IObstacle);
            }
            if (comp is Entity entity)
            {
                Entities.Add(entity);
                if (entity.HasComponent<Attack>())
                {
                    projectiles.Add(entity);
                }
                if (entity.HasComponent<Health>() || entity is Player)
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
    private bool isUpdating = false;
    private List<IMissionComponent> components = [];
    private List<IMissionComponent> addedComponents = [];
    private List<IObstacle> obstacles = [];
    public void Add(IMissionComponent component)
    {
        if (!isUpdating)
        {
            components.Add(component);
            if (component is IObstacle)
            {
                obstacles.Add(component as IObstacle);
            }
            if (component is Entity entity)
            {
                //Checks the entity type, and adds it to the corresponding list for each type
                Entities.Add(entity);
                if (entity.HasComponent<Attack>())
                {
                    projectiles.Add(entity);
                }
                if (entity.HasComponent<Health>() || entity is Player)
                {
                    enemies.Add(entity);
                }
            }
        }
        else
        {
            //Moves entities to the inactive list to prevent modifying a list while iterating
            addedComponents.Add(component);
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
        SoundManager.ChangeTrack(Assets.Get(music));

        Engine.SaveGame.Player.dockedEntity = null;
        spawner.Spawn();
        Engine.Camera.Position = Player.Position;

        foreach (var comp in components)
        {
            comp.Initialize();
        }
        objective.Initialize();

        //Easter Egg
        if(Util.Random.Next(0, 10000) == 0)
        {
            foreach (var planet in Entities)
            {
                if (planet is Planet p)
                {
                    p.EasterEgg = true;
                }
            }
        }
    }
    public void IngameUpdate()
    {
        Player.Update();
        if (!Player.IsDocked && Player.Progression > -1)
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
            Events.UpdateModulesStatus();
            Events.MissionSelectTrigger(new MissionSelect());
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
        Entity[] importantEntites = Engine.SaveGame.CurrentMission.Entities.Where(x => x.HasTag(Tags.IsImportant)).ToArray();
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
        components = [.. components.Where(x => x as Entity == null || !(x as Entity).isExpired)];
        Entities = [.. Entities.Where(x => !x.isExpired)];
        projectiles = [.. projectiles.Where(x => !x.isExpired)];
        enemies = [.. enemies.Where(x => !x.isExpired)];

        isUpdating = false;

        //Moves all newly created entities to the main list
        foreach (var entity in addedComponents)
        {
            Add(entity);
        }
        addedComponents.Clear();
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
        //Allows for explosions on the player
        if (dist < _radius && dist > float.Epsilon)
        {
            Player.Collide(_damage);
        }
    }
    public Dockable NearestDockableEntity(Entity _entity)
    {
        return Nearest(_entity.Position, GetEntities<Dockable>())?.GetComponent<Dockable>();
    }
    public Entity NearestEnemy(Entity entity, bool _getDeadEnemies = false)
    {
        Entity[] entities = [.. enemies.Where(x => IsEligible(x))];
        float maxDistSqr = StealthRange * StealthRange * StealthThreshold * StealthThreshold;
        float nearestDistance = float.MaxValue;
        Entity returnEnemy = null;
        foreach (var targetEnemy in entities)
        {
            float stealth = targetEnemy.GetComponent<Stealth>()?.StealthAbility ?? 0;
            float distance = Vector2.DistanceSquared(entity.Position, targetEnemy.Position);
            if (distance < nearestDistance && (stealth < entity.SensingAbility || distance < maxDistSqr))
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
            if (distance < nearestDistance && (Player.StealthAbility < entity.SensingAbility || distance < maxDistSqr))
            {
                returnEnemy = Player;
            }
        }
        return returnEnemy;
        bool IsEligible(Entity targetEnemy)
        {
            if(targetEnemy == null || targetEnemy.IsFriendly(entity))
            {
                return false;
            }
            return _getDeadEnemies || targetEnemy.Health > 0;
        }
    }
    public Entity NearestAlly(Entity entity)
    {
        Entity[] entities = [.. Entities.Where(IsEligible)];
        var returnEnemy = Nearest(entity.Position, entities);
        float nearestDistance = float.MaxValue;
        if (returnEnemy != null)
        {
            nearestDistance = Vector2.DistanceSquared(entity.Position, returnEnemy.Position);
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
        bool IsEligible(Entity targetEnemy)
        {
            return (entity.IsFriendly(targetEnemy)) && (targetEnemy != entity) && !(targetEnemy.HasTag(Tags.IsChild));
        }
    }
    public Entity NearestItem(Entity entity, bool _findAll)
    {
        return Util.Nearest(entity.Position, [.. Entities.Where(IsEligible)]);
        bool IsEligible(Entity targetEntity)
        {
            return !(targetEntity is not Pickup || targetEntity == entity || !_findAll && (targetEntity is Module || targetEntity.HasTag(Tags.IsSpecialized) || targetEntity.HasComponent<Behaviour>()));
        }
    }
    public Entity NearestProjectile(Vector2 _position, int _sensingAbility, Team _team)
    {
        float nearestDistance = float.MaxValue;
        Entity returnProjectile = null;
        foreach (var targetProjectile in projectiles)
        {
            float distance = Vector2.DistanceSquared(targetProjectile.Position, _position);
            if (distance < nearestDistance && _team != targetProjectile.Team
                && (targetProjectile.StealthAbility < _sensingAbility || targetProjectile.StealthAbility == _sensingAbility && distance < StealthRange * StealthRange * StealthThreshold * StealthThreshold))
            {
                nearestDistance = distance;
                returnProjectile = targetProjectile;
            }
        }
        return returnProjectile;
    }
    public Entity[] GetEntities<T>() where T : IComponent
    {
        return [.. Entities.Where(x => x.HasComponent<T>())];
    }
    public List<Entity> Hitscan(Vector2 _pos, Vector2 _dir, float _maxLength, bool _getAll, out Vector2 _end, Team[] _whitelist = null, bool _getProjectiles = false)
    {
        var dir = Vector2.Normalize(_dir);
        var list = new List<Entity>();
        float dist = _maxLength;
        Entity nearestEnemy = null;
        Engine.SaveGame.CurrentMission.IsColliding(_pos, dir * dist, 1, false, out float maxDist);
        maxDist += 1; //Makes hitting planets possible
        foreach (var entity in Entities)
        {
            if(_getProjectiles || entity.HasComponent<Health>())
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
            float closestLength = relativePos.X * dir.X + relativePos.Y * dir.Y;
            float closestDistance = Vector2.Distance(dir * closestLength + _pos, _entity.Position);            
            if (closestLength > 0 && closestDistance < _entity.ColliderRadius)
            {
                float discriminant = MathF.Sqrt(_entity.ColliderRadius * _entity.ColliderRadius - closestDistance * closestDistance);
                if(closestLength - discriminant > maxDist)
                {
                    return;
                }
                if (_getAll)
                {
                    list.Add(_entity);
                }
                else
                {
                    if (dist > closestLength - discriminant)
                    {
                        dist = closestLength - discriminant;
                        nearestEnemy = _entity;
                    }
                }
            }
        }
    }
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
            var comp = entity.GetComponent<Atmosphere>();
            if(comp != null)
            {
                sum += comp.GetAtmosphereDensity(_entity);
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
        Entity[] planets = [.. Engine.SaveGame.CurrentMission.Entities.Where(x => x is Planet)];
        ICollider[] Colliders = [];
        var comp = GetComponent<Colliders>();
        if (comp != null)
        {
            Colliders = comp.GetColliders;
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
                    if (!planet.Transform.IsImmovable)
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
                    var p = planets[i] as Planet;
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
        Vector2 acceleration = Vector2.Zero;
        foreach (var entity in Engine.SaveGame.CurrentMission.Entities)
        {
            if (entity is Planet planet)
            {
                acceleration -= planet.GetAcceleration(_position);
            }
        }
        return acceleration;
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
        foreach(var comp in components)
        {
            comp.Draw(_spriteBatch);
        }
        if (Engine.SaveGame.Player.Position.Length() > missions[Engine.SaveGame.CurrentMissionIndex].data.EdgeRadius)
        {
            _spriteBatch.Draw(Assets.Get(Sprites.Arrow), Engine.SaveGame.Player.Position - Vector2.Normalize(Engine.SaveGame.Player.Position) * 25, null, Engine.SaveGame.Player.Color, Util.ToAngle(-Engine.SaveGame.Player.Position), Assets.DimsOf(Sprites.Arrow) / 2, 1, 0, 0.2f);
            _spriteBatch.DrawString(Assets.TextFont, "Return to planet.", Engine.Camera.Position - new Vector2(Assets.TextFont.MeasureString("Return to planet.").X / 2, 225), Color.Crimson);
        }
        Player.Draw(_spriteBatch);
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