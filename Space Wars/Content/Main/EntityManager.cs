using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Components;
using Space_Wars.Content.Main.Particles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Space_Wars.Content.Main;

public class EntityManager
{
    private bool isUpdating = false;
    private List<Entity> entities = [];
    private List<Entity> addedEntities = [];
    private List<Entity> enemies = [];
    private List<Projectile> projectiles = [];
    private static float currentKarma = 0;
    private Player Player => Engine.SaveGame.Player;
    //Maximum distance for any detection when sensing = stealth
    public static float StealthRange { get; private set; } = 750;
    //Threshold of detection for enemies
    public static float StealthThreshold { get; private set; } = 0.75f;
    public int MissionLength => missions.Count;
    private readonly List<Mission> missions = 
    [
        new([ new(Vector2.Zero, Vector2.Zero, 10000, 8, true, Color.Cyan), new(new Vector2(1000, 0), GravitationalSource.GetOrbitalVelocity(new Vector2(1000, 0), Vector2.Zero, 10000), 250, 1.5f, false, Color.Cyan) ],
        [ (new EntityConstructor(Enemy.NewMothership, new Vector2(0, -8*50 - Assets.DimsOf(Sprite.Mothership).Y / 2), Vector2.Zero, 0f), [ Condition.Protect, Condition.CustomIncomplete ])],
        "Crash Landing",
        "A simple system with a large planet and one closely orbiting moon. Drone activity detected, but minimal.",
        1, 0, 0, IntroCutscene),

        new( [ new(Vector2.Zero, Vector2.Zero, 3500, 4, true, Color.Cyan, true) ],
        [
            (new EntityConstructor(Enemy.NewTurret, new Vector2(0, -200 - Assets.DimsOf(Sprite.TurretBase).Y / 2), Vector2.Zero, 0), [ Condition.Protect ]),
            (new EntityConstructor(Enemy.NewOrbiter, new Vector2(400, 0), GravitationalSource.GetOrbitalVelocity(new Vector2(400, 0), Vector2.Zero, 3500), 0), [ Condition.Protect ])],
        "Sentry Defense",
        "A small outpost is located orbiting this rogue planet. Defend it.", 0.75f, 40),

        new( [ new(Vector2.Zero, Vector2.Zero, 25000, 7f, true, Color.Cyan), new(new Vector2(800, 0), GravitationalSource.GetOrbitalVelocity(new Vector2(800, 0), Vector2.Zero, 25000), 150, 0.5f, false, Color.Cyan), ],
        [(new EntityConstructor(Enemy.NewMiner, new Vector2(0, -7*50 - Assets.DimsOf(Sprite.Miner).Y / 2), Vector2.Zero, 0), [ Condition.Protect ])],
        "Extraction",
        "This deceptively dense planet is rich with materials that our deployed miner will extract.", 1, 20),

        new([ new(Vector2.Zero, Vector2.Zero, 5000, 3, true, Color.Cyan),
            new(new Vector2(400, 0), GravitationalSource.GetOrbitalVelocity(new Vector2(400, 0), Vector2.Zero, 5000), 240, 1f, false, Color.Cyan),
            new(new Vector2(-600, 0), -GravitationalSource.GetOrbitalVelocity(new Vector2(-600, 0), Vector2.Zero, 5000) * 1.2f, 120, 0.6f, false, Color.Yellow), ],
        [(new EntityConstructor(Enemy.NewExcursionBoss, new Vector2(0, -6*50), Vector2.Zero, 0), [ Condition.Kill ])],
        "Showdown",
        "Defeat the advanced drone prototype, Excursion. Be warned: It may call for reinforcements.", 1.1f, 0, 0),

        new([new(Vector2.Zero, Vector2.Zero, 30000, 10f, true, Color.HotPink, true) ],
        [],
        "cool planet",
        "Super earth", 2, 0, 2, null, true),
    ];
    private Mission currentMission;
    public Mission CurrentMission => currentMission ?? missions[missionCount];
    private int missionCount = 0;
    public int MissionCount => missionCount;
    public List<Queueable> QueuedItems { get; private set; } = [];

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
        currentMission = missions[missionCount].Clone();
        entities.Clear();
        addedEntities.Clear();
        enemies.Clear();
        projectiles.Clear();
        Player.position = new Vector2(0, -CurrentMission.Planet.radius + 1);
        Player.velocity = Vector2.Zero;
        CurrentMission.Initialize();
    }
    public void PlayerUpdate()
    {
        Player.RestrictedActions();
    }
    public void IngameUpdate()
    {
        Player.Update();
        CurrentMission.AttractObject(Player);
        if (Player.dockedEntity == null)
        {
            CurrentMission.CalculateTrajectory(Player.position, Player.velocity, Player.ColliderRadius);
        }
        if (Player.isExpired)
        {
            foreach (var module in Player.modules)
            {
                //module.Value.Health = module.Value.MaxHealth;
                module.Value.Health = 20;
                module.Value.isFailed = false;
            }
            Engine.Startgame();
        }
        Engine.MousePositionOffset = new Vector2(Mouse.GetState().X, Mouse.GetState().Y) / 10 - Engine.BackBuffer / 20
            + Engine.ScreenShakeFactor * Engine.ScreenShakeFactor * new Vector2(Engine.Random.NextSingle() - 0.5f, Engine.Random.NextSingle() - 0.5f) * 50;
        Engine.Camera.Rotation = Engine.ScreenShakeFactor * Engine.ScreenShakeFactor * (Engine.Random.NextSingle() - 0.5f) * 0.15f;
        //If the player is further from the camera, put more weight on the player
        //Tanh prevents frac from going above 1
        float frac = MathF.Tanh(Vector2.Distance(Player.position, Engine.Camera.Position) / 750);
        Engine.Camera.Position = Player.position * frac + Engine.Camera.Position * (1 - frac);
        var time = Engine.IngameTime;
        time.Duration += Engine.DeltaSeconds;
        Engine.IngameTime = time;
        CurrentMission.Update();
        Engine.EntityManager.Update();
    }
    public void Update()
    {
        isUpdating = true;
        //Prevents modifying a list while iterating over it
        foreach (var entity in entities)
        {
            entity.Update();
            CurrentMission.AttractObject(entity);
        }

        if (projectiles.Count >= 150)
        {
            for (int i = 0; i < projectiles.Count - 150; i++)
            {
                projectiles[i].isExpired = true;
            }
        }
        entities = entities.Where(x => !x.isExpired).ToList();
        projectiles = projectiles.Where(x => !x.isExpired).ToList();
        enemies = enemies.Where(x => !x.isExpired).ToList();

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
    }
    public void CompleteMission(int _duration)
    {
        int points = _duration / 10;
        foreach (var item in QueuedItems)
        {
            points = item.AttemptConstruct(points);
            if (points <= 0)
            {
                break;
            }
        }
        QueuedItems = (from item in QueuedItems where !item.IsExpired select item).ToList();
    }
    public void SetMission(int _count)
    {
        missionCount = Math.Clamp(_count, 0, missions.Count - 1);
        currentMission = missions[missionCount].Clone();
        EventHandler.UpdateMissionText();
    }
    public void NextMission()
    {
        missionCount = Math.Clamp(missionCount + 1, 0, missions.Count - 1);
        currentMission = missions[missionCount].Clone();
        EventHandler.UpdateMissionText();
    }
    public void PrevMission()
    {
        missionCount = Math.Clamp(missionCount - 1, 0, missions.Count - 1);
        currentMission = missions[missionCount].Clone();
        EventHandler.UpdateMissionText();
    }
    public void DecayPickups()
    {
        foreach (var pickup in entities)
        {
            if (pickup is Pickup && Engine.Random.NextSingle() < 0.6f)
            {
                pickup.Collide(1);
            }
        }
    }
    public static void Collide(Entity entity, Entity targetEntity)
    {
        //Checks if two entities are closer than the radii combined
        if (entity == null || targetEntity == null)
        {
            return;
        }
        if (DistanceSqr(entity, targetEntity) <= MathF.Pow(entity.ColliderRadius + targetEntity.ColliderRadius, 2) && entity.isFriendly != targetEntity.isFriendly)
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
            if (entity.isFriendly != _entity.isFriendly)
            {
                continue;
            }
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
    public Entity NearestEnemy(Entity entity)
    {
        float maxDistSqr = StealthRange * StealthRange * StealthThreshold * StealthThreshold;
        float nearestDistance = float.MaxValue;
        Entity returnEnemy = null;
        foreach (Entity targetEnemy in enemies)
        {
            if (targetEnemy.isFriendly == entity.isFriendly)
            {
                continue;
            }
            if (targetEnemy.StealthAbility > entity.SensingAbility)
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
            float distance = DistanceSqr(entity, Player);
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
            if (targetEntity is not Pickup)
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
    public Entity NearestProjectile(Entity _entity)
    {
        float nearestDistance = float.MaxValue;
        Entity returnProjectile = null;
        foreach (Projectile targetProjectile in projectiles)
        {
            float distance = DistanceSqr(_entity, targetProjectile);
            if (distance < nearestDistance && _entity.isFriendly != targetProjectile.isFriendly
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
            if (distance < nearestDistance && _entity.isFriendly != missile.isFriendly
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
        if (Engine.Random.NextSingle() < (1 / _rarity) + karmaBonus)
        {
            currentKarma = 0;
            return true;
        }
        currentKarma += (1 / _rarity);
        return false;
    }
    private static Cutscene IntroCutscene()
    {
        List<IEvent> events = [];
        List<Actor> actors = [];
        var mothership = new Actor(Assets.Get(Sprite.Mothership), new Vector2(1500, -2000), new Color(0, 255, 0), MathF.PI / 12);
        var sound = Assets.Get(Sound.FireEngines).CreateInstance();
        sound.IsLooped = true;
        var col = Color.Coral;
        col.A = 0;
        var emitter = new ParticleEmitter(Assets.Get(Sprite.Circle), 1, new Vector2(1500, -2000), 165 + 45, 360, 2,
            Engine.Random.NextSingle() - 0.5f, 200, Color.Gray, col, EmitterType.EmissionOverTime);
        actors.Add(mothership);
        //Ensure planets still orbit and render
        events.Add(new Event(0, 3, delegate (float time)
        {
            Engine.EntityManager.CurrentMission.PlanetUpdate();
        }));
        //Starts engine sound
        events.Add(new TriggerEvent(0, delegate (float time)
        {
            sound.Play();
        }));
        //Linearly moves the mothership toward the planet
        events.Add(new Event(0, 3, delegate (float time)
        {
            emitter.position = mothership.Position;
            emitter.Update();
            mothership.Position = new Vector2(1500, -2000) * (3 - time) / 3 + new Vector2(0, -425) * time / 3;
            Engine.Camera.Position = mothership.Position + new Vector2(Engine.Random.NextSingle() * 10 - 5, Engine.Random.NextSingle() * 10 - 5);
        }));
        //Fails player core, plays explosion sound, and stops engine sound
        events.Add(new TriggerEvent(3, delegate (float time)
        {
            sound.Pause();
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Death));
            //Note: Make sure the player can't farm free core hp with this
            Engine.SaveGame.Player.modules[ModuleType.Core].isFailed = true;
            Engine.ShakeScreen(1);
        }));
        return new Cutscene(events, actors, new PlayingGame());
    }
}