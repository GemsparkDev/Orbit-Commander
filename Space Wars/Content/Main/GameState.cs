using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;
using System.Collections.Generic;
using System.Diagnostics;
using UILib.Content.Main;

namespace Space_Wars.Content.Main;

public static class CurrentGameState
{
    private static GameState currentGameState;
    public static void SwitchState(GameState _gameState)
    {
        SoundManager.SetAllSounds(false);
        currentGameState = _gameState;
        currentGameState.Initialize();
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
public abstract class GameState
{
    public virtual void Initialize() { }
    public abstract void Update();
    public abstract void Draw(SpriteBatch _spriteBatch);
}

public class MainMenu : GameState
{
    private GravitationalSource menuPlanet = new(new Vector2(0, 750), Vector2.Zero, 5000, 9, true, Color.Cyan);
    private GravitationalSource moonPlanet = new(new Vector2(0, 1750), GravitationalSource.GetOrbitalVelocity(new Vector2(0, 1750), new Vector2(0, 750), 5000), 250, 1.5f, false, Color.Cyan);
    private ParticleEmitter smokeParticles = new(Assets.Get(Sprite.Circle), 1f, new Vector2(0, 300 - Assets.DimsOf(Sprite.Mothership).Y + 10),
        0, 45, 1, 0, 40, 1, true, Color.Gray, Color.DarkGray, EmitterType.EmissionOverTime)
    { probability = 0.25f };
    public override void Initialize()
    {
        smokeParticles.isEmitterActive = true;
        Engine.UIManager.SetScreenMenuEnabled(false);
        SoundManager.ChangeTrack(Assets.Get(Sound.menu));
    }
    public override void Update()
    {
        menuPlanet.Update();
        moonPlanet.velocity += menuPlanet.GetAcceleration(moonPlanet.position);
        moonPlanet.Update();
        smokeParticles.Update();
        ParticleManager.Update();
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        _spriteBatch.Draw(Assets.Get(Sprite.Mothership), new Vector2(-Assets.DimsOf(Sprite.Mothership).X / 2, 300 - Assets.DimsOf(Sprite.Mothership).Y), new Color(0, 255, 0));
        ParticleManager.Draw(_spriteBatch);
    }
}
public class PlayingGame : GameState
{
    public override void Initialize()
    {
        Engine.UIManager.SetScreenMenuEnabled(true);
    }
    public override void Update()
    {
        if (!Engine.Self.IsActive)
        {
            Engine.UIManager.DisableAll();
            Engine.UIManager.GetContainer((int)Containers.PauseMenu).enabled = true;
            CurrentGameState.SwitchState(new PausedGame());
        }
        EntityManager.PlayerUpdate();
        EntityManager.IngameUpdate();
        Engine.EntityManager.Update();
        ParticleManager.Update();
        if (Input.OldState.IsKeyUp(Keys.Escape) && Input.NewState.IsKeyDown(Keys.Escape))
        {
            if (Engine.UIManager.ToggleToMenu(Engine.UIManager.GetContainer((int)Containers.PauseMenu)))
            {
                SoundManager.SetAllSounds(false);
                CurrentGameState.SwitchState(new PausedGame());
            }
        }
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        if (EntityManager.Player.modules[ModuleType.Core].isFailed)
        {
            _spriteBatch.DrawString(Assets.TextFont, "Power failure detected. Please restart system.", Engine.Camera.Position, Color.Red);
            return;
        }
        Engine.EntityManager.Draw(_spriteBatch);
        ParticleManager.Draw(_spriteBatch);
        if (Engine.DebugMode)
        {
            //Generates a grid
            for (int x = (int)(Engine.Camera.Position.X - Engine.ScreenSize.X / 2) / 50; x < (Engine.Camera.Position.X + Engine.ScreenSize.X/2) / 50; x++)
            {
                _spriteBatch.Draw(Engine.Line, new Vector2(x * 50, Engine.Camera.Position.Y - Engine.ScreenSize.Y/2), new Rectangle((int)(x * 50), 0, 1, (int)Engine.ScreenSize.Y), Color.Gray * 0.5f, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                _spriteBatch.DrawString(Assets.TextFont, ((int)x).ToString(), new Vector2(x * 50, Engine.Camera.Position.Y - Engine.ScreenSize.Y / 2), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
            }
            for (int y = (int)(Engine.Camera.Position.Y - Engine.ScreenSize.X / 2) / 50 - 1; y < (Engine.Camera.Position.Y + Engine.ScreenSize.Y/2) / 50 + 1; y++)
            {
                _spriteBatch.Draw(Engine.Line, new Vector2(Engine.Camera.Position.X - Engine.ScreenSize.X / 2, y * 50), new Rectangle(0, (int)(y * 50), (int)Engine.ScreenSize.X, 1), Color.Gray * 0.5f, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                _spriteBatch.DrawString(Assets.TextFont, ((int)y).ToString(), new Vector2(Engine.Camera.Position.X - Engine.ScreenSize.X / 2, y * 50), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
            }
        }
    }
}
public class PausedGame : GameState
{
    public override void Update()
    {
        if (Input.OldState.IsKeyUp(Keys.Escape) && Input.NewState.IsKeyDown(Keys.Escape))
        {
            if (Engine.UIManager.ToggleToMenu(Engine.UIManager.GetContainer((int)Containers.PauseMenu)))
            {
                CurrentGameState.SwitchState(new PlayingGame());
            }
        }
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        if (EntityManager.Player.modules[ModuleType.Core].isFailed)
        {
            _spriteBatch.DrawString(Assets.TextFont, "Power failure detected. Please restart system.", Engine.Camera.Position, Color.Red);
            return;
        }
        Engine.EntityManager.Draw(_spriteBatch);
        ParticleManager.Draw(_spriteBatch);
        if (Engine.DebugMode)
        {
            //Generates a grid
            for (int x = (int)(Engine.Camera.Position.X - Engine.ScreenSize.X / 2) / 50; x < (Engine.Camera.Position.X + Engine.ScreenSize.X / 2) / 50; x++)
            {
                _spriteBatch.Draw(Engine.Line, new Vector2(x * 50, Engine.Camera.Position.Y - Engine.ScreenSize.Y / 2), new Rectangle((int)(x * 50), 0, 1, (int)Engine.ScreenSize.Y), Color.Gray * 0.5f, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                _spriteBatch.DrawString(Assets.TextFont, ((int)x).ToString(), new Vector2(x * 50, Engine.Camera.Position.Y - Engine.ScreenSize.Y / 2), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
            }
            for (int y = (int)(Engine.Camera.Position.Y - Engine.ScreenSize.X / 2) / 50 - 1; y < (Engine.Camera.Position.Y + Engine.ScreenSize.Y / 2) / 50 + 1; y++)
            {
                _spriteBatch.Draw(Engine.Line, new Vector2(Engine.Camera.Position.X - Engine.ScreenSize.X / 2, y * 50), new Rectangle(0, (int)(y * 50), (int)Engine.ScreenSize.X, 1), Color.Gray * 0.5f, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                _spriteBatch.DrawString(Assets.TextFont, ((int)y).ToString(), new Vector2(Engine.Camera.Position.X - Engine.ScreenSize.X / 2, y * 50), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
            }
        }
    }
}
public class Garage : GameState
{
    public override void Initialize()
    {
        EventHandler.UpdateModulesUI();
        Engine.UIManager.SetScreenMenuEnabled(false);
    }
    public override void Update()
    {
        if (Input.OldState.IsKeyUp(Keys.Escape) && Input.NewState.IsKeyDown(Keys.Escape))
        {
            EventHandler.GarageTrigger();
        }
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        Engine.EntityManager.Draw(_spriteBatch);
        ParticleManager.Draw(_spriteBatch);
    }
}
public class MissionSelect : GameState
{
    public override void Initialize()
    {
        Engine.UIManager.SetScreenMenuEnabled(false);
        EventHandler.UpdateMissionText();
    }
    public override void Update() { }
    public override void Draw(SpriteBatch _spriteBatch) { }
}
public class Victory : GameState
{
    public override void Initialize()
    {
        Engine.UIManager.SetScreenMenuEnabled(false);
    }
    public override void Update() { }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        _spriteBatch.DrawString(Assets.TextFont, "You Win!", new Vector2(-12 * 8, -60) * Engine.UIScale + Engine.Camera.Position, Color.Yellow, 0, Vector2.Zero, Engine.UIScale * 2, SpriteEffects.None, 0);
        _spriteBatch.DrawString(Assets.TextFont, $"Your Time: {Engine.IngameTime.DrawText}", new Vector2(-12 * 12 / 2, (12 * 4 - 60)) * Engine.UIScale + Engine.Camera.Position, Color.White, 0, Vector2.Zero, Engine.UIScale/2, SpriteEffects.None, 0);
    }
}
public class Cutscene : GameState 
{
    private float time = 0;
    private bool isActive = true;
    private List<IEvent> events = new();
    private List<Actor> actors = new();
    private GameState nextGameState;
    public Cutscene(List<IEvent> _events, List<Actor> _actors, GameState _nextGameState)
    {
        events = _events;
        actors = _actors;
        nextGameState = _nextGameState;
    }
    public override void Initialize()
    {
        time = 0;
    }
    public override void Update()
    {
        ParticleManager.Update();
        time += Engine.DeltaSeconds;
        isActive = false;
        foreach (var _event in events)
        {
            //Checks every event to see if they are complete
            //If every event is no longer active, so too is the cutscene
            isActive = _event.Update(time) || isActive;
        }
        if (!isActive)
        {
            CurrentGameState.SwitchState(nextGameState);
        }
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        ParticleManager.Draw(_spriteBatch);
        foreach (var actor in actors)
        {
            actor.Draw(_spriteBatch);
        }
    }
}

