using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Particles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;
using UILib.Content.Main;

namespace Space_Wars.Content.Main.Entities;

public class Module : Pickup, IData
{
    //Serialized fields
    private int health = 20;
    public bool isFailed = false;
    public new Modules Type { get; }

    public int Health { get { return health; } set { health = value; UpdateHealth(); } }
    public int MaxHealth => (itemData as ModuleData).MaxHealth;
    public override Color Color => isFailed ? Color.Red : Color.White;
    public float cooldown = 0;
    private Decal healthDecal;

    public Module(Modules _type, Vector2 _position = default, Vector2 _velocity = default, float _angularVelocity = 0) : base(ItemFactory.moduleData[_type], _position, _velocity, _angularVelocity)
    {
        health = MaxHealth;
        Type = _type;
        healthDecal = new Decal(new Vector2(0, 5), Assets.TextFont, $"{Health} / {MaxHealth}", Color.Pink, 5f);
        Tooltip.AddWidget(healthDecal);
    }
    public Module(Modules _type, List<string> _disassembly, LoadLogger _logger) : base(ItemFactory.moduleData[_type], _disassembly, _logger)
    {
        _logger.Try(delegate { health = Int32.Parse(_disassembly[2]); }, 2);
        _logger.Try(delegate { isFailed = bool.Parse(_disassembly[3]); }, 3);
        Type = _type;
        healthDecal = new Decal(new Vector2(0, 5), Assets.TextFont, $"{Health} / {MaxHealth}", Color.Pink, 5f);
        Tooltip.AddWidget(healthDecal);
    }
    public void UpdateCooldown()
    {
        if (cooldown > 0)
        {
            cooldown -= Engine.DeltaSeconds;
        }
    }
    private void UpdateHealth()
    {
        healthDecal.text = $"{Health} / {MaxHealth}";
    }
    public bool IsCooldownReady()
    {
        return cooldown <= 0;
    }
    public void ModuleFunction()
    {
        (itemData as ModuleData).Action();
    }
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
        ParticleManager.Add(new Particle(null, 1, position + new Vector2(0, -1), new Vector2(0, -1.5f), 0, 0, Color.Orange, new Color(255, 0, 0, 0)) { drawText = $"Integrity: {Health}" });
        SoundManager.PlaySound(Assets.Get(Sound.Death), position);
        Engine.ShakeScreen(10 / ((position - Engine.Camera.Position).Length() + 150));
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
    public new string Serialize()
    {
        return $"{{{Type},{SerializeAttributes()},{health},{isFailed}}}";
    }
}
public class ModuleData(Sprite _realSprite, Sprite _virtualSprite, String _name, int _id, int _health, Action _action)
    : ItemData(_realSprite, _virtualSprite, _name, _id, Color.White)
{
    public int MaxHealth { get; } = _health;
    public Action Action { get; } = _action;
}

