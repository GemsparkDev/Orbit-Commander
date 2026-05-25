using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrbitCommander.Entities;
using System;
using OrbitCommander.Core;

namespace OrbitCommander.Components;
internal class Health(Entity _entity) : IComponent
{
    private int currentHealth;
    private int prevHealth;
    public int CurrentHealth { get => currentHealth; set { if (prevHealth <= currentHealth) { prevHealth = currentHealth; } healthCD = 0.5f; currentHealth = value; } }
    public int MaxHealth { get; set; }
    private float healthCD = 0;
    public void Update()
    {
        if (currentHealth > MaxHealth)
        {
            currentHealth = MaxHealth;
        }
        if (healthCD <= 0)
        {
            if (prevHealth > currentHealth)
            {
                prevHealth -= 1;
                healthCD = 1 / (float)MaxHealth;
            }
            else
            {
                prevHealth = currentHealth;
            }
        }
        else
        {
            healthCD -= Engine.DeltaSeconds;
        }
        float d = 1f;
        if (CurrentHealth <= 0)
        {
            _entity.Team = Team.Dead;
            d = 0.67f;
        }
        if (_entity is not Pickup and not Planet)
        {
            _entity.GetComponent<Sprite>().TargetColor = SaveGame.ColorScheme.TeamColors[_entity.Team] * d; //Sets color based on friendlyness
        }
    }
    public void Draw(SpriteBatch _spriteBatch)
    {
        var stealth = _entity.GetComponent<Stealth>();
        float val = 1;
        if (stealth != null)
        {
            val = Engine.SaveGame.Player.SensingAbility > stealth.TrueStealth ? 1 : 0;
            if (Engine.SaveGame.Player.SensingAbility <= stealth.TrueStealth)
            {
                val = Math.Clamp(val + stealth.RevealDuration, 0, 1);
            }
        }
        if (CurrentHealth > 0 && !(_entity.GetComponent<IsChild>()?.ChildEnemy ?? false))
        {
            //Health bar
            Vector2 barPosition = _entity.Position + new Vector2(-_entity.ColliderRadius * 0.875f, _entity.ColliderRadius * 1.1f);
            Rectangle sourceRectangle = new(0, 0, (int)(_entity.ColliderRadius * 1.75f), 2);
            _spriteBatch.Draw(Engine.Line, barPosition, sourceRectangle, new Color(0, 50, 25) * val);
            _spriteBatch.Draw(Engine.Line, barPosition, new Rectangle(sourceRectangle.Location, new Point((int)(sourceRectangle.Width * (prevHealth / (float)MaxHealth)), sourceRectangle.Height)), Color.White * val);
            _spriteBatch.Draw(Engine.Line, barPosition, new Rectangle(sourceRectangle.Location, new Point((int)(sourceRectangle.Width * (float)currentHealth / MaxHealth), sourceRectangle.Height)), Color.Green * val);
        }
    }
}
