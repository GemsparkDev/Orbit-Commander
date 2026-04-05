using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Wars.Content.Main.Components;
public class Sprite(Entity _entity) : Component(_entity)
{
    private Texture2D texture;
    private ParticleEmitter collider;
    public Texture2D Texture { 
        get { return texture; } 
        set 
        {
            texture = value;
            collider = new ParticleEmitter(Assets.Get(Sprites.Dot), Entity.Position, Entity.ColliderRadius, Color.Yellow) { isEmitterActive = false };
        } }
    public Color Color { get; set; } = Color.White;
    public virtual float ColliderRadius
    {
        get { return Texture == null ? 0 : SaveGame.EnemyHitboxModifier * (Texture.Height + Texture.Width) / 4 + 1; }
    }
    public float RevealDuration { get; set; } = 0;
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
        if(RevealDuration > 0)
        {
            RevealDuration -= Engine.DeltaSeconds;
        }
    }
}
