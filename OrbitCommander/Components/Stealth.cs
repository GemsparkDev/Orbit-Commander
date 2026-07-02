using OrbitCommander.Entities;
using OrbitCommander.Core;
using Microsoft.Xna.Framework;
using System;

namespace OrbitCommander.Components;
public class Stealth(Entity _entity) : IComponent
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
    public void Update()
    {
        if (RevealDuration > 0)
        {
            RevealDuration -= Engine.DeltaSeconds;
        }
    }
    public float StealthTransparency()
    {
        float stealth = 1;
        var maxDistance = Mission.StealthRange * Engine.SaveGame.Player.CountFuses(ModuleType.Sensors) / 4;
        //Player has superior sensing to stealth -> full detection
        //Player has equal sensing to stealth -> partial detection when nearby
        //Player has inferior sensing to stealth -> no detection
        if (Engine.SaveGame.Player.SensingAbility == StealthAbility)
        {
            float distanceSqr = Vector2.DistanceSquared(Engine.SaveGame.Player.Position, _entity.Position);
            if (distanceSqr > maxDistance * maxDistance)
            {
                stealth = 0;
            }
            else
            {
                stealth = MathF.Sqrt(maxDistance - MathF.Sqrt(distanceSqr)) / MathF.Sqrt(maxDistance);
            }
        }
        else if (Engine.SaveGame.Player.SensingAbility < StealthAbility)
        {
            stealth = 0;
        }
        stealth = MathF.Max(stealth, (float)Math.Clamp(RevealDuration, 0f, 1f));
        return stealth;
    }
}
