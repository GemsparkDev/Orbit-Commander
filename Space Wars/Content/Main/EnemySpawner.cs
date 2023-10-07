using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.UI_Elements;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Space_Wars.Content.Main
{
    class EnemySpawner
    {
        private int wave;
        private Dictionary<int, DelegateEnemy> enemyCreditValues; 
        private float waveTimer;
        private float difficulty;
        private Vector2 spawnLocation;
        private Player player;
        private Random random;
        public delegate Enemy DelegateEnemy(Vector2 position, Vector2 velocity, float angle, float angularVelocity);
        private int enemyCredits;
        public static int enemiesSpawned = 0;
        public EnemySpawner(Player _player)
        {
            wave = 0;
            enemiesSpawned = 6;
            waveTimer = enemiesSpawned * 4.5f;
            player = _player;
            random = new Random();

            enemyCreditValues = new()
            {
                { 1, Enemy.NewFighter },
                { 3, Enemy.NewCarrier },
                { 5, Enemy.NewSniper },
            };
        }
        public void AddEnemyType(int _credits, DelegateEnemy _enemyType)
        {
            enemyCreditValues.Add(_credits, _enemyType);
        }

        public Vector2 NewSpawnLocation()
        {
            //Creates a position vector defined by the angle from the player in radians and the distance from the edge of the screen
            float angle = (float)(random.NextDouble() - 0.5) * MathF.Tau;
            float distanceMultiplier = 1 + (float)(random.NextDouble() - 0.5) / 4;
            float distance = (Engine.screenSize.X + Engine.screenSize.Y) / 2 * distanceMultiplier;
            Vector2 spawnLocation = Engine.ToUnitVector(angle) * distance;
            return spawnLocation + EntityManager.player.position;

        }
        public void Update()
        {
            EventHandler.UpdateEnemyCountdownUI(waveTimer, enemiesSpawned * 4.5f, wave);
            if (waveTimer <= 0)
            {
                enemiesSpawned = 0;
                wave++;
                if (wave < 50)
                {
                    difficulty = (MathF.Pow(wave, 2) / 200) + 1;
                }
                else
                {
                    difficulty = wave / 2 - 11.5f;
                }
                enemyCredits = random.Next((int)(3 * difficulty), (int)(5 * difficulty));
                SpawnWaveBatch();
                waveTimer = 4.5f * enemiesSpawned;
            }
            waveTimer -= Engine.deltaSeconds;
        }

        private void SpawnWaveBatch()
        {
            int availableEnemies = (int)(wave / 10);
            if(availableEnemies >= enemyCreditValues.Count)
            {
                availableEnemies = enemyCreditValues.Count - 1;
            }
            while (enemyCredits > 0)
            {
                for (int i = 0; i <= availableEnemies; i++)
                {
                    spawnLocation = NewSpawnLocation();
                    KeyValuePair<int, DelegateEnemy> currentEnemy = enemyCreditValues.ElementAt(i);
                    float randomVal = random.Next(0, currentEnemy.Key);
                    if (randomVal == 0 && currentEnemy.Key <= enemyCredits)
                    {
                        EntityManager.Add(currentEnemy.Value(spawnLocation, player.velocity, 0, 0));
                        enemyCredits -= currentEnemy.Key;
                        enemiesSpawned++;
                    }
                }
            }
        }
        public void Draw(SpriteBatch _spriteBatch) { }
    }
}
