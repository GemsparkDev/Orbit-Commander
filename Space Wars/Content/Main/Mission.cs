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
    public bool relaunchable = false;
    public int Wave {get; set;} = 0;
    //TODO: Add behavior for when there is no planet
    public Planet Planet { get { return GetComponent<Planets>().GetPlanets[0]; }}
    private Sound music;
    private Conditional objective;
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
    public Mission(List<IMissionComponent> _components, Conditional _objective, string _name, 
    string _description, IPlayerSpawner _spawner, int _playerProgression = 3, Sound _music = Sound.main)
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
        spawner = _spawner;
        objective = _objective;
        playerProgression = _playerProgression;
        music = _music;
    }
    public void Initialize()
    {
        CurrentGameState.SwitchState(new PlayingGame());
        Engine.SaveGame.Player.dockedEntity = null;
        Engine.SaveGame.Player.Progression = PlayerProgression;
        foreach(var comp in components)
        {
            comp.Initialize();
        }
        objective.Initialize();
        spawner.Spawn();
        SoundManager.ChangeTrack(Assets.Get(music));
    }
    public void Update()
    {
        foreach(var comp in components)
        {
            comp.Update();
        }
        if(objective != null)
        {
            objective = objective.Update();    
        }
    }
    public ICollider IsColliding(Vector2 _position, Vector2 _velocity, float _colliderRadius, bool _override, out float end)
    {
        end = _velocity.Length();
        ICollider returnObstacle = null;
        foreach(var obstacle in obstacles)
        {
            var collider = obstacle.IsColliding(_position, _velocity, _colliderRadius, _override, out float _end);
            if(collider != null)
            {
                if(_end < end)
                {
                    end = _end;
                    returnObstacle = collider;
                }
            }
        }
        return returnObstacle;
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
        throw new NotImplementedException();
    }
    public void CompleteCustomRule(Entity _target)
    {
        objective.CompleteCustomRule(_target);
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
                if(collider.IsColliding(futurePosition, futureVelocity, Engine.SaveGame.Player.ColliderRadius, false, out float _))
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
        List<IMissionComponent> comps = [];
        foreach(var component in components)
        {
            comps.Add(component.Clone());
        }
        return new Mission(comps, objective.Clone(), Name, Description, spawner, PlayerProgression, music);
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
    }
    public static List<(int, Func<Vector2, Vector2, float, Team, Enemy>)> TierOneEnemies()
    {
        return
        [
            (1, Enemy.NewFighter),
            (3, Enemy.NewSniper),
            (4, Enemy.NewShotgunner),
            (4, Enemy.NewCarrier),
        ];
    }
    public static List<Func<Vector2, Vector2, float, Team, Enemy>> TierOneBosses()
    {
        return
        [
            Enemy.NewSymmetryBoss,
            Enemy.NewWyvernBoss,
            Enemy.NewDeadeyeBoss,
        ];
    }
    public static List<(int, Func<Vector2, Vector2, float, Team, Enemy>)> TierTwoEnemies()
    {
        return
        [
            (1, Enemy.NewAdvancedFighter),
            (2, Enemy.NewHovercraft),
            (2, Enemy.NewHealer),
        ];
    }
    public static List<Func<Vector2, Vector2, float, Team, Enemy>> TierTwoBosses()
    {
        return
        [
            Enemy.NewOverloadBoss,
            Enemy.NewSurgeBoss,
            Enemy.NewStreamlineBoss
        ];
    }
    public static List<(int, Func<Vector2, Vector2, float, Team, Enemy>)> TierThreeEnemies()
    {
        return
        [
            (1, Enemy.NewStealthFighter),
            (2, Enemy.NewHunter),
            (3, Enemy.NewEngineer),
        ];
    }
    public static List<Func<Vector2, Vector2, float, Team, Enemy>> TierThreeBosses()
    {
        return
        [
            Enemy.NewPursuerBoss,
            Enemy.NewContinuumBoss,
        ];
    }
    public static List<(int, Func<Vector2, Vector2, float, Team, Enemy>)> AllEnemies()
    {
        return [.. TierOneEnemies(), .. TierTwoEnemies(), .. TierThreeEnemies()];
    }
    public static List<Func<Vector2, Vector2, float, Team, Enemy>> AllBosses()
    {
        return [.. TierOneBosses(), .. TierTwoBosses(), .. TierThreeBosses()];
    }
    public static (List<(int, Func<Vector2, Vector2, float, Team, Enemy>)>, 
    List<Func<Vector2, Vector2, float, Team, Enemy>>) T1()
    {
        return (TierOneEnemies(), TierOneBosses());
    }
        public static (List<(int, Func<Vector2, Vector2, float, Team, Enemy>)>, 
    List<Func<Vector2, Vector2, float, Team, Enemy>>) T2()
    {
        return (TierTwoEnemies(), TierTwoBosses());
    }
        public static (List<(int, Func<Vector2, Vector2, float, Team, Enemy>)>, 
    List<Func<Vector2, Vector2, float, Team, Enemy>>) T3()
    {
        return (TierThreeEnemies(), TierThreeBosses());
    }
        public static (List<(int, Func<Vector2, Vector2, float, Team, Enemy>)>, 
    List<Func<Vector2, Vector2, float, Team, Enemy>>) All()
    {
        return (AllEnemies(), AllBosses());
    }
    //TODO: Find a better way to do this
    //Delegate stacking is messy
    public static Func<Conditional> SendPickup(Func<GameState> _scene = null)
    {
        return delegate{
        return new Conditional([new EntityCondition(new LaunchConstructor(Enemy.NewPickupDrone, new Vector2(-2000, 2000), Engine.SaveGame.CurrentMission.Planet.radius * 1.25f), [Condition.CustomIncomplete])], 
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
public class EntityConstructor(Func<Vector2, Vector2, float, Entity> _constructor, Vector2 _position, Vector2 _velocity, float _angle) : IConstructor
{
    public Entity Construct()
    {
        return _constructor(_position, _velocity, _angle);
    }
}
public class AdvancedConstructor(Func<Vector2, Vector2, float, Team, Entity> _constructor, Vector2 _position, Vector2 _velocity, float _angle, Team _team) : IConstructor
{
    public Entity Construct()
    {
        return _constructor(_position, _velocity, _angle, _team);
    }
}
public class PickupConstructor(Func<Vector2, Vector2, float, Entity> _constructor, Vector2 _position, Vector2 _velocity, float _angularVelocity) : IConstructor
{
    public Entity Construct()
    {
        return _constructor(_position, _velocity, _angularVelocity);
    }
}
public class LaunchConstructor(Func<Vector2, float, Entity> _constructor, Vector2 _position, float _distance) : IConstructor
{
    public Entity Construct()
    {
        return _constructor(_position, _distance);
    }
}
public interface IConstructor : IMissionComponent
{
    public Entity Construct();
    void IMissionComponent.Initialize()
    {
        Engine.EntityManager.Add(Construct());
    }
    void IMissionComponent.Update()
    {
        
    }
    void IMissionComponent.Draw(SpriteBatch _spriteBatch)
    {
        
    }
    //All constructors should be constant methods
    IMissionComponent IMissionComponent.Clone()
    {
        return this;
    }
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