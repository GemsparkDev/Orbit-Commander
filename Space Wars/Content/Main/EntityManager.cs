using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;
using Space_Wars.Content.Main.UI_Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Space_Wars.Content.Main
{
    public static class EntityManager
    {
        private static bool isUpdating;
        private static List<Entity> entities = new();
        private static List<Entity> addedEntities = new();
        private static List<Enemy> enemies = new();
        private static List<Projectile> projectiles = new();
        public static TrainingSimulator trainingSimulator;
        public static Player player;
        private static Mothership mothership;
        private static Engine root;
        private static EnemySpawner enemySpawner;
        private static Random random = new();
        private static GravitationalSource planet;
        private static float currentKarma;
        public static void Add(Entity entity)
        {
            if (isUpdating == false)
            {
                //Checks the entity type, and adds it to the corresponding list for each type
                entities.Add(entity);

                if (entity is Enemy)
                {
                    enemies.Add(entity as Enemy);
                }
                if (entity is Projectile)
                {
                    projectiles.Add(entity as Projectile);
                }
            }
            else
            {
                addedEntities.Add(entity);
            }

            //Moves entities to the inactive list to prevent modifying a list while iterating
        }

        public static void Initialize(Engine _root)
        {
            entities = new();
            addedEntities = new();
            enemies = new();
            projectiles = new();
            planet = new(Vector2.Zero, Vector2.Zero, 15000, 8, true, Color.Cyan);
            planet.AddMoon(1000, 250, 1.5f, false);
            //planet = new(Vector2.Zero, Vector2.Zero, random.Next(2500, 15000), random.Next(6, 12), true, new Color(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)));
            //planet.AddMoon(random.Next(1600, 2000), random.Next(100, 500), random.Next(3, 6)/2, false);
            root = _root;
            player = new(Vector2.Zero, Vector2.Zero, 0, 0f);
            mothership = new Mothership(new Vector2(0, -planet.radius - Assets.DimsOf(Sprite.Mothership).Y/2), Vector2.Zero, 0f, 0f);
            player.mothership = mothership;
            Add(mothership);
            enemySpawner = new EnemySpawner(player);
            EventHandler.Initialize(player, mothership);
            EventHandler.UpdateInventoryUI();
        }
        public static void PlayerUpdate()
        {
            player.Update();
            planet.AttractObject(player);
            if(player.isDocked == false)
            {
                planet.CalculateTrajectory(player);
            }
            if (player.isExpired == true)
            {
                root.Startgame();
            }
            Engine.mousePositionOffset = new Vector2(Mouse.GetState().X - Engine.screenSize.X / 2, Mouse.GetState().Y - Engine.screenSize.Y / 2) / 15;
            Engine.camera.Position = player.position;
        }
        public static void IngameUpdate()
        {
            Engine.ingameTime.Duration += Engine.deltaSeconds;
            enemySpawner.Update();
        }
        public static void Update()
        {
            planet.Update();

            isUpdating = true;
            //Updates all entities and moves deleted ones to a new list (prevents modifying a list while iterating over it)
            foreach (var entity in entities)
            {
                entity.Update();
                planet.AttractObject(entity);
            }

            if (projectiles.Count >= 150)
            {
                for (int i = 0; i < projectiles.Count - 150; i++)
                {
                    projectiles[i].isExpired = true;
                }
            }

            //Clears all expired entities from the entity lists
            foreach (Enemy enemy in enemies)
            {
                if(enemy.isExpired == true)
                {
                    enemy.enemyRange.isEmitterExpired = true;
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
            //planet.Draw(_spriteBatch);
            player.Draw(_spriteBatch);
            foreach (var entity in entities)
            {
                //Draws all entities in the main list
                entity.Draw(_spriteBatch);
            }
            enemySpawner.Draw(_spriteBatch);
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
        public static Enemy NearestEnemy(Entity entity)
        {
            float distance;
            float nearestDistance = float.MaxValue;
            Enemy returnEnemy = null;
            foreach (Enemy targetEnemy in enemies)
            {
                if(targetEnemy == entity)
                {
                    continue;
                }
                distance = DistanceSqr(entity, targetEnemy);
                if (distance < nearestDistance && entity.isFriendly)
                {
                    nearestDistance = distance;
                    returnEnemy = targetEnemy;
                }
            }
            return returnEnemy;
        }
        public static float DistanceSqr(Entity _entity1, Entity _entity2)
        {
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
    public class TrainingSimulator
    {
        private List<IEnumerator<int>> trainingStep = new();
        private string instructionText = "";
        private Player player;
        private Item item;
        private Enemy enemy;
        private Engine root;
        private int currentStep = 0;
        private float cooldown;
        public TrainingSimulator(Player _player, Engine _root)
        {
            player = _player;
            root = _root;
            EventHandler.isTraining = true;
            UIManager.ToggleMenu(Containers.MainMenu);
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
            player.modules[4].health = 20;
        }
        public void Draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.DrawString(Assets.textFont, $"{instructionText}", Engine.camera.Position - new Vector2(instructionText.Length*5, 250), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.45f);
            if (enemy != null)
            {
                enemy.Draw(_spriteBatch);
            }
            if (item != null)
            {
                item.Draw(_spriteBatch);
            }
        }
        IEnumerable<int> UndockFromMothership()
        {
            instructionText = "You are currently docked at the mothership. Press space to undock from it. You can redock with it at any time by pressing space when you are close to the mothership.";
            while (player.isDocked == true)
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
                cooldown -= Engine.deltaSeconds;
                yield return 0;
            }
        }
        IEnumerable<int> FightEnemy()
        {
            enemy = Enemy.NewFighter(new Vector2(0, -600), Vector2.Zero, 0, 0);
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
                cooldown -= Engine.deltaSeconds;
                yield return 0;
            }
        }
        IEnumerable<int> CollectScrap()
        {
            item = ItemFactory.NewScrap(new Vector2(0, -600), Vector2.Zero, 0);
            EntityManager.Add(item);
            instructionText = "Enemies will occasionally drop scrap. You can collect it by holding right click when close to the scrap, then docking with the mothership. Be careful not to let it run into the planet.";
            while (player.mothership.inventory[0,0] == null)
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
            while (player.mothership.scrap == 0)
            {
                yield return 0;
            }
        }
        IEnumerable<int> RepairShip()
        {
            player.Collide(1);
            for(int i = 0; i < 4; i++)
            {
                player.modules[i].health = 0;
            }
            player.mothership.scrap = 50;
            instructionText = "You can heal by going into the garage on the second tab, dragging a module to the repair slot, and pressing repair. Repairing costs 3 scrap per repair.";
            while (player.modules[0].health + player.modules[1].health + player.modules[2].health + player.modules[3].health < 20)
            {
                yield return 0;
            }
        }
        IEnumerable<int> RepairMothership()
        {
            instructionText = "Your objective is to fix the mothership by going to the third tab and pressing repair with 5 refined scrap. You will need 25 scrap total to complete repairs.";
            while (player.mothership.currentlyCrafting == false)
            {
                yield return 0;
            }
        }
        IEnumerable<int> CompletedTraining()
        {
            cooldown = 7.5f;
            instructionText = "Good job completing the training! You will soon be sent back to the menu.";
            while (cooldown > 0)
            {
                cooldown -= Engine.deltaSeconds;
                yield return 0;
            }
            EventHandler.isTraining = false;
            EventHandler.QuitToMenu();
        }
    }
}
