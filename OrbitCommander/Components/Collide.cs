using Microsoft.Xna.Framework.Audio;
using OrbitCommander.Entities;
using System;
using OrbitCommander.Core;

namespace OrbitCommander.Components;
internal class Collide(Entity _entity, Func<int, bool, bool> _onCollide) : Component()
{
    public bool WasHit { get; set; } = false;
    public SoundEffect HitSound { get; set; }
    public float InvincibilityCooldown { get; set; } = 0;
    public Func<int, bool, bool> OnCollide { get; set; } = _onCollide;
    public override void Update()
    {
        if (WasHit && HitSound != null)
        {
            SoundManager.PlaySound(HitSound, _entity.Position);
        }
        WasHit = false;
        if (InvincibilityCooldown > 0)
        {
            InvincibilityCooldown = 0;
        }
    }
}