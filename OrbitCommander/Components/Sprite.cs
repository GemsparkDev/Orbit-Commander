using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrbitCommander.Entities;
using OrbitCommander.Particles;
using System;
using OrbitCommander.Core;
//using System.Numerics;

namespace OrbitCommander.Components;
public class Sprite(Entity _entity, Color _color) : IComponent
{
    private Texture2D texture;
    private ParticleEmitter collider;
    public Texture2D Texture
    {
        get { return texture; }
        set
        {
            texture = value;
            collider = new ParticleEmitter(Assets.Get(Sprites.Dot), _entity.Position, ColliderRadius, Color.Yellow) { isEmitterActive = false };
        }
    }
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
    public void Update()
    {
        collider.position = _entity.Position;
        collider.isEmitterActive = SaveGame.DebugMode;
        collider.particleVelocity = _entity.ColliderRadius;
        collider.Update();
        if (Color != TargetColor)
        {
            float l = Util.FIED(0.025f);
            Color = new Color((byte)(_entity.Color.R * l + TargetColor.R * (1f - l)), (byte)(_entity.Color.G * l + TargetColor.G * (1f - l)), (byte)(_entity.Color.B * l + TargetColor.B * (1f - l)), TargetColor.A); //Lerp towards ideal color
        }
    }
    public void Draw(SpriteBatch _spriteBatch)
    {
        Vector2 halfSize = (Engine.BackBuffer + Size) / 2;
        Vector2 pos = Engine.Camera.Position + Engine.MousePositionOffset;
        if (_entity.Position.X - pos.X < -halfSize.X || _entity.Position.Y - pos.Y < -halfSize.Y
         || _entity.Position.X - pos.X > halfSize.X || _entity.Position.Y - pos.Y > halfSize.Y)
        {
            return;
        }
        float stealth = Convert.ToSingle(Color.A) / 255;
        var sC = _entity.GetComponent<Stealth>();
        if (sC != null)
        {
            var maxDistance = Mission.StealthRange * Engine.SaveGame.Player.CountFuses(ModuleType.Sensors) / 4;
            //Player has superior sensing to stealth -> full detection
            //Player has equal sensing to stealth -> partial detection when nearby
            //Player has inferior sensing to stealth -> no detection
            if (Engine.SaveGame.Player.SensingAbility == sC.StealthAbility)
            {
                float distanceSqr = Vector2.DistanceSquared(Engine.SaveGame.Player.Position, _entity.Position);
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
                stealth = 0;
            }
            stealth = MathF.Max(stealth, (float)Math.Clamp(sC.RevealDuration, 0f, 1f));
        }
        //Outline in atmosphere looks better
        _spriteBatch.Draw(Texture, _entity.Position + new Vector2(0, 1), null, Color.Black, _entity.Angle, Size / 2, 1, 0, 0);
        _spriteBatch.Draw(Texture, _entity.Position + new Vector2(0, -1), null, Color.Black, _entity.Angle, Size / 2, 1, 0, 0);
        _spriteBatch.Draw(Texture, _entity.Position + new Vector2(1, 0), null, Color.Black, _entity.Angle, Size / 2, 1, 0, 0);
        _spriteBatch.Draw(Texture, _entity.Position + new Vector2(-1, 0), null, Color.Black, _entity.Angle, Size / 2, 1, 0, 0);
        _spriteBatch.Draw(Texture, _entity.Position, null, Color * stealth, _entity.Angle, Size / 2, 1, 0, 0);
    }
}
