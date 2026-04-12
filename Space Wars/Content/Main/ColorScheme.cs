using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Space_Wars.Content.Main.Components;

namespace Space_Wars.Content.Main;
public abstract class ColorScheme
{
    public abstract Color Background();
    public Dictionary<Team, Color> TeamColors { get; protected set; } = [];
    public abstract Color Environment();
    public abstract bool IsOutlined();
}
public class StandardScheme : ColorScheme
{
    public override Color Background() { return Color.Black; }
    public override Color Environment() { return Color.Cyan; }
    public override bool IsOutlined() { return false; }
    public StandardScheme()
    {
        TeamColors.Add(Team.Friendly, new Color(0f, 1f, 0f));
        TeamColors.Add(Team.Hostile, Color.Red);
        TeamColors.Add(Team.Dead, Color.Gray);
    }
}
public class FinaleScheme : ColorScheme
{
    public override Color Background() { return Color.White; }
    public override Color Environment() { return Color.Gray; }
    public override bool IsOutlined() { return true; }
    public FinaleScheme()
    {
        TeamColors.Add(Team.Friendly, Color.Black);
        TeamColors.Add(Team.Hostile, Color.Red);
        TeamColors.Add(Team.Dead, Color.Gray);   
    }
}
