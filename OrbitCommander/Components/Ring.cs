using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrbitCommander.Entities;
using System;
using OrbitCommander.Core;

namespace OrbitCommander.Components;
public class Ring(Entity _entity) : Component
{
    public float Offset { get; set; }
    public float Mass { get; set; }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        int randomAngle = 3;
        float r = _entity.ColliderRadius + Offset;
        for (float i = 0; i < 2 * r; i++)
        {
            float j = i - r;
            float distance = r * 1.25f + j * j * j / (r * r * 2) + r / 2 + i / 2;
            float speed = MathF.Sqrt(Mass / distance) * 60;
            //Simple deterministic random number generator
            randomAngle = (randomAngle * 65535 + 997) % 628;
            float particleAngle = (i + (float)randomAngle / 628 + Engine.Time * speed / distance) % MathF.Tau;
            Vector2 particlePosition = new Vector2(MathF.Cos(particleAngle), MathF.Sin(particleAngle) * 0.25f) * distance;
            if (particlePosition.LengthSquared() > _entity.ColliderRadius * _entity.ColliderRadius || particlePosition.Y > 0)
            {
                _spriteBatch.Draw(Assets.Get(Sprites.Dot), _entity.Position + particlePosition, null, _entity.Color * 0.75f, particleAngle, Assets.DimsOf(Sprites.Dot), 1, 0, 0);
            }
        }
    }
}
