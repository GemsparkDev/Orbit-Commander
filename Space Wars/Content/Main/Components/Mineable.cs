using Microsoft.Xna.Framework;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;
using System;

namespace Space_Wars.Content.Main.Components;
public class Mineable(Entity _entity) : Component(_entity)
{
    public float MineTime { get; set; } = 0;
    public void Mine(float _maxHealth)
    {
        if (Entity.Health <= 0)
        {
            MineTime += Engine.DeltaSeconds * 5 / _maxHealth;
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 0.5f, Entity.Position, new Vector2(Util.OneToNegOne(), Util.OneToNegOne()), Util.OneToNegOne() * MathF.PI, Util.OneToNegOne() / 2, Color.Yellow, Color.Transparent));
        }
    }
}