using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UILib.Content;
using OrbitCommander.Core;
using System.Diagnostics;

namespace OrbitCommander.UIElements;
public class TerminalButton(Vector2 _offset, SpriteFont _textFont, string _text, Color _textColor, float textSize) : Button(_offset, _textFont, _text, _textColor, textSize)
{
    private bool isOver = false;
    private Color previousColor;
    public override void Draw(SpriteBatch _spriteBatch, Vector2 _parentPosition, float _transparency, Vector2 _center)
    {
        if(isOver)
        {
            Vector2 pos = _parentPosition + Offset - _center - Size/2 * UIManager.UIScale;
            _spriteBatch.Draw(Engine.Line, new Rectangle((int)pos.X, (int)pos.Y, (int)(Size.X * UIManager.UIScale), (int)(Size.Y * UIManager.UIScale * 1.25f)), Color.White);
        }
        base.Draw(_spriteBatch, _parentPosition, _transparency, _center);
        if(isOver)
        {
            TextColor = previousColor;
            isOver = false;
        }
    }
    public override void HoveringDraw(SpriteBatch _spriteBatch, Vector2 _parentPosition, float _transparency, Vector2 _center)
    {
        base.HoveringDraw(_spriteBatch, _parentPosition, _transparency, _center);
        if (!isOver)
        {
            isOver = true;
            previousColor = TextColor;
            TextColor = Color.Black;
        }
    }
}
