using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Space_Wars.Content.Main.Entities;
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
        public static Player player;
        public static Mothership mothership;
        public static Engine root;
        private static EnemySpawner enemySpawner;
        private static Random random = new();
        private static float[] currentKarma = { 0, 0, 0 };

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
            root = _root;
            player = new(Vector2.Zero, Vector2.Zero, 0f, 0f);
            mothership = new Mothership(Vector2.Zero, Vector2.Zero, 0f, 0f);
            player.mothership = mothership;
            Add(mothership);
            enemySpawner = new EnemySpawner(player);
            EventHandler.Initialize(player, mothership);
        }
        public static void Update()
        {
            Engine.mousePositionOffset = new Vector2(Mouse.GetState().X - Engine.screenSize.X/2, Mouse.GetState().Y - Engine.screenSize.Y/2) / 20;
            player.Update();
            Engine.screenPosition = new Vector2(Engine.screenSize.X / 2 - player.position.X, Engine.screenSize.Y / 2 - player.position.Y);
            enemySpawner.Update();

            isUpdating = true;

            //Updates all entities and moves deleted ones to a new list (prevents modifying a list while iterating over it)
            foreach (var entity in entities)
            {
                entity.Update();
            }

            if (projectiles.Count >= 100)
            {
                projectiles[0].isExpired = true;
            }

            //Clears all expired entities from the entity lists
            entities = entities.Where(x => !x.isExpired).ToList();
            projectiles = projectiles.Where(x => !x.isExpired).ToList();
            enemies = enemies.Where(x => !x.isExpired).ToList();

            if (player.isExpired == true)
            {
                entities = new List<Entity>();
                addedEntities = new List<Entity>();
                enemies = new List<Enemy>();
                projectiles = new List<Projectile>();
                player = new Player(Vector2.Zero, Vector2.Zero, 0f, 0f);
                entities.Add(mothership = new Mothership(Vector2.Zero, Vector2.Zero, 0f, 0f));
                player.mothership = mothership;
                enemySpawner.PlayerRespawn(player);
                EventHandler.Initialize(player, mothership);
                EventHandler.PairPlayerUIManager();
                EventHandler.UpdateInventoryUI();
            }

            isUpdating = false;

            //Moves all newly created entities to the main list
            foreach (var entity in addedEntities)
            {
                Add(entity);
            }
            addedEntities.Clear();
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            player.Draw(spriteBatch);
            foreach (var entity in entities)
            {
                //Draws all entities in the main list
                entity.Draw(spriteBatch);
            }
            enemySpawner.Draw(spriteBatch);
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
            int karmaType;
            if (_rarity <= 1) { return true; }
            else if (5 >= _rarity && _rarity > 1) { karmaType = 0; }
            else if (50 >= _rarity && _rarity > 5) { karmaType = 1; }
            else { karmaType = 2; }
            float randomNum = (float)random.NextDouble();
            float karmaBonus = (_rarity-1) / (_rarity + _rarity * MathF.Exp(-10 * currentKarma[karmaType] + 12.5f));
            if (randomNum < (1/_rarity) + karmaBonus)
            {
                currentKarma[karmaType] = 0;
                return true;
            }
            currentKarma[karmaType] += 1 / _rarity;
            return false;
        }
    }
}
