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
    public delegate Enemy DelegateEnemy(Vector2 position, Vector2 velocity, float angle, bool _isFriendly = false);
    //Change me when multiple main planets are added
    public GravitationalSource Planet { get { if (planets.Length > 0) { return planets[0]; } else { return new GravitationalSource(Vector2.Zero, Vector2.Zero, 0, 1, true, Color.White); } } }
    public string Name { get; }
    public string Description { get; }
    public int playerProgression = 3;
    public int WaveGoal { get; } = 0;
    public int Wave { get; private set; } = 0;
    public int EnemiesSpawned { get; private set; } = 0;
    public float restartTimer = -1;
    public bool playerDocked = false;
    public bool isAggressive = false;
    public bool music = true;
    public string tip = null;

    private static Player Player => Engine.SaveGame.Player;
    private Entity escapeVehicle = null;
    private GravitationalSource[] planets; 
    //Save original entity parameters to allow cloning
    private List<(IConstructor, Condition[])> CopyObjectives { get; }
    private List<(Entity entity, Condition[] conditions)> MissionObjectives { get; } = [];
    private List<Entity> enemiesSpawned = [];
    private List<(int cost, DelegateEnemy enemy)> enemyCreditValues;
    private List<DelegateEnemy> bosses;
    private Vector2 playerPosition;
    private Func<Cutscene> cutscene;
    private int currentBoss;
    private int tier;
    private float timerModifier;
    private float waveTimer = 5;
    private float maxWaveTimer = 5;
    private float difficulty;
    private bool currentWaveActive = false;

    public Mission(GravitationalSource[] _planets, List<(IConstructor, Condition[] conditions)> _missionObjectives, string _name, string _description, float _timerModifier, Vector2 _playerPosition, int _waveGoal = 0, int _enemyTier = 0, Func<Cutscene> _cutscene = null, bool _escapeVehicle = false)
    {
        Name = _name;
        Description = _description;
        planets = _planets;
        CopyObjectives = _missionObjectives;
        playerPosition = _playerPosition;
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
                (3, Enemy.NewSniper),
                (4, Enemy.NewShotgunner),
                (4, Enemy.NewCarrier),
            ];
        }
        else if (_enemyTier == 1)
        {
            enemyCreditValues = 
            [
                (1, Enemy.NewAdvancedFighter),
                (2, Enemy.NewHovercraft),
                (2, Enemy.NewHealer),
            ];
        }
        else if(_enemyTier == 2)
        {
            enemyCreditValues = 
            [
                (1, Enemy.NewStealthFighter),
                (2, Enemy.NewHunter),
            ];
        }
        else
        {
            enemyCreditValues =
            [
                (1, Enemy.NewFighter),
                (3, Enemy.NewSniper),
                (4, Enemy.NewShotgunner),
                (4, Enemy.NewCarrier),
                (1, Enemy.NewAdvancedFighter),
                (2, Enemy.NewHovercraft),
                (2, Enemy.NewHealer),
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
    public void Initialize()
    {
        Engine.SaveGame.Player.dockedEntity = null;
        foreach (var (entity, _) in MissionObjectives)
        {
            Engine.EntityManager.Add(entity);
        }
        Engine.SaveGame.Player.Progression = playerProgression;
        Engine.SaveGame.Player.position = playerPosition;
        TestCompletion();
        if (playerDocked)
        {
            Engine.SaveGame.Player.Dock();
        }
        if (Engine.SaveGame.CurrentMissionCompleted && Engine.Random.Next(0, 10000) == 0)
        {
            foreach (var planet in planets)
            {
                planet.EasterEgg = true;
            }
        }
        if (!music)
        {
            SoundManager.ChangeTrack(null);
        }
        else
        {
            SoundManager.ChangeTrack(Assets.Get(Sound.main));
        }
    }
    public void Update()
    {
        if (tip != null)
        {
            ParticleManager.Add(new Particle(null, tip.Length, Engine.SaveGame.Player.position + new Vector2(0, -50), Vector2.Zero, 0, 0, Color.White, Color.Transparent) { drawText = tip });
            tip = null;
        }
        PlanetUpdate();
        TestCompletion();
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
                    if (isAggressive)
                    {
                        waveTimer = (enemiesSpawned.Count * 4f + 5f) * timerModifier;
                        maxWaveTimer = waveTimer;
                    }
                }
                currentWaveActive = true;
            }
            else
            {
                waveTimer -= Engine.DeltaSeconds;
                foreach (var enemy in enemiesSpawned)
                {
                    ParticleManager.Add(new Particle(enemy.texture, enemy.position, enemy.angle, new Color(255,127, 0) * (Engine.Random.NextSingle() / 2 + 0.25f)));
                }
            }
        }
        if (isAggressive && currentWaveActive)
        {
            if (waveTimer <= 0)
            {
                isReady = true;
            }
            else
            {
                waveTimer -= Engine.DeltaSeconds;
            }
        }
        if (isReady)
        {
            currentWaveActive = false;
            enemiesSpawned.Clear();
            EnemiesSpawned = 0;
            waveTimer = 10f * timerModifier;
            maxWaveTimer = waveTimer;
            Wave++;
            Engine.EntityManager.DecayPickups();

            if(playerProgression > 1 && (Wave % 20 == 0))
            {
                var pos = NewSpawnLocation();
                Enemy boss = bosses[currentBoss](pos, Vector2.Zero, MathF.Atan2(-pos.X, pos.Y));
                if (Wave == 40)
                {
                    boss = bosses[2](NewSpawnLocation(), Vector2.Zero, 0);
                }
                enemiesSpawned.Add(boss);
                EnemiesSpawned = 1;
                currentBoss = (currentBoss + 1) % bosses.Count;
                return;
            }
            else
            {
                if (isAggressive)
                {
                    difficulty = (int)(Math.Pow(Wave, 1.5) + 5);
                }
                else
                {
                    difficulty = (int)((Wave + 1) * MathF.Log(Wave + 1, MathF.E) - Wave) + 1;
                }
                List<int> newCosts = [];
                int availableEnemies = Math.Min(enemyCreditValues.Count, Wave / 10 + 1 + ((isAggressive) ? 1 : 0));
                Enemy squadLeader = null;
                int count = 0;
                for (int i = 0; i < availableEnemies; i++)
                {
                    newCosts.Add(enemyCreditValues[i].cost);
                }
                var enemyCredits = Engine.Random.Next((int)(difficulty), (int)(difficulty * 2));
                while (enemyCredits > 0)
                {
                    int i = Engine.Random.Next(0, availableEnemies);
                    if (Engine.Random.Next(0, enemyCreditValues[i].cost / 2) == 0 && newCosts[i] <= enemyCredits)
                    {
                        Vector2 pos;
                        if (squadLeader != null && (count < 2 || Engine.Random.Next(0, 4) != 0))
                        {
                            var offset = Vector2.Normalize(new Vector2(squadLeader.position.X, squadLeader.position.Y));
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
            if (music)
            {
                if ((Wave - 1) % 20 == 0)
                {
                    SoundManager.ChangeTrack(Assets.Get(Sound.main));
                }
                if (Wave % 20 == 0)
                {
                    SoundManager.ChangeTrack(Assets.Get(Sound.boss));
                }
            }
        }
        EventHandler.UpdateEnemyCountdownUI(waveTimer, maxWaveTimer, Wave);
    }
    public void PlanetUpdate()
    {
        foreach (var planet in planets)
        {
            foreach (var planet2 in planets)
            {
                if (planet == planet2)
                {
                    continue;
                }
                planet.AttractObject(planet2);
            }
        }
        foreach (var planet in planets)
        {
            planet.Update();
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
    public void AttractObject(Entity _entity)
    {
        foreach (var planet in planets) { planet.AttractObject(_entity); }
    }
    public void AttractObject(Particle _particle)
    {
        foreach (var planet in planets) { planet.AttractObject(_particle); }
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
        int currentPlanet = 0;
        bool hasChanged = false;
        var emitter = new ParticleEmitter(Assets.Get(Sprite.Dot), Engine.DeltaSeconds, _startPosition, 0, 0, 0, 5f, Color.Cyan, EmitterType.EmissionOverDistance);
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
                hasChanged = false;
                for(int i = futurePlanetPositions.Length - 1; i >= 0; i--)
                {
                    float sphereOfInfluence = (i == 0) ? 9999 : (Vector2.Distance(futurePlanetPositions[i], futurePlanetPositions[0]) * (float)Math.Pow(planets[i].mass / planets[0].mass, 2 / 5) / 3);
                    if (Vector2.Distance(futurePosition, futurePlanetPositions[i]) < sphereOfInfluence)
                    {
                        particlePos += planets[i].position - futurePlanetPositions[i];
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
                ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), particlePos, 0, Color.Red * 0.75f));
                return;
            }
        }
    }
    public Vector2 GetNormalizedAcceleration(Vector2 _position)
    {
        Vector2 acceleration = Vector2.Zero;
        foreach (var planet in planets)
        {
            acceleration -= planet.GetAcceleration(_position);
        }
        return acceleration;
    }
    public float GetPotentialEnergy(Vector2 _position)
    {
        float energy = 0;
        foreach (var planet in planets)
        {
            float distance = (_position - planet.position).Length();
            energy += planet.mass / distance;
        }
        return energy;
    }
    public Mission Clone()
    {
        var _planets = new GravitationalSource[planets.Length];
        for(int i = 0; i < planets.Length; i++)
        {
            _planets[i] = planets[i].Copy();
        }
        return new Mission(_planets, CopyObjectives, Name, Description, timerModifier, playerPosition, WaveGoal, tier, cutscene, escapeVehicle != null)
        { playerProgression = this.playerProgression, playerDocked = this.playerDocked, isAggressive = this.isAggressive, music = this.music, tip = this.tip };
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
    public float Hitscan(Vector2 _pos, Vector2 _dir)
    {
        float distance = 999999;
        foreach (var planet in planets)
        {
            Vector2 relativePos = planet.position - _pos;
            float closestLength = (relativePos.X * _dir.X + relativePos.Y * _dir.Y);
            float closestDistance = Vector2.Distance(_dir * closestLength + _pos, planet.position);
            float discriminant = MathF.Sqrt(planet.radius * planet.radius - closestDistance * closestDistance);
            if (closestLength > 0 && closestDistance < planet.radius && distance > closestLength - discriminant)
            {
                distance = closestLength - discriminant;
            }
        }
        return distance;
    }
    private void TestCompletion()
    {
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
            }
            else
            {
                restartTimer = 2;
                Engine.SaveGame.CompleteMission(Wave);
            }
        }
    }
}
public class EntityConstructor(Func<Vector2, Vector2, float, Entity> _constructor, Vector2 _position, Vector2 _velocity, float _angle) : IConstructor
{
    public Entity Construct()
    {
        return _constructor(_position, _velocity, _angle);
    }
}
public class AdvancedConstructor(Func<Vector2, Vector2, float, bool, Entity> _constructor, Vector2 _position, Vector2 _velocity, float _angle, bool _isFriendly) : IConstructor
{
    public Entity Construct()
    {
        return _constructor(_position, _velocity, _angle, _isFriendly);
    }
}
public class PickupConstructor(Func<Vector2, Vector2, float, Entity> _constructor, Vector2 _position, Vector2 _velocity, float _angularVelocity) : IConstructor
{
    public Entity Construct()
    {
        return _constructor(_position, _velocity, _angularVelocity);
    }
}
public interface IConstructor
{
    public Entity Construct();
}