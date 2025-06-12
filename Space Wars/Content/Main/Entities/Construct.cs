using Microsoft.Xna.Framework;

namespace Space_Wars.Content.Main.Entities;
public class Construct : Pickup
{
    private ConstructType type;
    private float cooldown = 0;
    public Construct(ItemData _itemData, Color _worldColor, Vector2 _position, Vector2 _velocity, float _angle, float _angularVelocity, ConstructType _type) 
        : base(_itemData, _worldColor, _position, _velocity, _angularVelocity)
    {
        angle = _angle;
        type = _type;
    }
    public override void Update()
    {
        switch (type)
        {
            case ConstructType.Barricade:
                break;
            case ConstructType.Trap:
                var nearestEnemy = Engine.EntityManager.NearestEnemy(new Enemy(position, Vector2.Zero, 0, 0, 0, null, true));
                if (cooldown <= 0 && Vector2.Distance(nearestEnemy.position, position) < 500)
                {
                    cooldown = 2;
                }
                if (cooldown > 0)
                {
                    cooldown -= Engine.DeltaSeconds;
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