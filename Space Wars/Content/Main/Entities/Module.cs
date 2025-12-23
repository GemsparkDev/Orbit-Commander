using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;
using Space_Wars.Content.Main.Particles;
using System;
using System.Collections.Generic;
using UILib.Content.Main;
using System.Linq;
using Microsoft.Xna.Framework.Audio;

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
public class ReloadSystem(int _magazineSize, float _reloadSpeed)
{
    private int rounds = _magazineSize;
    private int magazineSize = _magazineSize;
    float reloadCD = 0;
    public void Update()
    {
        if(reloadCD > 0)
        {
            reloadCD -= Engine.DeltaSeconds;
            if (reloadCD < 0)
            {
                rounds = magazineSize;
            }
        }
        float val = rounds;
        if(reloadCD > 0)
        {
            val = (1 - reloadCD / _reloadSpeed) * (float)(magazineSize);
        }
        if(rounds != magazineSize && reloadCD <= 0 && Input.NewState.IsKeyDown(Keys.R))
        {
            rounds = 0;
            reloadCD = _reloadSpeed;
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Dock));
        }
        UI.PlayerAmmo.SetInterval(val, magazineSize);
    }
    public bool Fire()
    {
        if(rounds > 0)
        {
            rounds--;
            return true;
        }
        else
        {
            if (reloadCD <= 0)
            {
                reloadCD = _reloadSpeed;
                SoundManager.PlayGlobalSound(Assets.Get(Sound.Dock));
            }
            return false;
        }
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
            shieldEffect.Update();
            UI.PlayerSpecialHealth.Colors[0] = Color.Yellow;
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
        if (Util.Random.Next(0, 2) == 0)
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
        Engine.SaveGame.Player.Reveal(1);
    }
    public override void OnUpdate()
    {
        UI.PlayerSpecialHealth.Colors[0] = Color.Orange;
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
            return 0;
        }
        buffer = 0;
        return _damage - (int)Math.Round(buffer);
    }
    public override void OnUpdate()
    {
        UI.PlayerSpecialHealth.Colors[0] = Color.White;
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
        engineParticles.offsetVelocity = Player.velocity;
        engineTime = Math.Clamp(engineTime + Engine.DeltaSeconds, 0, 1);
        float engineTimeModifier = 1 - (1 - engineTime) * (1 - engineTime);
        float fuseRatio = (float)(Player.CountFuses(ModuleType.Engines)) / 3;
        engineParticles.speedOfEmission = Math.Max(450f * fuseRatio * engineTimeModifier, 10);
        if (Player.direction != Vector2.Zero)
        {
            Player.velocity += Vector2.Normalize(Player.direction) * 24 * Engine.DeltaSeconds * engineTimeModifier * fuseRatio / (Player.leashedMaterials.Count + 2);
            engineParticles.position = Player.position - Vector2.Normalize(Player.direction) * 8 - Player.velocity;
            engineParticles.sprayAngle = Util.ToAngle(Player.direction) + MathF.PI;
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
        var dir = Vector2.Normalize(-Player.direction + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 10);
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
        engineParticles.offsetVelocity = Player.velocity;
        engineTime = Math.Clamp(engineTime + Engine.DeltaSeconds / 3, 0, 1);
        float engineTimeModifier = 1 - (1 - engineTime) * (1 - engineTime);
        float fuseRatio = (float)(Player.CountFuses(ModuleType.Engines)) / 3;
        engineParticles.speedOfEmission = Math.Max(450f * fuseRatio * engineTimeModifier, 10);
        if (Player.direction != Vector2.Zero)
        {
            Player.velocity += Vector2.Normalize(Player.direction) * 14 * Engine.DeltaSeconds * engineTimeModifier * fuseRatio; 
            engineParticles.position = Player.position - Vector2.Normalize(Player.direction) * 8 - Player.velocity;
            engineParticles.sprayAngle = Util.ToAngle(Player.direction) + MathF.PI;
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
    private ReloadSystem ammo = new ReloadSystem(18, 2);
    public override void OnShoot()
    {
        if (cooldown > 0)
        {
            return;
        }
        if(ammo.Fire())
        {
            Vector2 vel = Player.IdealSpeedWithVelocity(9) + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 2;
            Engine.EntityManager.Add(new PulseShot(Player.position, vel, Util.ToAngle(vel - Player.velocity), 0, true, 3));
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.position);
            cooldown = 0.2f;
            Engine.ShakeScreen(0.3f);
            Engine.Camera.Position += Player.Direction * 8 + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
            Player.velocity -= Player.Direction / 3;
            Util.FiringParticles(Player.position + Player.Direction * 8, Player.velocity, Player.Direction);
        }
    }
    public override void OnUpdate()
    {
        ammo.Update();
        base.OnUpdate();
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
    ReloadSystem ammo = new ReloadSystem(1, 1.5f);
    public override void OnShoot()
    {
        if (cooldown > 0)
        {
            return;
        }
        if(ammo.Fire())
        {
            List<Entity> entities = Engine.EntityManager.Hitscan(Player.position, Player.Direction, 3000, true, out Vector2 end);
            foreach (var entity in entities)
            {
                entity.Collide(30);
            }
            SoundManager.PlaySound(Assets.Get(Sound.SniperFire), Player.position);
            Engine.Camera.Position += Player.Direction * 30 + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
            cooldown = 0.5f;
            Engine.ShakeScreen(0.7f);
            Player.velocity -= Player.Direction * 6;
            float distance = (end - Player.position).Length() / 4;
            for (int i = 0; i < distance; i++)
            {
                ParticleManager.Add(new Particle(Assets.Get(Sprite.Dot), 2, Player.position + Player.Direction * 4 * i, Vector2.Zero, Util.ToAngle(Player.Direction), 0, Color.Red, Color.Transparent));
            }
            Util.FiringParticles(Player.position, Player.velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
        }
    }
    public override void OnUpdate()
    {
        ammo.Update();
        base.OnUpdate();
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
        if (cooldown > 0)
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
            Engine.EntityManager.Add(new PulseShot(Player.position + positionOffset, targetVector * (1 + Util.OneToNegOne() / 10), Util.ToAngle(Player.Direction) + offsetAngle, 0, true, 2));
        }
        SoundManager.PlaySound(Assets.Get(Sound.ShotgunFire), Player.position);
        Player.velocity -= Player.Direction / 2;
        cooldown = 1f;
        Engine.ShakeScreen(0.4f);
    }
}
public class Missile() : Module(Modules.Missile)
{
    private ReloadSystem ammo = new ReloadSystem(8, 2f);
    public override void OnShoot()
    {
        if (cooldown > 0)
        {
            return;
        }
        if(ammo.Fire())
        {
            Engine.EntityManager.Add(Enemy.NewMissile(Player.position + new Vector2(Player.Direction.Y, -Player.Direction.X) * 6, Player.IdealSpeedWithVelocity(9), Util.ToAngle(Player.Direction), true));
            SoundManager.PlaySound(Assets.Get(Sound.MissileFire), Player.position);
            cooldown = 0.5f;
            Engine.ShakeScreen(0.5f);
        }
    }
    public override void OnUpdate()
    {
        if (Util.Random.NextSingle() > 0.33f)
        {
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), cooldown, Player.position - Player.Direction * 8 - Player.velocity, Player.velocity - Player.Direction * cooldown * 2 + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) * cooldown / 2, 0, 0, Color.Gray * (1 - (1 - cooldown * 2) * (1 - cooldown * 2)), Color.Transparent));
        }
        ammo.Update();
        base.OnUpdate();
    }
}
public class LMG() : Module(Modules.LMG)
{
    private ReloadSystem ammo = new ReloadSystem(80, 4);
    public override void OnShoot()
    {
        if (cooldown > 0)
        {
            return;
        }
        if(ammo.Fire())
        {
            Vector2 offset = new Vector2(Player.Direction.Y, -Player.Direction.X) * Util.Random.Next(-2, 3) + Util.ToUnitVector(Player.angle) * 8;
            Texture2D dot = Assets.Get(Sprite.Microshot);
            Projectile shot = new PulseShot(Player.position + offset, Player.Player.IdealSpeedWithVelocity(12) + offset / 4, Util.ToAngle(Player.Direction), 0, true, 2)
            {
                texture = dot,
                timeLeft = 3
            };
            Engine.EntityManager.Add(shot);
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.position);
            Engine.ShakeScreen(0.1f);
            Engine.Camera.Position += Player.Direction * 6 + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
            Player.velocity -= Player.Direction / 6;
            cooldown = 0.1f;
            Util.FiringParticles(Player.position + Player.Direction * 8, Player.velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
        }
    }
    public override void OnUpdate()
    {
        ammo.Update();
        base.OnUpdate();
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
        Projectile shot = new PulseShot(Player.position + offset, Player.Player.IdealSpeedWithVelocity(8) + offset / 4, Util.ToAngle(Player.Direction), 0, true, 8, true, 1)
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
        Engine.EntityManager.Add(new FlameBolt(Player.position, Player.IdealSpeedWithVelocity(8) + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 2, true, 4, 4, 0.5f));
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
    private bool isFiring = false;
    private SoundEffectInstance beam = Assets.Get(Sound.LMGFire).CreateInstance();
    private SoundEffectInstance beamBlend;
    private float duration = (float)Assets.Get(Sound.LMGFire).Duration.TotalSeconds;
    private float timeLeft = 0;
    public override void OnShoot()
    {
        isFiring = true;
        timeLeft += Engine.DeltaSeconds;
        if(timeLeft > duration)
        {
            timeLeft = 0;
        }
        Vector2 dir = Util.ToUnitVector(Util.ToAngle(Player.Direction));
        List<Entity> enemies = Engine.EntityManager.Hitscan(Player.position, dir, 250, true, out Vector2 _end);
        float end = (_end - Player.position - dir * 10).Length() / 5;
        for (float i = 0; i < end; i++)
        {
            float lerp = i / 50;
            Vector3 color = new Vector3(0, 1, 1) * (1 - lerp) + new Vector3(1, 1, 0) * (lerp);
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), dir * (i + 2f) * 5 + Player.position + new Vector2(dir.Y, -dir.X) * MathF.Sin(i / 2 - time * 5) / 2, Util.ToAngle(Player.Direction), new Color(color.X, color.Y, color.Z) * (1 - (lerp))));
        }
        if (1 - end / 40 > Util.Random.NextSingle() * 2f)
        {
            ParticleManager.Add(new Particle(Assets.Get(Sprite.Circle), 1, _end, Util.ToUnitVector(Util.Random.NextSingle() * MathF.Tau) * Util.Random.NextSingle() / 2 + Vector2.Normalize(Engine.SaveGame.CurrentMission.GetNormalizedAcceleration(_end)) / 1.5f, 0, 0, Color.DarkGray, Color.Transparent));
        }
        if (cooldown > 0)
        {
            return;
        }
        cooldown = 0.1f;
        foreach (var enemy in enemies)
        {
            if (enemy is Enemy)
            {
                enemy.Collide(1);
                enemy.ApplyWork(-10);
            }
        }
    }
    public override void OnUpdate()
    {
        if(isFiring)
        {
            //Interpolating looped sound effect with new sound prevents noticable sound jumping
            float lerp = 1;
            if(timeLeft < 1)
            {
                lerp = timeLeft;
            }
            else if(duration - timeLeft < 1)
            {
                lerp = duration - timeLeft;
            }
            if (lerp < 1)
            {
                beamBlend ??= Assets.Get(Sound.LMGFire).CreateInstance();
                beamBlend.Volume = (1 - lerp);
                beamBlend.Play();
            }
            else if(beamBlend != null)
            {
                beamBlend.Dispose();
                beamBlend = null;
            }
            beam.Volume = lerp;
            beam.Play();
        }
        else
        {
            beam.Pause();
            beamBlend?.Pause();
        }
        time += Engine.DeltaSeconds;
        base.OnUpdate();
        isFiring = false;
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
        Vector2 vel = Player.IdealSpeedWithVelocity(12);
        Engine.EntityManager.Add(new FlameBolt(Player.position, vel + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 2, true, 6,
            new ParticleEmitter(Assets.Get(Sprite.Circle), Player.position, 0, Color.Cyan) { sprayCone = MathF.PI * 2 / 3, sprayAngle = Util.ToAngle(vel - Player.velocity), speedOfEmission = 0.5f }, 4, 0, -20));
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
public class SplitterModule() : Module(Modules.SplitterModule)
{
    public override void OnShoot()
    {
        if (cooldown > 0)
        {
            return;
        }
        List<Entity> missiles = [];
        for (int i = 0; i < 3; i++)
        {
            missiles.Add(Enemy.NewMissile(position, velocity, 0, true, 1));
        }
        Engine.EntityManager.Add(new Splitter(Player.position, Player.IdealSpeedWithVelocity(8), Util.ToAngle(Player.Direction), Player.isFriendly, 8, missiles, 0.5f));
        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Player.position);
        Engine.ShakeScreen(0.2f);
        Player.velocity -= Player.Direction / 2;
        cooldown = 2f;
    }
}
public class Fractal() : Module(Modules.Fractal)
{
    public override void OnShoot()
    {
        if (cooldown > 0)
        {
            return;
        }
        List<Entity> splitters = [];
        for (int i = 0; i < 3; i++)
        {
            List<Entity> finalBullets = [];
            for (int j = 0; j < 3; j++)
            {
                finalBullets.Add(new PulseShot(position, velocity, 0, 0, Player.isFriendly, 3, false, 1));
            }
            splitters.Add(new Splitter(position, velocity, 0, Player.isFriendly, 5, finalBullets, 0.1f, 1));
        }
        Engine.EntityManager.Add(new Splitter(Player.position, Player.IdealSpeedWithVelocity(8), Util.ToAngle(Player.Direction), Player.isFriendly, 8, splitters, 0.1f));
        SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Player.position);
        Engine.ShakeScreen(0.1f);
        Player.velocity -= Player.Direction / 2;
        cooldown = 0.5f;
    }
}
public class CrackShot() : Module(Modules.CrackShot)
{
    public override void OnShoot()
    {
        if (cooldown > 0)
        {
            return;
        }
        Engine.EntityManager.Add(new Splitter(Player.position, Player.IdealSpeedWithVelocity(8), Util.ToAngle(Player.Direction), Player.isFriendly, 3, [new AssassinShot(position, default, 0, 0, Player.isFriendly, 3, 0)], 0.2f, 0, true));
        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.position);
        Engine.ShakeScreen(0.1f);
        Player.velocity -= Player.Direction / 2;
        cooldown = 0.25f;
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
    Enemy shield1;
    Enemy shield2;
    const float MaxCooldown = 15;
    public override void OnAbility()
    {
        if (cooldown > 0 || (shield1 != null && shield2 != null))
        {
            return;
        }
        shield1 = Enemy.NewShield(Player, 12, 20, MathF.PI / 4, 1, true);
        Engine.EntityManager.Add(shield1);
        shield2 = Enemy.NewShield(Player, 12, 20, -MathF.PI / 4, 1, true);
        Engine.EntityManager.Add(shield2);
        cooldown = MaxCooldown;
    }
    public override void OnUpdate()
    {
        if (shield1 != null && shield1.isExpired)
        {
            shield1 = null;
        }
        if (shield2 != null && shield2.isExpired)
        {
            shield2 = null;
        }
        UI.PlayerAbility.SetInterval(1 - cooldown / MaxCooldown, 1);
        base.OnUpdate();
    }
}
public class SummonGrapplingHook() : Module(Modules.GrapplingHook)
{
    const float MaxCooldown = 5;
    GrapplingHook hook;
    Planet p = null;
    Vector2 offset = Vector2.Zero;
    public override void OnAbility()
    {
        if (hook != null)
        {
            bool hasHooked = false;
            if (hook.Parent == Player)
            {
                var mousePos = new Vector2(Input.NewMouseState.X, Input.NewMouseState.Y) + Engine.Camera.Position - Engine.BackBuffer / 2;
                foreach (var planet in Engine.SaveGame.CurrentMission.Planets)
                {
                    var planetDir = Vector2.Normalize(mousePos + planet.position);
                    Engine.WriteLine(mousePos);
                    Engine.WriteLine(planet.position + planetDir * planet.radius);
                    if (Vector2.DistanceSquared(mousePos, planet.position + planetDir * planet.radius) < 10000)
                    {
                        hook.Parent = new Enemy(planet.position + planetDir * planet.radius, Vector2.Zero, 0, 0, 1, null, true);
                        hasHooked = true;
                        p = planet;
                        offset = planetDir * planet.radius;
                        Engine.WriteLine("Ahh");
                        break;
                    }
                }
                foreach (var entity in Engine.EntityManager.Entities)
                {
                    if(Vector2.DistanceSquared(mousePos, entity.position) < 10000)
                    {
                        hook.Parent = entity;
                        hasHooked = true;
                        break;
                    }
                }
            }
            if(!hasHooked)
            {
                cooldown /= 2;
                hook.isExpired = true;
                hook = null;
                p = null;
            }
            return;
        }
        if (cooldown > 0)
        {
            return;
        }
        hook = new GrapplingHook(Player.position, Player.IdealSpeedWithVelocity(50), Util.ToAngle(Player.Direction), Player);
        p = null;
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
            p = null;
        }
        if (p != null)
        {
            hook.Parent.position = p.position + offset;
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
        allies = [.. allies.Where(x => !x.isExpired)];
        base.Update();
    }
}
public class Assault() : Module(Modules.Assault)
{
    bool isShooting = false;
    const float MaxCooldown = 30;
    float count;
    float resistanceCooldown = 0;
    public override void OnAbility() 
    {
        if (isShooting || cooldown > 0)
        {
            return;
        }
        resistanceCooldown = 3;
        count = 1;
        for (float angle = 0; angle < MathF.Tau; angle += MathF.PI / 4)
        {
            Engine.EntityManager.Add(new PulseShot(Player.position, Util.ToUnitVector(angle) * 10, angle, 0, Player.isFriendly, 10, true, 1));
        }
        isShooting = true;
        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.position);
    }
    public override int OnCollide(int _damage)
    {
        if(resistanceCooldown > 0)
        {
            return damage * 4 / 5;
        }
        return damage;
    }
    public override void OnUpdate()
    {
        if(resistanceCooldown > 0)
        {
            resistanceCooldown -= Engine.DeltaSeconds;
        }
        if (isShooting && cooldown <= 0)
        {
            Engine.EntityManager.Add(new PulseShot(Player.position, Util.ToUnitVector(count * 1.61803398875f) * 10, count * 1.61803398875f, 0, Player.isFriendly, 10, true, 1));
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.position);
            cooldown = 0.1f;
            count++;
            if (count >= 11)
            {
                count = 0;
                cooldown = 30;
                isShooting = false;
            }
        }
        UI.PlayerAbility.SetInterval(1 - cooldown / MaxCooldown, 1);
        base.OnUpdate();
    }
}
public class Decoy() : Module(Modules.Decoy)
{
    public override void OnAbility()
    {
        if (cooldown <= 0)
        {
            return;
        }
        Engine.EntityManager.Add(Enemy.NewDecoy(Engine.SaveGame.Player.position, Vector2.Zero, Engine.SaveGame.Player.angle, Sprite.Player, Engine.SaveGame.Player.isFriendly));
        cooldown = 15f;
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

