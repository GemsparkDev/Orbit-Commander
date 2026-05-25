using Microsoft.Xna.Framework.Graphics;

namespace OrbitCommander.Components;

public interface IComponent
{
    public virtual void Update() { }
    public virtual void Draw(SpriteBatch _spriteBatch) { }
}
