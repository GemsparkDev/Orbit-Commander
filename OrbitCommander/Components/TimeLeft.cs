using Microsoft.Xna.Framework;
using OrbitCommander.Entities;
using OrbitCommander.Particles;
using OrbitCommander.Core;

namespace OrbitCommander.Components;
internal class ExpireTimer(Entity _entity) : Component()
{
    public float TimeLeft { get; set; }
    public override void Update()
    {
        TimeLeft -= Engine.DeltaSeconds;
        if (TimeLeft <= 0)
        {
            TimeLeft = 0;
            Sprite sprite = _entity.GetComponent<Sprite>();
            Transform transform = _entity.GetComponent<Transform>();
            if (sprite != null && transform != null)
            {
                ParticleManager.Add(new Particle(sprite.Texture, 1, transform.Position, transform.Velocity, transform.Angle, 0, sprite.Color, Color.Transparent));
            }
            _entity.isExpired = true;
        }
    }
}
