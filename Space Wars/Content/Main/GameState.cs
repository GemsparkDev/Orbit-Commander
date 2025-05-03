using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;
using UILib.Content.Main;

namespace Space_Wars.Content.Main;

public static class CurrentGameState
{
    private static GameState currentGameState;
    private static Engine root;
    public static void SwitchState(GameState _gameState)
    {
        SoundManager.SetAllSounds(false);
        currentGameState = _gameState;
        currentGameState.Initialize(root);
    }
    public static void Initialize(Engine _root)
    {
        root = _root;
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
    protected Engine root;
    public virtual void Initialize(Engine _root) { root = _root; }
    public abstract void Update();
    public abstract void Draw(SpriteBatch _spriteBatch);
}

public class MainMenu : GameState
{
    private GravitationalSource menuPlanet = new(new Vector2(0, 750), Vector2.Zero, 5000, 9, true, Color.Cyan);
    private GravitationalSource moonPlanet = new(new Vector2(0, 1750), GravitationalSource.GetOrbitalVelocity(new Vector2(0, 1750), new Vector2(0, 750), 5000), 250, 1.5f, false, Color.Cyan);
    private ParticleEmitter smokeParticles = new(Assets.Get(Sprite.Circle), 1f, new Vector2(0, 300 - Assets.DimsOf(Sprite.Mothership).Y + 10),
        0, 45, 1, 0, 10, 1, true, Color.Gray, Color.DarkGray, EmitterType.EmissionOverTime);
    public override void Initialize(Engine _root)
    {
        base.Initialize(_root);
        menuPlanet.RenderSurface();
        moonPlanet.RenderSurface();
        ParticleManager.Add(smokeParticles);
        smokeParticles.isEmitterActive = true;
        Engine.UIManager.SetScreenMenuEnabled(false);
        SoundManager.ChangeTrack(Assets.Get(Sound.menu));
    }
    public override void Update()
    {
        menuPlanet.Update();
        moonPlanet.velocity += menuPlanet.GetAcceleration(moonPlanet.position);
        moonPlanet.Update();
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
    public override void Initialize(Engine _root)
    {
        base.Initialize(_root);
        Engine.UIManager.SetScreenMenuEnabled(true);
    }
    public override void Update()
    {
        if (!root.IsActive)
        {
            Engine.UIManager.DisableAll();
            Engine.UIManager.GetContainer((int)Containers.PauseMenu).enabled = true;
            CurrentGameState.SwitchState(new PausedGame());
        }
        EntityManager.PlayerUpdate();
        EntityManager.IngameUpdate();
        EntityManager.Update();
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
        EntityManager.Draw(_spriteBatch);
        ParticleManager.Draw(_spriteBatch);
        if (Engine.DebugMode == true)
        {
            //Generates a grid
            for (int x = (int)(Engine.Camera.Position.X - Engine.ScreenSize.X / 2) / 50; x < (Engine.Camera.Position.X + Engine.ScreenSize.X/2) / 50; x++)
            {
                _spriteBatch.Draw(Engine.Line, new Vector2(x * 50 - Engine.mousePositionOffset.X, Engine.Camera.Position.Y - Engine.ScreenSize.Y/2), new Rectangle((int)(x * 50), 0, 1, (int)Engine.ScreenSize.Y), Color.Gray * 0.5f, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                _spriteBatch.DrawString(Assets.TextFont, ((int)x).ToString(), new Vector2(x * 50 - Engine.mousePositionOffset.X, Engine.Camera.Position.Y - Engine.ScreenSize.Y / 2), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
            }
            for (int y = (int)(Engine.Camera.Position.Y - Engine.ScreenSize.X / 2) / 50 - 1; y < (Engine.Camera.Position.Y + Engine.ScreenSize.Y/2) / 50 + 1; y++)
            {
                _spriteBatch.Draw(Engine.Line, new Vector2(Engine.Camera.Position.X - Engine.ScreenSize.X / 2, y * 50 - Engine.mousePositionOffset.Y), new Rectangle(0, (int)(y * 50), (int)Engine.ScreenSize.X, 1), Color.Gray * 0.5f, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                _spriteBatch.DrawString(Assets.TextFont, ((int)y).ToString(), new Vector2(Engine.Camera.Position.X - Engine.ScreenSize.X / 2, y * 50 - Engine.mousePositionOffset.Y), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
            }
        }
    }
}
public class PausedGame : GameState
{
    public Engine Root { get; set; }
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
        EntityManager.Draw(_spriteBatch);
        ParticleManager.Draw(_spriteBatch);
        if (Engine.DebugMode == true)
        {
            //Generates a grid
            for (int x = (int)(Engine.Camera.Position.X - Engine.ScreenSize.X / 2) / 50; x < (Engine.Camera.Position.X + Engine.ScreenSize.X / 2) / 50; x++)
            {
                _spriteBatch.Draw(Engine.Line, new Vector2(x * 50 - Engine.mousePositionOffset.X, Engine.Camera.Position.Y - Engine.ScreenSize.Y / 2), new Rectangle((int)(x * 50), 0, 1, (int)Engine.ScreenSize.Y), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                _spriteBatch.DrawString(Assets.TextFont, ((int)x).ToString(), new Vector2(x * 50 - Engine.mousePositionOffset.X, Engine.Camera.Position.Y - Engine.ScreenSize.Y / 2), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
            }
            for (int y = (int)(Engine.Camera.Position.Y - Engine.ScreenSize.X / 2) / 50 - 1; y < (Engine.Camera.Position.Y + Engine.ScreenSize.Y / 2) / 50 + 1; y++)
            {
                _spriteBatch.Draw(Engine.Line, new Vector2(Engine.Camera.Position.X - Engine.ScreenSize.X / 2, y * 50 - Engine.mousePositionOffset.Y), new Rectangle(0, (int)(y * 50), (int)Engine.ScreenSize.X, 1), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
                _spriteBatch.DrawString(Assets.TextFont, ((int)y).ToString(), new Vector2(Engine.Camera.Position.X - Engine.ScreenSize.X / 2, y * 50 - Engine.mousePositionOffset.Y), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.4f);
            }
        }
    }
}
public class Garage : GameState
{
    public override void Initialize(Engine _root)
    {
        EventHandler.UpdateModulesUI();
        Engine.UIManager.SetScreenMenuEnabled(false);
        base.Initialize(_root);
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
        EntityManager.Draw(_spriteBatch);
        ParticleManager.Draw(_spriteBatch);
    }
}
public class MissionSelect : GameState
{
    public override void Initialize(Engine _root)
    {
        base.Initialize(_root);
        Engine.UIManager.SetScreenMenuEnabled(false);
        EventHandler.UpdateMissionText();
    }
    public override void Update() { }
    public override void Draw(SpriteBatch _spriteBatch) { }
}
public class Victory : GameState
{
    public override void Initialize(Engine _root)
    {
        base.Initialize(_root);
        Engine.UIManager.SetScreenMenuEnabled(false);
    }
    public override void Update() { }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        _spriteBatch.DrawString(Assets.TextFont, "You Win!", new Vector2(-12 * 8, -60) * Engine.UIScale + Engine.Camera.Position, Color.Yellow, 0, Vector2.Zero, Engine.UIScale * 2, SpriteEffects.None, 0);
        _spriteBatch.DrawString(Assets.TextFont, $"Your Time: {Engine.ingameTime.DrawText}", new Vector2(-12 * 12 / 2, (12 * 4 - 60)) * Engine.UIScale + Engine.Camera.Position, Color.White, 0, Vector2.Zero, Engine.UIScale/2, SpriteEffects.None, 0);
    }
}
public class TrainingMode : GameState
{
    public override void Update()
    {
        EntityManager.PlayerUpdate();
        EntityManager.Update();
        ParticleManager.Update();
        EntityManager.TrainingSimulator.Update();
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        EntityManager.TrainingSimulator.Draw(_spriteBatch);
        EntityManager.Draw(_spriteBatch);
        ParticleManager.Draw(_spriteBatch);
    }
}
