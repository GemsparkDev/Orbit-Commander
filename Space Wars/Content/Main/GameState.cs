using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Space_Wars.Content.Main.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Wars.Content.Main
{
    public static class CurrentGameState
    {
        private static IGameState currentGameState;
        public static Engine engine;
        public static void SwitchState(IGameState _gameState)
        {
            currentGameState = _gameState;
            currentGameState._Engine = engine;
        }
        public static void Initialize(Engine _engine)
        {
            engine = _engine;
        }
        public static void Update()
        {
            currentGameState.Update();
        }

        public static void Draw(SpriteBatch _SpriteBatch)
        {
            currentGameState.Draw(_SpriteBatch);
        }
    }
    public interface IGameState
    {
        public Engine _Engine { get; set; }
        public abstract void Update();
        public abstract void Draw(SpriteBatch _spriteBatch);
    }

    public class MainMenu : IGameState
    {
        public Engine _Engine { get; set; }
        public void Update()
        {

        }
        public void Draw(SpriteBatch _spriteBatch)
        {

        }
    }
    public class PlayingGame : IGameState
    {
        public Engine _Engine { get; set; }
        public void Update()
        {
            EntityManager.Update();
            EntityManager.PlayerUpdate();
            ParticleManager.Update();
            if (_Engine.oldState.IsKeyUp(Keys.Escape) && _Engine.newState.IsKeyDown(Keys.Escape))
            {
                if (_Engine._UIManager.PauseMenuTrigger())
                {
                    SoundManager.SetAllSounds(false);
                    CurrentGameState.SwitchState(new PausedGame());
                }
            }
        }
        public void Draw(SpriteBatch _spriteBatch)
        {
            EntityManager.Draw(_spriteBatch);
            ParticleManager.Draw(_spriteBatch);
            if (Engine.debugMode == true)
            {
                //Generates a grid
                for (int x = (int)(-Engine.screenPosition.X) / 50 - 1; x < (-Engine.screenPosition.X + Engine.screenSize.X) / 50 + 1; x++)
                {
                    _spriteBatch.Draw(Engine.line, new Vector2(x * 50 + Engine.screenPosition.X - Engine.mousePositionOffset.X, 0), new Rectangle((int)(x * 50 + Engine.screenPosition.X), 0, 1, (int)Engine.screenSize.Y), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                    _spriteBatch.DrawString(Assets.textFont, ((int)x).ToString(), new Vector2(x * 50 + Engine.screenPosition.X - Engine.mousePositionOffset.X, 0), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                }
                for (int y = (int)(-Engine.screenPosition.Y) / 50 - 1; y < (-Engine.screenPosition.Y + Engine.screenSize.Y) / 50 + 1; y++)
                {
                    _spriteBatch.Draw(Engine.line, new Vector2(0, y * 50 + Engine.screenPosition.Y - Engine.mousePositionOffset.Y), new Rectangle(0, (int)(y * 50 + Engine.screenPosition.Y), (int)Engine.screenSize.X, 1), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                    _spriteBatch.DrawString(Assets.textFont, ((int)y).ToString(), new Vector2(0, y * 50 + Engine.screenPosition.Y - Engine.mousePositionOffset.Y), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                }
            }
        }
    }
    public class PausedGame : IGameState
    {
        public Engine _Engine { get; set; }
        public void Update()
        {
            if (_Engine.oldState.IsKeyUp(Keys.Escape) && _Engine.newState.IsKeyDown(Keys.Escape))
            {
                if (_Engine._UIManager.PauseMenuTrigger())
                {
                    CurrentGameState.SwitchState(new PlayingGame());
                }
            }
        }
        public void Draw(SpriteBatch _spriteBatch)
        {
            EntityManager.Draw(_spriteBatch);
            ParticleManager.Draw(_spriteBatch);
            if (Engine.debugMode == true)
            {
                //Generates a grid
                for (int x = (int)(-Engine.screenPosition.X) / 50 - 1; x < (-Engine.screenPosition.X + Engine.screenSize.X) / 50 + 1; x++)
                {
                    _spriteBatch.Draw(Engine.line, new Vector2(x * 50 + Engine.screenPosition.X - Engine.mousePositionOffset.X, 0), new Rectangle((int)(x * 50 + Engine.screenPosition.X), 0, 1, (int)Engine.screenSize.Y), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                    _spriteBatch.DrawString(Assets.textFont, ((int)x).ToString(), new Vector2(x * 50 + Engine.screenPosition.X - Engine.mousePositionOffset.X, 0), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                }
                for (int y = (int)(-Engine.screenPosition.Y) / 50 - 1; y < (-Engine.screenPosition.Y + Engine.screenSize.Y) / 50 + 1; y++)
                {
                    _spriteBatch.Draw(Engine.line, new Vector2(0, y * 50 + Engine.screenPosition.Y - Engine.mousePositionOffset.Y), new Rectangle(0, (int)(y * 50 + Engine.screenPosition.Y), (int)Engine.screenSize.X, 1), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                    _spriteBatch.DrawString(Assets.textFont, ((int)y).ToString(), new Vector2(0, y * 50 + Engine.screenPosition.Y - Engine.mousePositionOffset.Y), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                }
            }
        }
    }
    public class Garage : IGameState
    {
        public Engine _Engine { get; set; }
        public void Update()
        {
            EntityManager.Update();
            if (_Engine.oldState.IsKeyUp(Keys.Escape) && _Engine.newState.IsKeyDown(Keys.Escape))
            {
                EventHandler.GarageTrigger();
            }
        }
        public void Draw(SpriteBatch _spriteBatch)
        {

        }
    }
    public class Victory : IGameState
    {
        public Engine _Engine { get; set; }
        public void Update()
        {

        }
        public void Draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.DrawString(Assets.textFont, "You Win!", Engine.screenSize/2, Color.Yellow, 0, Vector2.Zero, Engine.UIScale * 5, SpriteEffects.None, 0.45f);
        }
    }
}
