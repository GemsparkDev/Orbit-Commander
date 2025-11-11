using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework.Audio;
using Space_Wars.Content.Main.Components;
using System.Diagnostics;
using Space_Wars.Content.Main.Particles;
using System.Collections.Generic;

namespace Space_Wars.Content.Main.Entities;

public enum EntityType { Projectile, Enemy, Item, None }
public abstract class Entity
{
    public Texture2D texture;
    public SoundEffect hitSound;
    protected Color color = Color.White;
    public Vector2 position;
    public Vector2 velocity;
    public float angularVelocity = 0;
    public float angle;
    public bool isExpired = false;
    public bool isFriendly;
    public int damage;
    public virtual int SensingAbility { get; protected set; } = 1;
    public virtual int StealthAbility { get { return stealthAbility; } protected set { stealthAbility = value; } }
    private int stealthAbility = 0;
    protected float revealDuration = 0;
    public EntityType entityType;
    public ComponentList Components { get; } = new();
    public virtual float ColliderRadius
    {
        get { return (texture.Height + texture.Width) / 4 + 1; }
    }
    protected ParticleEmitter collider;
    public Vector2 Size
    {
        get { return texture == null ? Vector2.Zero : new Vector2(texture.Width, texture.Height); }
    }
    public virtual void LowerCooldown() { }
    protected static Player Player => Engine.SaveGame.Player;
    public StatusHolder StatusHolder { get; private set; } = new();
    public Entity(Texture2D _texture, Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, int _damage, bool _isFriendly)
    {
        texture = _texture;
        position = _position;
        velocity = _velocity;
        angle = _angle;
        angularVelocity = _angularVelocity;
        damage = _damage;
        isFriendly = _isFriendly;
        collider = new ParticleEmitter(Assets.Get(Sprite.Dot), position, ColliderRadius, Color.Yellow) { isEmitterActive = false };
    }
    public virtual void Update()
    {
        if (revealDuration > 0)
        {
            revealDuration -= Engine.DeltaSeconds;
        }
        StatusHolder.Update(this);
        collider.position = position;
        collider.Update();
    }
    public abstract bool Collide(int _damage, bool _ignoreImmunity = false);
    public void ClearAll()
    {
        StatusHolder = new();
    }
    public virtual void UpdateColor()
    {
        color = Color.White;
    }
    public void ClampVelocity(float speed)
    {
        Vector2 clampVelocity = Vector2.Normalize(velocity) * speed;
        if (MathF.Abs(velocity.X) < 0.0001 && MathF.Abs(velocity.Y) < 0.0001)
        {
            clampVelocity = Vector2.One * speed;
        }
        if (MathF.Abs(velocity.X) > MathF.Abs(clampVelocity.X))
        {
            velocity.X = clampVelocity.X;
        }
        if (MathF.Abs(velocity.Y) > MathF.Abs(clampVelocity.Y))
        {
            velocity.Y = clampVelocity.Y;
        }
    }
    public void Reveal(float _duration)
    {
        revealDuration = _duration;
    }
    public virtual void Draw(SpriteBatch _spriteBatch)
    {
        Vector2 halfSize = (Engine.BackBuffer + Size) / 2;
        Vector2 pos = Engine.Camera.Position + Engine.MousePositionOffset;
        if (position.X - pos.X < -halfSize.X || position.Y - pos.Y < -halfSize.Y
         || position.X - pos.X >  halfSize.X || position.Y - pos.Y >  halfSize.Y)
        {
            return;
        }
        float stealth = Convert.ToSingle(color.A) / 255;
        var maxDistance = EntityManager.StealthRange * (float)Engine.SaveGame.Player.CountFuses(ModuleType.Sensors) / 4;
        //Player has superior sensing to stealth -> full detection
        //Player has equal sensing to stealth -> partial detection when nearby
        //Player has inferior sensing to stealth -> no detection
        if (Engine.SaveGame.Player.SensingAbility == stealthAbility)
        {
            float distanceSqr = EntityManager.DistanceSqr(Engine.SaveGame.Player, this);
            if (distanceSqr > maxDistance * maxDistance)
            {
                stealth = 0;
            }
            else
            {
                stealth = MathF.Sqrt(maxDistance - MathF.Sqrt(distanceSqr)) / MathF.Sqrt(maxDistance);
            }
        }
        else if (Engine.SaveGame.Player.SensingAbility < stealthAbility)
        {
            stealth  = 0;
        }
        stealth = MathF.Max(stealth, (float)Math.Clamp(revealDuration, 0f, 1f));
        //Outline in atmosphere looks better
        if (Engine.SaveGame.CurrentMission.GetAtmospherePressure(this) > 0 || Engine.ColorScheme.IsOutlined())
        {
            _spriteBatch.Draw(texture, position + new Vector2(0, 1), null, Color.Black, angle, Size / 2, 1, 0, 0);
            _spriteBatch.Draw(texture, position + new Vector2(0, -1), null, Color.Black, angle, Size / 2, 1, 0, 0);
            _spriteBatch.Draw(texture, position + new Vector2(1, 0), null, Color.Black, angle, Size / 2, 1, 0, 0);
            _spriteBatch.Draw(texture, position + new Vector2(-1, 0), null, Color.Black, angle, Size / 2, 1, 0, 0);
        }
        _spriteBatch.Draw(texture, position, null, color * stealth, angle, Size / 2, 1, 0, 0);

        if (Engine.DebugMode)
        {
            //Direction of motion
            _spriteBatch.Draw(Engine.Line, position, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.LightBlue,
                MathF.Atan2(velocity.Y, velocity.X), Vector2.Zero, new Vector2(velocity.Length(), 0.5f), SpriteEffects.None, 0.4f);
            //Direction the entity is pointing
            _spriteBatch.Draw(Engine.Line, position, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.Red,
                angle - MathF.PI / 2, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.4f);
        }
        collider.isEmitterActive = Engine.DebugMode;
    }
    public virtual Entity Clone() { throw new NotImplementedException(); }
}
