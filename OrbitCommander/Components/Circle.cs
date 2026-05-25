using Microsoft.Xna.Framework;
using OrbitCommander.Core;
using OrbitCommander.Particles;
using System;
using OrbitCommander.Entities;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitCommander.Components;
public class Circle(Entity _entity, float radius) : IComponent
{
    public void Draw(SpriteBatch _spriteBatch)
    {
        if(radius <= 0)
        {
            return;
        }
        Vector2 normalVector;
        float increment = MathF.Tau / radius / 0.75f;
        int count = (int)Math.Ceiling(Math.Truncate(MathF.Tau / increment));
        for (float i = 0; i < count; i++)
        {
            float angle = i / count * MathF.Tau;
            normalVector = Util.ToUnitVector(angle) * radius;
            _spriteBatch.Draw(Assets.Get(Sprites.Dot), _entity.Position + normalVector, null, _entity.Color, angle, Assets.DimsOf(Sprites.Dot), 1, 0, 0);
        }
    }
}
