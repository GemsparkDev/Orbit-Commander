using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main.Components;
internal class Temp(Entity _entity) : Component(_entity)
{
    public float Temperature { get; set; } = 0; //-1: Freeze, 0: Neutral, 1: Burn
    public void ApplyWork(float _q)
    {
        Temperature += _q * Engine.DeltaSeconds;
    }
    public void ConductHeat(float _temp, float _rate)
    {
        Temperature += (_temp - Temperature) * _rate * Engine.DeltaSeconds;
    }
}
