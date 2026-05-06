using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;
using Microsoft.Xna.Framework;

namespace Space_Wars.Content.Main.Components;
public class Ring(Entity _entity) : Component(_entity)
{
    public float Offset { get; set; }
    public float Mass { get; set; }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        int randomAngle = 3;
        float r = Entity.ColliderRadius + Offset;
        for (float i = 0; i < 2 * r; i++)
        {
            float j = i - r;
            float distance = r * 1.25f + j * j * j / (r * r * 2) + r / 2 + i / 2;
            float speed = MathF.Sqrt(Mass / distance) * 60;
            //Simple deterministic random number generator
            randomAngle = (randomAngle * 65535 + 997) % 628;
            float particleAngle = (i + (float)(randomAngle) / 628 + Engine.Time * speed / distance) % MathF.Tau;
            Vector2 particlePosition = new Vector2(MathF.Cos(particleAngle), MathF.Sin(particleAngle) * 0.25f) * distance;
            if (particlePosition.LengthSquared() > Entity.ColliderRadius * Entity.ColliderRadius || particlePosition.Y > 0)
            {
                _spriteBatch.Draw(Assets.Get(Sprites.Dot), Entity.Position + particlePosition, null, Entity.Color * 0.75f, particleAngle, Assets.DimsOf(Sprites.Dot), 1, 0, 0);
            }
        }
    }
}
