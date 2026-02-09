using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UILib.Content.Main;

namespace Space_Wars.Content.Main.UIElements;
public class Stack<T> : FunctionalWidget where T : IData
{
    public int Count { get; set; }
    public Texture2D StackTexture { get; private set; }
    private Vector2 stackOffset;
    private Vector2 stackDepth;
    private Func<T> constructor;
    private List<Action> behaviours = [];
    public Stack(Vector2 _offset, Texture2D _texture, int _count, Texture2D _stackTexture, Vector2 _stackOffset, Vector2 _stackDepth, Func<T> _constructor)
    {
        offset = _offset;
        Texture = _texture;
        Count = _count;
        StackTexture = _stackTexture;
        stackOffset = _stackOffset;
        stackDepth = _stackDepth;
        constructor = _constructor;
    }
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
    public override void ContinuousInteract(Vector2 parentPosition)
    {

    }

    public override void HoveringDraw(SpriteBatch _spriteBatch)
    {

    }

    public override void Interact(Vector2 parentPosition)
    {
        if(Engine.UIManager.selectedIcon == null && Count > 0)
        {
            Engine.UIManager.selectedIcon = constructor();
            Count--;
        }
        else
        {
            if(Engine.UIManager.selectedIcon is T)
            {
                Engine.UIManager.selectedIcon = null;
                Count++;
            }
        }
        ApplyBehaviours();
    }
    public override void Draw(SpriteBatch _spriteBatch, Vector2 _parentPositon, float _transparency, Vector2 _center)
    {
        base.Draw(_spriteBatch, _parentPositon, _transparency, _center);
        for (int i = Count - 1; i >= 0; i--)
        {
            _spriteBatch.Draw(StackTexture, _parentPositon + Offset - _center + (stackOffset + stackDepth * (float)(i)) * UIManager.UIScale, null, Color.White, 0, UIManager.DimsOf(StackTexture) / 2, UIManager.UIScale, 0, 0);
        }
    }
}
