using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;
using System.Collections.Generic;

namespace Space_Wars.Content.Main.Components;
public class Friendly(Entity _entity) : Component(_entity)
{
    public Team Team { get; set; }
    public static Team[] Blacklist(Team _team)
    {
        List<Team> teams = [Team.Friendly, Team.Hostile, Team.Dead];
        teams.Remove(_team);
        return [.. teams];
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        Vector2 halfSize = Engine.BackBuffer / 2;
        if (!Entity.HasComponent<IsChild>() && Engine.SaveGame.Player.Team == Team &&
           (Entity.Position.X - Engine.Camera.Position.X + Entity.Size.X / 2 < -halfSize.X || Entity.Position.Y - Engine.Camera.Position.Y + Entity.Size.Y / 2 < -halfSize.Y
         || Entity.Position.X - Engine.Camera.Position.X - Entity.Size.X / 2 > halfSize.X || Entity.Position.Y - Engine.Camera.Position.Y - Entity.Size.Y / 2 > halfSize.Y))
        {
            var pos = Entity.Position - Engine.SaveGame.Player.Position;
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), pos / 50 + Engine.SaveGame.Player.Position, 0, Entity.Color * 0.67f));
        }
    }
}
public enum Team
{
    Friendly,
    Hostile,
    Dead,
}