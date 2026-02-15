using Space_Wars.Content.Main.Entities;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using Space_Wars.Content.Main.Particles;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Space_Wars.Content.Main.Story;

namespace Space_Wars.Content.Main;
public class Mission
{
    public delegate Enemy DelegateEnemy(Vector2 position, Vector2 velocity, float angle, bool _isFriendly = false);
    //Change me when multiple main planets are added
    public Planet Planet => (planets.Length > 0) ? planets[0] : new Planet(Vector2.Zero, Vector2.Zero, 0, 1, true, Color.White);
    public string Name { get; }
    public string Description { get; }
    public int playerProgression = 3;
    public int Wave { get; private set; } = 0;
    public int EnemiesSpawned { get; private set; } = 0;
    public float RestartTimer { get; set; } = -1;
    public float TimerModifier { get; set; }
    public bool playerDocked = false;
    public bool isAggressive = false;
    public bool music = true;
    public bool relaunchable = false;
    public string tip = null;

    private EntityConstructor escapeVehicle = null;
    private Planet[] planets;
    public Planet[] Planets { get { return planets; } }
    //Save original entity parameters to allow cloning
    private List<ICondition> CopyObjectives { get; }
    private List<ICondition> MissionObjectives { get; } = [];
    private List<Enemy> enemiesSpawned = [];
    private List<(int cost, DelegateEnemy enemy)> enemyCreditValues;
    private List<DelegateEnemy> bosses;
    private Vector2 playerPosition;
    private Func<Cutscene> cutscene;
    private Func<Cutscene> endCutscene;
    private int currentBoss;
    private float waveTimer = 5;
    private float maxWaveTimer = 5;
    private bool currentWaveActive = false;

    public Mission(Planet[] _planets, List<ICondition> _missionObjectives, string _name, string _description, float _timerModifier, Vector2 _playerPosition, List<(int, DelegateEnemy)> _enemies, List<DelegateEnemy> _bosses, Func<Cutscene> _cutscene = null, bool _escapeVehicle = false, Func<Cutscene> _endCutscene = null)
    {
        Name = _name;
        Description = _description;
        planets = _planets;
        CopyObjectives = _missionObjectives;
        playerPosition = _playerPosition;
        foreach (var condition in _missionObjectives)
        {
            MissionObjectives.Add(condition.Clone());
        }
        TimerModifier = _timerModifier;
        enemyCreditValues = _enemies;
        bosses = _bosses;

        currentBoss = Util.Random.Next(bosses.Count);
        cutscene = _cutscene;
        endCutscene = _endCutscene;
        if (_escapeVehicle)
        {
            escapeVehicle = new EntityConstructor(Enemy.NewPickupDrone, new Vector2(-2000, -2000), Vector2.Zero, 0);
        }
        UI.WaveText.text = "0";
        UI.EnemiesLeft.text = "0";
    }
    public void Initialize()
    {
        Engine.SaveGame.Player.dockedEntity = null;
        foreach (var objective in MissionObjectives)
        {
            objective.Initialize();
        }
        Engine.SaveGame.Player.Progression = playerProgression;
        Engine.SaveGame.Player.position = playerPosition;
        TestCompletion();
        if (playerDocked)
        {
            Engine.SaveGame.Player.Dock();
        }
        if (Engine.SaveGame.CurrentMissionCompleted && Util.Random.Next(0, 10000) == 0)
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
            ParticleManager.Add(new Particle(null, tip.Length, new Vector2(0, -1.5f * Planet.radius) + new Vector2(0, -50), Vector2.Zero, 0, 0, Color.White, Color.Transparent) { drawText = tip });
            tip = null;
        }
        PlanetUpdate();
        TestCompletion();
        if (RestartTimer != -1)
        {
            if (RestartTimer > 0)
            {
                RestartTimer -= Engine.DeltaSeconds;
                return;
            }
            if(endCutscene != null)
            {
                EventHandler.MissionSelectTrigger(endCutscene());
            }
            else
            {
                EventHandler.MissionSelectTrigger(new MissionSelect());
            }
            if (TestCompletion())
            {
                Engine.SaveGame.CompleteMission(Wave);
            }
        }
        //Natural enemy spawning toggle
        if (TimerModifier == -1)
        {
            return;
        }
        var isReady = true;
        foreach (var enemy in enemiesSpawned)
        {
            if (enemy.health > 0)
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
                        waveTimer = (enemiesSpawned.Count * 4f + 5f) * TimerModifier;
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
                    ParticleManager.Add(new Particle(enemy.texture, enemy.position, enemy.angle, new Color(255, 127, 0) * (Util.Random.NextSingle() / 2 + 0.25f)));
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
            waveTimer = 10f * TimerModifier;
            maxWaveTimer = waveTimer;
            Wave++;
            Engine.EntityManager.DecayPickups();
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

            if (playerProgression > 1 && (Wave % 20 == 0))
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
            }
            else
            {
                float difficulty = 0;
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
                var enemyCredits = Util.Random.Next((int)(difficulty), (int)(difficulty * 2));
                while (enemyCredits > 0)
                {
                    int i = Util.Random.Next(0, availableEnemies);
                    if (Util.Random.Next(0, enemyCreditValues[i].cost / 2) == 0 && newCosts[i] <= enemyCredits)
                    {
                        Vector2 pos;
                        if (squadLeader != null && (count < 2 || Util.Random.Next(0, 4) != 0))
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
                        var enemy = enemyCreditValues[i].enemy(pos, Engine.SaveGame.Player.velocity, MathF.Atan2(-pos.X, pos.Y));
                        enemiesSpawned.Add(enemy);
                        squadLeader ??= enemy;
                        enemyCredits -= newCosts[i];
                        newCosts[i] += 1;
                        EnemiesSpawned++;
                    }
                    if (enemyCredits < newCosts.Min(c => c))
                    {
                        break;
                    }
                }
            }
            //Mess with probability later
            if (Util.Random.NextSingle() > 0.25f)
            {
                Enemy enemy = enemiesSpawned[Util.Random.Next(0, enemiesSpawned.Count)];
                enemy.AddBehaviour(enemy.DropItem(ItemFactory.NewScrap));
            }
        }
        UI.EnemiesLeft.text = (currentWaveActive ? enemiesSpawned.Where(x => x.health > 0).Count() : 0).ToString();
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
    public Planet IsColliding(Vector2 _position)
    {
        foreach (var planet in planets)
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
    public float GetAtmospherePressure(Entity _entity)
    {
        float sum = 0;
        foreach (var planet in planets) 
        { 
            sum += planet.GetAtmosphereDensity(_entity); 
        }
        return sum;
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
        if (RestartTimer != -1)
        {
            return;
        }
        RestartTimer = 2;
    }
    public void CompleteCustomRule(Entity _target)
    {
        foreach (var objective in MissionObjectives)
        {
            if (objective is EntityCondition)
            {
                (objective as EntityCondition).CustomCompleteRule(_target);
            }
        }
    }
    public void CalculateTrajectory(Vector2 _startPosition, Vector2 _startVelocity, float _radius)
    {
        Vector2 futurePosition = _startPosition;
        Vector2 futureVelocity = _startVelocity;
        Vector2[] futurePlanetPositions = [.. planets.Select(planet => planet.position)];
        Vector2[] futurePlanetVelocities = [.. planets.Select(planet => planet.velocity)];
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
                for (int i = futurePlanetPositions.Length - 1; i >= 0; i--)
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
    public Planet GetNearestPlanet(Vector2 _position)
    {
        float nearestDistance = float.MaxValue;
        Planet nearestPlanet = null;
        foreach(var planet in planets)
        {
            float distance = Vector2.Distance(_position, planet.position);
            if(distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPlanet = planet;
            }
        }
        return nearestPlanet;
    }
    public Mission Clone()
    {
        var _planets = new Planet[planets.Length];
        for (int i = 0; i < planets.Length; i++)
        {
            _planets[i] = planets[i].Copy();
        }
        bool isInFleet = Engine.SaveGame.FleetSystem > Engine.EntityManager.Systems[Engine.SaveGame.CurrentMissionIndex].system;
        float tm = TimerModifier;
        if (isInFleet && tm != -1)
        {
            tm /= 2;
        }
        return new Mission(_planets, CopyObjectives, Name, Description, tm, playerPosition, enemyCreditValues, bosses, cutscene, escapeVehicle != null, endCutscene)
        { playerProgression = this.playerProgression, playerDocked = this.playerDocked, isAggressive = this.isAggressive || isInFleet, music = this.music, tip = this.tip, relaunchable = this.relaunchable };
    }
    private Vector2 NewSpawnLocation()
    {
        float angle = (Util.Random.NextSingle() - 0.5f) * MathF.Tau;
        float distanceMultiplier = 1 + (Util.Random.NextSingle() - 0.5f) / 4;
        float distance = (Engine.ScreenSize.X + Engine.ScreenSize.Y) * distanceMultiplier / 3;
        Vector2 spawnLocation = Util.ToUnitVector(angle) * distance + Engine.SaveGame.Player.position;
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
    private bool TestCompletion()
    {
        bool allCompleted = true;
        foreach (var objective in MissionObjectives)
        {
            allCompleted &= objective.IsComplete();
        }
        if (allCompleted && RestartTimer == -1)
        {
            if (escapeVehicle != null)
            {
                var objective = new EntityCondition(escapeVehicle, [Condition.Protect, Condition.CustomIncomplete]);
                MissionObjectives.Add(objective);
                objective.Initialize();
                escapeVehicle = null;
            }
            else
            {
                RestartTimer = 2;
            }
        }
        return allCompleted;
    }
    public void Draw(SpriteBatch _spriteBatch)
    {
        foreach (var planet in planets)
        {
            planet.Draw(_spriteBatch);
        }
    }
    public static List<(int, DelegateEnemy)> TierOne()
    {
        return
        [
            (1, Enemy.NewFighter),
            (3, Enemy.NewSniper),
            (4, Enemy.NewShotgunner),
            (4, Enemy.NewCarrier),
        ];
    }
    public static List<DelegateEnemy> TierOneBosses()
    {
        return
        [
            Enemy.NewSymmetryBoss,
            Enemy.NewWyvernBoss,
            Enemy.NewDeadeyeBoss,
        ];
    }
    public static List<(int, DelegateEnemy)> TierTwo()
    {
        return
        [
            (1, Enemy.NewAdvancedFighter),
            (2, Enemy.NewHovercraft),
            (2, Enemy.NewHealer),
        ];
    }
    public static List<DelegateEnemy> TierTwoBosses()
    {
        return
        [
            Enemy.NewOverloadBoss,
            Enemy.NewSurgeBoss,
            Enemy.NewStreamlineBoss
        ];
    }
    public static List<(int, DelegateEnemy)> TierThree()
    {
        return
        [
            (1, Enemy.NewStealthFighter),
            (2, Enemy.NewHunter),
            (3, Enemy.NewEngineer),
        ];
    }
    public static List<DelegateEnemy> TierThreeBosses()
    {
        return
        [
            Enemy.NewPursuerBoss,
            Enemy.NewContinuumBoss,
        ];
    }
    public static List<(int, DelegateEnemy)> All()
    {
        return [.. TierOne(), .. TierTwo(), .. TierThree()];
    }
    public static List<DelegateEnemy> AllBosses()
    {
        return [.. TierOneBosses(), .. TierTwoBosses(), .. TierThreeBosses()];
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
public class LaunchConstructor(Func<Vector2, float, Entity> _constructor,Vector2 _position, float _distance) : IConstructor
{
    public Entity Construct()
    {
        return _constructor(_position, _distance);
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
public interface ICondition
{
    public bool IsComplete();
    public void Initialize();
    public ICondition Clone();
}
public class EntityCondition(IConstructor _constructor, Condition[] _conditions) : ICondition
{
    private Entity daughterEntity = null;
    public bool IsComplete()
    {
        foreach (var condition in _conditions)
        {
            switch (condition)
            {
                case Condition.Protect:
                    if (daughterEntity.isExpired)
                    {
                        Engine.SaveGame.CurrentMission.FailMission();
                        return false;
                    }
                    break;
                case Condition.Kill:
                    if (!daughterEntity.isExpired)
                    {
                        return false;
                    }
                break;
                case Condition.CustomIncomplete:
                    return false;
            }
        }
        return true;
    }
    public void Initialize()
    {
        daughterEntity = _constructor.Construct();
        Engine.EntityManager.Add(daughterEntity);
    }
    public ICondition Clone()
    {
        return new EntityCondition(_constructor, (Condition[])_conditions.Clone());
    }
    public void CustomCompleteRule(Entity _entity)
    {
        if (daughterEntity == _entity && _conditions.Contains(Condition.CustomIncomplete))
        {
            _conditions[Array.IndexOf(_conditions, Condition.CustomIncomplete)] = Condition.CustomComplete;
        }
    }
}
public class WaveGoal(int _targetWave) : ICondition
{
    public void Initialize() { }
    public bool IsComplete() 
    {
        return Engine.SaveGame.CurrentMission.Wave > _targetWave;
    }
    public ICondition Clone()
    {
        return new WaveGoal(_targetWave);
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
    public ICondition Clone()
    {
        return new DialogueCondition(_dialogues);
    }
}