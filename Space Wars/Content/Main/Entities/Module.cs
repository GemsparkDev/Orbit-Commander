using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main;
using Space_Wars.Content.Main.Particles;
using System;
using System.Collections.Generic;
using UILib.Content.Main;
using System.Linq;

namespace Space_Wars.Content.Main.Entities;

public abstract class Module : Pickup, IData
{
    //Serialized fields
    private int health = 20;
    public bool isFailed = false;
    public new Modules Type { get; }

    public int Health { get { return health; } set { health = value; UpdateHealth(); } }
    public int MaxHealth => (itemData as ModuleData).MaxHealth;
    public override Color Color => isFailed ? Color.Red : Color.White;
    private Decal healthDecal;
    protected float cooldown = 0;
    public float Cooldown => cooldown;

    public Module(Modules _type, Vector2 _position = default, Vector2 _velocity = default, float _angularVelocity = 0) 
        : base(ItemFactory.moduleData[_type], _position, _velocity, _angularVelocity)
    {
        health = MaxHealth;
        Type = _type;
        healthDecal = new Decal(new Vector2(0, 5), Assets.TextFont, $"{Health} / {MaxHealth}", Color.Pink, 5f);
        Tooltip.AddWidget(healthDecal);
    }
    private void UpdateHealth()
    {
        healthDecal.text = $"{Health} / {MaxHealth}";
    }

    public virtual int OnCollide(int _damage) { return _damage; }
    public virtual void OnShoot() { }
    public virtual void OnUpdate() 
    {
        if (cooldown > 0)
        {
            cooldown -= Engine.DeltaSeconds;
        }
    }
    public virtual void OnEngine() { }
    public virtual void OnAbility() { }

    public override bool Collide(int _damage, bool _ignoreImmunity = false)
    {
        if (_damage <= 0)
        {
            return false;
        }
        if (invincibilityCooldown > 0 && !_ignoreImmunity)
        {
            invincibilityCooldown = 0;
            return false;
        }
        ParticleManager.Add(new Particle(null, 1, Player.position + new Vector2(0, -1), new Vector2(0, -1.5f), 0, 0, Color.Orange, new Color(255, 0, 0, 0)) { drawText = $"Integrity: {Health}" });
        SoundManager.PlaySound(Assets.Get(Sound.Death), Player.position);
        Engine.ShakeScreen(10 / ((Player.position - Engine.Camera.Position).Length() + 150));
        if (Health > 0)
        {
            Health -= _damage;
            if (!_ignoreImmunity) 
            {
                invincibilityCooldown = 1;
            }
        }
        else
        {
            isExpired = true;
        }
        return true;
    }
    //Override to provide custom serialization for modules
    public virtual void Parse(Modules _type, List<string> _disassembly, LoadLogger _logger)
    {
        _logger.Try(delegate { health = Int32.Parse(_disassembly[2]); }, 2);
        _logger.Try(delegate { isFailed = bool.Parse(_disassembly[3]); }, 3);
        UpdateHealth();
        base.Parse(_disassembly, _logger);
    }
    public override string Serialize()
    {
        return $"{{{Type},{SerializeAttributes()},{health},{isFailed}}}";
    }
}
public class ModuleData(Sprite _realSprite, Sprite _virtualSprite, String _name, int _id, int _health, Type _type)
    : ItemData(_realSprite, _virtualSprite, _name, _id, Color.White)
{
    public int MaxHealth { get; } = _health;
    public Type ModuleType { get; } = _type;
    public Module Retrieve()
    {
        return (Module)Activator.CreateInstance(ModuleType);
    }
}
public class Hull() : Module(Modules.Hull)
{
    public override int OnCollide(int _damage)
    {
        return _damage / 2;
    }
}
public class Shield() : Module(Modules.Shield)
{
    private ParticleEmitter shieldEffect = new(Assets.Get(Sprite.Dot), Vector2.Zero, 10, Color.Violet) { particleAngularVelocity = 0.1f };
    public override int OnCollide(int _damage)
    {
        if (cooldown <= 0)
        {
            cooldown = 8;
            return 0;
        }
        Health = (int)MathF.Max(0, Health - _damage / 2);
        return _damage;
    }
    public override void OnUpdate()
    {
        if (cooldown <= 0)
        {
            shieldEffect.position = Player.position;
            shieldEffect.offsetVelocity = Player.velocity;
            shieldEffect.Update();
            UI.PlayerSpecialHealth.enabledColor = Color.Yellow;
        }
        base.OnUpdate();
    }
}
public class Stealth() : Module(Modules.Stealth)
{
    public override int OnCollide(int _damage)
    {
        return _damage * 2 / 3;
    }
}
public class Reflective() : Module(Modules.Reflective)
{
    public override int OnCollide(int _damage)
    {
        if (Util.Random.Next(0, 3) == 0)
        {
            for (float angle = 0; angle < MathF.Tau; angle += MathF.PI / 3)
            {
                Engine.EntityManager.Add(new AssassinShot(Player.position, Util.ToUnitVector(angle) * 8, angle, 0, isFriendly, 6, 1));
            }
            return 0;
        }
        return _damage;
    }
}
public class Turtle() : Module(Modules.Turtle)
{
    float time = 0;
    int flipped = 1;
    ParticleEmitter effect = new ParticleEmitter(Assets.Get(Sprite.Dot), Vector2.Zero, 10, Color.Orange) { sprayAngle = MathF.PI / 2};
    public override int OnCollide(int _damage)
    {
        return (int)(_damage * (1 - (1 - cooldown) * (1 - cooldown)));
    }
    public override void OnShoot()
    {
        cooldown = 1;
    }
    public override void OnUpdate()
    {
        UI.PlayerSpecialHealth.enabledColor = Color.Orange;
        UI.PlayerSpecialHealth.SetInterval((1 - cooldown) * (1 - cooldown), 1);
        time += Engine.DeltaSeconds;
        if (time > 1)
        {
            time = 0;
            flipped *= -1;
            effect.sprayAngle += MathF.PI;
            if (effect.sprayAngle > MathF.Tau)
            {
                effect.sprayAngle -= MathF.Tau;
            }
        }
        effect.position = Player.position;
        if (flipped == 1)
        {
            effect.sprayCone = MathF.Tau * time;
        }
        else
        {
            effect.sprayCone = MathF.Tau * (1 - time);
        }
        effect.particleColor = Color.Orange * (1 - cooldown) * (1 - cooldown);
        effect.Update();
        base.OnUpdate();
    }
}
public class Ablative() : Module(Modules.Ablative)
{
    float buffer = 25;
    public override int OnCollide(int _damage)
    {
        cooldown = 1;
        if (buffer >= _damage)
        {
            buffer -= (float)(_damage);
            Engine.WriteLine(buffer);
            return 0;
        }
        buffer = 0;
        return _damage - (int)Math.Round(buffer);
    }
    public override void OnUpdate()
    {
        UI.PlayerSpecialHealth.enabledColor = Color.White;
        UI.PlayerSpecialHealth.SetInterval(buffer, 25f);
        if (cooldown <= 0 && buffer < 25)
        {
            buffer += Engine.DeltaSeconds * 10;
        }
        base.OnUpdate();
    }
}
public class Adaptive() : Module(Modules.Adaptive)
{
    public override int OnCollide(int _damage)
    {
        if (Health > 0)
        {
            Player.StatusHolder.ApplyStatus(new Berserk(_damage));
            return _damage * 2 / 3;
        }
        return _damage / 2;
    }

}
public class StandardEngine() : Module(Modules.Engines)
{
    float engineTime = 0;
    ParticleEmitter engineParticles = new(Assets.Get(Sprite.Circle), 0.15f, Vector2.Zero, 0, MathF.PI / 4, 2, 450f, Color.Cyan, EmitterType.EmissionOverTime)
    { particleFadeToColor = new Color(72, 61, 139, 0) };
    public override void OnEngine()
    {
        engineParticles.position = Player.position - new Vector2(MathF.Sin(Player.angle), -MathF.Cos(Player.angle)) * 8;
        engineParticles.offsetVelocity = Player.velocity;
        engineTime = Math.Clamp(engineTime + Engine.DeltaSeconds, 0, 1);
        float engineTimeModifier = 1 - (1 - engineTime) * (1 - engineTime);
        engineParticles.sprayAngle = Player.angle + MathF.PI;
        float fuseRatio = (float)(Player.CountFuses(ModuleType.Engines)) / 3;
        engineParticles.speedOfEmission = 450f * fuseRatio * engineTimeModifier;
        if (Player.direction != Vector2.Zero)
        {
            Player.velocity += Vector2.Normalize(Player.direction) * 24 * Engine.DeltaSeconds * engineTimeModifier * fuseRatio / (Player.leashedMaterials.Count + 2);
        }
        engineParticles.Update();
    }
    public override void OnUpdate()
    {
        if (!Player.isEngineActive && engineTime > 0)
        {
            engineTime -= Engine.DeltaSeconds;
        }
    }
}
public class PlasmaEngine() : Module(Modules.Plasma)
{
    float engineTime = 0;
    public override void OnEngine()
    {
        engineTime = Math.Clamp(engineTime + Engine.DeltaSeconds * 2, 0, 1);
        float engineTimeModifier = 1 - (1 - engineTime) * (1 - engineTime);
        float fuseRatio = (float)(Player.CountFuses(ModuleType.Engines)) / 3;
        Vector2 dir = -Util.ToUnitVector(Player.angle + Util.OneToNegOne() / 20);
        for (float i = 0; i < 5 * fuseRatio * engineTimeModifier; i++)
        {
            float lerp = i / (5 * fuseRatio * engineTimeModifier);
            Vector3 color = new Vector3(0, 1, 1) * (1 - lerp) + new Vector3(1, 0.5f, 0) * (lerp);
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), Player.position + dir * (i + 2.5f) * 4, Player.angle, new Color(color.X, color.Y, color.Z) * (1 - lerp)));
        }
        if (Player.direction != Vector2.Zero)
        {
            Player.velocity += Vector2.Normalize(Player.direction) * 20 * Engine.DeltaSeconds * engineTimeModifier * fuseRatio / (Player.leashedMaterials.Count + 1);
        }
    }
    public override void OnUpdate()
    {
        if (!Player.isEngineActive && engineTime > 0)
        {
            engineTime -= Engine.DeltaSeconds;
        }
    }
}
public class WorkEngine() : Module(Modules.Work)
{
    float engineTime = 0;
    ParticleEmitter engineParticles = new(Assets.Get(Sprite.Circle), 0.15f, Vector2.Zero, 0, MathF.PI / 4, 2, 450f, Color.Orange, EmitterType.EmissionOverTime)
    { particleFadeToColor = new Color(1f, 0.1f, 0, 0) };
    public override void OnEngine()
    {
        engineParticles.position = Player.position - new Vector2(MathF.Sin(Player.angle), -MathF.Cos(Player.angle)) * 8;
        engineParticles.offsetVelocity = Player.velocity;
        engineTime = Math.Clamp(engineTime + Engine.DeltaSeconds / 3, 0, 1);
        float engineTimeModifier = 1 - (1 - engineTime) * (1 - engineTime);
        engineParticles.sprayAngle = Player.angle + MathF.PI;
        float fuseRatio = (float)(Player.CountFuses(ModuleType.Engines)) / 3;
        engineParticles.speedOfEmission = 450f * fuseRatio * engineTimeModifier;
        if (Player.direction != Vector2.Zero)
        {
            Player.velocity += Vector2.Normalize(Player.direction) * 14 * Engine.DeltaSeconds * engineTimeModifier * fuseRatio;
        }
        engineParticles.Update();
    }
    public override void OnUpdate()
    {
        if (!Player.isEngineActive && engineTime > 0)
        {
            engineTime -= Engine.DeltaSeconds;
        }
    }
}
public class OrionEngine() : Module(Modules.Orion)
{
    public override void OnEngine()
    {
        if (cooldown > 0)
        {
            return;
        }
        cooldown = 0.5f;
        var dir = Vector2.Normalize(Player.direction);
        if (Player.direction != Vector2.Zero)
        {
            Player.velocity += dir * 4 / (Player.leashedMaterials.Count + 1);
            Util.Explode(Player.position - dir * 30, Player.velocity, 10, 28);
            SoundManager.PlaySound(Assets.Get(Sound.ShieldHit), Player.position);
        }
    }
}
public class Basic() : Module(Modules.Basic)
{
    public override void OnShoot()
    {
        if (cooldown > 0)
        {
            return;
        }
        Engine.EntityManager.Add(new PulseShot(Player.position, Player.IdealSpeedWithVelocity(9), Util.ToAngle(Player.Direction), 0, true, 3, true));
        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.position);
        cooldown = 0.25f;
        Engine.ShakeScreen(0.2f);
        Player.velocity -= Player.Direction / 4;
    }
}
public class Sniper() : Module(Modules.Sniper)
{
    public override void OnShoot()
    {
        if (cooldown > 0)
        {
            return;
        }
        Engine.EntityManager.Add(new AssassinShot(Player.position, Player.IdealSpeedWithVelocity(20), Util.ToAngle(Player.Direction), 0, true, 20) { texture = Assets.Get(Sprite.Arrow) });
        SoundManager.PlaySound(Assets.Get(Sound.SniperFire), Player.position);
        cooldown = 2f;
        Engine.ShakeScreen(0.3f);
        Player.velocity -= Player.Direction / 2;
    }
}
public class Antimaterial() : Module(Modules.Antimaterial)
{
    public override void OnShoot()
    {
        if (cooldown > 0)
        {
            return;
        }
        List<Entity> entities = Engine.EntityManager.Hitscan(Player.position, Player.Direction, 3000, true, out Vector2 _);
        foreach (var entity in entities)
        {
            entity.Collide(30);
        }
        SoundManager.PlaySound(Assets.Get(Sound.SniperFire), Player.position);
        cooldown = 4f;
        Engine.ShakeScreen(0.5f);
        Player.velocity -= Player.Direction * 3;
        for (int i = 0; i < 300; i++)
        {
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), 2, Player.position + Player.Direction * 4 * i, Vector2.Zero, Util.ToAngle(Player.Direction), 0, Color.Red, Color.Transparent));
        }
    }
}
public class Spiral() : Module(Modules.Spiral)
{
    public override void OnShoot()
    {
        if (cooldown > 0)
        {
            return;
        }
        Vector2 speed = Player.IdealSpeedWithVelocity(12);
        Engine.EntityManager.Add(new SpiralShot(Player.position, speed, Util.ToAngle(Player.Direction), 0, true, 5, false, 1));
        Engine.EntityManager.Add(new SpiralShot(Player.position, speed, Util.ToAngle(Player.Direction), 0, true, 5, true, 1));
        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.position);
        cooldown = 0.7f;
        Engine.ShakeScreen(0.2f);
    }
}
public class Shotgun() : Module(Modules.Shotgun)
{
    public override void OnShoot()
    {
        if (cooldown < 0)
        {
            return;
        }
        int randomBulletCount = Util.Random.Next(4, 6);
        for (int i = 0; i < randomBulletCount; i++)
        {
            float angleDegrees = (Util.Random.NextSingle() - 0.5f) * 5;
            float offsetAngle = angleDegrees * MathF.PI / 180;
            Vector2 targetVector = Player.IdealSpeedWithVelocity(8) + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
            Vector2 positionOffset = new Vector2(Player.Direction.Y, -Player.Direction.X) * offsetAngle * 100;
            Engine.EntityManager.Add(new PulseShot(Player.position + positionOffset, targetVector, Util.ToAngle(Player.Direction) + offsetAngle, 0, true, 2));
        }
        SoundManager.PlaySound(Assets.Get(Sound.ShotgunFire), Player.position);
        Player.velocity -= Player.Direction / 2;
        cooldown = 1f;
        Engine.ShakeScreen(0.4f);
    }
}
public class Missile() : Module(Modules.Missile)
{
    public override void OnShoot()
    {
        if (cooldown > 0)
        {
            return;
        }
        Engine.EntityManager.Add(Enemy.NewMissile(Player.position + new Vector2(Player.Direction.Y, -Player.Direction.X) * 6, Player.Player.IdealSpeedWithVelocity(9), Util.ToAngle(Player.Direction), true));
        SoundManager.PlaySound(Assets.Get(Sound.MissileFire), Player.position);
        cooldown = 1.5f;
        Player.velocity -= Player.Direction / 4;
        Engine.ShakeScreen(0.3f);
    }
}
public class LMG() : Module(Modules.LMG)
{
    public override void OnShoot()
    {
        if (cooldown > 0)
        {
            return;
        }
        Vector2 offset = new Vector2(Player.Direction.Y, -Player.Direction.X) * Util.Random.Next(-2, 3);
        Texture2D dot = Assets.Get(Sprite.Microshot);
        Projectile shot = new PulseShot(Player.position + offset, Player.Player.IdealSpeedWithVelocity(8) + offset / 4, Util.ToAngle(Player.Direction), 0, true, 2)
        {
            texture = dot,
            timeLeft = 3
        };
        Engine.EntityManager.Add(shot);
        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Player.position);
        Engine.ShakeScreen(0.01f);
        Player.velocity -= Player.Direction / 8;
        cooldown = 0.15f;
    }
}
public class Crossbow() : Module(Modules.Crossbow)
{
    public override void OnShoot()
    {
        if (cooldown > 0)
        {
            return;
        }
        Vector2 offset = new Vector2(Player.Direction.Y, -Player.Direction.X) * Util.Random.Next(-2, 3);
        Texture2D dot = Assets.Get(Sprite.CrossbowShot);
        Projectile shot = new PulseShot(Player.position + offset, Player.Player.IdealSpeedWithVelocity(8) + offset / 4, Util.ToAngle(Player.Direction), 0, true, 8, false, 1)
        {
            texture = dot,
        };
        Engine.EntityManager.Add(shot);
        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Player.position);
        Engine.ShakeScreen(0.2f);
        Player.velocity -= Player.Direction / 4;
        cooldown = 0.5f;
    }
}
public class Flamethrower() : Module(Modules.Flamethrower)
{
    public override void OnShoot()
    {
        if (cooldown > 0)
        {
            return;
        }
        Engine.EntityManager.Add(new FlameBolt(Player.position, Player.IdealSpeedWithVelocity(5) + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 4, true, 1));
        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Player.position);
        cooldown = 0.08f;
        Engine.ShakeScreen(0.02f);
    }
}
public class Fireball() : Module(Modules.Fireball)
{
    public override void OnShoot()
    {
        if (cooldown > 0)
        {
            return;
        }
        Engine.EntityManager.Add(new FlameBolt(Player.position, Player.IdealSpeedWithVelocity(8) + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 2, true, 8, 4, 0.5f));
        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Player.position);
        cooldown = 0.8f;
        Engine.ShakeScreen(0.1f);
    }
}
public class GrenadeLauncher() : Module(Modules.GrenadeLauncher)
{
    public override void OnShoot()
    {
        if (cooldown > 0)
        {
            return;
        }
        Engine.EntityManager.Add(new Explosive(Player.position, Player.IdealSpeedWithVelocity(8) + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()), Util.ToAngle(Player.Direction), Util.OneToNegOne() / 8, true, 16, 40, 1));
        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.position);
        cooldown = 1f;
        Engine.ShakeScreen(0.3f);
        Player.velocity -= Player.Direction / 2;
    }
}
public class SpewerModule() : Module(Modules.Spewer)
{
    public override void OnShoot()
    {
        if (cooldown > 0)
        {
            return;
        }
        Engine.EntityManager.Add(new Spewer(Player.position, Player.IdealSpeedWithVelocity(4) + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 2, Util.ToAngle(Player.Direction), Util.OneToNegOne() / 8, true, 2));
        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.position);
        cooldown = 1f;
        Engine.ShakeScreen(0.3f);
        Player.velocity -= Player.Direction / 2;
    }
}
public class Triangle() : Module(Modules.Triangle)
{
    public override void OnShoot()
    {
        if (cooldown > 0)
        {
            return;
        }
        Vector2 vel = Player.IdealSpeedWithVelocity(8) - Player.velocity;
        var dir = Vector2.Normalize(vel);
        Player.velocity += dir;
        var offset = new Vector2(dir.Y, -dir.X);
        float angle = Util.ToAngle(Player.Direction);
        Engine.EntityManager.Add(new PulseShot(Player.position, vel + Player.velocity, angle, 0, true, 6));
        Engine.EntityManager.Add(new PulseShot(Player.position, -vel + offset * 5 + Player.velocity, angle, 0, true, 10) { texture = Assets.Get(Sprite.Explosive) });
        Engine.EntityManager.Add(new PulseShot(Player.position, -vel - offset * 5 + Player.velocity, angle, 0, true, 10) { texture = Assets.Get(Sprite.Explosive) });
        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.position);
        cooldown = 0.5f;
        Engine.ShakeScreen(0.3f);
    }
}
public class PrismArray() : Module(Modules.PrismArray)
{
    float time = 0;
    public override void OnShoot()
    {
        Vector2 dir = Util.ToUnitVector(Util.ToAngle(Player.Direction));
        List<Entity> enemies = Engine.EntityManager.Hitscan(Player.position, dir, 250, true, out Vector2 _end);
        for (float i = 0; i < (_end - Player.position - dir * 10).Length() / 5; i++)
        {
            float lerp = i / 50;
            Vector3 color = new Vector3(0, 1, 1) * (1 - lerp) + new Vector3(1, 1, 0) * (lerp);
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), dir * (i + 2f) * 5 + Player.position + new Vector2(dir.Y, -dir.X) * MathF.Sin(i / 2 - time * 5) / 2, Util.ToAngle(Player.Direction), new Color(color.X, color.Y, color.Z) * (1 - (lerp))));
        }
        if (cooldown > 0)
        {
            return;
        }
        cooldown = 0.1f;
        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Player.position);
        foreach (var enemy in enemies)
        {
            enemy.Collide(1);
        }
    }
    public override void OnUpdate()
    {
        time += Engine.DeltaSeconds;
        base.OnUpdate();
    }
}
public class MatrixLauncher() : Module(Modules.MatrixLauncher)
{
    public override void OnShoot()
    {
        if (cooldown > 0)
        {
            return;
        }
        Vector2 vel = Player.IdealSpeedWithVelocity(15);
        Engine.EntityManager.Add(new FlameBolt(Player.position, vel + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 2, true, 10,
            new ParticleEmitter(Assets.Get(Sprite.Circle), Player.position, 0, Color.Cyan) { sprayCone = MathF.PI * 2 / 3, sprayAngle = Util.ToAngle(vel - Player.velocity), speedOfEmission = 0.5f }, 4));
        SoundManager.PlaySound(Assets.Get(Sound.SniperFire), Player.position);
        cooldown = 1.5f;
        Engine.ShakeScreen(0.5f);
    }
}
public class Torch() : Module(Modules.Torch)
{
    public override void OnShoot()
    {
        if (cooldown > 0)
        {
            return;
        }
        Vector2 offset = Util.ToUnitVector(Util.ToAngle(Player.Direction) + MathF.PI / 2) * Util.OneToNegOne() * 3;
        Projectile shot = new FlameBolt(Player.position - offset * 5, Player.IdealSpeedWithVelocity(12) + offset, Player.isFriendly, 1, 2, 0.1f);
        Engine.EntityManager.Add(shot);
        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Player.position);
        Engine.ShakeScreen(0.02f);
        Player.velocity -= Player.Direction / 6;
        cooldown = 0.1f;
    }
}
public class Poison() : Module(Modules.Poison)
{
    public override void OnShoot() 
    {
        if (cooldown > 0)
        {
            return;
        }
        Vector2 vel = Player.Direction;
        Projectile shot = new FlameBolt(Player.position, vel, isFriendly, 0, 
        new ParticleEmitter(Assets.Get(Sprite.Circle), 10, Player.position, 0, MathF.Tau, 1.5f, 2000, Color.Green, EmitterType.EmissionOverTime) 
        { particleFadeToColor = Color.Transparent, offsetVelocity = vel, particlesExperienceGravity = true}, 20, 1);
        Engine.EntityManager.Add(shot);
        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Player.position);
        Engine.ShakeScreen(0.02f);
        Player.velocity -= Player.Direction / 6;
        cooldown = 1.5f;
    }
}
public class Dash() : Module(Modules.Dash)
{
    const float MaxCooldown = 2;
    public override void OnAbility()
    {
        if (cooldown > 0)
        {
            return;
        }
        Player.invincibilityCooldown = 0.5f;
        for (int i = 0; i < 200; i++)
        {
            float timeLeft = ((float)i / 200);
            var col = Color.SlateBlue;
            col.A = 0;
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), timeLeft, Player.position + Player.Direction * i, Player.velocity * timeLeft, Util.ToAngle(Player.Direction), 0, Color.Cyan, col));
        }
        Player.position += Player.Direction * 200;
        cooldown = MaxCooldown;
    }
    public override void OnUpdate()
    {
        UI.PlayerAbility.SetInterval(1 - cooldown / MaxCooldown, 1);
        base.OnUpdate();
    }
}
public class SummonShield() : Module(Modules.SummonShield)
{
    Enemy shield;
    const float MaxCooldown = 5;
    public override void OnAbility()
    {
        if (cooldown > 0 || shield != null)
        {
            return;
        }
        shield = Enemy.NewShield(Player, 5, 1, 0, 1);
        Engine.EntityManager.Add(shield);
        cooldown = MaxCooldown;
    }
    public override void OnUpdate()
    {
        if (shield != null && shield.isExpired)
        {
            shield = null;
        }
        UI.PlayerAbility.SetInterval(1 - cooldown / MaxCooldown, 1);
        base.OnUpdate();
    }
}
public class SummonGrapplingHook() : Module(Modules.GrapplingHook)
{
    const float MaxCooldown = 5;
    Projectile hook;
    public override void OnAbility()
    {
        if (hook != null)
        {
            cooldown /= 2;
            return;
        }
        if (cooldown > 0)
        {
            return;
        }
        hook = new GrapplingHook(Player.position, Player.IdealSpeedWithVelocity(50), Util.ToAngle(Player.Direction), Player);
        SoundManager.PlaySound(Assets.Get(Sound.Click), Player.position);
        Engine.ShakeScreen(0.3f);
        Player.velocity -= Player.Direction / 2;
        Engine.EntityManager.Add(hook);
        cooldown = MaxCooldown;
    }
    public override void OnUpdate()
    {
        if (hook != null && hook.isExpired)
        {
            hook = null;
        }
        UI.PlayerAbility.SetInterval(1 - cooldown / MaxCooldown, 1);
        base.OnUpdate();
    }
}
public class Nanomachines() : Module(Modules.Nanomachines)
{
    const float MaxCooldown = 30;
    public override void OnAbility()
    {
        if (cooldown > 0)
        {
            return;
        }
        foreach (var pickup in Player.leashedMaterials)
        {
            if (pickup is not Module and not Construct)
            {
                pickup.isExpired = true;
                StatusHolder.ApplyStatus(new Healing(4));
                cooldown = MaxCooldown;
                return;
            }
        }
    }
    public override void OnUpdate()
    {
        UI.PlayerAbility.SetInterval(1 - cooldown / MaxCooldown, 1);
        base.OnUpdate();
    }
}
public class CreateFighter() : Module(Modules.CreateFighter)
{
    const float MaxCooldown = 60;
    private List<Enemy> allies = [];
    public override void OnAbility()
    {
        if (cooldown > 0 || allies.Count >= 10)
        {
            return;
        }
        foreach (var pickup in Player.leashedMaterials)
        {
            if (pickup is not Module and not Construct)
            {
                pickup.isExpired = true;
                for(int i = 0; i < 10; i++)
                {
                    var enemy = Enemy.NewSurgeChild(Player.position + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()), Player.velocity, Player.angle, Player, allies);
                    enemy.isFriendly = true;
                    Engine.EntityManager.Add(enemy);
                    allies.Add(enemy);
                }
                cooldown = MaxCooldown;
                return;
            }
        }
    }
    public override void OnUpdate()
    {
        UI.PlayerAbility.SetInterval(1 - cooldown / MaxCooldown, 1);
        allies = allies.Where(x => !x.isExpired).ToList();
        base.Update();
    }
}
public class Sensors() : Module(Modules.Sensors)
{

}
public class Lidar() : Module(Modules.Lidar)
{
    public override void OnAbility()
    {
        if (cooldown > 0)
        {
            return;
        }
        Vector2 dir = Player.Direction + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 5;
        Engine.EntityManager.Hitscan(Player.position, dir, 1000, false, out Vector2 end);
        ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), 1f, end, Vector2.Zero, 0, 0, Color.White, Color.Transparent));
    }
}
public class Radar() : Module(Modules.Radar)
{
    float time = 0;
    public override void OnUpdate()
    {        
        time += Engine.DeltaSeconds;
        base.Update();
        if (cooldown > 0)
        {
            return;
        }
        int fuses = Player.CountFuses(ModuleType.Sensors);
        Vector2 dir = Util.ToUnitVector(time * (float)(fuses) / 3);
        List<Entity> revealedEntities = Engine.EntityManager.Hitscan(Player.position, dir, 2000, true, out Vector2 end);
        foreach (var entity in revealedEntities)
        {
            entity.Reveal(2f);
        }
        if (revealedEntities.Count > 0)
        {
            SoundManager.PlaySound(Assets.Get(Sound.Beep), Player.position);
        }
        for (int i = 0; i < 10; i++)
        {
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), Player.position + dir * 10 * i, 0, Color.Green * (1 - (float)(i) / 10)));
        }
        ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), end, 0, Color.White));
    }
}
public class PulseEmitter() : Module(Modules.PulseEmitter)
{
    public override void OnUpdate()
    {
        base.Update();
        if (cooldown > 0)
        {
            return;
        }
        Engine.EntityManager.NearestEnemy(this)?.Reveal(1);
        Engine.EntityManager.NearestProjectile(this, isFriendly)?.Reveal(1);
        cooldown = 2;
        SoundManager.PlayGlobalSound(Assets.Get(Sound.Beep));
    }
}

