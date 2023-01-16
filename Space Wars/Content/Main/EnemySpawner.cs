using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main
{
    class EnemySpawner
    {
        private int wave;
        private float waveTimer;
        private float difficulty;
        private Player Player;
        private Random random;
        public EnemySpawner(Player player)
        {
            wave = 0;
            waveTimer = 600;
            Player = player;
            random = new Random();
        }

        public Vector2 NewSpawnLocation()
        {
            //Creates a position vector defined by the angle from the player in radians and the distance from the edge of the screen
            return Engine.screenSize/2;

        }
        public void Update()
        {

            if (waveTimer <= 0)
            {
                /*
                difficulty = MathF.Sqrt(wave);
                if (difficulty < 1)
                {
                    difficulty = 1;
                }
                for (int i = 0; i < random.Next((int)(3 * difficulty), (int)(5 * difficulty)); i++)
                {
                    Vector2 spawnLocation = NewSpawnLocation();
                    EntityManager.Add(new Enemy(spawnLocation, Player.Velocity, 0, 0, 5, Assets.Sprites["FighterDrone"]));
                }
                if(wave > 5)
                {
                    for (int i = 0; i < (int)difficulty; i++)
                    {
                        //Vector2 spawnLocation = NewSpawnLocation();
                        //GenerateDrone(new EnemyCruiser(spawnLocation, Player.Velocity, 0, 0, Player, difficulty));
                    }
                }
                */
                Vector2 spawnLocation = NewSpawnLocation();
                Enemy.NewFighter(spawnLocation, Player.Velocity, 0, 0);
                wave++;
                waveTimer = 60;
            }

            waveTimer -= Engine.deltaSeconds;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Engine.line, new Vector2(Engine.screenSize.X/50, Engine.screenSize.Y/50), new Rectangle(0, 0, (int)Engine.screenSize.X - (int)Engine.screenSize.X / 25, 1), 
                Color.Black, 0, Vector2.Zero, new Vector2(waveTimer / 60, 1), SpriteEffects.None, 1);
        }
    }
}
