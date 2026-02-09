using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UILib.Content.Main;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Space_Wars.Content.Main.UIElements;
public class Fuse : IData
{
    public Texture2D Texture { get; private set; } = Assets.Get(Sprite.Fuse);
    public Window Tooltip { get; private set; }
    public int ID { get; private set; } = 0;
    public Color Color { get; private set; }
    public Fuse(Color _color)
    {
        Color = _color;
    }
}
