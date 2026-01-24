using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using UILib.Content.Main;

namespace Space_Wars.Content.Main;
public class Dial : FunctionalWidget
{
    private List<Action> behaviours = [];
    public float Target { get; set; } = 0;
    private Texture2D dialTexture;
    private float currentVal = 0;
    public Window Tooltip { get; private set; }
    public Dial(Texture2D _dialTexture, Vector2 _offset, Texture2D _texture)
    {
        dialTexture = _dialTexture;
        Size = UIManager.DimsOf(_texture);
        offset = _offset;
        texture = _texture;
    }
    public override void Interact(Vector2 parentPosition)
    {
        ApplyBehaviours();
    }
    public void AddTooltip(Window _tooltip)
    {
        Tooltip ??= _tooltip;
    }
    public override void ContinuousInteract(Vector2 parentPosition) { }
    public override void AddBehaviour(Action func)
    {
        behaviours.Add(func);
    }
    public override void ApplyBehaviours()
    {
        for (int i = 0; i < behaviours.Count; i++)
        {
            behaviours[i]();
        }
    }
    public override void Draw(SpriteBatch _spriteBatch, Vector2 _parentPosition, float _transparency, Vector2 _center)
    {
        _spriteBatch.Draw(dialTexture, _parentPosition + Offset - _center, null, color * _transparency, (2 * currentVal - 1) * MathF.PI/4, Size / 2, UIManager.UIScale, 0, 0);
        base.Draw(_spriteBatch, _parentPosition, _transparency, _center);
    }
    public override void HoveringDraw(SpriteBatch _spriteBatch)
    {
        if (Tooltip == null)
        {
            return;
        }
        MouseState newState = Mouse.GetState();
        Texture2D tex = Tooltip.texture;
        Tooltip.position = new Vector2(newState.Position.X, newState.Position.Y) + new Vector2(tex.Width, tex.Height) / 2 * UIManager.UIScale;
        Tooltip.Draw(_spriteBatch);
    }
    public override void Update() 
    {
        float lerp = Util.FIED(0.05f);
        currentVal = currentVal * lerp + (Target + Util.OneToNegOne() * 0.05f) * (1 - lerp);
    }
}
