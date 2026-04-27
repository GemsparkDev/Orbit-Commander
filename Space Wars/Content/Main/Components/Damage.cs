using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main.Components;
//Currently used as a projectile tag standin
internal class Attack(Entity _entity) : Component(_entity)
{
    public int Damage { get; set; }
}
