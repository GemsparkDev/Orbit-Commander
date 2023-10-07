using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Space_Wars.Content.Main.Entities;
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
        private GravitationalSource menuPlanet = new(new Vector2(0, 750), Vector2.Zero, 5000, 9, true, Color.Cyan);
        private ParticleEmitter smokeParticles = new(Assets.Sprites["Circle"], 1f, new Vector2(0, 300 - Assets.Sprites["Mothership"].Height + 10), 0, 45, 1, 0, 5, 1, true, Color.Gray, Color.DarkGray, EmitterType.EmissionOverTime);
        public void Update()
        {
            if(menuPlanet.moons.Count == 0)
            {
                menuPlanet.AddMoon(1000, 250, 1.5f, false);
                ParticleManager.Add(smokeParticles);
                smokeParticles.isEmitterActive = true;
            }
            smokeParticles.Update();
            menuPlanet.Update();
            ParticleManager.Update();
        }
        public void Draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(Assets.Sprites["Mothership"], new Vector2(-Assets.Sprites["Mothership"].Width/2, 300 - Assets.Sprites["Mothership"].Height), new Color(0, 255, 0)); ;
            ParticleManager.Draw(_spriteBatch);
        }
    }
    public class PlayingGame : IGameState
    {
        public Engine _Engine { get; set; }
        public void Update()
        {
            EntityManager.Update();
            EntityManager.PlayerUpdate();
            EntityManager.IngameUpdate();
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
                for (int x = (int)(Engine.camera.Position.X - Engine.screenSize.X / 2) / 50; x < (Engine.camera.Position.X + Engine.screenSize.X/2) / 50; x++)
                {
                    _spriteBatch.Draw(Engine.line, new Vector2(x * 50 - Engine.mousePositionOffset.X, Engine.camera.Position.Y - Engine.screenSize.Y/2), new Rectangle((int)(x * 50), 0, 1, (int)Engine.screenSize.Y), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                    _spriteBatch.DrawString(Assets.textFont, ((int)x).ToString(), new Vector2(x * 50 - Engine.mousePositionOffset.X, Engine.camera.Position.Y - Engine.screenSize.Y / 2), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                }
                for (int y = (int)(Engine.camera.Position.Y - Engine.screenSize.X / 2) / 50 - 1; y < (Engine.camera.Position.Y + Engine.screenSize.Y/2) / 50 + 1; y++)
                {
                    _spriteBatch.Draw(Engine.line, new Vector2(Engine.camera.Position.X - Engine.screenSize.X / 2, y * 50 - Engine.mousePositionOffset.Y), new Rectangle(0, (int)(y * 50), (int)Engine.screenSize.X, 1), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                    _spriteBatch.DrawString(Assets.textFont, ((int)y).ToString(), new Vector2(Engine.camera.Position.X - Engine.screenSize.X / 2, y * 50 - Engine.mousePositionOffset.Y), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
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
                for (int x = (int)(Engine.camera.Position.X - Engine.screenSize.X / 2) / 50; x < (Engine.camera.Position.X + Engine.screenSize.X / 2) / 50; x++)
                {
                    _spriteBatch.Draw(Engine.line, new Vector2(x * 50 - Engine.mousePositionOffset.X, Engine.camera.Position.Y - Engine.screenSize.Y / 2), new Rectangle((int)(x * 50), 0, 1, (int)Engine.screenSize.Y), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                    _spriteBatch.DrawString(Assets.textFont, ((int)x).ToString(), new Vector2(x * 50 - Engine.mousePositionOffset.X, Engine.camera.Position.Y - Engine.screenSize.Y / 2), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                }
                for (int y = (int)(Engine.camera.Position.Y - Engine.screenSize.X / 2) / 50 - 1; y < (Engine.camera.Position.Y + Engine.screenSize.Y / 2) / 50 + 1; y++)
                {
                    _spriteBatch.Draw(Engine.line, new Vector2(Engine.camera.Position.X - Engine.screenSize.X / 2, y * 50 - Engine.mousePositionOffset.Y), new Rectangle(0, (int)(y * 50), (int)Engine.screenSize.X, 1), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                    _spriteBatch.DrawString(Assets.textFont, ((int)y).ToString(), new Vector2(Engine.camera.Position.X - Engine.screenSize.X / 2, y * 50 - Engine.mousePositionOffset.Y), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
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
            if(EventHandler.isTraining == false)
            {
                EntityManager.IngameUpdate();
            }
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
            _spriteBatch.DrawString(Assets.textFont, $"Your Time: {Engine.ingameTime.drawText}", Engine.screenSize / 2 + new Vector2(0, 12), Color.White, 0, new Vector2(0, -5), Engine.UIScale, SpriteEffects.None, 0.45f);
        }
    }
    public class TrainingMode : IGameState
    {
        public Engine _Engine { get; set; }
        public void Update()
        {
            EntityManager.PlayerUpdate();
            EntityManager.Update();
            ParticleManager.Update();
            EntityManager.trainingSimulator.Update();
        }
        public void Draw(SpriteBatch _spriteBatch)
        {
            EntityManager.trainingSimulator.Draw(_spriteBatch);
            EntityManager.Draw(_spriteBatch);
            ParticleManager.Draw(_spriteBatch);
        }
    }
}
