using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Space_Wars.Content.Main;
public abstract class Status
{
    public bool IsExpired { get; protected set; } = false;
    public abstract StatusType Type { get; }
    public abstract void Update(Entity _parent);
    public abstract void Reset();
    public virtual int StealthChange() { return 0; }
    public virtual int SensingChange() { return 0; }
    public enum StatusType
    {
        Bomb,
        Fire,
        Healing,
    }
}
public class StatusHolder
{
    List<Status> effects = [];
    public int StealthChange { get; private set; }
    public int SensingChange { get; private set; }
    public void Update(Entity _parent)
    {
        StealthChange = 0;
        SensingChange = 0;
        effects = effects.Where(x => !x.IsExpired).ToList();
        foreach (var effect in effects)
        {
            effect.Update(_parent);
            StealthChange += effect.StealthChange();
            SensingChange += effect.SensingChange();
        }
    }
    public void ApplyStatus(Status _status)
    {
        if (_status == null)
        {
            return;
        }
        foreach (var status in effects)
        {
            if (status.Type == _status.Type)
            {
                status.Reset();
                return;
            }
        }
        effects.Add(_status);
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
public class Fire(float _duration) : Status
{
    float initialDuration = _duration;
    float duration = _duration;
    float fireCooldown = 0.05f;
    float attackCooldown = 0.5f;
    public override StatusType Type { get; } = StatusType.Fire;

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
    public override int StealthChange()
    {
        return -1;
    }
    public override void Reset()
    {
        duration = initialDuration;
    }
}
public class Healing(float _duration) : Status
{
    float initialDuration = _duration;
    float duration = _duration;
    float fireCooldown = 0.1f;
    float healCooldown = 0.5f;
    public override StatusType Type { get; } = StatusType.Healing;

    public override void Update(Entity _parent)
    {
        if (fireCooldown > 0)
        {
            fireCooldown -= Engine.DeltaSeconds;
        }
        else
        {
            fireCooldown = 0.2f;
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.5f + Engine.Random.NextSingle() / 10, _parent.position,
                _parent.velocity + new Vector2(Engine.OneToNegOne() / 3, -Engine.Random.NextSingle() - 0.5f), -0, Engine.OneToNegOne() / 5, Color.Green, Color.Transparent));
        }
        if (healCooldown > 0)
        {
            healCooldown -= Engine.DeltaSeconds;
        }
        else
        {
            healCooldown = 1f;
            _parent.Collide(-(int)Math.Ceiling(duration), true);
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
        duration += initialDuration;
    }
}
