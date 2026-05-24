using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrbitCommander.Entities;
using OrbitCommander.Components;
using OrbitCommander.Core;

namespace OrbitCommander.Components;
internal class Lock(Entity _entity) : Component()
{
    public Entity Key { get; set; }
    public Vector2 TargetLocation { get; set; } = Vector2.Zero;
    public override void Update()
    {
        if (Vector2.Distance(Key.Position, _entity.Position + TargetLocation) < Key.ColliderRadius)
        {
            _entity.isExpired = true;
            Key.isExpired = true;
        }
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        _spriteBatch.Draw(Assets.Get(Sprites.Circle), _entity.Position + TargetLocation, Color.White);
    }
}
