using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;
using System.Collections.Generic;
using UILib.Content.Main;
using System;

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
    private Player Player => Engine.SaveGame.Player;
    //Useful if several game states want to render the same game space
    internal void RenderGamespace(SpriteBatch _spriteBatch)
    {
        if (Player.modules[ModuleType.Core].isFailed)
        {
            string text = "Power failure detected. Please restart system.";
            Vector2 middlePoint = Assets.TextFont.MeasureString(text) / 2;
            _spriteBatch.DrawString(Assets.TextFont, text, Engine.Camera.Position - middlePoint, Color.Red);
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
            Vector2 screen = Engine.ScreenSize / 16;
            for (int i = (int)(-screen.X); i < screen.X + 1; i++)
            {
                for (int j = (int)(-screen.Y); j < screen.Y + 1; j++)
                {
                    Vector2 pos = Engine.Camera.Position + new Vector2(i, j) * 8;
                    float col = 0;
                    float strength = Engine.EntityManager.CurrentMission.GetNormalizedAcceleration(pos).Length() * 30;
                    int nearestInt = (int)Math.Round(strength);
                    if (Math.Abs(strength - nearestInt) < 0.01f * strength)
                    {
                        col = 1;
                    }
                    _spriteBatch.Draw(Assets.Get(Sprite.Dot), pos, new Color(1f, 1f - strength / 25f, 1f - strength / 25f) * col);
                }
            }
        }
    }
}

public class MainMenu : GameState
{
    private GravitationalSource menuPlanet = new(new Vector2(0, 750), Vector2.Zero, 5000, 9, true, Color.Cyan);
    private GravitationalSource moonPlanet = new(new Vector2(0, 1750), GravitationalSource.GetOrbitalVelocity(new Vector2(0, 1750), new Vector2(0, 750), 5000), 250, 1.5f, false, Color.Cyan);
    private ParticleEmitter smokeParticles = new(Assets.Get(Sprite.Circle), 1f, new Vector2(0, 300 - Assets.DimsOf(Sprite.Mothership).Y + 10),
        0, 45, 1, 0, 40, Color.Gray, new Color(169, 169, 169, 0), EmitterType.EmissionOverTime)
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
    }
    public override void Update()
    {
        if (!Engine.Self.IsActive)
        {
            Engine.UIManager.DisableAll();
            Engine.UIManager.GetContainer((int)Containers.PauseMenu).enabled = true;
            CurrentGameState.SwitchState(new PausedGame());
        }
        Engine.EntityManager.PlayerUpdate();
        Engine.EntityManager.IngameUpdate();
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
        Engine.EntityManager.IngameUpdate();
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
    private float time = Engine.Random.NextSingle() * 1000f;
    private List<(float distance, List<int> prerequisites, int system)> missions =
    [
        (200, [], 0), (160, [0], 0), (140, [0], 0), (100, [1, 2], 0),
        (210, [3], 1), (170, [4], 1), (145, [5], 1), (130, [5], 1), (50, [6], 1)
    ];
    private Vector2 playerPosition;
    private List<(int system, ParticleEmitter orbit)> missionOrbits = [];
    private ParticleEmitter sun = new (Assets.Get(Sprite.Dot), new Vector2(Engine.ScreenSize.X / 6, 0), 20, new Color(255, 255, 0));
    public MissionSelect()
    {
        var center = new Vector2(Engine.ScreenSize.X/6, 0);
        foreach (var (distance, _, system) in missions)
        {
            var orbit = (system, new ParticleEmitter(Assets.Get(Sprite.Dot), center, distance, new Color(0, 255, 255)));
            missionOrbits.Add(orbit);
        }
        var playerMission = missions[Engine.SaveGame.CurrentMissionIndex];
        float freq = MathF.Sqrt(playerMission.distance * playerMission.distance * playerMission.distance) / 100;
        playerPosition = new Vector2(Engine.ScreenSize.X / 6, 0) + new Vector2(MathF.Cos(time / freq), MathF.Sin(time / freq)) * playerMission.distance;
    }
    public override void Initialize()
    {
        Engine.UIManager.ScreenWindow.enabled = false;
        EventHandler.UpdateMissionText();
        Engine.Camera.Position = Vector2.Zero;
        Engine.MousePositionOffset = Vector2.Zero;
        ParticleManager.Initialize();
        EventHandler.UpdateModulesUI();
    }
    public override void Update() 
    {
        for (int i = 0; i < Engine.EntityManager.QueuedItems.Count; i++)
        {
            if (Engine.EntityManager.QueuedItems[i].IsExpired)
            {
                Engine.EntityManager.QueuedItems.RemoveAt(i);
            }
        }
        time += Engine.DeltaSeconds;
        ParticleManager.Update();
        var pos = new Vector2(Input.NewMouseState.Position.X, Input.NewMouseState.Position.Y);
        float distance = Vector2.Distance(pos, new Vector2(Engine.ScreenSize.X * 2 / 3, Engine.ScreenSize.Y/2));
        for(int i = 0; i < missions.Count; i++)
        {
            var mission = missions[i];
            float freq = MathF.Sqrt(mission.distance * mission.distance * mission.distance) / 100;
            pos = new Vector2(Engine.ScreenSize.X / 6, 0) + new Vector2(MathF.Cos(time / freq), MathF.Sin(time / freq)) * mission.distance;
            if (i == Engine.SaveGame.CurrentMissionIndex)
            {
                playerPosition = playerPosition * 0.95f + pos * 0.05f;
            }
            if (mission.system != Engine.SaveGame.System)
            {
                continue;
            }
            var color = new Color(0, 255, 255);
            bool canSelect = true;
            if (Engine.SaveGame.CompletedMissions[i])
            {
                color = new Color(255, 255, 0);
            }
            foreach (var prerequisite in mission.prerequisites)
            {
                if (!Engine.SaveGame.CompletedMissions[prerequisite])
                {
                    color = new Color(0, 100, 100);
                    canSelect = false;
                }
            }
            if (canSelect && Math.Abs(mission.distance - distance) < 10)
            {
                color = Color.White;
                if (Input.NewMouseState.LeftButton == ButtonState.Released && Input.OldMouseState.LeftButton == ButtonState.Pressed)
                {
                    Engine.EntityManager.SetMission(i);
                }
            }
            
            missionOrbits[i].orbit.particleColor = color;
            var orbit = missionOrbits[i];
            if (orbit.system == Engine.SaveGame.System)
            {
                ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), pos, 0, color));
                orbit.orbit.Update();
            }
        }
        sun.Update();
    }
    public override void Draw(SpriteBatch _spriteBatch) 
    {
        ParticleManager.Draw(_spriteBatch);
        if (Engine.SaveGame.System == missions[Engine.SaveGame.CurrentMissionIndex].system)
        {
            _spriteBatch.Draw(Assets.Get(Sprite.Miniplayer), playerPosition, null, new Color(0, 255, 0), 0, Vector2.Zero, 1, 0, 0);
        }
        for(int i = 0; i < Engine.EntityManager.QueuedItems.Count; i++)
        {
            var texture = Engine.EntityManager.QueuedItems[i].Texture;
            _spriteBatch.Draw(texture, (new Vector2(10, 10) + new Vector2(20, 0) * i) * UIManager.UIScale - Engine.ScreenSize/2, null, Color.White, 0, Vector2.Zero, UIManager.UIScale, 0, 0);
        }
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
        _spriteBatch.DrawString(Assets.TextFont, "You Win!", new Vector2(-12 * 8, -60) * UIManager.UIScale + Engine.Camera.Position, Color.Yellow, 0, Vector2.Zero, UIManager.UIScale * 2, SpriteEffects.None, 0);
        _spriteBatch.DrawString(Assets.TextFont, $"Your Time: {Engine.IngameTime.DrawText}", new Vector2(-12 * 12 / 2, (12 * 4 - 60)) * UIManager.UIScale + Engine.Camera.Position, Color.White, 0, Vector2.Zero, UIManager.UIScale /2, SpriteEffects.None, 0);
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
        Engine.EntityManager.IngameUpdate();
        ParticleManager.Update();
        if (Input.OldState.IsKeyUp(Keys.F) && Input.NewState.IsKeyDown(Keys.F))
        {
            CurrentGameState.SwitchState(new PlayingGame());
            Engine.UIManager.GetContainer((int)Containers.FuseMenu).enabled = false;
        }
    }
    public override void Draw(SpriteBatch _spriteBatch) { }
}

