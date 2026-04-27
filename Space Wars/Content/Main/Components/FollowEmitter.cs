using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;

namespace Space_Wars.Content.Main.Components;
internal class FollowEmitter(Entity _entity) : Component(_entity)
{
    public ParticleEmitter ParticleEmitter { get; set; }
    public override void Update()
    {
        ParticleEmitter.position = Entity.Position;
        ParticleEmitter.offsetVelocity = Entity.Velocity;
        ParticleEmitter.Update();
    }
}
internal class StationaryEmitter(Entity _entity) : Component(_entity)
{
    public ParticleEmitter ParticleEmitter { get; set; }
    public override void Update()
    {
        ParticleEmitter.position = Entity.Position;
        ParticleEmitter.Update();
    }
}
