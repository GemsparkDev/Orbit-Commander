using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrbitCommander.Core;
using System;
using System.Collections.Generic;
using UILib.Content;
using System.Diagnostics;

namespace OrbitCommander.UIElements;
public class Dial : FunctionalWidget
{
    public float Target { get; set; } = 0; //0 to 1
    private Texture2D dialTexture;
    public float Value { get; private set; } = 0;
    public float Range { get; set; } = MathF.PI;
    public bool IsTurnable { get; set; } = true;
    public Window Tooltip { get; private set; }
    public Dial(Texture2D _handTexture, Vector2 _offset, Texture2D _bodyTexture, Container _container, int[] _index) : base(_container, _index)
    {
        dialTexture = _handTexture;
        offset = _offset;
        Texture = _bodyTexture;
    }
    public void AddTooltip(Window _tooltip)
    {
        Tooltip ??= _tooltip;
    }
    public override void Draw(SpriteBatch _spriteBatch, Vector2 _parentPosition, float _transparency, Vector2 _center)
    {
        base.Draw(_spriteBatch, _parentPosition, _transparency, _center);
        _spriteBatch.Draw(dialTexture, _parentPosition + Offset - _center, null, color * _transparency, (2 * Value - 1) * Range, new Vector2(dialTexture.Width, dialTexture.Height) / 2, UIManager.UIScale, 0, 0);
    }
    public override void OnContinuousInteract(Vector2 _clickPosition)
    {
        if(!IsTurnable)
        {
            return;
        }
        float next = Math.Clamp((Util.ToAngle(_clickPosition) / (Range * 2) + 0.5f), 0, 1);
        if(MathF.Abs(Target - next) < 0.5f)
        {
            Target = next;
        }
        else
        {
            Target = MathF.Round(Target);
        }
        base.OnContinuousInteract(_clickPosition);
    }
    public override void HoveringDraw(SpriteBatch _spriteBatch, Vector2 _parentPosition, float _transparency, Vector2 _center)
    {
        if (Tooltip == null)
        {
            return;
        }
        Tooltip.Position = Engine.Cursor.HandPosition + Tooltip.Size / 2 * UIManager.UIScale;
        Tooltip.Draw(_spriteBatch);
    }
    public override void Update()
    {
        float lerp = Util.FIED(0.05f);
        if(IsTurnable)
        {
            Value = Value * lerp + Target * (1 - lerp);
        }
        else
        {
            Value = Value * lerp + (Target + Util.OneToNegOne() * 0.08f) * (1 - lerp);
        }
    }
}
