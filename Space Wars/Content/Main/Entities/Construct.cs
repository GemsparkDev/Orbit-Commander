using Microsoft.Xna.Framework;
using System;

namespace Space_Wars.Content.Main.Entities;
public class Construct : Pickup
{
    private ConstructType type;
    private float cooldown = 0;
    public Construct(ItemData _itemData, Color _worldColor, Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, ConstructType _type) 
        : base(_itemData, _worldColor, _position, _velocity, _angularVelocity, ((_type == ConstructType.Barricade) ? 10 : 5))
    {
        angle = _angle;
        type = _type;
    }
    public override void Update()
    {
        velocity = Vector2.Zero;
        if (cooldown > 0)
        {
            cooldown -= Engine.DeltaSeconds;
        }
        switch (type)
        {
            case ConstructType.Barricade:
                break;
            case ConstructType.Trap:
                var nearestEnemy = Engine.EntityManager.NearestEnemy(new Enemy(position, Vector2.Zero, 0, 0, 0, null, true));
                if (cooldown <= 0 && nearestEnemy != null && Vector2.Distance(nearestEnemy.position, position) < 300)
                {
                    var dir = Vector2.Normalize(nearestEnemy.position - position);
                    float rot = MathF.PI * 2 / 9;
                    for (float i = 0; i < 9; i++)
                    {
                        float angle = MathF.Atan2(dir.X, -dir.Y);
                        Engine.EntityManager.Add(new PulseShot(position, dir * 10, angle, 0, true, 5, true));
                        dir = new Vector2(dir.X * MathF.Cos(rot) - dir.Y * MathF.Sin(rot), dir.X * MathF.Sin(rot) + dir.Y * MathF.Cos(rot));
                    }
                    SoundManager.PlaySound(Assets.Get(Sound.PulseFire), position);
                    cooldown = 1.5f;
                }
                break;
        }
        base.Update();
    }
}
public enum ConstructType
{
    Barricade,
    Trap,
}