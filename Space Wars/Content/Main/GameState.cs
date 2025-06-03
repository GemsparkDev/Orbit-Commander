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

    public static void Draw(SpriteBatch _spriteBatch)
    {
        currentGameState.Draw(_spriteBatch);
    }
}
public abstract class GameState
{
    public virtual void Initialize() { }
    public abstract void Update();
    public abstract void Draw(SpriteBatch _spriteBatch);
    //Useful if several game states want to render the same game space
    internal static void RenderGamespace(SpriteBatch _spriteBatch)
    {
        if (EntityManager.Player.modules[ModuleType.Core].isFailed)
        {
            string text = "Power failure detected. Please restart system.";
            _spriteBatch.DrawString(Assets.TextFont, text, Engine.Camera.Position - new Vector2(text.Length * 3, 6), Color.Red);
            return;
        }
        Engine.EntityManager.Draw(_spriteBatch);
        ParticleManager.Draw(_spriteBatch);
        if (Engine.DebugMode)
        {
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
        Engine.UIManager.ScreenWindow.enabled = false;
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
        Engine.UIManager.ScreenWindow.enabled = true;
        Engine.UIManager.ScreenWindow.CurrentTab = 0;
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
        ParticleManager.Update();
        if (Input.OldState.IsKeyUp(Keys.Escape) && Input.NewState.IsKeyDown(Keys.Escape))
        {
            if (Engine.UIManager.ToggleToMenu(Engine.UIManager.GetContainer((int)Containers.PauseMenu)))
            {
                SoundManager.SetAllSounds(false);
                CurrentGameState.SwitchState(new PausedGame());
            }
        }
        if (Input.OldState.IsKeyUp(Keys.F) && Input.NewState.IsKeyDown(Keys.F))
        {
            SoundManager.SetAllSounds(false);
            CurrentGameState.SwitchState(new InShip());
            Engine.UIManager.GetContainer((int)Containers.FuseMenu).enabled = true;
        }
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        RenderGamespace(_spriteBatch);
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
        RenderGamespace(_spriteBatch);
    }
}
public class Garage : GameState
{
    public override void Initialize()
    {
        EventHandler.UpdateModulesUI();
        Engine.UIManager.ScreenWindow.enabled = true;
    }
    public override void Update()
    {
        EntityManager.IngameUpdate();
        ParticleManager.Update();
        if (Input.OldState.IsKeyUp(Keys.Escape) && Input.NewState.IsKeyDown(Keys.Escape))
        {
            //Only toggle game state if in valid module configuration
            if (EventHandler.SyncModules())
            {
                EventHandler.GarageTrigger();
            }
        }
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        RenderGamespace(_spriteBatch);
    }
}
public class MissionSelect : GameState
{
    private float time = 0;
    private List<int> missions = [1, 0, 0];
    private List<List<ParticleEmitter>> missionOrbits = [];
    public MissionSelect()
    {
        var center = new Vector2(Engine.ScreenSize.X/6, 0);
        foreach (var missionCount in missions)
        {
            var system = new List<ParticleEmitter>();
            for (int i = 0; i < missionCount; i++)
            {
                var element = Engine.UIManager.ScreenWindow.GetFuncWidget(i, missionCount) as Button;
                float distance = Vector2.Distance(element.Offset + element.Size / 2 * UIManager.UIScale, new Vector2(Engine.ScreenSize.X * 2 / 6, Engine.ScreenSize.Y/4) * UIManager.UIScale);
                
                system.Add(new ParticleEmitter(Assets.Get(Sprite.Dot), center, distance, 1, new Color(0, 255, 255)));
            }
            missionOrbits.Add(system);
        }
    }
    public override void Initialize()
    {
        Engine.UIManager.ScreenWindow.CurrentTab = 1;
        Engine.UIManager.ScreenWindow.enabled = true;
        EventHandler.UpdateMissionText();
    }
    public override void Update() 
    {
        time += Engine.DeltaSeconds;
        ParticleManager.Update();
        foreach (var orbit in missionOrbits[Engine.UIManager.ScreenWindow.CurrentTab - 1])
        { 
            orbit.Update();
        }
    }
    public override void Draw(SpriteBatch _spriteBatch) 
    {
        ParticleManager.Draw(_spriteBatch);
    }
}
public class Victory : GameState
{
    public override void Initialize()
    {
        Engine.UIManager.ScreenWindow.enabled = false;
    }
    public override void Update() { }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        _spriteBatch.DrawString(Assets.TextFont, "You Win!", new Vector2(-12 * 8, -60) * Engine.UIScale + Engine.Camera.Position, Color.Yellow, 0, Vector2.Zero, Engine.UIScale * 2, SpriteEffects.None, 0);
        _spriteBatch.DrawString(Assets.TextFont, $"Your Time: {Engine.IngameTime.DrawText}", new Vector2(-12 * 12 / 2, (12 * 4 - 60)) * Engine.UIScale + Engine.Camera.Position, Color.White, 0, Vector2.Zero, Engine.UIScale/2, SpriteEffects.None, 0);
    }
}
public class Cutscene(List<IEvent> _events, List<Actor> _actors, GameState _nextGameState) : GameState 
{
    private float time = 0;
    public override void Initialize()
    {
        time = 0;
        Engine.UIManager.ScreenWindow.enabled = false;
    }
    public override void Update()
    {
        ParticleManager.Update();
        time += Engine.DeltaSeconds;
        bool isActive = false;
        foreach (var _event in _events)
        {
            //Checks every event to see if they are complete
            //If every event is no longer active, so too is the cutscene
            isActive = _event.Update(time) || isActive;
        }
        if (!isActive)
        {
            CurrentGameState.SwitchState(_nextGameState);
        }
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        ParticleManager.Draw(_spriteBatch);
        foreach (var actor in _actors)
        {
            actor.Draw(_spriteBatch);
        }
    }
}
public class InShip : GameState
{
    public override void Initialize()
    {
        Engine.UIManager.ScreenWindow.enabled = false;
        EventHandler.DisableDockingMenus();
    }
    public override void Update() 
    {
        EntityManager.IngameUpdate();
        ParticleManager.Update();
        if (Input.OldState.IsKeyUp(Keys.F) && Input.NewState.IsKeyDown(Keys.F))
        {
            CurrentGameState.SwitchState(new PlayingGame());
            Engine.UIManager.GetContainer((int)Containers.FuseMenu).enabled = false;
        }
    }
    public override void Draw(SpriteBatch _spriteBatch) { }
}

