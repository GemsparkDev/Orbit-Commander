using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main.Components;
internal class Collide(Entity _entity, Func<int, bool, bool> _onCollide) : Component(_entity)
{
    public Func<int, bool, bool> OnCollide { get; set; } = _onCollide;
}