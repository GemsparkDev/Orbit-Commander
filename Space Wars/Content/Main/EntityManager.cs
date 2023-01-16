using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Space_Wars.Content.Main.Entities;

using System.Diagnostics;

namespace Space_Wars.Content.Main
{
    public static class EntityManager
    {
        private static bool isUpdating;
        private static List<Entity> entities = new List<Entity>();
        private static List<Entity> addedEntities = new List<Entity>();
        private static List<Enemy> enemies = new List<Enemy>();
        private static List<Projectile> projectiles = new List<Projectile>();
        public static Player player;
        private static EnemySpawner enemySpawner;

        public static void Add(Entity entity)
        {
            if (isUpdating == false)
            {
                //Checks the entity type, and adds it to the corresponding list for each type
                entities.Add(entity);

                if (entity.entityType is EntityType.Enemy)
                {
                    enemies.Add(entity as Enemy);
                }
                if(entity.entityType is EntityType.Projectile)
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

        public static void Initialize()
        {
            Player Player = new Player(Vector2.Zero, Vector2.Zero, 0f, 0f);
            Mothership Mothership = new Mothership(Vector2.Zero, Vector2.Zero, 0f, 0f);
            player = Player;
            player.mothership = Mothership;
            Add(Mothership);
            enemySpawner = new EnemySpawner(Player);
            Enemy.NewCarrier(new Vector2(100, 100), Player.Velocity, 0, 0);

        }
        public static void Update()
        {
            player.Update();
            Engine.screenPosition = new Vector2(Engine.screenSize.X / 2 - player.Position.X, Engine.screenSize.Y / 2 - player.Position.Y);
            enemySpawner.Update();

            isUpdating = true;

            //Updates all entities and moves deleted ones to a new list (prevents modifying a list while iterating over it)
            foreach (var entity in entities)
            {
                entity.Update();
            }

            if (projectiles.Count >= 100)
            {
                projectiles[0].IsExpired = true;
            }

            //Clears all expired entities from the entity lists
            entities = entities.Where(x => !x.IsExpired).ToList(); 
            projectiles = projectiles.Where(x => !x.IsExpired).ToList();
            enemies = enemies.Where(x => !x.IsExpired).ToList();

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
            if(entity == null || targetEntity == null)
            {
                return;
            }
            if (DistanceSqr(entity, targetEntity) <= MathF.Pow(entity.ColliderRadius + targetEntity.ColliderRadius, 2) && entity.IsFriendly != targetEntity.IsFriendly)
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
                if (distance < nearestDistance && entity.IsFriendly)
                {
                    nearestDistance = distance;
                    returnEnemy = targetEnemy;
                }
            }
            return returnEnemy;
        }
        public static float DistanceSqr(Entity entity1, Entity entity2)
        {
            if(entity1 == null || entity2 == null)
            {
                return 0;
            }
            Vector2 Target = new Vector2(entity2.Position.X - entity1.Position.X, entity2.Position.Y - entity1.Position.Y);
            return MathF.Pow(Target.X, 2) + MathF.Pow(Target.Y, 2);
        }
    }
}
