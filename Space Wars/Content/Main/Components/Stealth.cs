using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main.Components;
public class Stealth(Entity _entity) : Component(_entity)
{
    private int stealthAbility = 0;
    public int TrueStealth => stealthAbility;
    public int StealthAbility
    {
        get => (stealthAbility + (((RevealDuration > 0 || Entity.GetComponent<Health>()?.CurrentHealth <= 0) ? -5 : 0) + Entity.Statuses?.StealthChange ?? 0));
        set => stealthAbility = value;
    }
    private int sensingAbility = 0;
    public int SensingAbility
    {
        get => (sensingAbility + Entity.Statuses?.SensingChange ?? 0);
        set => sensingAbility = value;
    }
    public float RevealDuration { get; set; } = 0;
    public override void Update()
    {
        if(RevealDuration > 0)
        {
            RevealDuration -= Engine.DeltaSeconds;
        }
    }
}
