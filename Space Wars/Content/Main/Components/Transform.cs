using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Space_Wars.Content.Main.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Space_Wars.Content.Main.Components;
internal class Transform(Entity _entity) : Component(_entity)
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Velocity { get; set; } = Vector2.Zero;
    public float Angle { get; set; } = 0;
    public float AngularVelocity { get; set; } = 0;
    public override void Update()
    {
        Position += Velocity * Engine.DeltaSeconds * 60;
        Angle += AngularVelocity * Engine.DeltaSeconds * 60;
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        if (SaveGame.DebugMode)
        {
            //Direction of motion
            _spriteBatch.Draw(Engine.Line, Position, new Rectangle((int)Position.X, (int)Position.Y, 10, 1), Color.LightBlue,
                MathF.Atan2(Velocity.Y, Velocity.X), Vector2.Zero, new Vector2(Velocity.Length(), 0.5f), SpriteEffects.None, 0.4f);
            //Direction the entity is pointing
            _spriteBatch.Draw(Engine.Line, Position, new Rectangle((int)Position.X, (int)Position.Y, 10, 1), Color.Red,
                Angle - MathF.PI / 2, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.4f);
        }
    }
}
