using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Space_Wars.Content.Main.UI_Elements;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main
{
    public class Engine : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private UIManager _UIManager;
        public static Vector2 screenSize;
        public static Vector2 screenPosition;
        public static Vector2 mousePositionOffset;
        private KeyboardState oldState;
        public static float deltaSeconds;
        public static float timeScale = 1.0f;
        public static Texture2D line;
        public static bool debugMode;
        public static bool playingGame = true;
        public static bool startedGame = false;
        public static List<string> debugLog = new();
        private static int messageCount = 0;
        public static int UIScale = 1;
        public static int targetFramerate = 60;

        public Engine()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();

            graphics.PreferredBackBufferWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1d / (double)targetFramerate);


            debugMode = false;
            screenSize = new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            line = new Texture2D(graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            line.SetData(new[] { Color.White });

            _UIManager = new UIManager(this);
            EventHandler.root = this;
            EventHandler.UIManager = _UIManager;

            IsMouseVisible = false;
        }

        public void Startgame()
        {
            EntityManager.Initialize(this);
            UIManager.ToggleMenu(_UIManager.MainMenu);
            startedGame = true;
            EventHandler.PairPlayerUIManager();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Assets.LoadAssets(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            _UIManager.Update();
            KeyboardState newState = Keyboard.GetState();
            if (startedGame == true)
            {
                if (oldState.IsKeyUp(Keys.Escape) && newState.IsKeyDown(Keys.Escape))
                {
                    if (_UIManager.PauseMenuTrigger())
                    {
                        playingGame = !playingGame;
                    }
                }

                if (playingGame == true)
                {
                    EntityManager.Update();

                    deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds * timeScale;
                    base.Update(gameTime);
                }
            }

            if (oldState.IsKeyUp(Keys.OemTilde) && newState.IsKeyDown(Keys.OemTilde))
            {
                debugMode = !debugMode;
            }
            oldState = newState;
        }

        public static void WriteLine<T>(T arg)
        {
            String stringLog = arg?.ToString();
            debugLog.Insert(0, $"{messageCount}: {stringLog}");
            if (debugLog.Count > 10)
            {
                debugLog.RemoveAt(10);
            }
            messageCount++;
        }

        public static void PlaySound(SoundEffect sound, Vector2 playLocation)
        {
            Vector2 listenerLocation = EntityManager.player.position;
            float distance = MathF.Sqrt(MathF.Pow(playLocation.X - listenerLocation.X, 2) + MathF.Pow(playLocation.Y - listenerLocation.Y, 2));
            float volume = -(distance / 1000) + 1;
            //float pan = -(listenerLocation.X - playLocation.X) / (screenSize.X);
            //Monogame issue, audio cannot pan smoothly
            if (volume < 0) { volume = 0; }
            SoundEffectInstance soundInstance = sound.CreateInstance();
            soundInstance.Volume = volume;
            soundInstance.Play();
        }
        public static void PlayGlobalSound(SoundEffect sound)
        {
            sound.Play(1, 0, 0);
        }

        public static Vector2 ToUnitVector(float _angle)
        {
            return new Vector2(MathF.Sin(_angle), -MathF.Cos(_angle));
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            if (startedGame == true)
            {
                //var backgroundParallax = -EntityManager.player.position / 100;
                //spriteBatch.Draw(Assets.Sprites["Stars"], Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
                //spriteBatch.Draw(Assets.Sprites["Moon"], new Vector2(screenSize.X * 2 / 3 - Assets.Sprites["Moon"].Width/2, -100) + backgroundParallax, null, Color.White, 0, Vector2.Zero, 1.2f, SpriteEffects.None, 0.05f);
                EntityManager.Draw(spriteBatch);
                if (debugMode == true)
                {
                    //Generates a grid
                    for (int x = (int)(-screenPosition.X) / 50 - 1; x < (-screenPosition.X + screenSize.X) / 50 + 1; x++)
                    {
                        spriteBatch.Draw(line, new Vector2(x * 50 + screenPosition.X - mousePositionOffset.X, 0), new Rectangle((int)(x * 50 + screenPosition.X), 0, 1, (int)screenSize.Y ), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                        spriteBatch.DrawString(Assets.textFont, ((int)x).ToString(), new Vector2(x * 50 + screenPosition.X - mousePositionOffset.X, 0), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                    }
                    for (int y = (int)(-screenPosition.Y) / 50 - 1; y < (-screenPosition.Y + screenSize.Y) / 50 + 1; y++)
                    {
                        spriteBatch.Draw(line, new Vector2(0, y * 50 + screenPosition.Y - mousePositionOffset.Y), new Rectangle(0, (int)(y * 50 + screenPosition.Y), (int)screenSize.X, 1), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                        spriteBatch.DrawString(Assets.textFont, ((int)y).ToString(), new Vector2(0, y * 50 + screenPosition.Y - mousePositionOffset.Y), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                    }
                }
            }

            if (debugMode == true)
            {
                //Displays the debug log
                for (int i = 0; i < debugLog.Count; i++)
                {
                    Vector2 textPosition = new(10, 10 + 15 * i * UIScale);
                    spriteBatch.DrawString(Assets.textFont, debugLog[i], textPosition, Color.White, 0, Vector2.Zero, UIScale, SpriteEffects.None, 0.45f);
                }
            }
            spriteBatch.DrawString(Assets.textFont, $"{(int)(1/gameTime.ElapsedGameTime.TotalSeconds)}", new Vector2((int)(screenSize.X/1.25f), 0), Color.White, 0, Vector2.Zero, UIScale, SpriteEffects.None, 0.45f);
            _UIManager.Draw(spriteBatch);
            if(UIManager.lockMouseInput == false)
            {
                spriteBatch.Draw(Assets.Sprites["Cursor"], new Vector2(Mouse.GetState().X, Mouse.GetState().Y), null, Color.White, 0, Vector2.Zero, 1, 0, 0.5f);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
