using OrbitCommander.Entities;
using OrbitCommander.Core;

namespace OrbitCommander.Components;
public class Stealth(Entity _entity) : Component
{
    private int stealthAbility = 0;
    public int TrueStealth => stealthAbility;
    public int StealthAbility
    {
        get => stealthAbility + ((RevealDuration > 0 || _entity.GetComponent<Health>()?.CurrentHealth <= 0 ? -5 : 0) + _entity.Statuses?.StealthChange ?? 0);
        set => stealthAbility = value;
    }
    private int sensingAbility = 0;
    public int SensingAbility
    {
        get => sensingAbility + _entity.Statuses?.SensingChange ?? 0;
        set => sensingAbility = value;
    }
    public float RevealDuration { get; set; } = 0;
    public override void Update()
    {
        if (RevealDuration > 0)
        {
            RevealDuration -= Engine.DeltaSeconds;
        }
    }
}
