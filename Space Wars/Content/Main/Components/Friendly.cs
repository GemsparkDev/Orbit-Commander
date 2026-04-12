using System.Collections.Generic;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main.Components;
public class Friendly(Entity _entity) : Component(_entity)
{
    public Team Team { get; set; }
    public static Team[] Blacklist(Team _team)
    {
        List<Team> teams = [Team.Friendly, Team.Hostile, Team.Dead];
        teams.Remove(_team);
        return teams.ToArray();
    }
}
public enum Team
{
    Friendly,
    Hostile,
    Dead,
}