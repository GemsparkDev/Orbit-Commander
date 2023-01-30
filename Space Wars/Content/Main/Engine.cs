using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using Space_Wars.Content.Main.Entities;
using Myra;
using Myra.Graphics2D.UI;

using System.Diagnostics;

namespace Space_Wars.Content.Main
{
    public class Engine : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private UIManager _UIManager;
        public static Vector2 screenSize;
        public static Vector2 screenPosition;
        private KeyboardState oldState;
        public static float deltaSeconds;
        public static Texture2D line;
        public static bool debugMode;
        public static bool playingGame = true;       

        public Engine()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();

            graphics.PreferredBackBufferWidth = 1280;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = 720;   // set this value to the desired height of your window
            graphics.ApplyChanges();


            debugMode = false;
            screenSize = new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            line = new Texture2D(graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            line.SetData(new[] { Color.White });

            //Generates a new player at the center of the screen
            EntityManager.Initialize();
            MyraEnvironment.Game = this;
            _UIManager = new UIManager(this, new Desktop());

            IsMouseVisible = false;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Assets.LoadAssets(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState newState = Keyboard.GetState();
            if (oldState.IsKeyUp(Keys.Escape) && newState.IsKeyDown(Keys.Escape))
            {
                UIManager.PauseMenu();
                playingGame = !playingGame;
            }

            if (playingGame == true)
            {
                EntityManager.Update();

                deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
                base.Update(gameTime);
            }
            oldState = newState;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            //Generates a grid
            if (debugMode == true)
            {
                for (int x = (int)-screenPosition.X / 50; x < -screenPosition.X / 50 + screenSize.X / 50; x++)
                {
                    spriteBatch.Draw(line, new Rectangle((int)(x * 50 + screenPosition.X), 0, 1, (int)screenSize.Y), Color.Black);
                }
                for (int y = (int)-screenPosition.Y / 50; y < -screenPosition.Y / 50 + screenSize.Y / 50; y++)
                {
                    spriteBatch.Draw(line, new Rectangle((int)0, (int)(y * 50 + screenPosition.Y), (int)screenSize.X, 1), Color.Black);
                }
            }
            EntityManager.Draw(spriteBatch);
            _UIManager.Draw();
            spriteBatch.Draw(Assets.Sprites["Cursor"], new Vector2(Mouse.GetState().X, Mouse.GetState().Y), Color.White);
            spriteBatch.End();
            //Sends a spritebatch through to the entity manager every draw update

            base.Draw(gameTime);
        }
    }
}
