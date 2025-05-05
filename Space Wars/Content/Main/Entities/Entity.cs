using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using Space_Wars.Content.Main.Components;
using System.Diagnostics;

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
    public bool isExpired;
    public bool isFriendly;
    public int damage;
    public virtual int SensingAbility { get; protected set; } = 1;
    public virtual int StealthAbility { get; protected set; } = 0;
    public EntityType entityType;
    public ComponentList Components { get; } = new();
    public virtual float ColliderRadius
    {
        get { return (texture.Height + texture.Width) / 4 + 1; }
    }
    public Vector2 Size
    {
        get { return texture == null ? Vector2.Zero : new Vector2(texture.Width, texture.Height); }
    }

    public abstract void Update();
    public abstract void Collide(int _damage);

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
    public virtual void Draw(SpriteBatch _spriteBatch)
    {
        Color stealthColor = color;
        float maxDistance = EntityManager.StealthRange;
        //Player has superior sensing to stealth -> full detection
        //Player has equal sensing to stealth -> partial detection when nearby
        //Player has inferior sensing to stealth -> no detection
        if (EntityManager.Player.SensingAbility == StealthAbility)
        {
            float distanceSqr = EntityManager.DistanceSqr(EntityManager.Player, this);
            if ((distanceSqr > maxDistance * maxDistance))
            {
                stealthColor *= 0;
            }
            else
            {
                stealthColor *= MathF.Sqrt(maxDistance - MathF.Sqrt(distanceSqr)) / MathF.Sqrt(maxDistance);
            }
        }
        else if(EntityManager.Player.SensingAbility < StealthAbility)
        {
            stealthColor *= 0;
        }
        _spriteBatch.Draw(texture, position - Engine.mousePositionOffset, null, stealthColor, angle, Size / 2, 1, 0, 0);

        if (Engine.DebugMode == true)
        {
            //Draws a line in the direction of motion for X
            _spriteBatch.Draw(Engine.Line, position - Engine.mousePositionOffset, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.White,
                MathF.Atan2(0, velocity.X), Vector2.Zero, new Vector2(MathF.Abs(velocity.X), 1), SpriteEffects.None, 0.4f);
            //Draws a line in the direction of motion for Y
            _spriteBatch.Draw(Engine.Line, position - Engine.mousePositionOffset, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.White,
                MathF.Atan2(velocity.Y, 0), Vector2.Zero, new Vector2(MathF.Abs(velocity.Y), 1), SpriteEffects.None, 0.4f);
            //Draws a line in the direction the entity is pointing
            _spriteBatch.Draw(Engine.Line, position - Engine.mousePositionOffset, new Rectangle((int)position.X, (int)position.Y, 10, 1), Color.Red,
                angle - MathF.PI / 2, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.4f);
        }
    }
    public void SimpleDraw(SpriteBatch _spriteBatch)
    {
        //Simplified render, only draws the entity texture
        //Used during cutscenes
        _spriteBatch.Draw(texture, position - Engine.mousePositionOffset, null, color, angle, Size / 2, 1, 0, 0);
    }
    public virtual Entity Clone() { throw new NotImplementedException(); }
}
