using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrbitCommander.Entities;
using OrbitCommander.Particles;
using System.Collections.Generic;
using OrbitCommander.Core;

namespace OrbitCommander.Components;
public class Friendly(Entity _entity) : IComponent
{
    public Team Team { get; set; }
    public static Team[] Blacklist(Team _team)
    {
        List<Team> teams = [Team.Friendly, Team.Hostile, Team.Dead];
        teams.Remove(_team);
        return [.. teams];
    }
    public void Draw(SpriteBatch _spriteBatch)
    {
        Vector2 halfSize = Engine.BackBuffer / 2;
        if (_entity.HasComponent<Health>() && !(_entity.HasTag(Tags.IsChild)) && Engine.SaveGame.Player.Team == Team &&
           (_entity.Position.X - Engine.Camera.Position.X + _entity.Size.X / 2 < -halfSize.X || _entity.Position.Y - Engine.Camera.Position.Y + _entity.Size.Y / 2 < -halfSize.Y
         || _entity.Position.X - Engine.Camera.Position.X - _entity.Size.X / 2 > halfSize.X || _entity.Position.Y - Engine.Camera.Position.Y - _entity.Size.Y / 2 > halfSize.Y))
        {
            var pos = _entity.Position - Engine.SaveGame.Player.Position;
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), pos / 50 + Engine.SaveGame.Player.Position, 0, _entity.Color * 0.67f));
        }
    }
}
public enum Team
{
    Friendly,
    Hostile,
    Dead,
}