using Microsoft.Xna.Framework;
using OrbitCommander.Entities;
using UILib.Content;
using OrbitCommander.Core;

namespace OrbitCommander.Components;
public class Dockable(Entity _entity, Container _menu, bool hasInventory = true) : Component()
{
    public bool IsDocked { get; private set; } = false;
    public Container Menu { get; private set; } = _menu;
    public Entity Entity => _entity;
    private static void AddItem(Pickup _pickup)
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
    public bool Dock(Player _player, bool _withVelocity)
    {
        Events.DisableDockingMenus();
        if (Vector2.DistanceSquared(_player.Position, _entity.Position) > 1250)
        {
            return false;
        }
        if (IsDocked)
        {
            if (_withVelocity)
            {
                _player.Velocity += new Vector2(0, -2);
            }
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Undock));
            var pickup = Engine.UIManager.MoveSelectedIcon() as Pickup;
            if (pickup != null)
            {
                Events.UpdateInventoryUI();
                pickup.isExpired = false;
                pickup.Position = _entity.Position;
                _player.leashedMaterials.Add(pickup);
                Engine.SaveGame.CurrentMission.Add(pickup);
            }
        }
        else
        {
            Events.ToggleDockingMenus();
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Dock));
            if (hasInventory)
            {
                for (int i = 0; i < _player.leashedMaterials.Count; i++)
                {
                    //Launches the leashed material away if the docking module cannot store it
                    _player.leashedMaterials[i].Velocity += Engine.SaveGame.CurrentMission.GetNormalizedAcceleration(_player.leashedMaterials[i].Position) * 15;
                    bool isFull = true;
                    for (int j = 0; j < Engine.SaveGame.Inventory.Length; j++)
                    {
                        if (Engine.SaveGame.Inventory[j] == null)
                        {
                            isFull = false;
                            break;
                        }
                    }
                    if (isFull)
                    {
                        continue;
                    }
                    AddItem(_player.leashedMaterials[i]);
                    _player.leashedMaterials[i].isExpired = true;
                }
                _player.leashedMaterials.Clear();
                Events.UpdateScrapText();
            }
        }
        IsDocked = !IsDocked;
        Engine.ShakeScreen(0.35f);
        return true;
    }
    public void Collide(int _damage)
    {
        _entity.Collide(_damage);
    }
}
