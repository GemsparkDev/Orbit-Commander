using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Space_Wars.Content.Main.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Space_Wars.Content.Main.Components;
internal class Lock(Entity _entity) : Component(_entity)
{
    public Entity Key { get; set; }
    public Vector2 TargetLocation { get; set; } = Vector2.Zero;
    public override void Update()
    {
        if(Vector2.Distance(Key.Position, Entity.Position + TargetLocation) < Key.ColliderRadius)
        {
            Entity.isExpired = true;
            Key.isExpired = true;
        }
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        _spriteBatch.Draw(Assets.Get(Sprites.Circle), Entity.Position + TargetLocation, Color.White);
    }
}
