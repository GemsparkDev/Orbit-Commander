using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Space_Wars.Content.Main.Particles;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main.Components;
internal class Emitter(Entity _entity) : Component(_entity)
{
    public ParticleEmitter ParticleEmitter { get; set; }
    public override void Update()
    {
        ParticleEmitter.position = Entity.Position;
        ParticleEmitter.offsetVelocity = Entity.Velocity;
        ParticleEmitter.Update();
    }
}
