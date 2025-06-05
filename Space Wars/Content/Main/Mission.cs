using Space_Wars.Content.Main.Entities;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using Space_Wars.Content.Main.Particles;

namespace Space_Wars.Content.Main;
public class Mission
{
    public bool Completed { get; set; } = false;
    private Entity escapeVehicle = null;
    public string Name { get; }
    public string Description { get; }
    //Change me when multiple main planets are added
    public GravitationalSource Planet => planets[0];
    private GravitationalSource[] planets; 
    //Save original entity parameters to allow cloning
    public List<(EntityConstructor, Condition[])> CopyObjectives { get; }
    public List<(Entity entity, Condition[] conditions)> MissionObjectives { get; } = [];
    private Func<Cutscene> cutscene;
    private float timerModifier;
    public int WaveGoal { get; } = 0;
    public float restartTimer = -1;
    public int Wave { get; private set; } = 0;
    private List<(int cost, DelegateEnemy enemy)> enemyCreditValues;
    private List<DelegateEnemy> bosses;
    private Enemy aliveBoss = null;
    private int currentBoss;
    private int tier;
    private float waveTimer = 5;
    private float maxWaveTimer = 5;
    private float difficulty;
    private Random random = new();
    private Player Player => Engine.SaveGame.Player;
    public delegate Enemy DelegateEnemy(Vector2 position, Vector2 velocity, float angle, bool _isFriendly = false);
    public int EnemiesSpawned { get; private set; } = 0;

    public Mission(GravitationalSource[] _planets, List<(EntityConstructor, Condition[] conditions)> _missionObjectives, string _name, string _description, float _timerModifier, int _waveGoal = 0, int _enemyTier = 0, Func<Cutscene> _cutscene = null, bool _escapeVehicle = false)
    {
        Name = _name;
        Description = _description;
        planets = _planets;
        CopyObjectives = _missionObjectives;
        foreach (var (entityConstructor, conditions) in _missionObjectives)
        {
            MissionObjectives.Add((entityConstructor.Construct(), (Condition[])conditions.Clone()));
        }
        WaveGoal = _waveGoal;
        timerModifier = _timerModifier;

        tier = _enemyTier;
        if (_enemyTier == 0)
        {
            enemyCreditValues = 
            [
                (1, Enemy.NewFighter),
                (3, Enemy.NewCarrier),
                (3, Enemy.NewSniper),
                (4, Enemy.NewShotgunner),
            ];
        }
        else if (_enemyTier == 1)
        {
            enemyCreditValues = 
            [
                (1, Enemy.NewAdvancedFighter),
                (2, Enemy.NewHovercraft),
            ];
        }
        else
        {
            enemyCreditValues = 
            [
                (1, Enemy.NewStealthFighter)
            ];
        }

        bosses = 
        [
            Enemy.NewSymmetryBoss,
            Enemy.NewOverloadBoss,
            Enemy.NewExcursionBoss,
            Enemy.NewWyvernBoss,
        ];
        currentBoss = random.Next(bosses.Count);
        cutscene = _cutscene;
        if (_escapeVehicle)
        {
            escapeVehicle = Enemy.NewPickupDrone(new Vector2(-2000, -2000), Vector2.Zero, 0);
        }
    }
    //Whether to decay the pickups or not
    public void Update()
    {
        PlanetUpdate();
        bool allCompleted = true;
        foreach (var (entity, conditions) in MissionObjectives)
        {
            foreach (var condition in conditions)
            {
                if (condition == Condition.Protect && entity.isExpired)
                {
                    FailMission();
                }
                if (condition == Condition.Kill && !entity.isExpired)
                {
                    allCompleted = false;
                }
                if (condition == Condition.CustomIncomplete)
                {
                    allCompleted = false;
                }
            }
        }
        if (WaveGoal > 0 && (WaveGoal >= Wave))
        {
            allCompleted = false;
        }
        if (allCompleted && restartTimer == -1)
        {
            if (escapeVehicle != null)
            {
                MissionObjectives.Add((escapeVehicle, new Condition[2] { Condition.Protect, Condition.CustomIncomplete }));
                Engine.EntityManager.Add(escapeVehicle);
                escapeVehicle = null;
                allCompleted = false;
            }
            else
            {
                Engine.EntityManager.MarkMissionComplete();
            }
        }
        if (restartTimer != -1)
        {
            if (restartTimer > 0)
            {
                restartTimer -= Engine.DeltaSeconds;
                return;
            }
            EventHandler.MissionSelectTrigger();
        }
        if (timerModifier == -1)
        {
            return;
        }

        if (aliveBoss != null)
        {
            if (aliveBoss.isExpired)
            {
                aliveBoss = null;
                SoundManager.ChangeTrack(Assets.Get(Sound.main));
            }
            else
            {
                return;
            }
        }
        waveTimer -= Engine.DeltaSeconds;
        EventHandler.UpdateEnemyCountdownUI(waveTimer, maxWaveTimer, Wave);
        if (waveTimer <= 0)
        {
            EnemiesSpawned = 0;
            Wave++;
            Engine.EntityManager.DecayPickups();
            if (Wave % 20 == 0)
            {
                SoundManager.ChangeTrack(Assets.Get(Sound.boss));
                Enemy boss = bosses[currentBoss](NewSpawnLocation(), Vector2.Zero, 0);
                if (Wave == 40)
                {
                    boss = bosses[2](NewSpawnLocation(), Vector2.Zero, 0);
                }
                Engine.EntityManager.Add(boss);
                waveTimer = 10;
                maxWaveTimer = waveTimer;
                EnemiesSpawned = 1;
                currentBoss = (currentBoss + 1) % bosses.Count;
                aliveBoss = boss;
                EventHandler.UpdateEnemyCountdownUI(waveTimer, maxWaveTimer, Wave);
                return;
            }
            else
            {
                difficulty = (int)((Wave + 1) * MathF.Log(Wave + 1, MathF.E) - Wave) / 15 + 1;
                SpawnWaveBatch(random.Next((int)(3 * difficulty), (int)(5 * difficulty)));
                waveTimer = (5f * EnemiesSpawned + 6) * timerModifier;
                maxWaveTimer = waveTimer;
            }
        }
    }
    public void PlanetUpdate()
    {
        foreach (var planet in planets)
        {
            planet.Update();
            foreach (var planet2 in planets)
            {
                if (planet == planet2)
                {
                    continue;
                }
                planet.AttractObject(planet2);
            }
        }
    }
    public void PlayIntroCutscene()
    {
        if (cutscene != null)
        {
            CurrentGameState.SwitchState(cutscene());
        }
        else
        {
            CurrentGameState.SwitchState(new PlayingGame());
        }
    }
    public void Initialize()
    {
        foreach(var (entity, _) in MissionObjectives)
        {
            Engine.EntityManager.Add(entity);
        }
    }
    public void MarkComplete()
    {
        if (restartTimer != -1)
        {
            return;
        }
        Completed = true;
        restartTimer = 2;
    }
    public void AttractObject(Entity _entity)
    {
        foreach (var planet in planets) { planet.AttractObject(_entity); }
    }
    public void FailMission()
    {
        if (restartTimer != -1)
        {
            return;
        }
        restartTimer = 2;
    }
    public void CompleteCustomRule(Entity _target) 
    {
        for(int i = 0; i < MissionObjectives.Count; i++)
        {
            (Entity entity, Condition[] conditions) = MissionObjectives[i];
            if (_target == entity)
            {
                for(int j = 0; j < conditions.Length; j++)
                {
                    if (conditions[j] == Condition.CustomIncomplete)
                    {
                        conditions[j] = Condition.CustomComplete;
                    }
                }
            }
        }
    }
    public void CalculateTrajectory(Vector2 _startPosition, Vector2 _startVelocity, float _radius)
    {
        Vector2 futurePosition = _startPosition;
        Vector2 futureVelocity = _startVelocity;
        Vector2[] futurePlanetPositions = planets.Select(planet => planet.position).ToArray();
        Vector2[] futurePlanetVelocities = planets.Select(planet => planet.velocity).ToArray();
        bool exit = false;

        for (int n = 0; n < 1000; n++)
        {
            for (int i = 0; i < planets.Length; i++)
            {
                if (planets[i].radius + _radius > Vector2.Distance(futurePlanetPositions[i], futurePosition))
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
                    if (!planets[i].isImmovable)
                    {
                        futurePlanetVelocities[i] += Vector2.Normalize(-relativePlanetPosition) * planets[j].mass / relativePlanetPosition.LengthSquared();
                    }
                }
                Vector2 relativePosition = futurePosition - futurePlanetPositions[i];
                futureVelocity += Vector2.Normalize(-relativePosition) * planets[i].mass / relativePosition.LengthSquared();
            }
            for (int i = 0; i < planets.Length; i++)
            {
                futurePlanetPositions[i] += futurePlanetVelocities[i];
            }
            futurePosition += futureVelocity;
            Vector2 particlePos = futurePosition;
            if (Engine.PatchedConics)
            {
                for(int i = 0; i < futurePlanetPositions.Length; i++)
                {
                    if (i == 0)
                    {
                        continue;
                    }
                    float sphereOfInfluence = (float)Vector2.Distance(futurePlanetPositions[i], futurePlanetPositions[0]) * (float)Math.Pow(planets[i].mass / planets[0].mass, 2 / 5) / 3;
                    if (Vector2.Distance(futurePosition, futurePlanetPositions[i]) < sphereOfInfluence)
                    {
                        particlePos = particlePos - futurePlanetPositions[i] + planets[i].position;
                    }
                }
            }
            if (exit)
            {
                ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), particlePos, 0, 0.5f, Color.Red));
                return;
            }
            if (n % 3 == 0)
            {
                ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), particlePos, 0, 0.5f, Color.Cyan));
            }
        }
    }
    public Vector2 GetNormalizedAcceleration(Vector2 _position)
    {
        Vector2 acceleration = Vector2.Zero;
        foreach (var planet in planets)
        {
            Vector2 normalVector = Vector2.Normalize(_position - planet.position);
            acceleration += normalVector * (planet.radius * planet.radius / EntityManager.DistanceSqr(planet.position, _position));
        }
        return acceleration;
    }
    public Mission Clone()
    {
        var _planets = new GravitationalSource[planets.Length];
        for(int i = 0; i < planets.Length; i++)
        {
            _planets[i] = planets[i].Copy();
        }
        return new Mission(_planets, CopyObjectives, Name, Description, timerModifier, WaveGoal, tier, cutscene, escapeVehicle != null) { Completed = this.Completed};
    }
    private void SpawnWaveBatch(int enemyCredits)
    {
        List<int> newCosts = [];
        int availableEnemies = Math.Min(enemyCreditValues.Count, Wave / 10 + 1);
        for (int i = 0; i < availableEnemies; i++)
        {
            newCosts.Add(enemyCreditValues[i].cost);
        }
        while (enemyCredits > 0)
        {
            for (int i = 0; i < availableEnemies; i++)
            {
                if (random.Next(0, enemyCreditValues[i].cost / 2) == 0 && newCosts[i] <= enemyCredits)
                {
                    Engine.EntityManager.Add(enemyCreditValues[i].enemy(NewSpawnLocation(), Player.velocity, 0));
                    enemyCredits -= newCosts[i];
                    newCosts[i] += 1;
                    EnemiesSpawned++;
                }
                if (enemyCredits < newCosts.Min(c => c))
                {
                    return;
                }
            }
        }
    }
    private Vector2 NewSpawnLocation()
    {
        //Creates a position vector defined by the angle from the player in radians and the distance from the edge of the screen
        float angle = (random.NextSingle() - 0.5f) * MathF.Tau;
        float distanceMultiplier = 1 + (random.NextSingle() - 0.5f) / 4;
        float distance = (Engine.ScreenSize.X + Engine.ScreenSize.Y) / 2 * distanceMultiplier;
        Vector2 spawnLocation = Engine.ToUnitVector(angle) * distance;
        return spawnLocation + Player.position;
    }
}
public class EntityConstructor(Func<Vector2, Vector2, float, Entity> _constructor, Vector2 _position, Vector2 _velocity, float _angle)
{
    public Entity Construct()
    {
        return _constructor(_position, _velocity, _angle);
    }
}
