using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Space_Wars.Content.Main.Components;
public class Sprite(Entity _entity, Color _color) : Component(_entity)
{
    private Texture2D texture;
    private ParticleEmitter collider;
    public Texture2D Texture { 
        get { return texture; } 
        set 
        {
            texture = value;
            collider = new ParticleEmitter(Assets.Get(Sprites.Dot), Entity.Position, ColliderRadius, Color.Yellow) { isEmitterActive = false };
        } }
    public Color Color { get; set; } = _color;
    public Color TargetColor { get; set; } = _color;
    public virtual float ColliderRadius
    {
        get { return Texture == null ? 0 : SaveGame.EnemyHitboxModifier * (Texture.Height + Texture.Width) / 4 + 1; }
    }
    public Vector2 Size
    {
        get { return Texture == null ? Vector2.Zero : new Vector2(Texture.Width, Texture.Height); }
    }
    public override void Update()
    {
        collider.position = Entity.Position;
        collider.isEmitterActive = SaveGame.DebugMode;
        collider.particleVelocity = Entity.ColliderRadius;
        collider.Update();
        if (Color != TargetColor)
        {
            float l = Util.FIED(0.025f);
            Color = new Color((byte)(Entity.Color.R * l + TargetColor.R * (1f - l)), (byte)(Entity.Color.G * l + TargetColor.G * (1f - l)), (byte)(Entity.Color.B * l + TargetColor.B * (1f - l)), (byte)(TargetColor.A)); //Lerp towards ideal color
        }
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        Vector2 halfSize = (Engine.BackBuffer + Size) / 2;
        Vector2 pos = Engine.Camera.Position + Engine.MousePositionOffset;
        if (Entity.Position.X - pos.X < -halfSize.X || Entity.Position.Y - pos.Y < -halfSize.Y
         || Entity.Position.X - pos.X >  halfSize.X || Entity.Position.Y - pos.Y >  halfSize.Y)
        {
            return;
        }
        float stealth = Convert.ToSingle(Color.A) / 255;
        var sC = Entity.GetComponent<Stealth>();
        if(sC != null)
        {
            var maxDistance = Mission.StealthRange * (float)Engine.SaveGame.Player.CountFuses(ModuleType.Sensors) / 4;
            //Player has superior sensing to stealth -> full detection
            //Player has equal sensing to stealth -> partial detection when nearby
            //Player has inferior sensing to stealth -> no detection
            if (Engine.SaveGame.Player.SensingAbility == sC.StealthAbility)
            {
                float distanceSqr = Vector2.DistanceSquared(Engine.SaveGame.Player.Position, Entity.Position);
                if (distanceSqr > maxDistance * maxDistance)
                {
                    stealth = 0;
                }
                else
                {
                    stealth = MathF.Sqrt(maxDistance - MathF.Sqrt(distanceSqr)) / MathF.Sqrt(maxDistance);
                }
            }
            else if (Engine.SaveGame.Player.SensingAbility < sC.StealthAbility)
            {
                stealth  = 0;
            }
            stealth = MathF.Max(stealth, (float)Math.Clamp(sC.RevealDuration, 0f, 1f));   
        }
        //Outline in atmosphere looks better
        if (Engine.SaveGame.CurrentMission.GetAtmospherePressure(Entity) > 0 || SaveGame.ColorScheme.IsOutlined())
        {
            _spriteBatch.Draw(Texture, Entity.Position + new Vector2(0, 1), null, Color.Black, Entity.Angle, Size / 2, 1, 0, 0);
            _spriteBatch.Draw(Texture, Entity.Position + new Vector2(0, -1), null, Color.Black, Entity.Angle, Size / 2, 1, 0, 0);
            _spriteBatch.Draw(Texture, Entity.Position + new Vector2(1, 0), null, Color.Black, Entity.Angle, Size / 2, 1, 0, 0);
            _spriteBatch.Draw(Texture, Entity.Position + new Vector2(-1, 0), null, Color.Black, Entity.Angle, Size / 2, 1, 0, 0);
        }
        _spriteBatch.Draw(Texture, Entity.Position, null, Color * stealth, Entity.Angle, Size / 2, 1, 0, 0);
    }
}
