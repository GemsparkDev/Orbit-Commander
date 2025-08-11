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
    //Useful if several game states want to render the same game space
    protected static void RenderGamespace(SpriteBatch _spriteBatch)
    {
        Engine.EntityManager.Draw(_spriteBatch);
        ParticleManager.Draw(_spriteBatch);
        if (Engine.DebugMode)
        {
            Vector2 cameraPos = Engine.Camera.Position + Engine.MousePositionOffset;
            for (int x = (int)Math.Ceiling((cameraPos.X - Engine.ScreenSize.X / 2) / 50); x < (cameraPos.X + Engine.ScreenSize.X / 2) / 50; x++)
            {
                Color col = ((x % 10 == 0) ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.1f, 0.1f, 0.1f));
                _spriteBatch.Draw(Engine.Line, new Vector2(x * 50, cameraPos.Y - Engine.ScreenSize.Y / 2), new Rectangle(x * 50, 0, 1, (int)Engine.ScreenSize.Y), col, 0, Vector2.Zero, 1, 0, 0);
            }
            for (int y = (int)Math.Ceiling((cameraPos.Y - Engine.ScreenSize.Y / 2) / 50); y < (cameraPos.Y + Engine.ScreenSize.Y / 2) / 50; y++)
            {
                Color col = ((y % 10 == 0) ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.1f, 0.1f, 0.1f));
                _spriteBatch.Draw(Engine.Line, new Vector2(cameraPos.X - Engine.ScreenSize.X / 2, y * 50), new Rectangle(0, y * 50, (int)Engine.ScreenSize.X, 1), col, 0, Vector2.Zero, 1, 0, 0);
                if (y % 10 == 0)
                {
                    for (int x = (int)Math.Ceiling((cameraPos.X - Engine.ScreenSize.X / 2) / 500) * 10; x < (cameraPos.X + Engine.ScreenSize.X / 2) / 50; x += 10)
                    {
                        var text = $"{x},{y}";
                        _spriteBatch.DrawString(Assets.TextFont, text, new Vector2(x * 50, y * 50), Color.White, 0, Assets.TextFont.MeasureString(text)/2, 1, 0, 0);
                    }
                }
            }
        }
    }
}
public class MainMenu : GameState
{
    private Planet menuPlanet = new(new Vector2(0, 750), Vector2.Zero, 5000, 9, true, Color.Cyan);
    private Planet moonPlanet = new(new Vector2(0, 1750), Planet.GetOrbitalVelocity(new Vector2(0, 1750), new Vector2(0, 750), 5000), 250, 1.5f, false, Color.Cyan);
    private ParticleEmitter smokeParticles = new(Assets.Get(Sprite.Circle), 1f, new Vector2(0, 300 - Assets.DimsOf(Sprite.Mothership).Y + 10), 0, MathF.PI/4, 1, 40, Color.Gray, EmitterType.EmissionOverTime) 
    { particleFadeToColor = new Color(169, 169, 169, 0), probability = 0.25f };
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
        Engine.EntityManager.IngameUpdate();
        Engine.SaveGame.Player.RestrictedActions();
        ParticleManager.Update();
        if (Input.OldState.IsKeyUp(Keys.Escape) && Input.NewState.IsKeyDown(Keys.Escape))
        {
            if (Engine.UIManager.ToggleToMenu(UI.PauseMenu))
            {
                SoundManager.SetAllSounds(false);
                CurrentGameState.SwitchState(new PausedGame());
            }
        }
        if (!Engine.Self.IsActive)
        {
            Engine.UIManager.DisableAll();
            UI.PauseMenu.enabled = true;
            CurrentGameState.SwitchState(new PausedGame());
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
            if (Engine.UIManager.ToggleToMenu(UI.PauseMenu))
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
        (200, [], 0), (160, [0], 0), (140, [0], 0), (100, [1, 2], 0), (400, [3], 0), (50, [3], 0),
        (210, [], 1), (170, [6], 1), (145, [7], 1), (130, [8], 1), (150, [9], 1),
        (200, [], 2), (150, [11], 2), (100, [12], 2)
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
        Engine.Camera.Position = Vector2.Zero;
        Engine.MousePositionOffset = Vector2.Zero;
        ParticleManager.Initialize();
        EventHandler.UpdateModulesUI();
        EventHandler.UpdateMissionText();
        EventHandler.UpdateInventoryUI();
    }
    public override void Update() 
    {
        for (int i = 0; i < Engine.SaveGame.QueuedItems.Count; i++)
        {
            if (Engine.SaveGame.QueuedItems[i].IsExpired)
            {
                Engine.SaveGame.QueuedItems.RemoveAt(i);
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
                    Engine.SaveGame.SetMission(i);
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
        for(int i = 0; i < Engine.SaveGame.QueuedItems.Count; i++)
        {
            var item = Engine.SaveGame.QueuedItems[i];
            var texture = item.Texture;
            var pos = (new Vector2(20, 20) + new Vector2(30, 0) * i) * UIManager.UIScale - Engine.ScreenSize / 2;
            _spriteBatch.Draw(texture, pos, null, Color.White, 0, new Vector2(texture.Width, texture.Height) / 2, UIManager.UIScale, 0, 0);
            for (float j = 0; j <= MathF.Tau * ((float)item.Cost / (float)item.MaxCost) + float.Epsilon; j+= MathF.Tau/(3 * UIManager.UIScale * UIManager.UIScale))
            {
                _spriteBatch.Draw(Assets.Get(Sprite.Dot), Engine.ToUnitVector(j) * texture.Height / 1.5f * UIManager.UIScale + pos, null, Color.White, j, Assets.DimsOf(Sprite.Dot)/2, UIManager.UIScale, 0, 0);
            }
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
    private float escapeTime = 0;
    public override void Initialize()
    {
        time = 0;
        Engine.UIManager.ScreenWindow.enabled = false;
    }
    public override void Update()
    {
        if (Input.NewState.IsKeyDown(Keys.Escape))
        {
            if (escapeTime < 1)
            {
                escapeTime += Engine.DeltaSeconds;
            }
            else
            {
                time = 99999;
            }
        }
        else if (escapeTime > 0)
        {
            escapeTime -= Engine.DeltaSeconds;
        }
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
        _spriteBatch.DrawString(Assets.TextFont, "esc to skip", Engine.Camera.Position + Engine.ScreenSize / 2 - Assets.TextFont.MeasureString("esc to skip") / 2 - new Vector2(100, 100), Color.White * (0.5f + escapeTime * 0.5f));
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
            UI.FuseMenu.enabled = false;
        }
    }
    public override void Draw(SpriteBatch _spriteBatch) { }
}

