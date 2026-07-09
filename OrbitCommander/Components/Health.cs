using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrbitCommander.Entities;
using System;
using OrbitCommander.Core;

namespace OrbitCommander.Components;
internal class Health(Entity _entity, int _maxHealth) : IComponent
{
    private int currentHealth = _maxHealth;
    private int prevHealth;
    public int CurrentHealth { get => currentHealth; 
        set 
        { 
            if (prevHealth <= currentHealth) 
            { 
                prevHealth = currentHealth; 
            } 
            healthCD = 0.5f; 
            currentHealth = value;
            if (currentHealth > MaxHealth)
            {
                currentHealth = MaxHealth;
            }
        } 
    }
    public int MaxHealth { get; set; } = _maxHealth;
    private float healthCD = 0;
    public void SetOverhealth(int _health)
    {
        currentHealth = _health;
        if (currentHealth > MaxHealth * 2)
        {
            currentHealth = MaxHealth * 2;
        }
        prevHealth = currentHealth;
    }
    public void Update()
    {
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
        if (CurrentHealth > 0 && !_entity.HasTag(Tags.IsChild))
        {
            //Health bar
            Vector2 barPosition = _entity.Position + new Vector2(-_entity.ColliderRadius * 0.875f, _entity.ColliderRadius * 1.1f);
            Rectangle sourceRectangle = new(0, 0, (int)(_entity.ColliderRadius * 1.75f), 2);
            _spriteBatch.Draw(Engine.Line, barPosition, sourceRectangle, new Color(0, 50, 25) * val);
            if(CurrentHealth > MaxHealth)
            {
                DrawGreenHealth();
                DrawWhiteOverhealth();
                _spriteBatch.Draw(Engine.Line, barPosition, new Rectangle(sourceRectangle.Location, new Point((int)(sourceRectangle.Width * (currentHealth / (float)MaxHealth - 1)), sourceRectangle.Height)), Color.Yellow * val);
            }
            else if(CurrentHealth < MaxHealth && prevHealth > MaxHealth)
            {
                _spriteBatch.Draw(Engine.Line, barPosition, new Rectangle(sourceRectangle.Location, new Point(sourceRectangle.Width, sourceRectangle.Height)), Color.White * val);
                DrawGreenHealth();
                DrawWhiteOverhealth();
            }
            else
            {
                _spriteBatch.Draw(Engine.Line, barPosition, new Rectangle(sourceRectangle.Location, new Point((int)(sourceRectangle.Width * (prevHealth / (float)MaxHealth)), sourceRectangle.Height)), Color.White * val);
                DrawGreenHealth();
            }
            void DrawGreenHealth()
            {
                _spriteBatch.Draw(Engine.Line, barPosition, new Rectangle(sourceRectangle.Location, new Point((int)(sourceRectangle.Width * Math.Clamp((float)currentHealth / MaxHealth, 0, 1)), sourceRectangle.Height)), Color.Green * val);
            }
            void DrawWhiteOverhealth()
            {
                _spriteBatch.Draw(Engine.Line, barPosition, new Rectangle(sourceRectangle.Location, new Point((int)(sourceRectangle.Width * (prevHealth / (float)MaxHealth - 1)), sourceRectangle.Height)), Color.White * val);
            }
        }
    }
}
