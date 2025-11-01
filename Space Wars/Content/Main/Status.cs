using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace Space_Wars.Content.Main;
public abstract class Status(Sprite _icon)
{
    public bool IsExpired { get; protected set; } = false;
    public Sprite Icon { get; } = _icon;
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
        Berserk,
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
    public void Draw(SpriteBatch _spriteBatch, Entity _parent)
    {
        float maxOffset = (float)(effects.Count - 1) / 2;
        foreach (var effect in effects)
        {
            _spriteBatch.Draw(Assets.Get(effect.Icon), _parent.position + new Vector2(maxOffset * 20, 20), null, Color.White, 0, Assets.DimsOf(effect.Icon) / 2, 1, 0, 0);
            maxOffset -= 1;
        }
    }
}
public class Bomb() : Status(Sprite.Knob)
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
        ParticleManager.Add(new Particle(null, _parent.position + _parent.velocity + new Vector2(0, -15), 0, Color.Red) { drawText = (Math.Truncate((maxTime - time) * 100) / 100).ToString()});
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
public class Fire(float _duration, Color _color) : Status(Sprite.Knob)
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
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.5f + Util.Random.NextSingle() / 10, _parent.position - _parent.velocity, 
                _parent.velocity + new Vector2(Util.OneToNegOne() / 3, -Util.Random.NextSingle() - 0.5f), 0, Util.OneToNegOne() / 5, _color, Color.Transparent));
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
public class Healing(float _duration) : Status(Sprite.Knob)
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
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 0.5f + Util.Random.NextSingle() / 10, _parent.position,
                _parent.velocity + new Vector2(Util.OneToNegOne() / 3, -Util.Random.NextSingle() - 0.5f), -0, Util.OneToNegOne() / 5, Color.Green, Color.Transparent));
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
public class Berserk(float _timeLeft) : Status(Sprite.Knob)
{
    private bool bonus = false;
    float timeLeft = _timeLeft;
    private ParticleEmitter effect = new ParticleEmitter(Assets.Get(Sprite.Circle), Vector2.Zero, 28.3f, Color.Red * 0.5f) 
    { particleFadeToColor = Color.Transparent, particleTimeAlive = 0.5f, speedOfEmission = 0.25f };

    public override StatusType Type => StatusType.Berserk;

    public override void Update(Entity _parent)
    {
        if (bonus)
        {
            _parent.LowerCooldown();
        }
        bonus = !bonus;
        effect.position = _parent.position;
        effect.sprayAngle += Engine.DeltaSeconds * 2;
        effect.offsetVelocity = _parent.velocity;
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
}
public class Pressure(Color _color, bool _isFatal) : Status(Sprite.Knob)
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
            fireCooldown = 0.05f;
            ParticleManager.Add(new Particle(_parent.texture, 1f, _parent.position - _parent.velocity,
                _parent.velocity + new Vector2(Util.OneToNegOne() / 3, Util.OneToNegOne() / 3), Util.OneToNegOne() / 10, Util.OneToNegOne() / 5, _color * (duration / 10), Color.Transparent));
        }
        if (attackCooldown > 0)
        {
            attackCooldown -= Engine.DeltaSeconds;
        }
        else
        {
            //Only inflicts fatal damage on player (prevents cheese)
            if (_isFatal && _parent.isFriendly)
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
