using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrbitCommander.Core;
using UILib.Content;

namespace OrbitCommander.UIElements;
public class Fuse(Color _color) : IData
{
    public Texture2D Texture { get; private set; } = Assets.Get(Sprites.Fuse);
    public Window Tooltip { get; private set; }
    public int ID { get; private set; } = 0;
    public Color Color { get; private set; } = _color;
}
