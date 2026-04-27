using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main.Components;
internal class IsChild(Entity _entity) : Component(_entity)
{
    public bool ChildEnemy { get; set; } = false;
}