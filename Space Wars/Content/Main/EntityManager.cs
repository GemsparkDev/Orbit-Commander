using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Space_Wars.Content.Main;

public static class EntityManager
{
    private static bool isUpdating = false;
    private static List<Entity> entities = new();
    private static List<Entity> addedEntities = new();
    private static List<Entity> enemies = new();
    private static List<Projectile> projectiles = new();
    public static TrainingSimulator TrainingSimulator { get; set; }
    public static Player Player { get; private set; }
    private static Random random = new();
    private static float currentKarma;
    //Maximum distance for any detection when sensing = stealth
    public static float StealthRange { get; private set; } = 500;
    //Threshold of detection for enemies
    public static float StealthThreshold { get; private set; } = 0.75f;
    public readonly static Pickup[] globalInventory = new Pickup[5];
    public static int Scrap { get; private set; }
    public readonly static List<Mission> missions = new()
    {
        new(new(Vector2.Zero, Vector2.Zero, 10000, 8, true, Color.Cyan),
        new List<GravitationalSource>(){ new(new Vector2(1000, 0), Vector2.Zero, 250, 1.5f, false, Color.Cyan), },
        new List<(Func<Vector2, Vector2, float, Entity>, Vector2, Vector2, float, Condition[])>(){ (Enemy.NewMothership, new Vector2(0, -8*50 - Assets.DimsOf(Sprite.Mothership).Y / 2), Vector2.Zero, 0f, new Condition[2] { Condition.Protect, Condition.CustomIncomplete })},
        "Crash Landing",
        "A simple system with a large planet and one closely orbiting moon. Drone activity detected, but minimal.", 1, 0),

        new(new(Vector2.Zero, Vector2.Zero, 3500, 4, true, Color.Cyan, true),
        new List<GravitationalSource>(),
        new List<(Func<Vector2, Vector2, float, Entity>, Vector2, Vector2, float, Condition[])>(){ 
            (Enemy.NewTurret, new Vector2(0, -200 - Assets.DimsOf(Sprite.TurretBase).Y / 2), Vector2.Zero, 0, new Condition[1] { Condition.Protect }),
            (Enemy.NewOrbiter, new Vector2(400, 0), new Vector2(0, GravitationalSource.GetOrbitalVelocity(new Vector2(400, 0), Vector2.Zero, 3500)), 0, new Condition[1] { Condition.Protect })},
        "Sentry Defense",
        "A small outpost is located orbiting this rogue planet. Defend it.", 0.75f, 40),

        new(new(Vector2.Zero, Vector2.Zero, 25000, 7f, true, Color.Cyan),
        new List<GravitationalSource>(){ new(new Vector2(800, 0), Vector2.Zero, 150, 0.5f, false, Color.Cyan), },
        new List<(Func<Vector2, Vector2, float, Entity>, Vector2, Vector2, float, Condition[])>(){ (Enemy.NewMiner, new Vector2(0, -7*50 - Assets.DimsOf(Sprite.Miner).Y / 2), Vector2.Zero, 0, new Condition[1] { Condition.Protect }) },
        "Extraction",
        "This deceptively dense planet is rich with materials that our deployed miner will extract.", 1, 20),

        new(new(Vector2.Zero, Vector2.Zero, 3000, 3, true, Color.Cyan),
        new List<GravitationalSource>(){ 
            new(new Vector2(500, 0), Vector2.Zero, 240, 1f, false, Color.Cyan),
            new(new Vector2(800, 0), new Vector2(0, -GravitationalSource.GetOrbitalVelocity(new Vector2(800, 0), Vector2.Zero, 3000) - 0.06f), 120, 0.6f, false, Color.Yellow), },
        new List<(Func<Vector2, Vector2, float, Entity>, Vector2, Vector2, float, Condition[])>(){ (Enemy.NewExcursionBoss, new Vector2(0, -6*50), Vector2.Zero, 0, new Condition[1] { Condition.Kill }) },
        "Showdown",
        "Defeat the advanced drone prototype, Excursion. Be warned: It may call for reinforcements.", 1.1f, 0, 1),

        new(new(Vector2.Zero, Vector2.Zero, 30000, 10f, true, Color.HotPink, true),

        new List<GravitationalSource>(){ },
        new List<(Func<Vector2, Vector2, float, Entity>, Vector2, Vector2, float, Condition[])>(){ (Enemy.NewPickupDrone, new Vector2(0, -8*50 - Assets.DimsOf(Sprite.PickupDrone).Y / 2), Vector2.Zero, 0f, new Condition[2] { Condition.Protect, Condition.CustomIncomplete })},
        "cool planet",
        "Super earth", 2, 5),
    };

    /*
     * (Enemy.NewOrbiter, 
            new Vector2(400, 0), 
            GravitationalSource.GetOrbitalVelocity(new Vector2(400, 0), Vector2.Zero, 3500), 
            0, 
            new Condition[0])
    */
    public static Mission CurrentMission { get { return currentMission ?? missions[missionCount]; } }
    private static int missionCount = 0;
    private static Mission currentMission;
    public static void Add(Entity entity)
    {
        if (isUpdating == false)
        {
            //Checks the entity type, and adds it to the corresponding list for each type
            entities.Add(entity);
            if (entity is Projectile)
            {
                projectiles.Add(entity as Projectile);
            }
            if(entity is Enemy || entity is Player)
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
    public static Player Initialize()
    {
        currentMission = missions[missionCount].Clone();
        entities = new();
        addedEntities = new();
        enemies = new();
        projectiles = new();
        Player = new(new Vector2(0, -CurrentMission.Planet.radius + 1000), new Vector2(CurrentMission.Planet.GetOrbitalVelocity(new Vector2(0, -CurrentMission.Planet.radius + 1000)),0), 0, 0f);
        CurrentMission.Initialize();
        return Player;
    }
    public static void PlayerUpdate()
    {
        Player.Update();
        GravitationalSource planet = CurrentMission.Planet;
        planet.AttractObject(Player);
        if(Player.dockedEntity == null)
        {
            planet.CalculateTrajectory(Player);
        }
        if (Player.isExpired == true)
        {
            Engine.Startgame();
        }
        Engine.mousePositionOffset = new Vector2(Mouse.GetState().X - Engine.ScreenSize.X / 2, Mouse.GetState().Y - Engine.ScreenSize.Y / 2) / 10 
            + Engine.ScreenShakeFactor * Engine.ScreenShakeFactor * new Vector2((float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f) * 50;
        Engine.Camera.Rotation = Engine.ScreenShakeFactor * Engine.ScreenShakeFactor * ((float)random.NextDouble() - 0.5f) * 0.15f;
        //If the player is further from the camera, put more weight on the player
        //Tanh prevents frac from going above 1
        float frac = MathF.Tanh(Vector2.Distance(Player.position, Engine.Camera.Position) / 750);
        Engine.Camera.Position = Player.position * frac + Engine.Camera.Position * (1-frac);
    }
    public static void IngameUpdate()
    {
        Engine.ingameTime.Duration += Engine.DeltaSeconds;
        CurrentMission.Update();
    }
    public static void Update()
    {
        isUpdating = true;
        //Updates all entities and moves deleted ones to a new list (prevents modifying a list while iterating over it)
        foreach (var entity in entities)
        {
            entity.Update();
            CurrentMission.Planet.AttractObject(entity);
        }

        if (projectiles.Count >= 150)
        {
            for (int i = 0; i < projectiles.Count - 150; i++)
            {
                projectiles[i].isExpired = true;
            }
        }

        //Clears all expired entities from the entity lists
        foreach (Entity enemy in enemies)
        {
            if(enemy.isExpired == true && enemy is Enemy)
            {
                (enemy as Enemy).enemyRange.isEmitterExpired = true;
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
    public static void Draw(SpriteBatch _spriteBatch)
    {
        //CurrentMission.Planet.Draw(_spriteBatch);
        Player.Draw(_spriteBatch);
        foreach (var entity in entities)
        {
            //Draws all entities in the main list
            entity.Draw(_spriteBatch);
        }
    }
    public static void MarkMissionComplete()
    {
        Scrap += CurrentMission.MissionScrap;
        missions[missionCount].Completed = true;
        CurrentMission.MarkComplete();
    }
    public static void NextMission()
    {
        missionCount = Math.Clamp(missionCount + 1, 0, missions.Count - 1);
        currentMission = missions[missionCount].Clone();
        EventHandler.UpdateMissionText();
    }
    public static void PrevMission()
    {
        missionCount = Math.Clamp(missionCount - 1, 0, missions.Count - 1);
        currentMission = missions[missionCount].Clone();
        EventHandler.UpdateMissionText();
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
    public static DockableComponent NearestDockableEntity(Entity _entity)
    {
        float nearestDistance = float.MaxValue;
        DockableComponent returnEntity = null;
        foreach (Entity entity in entities)
        {
            if(entity.isFriendly != _entity.isFriendly)
            {
                continue;
            }
            DockableComponent component = entity.Components.GetComponent<DockableComponent>(ComponentType.DockableComponent);
            if (component != null)
            {
                float distanceSqr = DistanceSqr(entity, _entity);
                if (distanceSqr < nearestDistance)
                {
                    nearestDistance = distanceSqr;
                    returnEntity = component;
                }
            }
        }
        return returnEntity;
    }
    public static Entity NearestEnemy(Entity entity)
    {
        float nearestDistance = float.MaxValue;
        Entity returnEnemy = null;
        foreach(Entity targetEnemy in enemies)
        {
            if(targetEnemy.isFriendly == entity.isFriendly)
            {
                continue;
            }
            if (targetEnemy.StealthAbility > entity.SensingAbility)
            {
                continue;
            }
            float distance = DistanceSqr(entity, targetEnemy);
            if (distance < nearestDistance && (targetEnemy.StealthAbility < entity.SensingAbility || (targetEnemy.StealthAbility == entity.SensingAbility && distance < StealthRange * StealthThreshold)))
            {
                nearestDistance = distance;
                returnEnemy = targetEnemy;
            }
        }
        if (entity.isFriendly == false)
        {
            float distance = DistanceSqr(entity, Player);
            if (distance < nearestDistance)
            {
                returnEnemy = Player;
            }
        }
        return returnEnemy;
    }
    public static Entity NearestAlly(Entity entity)
    {
        float nearestDistance = float.MaxValue;
        Entity returnEnemy = null;
        foreach (Entity targetEnemy in enemies)
        {
            if (targetEnemy.isFriendly != entity.isFriendly)
            {
                continue;
            }
            if(targetEnemy == entity)
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
        if (entity.isFriendly == true)
        {
            float distance = DistanceSqr(entity, Player);
            if (distance < nearestDistance)
            {
                returnEnemy = Player;
            }
        }
        return returnEnemy;
    }
    public static Entity NearestItem(Entity entity)
    {
        float nearestDistance = float.MaxValue;
        Entity returnItem = null;
        foreach (Entity targetEntity in entities)
        {
            if (targetEntity is not Pickup)
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
    public static Entity NearestProjectile(Entity _entity)
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
        if(_entity1 == null || _entity2 == null)
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
        float randomNum = (float)random.NextDouble();
        float karmaBonus = (_rarity-1) / (_rarity + _rarity * MathF.Exp(-10 * currentKarma + 12.5f));
        if (randomNum < (1/_rarity) + karmaBonus)
        {
            currentKarma = 0;
            return true;
        }
        currentKarma += (1 / _rarity);
        return false;
    }
    public static Vector2 GetNormalizedAcceleration(Vector2 _position)
    {
        GravitationalSource planet = CurrentMission.Planet;
        Vector2 normalVector = Vector2.Normalize(_position - planet.position);
        Vector2 acceleration = normalVector * (planet.radius * planet.radius / DistanceSqr(planet.position, _position));
        foreach (GravitationalSource moon in planet.moons)
        {
            normalVector = Vector2.Normalize(_position - moon.position);
            acceleration += normalVector * (moon.radius * moon.radius / DistanceSqr(moon.position, _position));
            //Note: only goes one layer deep; Potential fix: eliminate moons, make all planets one list
        }
        return acceleration;
    }
}


//Currently bugged
public class TrainingSimulator
{
    private List<IEnumerator<int>> trainingStep = new();
    private string instructionText = "";
    private Player player;
    private Pickup item;
    private Enemy enemy;
    private int currentStep = 0;
    private float cooldown;
    public TrainingSimulator(Player _player)
    {
        player = _player;
        EventHandler.isTraining = true;
        Engine.UIManager.ToggleMenu((int)Containers.MainMenu);
        AddBehaviour(UndockFromMothership());
        AddBehaviour(MoveAround());
        AddBehaviour(FightEnemy());
        AddBehaviour(TeachSkill());
        AddBehaviour(TeachEnergy());
        AddBehaviour(CollectScrap());
        AddBehaviour(SmeltScrap());
        AddBehaviour(RepairShip());
        AddBehaviour(RepairMothership());
        AddBehaviour(CompletedTraining());
    }
    private void AddBehaviour(IEnumerable<int> behaviour)
    {
        trainingStep.Add(behaviour.GetEnumerator());
    }

    private void ApplyBehaviours()
    {
        if (!trainingStep[currentStep].MoveNext())
        {
            currentStep += 1;
        }
    }
    public void Update()
    {
        ApplyBehaviours();
        if(player != null)
        {
            KeepPlayerAlive();
        }
    }
    private void KeepPlayerAlive()
    {
        player.modules[ModuleType.Core].Health = 20;
    }
    public void Draw(SpriteBatch _spriteBatch)
    {
        _spriteBatch.DrawString(Assets.TextFont, $"{instructionText}", Engine.Camera.Position - new Vector2(instructionText.Length*5, 250), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.45f);
        enemy?.Draw(_spriteBatch);
        item?.Draw(_spriteBatch);
    }
    IEnumerable<int> UndockFromMothership()
    {
        instructionText = "You are currently docked at the mothership. Press space to undock from it. You can redock with it at any time by pressing space when you are close to the mothership.";
        while (player.dockedEntity != null)
        {
            yield return 0;
        }
    }
    IEnumerable<int> MoveAround()
    {
        cooldown = 15;
        instructionText = "Use WASD to move around. Notice that your velocity is preserved, and can be changed by any nearby planets. Your path of motion is represented by the faint dotted line.";
        while (cooldown > 0)
        {
            cooldown -= Engine.DeltaSeconds;
            yield return 0;
        }
    }
    IEnumerable<int> FightEnemy()
    {
        enemy = Enemy.NewFighter(new Vector2(0, -600), Vector2.Zero, 0);
        EntityManager.Add(enemy);
        instructionText = "An enemy has spawned near the mothership. You can attack it with left click. Destroy the enemy to proceed.";
        while (enemy.isExpired == false)
        {
            yield return 0;
        }
        enemy = null;
    }
    IEnumerable<int> TeachSkill()
    {  
        instructionText = "You are equipped with a dash that teleports you forward. You can activate it by pressing Q when the cyan bar is full.";
        while (Keyboard.GetState().IsKeyDown(Keys.Q) == false)
        {
            yield return 0;
        }
    }
    IEnumerable<int> TeachEnergy()
    {
        cooldown = 15;
        instructionText = "Additionally, your craft requires energy to function, represented by the yellow bar. It is used by your modules and regenerates quickly when usage stops.";
        while (cooldown > 0)
        {
            cooldown -= Engine.DeltaSeconds;
            yield return 0;
        }
    }
    IEnumerable<int> CollectScrap()
    {
        item = ItemFactory.NewScrap(new Vector2(0, -600), Vector2.Zero, 0);
        EntityManager.Add(item);
        instructionText = "Enemies will occasionally drop scrap. You can collect it by holding right click when close to the scrap, then docking with the mothership. Be careful not to let it run into the planet.";
        while (player.dockedEntity.Inventory[0,0] == null)
        {
            if(item.isExpired == true)
            {
                item = ItemFactory.NewScrap(new Vector2(0, -600), Vector2.Zero, 0);
                EntityManager.Add(item);
            }
            yield return 0;
        }
        item = null;
    }
    IEnumerable<int> SmeltScrap()
    {
        instructionText = "You can refine the scrap by pressing I while docked, then dragging the scrap to the smelting slot on the first tab.";
        //while (player.dockableEntity.scrap == 0)
        //{
        //    yield return 0;
        //}
        yield return 0;
    }
    IEnumerable<int> RepairShip()
    {
        player.Collide(1);
        for(int i = 0; i < 4; i++)
        {
            player.modules.ElementAt(i).Value.Health = 0;
        }
        //player.dockableEntity.scrap = 50;
        instructionText = "You can heal by going into the garage on the second tab, dragging a module to the repair slot, and pressing repair. Repairing costs 3 scrap per repair.";
        while (player.modules[ModuleType.Hull].Health + player.modules[ModuleType.Guns].Health + player.modules[ModuleType.Engines].Health + player.modules[ModuleType.Sensors].Health < 20)
        {
            yield return 0;
        }
    }
    IEnumerable<int> RepairMothership()
    {
        instructionText = "Your objective is to fix the mothership by going to the third tab and pressing repair with 5 refined scrap. You will need 25 scrap total to complete repairs.";
        //while (player.dockableEntity.currentlyCrafting == false)
        //{
        //    yield return 0;
        //}
        yield return 0;
    }
    IEnumerable<int> CompletedTraining()
    {
        cooldown = 7.5f;
        instructionText = "Good job completing the training! You will soon be sent back to the menu.";
        while (cooldown > 0)
        {
            cooldown -= Engine.DeltaSeconds;
            yield return 0;
        }
        EventHandler.isTraining = false;
        EventHandler.QuitToMenu();
    }
}
