using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Runtime.Intrinsics;

namespace Space_Wars.Content.Main.UI_Elements
{
    public class Slider : Widget, IFunctional
    {
        private List<DelegateMethod> behaviours = new();
        public float sliderInterval = 0.5f;
        private Vector2 sliderSize = new Vector2(100, 2);
        public bool visualSlider;
        public Color enabledColor;
        public Color disabledColor;
        public Slider(Vector2 _offset, float _sliderWidth, bool _visualSlider)
        {
            texture = null;
            if(_visualSlider == false)
            {
                size = new Vector2(sliderSize.X, Assets.Sprites["Knob"].Height);
            }
            else
            {
                size = sliderSize;
            }
            offset = _offset;
            sliderSize.X = _sliderWidth;
            visualSlider = _visualSlider;
            enabledColor = Color.Green;
            disabledColor = Color.Red;
        }
        public Slider(Vector2 _offset, float _sliderWidth, bool _visualSlider, Color _enabledColor, Color _disabledColor)
        {
            texture = null;
            if (_visualSlider == false)
            {
                size = new Vector2(sliderSize.X, Assets.Sprites["Knob"].Height);
            }
            else
            {
                size = sliderSize;
            }
            offset = _offset;
            sliderSize.X = _sliderWidth;
            visualSlider = _visualSlider;
            enabledColor = _enabledColor;
            disabledColor = _disabledColor;
        }
        public void SetInterval(float _value, float _maxValue)
        {
            sliderInterval = _value/_maxValue;

            if(sliderInterval > 1)
            {
                sliderInterval = 1;
            }
            if(sliderInterval < 0)
            {
                sliderInterval = 0;
            }
        }
        public void Interact(Vector2 parentPosition)
        {

        }
        public void ContinuousInteract(Vector2 parentPosition)
        {
            if(visualSlider == false)
            {
                SetInterval(Mouse.GetState().X - offset.X - parentPosition.X, sliderSize.X);
            }
        }
        public void AddBehaviour(DelegateMethod func)
        {
            behaviours.Add(func);
        }
        public void ApplyBehaviours()
        {
            for (int i = 0; i < behaviours.Count; i++)
            {
                DelegateMethod func = behaviours[i];
                func();
            }
        }
        public override void Initialize() { }
        public override void Draw(SpriteBatch _spriteBatch, Vector2 _parentPosition)
        {
            Vector2 centeringVector = new Vector2(0, Size.Y/2);
            Vector2 knobPosition = offset + _parentPosition + centeringVector + new Vector2((int)(sliderSize.X * sliderInterval), 0) - new Vector2(Assets.Sprites["Knob"].Width / 2, Assets.Sprites["Knob"].Height / 2);
            _spriteBatch.Draw(Engine.line, offset + _parentPosition + centeringVector, new Rectangle(0, 0, (int)(sliderSize.X), 2),
                disabledColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            _spriteBatch.Draw(Engine.line, offset + _parentPosition + centeringVector, new Rectangle(0, 0, (int)(sliderSize.X * sliderInterval), 2),
                enabledColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            if (visualSlider == false)
            {
                _spriteBatch.Draw(Assets.Sprites["Knob"], knobPosition, null, Color.White);
            }
        }
    }
}
