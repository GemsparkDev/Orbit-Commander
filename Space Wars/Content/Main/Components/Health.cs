using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main.Components;
internal class Health(Entity _entity) : Component(_entity)
{
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public override void Update()
    {
        if (CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
    }
}
