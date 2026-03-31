using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Space_Wars.Content.Main.Entities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Space_Wars.Content.Main.Components;
public class Sprite(Entity _entity) : Component(_entity)
{
    public Texture2D Texture { get; set; }
    public Color Color { get; set; } = Color.White;
    public virtual float ColliderRadius
    {
        get { return Texture == null ? 0 : SaveGame.EnemyHitboxModifier * (Texture.Height + Texture.Width) / 4 + 1; }
    }
    public Vector2 Size
    {
        get { return Texture == null ? Vector2.Zero : new Vector2(Texture.Width, Texture.Height); }
    }
}
