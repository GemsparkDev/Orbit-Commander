using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace Space_Wars.Content.Main
{
    class EnemySpawner
    {
        private int wave;
        private List<Enemy> enemiesAlive = new();
        private float waveTimer;
        private float difficulty;
        private Vector2 spawnLocation;
        private Player Player;
        private Random random;
        private int randomValue;
        private int enemiesSpawned = 0;
        public EnemySpawner(Player player)
        {
            wave = 0;
            waveTimer = 5;
            Player = player;
            random = new Random();
        }

        public void PlayerRespawn(Player player)
        {
            wave = 0;
            waveTimer = 5;
            Player = player;
        }

        public Vector2 NewSpawnLocation()
        {
            //Creates a position vector defined by the angle from the player in radians and the distance from the edge of the screen
            float angle = (float)(random.NextDouble() - 0.5) * MathF.Tau;
            float distanceMultiplier = 1 + (float)(random.NextDouble() - 0.5) / 5;
            float distance = (Engine.screenSize.X + Engine.screenSize.Y) / 4 * distanceMultiplier;
            Vector2 spawnLocation = Engine.ToUnitVector(angle) * distance;
            return spawnLocation + EntityManager.player.position;

        }
        public void Update()
        {

            if (waveTimer <= 0)
            {
                wave++;
                if (wave < 50)
                {
                    difficulty = (MathF.Pow(wave, 2) / 200) + 1;
                }
                else
                {
                    difficulty = wave / 2 - 11.5f;
                }
                randomValue = random.Next((int)(3 * difficulty), (int)(5 * difficulty));
                SpawnWaveBatch(randomValue);
                waveTimer = 5f * enemiesSpawned;
                enemiesSpawned = 0;
            }

            waveTimer -= Engine.deltaSeconds;
        }

        private void SpawnWaveBatch(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                spawnLocation = NewSpawnLocation();
                Enemy.NewFighter(spawnLocation, Player.velocity, 0, 0);
                enemiesSpawned++;
                if (wave > 5)
                {
                    randomValue = random.Next(0, 3);
                    if (randomValue >= 2)
                    {
                        spawnLocation = NewSpawnLocation();
                        Enemy.NewSniper(spawnLocation, Player.velocity, 0, 0);
                        enemiesSpawned++;
                    }

                }
                if (wave > 12)
                {
                    randomValue = random.Next(0, 8);
                    if (randomValue >= 7)
                    {
                        spawnLocation = NewSpawnLocation();
                        Enemy.NewCarrier(spawnLocation, Player.velocity, 0, 0);
                        enemiesSpawned++;
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Engine.line, new Vector2(Engine.screenSize.X / 50, Engine.screenSize.Y / 50), new Rectangle(0, 0, (int)Engine.screenSize.X - (int)Engine.screenSize.X / 25, 1),
                Color.White, 0, Vector2.Zero, new Vector2(waveTimer / 60, 1), SpriteEffects.None, 0);

        }
    }
}
