using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UILib.Content;
using OrbitCommander.Core;

namespace OrbitCommander.UIElements;
public class TerminalSlider(Texture2D _line, Texture2D _knob, Vector2 _offset, Vector2 _sliderSize, bool _visualSlider, Color[] _colors) : Slider(_line, _knob, _offset, _sliderSize, _visualSlider, _colors)
{
    public override void HoveringDraw(SpriteBatch _spriteBatch, Vector2 _parentPosition, float _transparency, Vector2 _center) 
    {
        if (!(visualSlider || _knob == null))
        {
            Vector2 knobPosition = _parentPosition + Offset - sliderSize / 2 * UIManager.UIScale + new Vector2((int)(sliderSize.X * Intervals[0]), (int)(sliderSize.Y / 2)) * UIManager.UIScale - _center;
            _spriteBatch.Draw(Engine.Line, knobPosition, new Rectangle((int)knobPosition.X, (int)knobPosition.Y, _knob.Width, _knob.Height), Color.White, 0, UIManager.DimsOf(_knob) / 2, UIManager.UIScale / 2, 0, 0);
        }
        base.HoveringDraw(_spriteBatch, _parentPosition, _transparency, _center);
    }
}
