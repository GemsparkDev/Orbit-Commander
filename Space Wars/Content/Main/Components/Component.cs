using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main.Components;

public abstract class Component(Entity _entity)
{
    public Entity Entity { get; } = _entity;
    public virtual void Update() { }
    public virtual void Draw(SpriteBatch _spriteBatch) { }
}
