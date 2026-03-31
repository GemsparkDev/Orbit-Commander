using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Space_Wars.Content.Main.Entities;
using Microsoft.Xna.Framework;

namespace Space_Wars.Content.Main.Components;
internal class Transform(Entity _entity) : Component(_entity)
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Velocity { get; set; } = Vector2.Zero;
    public float Angle { get; set; } = 0;
    public float AngularVelocity { get; set; } = 0;
}
