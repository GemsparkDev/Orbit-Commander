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
//using System.Numerics;

namespace Space_Wars.Content.Main;
public class Mission
{
    public string Name { get; }
    public string Description { get; }
    private int playerProgression = 3;
    public int PlayerProgression { get { if (SaveGame.DebugMode) { return 99; } else { return playerProgression; } } set { playerProgression = value; } }
    public float RestartTimer { get; set; } = -1;
    public bool music = true;
    public bool relaunchable = false;
    public int Wave {get; set;} = 0;
    //TODO: Add behavior for when there is no planet
    public Planet Planet { get { return GetComponent<Planets>().GetPlanets[0]; }}

    private EntityConstructor escapeVehicle = null;
    //Save original entity parameters to allow cloning
    private List<ICondition> CopyObjectives { get; }
    private List<ICondition> MissionObjectives { get; } = [];
    private IPlayerSpawner spawner;

    private List<IMissionComponent> components = [];
    private List<IObstacle> obstacles = [];
    public void AddComponent(IMissionComponent component)
    {
        components.Add(component);
        if(component is IObstacle)
        {
            obstacles.Add(component as IObstacle);
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
    public Mission(List<IMissionComponent> _components, List<ICondition> _missionObjectives, string _name, string _description, IPlayerSpawner _spawner, bool _escapeVehicle = false)
    {
        components = _components;
        foreach(var comp in _components)
        {
            if(comp is IObstacle)
            {
                obstacles.Add(comp as IObstacle);
            }
        }
        Name = _name;
        Description = _description;
        CopyObjectives = _missionObjectives;
        spawner = _spawner;
        foreach (var condition in _missionObjectives)
        {
            MissionObjectives.Add(condition.Clone());
        }
        if (_escapeVehicle)
        {
            escapeVehicle = new EntityConstructor(Enemy.NewPickupDrone, new Vector2(-2000, -2000), Vector2.Zero, 0);
        }
    }
    public void Initialize()
    {
        foreach(var comp in components)
        {
            comp.Initialize();
        }
        Engine.SaveGame.Player.dockedEntity = null;
        foreach (var objective in MissionObjectives)
        {
            objective.Initialize();
        }
        Engine.SaveGame.Player.Progression = PlayerProgression;
        spawner.Spawn();
        TestCompletion();
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
        foreach(var comp in components)
        {
            comp.Update();
        }
        TestCompletion();
        if (RestartTimer != -1)
        {
            if (RestartTimer > 0)
            {
                RestartTimer -= Engine.DeltaSeconds;
                return;
            }
            if (TestCompletion())
            {
                Engine.SaveGame.CompleteMission(Wave);
            }
        }        
    }
    public ICollider IsColliding(Vector2 _position, Vector2 _velocity, float _colliderRadius, bool _override)
    {
        foreach(var obstacle in obstacles)
        {
            var collider = obstacle.IsColliding(_position, _velocity, _colliderRadius, _override);
            if(collider != null)
            {
                return collider;
            }
        }
        return null;
    }
    public float GetAtmospherePressure(Entity _entity)
    {
        float sum = 0;
        var comp = GetComponent<Planets>();
        if(comp != null)
        {
            sum = comp.GetAtmospherePressure(_entity);
        }
        return sum;
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
        Planet[] planets = [];
        if(GetComponent<Planets>() != null)
        {
            planets = GetComponent<Planets>().GetPlanets;
        }
        ICollider[] Colliders = [];
        if (GetComponent<Colliders>() != null)
        {
            Colliders = GetComponent<Colliders>().GetColliders;
        }
        Vector2 futurePosition = _startPosition;
        Vector2 futureVelocity = _startVelocity;
        Vector2[] futurePlanetPositions = [.. planets.Select(planet => planet.position)];
        Vector2[] futurePlanetVelocities = [.. planets.Select(planet => planet.velocity)];
        int currentPlanet = 0;
        bool hasChanged = false;
        var emitter = new ParticleEmitter(Assets.Get(Sprites.Dot), Engine.DeltaSeconds, _startPosition, 0, 0, 0, 5f, Color.Cyan, EmitterType.EmissionOverDistance);
        bool exit = false;

        for (int n = 0; n < 1000; n++)
        {
            foreach(var collider in Colliders)
            {
                if(collider.IsColliding(futurePosition, futureVelocity, Engine.SaveGame.Player.ColliderRadius))
                {
                    exit = true;
                    break;
                }
            }
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
            if (SaveGame.PatchedConics)
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
    public Mission Clone()
    {
        throw new NotImplementedException();
    }
    public Vector2 NewSpawnLocation()
    {
        float angle = (Util.Random.NextSingle() - 0.5f) * MathF.Tau;
        float distanceMultiplier = 1 + (Util.Random.NextSingle() - 0.5f) / 4;
        float distance = (Engine.ScreenSize.X + Engine.ScreenSize.Y) * distanceMultiplier / 3;
        Vector2 spawnLocation = Util.ToUnitVector(angle) * distance + Engine.SaveGame.Player.Position;
        if(IsColliding(spawnLocation, Vector2.Zero, 10, true) != null)
        {
            return NewSpawnLocation();
        }
        return spawnLocation;
    }
    public float Hitscan(Vector2 _pos, Vector2 _dir)
    {
        Enemy enemy = new Enemy(_pos, _dir * 99999, 0, 1, Assets.Get(Sprites.Dot));
        foreach(var obstacle in obstacles)
        {
            obstacle.Collide(enemy);
        }
        return Vector2.Distance(_pos, enemy.Position);
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
        foreach(var comp in components)
        {
            comp.Draw(_spriteBatch);
        }
    }
    public static List<(int, Func<Vector2, Vector2, float, bool, Enemy>)> TierOne()
    {
        return
        [
            (1, Enemy.NewFighter),
            (3, Enemy.NewSniper),
            (4, Enemy.NewShotgunner),
            (4, Enemy.NewCarrier),
        ];
    }
    public static List<Func<Vector2, Vector2, float, bool, Enemy>> TierOneBosses()
    {
        return
        [
            Enemy.NewSymmetryBoss,
            Enemy.NewWyvernBoss,
            Enemy.NewDeadeyeBoss,
        ];
    }
    public static List<(int, Func<Vector2, Vector2, float, bool, Enemy>)> TierTwo()
    {
        return
        [
            (1, Enemy.NewAdvancedFighter),
            (2, Enemy.NewHovercraft),
            (2, Enemy.NewHealer),
        ];
    }
    public static List<Func<Vector2, Vector2, float, bool, Enemy>> TierTwoBosses()
    {
        return
        [
            Enemy.NewOverloadBoss,
            Enemy.NewSurgeBoss,
            Enemy.NewStreamlineBoss
        ];
    }
    public static List<(int, Func<Vector2, Vector2, float, bool, Enemy>)> TierThree()
    {
        return
        [
            (1, Enemy.NewStealthFighter),
            (2, Enemy.NewHunter),
            (3, Enemy.NewEngineer),
        ];
    }
    public static List<Func<Vector2, Vector2, float, bool, Enemy>> TierThreeBosses()
    {
        return
        [
            Enemy.NewPursuerBoss,
            Enemy.NewContinuumBoss,
        ];
    }
    public static List<(int, Func<Vector2, Vector2, float, bool, Enemy>)> All()
    {
        return [.. TierOne(), .. TierTwo(), .. TierThree()];
    }
    public static List<Func<Vector2, Vector2, float, bool, Enemy>> AllBosses()
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