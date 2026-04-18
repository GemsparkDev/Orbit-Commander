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
using System.Security.Cryptography.X509Certificates;
//using System.Numerics;

namespace Space_Wars.Content.Main;
public class Mission
{
    public int Wave { get; set; } = 0;
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
    public Mission(List<IMissionComponent> _components, Conditional _objective, IPlayerSpawner _spawner, int _playerProgression = 3, Sound _music = Sound.main)
    {
        components = _components;
        foreach(var comp in _components)
        {
            if(comp is IObstacle)
            {
                obstacles.Add(comp as IObstacle);
            }
        }
        spawner = _spawner;
        objective = _objective;
        music = _music;
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
        //Prevents players from losing important items
        Entity[] importantEntites = Engine.EntityManager.GetEntity<KeyTag>();
        float r = EntityManager.missions[Engine.SaveGame.CurrentMissionIndex].data.EdgeRadius;
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
    }
    public ICollider IsColliding(Vector2 _position, Vector2 _velocity, float _colliderRadius, bool _override, out float end)
    {
        end = _velocity.Length();
        ICollider returnObstacle = null;
        foreach(var entity in Engine.EntityManager.Entities)
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
        foreach(var entity in Engine.EntityManager.Entities)
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
        Entity[] planets = Engine.EntityManager.Entities.Where(x => x is Planet).ToArray();
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
                for (int i = futurePlanetPositions.Length - 1; i >= 0; i--)
                {
                    float sphereOfInfluence = (i == 0) ? 9999 : (Vector2.Distance(futurePlanetPositions[i], futurePlanetPositions[0])
                        * (float)Math.Pow((planets[i] as Planet).mass / (planets[0] as Planet).mass, 2 / 5) / 3);
                    if (Vector2.Distance(futurePosition, futurePlanetPositions[i]) < sphereOfInfluence)
                    {
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
        return new Conditional([new Custom(Enemy.NewPickupDrone(new Vector2(-2000, 2000), Engine.SaveGame.CurrentMission.Planet.ColliderRadius * 1.25f))], 
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