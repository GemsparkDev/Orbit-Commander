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
        var sc = _entity.GetComponent<Stealth>();

        Color tc = TargetColor * ((sc != null) ? sc.StealthTransparency() : 1);
        if (Color != tc)
        {
            float l = Util.FIED(0.025f);
            Color = new Color((byte)(_entity.Color.R * l + tc.R * (1f - l)), (byte)(_entity.Color.G * l + tc.G * (1f - l)), (byte)(_entity.Color.B * l + tc.B * (1f - l)), tc.A); //Lerp towards ideal color
        }
    }
    public void Draw(SpriteBatch _spriteBatch)
    {
        Vector2 halfSize = (Engine.BackBuffer / Engine.Camera.Zoom + Size) / 2;
        Vector2 pos = Engine.Camera.Position + Engine.MousePositionOffset;
        if (_entity.Position.X - pos.X < -halfSize.X || _entity.Position.Y - pos.Y < -halfSize.Y
         || _entity.Position.X - pos.X > halfSize.X || _entity.Position.Y - pos.Y > halfSize.Y)
        {
            return;
        }
        var attack = _entity.GetComponent<Attack>();
        if(attack != null && attack.IsCrit)
        {
            TargetColor = Color.White;
        }
        //Outline in atmosphere looks better
        _spriteBatch.Draw(Texture, _entity.Position + new Vector2(0, 1), null, Color.Black, _entity.Angle, Size / 2, 1, 0, 0);
        _spriteBatch.Draw(Texture, _entity.Position + new Vector2(0, -1), null, Color.Black, _entity.Angle, Size / 2, 1, 0, 0);
        _spriteBatch.Draw(Texture, _entity.Position + new Vector2(1, 0), null, Color.Black, _entity.Angle, Size / 2, 1, 0, 0);
        _spriteBatch.Draw(Texture, _entity.Position + new Vector2(-1, 0), null, Color.Black, _entity.Angle, Size / 2, 1, 0, 0);
        _spriteBatch.Draw(Texture, _entity.Position, null, Color, _entity.Angle, Size / 2, 1, 0, 0);
    }
}
