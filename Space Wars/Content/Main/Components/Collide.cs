using Microsoft.Xna.Framework.Audio;
using Space_Wars.Content.Main.Entities;
using System;

namespace Space_Wars.Content.Main.Components;
internal class Collide(Entity _entity, Func<int, bool, bool> _onCollide) : Component(_entity)
{
    public bool WasHit { get; set; } = false;
    public SoundEffect HitSound { get; set; }
    public float InvincibilityCooldown { get; set; } = 0;
    public Func<int, bool, bool> OnCollide { get; set; } = _onCollide;
    public override void Update()
    {
        if (WasHit && HitSound != null)
        {
            SoundManager.PlaySound(HitSound, Entity.Position);
        }
        WasHit = false;
        if (InvincibilityCooldown > 0)
        {
            InvincibilityCooldown = 0;
        }
    }
}