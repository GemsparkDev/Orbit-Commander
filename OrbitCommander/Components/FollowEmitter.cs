using OrbitCommander.Entities;
using OrbitCommander.Particles;
using OrbitCommander.Core;

namespace OrbitCommander.Components;
internal class FollowEmitter(Entity _entity) : Component()
{
    public ParticleEmitter ParticleEmitter { get; set; }
    public bool IsDebug { get; set; } = true;
    public override void Update()
    {
        var comp = _entity.GetComponent<Health>();
        if (IsDebug && comp != null && comp.CurrentHealth > 0)
        {
            ParticleEmitter.isEmitterActive = SaveGame.DebugMode;
        }
        ParticleEmitter.position = _entity.Position;
        ParticleEmitter.offsetVelocity = _entity.Velocity;
        ParticleEmitter.Update();
    }
}
internal class StationaryEmitter(Entity _entity) : Component()
{
    public ParticleEmitter ParticleEmitter { get; set; }
    public override void Update()
    {
        ParticleEmitter.position = _entity.Position;
        ParticleEmitter.Update();
    }
}
