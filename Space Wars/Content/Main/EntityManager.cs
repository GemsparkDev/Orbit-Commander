using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Components;
using Space_Wars.Content.Main.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using UILib.Content.Main;
using Space_Wars.Content.Main.Story;
using Microsoft.Xna.Framework.Audio;
using System.Diagnostics;

namespace Space_Wars.Content.Main;

public class EntityManager
{
    private bool isUpdating = false;
    private List<Entity> entities = [];
    public List<Entity> Entities { get { return entities; } }
    private List<Entity> addedEntities = [];
    private List<Entity> enemies = [];
    private List<Projectile> projectiles = [];
    private static float currentKarma = 0;
    private static Player Player => Engine.SaveGame.Player;
    //Maximum distance for any detection when sensing = stealth
    public static float StealthRange { get; private set; } = 750;
    //Threshold of detection for enemies
    public static float StealthThreshold { get; private set; } = 0.75f;
    public int MissionLength => missions.Count;
    private readonly List<Mission> missions =
    [        
        new Mission([new Planet(Vector2.Zero, Vector2.Zero, 10000, 8, true, Color.Cyan, false),
        new Planet(new Vector2(1000, 0), Planet.GetOrbitalVelocity(new Vector2(1000, 0), Vector2.Zero, 10000), 250, 1.5f, false, Color.Cyan) ],
        [ new EntityCondition(new EntityConstructor(Enemy.NewMothership, new Vector2(0, -8*50 - Assets.DimsOf(Sprite.Mothership).Y / 2), Vector2.Zero, 0f), [ Condition.Protect, Condition.CustomIncomplete ]),
        new EntityCondition(new PickupConstructor(ItemFactory.NewScrap, new Vector2(0, -8*50), new Vector2(10, -10), 0.07f),[]),
        new EntityCondition(new PickupConstructor(ItemFactory.NewScrap, new Vector2(0, -8*50), new Vector2(-8, -4), -0.03f),[])],
        "Crash Landing",
        "The crash landing site. Objective: Explore the system.",
        -1, new Vector2(0, -8*50 - Assets.DimsOf(Sprite.Mothership).Y / 2), Mission.TierOne(), Mission.TierOneBosses(), RestartCutscene, false, DayOneLog) { playerProgression = 0, playerDocked = true, tip = "WASD to move, Space to dock and undock.\nRmb to collect scrap, Lmb to shoot." },

        //Note: Add custom "Humanlike" enemies
        new Mission([new Planet(Vector2.Zero, Vector2.Zero, 15000, 12, true, Color.White, true)],
        [
        new EntityCondition(new AdvancedConstructor(Enemy.NewEnemySpawner, new Vector2(1200, 0), Planet.GetOrbitalVelocity(new Vector2(1200, 0), Vector2.Zero, 20000), 0, true), []),
        new EntityCondition(new AdvancedConstructor(Enemy.NewEnemySpawner, new Vector2(-1200, 0), Planet.GetOrbitalVelocity(new Vector2(-1200, 0), Vector2.Zero, 20000), 0, true), []),
        new EntityCondition(new AdvancedConstructor(Enemy.NewCarrier, new Vector2(50, -750), Vector2.Zero, 0, true), []),
        new EntityCondition(new AdvancedConstructor(Enemy.NewCarrier, new Vector2(-50, -750), Vector2.Zero, 0, true), []),
        new EntityCondition(new AdvancedConstructor(Enemy.NewSymmetryBoss, new Vector2(700, 0), Vector2.Zero, 0, true), []),
        new EntityCondition(new AdvancedConstructor(Enemy.NewTurret, new Vector2(MathF.Sin(0.3f), -MathF.Cos(0.3f)) * 12 * 50, Vector2.Zero, 0.3f, true), []),
        new EntityCondition(new AdvancedConstructor(Enemy.NewTurret, new Vector2(MathF.Sin(-0.3f), -MathF.Cos(-0.3f)) * 12 * 50, Vector2.Zero, -0.3f, true), []),
        new EntityCondition(new LaunchConstructor(Enemy.NewDropPod, new Vector2(0,-4000), 600), []),
        new EntityCondition(new EntityConstructor(Enemy.NewCrashedShip, new Vector2(-6000, -6640), Vector2.Zero, -MathF.PI * 3 / 8), [Condition.Protect, Condition.CustomIncomplete]),
        ], "Crossfire", "Sensors indicate a group fighting against the same hostiles encountered during our crash landing.\nAiding them could gain us a powerful ally.", 0.18f, new Vector2(0, -4000), Mission.TierOne(), Mission.TierOneBosses(), QueueCrossfireDialogue, true, RepairCrashedShip) 
        { isAggressive = true, playerProgression = 2, playerDocked = true },

        new Mission( [new Planet(Vector2.Zero, Vector2.Zero, 3500, 4, true, Color.Cyan, true) ],
        [
            new EntityCondition(new EntityConstructor(Enemy.NewTurret, new Vector2(0, -200 - Assets.DimsOf(Sprite.TurretBase).Y / 2), Vector2.Zero, 0), [ Condition.Protect ]),
            new EntityCondition(new EntityConstructor(Enemy.NewOrbiter, new Vector2(400, 0), Planet.GetOrbitalVelocity(new Vector2(400, 0), Vector2.Zero, 3500), 0), [ Condition.Protect ]),
            new EntityCondition(new LaunchConstructor(Enemy.NewDropPod, new Vector2(0, -1500), 200),[ ]),
            new WaveGoal(30) ],
        "Sentry Defense", "We've been assigned to defend this small planet.\n *Repair will not be available for this mission*", 0.75f, new Vector2(0, -1500), Mission.TierOne(), Mission.TierOneBosses(), SentryDialogue)
        { playerProgression = 1, isAggressive = true, playerDocked = true },

        new Mission( [new Planet(Vector2.Zero, Vector2.Zero, 15000, 6f, true, Color.Cyan),
        new Planet(new Vector2(0, 800), Planet.GetOrbitalVelocity(new Vector2(0, 800), Vector2.Zero, 15000) * 0.85f, 1000, 1f, false, Color.Cyan), ],
        [
            new EntityCondition(new EntityConstructor(Enemy.NewLargeMiner, new Vector2(0, -6*50 - Assets.Get(Sprite.LargeMiner).Height/2), Vector2.Zero, 0), [ Condition.Kill ]),
            new EntityCondition(new LaunchConstructor(Enemy.NewDropPod,new Vector2(0, -1500), 300),[ ])],
        "Meet the locals", "Local scans indicate a nearby mineral rich planet occupied by enemy forces. \nCapturing this site will aid in future resource gathering.", 0.75f, new Vector2(0, -1500), Mission.TierOne(), Mission.TierOneBosses(), null, true)
        { playerProgression = 2, isAggressive = true, tip = "Press Q to use your special ability.\nCtrl to toggle aim assist.", playerDocked = true },

        new Mission([new Planet(Vector2.Zero, Vector2.Zero, 5000, 3, true, Color.Cyan),
            new Planet(new Vector2(400, 0), Planet.GetOrbitalVelocity(new Vector2(400, 0), Vector2.Zero, 5000), 240, 1f, false, Color.Cyan),
            new Planet(new Vector2(-600, 0), -Planet.GetOrbitalVelocity(new Vector2(-600, 0), Vector2.Zero, 5000) * 1.2f, 120, 0.6f, false, Color.Yellow), ],
        [
            new EntityCondition(new EntityConstructor(Enemy.NewScrambled, new Vector2(0, -6*50), Vector2.Zero, 0), [ Condition.Kill ]),
            new EntityCondition(new LaunchConstructor(Enemy.NewDropPod,new Vector2(0, -1500), 150),[ ])],
        "Showdown", "Our activities appear to have gathered the attention of an advanced drone.\nAttacking now will suprise the enemy before it can develop any reinforcements.", 1f, new Vector2(0, -1500), Mission.TierOne(), Mission.TierOneBosses(), null, true)
        { playerProgression = 2, isAggressive = true, playerDocked = true },

        new Mission([new Planet(Vector2.Zero, Vector2.Zero, 16000, 4, true, new Color(167, 156, 134), true, 6, 500f),
            new Planet(new Vector2(900, 0), Planet.GetOrbitalVelocity(new Vector2(900, 0), Vector2.Zero, 16000), 100, 0.5f, false, Color.OldLace),
            new Planet(new Vector2(-1200, 0), Planet.GetOrbitalVelocity(new Vector2(-1200, 0), Vector2.Zero, 16000) * 1.05f, 100, 0.5f, false, Color.OldLace),],
        [
            new EntityCondition(new LaunchConstructor(Enemy.NewGlider,new Vector2(-800, -1100), -900),[ ]),
            new WaveGoal(30)],
        "Gas giant", "", 1f, new Vector2(-800, -1100), Mission.TierOne(), Mission.TierOneBosses(), null, true)
        { playerDocked = true },

        new Mission([], [new EntityCondition(new EntityConstructor(Enemy.NewWarpGate, Vector2.Zero, Vector2.Zero, 0), [ Condition.CustomIncomplete ])],
        "Warp Gate", "Scans indicate that a large enemy fleet is coming our way after the loss of their prototype.\nRecommended action: Leave the system immediately.", -1, new Vector2(0, 500), Mission.TierOne(), Mission.TierOneBosses()) { music = false },

        //Note: The player construct menu and the Quantum Resonator both use the name of this mission for their special behavior. When changing, make sure their name is updated as well.
        new Mission([new Planet(Vector2.Zero, Vector2.Zero, 50000, 12, true, new Color(255, 219, 0), true, 1.5f) { Temperature = 0.5f } ],
        [],
        "???", "Sensor data shows that this site has an unusually high temperature.\nInvestigate possible enemy interferance.", -1, new Vector2(-2000, -2000), Mission.TierOne(), Mission.TierOneBosses(), null, true) { playerDocked = true, music = false, relaunchable = true },

        new Mission([new Planet(Vector2.Zero, Vector2.Zero, 30000, 10f, true, Color.HotPink, true) ],
        [new EntityCondition(new AdvancedConstructor(Enemy.NewCommunicator, new Vector2(MathF.Sin(1.02f), -MathF.Cos(1.02f)), Vector2.Zero, 1.02f, true), [Condition.Protect]),
            new EntityCondition(new AdvancedConstructor(Enemy.NewCommunicator, new Vector2(MathF.Sin(2.7f), -MathF.Cos(2.7f)), Vector2.Zero, 2.7f, true), [Condition.Protect]),
            new EntityCondition(new AdvancedConstructor(Enemy.NewCommunicator, new Vector2(MathF.Sin(5.33f), -MathF.Cos(5.33f)), Vector2.Zero, 5.33f, true), [Condition.Protect]),
            new EntityCondition(new LaunchConstructor(Enemy.NewDropPod,new Vector2(0, -1500), 500),[ ]),
            new WaveGoal(30),
        ],
        "Base of Operations", "We have deployed several communication stations to this site.\nProtect the location for future development.", 0, new Vector2(0, -1500), Mission.TierTwo(), Mission.TierTwoBosses(), null, true) { playerDocked = true },

        new Mission([new Planet(Vector2.Zero, Vector2.Zero, 20000, 9f, true, Color.Cyan, false, 1.8f),
        new(new Vector2(0, 1800), Planet.GetOrbitalVelocity(new Vector2(0, 1800), Vector2.Zero, 20000), 1500, 2f, false, Color.Cyan) ],
        [
            new EntityCondition(new AdvancedConstructor(Enemy.NewTurret, new Vector2(MathF.Sin(5.5f), -MathF.Cos(5.5f)) * 9 * 50, Vector2.Zero, 5.5f, false), [ Condition.Kill ]),
            new EntityCondition(new AdvancedConstructor(Enemy.NewTurret, new Vector2(MathF.Sin(3.2f), -MathF.Cos(3.2f)) * 9 * 50, Vector2.Zero, 3.2f, false), [ Condition.Kill ]),
            new EntityCondition(new AdvancedConstructor(Enemy.NewTurret, new Vector2(MathF.Sin(2.6f), -MathF.Cos(2.6f)) * 9 * 50, Vector2.Zero, 2.6f, false), [ Condition.Kill ]),
            new EntityCondition(new AdvancedConstructor(Enemy.NewTurret, new Vector2(MathF.Sin(1.1f), -MathF.Cos(1.1f)) * 9 * 50, Vector2.Zero, 1.1f, false), [ Condition.Kill ]),
            new EntityCondition(new AdvancedConstructor(Enemy.NewMiner, new Vector2(MathF.Sin(3), -MathF.Cos(3)) * 9 * 50, Vector2.Zero, 3, false), [ Condition.Kill ]),
            new EntityCondition(new AdvancedConstructor(Enemy.NewMiner, new Vector2(MathF.Sin(5.2f), -MathF.Cos(5.2f)) * 9 * 50, Vector2.Zero, 5.2f, false), [ Condition.Kill ]),
            new EntityCondition(new EntityConstructor(Enemy.NewOrbiter, new Vector2(0, 1650), Planet.GetOrbitalVelocity(new Vector2(0, 1650), new Vector2(0, 1800), 1500)
                + Planet.GetOrbitalVelocity(new Vector2(0, 1800), Vector2.Zero, 20000), 0), [ Condition.Protect ])],
        "Assault", "It appears the enemy has improved their fleet, and has pushed the mothership to a non-ideal location.\nDefend the mothership and defeat the fortified miner base on this planet.", 0.75f, new Vector2(0, 1650), Mission.TierTwo(), Mission.TierTwoBosses(), null, false)
        { playerDocked = true, isAggressive = true, tip = "Press C to open the construct menu.\nEach construct requires one scrap to craft." },

        new Mission( [new Planet(Vector2.Zero, Vector2.Zero, 25000, 7f, true, Color.Cyan), new(new Vector2(800, 0), Planet.GetOrbitalVelocity(new Vector2(800, 0), Vector2.Zero, 25000), 150, 0.5f, false, Color.Cyan), ],
        [
            new EntityCondition(new EntityConstructor(Enemy.NewMiner, new Vector2(0, -7*50), Vector2.Zero, 0), [ Condition.Protect ]),
            new EntityCondition(new LaunchConstructor(Enemy.NewDropPod,new Vector2(0, -1500), 350),[ ]),
            new WaveGoal(30)],
        "Extraction", "Our success has led us to deploying a miner on this deceptively dense planet.\nDefend it from the incoming enemy forces.", 1, new Vector2(0, -1500), Mission.TierTwo(), Mission.TierTwoBosses())
        { playerDocked = true, isAggressive = true},

        new Mission([new Planet(Vector2.Zero, Vector2.Zero, 5000, 4.5f, true, Color.Cyan),
        new(new Vector2(600, 0), Planet.GetOrbitalVelocity(new Vector2(600, 0), Vector2.Zero, 5000), 240, 1f, false, Color.Cyan),
        new(new Vector2(-600, 0), Planet.GetOrbitalVelocity(new Vector2(-600, 0), Vector2.Zero, 5000), 240, 1f, false, Color.Cyan), ],
        [
            new EntityCondition(new AdvancedConstructor(Enemy.NewExodusBoss, new Vector2(0, -6*50), Vector2.Zero, 0, false), [ Condition.Kill ]),
            new EntityCondition(new LaunchConstructor(Enemy.NewDropPod,new Vector2(0, -1500), 225),[ ])],
        "Flight of the bumblebee.", "The enemy fleet's fastest fighter appears to have arrived to this planet and is blocking our path.\nDefeating it appears to be the only way forward.", 1.1f, new Vector2(0, -1500), Mission.TierTwo(), Mission.TierTwoBosses(), null, true)
        { playerDocked = true },

        new([new Planet(Vector2.Zero, Vector2.Zero, 150, 3, true, Color.OldLace)],
        [new EntityCondition(new EntityConstructor(Enemy.NewWarpGate, new Vector2(0, 450), -Planet.GetOrbitalVelocity(new Vector2(0, 450), Vector2.Zero, 150), 0), [ Condition.CustomIncomplete ])],
        "Warp Gate", "The enemy fleet is still hot on our tail.", -1, new Vector2(0, 500), Mission.TierOne(), Mission.TierOneBosses())
        { music = false, tip = "Press left shift to return to the previous system. Press right shift to enter the next system.", relaunchable = true },

        new Mission([new Planet(Vector2.Zero, Vector2.Zero, 6000, 6, true, Color.Cyan, true, 0.8f)],
        [
            new EntityCondition(new EntityConstructor(Enemy.NewTrader, new Vector2(0, 500), Planet.GetOrbitalVelocity(new Vector2(0, 500), Vector2.Zero, 6000), 0), [ Condition.Protect ])],
        "Trader", "This friendly trader invites us to upgrade our modules in exchange for resources", -1, new Vector2(-2000, -2000),
        Mission.TierOne(), Mission.TierOneBosses(), null, true) { relaunchable = true, playerDocked = true },

        new Mission([new Planet(Vector2.Zero, Vector2.Zero, 4000, 3, true, Color.Wheat, false, 1.5f),
            new Planet(new Vector2(500, 0), Planet.GetOrbitalVelocity(new Vector2(500, 0), Vector2.Zero, 4000), 300, 1f, false, Color.Wheat)],
        [
            new EntityCondition(new AdvancedConstructor(Enemy.NewClockworkBoss, new Vector2(0, -6*50), Vector2.Zero, 0, false), [ Condition.Kill ]),
            new EntityCondition(new LaunchConstructor(Enemy.NewDropPod,new Vector2(0, -1500), 150),[ ])],
        "Clockwork creation", "A strange sentinal appears to be housed on this ancient planet.\nDismantling it may yield unusual resources.", 1f, new Vector2(0, -1500), Mission.TierTwo(), Mission.TierTwoBosses(), null, true)
        { isAggressive = true, playerDocked = true },

        new Mission([new Planet(Vector2.Zero, Vector2.Zero, 10000, 3, true, new Color(41, 144, 181), false, 15f) { Temperature = -20 }],
        [
            new EntityCondition(new LaunchConstructor(Enemy.NewGlider,new Vector2(-800, -1300), -1150),[ ]),
            new WaveGoal(30)],
        "Ice giant", "The unusual conditions in this system have resulted in unique developments in the enemies technology.\nBe prepared for advanced enemy cloaking.", 1f, new Vector2(-800, -1300), Mission.TierTwo(), Mission.TierTwoBosses(), null, true)
        { playerDocked = true },

        new Mission([new(new Vector2(500, 0), new Vector2(0, 1.05f), 10000, 7, false, Color.Cyan) { Temperature = -5},
        new(new Vector2(-1000, 0), new Vector2(0, -2.1f), 5000, 4f, false, Color.Orange) { Temperature = 5 }],
        [
            new EntityCondition(new EntityConstructor(Enemy.MassRelay, Vector2.Zero, Vector2.Zero, 0), [ Condition.Protect, Condition.CustomIncomplete ]),
            new EntityCondition(new LaunchConstructor(Enemy.NewDropPod,new Vector2(0, -1500), 0),[ ])],
        "Binary system", "It seems plans for a mass relay have been abandoned here.\nConstruct it to recieve some advanced equipment from our previous stations.", -1f, new Vector2(0, -1500), Mission.TierThree(), Mission.TierThreeBosses(), null, true)
        { isAggressive = true, playerDocked = true },

        new Mission([new(Vector2.Zero, Vector2.Zero, 4000, 4.5f, true, new Color(0.03f, 0.05f, 0.08f)),
        new(new Vector2(600, 0), Planet.GetOrbitalVelocity(new Vector2(600, 0), Vector2.Zero, 4000) * 1.05f, 500, 1.5f, false, new Color(0.06f, 0.08f, 0.12f)), ],
        [
            new EntityCondition(new EntityConstructor(Enemy.NewVeilBoss, new Vector2(0, -6*50), Vector2.Zero, 0), [ Condition.Kill ]),
            new EntityCondition(new LaunchConstructor(Enemy.NewDropPod,new Vector2(0, -1500), 225),[ ])],
        "Veiled", "Sensors indicate that our actions have been spied on by the enemy.\nDestroy it.", 1.1f, new Vector2(0, -1500), Mission.TierThree(), Mission.TierThreeBosses(), null, true)
        { playerDocked = true},

        new Mission([new(Vector2.Zero, Vector2.Zero, 18000, 6f, true, Color.Cyan, false, 1.5f) { Temperature = -2 },
        new(new Vector2(1100, 0), Planet.GetOrbitalVelocity(new Vector2(1100, 0), Vector2.Zero, 18000), 1500, 1.5f, false, Color.Cyan) { Temperature = -2 }],
        [
            new EntityCondition(new EntityConstructor(Enemy.NewMeshNetworkNode, new Vector2(0, -1), Vector2.Zero, 0), [Condition.Protect, Condition.CustomIncomplete]),
            new EntityCondition(new EntityConstructor(Enemy.NewMeshNetworkNode, new Vector2(1250, 0),-Planet.GetOrbitalVelocity(new Vector2(150, 0), 
                Vector2.Zero, 1500) + Planet.GetOrbitalVelocity(new Vector2(1100, 0), Vector2.Zero, 18000), 0), [Condition.Protect, Condition.CustomIncomplete]),
            new EntityCondition(new EntityConstructor(Enemy.NewMeshNetworkNode, new Vector2(-500, 0), -Planet.GetOrbitalVelocity(new Vector2(-500, 0), Vector2.Zero, 18000), 0), [Condition.Protect, Condition.CustomIncomplete]),
            new EntityCondition(new LaunchConstructor(Enemy.NewDropPod,new Vector2(0, -1500), 310),[ ])],
        "Hack", "The enemy has set up a mesh node network for storing information.\nHack it to discover the location of their leader.", 0.8f, new Vector2(0, -1500), Mission.TierThree(), Mission.TierThreeBosses(), null, true)
        { playerDocked = true, isAggressive = true },

        new Mission([new Planet(Vector2.Zero, Vector2.Zero, 160000, 6, true, new Color(0.9f, 1f, 0.75f), false, 50f) { isSun = true, Temperature = 5 },
        new Planet(new Vector2(0, 2000), Planet.GetOrbitalVelocity(new Vector2(0, 2000) * 0.99f, Vector2.Zero, 16-000), 8000, 4, false, new Color(0.95f, 0.2f, 0.1f))],
        [
            new EntityCondition(new EntityConstructor(Enemy.NewEpitomeBoss, new Vector2(0, 2800), Vector2.Zero, 0), [ Condition.Kill ]),
            new EntityCondition(new LaunchConstructor(Enemy.NewGlider,new Vector2(-1500, -2000), -1500),[ ])],
        "Inferno", "Your intel has led you here. Finish this.", -1, new Vector2(-1500, -2000), Mission.TierThree(), Mission.TierThreeBosses(), null, true)
        { music = false, playerDocked = true },

        new Mission([ new(Vector2.Zero, Vector2.Zero, 20000, 9, true, Color.OrangeRed, true, 1.5f),
        new(new Vector2(1200, 0), Planet.GetOrbitalVelocity(new Vector2(1200, 0), Vector2.Zero, 20000), 750, 2f, false, Color.Red) ],
        [
            new EntityCondition(new LaunchConstructor(Enemy.NewDropPod,new Vector2(0, -1500), 950),[ ]),
            new WaveGoal(1000)],
        "Last Stand", "Survive", 0.25f, new Vector2(0, -1500), Mission.All(), Mission.AllBosses())
        { isAggressive = true, playerProgression = 4, tip = "You can now construct the makeshift mothership in the construct menu.\n Requires 3 scrap.", relaunchable = true },

        new Mission([], [], "Penultimate", "", 1, Vector2.Zero, [], []),
    ];
    public List<(float distance, List<int> prerequisites, int system)> Systems { get; } =
    [
        (200, [], 0), (160, [0], 0), (140, [0], 0), (100, [1, 2], 0), (400, [3], 0), (50, [3], 0),
        (210, [], 1), (170, [6], 1), (145, [7], 1), (130, [8], 1), (150, [9], 1),
        (200, [], 2), (150, [11], 2), (100, [12], 2), (80, [13], 2), (60, [14], 2), (0, [15], 2), (0, [], 2), (0, [], 2), (0, [], 2), (0, [], 2)
    ];
    public Mission GetMission(int _index)
    {
        return missions[_index].Clone();
    }
    public int Missions()
    {
        return missions.Count;
    }
    public void UpdateColor()
    {
        foreach (var entity in entities)
        {
            entity.UpdateColor();
        }
    }
    public void Add(Entity entity)
    {
        if (!isUpdating)
        {
            //Checks the entity type, and adds it to the corresponding list for each type
            entities.Add(entity);
            if (entity is Projectile)
            {
                projectiles.Add(entity as Projectile);
            }
            if (entity is Enemy || entity is Player)
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
    public void Initialize()
    {
        Engine.SaveGame.CurrentMission = missions[Engine.SaveGame.CurrentMissionIndex].Clone();
        entities.Clear();
        addedEntities.Clear();
        enemies.Clear();
        projectiles.Clear();
        Player.velocity = Vector2.Zero;
        Engine.SaveGame.CurrentMission.Initialize();
    }
    public void IngameUpdate()
    {
        Player.Update();
        Engine.SaveGame.CurrentMission.AttractObject(Player);
        if (Player.dockedEntity == null && Player.Progression > -1)
        {
            Engine.SaveGame.CurrentMission.CalculateTrajectory(Player.position, Player.velocity, Player.ColliderRadius);
        }
        Engine.MousePositionOffset = new Vector2(Mouse.GetState().X, Mouse.GetState().Y) / 10 - Engine.BackBuffer / 20
        + Engine.ScreenShakeFactor * Engine.ScreenShakeFactor * new Vector2(Util.Random.NextSingle() - 0.5f, Util.Random.NextSingle() - 0.5f) * 50;
        Engine.Camera.Rotation = Engine.ScreenShakeFactor * Engine.ScreenShakeFactor * (Util.Random.NextSingle() - 0.5f) * 0.15f;
        //If the player is further from the camera, put more weight on the player
        //Tanh prevents frac from going above 1
        float frac = MathF.Tanh(Vector2.Distance(Player.position, Engine.Camera.Position) / 750);
        Engine.Camera.Position = Player.position * frac + Engine.Camera.Position * (1 - frac);
        var time = Engine.IngameTime;
        time.Duration += Engine.DeltaSeconds;
        Engine.IngameTime = time;
        Engine.SaveGame.CurrentMission.Update();
        Update();
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
    }
    public void Update()
    {
        isUpdating = true;
        //Prevents modifying a list while iterating over it
        foreach (var entity in entities)
        {
            entity.Update();
            Engine.SaveGame.CurrentMission.AttractObject(entity);
        }
        if (projectiles.Count >= 150)
        {
            for (int i = 0; i < projectiles.Count - 150; i++)
            {
                projectiles[i].isExpired = true;
            }
        }
        entities = [.. entities.Where(x => !x.isExpired)];
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
    public void Draw(SpriteBatch _spriteBatch)
    {
        //CurrentMission.Planet.Draw(_spriteBatch);
        Player.Draw(_spriteBatch);
        foreach (var entity in entities)
        {
            //Draws all entities in the main list
            entity.Draw(_spriteBatch);
        }
        foreach(var collider in Engine.SaveGame.CurrentMission.Colliders)
        {
            collider.Draw(_spriteBatch);
        }
    }
    public void Explode(int _damage, float _radius, Vector2 _position)
    {
        float dist;
        foreach (var entity in entities)
        {
            if (entity is Projectile)
            {
                continue;
            }
            dist = Vector2.Distance(_position, entity.position);
            if (dist < _radius + entity.ColliderRadius && dist > float.Epsilon)
            {
                entity.Collide(_damage);
            }
        }
        dist = Vector2.Distance(_position, Player.position);
        if (dist < _radius && dist > 0.001f)
        {
            Player.Collide(_damage);
        }
    }
    public void DecayPickups()
    {
        foreach (var pickup in entities)
        {
            if (pickup is Pickup && Util.Random.NextSingle() > (0.4f + Engine.SaveGame.CurrentMission.GetAtmospherePressure(pickup))) //Update changelog when available
            {
                pickup.Collide(1);
            }
        }
    }
    public static void Collide(Entity entity, Entity targetEntity)
    {
        if (entity == null || targetEntity == null)
        {
            return;
        }
        float combinedRadius = entity.ColliderRadius + targetEntity.ColliderRadius;
        if (entity.isFriendly != targetEntity.isFriendly && DistanceSqr(entity, targetEntity) <= combinedRadius * combinedRadius)
        {
            entity.Collide(targetEntity.damage);
            targetEntity.Collide(entity.damage);
        }
    }
    public DockableComponent NearestDockableEntity(Entity _entity)
    {
        float nearestDistance = float.MaxValue;
        DockableComponent returnEntity = null;
        foreach (Entity entity in entities)
        {
            var component = entity.Components.GetComponent(ComponentType.DockableComponent);
            if (component.IsValid)
            {
                float distanceSqr = DistanceSqr(entity, _entity);
                if (distanceSqr < nearestDistance)
                {
                    nearestDistance = distanceSqr;
                    returnEntity = component as DockableComponent;
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
            if (targetEnemy.StealthAbility > entity.SensingAbility || targetEnemy.isFriendly == entity.isFriendly)
            {
                continue;
            }
            if(!_getDeadEnemies && targetEnemy as Enemy != null && (targetEnemy as Enemy).health <= 0)
            {
                continue;
            }
            float distance = DistanceSqr(entity, targetEnemy);
            if (distance < nearestDistance && (targetEnemy.StealthAbility < entity.SensingAbility || (distance < maxDistSqr)))
            {
                nearestDistance = distance;
                returnEnemy = targetEnemy;
            }
        }
        if (!entity.isFriendly)
        {
            //Enemies will prioritize the player
            float distance = DistanceSqr(entity, Player) / 1.5f;
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
            if (targetEnemy.isFriendly != entity.isFriendly)
            {
                continue;
            }
            if (targetEnemy == entity)
            {
                continue;
            }
            if ((targetEnemy as Enemy) != null && (targetEnemy as Enemy).ChildEnemy)
            {
                continue;
            }
            float distance = DistanceSqr(entity, targetEnemy);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                returnEnemy = targetEnemy;
            }
        }
        if (entity.isFriendly)
        {
            float distance = DistanceSqr(entity, Player);
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
        foreach (Entity targetEntity in entities)
        {
            if (targetEntity is not Pickup || targetEntity == entity)
            {
                continue;
            }
            if (!_findAll && (targetEntity is Module || targetEntity is Construct))
            {
                continue;
            }
            float distance = DistanceSqr(entity, targetEntity);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                returnItem = targetEntity;
            }
        }
        return returnItem;
    }
    public Entity NearestProjectile(Entity _entity, bool _isFriendly)
    {
        float nearestDistance = float.MaxValue;
        Entity returnProjectile = null;
        foreach (Projectile targetProjectile in projectiles)
        {
            float distance = DistanceSqr(_entity, targetProjectile);
            if (distance < nearestDistance && _isFriendly != targetProjectile.isFriendly
                && (targetProjectile.StealthAbility < _entity.SensingAbility || (targetProjectile.StealthAbility == _entity.SensingAbility && distance < StealthRange * StealthThreshold)))
            {
                nearestDistance = distance;
                returnProjectile = targetProjectile;
            }
        }
        foreach (Entity missile in enemies)
        {
            if (missile.entityType != EntityType.Projectile)
            {
                continue;
            }
            float distance = DistanceSqr(_entity, missile);
            if (distance < nearestDistance && _isFriendly != missile.isFriendly
                && (missile.StealthAbility < _entity.SensingAbility || (missile.StealthAbility == _entity.SensingAbility && distance < StealthRange * StealthThreshold)))
            {
                nearestDistance = distance;
                returnProjectile = missile;
            }
        }
        return returnProjectile;
    }
    public static float DistanceSqr(Entity _entity1, Entity _entity2)
    {
        if (_entity1 == null || _entity2 == null)
        {
            return float.MaxValue;
        }
        Vector2 Target = _entity2.position - _entity1.position;
        return Target.X * Target.X + Target.Y * Target.Y;
    }
    public static float DistanceSqr(Vector2 _vectorOne, Vector2 _vectorTwo)
    {
        Vector2 Target = _vectorTwo - _vectorOne;
        return Target.X * Target.X + Target.Y * Target.Y;
    }

    public static bool RandomWithKarma(float _rarity)
    {
        float karmaBonus = (_rarity - 1) / (_rarity + _rarity * MathF.Exp(-10 * currentKarma + 12.5f));
        if (Util.Random.NextSingle() < (1 / _rarity) + karmaBonus)
        {
            currentKarma = 0;
            return true;
        }
        currentKarma += (1 / _rarity);
        return false;
    }
    public List<Entity> Hitscan(Vector2 _pos, Vector2 _dir, float _maxLength, bool _getAll, out Vector2 _end, int _type = 0, bool _getProjectiles = false)
    {
        var dir = Vector2.Normalize(_dir);
        var list = new List<Entity>();
        float dist = 9999;
        Entity nearestEnemy = null;
        float maxDist = Engine.SaveGame.CurrentMission.Hitscan(_pos, dir);
        if (maxDist > _maxLength)
        {
            maxDist = _maxLength;
        }
        foreach (var entity in entities)
        {
            if(entity.entityType != EntityType.Projectile || _getProjectiles)
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
            if (_type != (_entity.isFriendly ? 1 : -1) && _type != 0)
            {
                return;
            }
            Vector2 relativePos = _entity.position - _pos;
            float closestLength = (relativePos.X * dir.X + relativePos.Y * dir.Y);
            float closestDistance = Vector2.Distance((dir * closestLength + _pos), _entity.position);
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
        var floppy = new Actor(Assets.Get(Sprite.Floppy), new Vector2(Engine.BackBuffer.X * 4 / 5, Engine.BackBuffer.Y), Color.Gray, MathF.PI / 8) { Scale = UIManager.UIScale };
        var floppyFlat = new Actor(Assets.Get(Sprite.FloppyFlat), new Vector2(Engine.BackBuffer.X * 4 / 5, Engine.BackBuffer.Y), Color.White, 0) { Scale = UIManager.UIScale };
        var floppyVel = Vector2.Zero;
        var ledGlow = new Actor(Assets.Get(Sprite.LEDGlow), UI.FloppyTerminal.position + (new Vector2(72.5f, 94.5f) * UIManager.UIScale - Assets.DimsOf(Sprite.Terminal) / 2) * UIManager.UIScale, Color.Red, 0) { Scale = UIManager.UIScale };
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
                floppyFlat.Position = UI.FloppyTerminal.position + (new Vector2(107, 94.5f) * UIManager.UIScale - Assets.DimsOf(Sprite.Terminal) / 2) * UIManager.UIScale;
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
        var ship = new Actor(Assets.Get(Sprite.Mothership), Vector2.Zero, Color.White, 0);
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
}
