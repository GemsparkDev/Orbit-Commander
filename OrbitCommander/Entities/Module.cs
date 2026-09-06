using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OrbitCommander.Components;
using OrbitCommander.Core;
using OrbitCommander.Particles;
using UILib.Content;

namespace OrbitCommander.Entities;

public abstract class Module : Pickup, IData
{
    //Serialized fields
    public bool isFailed = false;
    public new Modules Type { get; }
    Color IData.Color => isFailed ? Color.Red : Color.White;
    private Decal healthDecal;
    private Decal description;
    public float Cooldown { get; protected set; } = 0;
    private Health health;
    public override int Health { get => base.Health; set { base.Health = value; UpdateHealth(); } }

    public Module(Modules _type, Vector2 _position = default, Vector2 _velocity = default, float _angularVelocity = 0)
        : base(ItemFactory.moduleData[_type], _position, _velocity, _angularVelocity, ItemFactory.moduleData[_type].MaxHealth)
    {
        Type = _type;
        healthDecal = new Decal(new Vector2(0, 5), Assets.TextFont, $"{Health} / {MaxHealth}", Color.Pink, 5f);
        description = new Decal(new Vector2(-5, 15), Assets.TextFont, ItemFactory.moduleData[_type].Description, Color.White, 3f);
        Tooltip.AddWidget(healthDecal);
        Tooltip.AddWidget(description);
        AddComponent(new Smelt() { Value = 3 });
        health = GetComponent<Health>();
    }
    public void UpdateHealth()
    {
        healthDecal.Text = $"{health.CurrentHealth} / {health.MaxHealth}";
    }

    public virtual int OnCollide(int _damage) { return _damage; }
    public virtual void OnShoot() { }
    public virtual void OnEnemyHit(Entity _entity, int _damage) { }
    public virtual void OnContruct(Pickup _c) { }
    public virtual int SensingChange() { return 0; }
    public virtual int StealthChange() { return 0; }
    public virtual float ModifyCrit(float _crit) { return _crit; }
    public virtual void OnUpdate(float _fuseRatio)
    {
        if (Cooldown > 0)
        {
            Cooldown -= Engine.DeltaSeconds * _fuseRatio;
        }
        UpdateHealth();
    }
    protected int GunStealthChange()
    {
        if(Cooldown > 0)
        {
            return -1;
        }
        return 0;
    }
    public virtual void OnEngine() { }
    public virtual void OnAbility() { }
    public virtual Entity ModifyProjectile(Entity _projectile) { return _projectile; }
    //Override to provide custom serialization for modules
    public virtual void Parse(Modules _type, List<string> _disassembly, LoadLogger _logger)
    {
        _logger.Try(delegate { isFailed = bool.Parse(_disassembly[2]); }, 2);
        UpdateHealth();
        Parse(_disassembly, _logger);
    }
    public override string Serialize()
    {
        return $"{{{Type},{SerializeAttributes()},{isFailed}}}";
    }
}
public abstract class Weapon(Modules _type, Vector2 _position = default, Vector2 _velocity = default, float _angularVelocity = 0) : Module(_type, _position, _velocity, _angularVelocity)
{
    public abstract float Speed { get; }
    public abstract bool CritCondition { get; }
}
public class ReloadSystem(int _magazineSize, float _reloadSpeed, Action _reloadCallback = null)
{
    private int magazineSize = _magazineSize;
    float reloadCD = 0;
    public int Rounds { get; private set; } = _magazineSize;
    public void Update(Module _module, float _fuseRatio)
    {
        if (reloadCD > 0)
        {
            reloadCD -= Engine.DeltaSeconds * _fuseRatio;
            if (reloadCD < 0)
            {
                Rounds = magazineSize;
            }
        }
        float val = Rounds;
        if (reloadCD > 0)
        {
            val = (1 - reloadCD / _reloadSpeed) * magazineSize;
        }
        if (Rounds != magazineSize && reloadCD <= 0 && Input.NewState.IsKeyDown(Keys.R))
        {
            Rounds = 0;
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
        if (Rounds > 0)
        {
            Rounds--;
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
public class ModuleData(Sprites _realSprite, Sprites _virtualSprite, string _name, string _description, int _id, int _health, Type _type, Color? _textColor = null)
    : ItemData(_realSprite, _virtualSprite, _name, _id, Color.White, _textColor, _health)
{
    public string Description { get; } = _description;
    public int MaxHealth { get; } = _health;
    public Type ModuleType { get; } = _type;
    public Module Retrieve()
    {
        return (Module)Activator.CreateInstance(ModuleType);
    }
}
public class Hull() : Module(Modules.Hull)
{
    private float resistanceTime = 0;
    public override int OnCollide(int _damage)
    {
        return (int)(_damage * (1.1f - Math.Clamp(resistanceTime, 0, 0.5f)));
    }
    public override void OnUpdate(float _fuseRatio)
    {
        UI.PlayerSpecialHealth.Colors[0] = Color.Orange * 0.5f;
        UI.PlayerSpecialHealth.SetInterval(resistanceTime, 2f);
        UI.PlayerSpecialHealth.Colors[1] = Color.Transparent;
        if(resistanceTime > 0)
        {
            resistanceTime -= Engine.DeltaSeconds * 0.33f / _fuseRatio;
        }
        base.OnUpdate(_fuseRatio);
    }
    public override void OnEnemyHit(Entity _entity, int _damage)
    {
        if(_entity.Health <= 0)
        {
            resistanceTime = Math.Min(resistanceTime + 0.5f, 2);
        }
    }
}
public class Reflective() : Module(Modules.Reflective)
{
    private ParticleEmitter shieldEffect = new(Assets.Get(Sprites.Dot), Vector2.Zero, 10, Color.Violet) { particleAngularVelocity = 0.1f };
    private float max = 1;
    public override void OnUpdate(float _fuseRatio)
    {
        if (Cooldown <= 0)
        {
            shieldEffect.position = Player.Position;
            shieldEffect.offsetVelocity = Player.Velocity;
            shieldEffect.Update();
        }
        UI.PlayerSpecialHealth.SetInterval(max-Cooldown, max);
        UI.PlayerSpecialHealth.Colors[0] = Color.Yellow;
        UI.PlayerSpecialHealth.Colors[1] = Color.Transparent;
        base.OnUpdate(_fuseRatio);
    }
    public override int OnCollide(int _damage)
    {
        if (Cooldown <= 0)
        {
            Cooldown = _damage;
            max = _damage;
            var entity = Util.Nearest(Player.Position, [.. Engine.SaveGame.CurrentMission.enemies.Where(x => !x.HasTag(Tags.IsMissile)).Where(x => !x.IsFriendly(Player))]);
            if(entity != null)
            {
                var vel = Util.PredictEnemy(entity, Player, 6 + _damage / 2);
                Player.Shoot(NewAssassinShot(Player.Position, vel, Util.ToAngle(vel), 0, Team, _damage, 1), 1, false);
            }
            else
            {
                var vel = Player.IdealSpeedWithVelocity(6 + _damage / 2);
                Player.Shoot(NewAssassinShot(Player.Position, vel, Util.ToAngle(vel), 0, Team, _damage, 1), 1, false);
            }
            return 0;
        }
        return (int)(_damage * 1.5f);
    }
}
public class StealthHull() : Module(Modules.Stealth)
{
    private float stealthCd = 0;
    public override int OnCollide(int _damage)
    {
        return (int)(_damage * 1.1f);
    }
    public override void OnUpdate(float _fuseRatio)
    {
        if(stealthCd > 0)
        {
            stealthCd -= Engine.DeltaSeconds / _fuseRatio;
        }
        UI.PlayerSpecialHealth.Colors[0] = Color.Transparent;
        UI.PlayerSpecialHealth.Colors[1] = Color.Transparent;
        base.OnUpdate(_fuseRatio);
    }
    public override int StealthChange()
    {
        if(stealthCd > 0)
        {
            return 2;
        }
        return 1;
    }
    public override void OnEnemyHit(Entity _entity, int _damage)
    {
        stealthCd += MathF.Sqrt(_damage)/3;
    }
}
public class Turtle() : Module(Modules.Turtle)
{
    float time = 0;
    int flipped = 1;
    float dr = 1.5f;
    ParticleEmitter effect = new ParticleEmitter(Assets.Get(Sprites.Dot), Vector2.Zero, 10, Color.Orange) { sprayAngle = MathF.PI / 2 };
    public override int OnCollide(int _damage)
    {
        Player.RevealDuration = 1;
        return (int)(_damage * dr);
    }
    public override void OnUpdate(float _fuseRatio)
    {
        var entity = Util.Nearest(Player.Position, [.. Engine.SaveGame.CurrentMission.enemies.Where(x => !x.HasTag(Tags.IsMissile)).Where(x => !x.IsFriendly(Player))]);
        if (entity == null)
        {
            dr = 1.5f;
        }
        else
        {
            var distance = MathF.Tanh(Vector2.Distance(Player.Position, entity.Position) / 300);
            dr = 0.5f + distance * distance;
        }

        UI.PlayerSpecialHealth.Colors[0] = Color.Orange * 0.5f;
        UI.PlayerSpecialHealth.SetInterval(1.5f - dr, 1);
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
        effect.offsetVelocity = Player.Velocity;
        if (flipped == 1)
        {
            effect.sprayCone = MathF.Tau * time;
        }
        else
        {
            effect.sprayCone = MathF.Tau * (1 - time);
        }
        effect.particleColor = Color.Orange * (1.5f - dr);
        effect.Update();
        base.OnUpdate(_fuseRatio);
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
            buffer -= _damage;
            return 0;
        }
        buffer = 0;
        return (_damage - (int)Math.Round(buffer)) * 2;
    }
    public override void OnUpdate(float _fuseRatio)
    {
        UI.PlayerSpecialHealth.Colors[0] = Color.Cyan;
        UI.PlayerSpecialHealth.SetInterval(buffer, 25f);
        if (Cooldown <= 0 && buffer < 25)
        {
            buffer += Engine.DeltaSeconds * 10;
        }
        base.OnUpdate(_fuseRatio);
    }
}
public class Adaptive() : Module(Modules.Adaptive)
{
    public override int OnCollide(int _damage)
    {
        if (Health > 0)
        {
            Player.Statuses.ApplyStatus(new Berserk(_damage));
            return _damage * 4 / 3;
        }
        return _damage;
    }
}
public class ThermalShield() : Module(Modules.ThermalShield)
{
    public override int OnCollide(int _damage)
    {
        return (int)(_damage * 5 / 4 * Math.Clamp(1 - MathF.Abs(Player.Temperature), 0, 1));
    }
    public override void OnUpdate(float _fuseRatio)
    {
        UI.PlayerSpecialHealth.Colors[0] = Color.Transparent;
        UI.PlayerSpecialHealth.Colors[1] = Color.Transparent;
        Player.ApplyWork(0.33f * (float)Math.Sign(-Player.Temperature));
        base.OnUpdate(_fuseRatio);
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
        float fuseRatio = (float)Player.CountFuses(ModuleType.Engines) / 3;
        engineParticles.speedOfEmission = Math.Max(450f * fuseRatio * engineTimeModifier, 10);
        if (Player.EngineDirection != Vector2.Zero)
        {
            Player.Velocity += Vector2.Normalize(Player.EngineDirection) * 24 * Engine.DeltaSeconds * engineTimeModifier * fuseRatio / (Player.leashedMaterials.Count + 2);
            engineParticles.position = Player.Position - Vector2.Normalize(Player.EngineDirection) * 8 - Player.Velocity;
            engineParticles.sprayAngle = Util.ToAngle(Player.EngineDirection) + MathF.PI;
        }
        engineParticles.Update();
    }
    public override void OnUpdate(float _fuseRatio)
    {
        if (!Player.isEngineActive && engineTime > 0)
        {
            engineTime -= Engine.DeltaSeconds / _fuseRatio;
        }
        base.OnUpdate(_fuseRatio);
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
        float fuseRatio = (float)Player.CountFuses(ModuleType.Engines) / 3;
        var dir = Vector2.Normalize(-Player.EngineDirection + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 10);
        for (float i = 0; i < 5 * fuseRatio * engineTimeModifier; i++)
        {
            float lerp = i / (5 * fuseRatio * engineTimeModifier);
            Vector3 color = new Vector3(0, 1, 1) * (1 - lerp) + new Vector3(1, 0.5f, 0) * lerp;
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), Player.Position + dir * (i + 2.5f) * 4, Player.Angle, new Color(color.X, color.Y, color.Z) * (1 - lerp)));
        }
        if (Player.EngineDirection != Vector2.Zero)
        {
            Player.Velocity += Vector2.Normalize(Player.EngineDirection) * 20 * Engine.DeltaSeconds * engineTimeModifier * fuseRatio * (0.75f - MathF.Tanh((burstTime - 5) / 2) / 4) / (Player.leashedMaterials.Count + 1);
        }
        if (burstTime < 6)
        {
            burstTime += Engine.DeltaSeconds * 3;
        }
    }
    public override void OnUpdate(float _fuseRatio)
    {
        if (!Player.isEngineActive && engineTime > 0)
        {
            engineTime -= Engine.DeltaSeconds / _fuseRatio;
        }
        if (burstTime > 0)
        {
            burstTime -= Engine.DeltaSeconds * 2;
        }
        base.OnUpdate(_fuseRatio);
    }
    public override int StealthChange()
    {
        if(!Player.isEngineActive)
        {
            return 0;
        }
        return -1;
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
        float fuseRatio = (float)Player.CountFuses(ModuleType.Engines) / 3;
        engineParticles.speedOfEmission = Math.Max(450f * fuseRatio * engineTimeModifier, 10);
        if (Player.EngineDirection != Vector2.Zero)
        {
            Player.Velocity += Vector2.Normalize(Player.EngineDirection) * 14 * Engine.DeltaSeconds * engineTimeModifier * fuseRatio;
            engineParticles.position = Player.Position - Vector2.Normalize(Player.EngineDirection) * 8 - Player.Velocity;
            engineParticles.sprayAngle = Util.ToAngle(Player.EngineDirection) + MathF.PI;
        }
        engineParticles.Update();
    }
    public override void OnUpdate(float _fuseRatio)
    {
        if (!Player.isEngineActive && engineTime > 0)
        {
            engineTime -= Engine.DeltaSeconds / _fuseRatio;
        }
        base.OnUpdate(_fuseRatio);
    }
    public override void OnContruct(Pickup _c)
    {
        _c.AddTag(Tags.IsImmune);
    }
}
public class OrionEngine() : Module(Modules.Orion)
{
    private float explosionTime = 0;
    public override void OnEngine()
    {
        if (Cooldown > 0)
        {
            return;
        }
        Cooldown = 0.33f;
        var dir = Vector2.Normalize(Player.EngineDirection);
        if (Player.EngineDirection != Vector2.Zero)
        {
            Player.Velocity += dir * 4 / (Player.leashedMaterials.Count + 1);
            Util.Explode(Player.Position - dir * 45, Player.Velocity, 25, 42, [Player.Team]);
            SoundManager.PlaySound(Assets.Get(Sound.ShieldHit), Player.Position);
        }
    }
    public override void OnUpdate(float _fuseRatio)
    {
        if(explosionTime > 0)
        {
            explosionTime -= Engine.DeltaSeconds * _fuseRatio;
        }
        base.OnUpdate(_fuseRatio);
    }
    public override void OnEnemyHit(Entity _entity, int _damage)
    {
        if(_damage > 3 && explosionTime <= 0)
        {
            explosionTime = (float)(_damage) / 100;
            Util.Explode(_entity.Position, _entity.Velocity, _damage / 3, MathF.Sqrt(_damage) * 20, [Player.Team]);
        }
    }
}
public class Basic() : Weapon(Modules.Basic)
{
    private ReloadSystem ammo = new ReloadSystem(18, 2);
    public override float Speed => 9;
    public override bool CritCondition => ammo.Rounds < 4;
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            Vector2 vel = Player.IdealSpeedWithVelocity(Speed) + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 2;
            Player.Shoot(NewPulseShot(Player.Position, vel, Util.ToAngle(vel - Player.Velocity), 0, Team, 3), 3f, CritCondition);
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
            Cooldown = 0.2f;
            Engine.ShakeScreen(0.3f);
            Engine.Camera.Position += Player.Direction * Speed + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
            Player.Velocity -= Player.Direction / 3;
            Util.FiringParticles(Player.Position + Player.Direction * 6, Player.Velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
        }
    }
    public override void OnUpdate(float _fuseRatio)
    {
        ammo.Update(this, _fuseRatio);
        base.OnUpdate(_fuseRatio);
    }
    public override int StealthChange() => GunStealthChange();
}
public class Antimaterial() : Weapon(Modules.Antimaterial)
{
    ReloadSystem ammo = new ReloadSystem(4, 2f);
    public override float Speed => 20;
    bool nextShotCrit = false;
    public override bool CritCondition => nextShotCrit;
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            var p1 = NewAssassinShot(Player.Position, Player.IdealSpeedWithVelocity(Speed), Util.ToAngle(Player.Direction), 0, Team, 16);
            p1.Texture = Assets.Get(Sprites.Arrow);
            Player.Shoot(p1, 3, CritCondition);
            SoundManager.PlaySound(Assets.Get(Sound.SniperFire), Player.Position);
            Cooldown = 0.75f;
            Engine.Camera.Position += Player.Direction * Speed + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) * 2;
            Engine.ShakeScreen(0.5f);
            Player.Velocity -= Player.Direction / 2;
            Util.FiringParticles(Player.Position + Player.Direction * 6/2, Player.Velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
            nextShotCrit = false;
        }
    }
    public override void OnUpdate(float _fuseRatio)
    {
        ammo.Update(this, _fuseRatio);
        base.OnUpdate(_fuseRatio);
    }
    public override void OnEnemyHit(Entity _entity, int _damage)
    {
        if(_entity.Health <= 0 && Vector2.Distance(Player.Position, _entity.Position) > 10)
        {
            nextShotCrit = true;
        }
    }
    public override int StealthChange() => GunStealthChange();
}
public class Railgun() : Weapon(Modules.Railgun)
{
    ReloadSystem ammo = new ReloadSystem(1, 1.5f);
    public override float Speed => float.MaxValue;
    public override bool CritCondition 
    { 
        get 
        {
            return Engine.SaveGame.CurrentMission.Hitscan(Player.Position, Player.Direction, 3000, true, out Vector2 _, null).Count > 1;
        } 
    }
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            List<Entity> entities = Engine.SaveGame.CurrentMission.Hitscan(Player.Position, Player.Direction, 3000, true, out Vector2 end, null);
            var proj = Player.Modify(NewAssassinShot(Player.Position, Player.Direction * 50, Util.ToAngle(Player.Direction), 0, Player.Team, 30, 0), 0.5f + 0.5f * (float)(entities.Count), entities.Count > 1);
            for(int i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                entity.Collide(proj.Damage);
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
    public override void OnUpdate(float _fuseRatio)
    {
        ammo.Update(this, _fuseRatio);
        base.OnUpdate(_fuseRatio);
    }
    public override int StealthChange() => GunStealthChange();
}
public class Spiral() : Weapon(Modules.Spiral)
{
    ReloadSystem ammo = new ReloadSystem(10, 2);
    public override float Speed => 12;
    private float critCD = 0;
    public override bool CritCondition => critCD > 0;
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            for (int i = 0; i < Util.Random.Next(3, 5); i++)
            {
                Player.Shoot(NewSpiralShot(Player.Position, Player.IdealSpeedWithVelocity(Speed), Util.ToAngle(Player.Direction), 0, Team, 5, Util.OneToNegOne() * MathF.PI, 1), 1.25f, CritCondition);
            }
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
            Cooldown = 0.5f;
            Engine.ShakeScreen(0.4f);
            Player.Flash(Color.BurlyWood);
            Player.Velocity -= Player.Direction;
            Util.FiringParticles(Player.Position + Player.Direction * 6, Player.Velocity, Player.Direction);
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 60, Player.Position - Player.Velocity, Player.Velocity
                + new Vector2(-Player.Direction.Y + Util.OneToNegOne() / 2, Player.Direction.X + Util.OneToNegOne() / 4), 0, Util.OneToNegOne() / 5, Color.Yellow, Color.Transparent)
            { experienceGravity = true });
        }
    }
    public override void OnUpdate(float _fuseRatio)
    {
        ammo.Update(this, _fuseRatio);
        if(critCD > 0)
        {
            critCD -= Engine.DeltaSeconds;
        }
        base.OnUpdate(_fuseRatio);
    }
    public override void OnAbility()
    {
        critCD += Player.modules[ModuleType.Core].Cooldown / 3;
    }
    public override int StealthChange() => GunStealthChange();
}
public class Shotgun() : Weapon(Modules.Shotgun)
{
    ReloadSystem ammo = new ReloadSystem(20, 3);
    float fireCD = 0;
    public override float Speed => 10;
    public override bool CritCondition => fireCD > 0;
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
                Vector2 targetVector = Player.IdealSpeedWithVelocity(Speed) + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
                Vector2 positionOffset = new Vector2(Player.Direction.Y, -Player.Direction.X) * offsetAngle * 100;
                Player.Shoot(NewPulseShot(Player.Position + positionOffset, targetVector * (1 + Util.OneToNegOne() / Speed), Util.ToAngle(Player.Direction) + offsetAngle, 0, Team, 2), 2, CritCondition);
            }
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
            Player.Velocity -= Player.Direction / 2;
            Cooldown = 0.5f;
            Engine.Camera.Position += Player.Direction * Speed + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
            Engine.ShakeScreen(0.5f);
            Util.FiringParticles(Player.Position + Player.Direction * 6, Player.Velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
        }
    }
    public override int OnCollide(int _damage)
    {
        fireCD += _damage / 2;
        return _damage;
    }
    public override void OnUpdate(float _fuseRatio)
    {
        ammo.Update(this, _fuseRatio);
        if(fireCD > 0)
        {
            fireCD -= Engine.DeltaSeconds;
        }
        base.OnUpdate(_fuseRatio);
    }
    public override int StealthChange() => GunStealthChange();
}
public class Missile() : Weapon(Modules.Missile)
{
    private ReloadSystem ammo = new ReloadSystem(8, 2f);
    private List<(float timer, Entity entity)> hitEntities = [];
    bool nextCrit = false;
    public override bool CritCondition => nextCrit;
    public override float Speed => 9;
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            Player.Shoot(NewMissile(Player.Position + new Vector2(Player.Direction.Y, -Player.Direction.X) * 6, Player.IdealSpeedWithVelocity(Speed), Util.ToAngle(Player.Direction), Team, 1, 8, 10), 2f, CritCondition);
            SoundManager.PlaySound(Assets.Get(Sound.MissileFire), Player.Position);
            Cooldown = 0.5f;
            Engine.ShakeScreen(0.5f);
            nextCrit = false;
        }
    }
    public override void OnUpdate(float _fuseRatio)
    {
        if (Util.Random.NextSingle() > 0.33f)
        {
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), Cooldown, Player.Position - Player.Direction * 8 - Player.Velocity, Player.Velocity - Player.Direction * Cooldown * 2 + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) * Cooldown / 2, 0, 0, Color.Gray * (1 - (1 - Cooldown * 2) * (1 - Cooldown * 2)), Color.Transparent));
        }
        for(int i = 0; i < hitEntities.Count; i++)
        {
            var (timer, entity) = hitEntities[i];
            hitEntities[i] = (timer - Engine.DeltaSeconds, entity);
        }
        hitEntities = [.. hitEntities.Where(x => x.timer > 0)];
        ammo.Update(this, _fuseRatio);
        base.OnUpdate(_fuseRatio);
    }
    public override void OnEnemyHit(Entity _entity, int _damage)
    {
        foreach(var pair in hitEntities)
        {
            if(pair.entity == _entity)
            {
                return;
            }
        }
        hitEntities.Add((0.05f, _entity));
        if(hitEntities.Count > 2)
        {
            hitEntities.Clear();
            nextCrit = true;
        }
    }
    public override int StealthChange() => GunStealthChange();
}
public class LMG() : Weapon(Modules.LMG)
{
    private ReloadSystem ammo = new ReloadSystem(80, 4);
    private float critCD = 0;
    public override float Speed => 12;
    public override bool CritCondition => critCD > 0.5f;
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
            var shot = NewPulseShot(Player.Position + offset, Player.IdealSpeedWithVelocity(Speed) + offset / 4, Util.ToAngle(Player.Direction), 0, Team, 2);
            shot.Texture = dot;
            shot.TimeLeft = 3;
            Player.Shoot(shot, 1.5f, CritCondition);
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
            Engine.ShakeScreen(0.1f);
            Engine.Camera.Position += Player.Direction * Speed + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
            Player.Velocity -= Player.Direction / 6;
            Cooldown = 0.1f;
            Util.FiringParticles(Player.Position + Player.Direction * 6, Player.Velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
        }
    }
    public override void OnUpdate(float _fuseRatio)
    {
        ammo.Update(this, _fuseRatio);
        if(critCD > 0)
        {
            critCD -= Engine.DeltaSeconds;
        }
        base.OnUpdate(_fuseRatio);
    }
    public override void OnEnemyHit(Entity _entity, int _damage)
    {
        critCD = critCD * 4 / 5 + 0.2f;
    }
    public override int StealthChange() => GunStealthChange();
}
public class Crossbow() : Weapon(Modules.Crossbow)
{
    private float chargeTime = 0;
    public override float Speed => 15;
    public override bool CritCondition => chargeTime > 1.5f;
    public override void OnUpdate(float _fuseRatio)
    {
        if(Input.NewMouseState.LeftButton == ButtonState.Pressed)
        {
            if (chargeTime < 2)
            {
                chargeTime += Engine.DeltaSeconds / _fuseRatio;
            }
        }
        else
        {
            if(chargeTime > 1.5f)
            {
                Vector2 offset = new Vector2(Player.Direction.Y, -Player.Direction.X) * Util.Random.Next(-2, 3);
                var shot = NewPulseShot(Player.Position + offset, Player.IdealSpeedWithVelocity(Speed) + offset / 4, Util.ToAngle(Player.Direction), 0, Team, 18, true, 1);
                shot.Texture = Assets.Get(Sprites.CrossbowShot);
                Player.Shoot(shot, 1.5f, CritCondition);
                Engine.Camera.Position += Player.Direction * Speed / 2;
                Engine.ShakeScreen(0.2f);
                Player.Velocity -= Player.Direction / 4;
                chargeTime = 0;
            }
            else if(chargeTime > 0)
            {
                chargeTime -= Engine.DeltaSeconds;
            }
        }
        base.OnUpdate(_fuseRatio);
        if (Engine.SaveGame.Player.modules.ContainsValue(this))
        {
            UI.PlayerAmmo.SetInterval(Math.Min(chargeTime, 1.5f), 1.5f);
        }
    }
}
public class Flamethrower() : Weapon(Modules.Flamethrower)
{
    ReloadSystem ammo = new ReloadSystem(60, 1, delegate ()
    { ParticleManager.Add(new Particle(Assets.Get(Sprites.Cog), 60, Player.Position, Player.Velocity + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()), 0, Util.OneToNegOne() / 2, Color.Green, Color.Transparent) { experienceGravity = true }); });
    public override float Speed => 5;
    public override bool CritCondition => Player.Temperature > 1;
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            Player.Shoot(new FlameBolt(Player.Position, Player.IdealSpeedWithVelocity(Speed) + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 4, Team, 1), 3, CritCondition);
            SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Player.Position);
            Player.Velocity -= Player.Direction / 10;
            Cooldown = 0.08f;
            Engine.ShakeScreen(0.1f);
            Player.Flash(Color.Orange);
        }
    }
    public override void OnUpdate(float _fuseRatio)
    {
        ammo.Update(this, _fuseRatio);
        base.OnUpdate(_fuseRatio);
    }
    public override int StealthChange() => GunStealthChange();
}
public class Fireball() : Weapon(Modules.Fireball)
{
    ReloadSystem ammo = new ReloadSystem(3, 2);
    public override float Speed => 8;
    public override bool CritCondition => Player.Temperature > 1;
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            Player.Shoot(new FlameBolt(Player.Position, Player.IdealSpeedWithVelocity(Speed) + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 2, Team, 4, 4, 0.5f), 3f, CritCondition);
            SoundManager.PlaySound(Assets.Get(Sound.LMGFire), Player.Position);
            Cooldown = 0.5f;
            Engine.ShakeScreen(0.3f);
            Player.Flash(Color.Orange);
        }
    }
    public override void OnUpdate(float _fuseRatio)
    {
        ammo.Update(this, _fuseRatio);
        base.OnUpdate(_fuseRatio);
    }
    public override int StealthChange() => GunStealthChange();
}
public class GrenadeLauncher() : Weapon(Modules.GrenadeLauncher)
{
    ReloadSystem ammo = new ReloadSystem(8, 3);
    private float critCD = 0;
    public override float Speed => 8;
    public override bool CritCondition => critCD > 0;
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            Player.Shoot(NewExplosive(Player.Position, Player.IdealSpeedWithVelocity(Speed) + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()), Util.ToAngle(Player.Direction), Util.OneToNegOne() / 8, Team, 16, 40, 1), 1.667f, CritCondition);
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
            Cooldown = 0.8f;
            Engine.ShakeScreen(0.4f);
            Player.Velocity -= Player.Direction / 2;
            Engine.Camera.Position += Player.Direction * Speed + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 2;
            Util.FiringParticles(Player.Position + Player.Direction * 6, Player.Velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
        }
    }
    public override void OnUpdate(float _fuseRatio)
    {
        ammo.Update(this, _fuseRatio);
        if(critCD > 0)
        {
            critCD -= Engine.DeltaSeconds;
        }
        base.OnUpdate(_fuseRatio);
    }
    public override void OnContruct(Pickup _c)
    {
        critCD += 30;
    }
}
public class SpewerModule() : Weapon(Modules.Spewer)
{
    ReloadSystem ammo = new ReloadSystem(3, 5);
    private float fireCD = 0;
    public override float Speed => 4;
    public override bool CritCondition => fireCD < 0;
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            Player.Shoot(NewSpewer(Player.Position, Player.IdealSpeedWithVelocity(Speed) + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 2, Util.ToAngle(Player.Direction), Util.OneToNegOne() / 8, Team, 2), 2, CritCondition);
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
            Cooldown = 1f;
            Engine.ShakeScreen(0.6f);
            Player.Velocity -= Player.Direction;
            Engine.Camera.Position += Player.Direction * Speed * 3 + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
            Util.FiringParticles(Player.Position + Player.Direction * 6 * 2, Player.Velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
            fireCD = 2;
        }
    }
    public override void OnUpdate(float _fuseRatio)
    {
        ammo.Update(this, _fuseRatio);
        if(fireCD > 0)
        {
            fireCD -= Engine.DeltaSeconds;
        }
        base.OnUpdate(_fuseRatio);
    }
    public override int StealthChange() => GunStealthChange();
}
public class PrismArray() : Weapon(Modules.PrismArray)
{
    float time = 0;
    private bool isFiring = false;
    private SoundEffectInstance beam = Assets.Get(Sound.FireLaser).CreateInstance();
    private SoundEffectInstance beamBlend;
    private float duration = (float)Assets.Get(Sound.FireLaser).Duration.TotalSeconds;
    private float timeLeft = 0;
    public override float Speed => float.MaxValue;
    public override bool CritCondition => Player.Health <= 20;
    public override void OnShoot()
    {
        var proj = Player.Modify(NewAssassinShot(Player.Position, Player.Direction * 50, Util.ToAngle(Player.Direction), 0, Player.Team, 30, 0), 2, CritCondition);
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
            Vector3 color = new Vector3(0, 1, 1) * (1 - lerp) + new Vector3(1, 1, 0) * lerp;
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), dir * (i + 2f) * 5 + Player.Position + new Vector2(dir.Y, -dir.X) * MathF.Sin(i / 2 - time * 5) / 2, Util.ToAngle(Player.Direction), new Color(color.X, color.Y, color.Z) * (1 - lerp)));
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
                enemy.Collide(proj.Damage);
                enemy.ApplyWork(-10);
            }
        }
    }
    public override void OnUpdate(float _fuseRatio)
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
                beamBlend.Volume = 1 - lerp;
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
        base.OnUpdate(_fuseRatio);
        isFiring = false;
    }
    public override int StealthChange() => GunStealthChange();
}
public class MatrixLauncher() : Weapon(Modules.MatrixLauncher)
{
    ReloadSystem ammo = new ReloadSystem(3, 2);
    public override float Speed => 12;
    public override bool CritCondition => Player.Health <= 20;
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            Vector2 vel = Player.IdealSpeedWithVelocity(Speed);
            Player.Shoot(new FlameBolt(Player.Position, vel + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 2, Team, 6,
                new ParticleEmitter(Assets.Get(Sprites.Circle), Player.Position, 0, Color.Cyan) { sprayCone = MathF.PI * 2 / 3, sprayAngle = Util.ToAngle(vel - Player.Velocity), speedOfEmission = 0.5f }, 4, 0, -20), 2, CritCondition);
            SoundManager.PlaySound(Assets.Get(Sound.SniperFire), Player.Position);
            Cooldown = 1.5f;
            Engine.Camera.Position += Player.Direction * Speed + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
            Engine.ShakeScreen(0.5f);
            Player.Flash(Color.Cyan);
        }
    }
    public override void OnUpdate(float _fuseRatio)
    {
        ammo.Update(this, _fuseRatio);
        base.OnUpdate(_fuseRatio);
    }
    public override int StealthChange() => GunStealthChange();
}
public class Torch() : Weapon(Modules.Torch)
{
    ReloadSystem ammo = new ReloadSystem(8, 1);
    int count = 0;
    float betweenShots = 0;
    private float critCD = 0;
    public override float Speed => 12;
    public override bool CritCondition => critCD > 0;
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
    public override void OnUpdate(float _fuseRatio)
    {
        if (count > 0)
        {
            if (betweenShots <= 0)
            {
                betweenShots = 0.05f;
                Vector2 offset = new Vector2(Player.Direction.Y, -Player.Direction.X) * Util.OneToNegOne() * 3;
                var shot = new FlameBolt(Player.Position - offset * 5, Player.IdealSpeedWithVelocity(Speed) + offset / 3, Team, 2, 2, 0.1f, 0, 20);
                Player.Shoot(shot, 2.5f, CritCondition);
                SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
                Engine.ShakeScreen(0.2f);
                Util.FiringParticles(Player.Position + Player.Direction * 6, Player.Velocity, Player.Direction);
                Engine.Camera.Position += Player.Direction * Speed / 2;
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
        if(critCD > 0)
        {
            critCD -= Engine.DeltaSeconds;
        }
        ammo.Update(this, _fuseRatio);
        base.OnUpdate(_fuseRatio);
    }
    public override int StealthChange() => GunStealthChange();
    public override void OnEnemyHit(Entity _entity, int _damage)
    {
        if(_entity.Temperature > 1)
        {
            critCD = 0.5f;
        }
    }
}
public class SplitterModule() : Weapon(Modules.SplitterModule)
{
    ReloadSystem ammo = new ReloadSystem(6, 3);
    public override float Speed => 8;
    public override bool CritCondition 
    { 
        get 
        {
            var nearestPlanet = Util.Nearest(Player.Position, [.. Engine.SaveGame.CurrentMission.Entities.Where(x => x is Planet)]);
            return Vector2.Distance(Player.Position, nearestPlanet.Position) < nearestPlanet.ColliderRadius * 1.2f;
        } 
    }
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            List<Func<Vector2, Vector2, float, Entity>> missiles = [];
            for (int i = 0; i < 3; i++)
            {
                missiles.Add(delegate(Vector2 _position, Vector2 _velocity, float _angle) { return NewMissile(_position, _velocity, _angle, Team, 1); });
            }
            Player.Shoot(NewSplitter(Player.Position + Player.Direction * 6, Player.IdealSpeedWithVelocity(Speed), Util.ToAngle(Player.Direction), Team, 8, missiles, 0.5f), 1.5f, CritCondition);
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
            Engine.ShakeScreen(0.5f);
            Player.Velocity -= Player.Direction;
            Cooldown = 0.75f;
            Engine.Camera.Position += Player.Direction * Speed + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
            Util.FiringParticles(Player.Position + Player.Direction * 6, Player.Velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
        }
    }
    public override void OnUpdate(float _fuseRatio)
    {
        ammo.Update(this, _fuseRatio);
        base.OnUpdate(_fuseRatio);
    }
    public override int StealthChange() => GunStealthChange();
}
public class Fractal() : Weapon(Modules.Fractal)
{
    ReloadSystem ammo = new ReloadSystem(10, 3);
    public override float Speed => 6;
    public override bool CritCondition => Player.Velocity.Length() > 20;
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            List<Func<Vector2, Vector2, float, Entity>> splitters = [];
            for (int i = 0; i < 3; i++)
            {
                List<Func<Vector2, Vector2, float, Entity>> finalBullets = [];
                for (int j = 0; j < 8; j++)
                {
                    finalBullets.Add(delegate (Vector2 _position, Vector2 _velocity, float _angle) { return Player.Modify(NewPulseShot(_position, _velocity, _angle, 0, Team, 3, false, 1), 1.5f, CritCondition); });
                }
                splitters.Add(delegate (Vector2 _position, Vector2 _velocity, float _angle) { var p2 = Player.Modify(NewSplitter(_position, _velocity, _angle, Team, 5, finalBullets, 0.2f, 1), 1.75f, CritCondition); p2.Texture = Assets.Get(Sprites.Glow); return p2; });
            }
            var p1 = NewSplitter(Player.Position, Player.IdealSpeedWithVelocity(Speed), Util.ToAngle(Player.Direction), Team, 8, splitters, 0.2f);
            p1.Texture = Assets.Get(Sprites.Glow);
            Player.Shoot(p1, 2, CritCondition);
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
            Engine.ShakeScreen(0.3f);
            Player.Velocity -= Player.Direction / 2;
            Cooldown = 0.25f;
            Engine.Camera.Position += Player.Direction * Speed + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
            Util.FiringParticles(Player.Position + Player.Direction * 6, Player.Velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
        }
    }
    public override void OnUpdate(float _fuseRatio)
    {
        ammo.Update(this, _fuseRatio);
        base.OnUpdate(_fuseRatio);
    }
    public override int StealthChange() => GunStealthChange();
}
public class CrackShot : Weapon
{
    ReloadSystem ammo;
    public override float Speed => 8;
    public override bool CritCondition => critCD > 0;
    private float critCD = 0;
    public CrackShot() : base (Modules.CrackShot)
    {
        ammo = new ReloadSystem(6, 2.5f, ReloadCallback);
    }
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            Player.Shoot(NewSplitter(Player.Position, Player.IdealSpeedWithVelocity(Speed), Util.ToAngle(Player.Direction), Team, 5, [delegate (Vector2 _position, Vector2 _velocity, float _angle) { return Player.Modify(NewAssassinShot(_position, _velocity, _angle, 0, Team, 3, 0), 1.25f, CritCondition); }], 0.2f, 0, true), 1.25f, CritCondition);
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
            Engine.ShakeScreen(0.3f);
            Player.Velocity -= Player.Direction / 2;
            Cooldown = 0.2f;
            Engine.Camera.Position += Player.Direction * Speed;
            Util.FiringParticles(Player.Position + Player.Direction * 6, Player.Velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
        }
    }
    private void ReloadCallback()
    {
        critCD = 1.2f;
    }
    public override void OnUpdate(float _fuseRatio)
    {
        ammo.Update(this, _fuseRatio);
        if(critCD > 0)
        {
            critCD -= Engine.DeltaSeconds;
        }
        base.OnUpdate(_fuseRatio);
    }
    public override int StealthChange() => GunStealthChange();
}
public class MicroRocketLauncher() : Weapon(Modules.MicroRocketLauncher)
{
    ReloadSystem ammo = new ReloadSystem(30, 4);
    float offset = 2;
    private List<(float timer, Entity entity)> hitEntities = [];
    public override float Speed => 5;
    public override bool CritCondition => hitEntities.Count > 3;
    public override void OnShoot()
    {
        if (Cooldown > 0)
        {
            return;
        }
        if (ammo.Fire())
        {
            Vector2 speed = Player.IdealSpeedWithVelocity(Speed);
            var dir = Vector2.Normalize(speed - Player.Velocity);
            Vector2 finalSpeed = speed + new Vector2(dir.Y, -dir.X) * offset;
            Player.Shoot(NewMissile(Player.Position + Player.Direction * 6, finalSpeed, Util.ToAngle(finalSpeed), Team, 3, 3, 5), 1.7f, CritCondition);
            Engine.Camera.Position += Player.Direction * Speed;
            SoundManager.PlaySound(Assets.Get(Sound.MissileFire), Player.Position);
            Cooldown = 0.25f;
            Engine.ShakeScreen(0.2f);
            offset *= -1;
        }

    }
    public override void OnUpdate(float _fuseRatio)
    {
        ammo.Update(this, _fuseRatio);
        for (int i = 0; i < hitEntities.Count; i++)
        {
            var (timer, entity) = hitEntities[i];
            hitEntities[i] = (timer - Engine.DeltaSeconds, entity);
        }
        hitEntities = [.. hitEntities.Where(x => x.timer > 0)];
        base.OnUpdate(_fuseRatio);
    }
    public override void OnEnemyHit(Entity _entity, int _damage)
    {
        foreach (var pair in hitEntities)
        {
            if (pair.entity == _entity)
            {
                return;
            }
        }
        hitEntities.Add((5f, _entity));
    }
    public override int StealthChange() => GunStealthChange();
}
public class AdaptiveShotgun() : Weapon(Modules.AdaptiveShotgun)
{
    ReloadSystem ammo = new ReloadSystem(2, 3);
    public override float Speed => 18;
    public override bool CritCondition => ammo.Rounds == 1;
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
                Vector2 speed = Player.IdealSpeedWithVelocity(Speed);
                var dir = Vector2.Normalize(speed);
                Vector2 offset = (dir * i / 5 + new Vector2(dir.Y, -dir.X)) * i * 100 / distance;
                Vector2 targetVector = speed + offset;
                var p1 = NewPulseShot(Player.Position, targetVector, Util.ToAngle(targetVector - Player.Velocity), 0, Team, 6 - (int)MathF.Abs(i), true, 0);
                p1.Texture = Assets.Get(Sprites.Microshot);
                p1.GetComponent<ExpireTimer>().TimeLeft = 5;
                Player.Shoot(p1, 1.5f, CritCondition);
            }
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
            Player.Velocity -= Player.Direction * 2;
            Cooldown = 0.75f;
            Engine.Camera.Position += Player.Direction * Speed + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
            Engine.ShakeScreen(0.6f);
            Util.FiringParticles(Player.Position + Player.Direction * 8, Player.Velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
        }
    }
    public override void OnUpdate(float _fuseRatio)
    {
        ammo.Update(this, _fuseRatio);
        base.OnUpdate(_fuseRatio);
    }
    public override int StealthChange() => GunStealthChange();
}
public class GuidedRound() : Weapon(Modules.GuidedRound)
{
    private ReloadSystem ammo = new ReloadSystem(3, 3);
    private List<Entity> rounds = [];
    public override float Speed => 9;
    public override bool CritCondition => rounds.Count >= 3;
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
            Vector2 vel = Player.IdealSpeedWithVelocity(Speed) + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()) / 2;
            var round = NewAssassinShot(Player.Position + Util.ToUnitVector(Player.Angle) * 10, vel, Util.ToAngle(vel - Player.Velocity), 0, Team, 10);
            round.TimeLeft = 20;
            round.Texture = Assets.Get(Sprites.Glow);
            rounds.Add(round);
            Player.Shoot(round, 1, false);
            SoundManager.PlaySound(Assets.Get(Sound.PulseFire), Player.Position);
            Cooldown = 0.5f;
            Engine.ShakeScreen(0.2f);
            Engine.Camera.Position += Player.Direction * Speed + new Vector2(Util.OneToNegOne(), Util.OneToNegOne());
            Player.Velocity -= Player.Direction / 3;
            Util.FiringParticles(Player.Position + Player.Direction * 6, Player.Velocity, Player.Direction);
            Player.Flash(Color.BurlyWood);
        }
    }
    public override void OnUpdate(float _fuseRatio)
    {
        rounds = [.. rounds.Where(x => !x.isExpired)];
        ammo.Update(this, _fuseRatio);
        if(Input.NewMouseState.LeftButton == ButtonState.Released && rounds.Count >= 3)
        {
            foreach(var round in rounds)
            {
                var component = round.GetComponent<Attack>();
                component.Damage = (int)MathF.Round(round.Damage * 1.5f);
                component.IsCrit = true;
            }
            rounds.Clear();
        }
        base.OnUpdate(_fuseRatio);
    }
    public override int StealthChange() => GunStealthChange();
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
        Player.invincibilityCd = 0.5f;
        Player.Velocity += Player.Direction * 10;
        for (int i = 0; i < 300; i++)
        {
            float timeLeft = (float)i / 300;
            var col = Color.SlateBlue;
            col.A = 0;
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), timeLeft, Player.Position + Player.Direction * i, Player.Velocity * timeLeft, Util.ToAngle(Player.Direction), 0, Color.Cyan, col));
        }
        Player.Position += Player.Direction * 300;
        Cooldown = MaxCooldown;
    }
    public override void OnUpdate(float _fuseRatio)
    {
        UI.PlayerAbility.SetInterval(1 - Cooldown / MaxCooldown, 1);
        base.OnUpdate(_fuseRatio);
    }
}
public class SummonShield() : Module(Modules.SummonShield)
{
    Entity shield1;
    Entity shield2;
    const float MaxCooldown = 15;
    public override void OnAbility()
    {
        if (Cooldown > 0 || shield1 != null && shield2 != null)
        {
            return;
        }
        shield1 = NewShield(Player, 12, 20, MathF.PI / 4, 1, Team);
        Engine.SaveGame.CurrentMission.Add(shield1);
        shield2 = NewShield(Player, 12, 20, -MathF.PI / 4, 1, Team);
        Engine.SaveGame.CurrentMission.Add(shield2);
        Cooldown = MaxCooldown;
    }
    public override void OnUpdate(float _fuseRatio)
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
        base.OnUpdate(_fuseRatio);
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
    public override void OnUpdate(float _fuseRatio)
    {
        if (hook != null && hook.isExpired)
        {
            hook = null;
            p = null;
        }
        if (p != null)
        {
            hook.Parent.Position = p.Position += offset;
        }
        UI.PlayerAbility.SetInterval(1 - Cooldown / MaxCooldown, 1);
        base.OnUpdate(_fuseRatio);
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
    public override void OnUpdate(float _fuseRatio)
    {
        UI.PlayerAbility.SetInterval(1 - Cooldown / MaxCooldown, 1);
        base.OnUpdate(_fuseRatio);
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
                    var enemy = NewSurgeChild(Player.Position + new Vector2(Util.OneToNegOne(), Util.OneToNegOne()), Player.Velocity, Player.Angle, Player, allies);
                    enemy.Team = Team.Friendly;
                    enemy.GetComponent<Behaviour>().AddBehaviour(enemy.AvoidProjectiles(1));
                    Engine.SaveGame.CurrentMission.Add(enemy);
                    allies.Add(enemy);
                }
                Cooldown = MaxCooldown;
                return;
            }
        }
    }
    public override void OnUpdate(float _fuseRatio)
    {
        UI.PlayerAbility.SetInterval(1 - Cooldown / MaxCooldown, 1);
        allies = [.. allies.Where(x => !x.isExpired)];
        base.OnUpdate(_fuseRatio);
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
            Player.Shoot(NewPulseShot(Player.Position, Util.ToUnitVector(angle) * 10, angle, 0, Team, 20, true, 1), 1, false);
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
    public override void OnUpdate(float _fuseRatio)
    {
        if (resistanceCooldown > 0)
        {
            resistanceCooldown -= Engine.DeltaSeconds;
        }
        if (isShooting && Cooldown <= 0)
        {
            Player.Shoot(NewMissile(Player.Position, Util.ToUnitVector(count * MathF.PI * 2 / 3) * 5, count * MathF.PI * 2 / 3, Team), 1, false);
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
        base.OnUpdate(_fuseRatio);
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
        Engine.SaveGame.CurrentMission.Add(NewDecoy(Engine.SaveGame.Player.Position, Vector2.Zero, Engine.SaveGame.Player.Angle, Sprites.Player, Team));
        Cooldown = 15f;
    }
    public override int StealthChange()
    {
        if (Cooldown > 0)
        {
            return 1;
        }
        return 0;
    }
}
public class TargettingModifer() : Module(Modules.Sensors)
{
    public override float ModifyCrit(float _crit)
    {
        return _crit * 1.25f + 0.1f;
    }
    public override Entity ModifyProjectile(Entity _projectile)
    {
        var damage = _projectile.GetComponent<Attack>();
        if (damage != null)
        {
            damage.Damage = (int)MathF.Round(damage.Damage*0.9f);
        }
        return _projectile;
    }
}
public class Expose() : Module(Modules.Expose)
{
    const float MaxCooldown = 15;
    FlameBolt aura = null;
    private bool isFire = false;
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
            Player.Shoot(aura = new FlameBolt(Player.Position + new Vector2(Input.NewMouseState.X, Input.NewMouseState.Y) + Engine.MousePositionOffset - Engine.BackBuffer / 2, Vector2.Zero, Team, 0, new ParticleEmitter(Assets.Get(Sprites.Dot), Player.Position, 0, Color.Orange * 0.75f) { speedOfEmission = 0.5f }, 10, 2, 20), 1, false);
            isFire = true;        
        }
        else
        {
            Player.Shoot(aura = new FlameBolt(Player.Position + new Vector2(Input.NewMouseState.X, Input.NewMouseState.Y) + Engine.MousePositionOffset - Engine.BackBuffer / 2, Vector2.Zero, Team, 0, new ParticleEmitter(Assets.Get(Sprites.Dot), Player.Position, 0, Color.Cyan * 0.75f) { speedOfEmission = 0.5f }, 10, 2, -20), 1, false);
            isFire = false;
        }
        aura.Transform.IsImmovable = true;
        Cooldown = 15;
    }
    public override void OnUpdate(float _fuseRatio)
    {
        UI.PlayerAbility.SetInterval(1 - Cooldown / MaxCooldown, 1);
        //Helps the player with temperature control
        if(aura != null)
        {
            if(isFire)
            {
                Engine.SaveGame.Player.ConductHeat(-1.5f, 0.1f);
            }
            else
            {
                Engine.SaveGame.Player.ConductHeat(1.5f, 0.1f);
            }
        }
        base.OnUpdate(_fuseRatio);
    }
}
public class CloakingModifier() : Module(Modules.CloakingModifier)
{
    public override Entity ModifyProjectile(Entity _projectile)
    {
        if(Util.Random.NextSingle() > 0.5f * Player.CalculateFuseRatio(ModuleType.Sensors))
        {
            _projectile.StealthAbility = 99;
        }
        return _projectile;
    }
}
public class ProjectingModifier() : Module(Modules.ProjectingModifier)
{
    public override void OnUpdate(float _fuseRatio)
    {
        if(!Player.IsDocked && Player.Progression > 0)
        {
            Engine.SaveGame.CurrentMission.CalculateTrajectory(Player.Position, Player.IdealSpeedWithVelocity(Player.Gun.Speed), 8 * SaveGame.EnemyHitboxModifier, 1);
        }
    }
}
public class AmplifyingModifier() : Module(Modules.AmplifyingModifier)
{
    public override Entity ModifyProjectile(Entity _projectile)
    {
        var damage = _projectile.GetComponent<Attack>();
        if(damage != null)
        {
            damage.Damage *= 2;
        }
        _projectile.Velocity *= 1.25f;
        return _projectile;
    }
    public override int OnCollide(int _damage)
    {
        return _damage * 2;
    }
}
public class EmergencyModule : Weapon
{
    public EmergencyModule() : base(Modules.EmergencyModule)
    {
        GetComponent<Smelt>().Value = 0;
    }
    float engineTime = 0;
    ParticleEmitter engineParticles = new(Assets.Get(Sprites.Circle), 0.15f, Vector2.Zero, 0, MathF.PI / 4, 2, 450f, Color.Cyan, EmitterType.EmissionOverTime)
    { particleFadeToColor = new Color(72, 61, 139, 0) };

    public override float Speed => 0;

    public override bool CritCondition => false;

    public override void OnEngine()
    {
        engineParticles.offsetVelocity = Player.Velocity;
        engineTime = Math.Clamp(engineTime + Engine.DeltaSeconds, 0, 1);
        float engineTimeModifier = 1 - (1 - engineTime) * (1 - engineTime);
        float fuseRatio = (float)Player.CountFuses(ModuleType.Engines) / 3;
        engineParticles.speedOfEmission = Math.Max(450f * fuseRatio * engineTimeModifier, 10);
        if (Player.EngineDirection != Vector2.Zero)
        {
            Player.Velocity += Vector2.Normalize(Player.EngineDirection) * 24 * Engine.DeltaSeconds * engineTimeModifier * fuseRatio / (Player.leashedMaterials.Count + 2);
            engineParticles.position = Player.Position - Vector2.Normalize(Player.EngineDirection) * 8 - Player.Velocity;
            engineParticles.sprayAngle = Util.ToAngle(Player.EngineDirection) + MathF.PI;
        }
        engineParticles.Update();
    }
    public override void OnUpdate(float _fuseRatio)
    {
        if (!Player.isEngineActive && engineTime > 0)
        {
            engineTime -= Engine.DeltaSeconds / _fuseRatio;
        }
        base.OnUpdate(_fuseRatio);
    }
}

