using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main.Components;
internal class Cooldown(Entity _entity) : Component(_entity)
{
    public List<float> Cooldowns { get; set; } = [];
    public override void Update()
    {
        for(int i = 0; i < Cooldowns.Count; i++)
        {
            if (Cooldowns[i] > 0)
            {
                Cooldowns[i] -= Engine.DeltaSeconds;
            }
        }
    }
}
