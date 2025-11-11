using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Space_Wars.Content.Main;
public abstract class ColorScheme
{
    public abstract Color Background();
    public abstract Color FriendlyEnemy();
    public abstract Color FriendlyProjectile();
    public abstract Color HostileEnemy();
    public abstract Color Environment();
    public abstract bool IsOutlined();
}
public class StandardScheme : ColorScheme
{
    public override Color Background() { return Color.Black; }
    public override Color FriendlyEnemy() { return new Color(0f, 1f, 0f); }
    public override Color FriendlyProjectile() { return Color.Orange; }
    public override Color HostileEnemy() { return Color.Red; }
    public override Color Environment() { return Color.Cyan; }
    public override bool IsOutlined() { return false; }

}
public class FinaleScheme : ColorScheme
{
    public override Color Background() { return Color.White; }
    public override Color FriendlyEnemy() { return Color.Black; }
    public override Color FriendlyProjectile() { return Color.Black; }
    public override Color HostileEnemy() { return Color.Red; }
    public override Color Environment() { return Color.Gray; }
    public override bool IsOutlined() { return true; }
}
