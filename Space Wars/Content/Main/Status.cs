using Microsoft.Xna.Framework;
using Space_Wars.Content.Main.Components;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;
using System;

namespace Space_Wars.Content.Main;
public abstract class Status(Sprites _icon)
{
    public bool IsExpired { get; protected set; } = false;
    public Sprites Icon { get; } = _icon;
    public abstract StatusType Type { get; }
    public abstract void Update(Entity _parent);
    public abstract void Reset();
    public virtual int StealthChange() { return 0; }
    public virtual int SensingChange() { return 0; }
    public virtual int ModifyDamage(int _damage) { return _damage; }
    public enum StatusType
    {
        Bomb,
        Fire,
        Frost,
        Healing,
        Berserk,
    }
}
public class Bomb() : Status(Sprites.Knob)
{
    float time = 0;
    float maxTime = 300;
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
        ParticleManager.Add(new Particle(null, _parent.Position + _parent.Velocity + new Vector2(0, -15), 0, Color.Red) { drawText = (Math.Truncate((maxTime - time) * 100) / 100).ToString() });
        if (time > maxTime)
        {
            _parent.Collide(999);
            _parent.isExpired = true;
            IsExpired = true;
        }
    }
    public override void Reset()
    {
        IsExpired = true;
    }
}
public class Fire(float _duration, Color _color) : Status(Sprites.Knob)
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
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), 0.5f + Util.Random.NextSingle() / 10, _parent.Position,
                _parent.Velocity + new Vector2(Util.OneToNegOne() / 3, -Util.Random.NextSingle() - 0.5f), 0, Util.OneToNegOne() / 5, _color, Color.Transparent));
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
        return -10;
    }
    public override void Reset()
    {
        duration = initialDuration;
    }
}
public class Frost(float _duration) : Status(Sprites.Knob)
{
    float initialDuration = _duration;
    float duration = _duration;
    float fireCooldown = 0.25f;

    public override StatusType Type { get; } = StatusType.Frost;

    public override void Update(Entity _parent)
    {
        if (fireCooldown > 0)
        {
            fireCooldown -= Engine.DeltaSeconds;
        }
        else
        {
            fireCooldown = 1024 / (_parent.Size.X * _parent.Size.Y + 1024);
            Vector2 pos = Util.ToUnitVector(Util.Random.NextSingle() * MathF.Tau) * Util.Random.NextSingle() * _parent.Size / 1.5f;
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Glow), 1.5f + Util.Random.NextSingle() / 5, _parent.Position + pos,
             _parent.Velocity, 0, 0, Color.Cyan, Color.Transparent));
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
    public override int ModifyDamage(int _damage)
    {
        return _damage * 2;
    }
    public override void Reset()
    {
        duration = initialDuration;
    }
}
public class Healing(float _duration) : Status(Sprites.Knob)
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
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), 0.5f + Util.Random.NextSingle() / 10, _parent.Position,
                _parent.Velocity + new Vector2(Util.OneToNegOne() / 3, -Util.Random.NextSingle() - 0.5f), -0, Util.OneToNegOne() / 5, Color.Green, Color.Transparent));
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
    public override int ModifyDamage(int _damage)
    {
        return (int)(_damage * 0.5f);
    }
    public override void Reset()
    {
        duration += initialDuration;
    }
}
public class Berserk(float _timeLeft) : Status(Sprites.Knob)
{
    private bool bonus = false;
    float timeLeft = _timeLeft;
    private ParticleEmitter effect = new ParticleEmitter(Assets.Get(Sprites.Circle), Vector2.Zero, 28.3f, Color.Red * 0.5f)
    { particleFadeToColor = Color.Transparent, particleTimeAlive = 0.5f, speedOfEmission = 0.25f };

    public override StatusType Type => StatusType.Berserk;

    public override void Update(Entity _parent)
    {
        if (bonus)
        {
            var comp = _parent.GetComponent<Cooldown>();
            if (comp != null)
            {
                for (int i = 0; i < comp.Cooldowns.Count; i++)
                {
                    if (comp.Cooldowns[i] > 0)
                    {
                        comp.Cooldowns[i] -= Engine.DeltaSeconds;
                    }
                }
            }
            if (_parent is Player)
            {
                (_parent as Player).LowerCooldown();
            }
        }
        bonus = !bonus;
        effect.position = _parent.Position;
        effect.sprayAngle += Engine.DeltaSeconds * 2;
        effect.offsetVelocity = _parent.Velocity;
        effect.Update();
        if (timeLeft > 0)
        {
            timeLeft -= Engine.DeltaSeconds;
        }
        else
        {
            IsExpired = true;
        }
    }
    public override void Reset()
    {
        timeLeft = 10;
        bonus = true;
    }
    public override int SensingChange()
    {
        return -1;
    }
    public override int StealthChange()
    {
        return -1;
    }
    public override int ModifyDamage(int _damage)
    {
        return (int)(_damage * 1.5f);
    }
}
public class Pressure(Color _color, bool _isFatal) : Status(Sprites.Knob)
{
    float duration = Engine.DeltaSeconds * 2;
    float fireCooldown = 0.05f;
    float attackCooldown = 0.1f;
    public override StatusType Type { get; } = StatusType.Fire;
    public override void Update(Entity _parent)
    {
        if (fireCooldown > 0)
        {
            fireCooldown -= Engine.DeltaSeconds;
        }
        else
        {
            fireCooldown = 0.1f;
            ParticleManager.Add(new Particle(_parent.Texture, 1f, _parent.Position - _parent.Velocity,
                _parent.Velocity + new Vector2(Util.OneToNegOne() / 5, Util.OneToNegOne() / 5), Util.OneToNegOne() / 10, Util.OneToNegOne() / 50, _color * MathF.Sqrt(duration / 10), Color.Transparent));
        }
        if (attackCooldown > 0)
        {
            attackCooldown -= Engine.DeltaSeconds;
        }
        else
        {
            //Only inflicts fatal damage on player (prevents cheese)
            if (_isFatal && _parent is Player)
            {
                attackCooldown = 0.1f;
                _parent.Collide((int)MathF.Pow(3, Math.Min(duration * 10, 5)), true);
            }
            else if (duration > 1)
            {
                attackCooldown = 0.1f;
                _parent.Collide(3, true);
            }
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
        if (duration < 10)
        {
            duration += Engine.DeltaSeconds * 2;
        }
    }
}
