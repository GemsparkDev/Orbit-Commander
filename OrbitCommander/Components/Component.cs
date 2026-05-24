using Microsoft.Xna.Framework.Graphics;

namespace OrbitCommander.Components;

public abstract class Component()
{
    public virtual void Update() { }
    public virtual void Draw(SpriteBatch _spriteBatch) { }
}
