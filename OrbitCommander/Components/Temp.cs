using OrbitCommander.Core;
namespace OrbitCommander.Components;
internal class Temp() : Component()
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
