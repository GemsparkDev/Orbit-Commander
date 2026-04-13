using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main.Components;
public class Stealth(Entity _entity) : Component(_entity)
{
    public int StealthAbility { get; set; }
    public int SensingAbility { get; set; }
    public float RevealDuration { get; set; } = 0;
    public override void Update()
    {
        if(RevealDuration > 0)
        {
            RevealDuration -= Engine.DeltaSeconds;
        }
    }
}
