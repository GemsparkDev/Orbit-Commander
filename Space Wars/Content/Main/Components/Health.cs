using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;
using Microsoft.Xna.Framework;

namespace Space_Wars.Content.Main.Components;
internal class Health(Entity _entity) : Component(_entity)
{
    private int currentHealth;
    private int prevHealth;
    public int CurrentHealth { get => currentHealth; set { prevHealth = currentHealth; healthCD = 0.5f; currentHealth = value; } }
    public int MaxHealth { get; set; }
    private float healthCD = 0;
    public override void Update()
    {
        if (currentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
        if (healthCD <= 0)
        {
            if (prevHealth > currentHealth)
            {
                prevHealth -= 1 + MaxHealth / 20;
                healthCD = 0.05f;
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
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        var stealth = Entity.GetComponent<Stealth>();
        float val = 1;
        if(stealth != null)
        {
            val = (Engine.SaveGame.Player.SensingAbility > stealth.TrueStealth ? 1 : 0);
            if (Engine.SaveGame.Player.SensingAbility <= stealth.TrueStealth)
            {
                val = Math.Clamp(val + stealth.RevealDuration, 0, 1);
            }
        }
        if (CurrentHealth > 0)
        {
            //Health bar
            Vector2 barPosition = Entity.Position + new Vector2(-Entity.ColliderRadius, Entity.ColliderRadius / 2);
            Rectangle sourceRectangle = new(0, 0, (int)(Entity.ColliderRadius * 2), 2);
            _spriteBatch.Draw(Engine.Line, barPosition, sourceRectangle, new Color(0, 50, 25) * val);
            _spriteBatch.Draw(Engine.Line, barPosition, new Rectangle(sourceRectangle.Location, new Point((int)(sourceRectangle.Width * (float)(prevHealth) / (float)(MaxHealth)), sourceRectangle.Height)), Color.White * val);
            _spriteBatch.Draw(Engine.Line, barPosition, new Rectangle(sourceRectangle.Location, new Point((int)(sourceRectangle.Width * (float)(currentHealth) / (float)(MaxHealth)), sourceRectangle.Height)), Color.Green * val);
        }
    }
}
