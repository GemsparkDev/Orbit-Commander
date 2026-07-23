using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UILib.Content;

namespace OrbitCommander.UIElements;
internal class TerminalSlider : Slider
{
    Texture2D knob = null;
    public TerminalSlider(Texture2D _line, Texture2D _knob, Vector2 _offset, Vector2 _sliderSize, bool _visualSlider, Color[] _colors) : base(_line, _knob, _offset, _sliderSize, _visualSlider, _colors)
    {
        knob = _knob;
    }
    public override void HoveringDraw(SpriteBatch _spriteBatch) 
    {
        if (!(visualSlider || knob == null))
        {
            Vector2 knobPosition = _parentPosition + Offset - sliderSize / 2 * UIManager.UIScale + new Vector2((int)(sliderSize.X * Intervals[0]), (int)(sliderSize.Y / 2)) * UIManager.UIScale - _center;
            _spriteBatch.Draw(knob, knobPosition, null, Color.White, 0, UIManager.DimsOf(knob) / 2, UIManager.UIScale / 2, 0, 0);
        }
    }
}
