using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Space_Wars.Content.Main.Components;
using Space_Wars.Content.Main.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using UILib.Content.Main;

namespace Space_Wars.Content.Main.Entities;

public abstract class Module : Pickup, IData
{
    //Serialized fields
    public bool isFailed = false;
    public new Modules Type { get; }
    Color IData.Color => isFailed ? Color.Red : Color.White;
    private Decal healthDecal;
    public float Cooldown { get; protected set; } = 0;

    public Module(Modules _type, Vector2 _position = default, Vector2 _velocity = default, float _angularVelocity = 0)
        : base(ItemFactory.moduleData[_type], _position, _velocity, _angularVelocity, ItemFactory.moduleData[_type].MaxHealth)
    {
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
        if (Cooldown > 0)
        {
            Cooldown -= Engine.DeltaSeconds;
        }
        UpdateHealth();
    }
    public virtual void OnEngine() { }
    public virtual void OnAbility() { }
    //Override to provide custom serialization for modules
    public virtual void Parse(Modules _type, List<string> _disassembly, LoadLogger _logger)
    {
        _logger.Try(delegate { Health = Int32.Parse(_disassembly[2]); }, 2);
        _logger.Try(delegate { isFailed = bool.Parse(_disassembly[3]); }, 3);
        UpdateHealth();
        base.Parse(_disassembly, _logger);
    }
    public override string Serialize()
    {
        return $"{{{Type},{SerializeAttributes()},{Health},{isFailed}}}";
    }
}
public class ReloadSystem(int _magazineSize, float _reloadSpeed, Action _reloadCallback = null)
{
    private int rounds = _magazineSize;
    private int magazineSize = _magazineSize;
    float reloadCD = 0;
    public void Update(Module _module)
    {
        if (reloadCD > 0)
        {
            reloadCD -= Engine.DeltaSeconds;
            if (reloadCD < 0)
            {
                rounds = magazineSize;
            }
        }
        float val = rounds;
        if (reloadCD > 0)
        {
            val = (1 - reloadCD / _reloadSpeed) * (float)(magazineSize);
        }
        if (rounds != magazineSize && reloadCD <= 0 && Input.NewState.IsKeyDown(Keys.R))
        {
            rounds = 0;
            reloadCD = _reloadSpeed;
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Dock));
        }
        if (Engine.SaveGame.Player.modules.ContainsValue(_module))
        {
            UI.PlayerAmmo.SetInterval(val, magazineSize);
        }
    }
    public bool Fire()
    {
        if (rounds > 0)
        {
            rounds--;
            return true;
        }
        else
        {
            if (reloadCD <= 0)
            {
                reloadCD = _reloadSpeed;
                _reloadCallback?.Invoke();
            }
            return false;
        }
    }
}
public class ModuleData(Sprites _realSprite, Sprites _virtualSprite, String _name, int _id, int _health, Type _type, Color? _textColor = null)
    : ItemData(_realSprite, _virtualSprite, _name, _id, Color.White, _textColor, _health)
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
    private ParticleEmitter shieldEffect = new(Assets.Get(Sprites.Dot), Vector2.Zero, 10, Color.Violet) { particleAngularVelocity = 0.1f };
    public override int OnCollide(int _damage)
    {
        if (Cooldown <= 0)
        {
            Cooldown = 8;
            return 0;
        }
        return (int)(_damage * 1.5f);
    }
    public override void OnUpdate()
    {
        if (Cooldown <= 0)
        {
            shieldEffect.position = Player.Position;
            shieldEffect.Update();
            UI.PlayerSpecialHealth.Colors[0] = Color.Yellow;
        }
        base.OnUpdate();
    }
}
public class StealthHull() : Module(Modules.Stealth)
{
    public override int OnCollide(int _damage)
    {
        return (int)(_damage / 1.75f);
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
                Engine.SaveGame.CurrentMission.Add(NewAssassinShot(Player.Position, Util.ToUnitVector(angle) * 8, angle, 0, Team, 6, 1));
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
    ParticleEmitter effect = new ParticleEmitter(Assets.Get(Sprites.Dot), Vector2.Zero, 10, Color.Orange) { sprayAngle = MathF.PI / 2 };
    public override int OnCollide(int _damage)
    {
        float dr = 0.5f * ((1 - Cooldown / 5) * (1 - Cooldown / 5) + 0.5f);
        Cooldown = 5;
        Engine.SaveGame.Player.RevealDuration = 1;
        return (int)(_damage * dr);
    }
    public override void OnUpdate()
    {
        UI.PlayerSpecialHealth.Colors[0] = Color.Orange;
        UI.PlayerSpecialHealth.SetInterval(1 - (1 - Cooldown / 5) * (1 - Cooldown / 5), 1);
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
        effect.position = Player.Position;
        if (flipped == 1)
        {
            effect.sprayCone = MathF.Tau * time;
        }
        else
        {
            effect.sprayCone = MathF.Tau * (1 - time);
        }
        effect.particleColor = Color.Orange * (1 - (1 - Cooldown / 5) * (1 - Cooldown / 5));
        effect.Update();
        base.OnUpdate();
    }
}
public class Ablative() : Module(Modules.Ablative)
{
    float buffer = 25;
    public override int OnCollide(int _damage)
    {
        Cooldown = 1;
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
        UI.PlayerSpecialHealth.Colors[0] = Color.Cyan;
        UI.PlayerSpecialHealth.SetInterval(buffer, 25f);
        if (Cooldown <= 0 && buffer < 25)
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
            Player.Statuses.ApplyStatus(new Berserk(_damage));
            return _damage * 2 / 3;
        }
        return _damage / 2;
    }

}
public class ThermalShield() : Module(Modules.ThermalShield)
{
    public override int OnCollide(int _damage)
    {
        if (Player.Temperature is > 1 or < (-1))
        {
            return _damage * 2 / 3;
        }
        return _damage / 2;
    }
    public override void OnUpdate()
    {
        Player.ApplyWork(Math.Sign(-Player.Temperature));
        base.OnUpdate();
    }
}
public class StandardEngine() : Module(Modules.Engines)
{
    float engineTime = 0;
    ParticleEmitter engineParticles = new(Assets.Get(Sprites.Circle), 0.15f, Vector2.Zero, 0, MathF.PI / 4, 2, 450f, Color.Cyan, EmitterType.EmissionOverTime)
    { particleFadeToColor = new Color(72, 61, 139, 0) };
    public override void OnEngine()
    {
        engineParticles.offsetVelocity = Player.Velocity;
        engineTime = Math.Clamp(engineTime + Engine.DeltaSeconds, 0, 1);
        float engineTimeModifier = 1 - (1 - engineTime) * (1 - engineTime);
        float fuseRatio = (float)(Player.CountFuses(ModuleType.Engines)) / 3;
        engineParticles.speedOfEmission = Math.Max(450f * fuseRatio * engineTimeModifier, 10);
        if (Player.direction != Vector2.Zero)
        {
            Player.Velocity += Vector2.Normalize(Player.direction) * 24 * Engine.DeltaSeconds * engineTimeModifier * fuseRatio / (Player.leashedMaterials.Count + 2);
            engineParticles.position = Player.Position - Vector2.Normalize(Player.direction) * 8 - Player.Velocity;
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
    float burstTime = 0;
    public override void OnEngine()
    {
        engineTime = Math.Clamp(engineTime + Engine.DeltaSeconds * 3, 0, 1);
        float engineTimeModifier = 1 - (1 - engineTime) * (1 - engineTime);
        float fuseRatio = (float)(Player.CountFuses(ModuleType.Engines)) / 3;
        var dir = Vector2.Normalize(-Player.direction + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 10);
        for (float i = 0; i < 5 * fuseRatio * engineTimeModifier; i++)
        {
            float lerp = i / (5 * fuseRatio * engineTimeModifier);
            Vector3 color = new Vector3(0, 1, 1) * (1 - lerp) + new Vector3(1, 0.5f, 0) * (lerp);
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), Player.Position + dir * (i + 2.5f) * 4, Player.Angle, new Color(color.X, color.Y, color.Z) * (1 - lerp)));
        }
        if (Player.direction != Vector2.Zero)
        {
            Player.Velocity += Vector2.Normalize(Player.direction) * 20 * Engine.DeltaSeconds * engineTimeModifier * fuseRatio * (0.75f - MathF.Tanh((burstTime - 5) / 2) / 4) / (Player.leashedMaterials.Count + 1);
        }
        if (burstTime < 6)
        {
            burstTime += Engine.DeltaSeconds * 3;
        }
    }
    public override void OnUpdate()
    {
        if (!Player.isEngineActive && engineTime > 0)
        {
            engineTime -= Engine.DeltaSeconds;
        }
        if (burstTime > 0)
        {
            burstTime -= Engine.DeltaSeconds * 2;
        }
    }
}
public class WorkEngine() : Module(Modules.Work)
{
    float engineTime = 0;
    ParticleEmitter engineParticles = new(Assets.Get(Sprites.Circle), 0.15f, Vector2.Zero, 0, MathF.PI / 4, 2, 450f, Color.Orange, EmitterType.EmissionOverTime)
    { particleFadeToColor = new Color(1f, 0.1f, 0, 0) };
    public override void OnEngine()
    {
        engineParticles.offsetVelocity = Player.Velocity;
        engineTime = Math.Clamp(engineTime + Engine.DeltaSeconds / 3, 0, 1);
        float engineTimeModifier = 1 - (1 - engineTime) * (1 - engineTime);
        float fuseRatio = (float)(Player.CountFuses(ModuleType.Engines)) / 3;
        engineParticles.speedOfEmission = Math.Max(450f * fuseRatio * engineTimeModifier, 10);
        if (Player.direction != Vector2.Zero)
        {
            Player.Velocity += Vector2.Normalize(Player.direction) * 14 * Engine.DeltaSeconds * engineTimeModifier * fuseRatio;
            engineParticles.position = Player.Position - Vector2.Normalize(Player.direction) * 8 - Player.Velocity;
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
        if (Cooldown > 0)
        {
            return;
        }
        Cooldown = 0.5f;
        var dir = Vector2.Normalize(Player.direction);
        if (Player.direction != Vector2.Zero)
        {
            Player.Velocity += dir * 4 / (Player.leashedMaterials.Count + 1);
            Util.Explode(Player.Position - dir * 30, Player.Velocity, 10, 28);
            SoundManager.PlaySound(Assets.Get(Sound.ShieldHit), Player.Position);
        }
    }
}
public class Basic() : Module(Modules.Basic)
{
    private ReloadSystem ammo = new ReloadSystem(18, 2);
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            Vector2 vel = Player.IdealSpeedWithVelocity(9) + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 2;
            Engine.SaveGame.CurrentMission.Add(NewPulseShot(Player.Position, vel, Util.ToAngle(vel - Player.Velocity), 0, Team, 3));
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
            Cooldown = 0.2f;
            Engine.ShakeScreen(0.3f);
            Engine.Camera.Position += Player.Direction * 8 + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
            Player.Velocity -= Player.Direction / 3;
            Util.FiringParticles(Player.Position + Player.Direction * 8, Player.Velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
        }
    }
    public override void OnUpdate()
    {
        ammo.Update(this);
        base.OnUpdate();
    }
}
public class Antimaterial() : Module(Modules.Sniper)
{
    ReloadSystem ammo = new ReloadSystem(4, 2f);
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            var p1 = NewAssassinShot(Player.Position, Player.IdealSpeedWithVelocity(20), Util.ToAngle(Player.Direction), 0, Team, 16);
            p1.Texture = Assets.Get(Sprites.Arrow);
            Engine.SaveGame.CurrentMission.Add(p1);
            SoundManager.PlaySound(Assets.Get(Sound.SniperFire), Player.Position);
            Cooldown = 0.75f;
            Engine.Camera.Position += Player.Direction * 12 + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) * 2;
            Engine.ShakeScreen(0.5f);
            Player.Velocity -= Player.Direction / 2;
            Util.FiringParticles(Player.Position + Player.Direction * 8, Player.Velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
        }
    }
    public override void OnUpdate()
    {
        ammo.Update(this);
        base.OnUpdate();
    }
}
public class Railgun() : Module(Modules.Antimaterial)
{
    ReloadSystem ammo = new ReloadSystem(1, 1.5f);
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            List<Entity> entities = Engine.SaveGame.CurrentMission.Hitscan(Player.Position, Player.Direction, 3000, true, out Vector2 end, null);
            foreach (var entity in entities)
            {
                entity.Collide(30);
            }
            SoundManager.PlaySound(Assets.Get(Sound.SniperFire), Player.Position);
            Engine.Camera.Position += Player.Direction * 30 + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
            Cooldown = 0.5f;
            Engine.ShakeScreen(0.7f);
            Player.Velocity -= Player.Direction * 6;
            float distance = (end - Player.Position).Length() / 4;
            for (int i = 0; i < distance; i++)
            {
                ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 2, Player.Position + Player.Direction * 4 * i, Vector2.Zero, Util.ToAngle(Player.Direction), 0, Color.Red, Color.Transparent));
            }
            Util.FiringParticles(Player.Position + Player.Direction * 8, Player.Velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
        }
    }
    public override void OnUpdate()
    {
        ammo.Update(this);
        base.OnUpdate();
    }
}
public class Spiral() : Module(Modules.Spiral)
{
    ReloadSystem ammo = new ReloadSystem(10, 2);
    float offset = 0;
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            Vector2 speed = Player.IdealSpeedWithVelocity(12);
            Engine.SaveGame.CurrentMission.Add(NewSpiralShot(Player.Position, speed, Util.ToAngle(Player.Direction), 0, Team, 5, offset, 1));
            Engine.SaveGame.CurrentMission.Add(NewSpiralShot(Player.Position, speed, Util.ToAngle(Player.Direction), 0, Team, 5, MathF.PI + offset, 1));
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
            Cooldown = 0.5f;
            Engine.ShakeScreen(0.4f);
            Player.Flash(Color.BurlyWood);
            Player.Velocity -= Player.Direction;
            Util.FiringParticles(Player.Position + Player.Direction * 8, Player.Velocity, Player.Direction);
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 60, Player.Position - Player.Velocity, Player.Velocity
                + new Vector2(-Player.Direction.Y + Util.OneToNegOne() / 2, Player.Direction.X + Util.OneToNegOne() / 4), 0, Util.OneToNegOne() / 5, Color.Yellow, Color.Transparent)
            { experienceGravity = true });
            offset += MathF.PI / 8;
            if (offset > MathF.Tau)
            {
                offset -= MathF.Tau;
            }
        }
    }
    public override void OnUpdate()
    {
        ammo.Update(this);
        base.OnUpdate();
    }
}
public class Shotgun() : Module(Modules.Shotgun)
{
    ReloadSystem ammo = new ReloadSystem(20, 3);
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            int randomBulletCount = Util.Random.Next(4, 6);
            for (int i = 0; i < randomBulletCount; i++)
            {
                float angleDegrees = (Util.Random.NextSingle() - 0.5f) * 5;
                float offsetAngle = angleDegrees * MathF.PI / 180;
                Vector2 targetVector = Player.IdealSpeedWithVelocity(10) + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
                Vector2 positionOffset = new Vector2(Player.Direction.Y, -Player.Direction.X) * offsetAngle * 100;
                Engine.SaveGame.CurrentMission.Add(NewPulseShot(Player.Position + positionOffset, targetVector * (1 + Util.OneToNegOne() / 10), Util.ToAngle(Player.Direction) + offsetAngle, 0, Team, 2));
            }
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
            Player.Velocity -= Player.Direction / 2;
            Cooldown = 0.5f;
            Engine.Camera.Position += Player.Direction * 10 + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
            Engine.ShakeScreen(0.5f);
            Util.FiringParticles(Player.Position + Player.Direction * 8, Player.Velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
        }
    }
    public override void OnUpdate()
    {
        ammo.Update(this);
        base.OnUpdate();
    }
}
public class Missile() : Module(Modules.Missile)
{
    private ReloadSystem ammo = new ReloadSystem(8, 2f);
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            Engine.SaveGame.CurrentMission.Add(Entity.NewMissile(Player.Position + new Vector2(Player.Direction.Y, -Player.Direction.X) * 6, Player.IdealSpeedWithVelocity(9), Util.ToAngle(Player.Direction), Team));
            SoundManager.PlaySound(Assets.Get(Sound.MissileFire), Player.Position);
            Cooldown = 0.5f;
            Engine.ShakeScreen(0.5f);
        }
    }
    public override void OnUpdate()
    {
        if (Util.Random.NextSingle() > 0.33f)
        {
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), Cooldown, Player.Position - Player.Direction * 8 - Player.Velocity, Player.Velocity - Player.Direction * Cooldown * 2 + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) * Cooldown / 2, 0, 0, Color.Gray * (1 - (1 - Cooldown * 2) * (1 - Cooldown * 2)), Color.Transparent));
        }
        ammo.Update(this);
        base.OnUpdate();
    }
}
public class LMG() : Module(Modules.LMG)
{
    private ReloadSystem ammo = new ReloadSystem(80, 4);
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            Vector2 offset = new Vector2(Player.Direction.Y, -Player.Direction.X) * Util.Random.Next(-2, 3) + Util.ToUnitVector(Player.Angle) * 8;
            Texture2D dot = Assets.Get(Sprites.Microshot);
            var shot = NewPulseShot(Player.Position + offset, Player.Player.IdealSpeedWithVelocity(12) + offset / 4, Util.ToAngle(Player.Direction), 0, Team, 2);
            shot.Texture = dot;
            shot.GetComponent<ExpireTimer>().TimeLeft = 3;
            Engine.SaveGame.CurrentMission.Add(shot);
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
            Engine.ShakeScreen(0.1f);
            Engine.Camera.Position += Player.Direction * 6 + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
            Player.Velocity -= Player.Direction / 6;
            Cooldown = 0.1f;
            Util.FiringParticles(Player.Position + Player.Direction * 8, Player.Velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
        }
    }
    public override void OnUpdate()
    {
        ammo.Update(this);
        base.OnUpdate();
    }
}
public class Crossbow() : Module(Modules.Crossbow)
{
    //Does not use cooldown, as that decreases player stealth
    ReloadSystem ammo = new ReloadSystem(1, 1.5f);
    public override void OnShoot()
    {
        if (ammo.Fire())
        {
            Vector2 offset = new Vector2(Player.Direction.Y, -Player.Direction.X) * Util.Random.Next(-2, 3);
            var shot = NewPulseShot(Player.Position + offset, Player.IdealSpeedWithVelocity(15) + offset / 4, Util.ToAngle(Player.Direction), 0, Team, 18, true, 1);
            shot.Texture = Assets.Get(Sprites.CrossbowShot);
            Engine.SaveGame.CurrentMission.Add(shot);
            Engine.Camera.Position += Player.Direction * 6;
            Engine.ShakeScreen(0.2f);
            Player.Velocity -= Player.Direction / 4;
        }
    }
    public override void OnUpdate()
    {
        ammo.Update(this);
        base.OnUpdate();
    }
}
public class Flamethrower() : Module(Modules.Flamethrower)
{
    ReloadSystem ammo = new ReloadSystem(60, 1, delegate ()
    { ParticleManager.Add(new Particle(Assets.Get(Sprites.Cog), 60, Player.Position, Player.Velocity + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()), 0, Util.OneToNegOne() / 2, Color.Green, Color.Transparent) { experienceGravity = true }); });
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            Engine.SaveGame.CurrentMission.Add(new FlameBolt(Player.Position, Player.IdealSpeedWithVelocity(5) + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 4, Team, 1));
            SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Player.Position);
            Player.Velocity -= Player.Direction / 10;
            Cooldown = 0.08f;
            Engine.ShakeScreen(0.1f);
            Player.Flash(Color.Orange);
        }
    }
    public override void OnUpdate()
    {
        ammo.Update(this);
        base.OnUpdate();
    }
}
public class Fireball() : Module(Modules.Fireball)
{
    ReloadSystem ammo = new ReloadSystem(3, 2);
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            Engine.SaveGame.CurrentMission.Add(new FlameBolt(Player.Position, Player.IdealSpeedWithVelocity(8) + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 2, Team, 4, 4, 0.5f));
            SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Player.Position);
            Cooldown = 0.5f;
            Engine.ShakeScreen(0.3f);
            Player.Flash(Color.Orange);
        }
    }
    public override void OnUpdate()
    {
        ammo.Update(this);
        base.OnUpdate();
    }
}
public class GrenadeLauncher() : Module(Modules.GrenadeLauncher)
{
    ReloadSystem ammo = new ReloadSystem(8, 3);
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            Engine.SaveGame.CurrentMission.Add(NewExplosive(Player.Position, Player.IdealSpeedWithVelocity(8) + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()), Util.ToAngle(Player.Direction), Util.OneToNegOne() / 8, Team, 16, 40, 1));
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
            Cooldown = 0.8f;
            Engine.ShakeScreen(0.4f);
            Player.Velocity -= Player.Direction / 2;
            Engine.Camera.Position += Player.Direction * 8 + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 2;
            Util.FiringParticles(Player.Position + Player.Direction * 8, Player.Velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
        }
    }
    public override void OnUpdate()
    {
        ammo.Update(this);
        base.OnUpdate();
    }
}
public class SpewerModule() : Module(Modules.Spewer)
{
    ReloadSystem ammo = new ReloadSystem(3, 5);
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            Engine.SaveGame.CurrentMission.Add(NewSpewer(Player.Position, Player.IdealSpeedWithVelocity(4) + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 2, Util.ToAngle(Player.Direction), Util.OneToNegOne() / 8, Team, 2));
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
            Cooldown = 1f;
            Engine.ShakeScreen(0.6f);
            Player.Velocity -= Player.Direction;
            Engine.Camera.Position += Player.Direction * 12 + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
            Util.FiringParticles(Player.Position + Player.Direction * 8, Player.Velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
        }
    }
    public override void OnUpdate()
    {
        ammo.Update(this);
        base.OnUpdate();
    }
}
public class PrismArray() : Module(Modules.PrismArray)
{
    float time = 0;
    private bool isFiring = false;
    private SoundEffectInstance beam = Assets.Get(Sound.FireLaser).CreateInstance();
    private SoundEffectInstance beamBlend;
    private float duration = (float)Assets.Get(Sound.FireLaser).Duration.TotalSeconds;
    private float timeLeft = 0;
    public override void OnShoot()
    {
        isFiring = true;
        timeLeft += Engine.DeltaSeconds;
        if (timeLeft > duration)
        {
            timeLeft = 0;
        }
        Vector2 dir = Util.ToUnitVector(Util.ToAngle(Player.Direction));
        List<Entity> enemies = Engine.SaveGame.CurrentMission.Hitscan(Player.Position, dir, 250, true, out Vector2 _end, null);
        float end = (_end - Player.Position - dir * 10).Length() / 5;
        for (float i = 0; i < end; i++)
        {
            float lerp = i / 50;
            Vector3 color = new Vector3(0, 1, 1) * (1 - lerp) + new Vector3(1, 1, 0) * (lerp);
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), dir * (i + 2f) * 5 + Player.Position + new Vector2(dir.Y, -dir.X) * MathF.Sin(i / 2 - time * 5) / 2, Util.ToAngle(Player.Direction), new Color(color.X, color.Y, color.Z) * (1 - (lerp))));
        }
        if (1 - end / 40 > Util.Random.NextSingle() * 2f)
        {
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), 1, _end, Util.ToUnitVector(Util.Random.NextSingle() * MathF.Tau) * Util.Random.NextSingle() / 2 + Vector2.Normalize(Engine.SaveGame.CurrentMission.GetNormalizedAcceleration(_end)) / 1.5f, 0, 0, Color.DarkGray, Color.Transparent));
        }
        if (Cooldown > 0)
        {
            return;
        }
        Cooldown = 0.1f;
        foreach (var enemy in enemies)
        {
            if (enemy.GetComponent<Health>() != null)
            {
                enemy.Collide(1);
                enemy.ApplyWork(-10);
            }
        }
    }
    public override void OnUpdate()
    {
        if (isFiring)
        {
            //Interpolating looped sound effect with new sound prevents noticable sound jumping
            float lerp = 1;
            if (timeLeft < 1)
            {
                lerp = timeLeft;
            }
            else if (duration - timeLeft < 1)
            {
                lerp = duration - timeLeft;
            }
            if (lerp < 1)
            {
                beamBlend ??= Assets.Get(Sound.FireLaser).CreateInstance();
                beamBlend.Volume = (1 - lerp);
                beamBlend.Play();
            }
            else if (beamBlend != null)
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
    ReloadSystem ammo = new ReloadSystem(3, 2);
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            Vector2 vel = Player.IdealSpeedWithVelocity(12);
            Engine.SaveGame.CurrentMission.Add(new FlameBolt(Player.Position, vel + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 2, Team, 6,
                new ParticleEmitter(Assets.Get(Sprites.Circle), Player.Position, 0, Color.Cyan) { sprayCone = MathF.PI * 2 / 3, sprayAngle = Util.ToAngle(vel - Player.Velocity), speedOfEmission = 0.5f }, 4, 0, -20));
            SoundManager.PlaySound(Assets.Get(Sound.SniperFire), Player.Position);
            Cooldown = 1.5f;
            Engine.Camera.Position += Player.Direction * 12 + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
            Engine.ShakeScreen(0.5f);
            Player.Flash(Color.Cyan);
        }
    }
    public override void OnUpdate()
    {
        ammo.Update(this);
        base.OnUpdate();
    }
}
public class Torch() : Module(Modules.Torch)
{
    ReloadSystem ammo = new ReloadSystem(8, 1);
    int count = 0;
    float betweenShots = 0;
    public override void OnShoot()
    {
        if (Cooldown > 0 || count > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            count++;
        }
    }
    public override void OnUpdate()
    {
        if (count > 0)
        {
            if (betweenShots <= 0)
            {
                betweenShots = 0.05f;
                Vector2 offset = new Vector2(Player.Direction.Y, -Player.Direction.X) * Util.OneToNegOne() * 3;
                var shot = new FlameBolt(Player.Position - offset * 5, Player.IdealSpeedWithVelocity(12) + offset / 3, Team, 2, 2, 0.1f, 0, 20);
                Engine.SaveGame.CurrentMission.Add(shot);
                SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
                Engine.ShakeScreen(0.2f);
                Util.FiringParticles(Player.Position + Player.Direction * 8, Player.Velocity, Player.Direction);
                Engine.Camera.Position += Player.Direction * 6;
                Player.Velocity -= Player.Direction / 6;
                Player.Flash(Color.Orange);
                count++;
            }
            else
            {
                betweenShots -= Engine.DeltaSeconds;
            }
            if (count > 3)
            {
                count = 0;
                betweenShots = 0;
                Cooldown = 0.25f;
            }
        }
        ammo.Update(this);
        base.OnUpdate();
    }
}
public class SplitterModule() : Module(Modules.SplitterModule)
{
    ReloadSystem ammo = new ReloadSystem(6, 3);
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            List<Entity> missiles = [];
            for (int i = 0; i < 3; i++)
            {
                missiles.Add(Entity.NewMissile(Position, Velocity, 0, Team, 1));
            }
            Engine.SaveGame.CurrentMission.Add(NewSplitter(Player.Position, Player.IdealSpeedWithVelocity(8), Util.ToAngle(Player.Direction), Team, 8, missiles, 0.5f));
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
            Engine.ShakeScreen(0.5f);
            Player.Velocity -= Player.Direction;
            Cooldown = 0.75f;
            Engine.Camera.Position += Player.Direction * 12 + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
            Util.FiringParticles(Player.Position + Player.Direction * 8, Player.Velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
        }
    }
    public override void OnUpdate()
    {
        ammo.Update(this);
        base.OnUpdate();
    }
}
public class Fractal() : Module(Modules.Fractal)
{
    ReloadSystem ammo = new ReloadSystem(10, 3);
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            List<Entity> splitters = [];
            for (int i = 0; i < 3; i++)
            {
                List<Entity> finalBullets = [];
                for (int j = 0; j < 8; j++)
                {
                    finalBullets.Add(NewPulseShot(Position, Velocity, 0, 0, Team, 3, false, 1));
                }
                var p2 = NewSplitter(Position, Velocity, 0, Team, 5, finalBullets, 0.2f, 1);
                p2.Texture = Assets.Get(Sprites.Glow);
                splitters.Add(p2);
            }
            var p1 = NewSplitter(Player.Position, Player.IdealSpeedWithVelocity(6), Util.ToAngle(Player.Direction), Team, 8, splitters, 0.2f);
            p1.Texture = Assets.Get(Sprites.Glow);
            Engine.SaveGame.CurrentMission.Add(p1);
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
            Engine.ShakeScreen(0.3f);
            Player.Velocity -= Player.Direction / 2;
            Cooldown = 0.25f;
            Engine.Camera.Position += Player.Direction * 8 + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
            Util.FiringParticles(Player.Position + Player.Direction * 8, Player.Velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
        }
    }
    public override void OnUpdate()
    {
        ammo.Update(this);
        base.OnUpdate();
    }
}
public class CrackShot() : Module(Modules.CrackShot)
{
    ReloadSystem ammo = new ReloadSystem(6, 2.5f);
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            Engine.SaveGame.CurrentMission.Add(NewSplitter(Player.Position, Player.IdealSpeedWithVelocity(8), Util.ToAngle(Player.Direction), Team, 3, [NewAssassinShot(Position, default, 0, 0, Team, 3, 0)], 0.2f, 0, true));
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
            Engine.ShakeScreen(0.3f);
            Player.Velocity -= Player.Direction / 2;
            Cooldown = 0.2f;
            Engine.Camera.Position += Player.Direction * 6;
            Util.FiringParticles(Player.Position + Player.Direction * 8, Player.Velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
        }
    }
    public override void OnUpdate()
    {
        ammo.Update(this);
        base.OnUpdate();
    }
}
public class MicroRocketLauncher() : Module(Modules.MicroRocketLauncher)
{
    ReloadSystem ammo = new ReloadSystem(30, 4);
    float offset = 2;
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            Vector2 speed = Player.IdealSpeedWithVelocity(5);
            var dir = Vector2.Normalize(speed - Player.Velocity);
            Vector2 finalSpeed = speed + new Vector2(dir.Y, -dir.X) * offset;
            Engine.SaveGame.CurrentMission.Add(Entity.NewMissile(Player.Position + Player.Direction * 6, finalSpeed, Util.ToAngle(finalSpeed), Team, 3, 3, 5));
            SoundManager.PlaySound(Assets.Get(Sound.MissileFire), Player.Position);
            Cooldown = 0.25f;
            Engine.ShakeScreen(0.2f);
            offset *= -1;
        }

    }
    public override void OnUpdate()
    {
        ammo.Update(this);
        base.OnUpdate();
    }
}
public class AdaptiveShotgun() : Module(Modules.AdaptiveShotgun)
{
    ReloadSystem ammo = new ReloadSystem(2, 3);
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            float distance = Vector2.Distance(new Vector2(Input.NewMouseState.X, Input.NewMouseState.Y), Engine.BackBuffer / 2) + 1; //Plus one prevents division by zero
            for (float i = -5; i <= 5; i++)
            {
                Vector2 speed = Player.IdealSpeedWithVelocity(18);
                var dir = Vector2.Normalize(speed);
                Vector2 offset = (dir * i / 5 + new Vector2(dir.Y, -dir.X)) * i * 100 / (distance);
                Vector2 targetVector = speed + offset;
                var p1 = NewPulseShot(Player.Position, targetVector, Util.ToAngle(targetVector - Player.Velocity), 0, Team, 6 - (int)MathF.Abs(i), true, 0);
                p1.Texture = Assets.Get(Sprites.Microshot);
                p1.GetComponent<ExpireTimer>().TimeLeft = 5;
                Engine.SaveGame.CurrentMission.Add(p1);
            }
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
            Player.Velocity -= Player.Direction * 2;
            Cooldown = 0.75f;
            Engine.Camera.Position += Player.Direction * 12 + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
            Engine.ShakeScreen(0.6f);
            Util.FiringParticles(Player.Position + Player.Direction * 8, Player.Velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
        }
    }
    public override void OnUpdate()
    {
        ammo.Update(this);
        base.OnUpdate();
    }
}
public class GuidedRound() : Module(Modules.GuidedRound)
{
    private ReloadSystem ammo = new ReloadSystem(3, 3);
    private List<Entity> rounds = [];
    public override void OnShoot()
    {
        Vector2 mousePos = new Vector2(Input.NewMouseState.X, Input.NewMouseState.Y) - Engine.BackBuffer / 2 + Engine.MousePositionOffset * 1.5f;
        foreach (var round in rounds)
        {
            round.Velocity += Vector2.Normalize(mousePos - (round.Position - Player.Position)) * Engine.DeltaSeconds * 60;
            round.Velocity *= Util.FIED(0.3f);
            round.Angle = Util.ToAngle(round.Velocity - Player.Velocity);
        }
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            Vector2 vel = Player.IdealSpeedWithVelocity(9) + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 2;
            var round = NewAssassinShot(Player.Position, vel, Util.ToAngle(vel - Player.Velocity), 0, Team, 10);
            round.TimeLeft = 20;
            round.Texture = Assets.Get(Sprites.Glow);
            rounds.Add(round);
            Engine.SaveGame.CurrentMission.Add(round);
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
            Cooldown = 0.5f;
            Engine.ShakeScreen(0.2f);
            Engine.Camera.Position += Player.Direction * 8 + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
            Player.Velocity -= Player.Direction / 3;
            Util.FiringParticles(Player.Position + Player.Direction * 8, Player.Velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
        }
    }
    public override void OnUpdate()
    {
        rounds = [.. rounds.Where(x => !x.isExpired)];
        ammo.Update(this);
        base.OnUpdate();
    }
}
public class Dash() : Module(Modules.Dash)
{
    const float MaxCooldown = 2;
    public override void OnAbility()
    {
        if (Cooldown > 0)
        {
            return;
        }
        Player.invincibilityCooldown = 0.5f;
        for (int i = 0; i < 200; i++)
        {
            float timeLeft = ((float)i / 200);
            var col = Color.SlateBlue;
            col.A = 0;
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), timeLeft, Player.Position + Player.Direction * i, Player.Velocity * timeLeft, Util.ToAngle(Player.Direction), 0, Color.Cyan, col));
        }
        Player.Position += Player.Direction * 200;
        Cooldown = MaxCooldown;
    }
    public override void OnUpdate()
    {
        UI.PlayerAbility.SetInterval(1 - Cooldown / MaxCooldown, 1);
        base.OnUpdate();
    }
}
public class SummonShield() : Module(Modules.SummonShield)
{
    Entity shield1;
    Entity shield2;
    const float MaxCooldown = 15;
    public override void OnAbility()
    {
        if (Cooldown > 0 || (shield1 != null && shield2 != null))
        {
            return;
        }
        shield1 = Entity.NewShield(Player, 12, 20, MathF.PI / 4, 1, Team);
        Engine.SaveGame.CurrentMission.Add(shield1);
        shield2 = Entity.NewShield(Player, 12, 20, -MathF.PI / 4, 1, Team);
        Engine.SaveGame.CurrentMission.Add(shield2);
        Cooldown = MaxCooldown;
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
        UI.PlayerAbility.SetInterval(1 - Cooldown / MaxCooldown, 1);
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
            var mousePos = new Vector2(Input.NewMouseState.X, Input.NewMouseState.Y) + Engine.Camera.Position - Engine.BackBuffer / 2 + Engine.MousePositionOffset;
            if (Vector2.Distance(mousePos, Player.Position) < 100)
            {
                foreach (var entity in Engine.SaveGame.CurrentMission.Entities)
                {
                    if (Vector2.DistanceSquared(mousePos, entity.Position) < 1000)
                    {
                        hook.Parent = entity;
                        break;
                    }
                }
            }
            else
            {
                Cooldown /= 2;
                hook.isExpired = true;
                hook = null;
                p = null;
            }
        }
        else
        {
            if (Cooldown > 0)
            {
                return;
            }
            hook = new GrapplingHook(Player.Position, Player.IdealSpeedWithVelocity(50), Util.ToAngle(Player.Direction), Player);
            p = null;
            SoundManager.PlaySound(Assets.Get(Sound.Click), Player.Position);
            Engine.ShakeScreen(0.3f);
            Player.Velocity -= Player.Direction / 2;
            Engine.SaveGame.CurrentMission.Add(hook);
            Cooldown = MaxCooldown;
        }
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
            hook.Parent.Position = p.Position + offset;
        }
        UI.PlayerAbility.SetInterval(1 - Cooldown / MaxCooldown, 1);
        base.OnUpdate();
    }
}
public class Nanomachines() : Module(Modules.Nanomachines)
{
    const float MaxCooldown = 30;
    public override void OnAbility()
    {
        if (Cooldown > 0)
        {
            return;
        }
        foreach (var pickup in Player.leashedMaterials)
        {
            if (pickup is not Module)
            {
                pickup.isExpired = true;
                Statuses.ApplyStatus(new Healing(4));
                Cooldown = MaxCooldown;
                return;
            }
        }
    }
    public override void OnUpdate()
    {
        UI.PlayerAbility.SetInterval(1 - Cooldown / MaxCooldown, 1);
        base.OnUpdate();
    }
}
public class CreateFighter() : Module(Modules.CreateFighter)
{
    const float MaxCooldown = 60;
    private List<Entity> allies = [];
    public override void OnAbility()
    {
        if (Cooldown > 0 || allies.Count >= 20)
        {
            return;
        }
        foreach (var pickup in Player.leashedMaterials)
        {
            if (pickup is not Module)
            {
                pickup.isExpired = true;
                for (int i = 0; i < 10; i++)
                {
                    var enemy = Entity.NewSurgeChild(Player.Position + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()), Player.Velocity, Player.Angle, Player, allies);
                    enemy.Team = Team.Friendly;
                    enemy.AddComponent(new Behaviour(enemy).AddBehaviour(enemy.AvoidProjectiles(1)));
                    Engine.SaveGame.CurrentMission.Add(enemy);
                    allies.Add(enemy);
                }
                Cooldown = MaxCooldown;
                return;
            }
        }
    }
    public override void OnUpdate()
    {
        UI.PlayerAbility.SetInterval(1 - Cooldown / MaxCooldown, 1);
        allies = [.. allies.Where(x => !x.isExpired)];
        base.OnUpdate();
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
        if (isShooting || Cooldown > 0)
        {
            return;
        }
        resistanceCooldown = 3;
        count = 1;
        for (float angle = 0; angle < MathF.Tau; angle += MathF.PI / 4)
        {
            Engine.SaveGame.CurrentMission.Add(NewPulseShot(Player.Position, Util.ToUnitVector(angle) * 10, angle, 0, Team, 20, true, 1));
        }
        Cooldown = 0.1f;
        isShooting = true;
        SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
    }
    public override int OnCollide(int _damage)
    {
        if (resistanceCooldown > 0)
        {
            return _damage * 4 / 5;
        }
        return _damage;
    }
    public override void OnUpdate()
    {
        if (resistanceCooldown > 0)
        {
            resistanceCooldown -= Engine.DeltaSeconds;
        }
        if (isShooting && Cooldown <= 0)
        {
            Engine.SaveGame.CurrentMission.Add(Entity.NewMissile(Player.Position, Util.ToUnitVector(count * MathF.PI * 2 / 3) * 5, count * MathF.PI * 2 / 3, Team));
            SoundManager.PlaySound(Assets.Get(Sound.MissileFire), Player.Position);
            Cooldown = 0.25f;
            count++;
            if (count > 6)
            {
                count = 0;
                Cooldown = 30;
                isShooting = false;
            }
        }
        UI.PlayerAbility.SetInterval(1 - Cooldown / MaxCooldown, 1);
        base.OnUpdate();
    }
}
public class Decoy() : Module(Modules.Decoy)
{
    public override void OnAbility()
    {
        if (Cooldown <= 0)
        {
            return;
        }
        Engine.SaveGame.CurrentMission.Add(Entity.NewDecoy(Engine.SaveGame.Player.Position, Vector2.Zero, Engine.SaveGame.Player.Angle, Sprites.Player, Team));
        Cooldown = 15f;
    }
}
public class Sensors() : Module(Modules.Sensors)
{

}
public class Lidar() : Module(Modules.Lidar)
{
    public override void OnAbility()
    {
        if (Cooldown > 0)
        {
            return;
        }
        Vector2 dir = Player.Direction + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 5;
        Engine.SaveGame.CurrentMission.Hitscan(Player.Position, dir, 1000, false, out Vector2 end);
        ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 1f, end, Vector2.Zero, 0, 0, Color.White, Color.Transparent));
    }
}
public class Radar() : Module(Modules.Radar)
{
    float time = 0;
    public override void OnUpdate()
    {
        time += Engine.DeltaSeconds;
        base.Update();
        if (Cooldown > 0)
        {
            return;
        }
        int fuses = Player.CountFuses(ModuleType.Sensors);
        Vector2 dir = Util.ToUnitVector(time * (float)(fuses) / 3);
        List<Entity> revealedEntities = Engine.SaveGame.CurrentMission.Hitscan(Player.Position, dir, 2000, true, out Vector2 end, null);
        foreach (var entity in revealedEntities)
        {
            entity.RevealDuration = 2f;
        }
        for (int i = 0; i < 10; i++)
        {
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), Player.Position + dir * 10 * i, 0, Color.Green * (1 - (float)(i) / 10)));
        }
        ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), end, 0, Color.White));
    }
}
public class PulseEmitter() : Module(Modules.PulseEmitter)
{
    public override void OnUpdate()
    {
        base.Update();
        if (Cooldown > 0)
        {
            return;
        }
        var enemy = Engine.SaveGame.CurrentMission.NearestEnemy(this);
        if (enemy != null)
        {
            enemy.RevealDuration = 1;
        }
        var proj = Engine.SaveGame.CurrentMission.NearestProjectile(Position, SensingAbility, Team);
        if (proj != null)
        {
            proj.RevealDuration = 1;
        }
        Cooldown = 2;
        SoundManager.PlayGlobalSound(Assets.Get(Sound.Beep));
    }
}
public class Expose() : Module(Modules.Expose)
{
    const float MaxCooldown = 15;
    FlameBolt aura = null;
    public override void OnAbility()
    {
        if (Cooldown > 0)
        {
            if (aura != null && !aura.isExpired)
            {
                aura.isExpired = true;
                Cooldown -= aura.TimeLeft;
                aura = null;
            }
            return;
        }
        if (Input.NewState.IsKeyDown(Keys.LeftShift))
        {
            Engine.SaveGame.CurrentMission.Add(aura = new FlameBolt(Player.Position + new Vector2(Input.NewMouseState.X, Input.NewMouseState.Y) + Engine.MousePositionOffset - Engine.BackBuffer / 2, Vector2.Zero, Team, 0, new ParticleEmitter(Assets.Get(Sprites.Dot), Player.Position, 0, Color.Orange * 0.75f) { speedOfEmission = 0.5f }, 10, 2, 20));
        }
        else
        {
            Engine.SaveGame.CurrentMission.Add(aura = new FlameBolt(Player.Position + new Vector2(Input.NewMouseState.X, Input.NewMouseState.Y) + Engine.MousePositionOffset - Engine.BackBuffer / 2, Vector2.Zero, Team, 0, new ParticleEmitter(Assets.Get(Sprites.Dot), Player.Position, 0, Color.Cyan * 0.75f) { speedOfEmission = 0.5f }, 10, 2, -20));
        }
        aura.Transform.IsImmovable = true;
        Cooldown = 15;
    }
    public override void OnUpdate()
    {
        UI.PlayerAbility.SetInterval(1 - Cooldown / MaxCooldown, 1);
        base.OnUpdate();
    }
}

