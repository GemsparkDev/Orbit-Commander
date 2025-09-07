using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;
using System;
using Microsoft.Xna.Framework;

namespace Space_Wars.Content.Main;
public abstract class Status
{
    public bool IsExpired { get; protected set; } = false;
    public abstract StatusType Type { get; }
    public abstract void Update(Entity _parent);
    public abstract void Reset();
    public enum StatusType
    {
        Bomb,
        Fire,
    }
}
public class Bomb : Status
{
    float time = 0;
    public override StatusType Type { get; } = StatusType.Bomb;
    public override void Update(Entity _parent)
    {
        float prevTime = time;
        time += Engine.DeltaSeconds;
        int second = (int)Math.Truncate(time);
        if (prevTime < second && time > second)
        {
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Beep));
        }
        ParticleManager.Add(new Particle(null, _parent.position + new Vector2(0, -15), 0, Color.Red) { drawText = (Math.Truncate((60 - time) * 100) / 100).ToString()});
        if (time > 60)
        {
            _parent.Collide(999);
            _parent.isExpired = true;
            IsExpired = true;
        }
    }
    public override void Reset()
    {
        time = 0;
    }
}
public class Fire : Status
{
    float initialDuration;
    float duration;
    float fireCooldown = 0.05f;
    float attackCooldown = 0.5f;
    public override StatusType Type { get; } = StatusType.Fire;
    public Fire(float _duration)
    {
        duration = _duration;
        initialDuration = _duration;
    }
    public override void Update(Entity _parent)
    {
        if (fireCooldown > 0)
        {
            fireCooldown -= Engine.DeltaSeconds;
        }
        else
        {
            fireCooldown = 0.05f;
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.5f + Engine.Random.NextSingle() / 10, _parent.position, 
                _parent.velocity + new Vector2(Engine.OneToNegOne() / 3, -Engine.Random.NextSingle() - 0.5f), -0, Engine.OneToNegOne() / 5, Color.Orange, Color.Transparent));
        }
        if (attackCooldown > 0)
        {
            attackCooldown -= Engine.DeltaSeconds;
        }
        else
        {
            attackCooldown = 1f;
            _parent.Collide(3, true);
        }
        if (duration > 0)
        {
            duration -= Engine.DeltaSeconds;
        }
        else
        {
            IsExpired = true;
        }
    }
    public override void Reset()
    {
        duration = initialDuration;
    }
}
