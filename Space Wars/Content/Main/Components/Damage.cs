using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main.Components;
//Currently used as a projectile tag standin
internal class Attack(Entity _entity) : Component(_entity)
{
    public int Damage { get; set; }
}
