using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Space_Wars.Content.Main.UI_Elements;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;


namespace Space_Wars.Content.Main
{
    public class Engine : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        public UIManager _UIManager;
        public static Camera camera;
        public static Vector2 screenSize;
        public static Vector2 mousePositionOffset;
        public KeyboardState oldState;
        public KeyboardState newState;
        public static Timespan ingameTime = new();
        public static float deltaSeconds;
        public static float timeScale = 1f;
        public static Texture2D line;
        public static bool debugMode;
        public static bool patchedConics = true;
        public static List<string> debugLog = new();
        private static int messageCount = 0;
        public static float UIScale = 2f;
        public static int targetFramerate = 60;

        public Engine()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
            Window.Title = ("Lagrange Commander");
            graphics.PreferredBackBufferWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            IsFixedTimeStep = true;
            if(timeScale > 1)
            {
                TargetElapsedTime = TimeSpan.FromSeconds(1d / (double)(targetFramerate * timeScale));
            }
            else
            {
                TargetElapsedTime = TimeSpan.FromSeconds(1 / (double)(targetFramerate));
            }

            screenSize = new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            debugMode = false;
            line = new Texture2D(graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            line.SetData(new[] { Color.White });

            camera = new Camera(GraphicsDevice.Viewport)
            {
                Origin = screenSize / 2,
                Zoom = new Vector2(1f, 1f),
                Position = Vector2.Zero
            };

            _UIManager = new UIManager();
            CurrentGameState.Initialize(this);
            CurrentGameState.SwitchState(new MainMenu());
            EventHandler.root = this;
            EventHandler.UIManager = _UIManager;

            IsMouseVisible = false;
        }

        public void Startgame()
        {
            if (_UIManager.MainMenu.enabled == true)
            {
                UIManager.ToggleMenu(_UIManager.MainMenu);
            }
            ParticleManager.Initialize();
            EntityManager.Initialize(this);
            SoundManager.Initialize();
            EventHandler.PairPlayerUIManager();
            CurrentGameState.SwitchState(new PlayingGame());
            ingameTime = new();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Assets.LoadAssets(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            _UIManager.Update();
            SoundManager.Update();
            newState = Keyboard.GetState();
            CurrentGameState.Update();

            if (oldState.IsKeyUp(Keys.OemTilde) && newState.IsKeyDown(Keys.OemTilde))
            {
                debugMode = !debugMode;
            }
            oldState = newState;
            deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds * timeScale;
            base.Update(gameTime);
        }

        public static void WriteLine<T>(T arg)
        {
            String stringLog = arg?.ToString();
            debugLog.Insert(0, $"{stringLog}");
            messageCount++;
        }

        public static Vector2 ToUnitVector(float _angle)
        {
            return new Vector2(MathF.Sin(_angle), -MathF.Cos(_angle));
        }
        public static float Lerp(float _valueOne, float _valueTwo, float _length)
        {
            return _valueOne * (1 - _length) + _valueTwo * _length;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, transformMatrix: camera.ViewMatrix);
            //Assets.effect.Parameters["Time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            CurrentGameState.Draw(spriteBatch);
            spriteBatch.End();
            
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
            spriteBatch.DrawString(Assets.textFont, $"{(int)(1 / gameTime.ElapsedGameTime.TotalSeconds)}", new Vector2((int)(screenSize.X / 1.25f), 0), Color.White, 0, new Vector2(0, -5), UIScale, SpriteEffects.None, 0.45f);
            _UIManager.Draw(spriteBatch);
            if (UIManager.lockMouseInput == false)
            {
                spriteBatch.Draw(Assets.Get(Sprite.Cursor), new Vector2(Mouse.GetState().X, Mouse.GetState().Y), null, Color.White, 0, Vector2.Zero, 1, 0, 0.5f);
            }
            if (debugMode == true)
            {
                //Displays the debug log
                int logCount = debugLog.Count;
                if (logCount > 10)
                {
                    logCount = 10;
                }
                for (int i = 0; i < logCount; i++)
                {
                    Vector2 textPosition = new(35, 20 + 15 * i * UIScale);
                    try
                    {
                        spriteBatch.DrawString(Assets.textFont, $"{i + 1}: {debugLog[i]}", textPosition, Color.White, 0, Vector2.Zero, UIScale, SpriteEffects.None, 0.45f);
                    }
                    catch (Exception e)
                    {
                        spriteBatch.DrawString(Assets.textFont, $"{i + 1}: {e.Message}", textPosition, Color.Red, 0, Vector2.Zero, UIScale, SpriteEffects.None, 0.45f);
                    }
                }
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
    public struct Timespan
    {
        private float _seconds;
        public float Duration
        {
            get { return _seconds; }
            set { _seconds = value; }
        }
        public float Seconds
        {
            get { return _seconds % 60; }
        }
        public float Minutes
        {
            get { return (int)(_seconds / 60) % 60; }
        }
        public float Hours
        {
            get { return (int)(_seconds / 3600); }
        }
        public string drawText
        {
            get { return $"{Hours:00}:{Minutes:00}:{Seconds:00.00}"; }
        }
    }
}
