using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Particles;
using System;
using UILib.Content.Main;

namespace Space_Wars.Content.Main.Entities;

public class Module : Pickup, IData
{
    private ModuleData moduleData;
    public float Health { get; set; }
    public float MaxHealth 
    {
        get { return moduleData.MaxHealth; }
    }
    public float[] Cost
    {
        get { return moduleData.Cost; }
    }
    public override Color Color 
    { 
        get { return (isFailed ? Color.Red : Color.White); } 
    }
    public bool isFailed = false;
    public float cooldown = 0;

    public Module(ModuleData _itemData, Texture2D _worldTexture, Color _worldColor, Vector2 _position, Vector2 _velocity, float _angularVelocity) : base(_itemData, _worldTexture, _worldColor, _position, _velocity, _angularVelocity)
    {
        Health = _itemData.MaxHealth;
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
    public void UpdateHealth()
    {
        Tooltip.GetWidget(2).text = $"{Health} / {moduleData.MaxHealth}";
    }
    public bool IsCooldownReady()
    {
        return cooldown <= 0;
    }
    public void ModuleFunction()
    {
        moduleData.Action.Value();
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
        ParticleManager.Add(new Particle(null, 1, position + new Vector2(0, -1), new Vector2(0, -1.5f), 0, 0, 1, true, Color.Orange, Color.Red) { drawText = $"Integrity: {Health}" });
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
public class ModuleData : ItemData
{
    public float MaxHealth { get; private set; }
    public float[] Cost { get; private set; }
    public Lazy<Action> Action { get; private set; }
    public ModuleData(Sprite _sprite, String _name, int _id, int _health, float[] _cost, Lazy<Action> _action) : base(_sprite, _name, _id, Color.White)
    {
        MaxHealth = _health;
        Cost = _cost;
        //Lazy evaluation to prevent error during module creation if the player is not valid
        Action = _action;
    }
}

