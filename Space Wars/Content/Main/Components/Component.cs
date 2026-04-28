using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main.Components;

public abstract class Component
{
    public Component() { }
    public Component(Entity _entity)
    {
        Entity = _entity;
    }
    public Entity Entity { get; set; }
    public virtual void Update() { }
    public virtual void Draw(SpriteBatch _spriteBatch) { }
}
