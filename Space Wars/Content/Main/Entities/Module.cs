using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Particles;
using System;
using UILib.Content.Main;

namespace Space_Wars.Content.Main.Entities;

public class Module : Pickup, IData
{
    private ModuleData moduleData;
    private float health;
    public float Health { get { return health; } set { health = value; UpdateHealth(); } }
    public float MaxHealth => moduleData.MaxHealth;
    public override Color Color => isFailed ? Color.Red : Color.White;
    public bool isFailed = false;
    public float cooldown = 0;

    public Module(ModuleData _itemData, Color _worldColor, Vector2 _position, Vector2 _velocity, float _angularVelocity) : base(_itemData, _worldColor, _position, _velocity, _angularVelocity)
    {
        health = _itemData.MaxHealth;
        moduleData = _itemData;
        Tooltip.AddWidget(new Decal(new Vector2(0, 5), Assets.TextFont, $"{Health} / {moduleData.MaxHealth}", Color.Pink, 5f));
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
        Tooltip.GetWidget(2).text = $"{Health} / {moduleData.MaxHealth}";
    }
    public bool IsCooldownReady()
    {
        return cooldown <= 0;
    }
    public void ModuleFunction()
    {
        moduleData.Action();
    }
    public override void Collide(int _damage)
    {
        if (_damage <= 0)
        {
            return;
        }
        if (invincibilityCooldown > 0)
        {
            invincibilityCooldown = 0;
            return;
        }
        ParticleManager.Add(new Particle(null, 1, position + new Vector2(0, -1), new Vector2(0, -1.5f), 0, 0, Color.Orange, new Color(255, 0, 0, 0)) { drawText = $"Integrity: {Health}" });
        SoundManager.PlaySound(Assets.Get(Sound.Death), position);
        Engine.ShakeScreen(10 / ((position - Engine.Camera.Position).Length() + 150));
        if (Health > 0)
        {
            Health -= _damage;
            invincibilityCooldown = 1;
        }
        else
        {
            isExpired = true;
        }
    }
}
public class ModuleData(Sprite _realSprite, Sprite _virtualSprite, String _name, int _id, int _health, Action _action) : ItemData(_realSprite, _virtualSprite, _name, _id, Color.White)
{
    public float MaxHealth { get; } = _health;
    public Action Action { get; } = _action;
}

