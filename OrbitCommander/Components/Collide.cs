using Microsoft.Xna.Framework.Audio;
using OrbitCommander.Entities;
using System;
using OrbitCommander.Core;

namespace OrbitCommander.Components;
internal class Collide(Entity _entity, Func<int, bool, int> _onCollide) : IComponent
{
    private bool prev = false;
    public bool WasHit { get; set; } = false;
    public SoundEffect HitSound { get; set; }
    public float InvincibilityCooldown { get; set; } = 0;
    public Func<int, bool, int> OnCollide { get; set; } = _onCollide;
    public void Update()
    {
        if(prev)
        {
            prev = false;
            WasHit = false;
        }
        if (WasHit && HitSound != null)
        {
            SoundManager.PlaySound(HitSound, _entity.Position);
            prev = true;
        }
        if (InvincibilityCooldown > 0)
        {
            InvincibilityCooldown = 0;
        }
    }
}