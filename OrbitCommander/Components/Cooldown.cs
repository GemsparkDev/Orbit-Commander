using System.Collections.Generic;
using OrbitCommander.Core;

namespace OrbitCommander.Components;
internal class Cooldown : IComponent
{
    public List<float> Cooldowns { get; set; } = [];
    public void Update()
    {
        for (int i = 0; i < Cooldowns.Count; i++)
        {
            if (Cooldowns[i] > 0)
            {
                Cooldowns[i] -= Engine.DeltaSeconds;
            }
        }
    }
}
