using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System;
using Space_Wars.Content.Main.Entities;

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
        public static bool startedGame = false;
        public static List<string> debugLog = new();
        private static int messageCount = 0;
        public static int UIScale = 1;

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

            _UIManager = new UIManager(this);

            IsMouseVisible = false;
        }

        public void Startgame()
        {
            EntityManager.Initialize();
            _UIManager.MainMenuToggle();
            startedGame = true;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Assets.LoadAssets(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState newState = Keyboard.GetState();
            if (startedGame == true)
            {
                if (oldState.IsKeyUp(Keys.Escape) && newState.IsKeyDown(Keys.Escape))
                {
                    _UIManager.PauseMenuToggle();
                    playingGame = !playingGame;
                }

                if (playingGame == true)
                {
                    EntityManager.Update();

                    deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
                    base.Update(gameTime);
                }
            }

            if (oldState.IsKeyUp(Keys.OemTilde) && newState.IsKeyDown(Keys.OemTilde))
            {
                debugMode = !debugMode;
            }
            _UIManager.Update();
            oldState = newState;
        }

        public static void WriteLine<T>(T arg)
        {
            String stringLog = arg?.ToString();
            debugLog.Insert(0, $"{messageCount}: {stringLog}");
            if(debugLog.Count > 10)
            {
                debugLog.RemoveAt(10); 
            }
            messageCount++;
        }

        public static void PlaySound(SoundEffect sound, Vector2 playLocation)
        {
            Vector2 listenerLocation = EntityManager.player.Position;
            float distance = MathF.Sqrt(MathF.Pow(playLocation.X-listenerLocation.X, 2)+MathF.Pow(playLocation.Y - listenerLocation.Y, 2));
            float volume = -MathF.Pow(distance/800, 2) + 1;
            if(volume < 0)
            {
                volume = 0;
            }
            sound.Play(volume, 0, 0);
            WriteLine(volume);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            if (startedGame == true)
            {
                var backgroundParallax = -EntityManager.player.Position / 100;
                spriteBatch.Draw(Assets.Sprites["Stars"], Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.3f);
                spriteBatch.Draw(Assets.Sprites["Moon"], new Vector2(screenSize.X / 5f, -100) + backgroundParallax, null, Color.White, 0, Vector2.Zero, 1.2f, SpriteEffects.None, 0.3f);
                EntityManager.Draw(spriteBatch);
                if (debugMode == true)
                {
                    //Generates a grid
                    for (int x = (int)-screenPosition.X / 50; x < -screenPosition.X / 50 + screenSize.X / 50; x++)
                    {
                        spriteBatch.Draw(line, new Vector2(x * 50 + screenPosition.X, 0), new Rectangle((int)(x * 50 + screenPosition.X), 0, 1, (int)screenSize.Y), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.2f);
                    }
                    for (int y = (int)-screenPosition.Y / 50; y < -screenPosition.Y / 50 + screenSize.Y / 50; y++)
                    {
                        spriteBatch.Draw(line, new Vector2(0, y * 50 + screenPosition.Y), new Rectangle(0, (int)(y * 50 + screenPosition.Y), (int)screenSize.X, 1), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.2f);
                    }
                }
            }

            if (debugMode == true)
            {
                //Displays the debug log
                for (int i = 0; i < debugLog.Count; i++)
                {
                    Vector2 textPosition = new(10, 10 + 15 * i * UIScale);
                    spriteBatch.DrawString(Assets.textFont, debugLog[i], textPosition, Color.White, 0, Vector2.Zero, UIScale, SpriteEffects.None, 0);
                }

                //Displays a centering line
                spriteBatch.Draw(line, new Vector2(0, screenSize.Y / 2), new Rectangle(0, (int)screenPosition.Y, (int)screenSize.X, 1), Color.Gray);
                spriteBatch.Draw(line, new Vector2(screenSize.X / 2, 0), new Rectangle((int)screenPosition.X, 0, 1, (int)screenSize.Y), Color.Gray);
            }
            _UIManager.Draw(spriteBatch);
            spriteBatch.Draw(Assets.Sprites["Cursor"], new Vector2(Mouse.GetState().X, Mouse.GetState().Y), Color.White);
            spriteBatch.End();
            //Sends a spritebatch through to the entity manager every draw update

            base.Draw(gameTime);
        }
    }
}
