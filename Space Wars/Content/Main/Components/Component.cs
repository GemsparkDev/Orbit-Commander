using System;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main.Components;

public abstract class Component(Entity _entity, ComponentType _type)
{
    public Entity entity = _entity;
    public ComponentType Type { get; } = _type;
    public virtual void Update() { }
}
