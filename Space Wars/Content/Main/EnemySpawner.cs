using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Space_Wars.Content.Main.Entities;
using System.Collections;
using System.Threading.Tasks;

namespace Space_Wars.Content.Main
{
    class EnemySpawner
    {
        private int wave;
        private int waveBatches;
        private List<IEnumerator> batches = new List<IEnumerator>();
        private Enemy[] enemies;
        private float waveTimer;
        private float difficulty;
        private Vector2 spawnLocation;
        private Player Player;
        private Random random;
        private int randomValue;
        public EnemySpawner(Player player)
        {
            wave = 0;
            waveTimer = 10;
            Player = player;
            random = new Random();
        }

        public Vector2 NewSpawnLocation()
        {
            //Creates a position vector defined by the angle from the player in radians and the distance from the edge of the screen
            return Vector2.Zero;

        }
        public void Update()
        {

            if (waveTimer <= 0)
            {
                wave++;
                difficulty = MathF.Sqrt(wave);
                randomValue = (random.Next((int)(3 * difficulty), (int)(5 * difficulty)));
                if (difficulty < 1)
                {
                    difficulty = 1;
                }
                for (int i = 0; i < randomValue; i++)
                {
                    SpawnWaveBatch(1);
                }
                if(wave > 5)
                {
                    for (int i = 0; i < difficulty; i++)
                    {
                        //Vector2 spawnLocation = NewSpawnLocation();
                        //GenerateDrone(new EnemyCruiser(spawnLocation, Player.Velocity, 0, 0, Player, difficulty));
                    }
                }
                waveTimer = 60;
            }

            waveTimer -= Engine.deltaSeconds;
        }

        private void SpawnWaveBatch(int amount)
        {
            for(int i = 0; i < amount; i++)
            {
                spawnLocation = NewSpawnLocation();
                Enemy.NewFighter(spawnLocation, Player.Velocity, 0, 0);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Engine.line, new Vector2(Engine.screenSize.X/50, Engine.screenSize.Y/50), new Rectangle(0, 0, (int)Engine.screenSize.X - (int)Engine.screenSize.X / 25, 1), 
                Color.Black, 0, Vector2.Zero, new Vector2(waveTimer / 60, 1), SpriteEffects.None, 1);
        }
    }
}
