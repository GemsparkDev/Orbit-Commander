using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;
using System;

namespace Space_Wars.Content.Main.Components;
internal class Health(Entity _entity) : Component(_entity)
{
    private int currentHealth;
    private int prevHealth;
    public int CurrentHealth { get => currentHealth; set { if (prevHealth <= currentHealth) { prevHealth = currentHealth; } healthCD = 0.5f; currentHealth = value; } }
    public int MaxHealth { get; set; }
    private float healthCD = 0;
    public override void Update()
    {
        if (currentHealth > MaxHealth)
        {
            currentHealth = MaxHealth;
        }
        var comp = Entity.GetComponent<FollowEmitter>();
        if (comp != null && CurrentHealth > 0)
        {
            comp.ParticleEmitter.isEmitterActive = SaveGame.DebugMode;
        }
        if (healthCD <= 0)
        {
            if (prevHealth > currentHealth)
            {
                prevHealth -= 1;
                healthCD = 1 / (float)(MaxHealth);
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
            Entity.Team = Team.Dead;
            d = 0.67f;
        }
        if (Entity is not Pickup and not Planet)
        {
            Entity.GetComponent<Sprite>().TargetColor = SaveGame.ColorScheme.TeamColors[Entity.Team] * d; //Sets color based on friendlyness
        }
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        var stealth = Entity.GetComponent<Stealth>();
        float val = 1;
        if (stealth != null)
        {
            val = (Engine.SaveGame.Player.SensingAbility > stealth.TrueStealth ? 1 : 0);
            if (Engine.SaveGame.Player.SensingAbility <= stealth.TrueStealth)
            {
                val = Math.Clamp(val + stealth.RevealDuration, 0, 1);
            }
        }
        if (CurrentHealth > 0 && !(Entity.GetComponent<IsChild>()?.ChildEnemy ?? false))
        {
            //Health bar
            Vector2 barPosition = Entity.Position + new Vector2(-Entity.ColliderRadius * 0.875f, Entity.ColliderRadius * 1.1f);
            Rectangle sourceRectangle = new(0, 0, (int)(Entity.ColliderRadius * 1.75f), 2);
            _spriteBatch.Draw(Engine.Line, barPosition, sourceRectangle, new Color(0, 50, 25) * val);
            _spriteBatch.Draw(Engine.Line, barPosition, new Rectangle(sourceRectangle.Location, new Point((int)(sourceRectangle.Width * ((float)(prevHealth) / (float)(MaxHealth))), sourceRectangle.Height)), Color.White * val);
            _spriteBatch.Draw(Engine.Line, barPosition, new Rectangle(sourceRectangle.Location, new Point((int)(sourceRectangle.Width * (float)(currentHealth) / (float)(MaxHealth)), sourceRectangle.Height)), Color.Green * val);
        }
    }
}
