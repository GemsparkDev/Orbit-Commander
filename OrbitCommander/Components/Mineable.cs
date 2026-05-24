using Microsoft.Xna.Framework;
using OrbitCommander.Entities;
using OrbitCommander.Particles;
using System;
using OrbitCommander.Core;

namespace OrbitCommander.Components;
public class Mineable(Entity _entity) : Component()
{
    public float MineTime { get; set; } = 0;
    public void Mine(float _maxHealth)
    {
        if (_entity.Health <= 0)
        {
            MineTime += Engine.DeltaSeconds * 5 / _maxHealth;
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 0.5f, _entity.Position, new Vector2(Util.OneToNegOne(), Util.OneToNegOne()), Util.OneToNegOne() * MathF.PI, Util.OneToNegOne() / 2, Color.Yellow, Color.Transparent));
        }
    }
}