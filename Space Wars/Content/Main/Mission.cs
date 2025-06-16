using Space_Wars.Content.Main.Entities;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using Space_Wars.Content.Main.Particles;
using Microsoft.Xna.Framework.Graphics;

namespace Space_Wars.Content.Main;
public class Mission
{
    private Entity escapeVehicle = null;
    public string Name { get; }
    public string Description { get; }
    //Change me when multiple main planets are added
    public GravitationalSource Planet => planets[0];
    private GravitationalSource[] planets; 
    //Save original entity parameters to allow cloning
    public List<(EntityConstructor, Condition[])> CopyObjectives { get; }
    public List<(Entity entity, Condition[] conditions)> MissionObjectives { get; } = [];
    private List<Entity> enemiesSpawned = [];
    private bool currentWaveActive = false;
    private Func<Cutscene> cutscene;
    private float timerModifier;
    public int WaveGoal { get; } = 0;
    public float restartTimer = -1;
    public int Wave { get; private set; } = 0;
    private List<(int cost, DelegateEnemy enemy)> enemyCreditValues;
    private List<DelegateEnemy> bosses;
    private int currentBoss;
    private int tier;
    private float waveTimer = 5;
    private float maxWaveTimer = 5;
    private float difficulty;
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
                (1, Enemy.NewStealthFighter),
                (2, Enemy.NewHunter),
            ];
        }

        bosses = 
        [
            Enemy.NewSymmetryBoss,
            Enemy.NewOverloadBoss,
            Enemy.NewExcursionBoss,
            Enemy.NewWyvernBoss,
        ];
        currentBoss = Engine.Random.Next(bosses.Count);
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
                Engine.SaveGame.CompletedMissions[Engine.EntityManager.MissionCount] = true;
                restartTimer = 2;
                Engine.EntityManager.CompleteMission(Wave);
            }
        }
        if (restartTimer != -1)
        {
            if (restartTimer > 0)
            {
                restartTimer -= Engine.DeltaSeconds;
                return;
            }
            foreach (var entity in MissionObjectives)
            {
                entity.entity.isExpired = true;
            }
            EventHandler.MissionSelectTrigger();
        }
        //Natural enemy spawning toggle
        if (timerModifier == -1)
        {
            return;
        }

        var isReady = true;
        foreach (var enemy in enemiesSpawned)
        {
            if (!enemy.isExpired)
            {
                isReady = false;
            }
        }
        if (!currentWaveActive)
        {
            if (waveTimer <= 0)
            {
                foreach (var enemy in enemiesSpawned)
                {
                    Engine.EntityManager.Add(enemy);
                    float angle = MathF.Atan2(-enemy.position.X, enemy.position.Y);
                    float height = Assets.DimsOf(Sprite.Dot).X;
                    for (float i = 0; i < 500; i++)
                    {
                        var dir = Vector2.Normalize(enemy.position);
                        float pow = (500 - i) / 500;
                        ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), 0.5f, enemy.position + dir * i * height, Vector2.Zero, angle, 0, new Color(255, 0, 0), Color.Transparent));
                    }
                }
                currentWaveActive = true;
            }
            else
            {
                waveTimer -= Engine.DeltaSeconds;
                EventHandler.UpdateEnemyCountdownUI(waveTimer, maxWaveTimer, Wave);
                foreach (var enemy in enemiesSpawned)
                {
                    ParticleManager.Add(new Particle(enemy.texture, enemy.position, enemy.angle, new Color(255,127, 0) * (Engine.Random.NextSingle() / 2 + 0.25f)));
                }
            }
        }
        if (isReady)
        {
            if (Wave % 20 == 0)
            {
                SoundManager.ChangeTrack(Assets.Get(Sound.main));
            }
            currentWaveActive = false;
            enemiesSpawned.Clear();
            EnemiesSpawned = 0;
            waveTimer = 10;
            maxWaveTimer = waveTimer;
            Wave++;
            Engine.EntityManager.DecayPickups();
            if (Wave % 20 == 0)
            {
                SoundManager.ChangeTrack(Assets.Get(Sound.boss));
                var pos = NewSpawnLocation();
                Enemy boss = bosses[currentBoss](pos, Vector2.Zero, MathF.Atan2(-pos.X, pos.Y));
                if (Wave == 40)
                {
                    boss = bosses[2](NewSpawnLocation(), Vector2.Zero, 0);
                }
                enemiesSpawned.Add(boss);
                EnemiesSpawned = 1;
                currentBoss = (currentBoss + 1) % bosses.Count;
                EventHandler.UpdateEnemyCountdownUI(waveTimer, maxWaveTimer, Wave);
                return;
            }
            else
            {
                difficulty = (int)((Wave + 1) * MathF.Log(Wave + 1, MathF.E) - Wave) / 15 + 1;
                List<int> newCosts = [];
                int availableEnemies = Math.Min(enemyCreditValues.Count, Wave / 10 + 1);
                Enemy squadLeader = null;
                int count = 0;
                for (int i = 0; i < availableEnemies; i++)
                {
                    newCosts.Add(enemyCreditValues[i].cost);
                }
                var enemyCredits = Engine.Random.Next((int)(3 * difficulty), (int)(5 * difficulty));
                while (enemyCredits > 0)
                {
                    for (int i = 0; i < availableEnemies; i++)
                    {
                        if (Engine.Random.Next(0, enemyCreditValues[i].cost / 2) == 0 && newCosts[i] <= enemyCredits)
                        {
                            Vector2 pos;
                            
                            if (squadLeader != null && (count < 2 || Engine.Random.Next(0, 4) != 0))
                            {
                                Vector2 offset = Vector2.Normalize(new Vector2(squadLeader.position.X, squadLeader.position.Y));
                                int isOdd = (count % 2 == 0) ? 1 : -1;
                                
                                pos = squadLeader.position 
                                    //Horizontal offset
                                    + new Vector2(offset.Y, -offset.X) * 10 * isOdd * (count / 2 + 1) 
                                    //Vertical offset
                                    + offset * (count / 2 + 1) * 10;
                                count++;
                            }
                            else
                            {
                                pos = NewSpawnLocation();
                                squadLeader = null;
                                count = 0;
                            }
                            var enemy = enemyCreditValues[i].enemy(pos, Player.velocity, MathF.Atan2(-pos.X, pos.Y));
                            enemiesSpawned.Add(enemy);
                            squadLeader ??= enemy;
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
    public GravitationalSource IsColliding(Vector2 _position)
    {
        foreach(var planet in planets)
        {
            if (planet.IsColliding(_position))
            {
                return planet;
            }
        }
        return null;
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
                ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), particlePos, 0, Color.Red * 0.5f));
                return;
            }
            if (n % 3 == 0)
            {
                ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), particlePos, 0, Color.Cyan * 0.5f));
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
        return new Mission(_planets, CopyObjectives, Name, Description, timerModifier, WaveGoal, tier, cutscene, escapeVehicle != null);
    }
    private Vector2 NewSpawnLocation()
    {
        //Creates a position vector defined by the angle from the player in radians and the distance from the edge of the screen
        float angle = (Engine.Random.NextSingle() - 0.5f) * MathF.Tau;
        float distanceMultiplier = 1 + (Engine.Random.NextSingle() - 0.5f) / 4;
        float distance = (Engine.ScreenSize.X + Engine.ScreenSize.Y) * distanceMultiplier / 3;
        Vector2 spawnLocation = Engine.ToUnitVector(angle) * distance + Player.position;
        foreach (var planet in planets)
        {
            if (Vector2.Distance(spawnLocation, planet.position) < planet.radius)
            {
                return NewSpawnLocation();
            }
        }
        return spawnLocation;
    }
}
public class EntityConstructor(Func<Vector2, Vector2, float, Entity> _constructor, Vector2 _position, Vector2 _velocity, float _angle)
{
    public Entity Construct()
    {
        return _constructor(_position, _velocity, _angle);
    }
}
