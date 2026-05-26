using Microsoft.Xna.Framework;
using OrbitCommander.Entities;
using UILib.Content;
using OrbitCommander.Core;

namespace OrbitCommander.Components;
public class Dockable(Entity _entity, Container _menu, bool _hasInventory = true) : IComponent
{
    public Entity Entity => _entity;
    public Container Menu => _menu;
    public bool HasInventory => _hasInventory;
    public static void AddItem(Pickup _pickup)
    {
        for (int i = 0; i < Engine.SaveGame.Inventory.Length; i++)
        {
            if (Engine.SaveGame.Inventory[i] != null)
            {
                continue;
            }
            Engine.SaveGame.Inventory[i] = _pickup;
            Events.UpdateInventoryUI();
            return;
        }
    }
    public void Collide(int _damage)
    {
        _entity.Collide(_damage);
    }
}
