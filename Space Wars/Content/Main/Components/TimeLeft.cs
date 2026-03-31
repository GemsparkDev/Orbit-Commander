using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Space_Wars.Content.Main.Components;
internal class ExpireTimer(Entity _entity) : Component(_entity)
{
    public float TimeLeft { get; set; }
    public override void Update()
    {
        TimeLeft -= Engine.DeltaSeconds;
        if (TimeLeft <= 0)
        {
            Sprite sprite = Entity.GetComponent<Sprite>();
            Transform transform = Entity.GetComponent<Transform>();
            if (sprite != null && transform != null)
            {
                ParticleManager.Add(new Particle(sprite.Texture, 1, transform.Position, transform.Velocity, transform.Angle, 0, sprite.Color, Color.Transparent));
            }
            Entity.isExpired = true;
        }
    }
}
