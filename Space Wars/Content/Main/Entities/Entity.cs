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
    public Vector2 Position { get { return GetComponent<Transform>().Position; } set { GetComponent<Transform>().Position = value; } }
    public Vector2 Velocity { get { return GetComponent<Transform>().Velocity; } set { GetComponent<Transform>().Velocity = value; } }
    public float Angle { get { return GetComponent<Transform>().Angle; } set { GetComponent<Transform>().Angle = value; } }
    public float AngularVelocity { get { return GetComponent<Transform>().AngularVelocity; } set { GetComponent<Transform>().AngularVelocity = value; } }

    public bool isExpired = false;
    public bool isFriendly;
    public virtual int SensingAbility { get; protected set; } = 1;
    public virtual int StealthAbility { get { return stealthAbility; } protected set { stealthAbility = value; } }
    private int stealthAbility = 0;
    protected float revealDuration = 0;
    public EntityType entityType;
    private List<Component> components;

    protected ParticleEmitter collider;
    public virtual float ColliderRadius => GetComponent<Sprite>().ColliderRadius;
    public Vector2 Size => GetComponent<Sprite>().Size;
    public float Temperature { get; private set; } = 0; //-1: Freeze, 0: Neutral, 1: Burn
    public virtual void LowerCooldown() { }
    protected static Player Player => Engine.SaveGame.Player;
    public Texture2D Texture { get { return GetComponent<Sprite>().Texture; } set { GetComponent<Sprite>().Texture = value; } }
    public Color Color { get { return GetComponent<Sprite>().Color; } set { GetComponent<Sprite>().Color = value; } }
    public StatusHolder StatusHolder { get; private set; } = new();
    public Entity(Texture2D _texture, Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, bool _isFriendly)
    {
        components = [new Transform(this) { Position = _position, Velocity = _velocity, Angle = _angle, AngularVelocity = _angularVelocity }, new Sprite(this) { Texture = _texture }];
        isFriendly = _isFriendly;
        collider = new ParticleEmitter(Assets.Get(Sprites.Dot), Position, ColliderRadius, Color.Yellow) { isEmitterActive = false };
    }
    public virtual void Update()
    {
        foreach (var comp in components)
        {
            comp.Update();
        }
        if (revealDuration > 0)
        {
            revealDuration -= Engine.DeltaSeconds;
        }
        StatusHolder.Update(this);
        collider.Update();
        collider.position = Position;
        collider.particleVelocity = ColliderRadius;
        Temperature *= Util.FIED(0.707f); //Radiative
        if(Temperature > 1)
        {
            StatusHolder.ApplyStatus(new Fire(1, Color.Orange));
        }
        if(Temperature < -1)
        {
            StatusHolder.ApplyStatus(new Frost(1));
        }
    }
    public T GetComponent<T>() where T : Component
    {
        foreach (Component component in components)
        {
            if (component.GetType().Equals(typeof(T)))
            {
                return (T)component;
            }
        }
        return null;
    }
    public void AddComponent(Component component)
    {
        components.Add(component);
    }
    public abstract bool Collide(int _damage, bool _ignoreImmunity = false);
    public void ClearAll()
    {
        StatusHolder = new();
    }
    public virtual void UpdateColor()
    {
        Color = Color.White;
    }
    public void Flash(Color _color)
    {
        Color = _color;
    }
    public void ClampVelocity(float speed)
    {
        Vector2 clampVelocity = Vector2.Normalize(Velocity) * speed;
        if (MathF.Abs(Velocity.X) < 0.0001 && MathF.Abs(Velocity.Y) < 0.0001)
        {
            clampVelocity = Vector2.One * speed;
        }
        if (MathF.Abs(Velocity.X) > MathF.Abs(clampVelocity.X))
        {
            Velocity = new Vector2(clampVelocity.X, Velocity.Y);
        }
        if (MathF.Abs(Velocity.Y) > MathF.Abs(clampVelocity.Y))
        {
            Velocity = new Vector2(Velocity.X, clampVelocity.Y);
        }
    }
    public void Reveal(float _duration)
    {
        revealDuration = _duration;
    }
    public void ApplyWork(float _q)
    {
        Temperature += _q * Engine.DeltaSeconds;
    }
    public void ConductHeat(float _temp, float _rate)
    {
        Temperature += (_temp - Temperature) * _rate * Engine.DeltaSeconds;
    }
    public virtual void Draw(SpriteBatch _spriteBatch)
    {
        Vector2 halfSize = (Engine.BackBuffer + Size) / 2;
        Vector2 pos = Engine.Camera.Position + Engine.MousePositionOffset;
        if (Position.X - pos.X < -halfSize.X || Position.Y - pos.Y < -halfSize.Y
         || Position.X - pos.X >  halfSize.X || Position.Y - pos.Y >  halfSize.Y)
        {
            return;
        }
        float stealth = Convert.ToSingle(Color.A) / 255;
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
        if (Engine.SaveGame.CurrentMission.GetAtmospherePressure(this) > 0 || SaveGame.ColorScheme.IsOutlined())
        {
            _spriteBatch.Draw(Texture, Position + new Vector2(0, 1), null, Color.Black, Angle, Size / 2, 1, 0, 0);
            _spriteBatch.Draw(Texture, Position + new Vector2(0, -1), null, Color.Black, Angle, Size / 2, 1, 0, 0);
            _spriteBatch.Draw(Texture, Position + new Vector2(1, 0), null, Color.Black, Angle, Size / 2, 1, 0, 0);
            _spriteBatch.Draw(Texture, Position + new Vector2(-1, 0), null, Color.Black, Angle, Size / 2, 1, 0, 0);
        }
        _spriteBatch.Draw(Texture, Position, null, Color * stealth, Angle, Size / 2, 1, 0, 0);

        if (SaveGame.DebugMode)
        {
            //Direction of motion
            _spriteBatch.Draw(Engine.Line, Position, new Rectangle((int)Position.X, (int)Position.Y, 10, 1), Color.LightBlue,
                MathF.Atan2(Velocity.Y, Velocity.X), Vector2.Zero, new Vector2(Velocity.Length(), 0.5f), SpriteEffects.None, 0.4f);
            //Direction the entity is pointing
            _spriteBatch.Draw(Engine.Line, Position, new Rectangle((int)Position.X, (int)Position.Y, 10, 1), Color.Red,
                Angle - MathF.PI / 2, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.4f);
        }
        collider.isEmitterActive = SaveGame.DebugMode;
    }
    public virtual Entity Clone() { throw new NotImplementedException(); }
}
