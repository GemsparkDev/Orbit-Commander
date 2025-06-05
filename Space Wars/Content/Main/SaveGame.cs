using Space_Wars.Content.Main.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UILib.Content.Main;

namespace Space_Wars.Content.Main;
public class SaveGame
{
    public Player Player { get; } = new Player(Vector2.Zero, Vector2.Zero, 0, 0);
    public int Scrap { get; set; }
}
