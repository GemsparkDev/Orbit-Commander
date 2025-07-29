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

namespace Space_Wars.Content.Main;

public class EntityManager
{
    private bool isUpdating = false;
    private List<Entity> entities = [];
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
        new([ new(Vector2.Zero, Vector2.Zero, 10000, 8, true, Color.Cyan),
        new(new Vector2(1000, 0), GravitationalSource.GetOrbitalVelocity(new Vector2(1000, 0), Vector2.Zero, 10000), 250, 1.5f, false, Color.Cyan) ],
        [ (new EntityConstructor(Enemy.NewMothership, new Vector2(0, -8*50 - Assets.DimsOf(Sprite.Mothership).Y / 2), Vector2.Zero, 0f), [ Condition.Protect, Condition.CustomIncomplete ]),
        (new PickupConstructor(ItemFactory.NewScrap, new Vector2(0, -8*50), new Vector2(10, -10), 0.07f),[]),
        (new PickupConstructor(ItemFactory.NewScrap, new Vector2(0, -8*50), new Vector2(-8, -4), -0.03f),[])],
        "Crash Landing",
        "A simple system with a large planet and one closely orbiting moon. Drone activity detected, but minimal.",
        1, new Vector2(0, -8*50 - Assets.DimsOf(Sprite.Mothership).Y / 2), 0, 0, IntroCutscene) { playerProgression = 0, playerDocked = true, tip = "WASD to move, Space to dock and undock.\nRmb to collect scrap, Lmb to shoot." },

        new( [ new(Vector2.Zero, Vector2.Zero, 3500, 4, true, Color.Cyan, true) ],
        [
            (new EntityConstructor(Enemy.NewTurret, new Vector2(0, -200 - Assets.DimsOf(Sprite.TurretBase).Y / 2), Vector2.Zero, 0), [ Condition.Protect ]),
            (new EntityConstructor(Enemy.NewOrbiter, new Vector2(400, 0), GravitationalSource.GetOrbitalVelocity(new Vector2(400, 0), Vector2.Zero, 3500), 0), [ Condition.Protect ])],
        "Sentry Defense",
        "A small outpost is located orbiting this rogue planet. Defend it.", 0.75f, new Vector2(0, 1), 40) { playerProgression = 1, isAggressive = true, tip = "Open the side panel to restart failed modules. \nPress F to open the fuse menu and switch around your setup.\nFuses power your modules, keep them safe." },

        new( [ new(Vector2.Zero, Vector2.Zero, 15000, 6f, true, Color.Cyan),
        new(new Vector2(0, 800), GravitationalSource.GetOrbitalVelocity(new Vector2(0, 800), Vector2.Zero, 15000) * 0.85f, 1000, 1f, false, Color.Cyan), ],
        [(new EntityConstructor(Enemy.NewLargeMiner, new Vector2(0, -6*50 - Assets.Get(Sprite.LargeMiner).Height/2), Vector2.Zero, 0), [ Condition.Kill ])],
        "Assault prequel",
        "Defeat the mega miner", 0.75f, new Vector2(0, 1), 0, 0, null, true) { playerProgression = 2, isAggressive = true, tip = "Press Q to use your special ability.\nCtrl to toggle aim assist." },

        new([ new(Vector2.Zero, Vector2.Zero, 5000, 3, true, Color.Cyan),
            new(new Vector2(400, 0), GravitationalSource.GetOrbitalVelocity(new Vector2(400, 0), Vector2.Zero, 5000), 240, 1f, false, Color.Cyan),
            new(new Vector2(-600, 0), -GravitationalSource.GetOrbitalVelocity(new Vector2(-600, 0), Vector2.Zero, 5000) * 1.2f, 120, 0.6f, false, Color.Yellow), ],
        [(new EntityConstructor(Enemy.NewExcursionBoss, new Vector2(0, -6*50), Vector2.Zero, 0), [ Condition.Kill ])],
        "Showdown",
        "Defeat the advanced drone prototype, Excursion. Be warned: It may call for reinforcements.", 1.1f, new Vector2(0, 1), 0, 0, null, true) { playerProgression = 2 },

        new([], [(new EntityConstructor(Enemy.NewWarpGate, Vector2.Zero, Vector2.Zero, 0), [ Condition.CustomIncomplete ])],
        "Warp Gate", "Warp to the next mission once you are done here", -1, new Vector2(0, 500)) { music = false },

        //Note: The player construct menu and the Quantum Resonator both use the name of this mission for their special behavior. When changing, make sure their name is updated as well.
        new([new(Vector2.Zero, Vector2.Zero, 50000, 12, true, new Color(255, 219, 0), true) ],
        [],
        "???",
        "", -1, new Vector2(-2000, -2000), 0, 1, null, true) { playerDocked = true, music = false},

        new([new(Vector2.Zero, Vector2.Zero, 30000, 10f, true, Color.HotPink, true) ],
        [(new AdvancedConstructor(Enemy.NewCommunicator, new Vector2(MathF.Sin(1.02f), -MathF.Cos(1.02f)), Vector2.Zero, 1.02f, true), [Condition.Protect]),
            (new AdvancedConstructor(Enemy.NewCommunicator, new Vector2(MathF.Sin(2.7f), -MathF.Cos(2.7f)), Vector2.Zero, 2.7f, true), [Condition.Protect]),
            (new AdvancedConstructor(Enemy.NewCommunicator, new Vector2(MathF.Sin(5.33f), -MathF.Cos(5.33f)), Vector2.Zero, 5.33f, true), [Condition.Protect]),
        ],
        "cool planet",
        "Super earth", 0, new Vector2(0, 1), 0, 1, null, true),

        new([new(Vector2.Zero, Vector2.Zero, 20000, 9f, true, Color.Cyan, false),
        new(new Vector2(0, 1800), GravitationalSource.GetOrbitalVelocity(new Vector2(0, 1800), Vector2.Zero, 20000), 1500, 2f, false, Color.Cyan) ],
        [
            (new AdvancedConstructor(Enemy.NewTurret, new Vector2(MathF.Sin(5.5f), -MathF.Cos(5.5f)) * 9 * 50, Vector2.Zero, 5.5f, false), [ Condition.Kill ]),
            (new AdvancedConstructor(Enemy.NewTurret, new Vector2(MathF.Sin(3.2f), -MathF.Cos(3.2f)) * 9 * 50, Vector2.Zero, 3.2f, false), [ Condition.Kill ]),
            (new AdvancedConstructor(Enemy.NewTurret, new Vector2(MathF.Sin(2.6f), -MathF.Cos(2.6f)) * 9 * 50, Vector2.Zero, 2.6f, false), [ Condition.Kill ]),
            (new AdvancedConstructor(Enemy.NewTurret, new Vector2(MathF.Sin(1.1f), -MathF.Cos(1.1f)) * 9 * 50, Vector2.Zero, 1.1f, false), [ Condition.Kill ]),
            (new AdvancedConstructor(Enemy.NewMiner, new Vector2(MathF.Sin(3), -MathF.Cos(3)) * 9 * 50, Vector2.Zero, 3, false), [ Condition.Kill ]),
            (new AdvancedConstructor(Enemy.NewMiner, new Vector2(MathF.Sin(5.2f), -MathF.Cos(5.2f)) * 9 * 50, Vector2.Zero, 5.2f, false), [ Condition.Kill ]),
            (new EntityConstructor(Enemy.NewOrbiter, new Vector2(0, 1650), GravitationalSource.GetOrbitalVelocity(new Vector2(0, 1650), new Vector2(0, 1800), 1500)
                + GravitationalSource.GetOrbitalVelocity(new Vector2(0, 1800), Vector2.Zero, 20000), 0), [ Condition.Protect ])],
        "Assault",
        "You have been placed in high orbit. Destroy the enemy base on the surface planet, and all reinforcements that arrive.", 0.75f, new Vector2(0, 1650), 2, 1, null, false) { playerDocked = true, isAggressive = true, tip = "Press C to open the construct menu.\nEach construct requires one scrap to craft." },

        new( [ new(Vector2.Zero, Vector2.Zero, 25000, 7f, true, Color.Cyan), new(new Vector2(800, 0), GravitationalSource.GetOrbitalVelocity(new Vector2(800, 0), Vector2.Zero, 25000), 150, 0.5f, false, Color.Cyan), ],
        [(new EntityConstructor(Enemy.NewMiner, new Vector2(0, -7*50), Vector2.Zero, 0), [ Condition.Protect ])],
        "Extraction",
        "This deceptively dense planet is rich with materials that our deployed miner will extract.", 1, new Vector2(0, 1), 20),

        new([ new(Vector2.Zero, Vector2.Zero, 5000, 4.5f, true, Color.Cyan),
        new(new Vector2(600, 0), GravitationalSource.GetOrbitalVelocity(new Vector2(600, 0), Vector2.Zero, 5000), 240, 1f, false, Color.Cyan),
        new(new Vector2(-600, 0), GravitationalSource.GetOrbitalVelocity(new Vector2(-600, 0), Vector2.Zero, 5000), 240, 1f, false, Color.Cyan), ],
        [(new AdvancedConstructor(Enemy.NewExodus, new Vector2(0, -6*50), Vector2.Zero, 0, false), [ Condition.Kill ])],
        "Showdown Pt. 2",
        "Defeat the advanced drone prototype, Exodus. Be warned: It may call for reinforcements.", 1.1f, new Vector2(0, 1), 0, 1, null, true),

        new([new(Vector2.Zero, Vector2.Zero, 150, 3, true, Color.OldLace)],
        [(new EntityConstructor(Enemy.NewWarpGate, new Vector2(0, 450), -GravitationalSource.GetOrbitalVelocity(new Vector2(0, 450), Vector2.Zero, 150), 0), [ Condition.CustomIncomplete ])],
        "Warp Gate", "Warp to the next mission once you are done here", -1, new Vector2(0, 500)) { music = false, tip = "Press left shift to return to the previous system. Press right shift to enter the next system." },

        new([ new(Vector2.Zero, Vector2.Zero, 4000, 4.5f, true, new Color(0.03f, 0.05f, 0.08f)),
        new(new Vector2(600, 0), GravitationalSource.GetOrbitalVelocity(new Vector2(600, 0), Vector2.Zero, 4000) * 1.05f, 500, 1.5f, false, new Color(0.03f, 0.05f, 0.08f)), ],
        [(new EntityConstructor(Enemy.NewVeilBoss, new Vector2(0, -6*50), Vector2.Zero, 0), [ Condition.Kill ])],
        "Showdown Pt. 3",
        "Defeat the advanced drone prototype, Veil. Be warned: It may call for reinforcements.", 1.1f, new Vector2(0, 1), 0, 2, null, true),

        new([new(new Vector2(320, 0), new Vector2(0, 1f), 10000, 7, false, Color.Cyan),
        new(new Vector2(-800, 0), new Vector2(0, -2.5f), 4000, 3.5f, false, Color.Cyan)],
        [], "Binary system", "Demo Binary System", -1, new Vector2(0, 400), 0, 0, null, true),

        new([ new(Vector2.Zero, Vector2.Zero, 20000, 9, true, Color.OrangeRed, true),
        new(new Vector2(1200, 0), GravitationalSource.GetOrbitalVelocity(new Vector2(1200, 0), Vector2.Zero, 20000), 750, 2f, false, Color.Red) ],
        [],
        "Last Stand",
        "Survive",
        0.25f, new Vector2(0, -8*50), 1000, 4) { isAggressive = true, playerProgression = 4, tip = "You can now construct the makeshift mothership in the construct menu.\n Requires 3 scrap." },
    ];
    public Mission GetMission(int _index)
    {
        return missions[_index].Clone();
    }
    public int Missions()
    {
        return missions.Count;
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
        + Engine.ScreenShakeFactor * Engine.ScreenShakeFactor * new Vector2(Engine.Random.NextSingle() - 0.5f, Engine.Random.NextSingle() - 0.5f) * 50;
        Engine.Camera.Rotation = Engine.ScreenShakeFactor * Engine.ScreenShakeFactor * (Engine.Random.NextSingle() - 0.5f) * 0.15f;
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
            EventHandler.MissionSelectTrigger();
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
        foreach (var projectile in projectiles)
        {
            if (projectile.ExtraUpdates > 1)
            {
                for (int i = 0; i < projectile.ExtraUpdates - 1 && !projectile.isExpired; i++)
                {
                    projectile.Update();
                }
            }
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
    public void Explode(int _damage, float _radius, Vector2 _position)
    {
        float dist;
        foreach (var entity in entities)
        {
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
            if (pickup is Pickup && Engine.Random.NextSingle() < 0.6f)
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
            if (targetEnemy.StealthAbility > entity.SensingAbility || targetEnemy.isFriendly == entity.isFriendly)
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
        if (Engine.Random.NextSingle() < (1 / _rarity) + karmaBonus)
        {
            currentKarma = 0;
            return true;
        }
        currentKarma += (1 / _rarity);
        return false;
    }
    public List<Entity> Hitscan(Vector2 _pos, Vector2 _dir, float _maxLength, bool _getAll, out Vector2 _end)
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
            Vector2 relativePos = entity.position - _pos;
            float closestLength = (relativePos.X * dir.X + relativePos.Y * dir.Y);
            float closestDistance = Vector2.Distance((dir * closestLength + _pos), entity.position);
            if (closestLength > 0 && closestLength < maxDist && closestDistance < entity.ColliderRadius)
            {
                if (_getAll)
                {
                    list.Add(entity);
                }
                else
                {
                    float discriminant = MathF.Sqrt(entity.ColliderRadius * entity.ColliderRadius - closestDistance * closestDistance);
                    if (dist > closestLength - discriminant) 
                    {
                        dist = closestLength - discriminant;
                        nearestEnemy = entity;
                    }
                }
            }
        }
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
            Engine.SaveGame.CurrentMission.PlanetUpdate();
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
            Engine.ShakeScreen(1);
        }));
        return new Cutscene(events, actors, new PlayingGame());
    }
}