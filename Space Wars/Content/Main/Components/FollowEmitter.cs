using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;

namespace Space_Wars.Content.Main.Components;
internal class FollowEmitter(Entity _entity) : Component(_entity)
{
    public ParticleEmitter ParticleEmitter { get; set; }
    public bool IsDebug { get; set; } = true;
    public override void Update()
    {
        var comp = Entity.GetComponent<Health>();
        if (IsDebug && comp != null && comp.CurrentHealth > 0)
        {
            ParticleEmitter.isEmitterActive = SaveGame.DebugMode;
        }
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
public class RelativePlayerEmitter(Entity _entity) : Component(_entity)
{
    public ParticleEmitter ParticleEmitter { get; set; }
    public override void Update()
    {
        ParticleEmitter.offsetVelocity = -Engine.SaveGame.Player.Velocity;
        ParticleEmitter.Update();
        ParticleEmitter.position = Entity.Position;
    }
}
