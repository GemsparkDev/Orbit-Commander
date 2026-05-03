using Microsoft.Xna.Framework;
using Space_Wars.Content.Main.Entities;
using UILib.Content.Main;

namespace Space_Wars.Content.Main.Components;


public class Dockable(Entity _parentEntity, Container _menu, bool hasInventory = true) : Component(_parentEntity)
{
    public bool IsDocked { get; private set; } = false;
    public Container Menu { get; private set; } = _menu;
    private static void AddItem(Pickup _pickup)
    {
        for (int i = 0; i < Engine.SaveGame.Inventory.Length; i++)
        {
            if (Engine.SaveGame.Inventory[i] != null)
            {
                continue;
            }
            Engine.SaveGame.Inventory[i] = _pickup;
            EventHandler.UpdateInventoryUI();
            return;
        }
    }
    public bool Dock(Player _player, bool _withVelocity)
    {
        EventHandler.DisableDockingMenus();
        if (Vector2.DistanceSquared(_player.Position, Entity.Position) > 1250)
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
            Pickup pickup = Engine.UIManager.MoveSelectedIcon() as Pickup;
            if (pickup != null)
            {
                EventHandler.UpdateInventoryUI();
                pickup.isExpired = false;
                pickup.Position = Entity.Position;
                _player.leashedMaterials.Add(pickup);
                Engine.SaveGame.CurrentMission.Add(pickup);
            }
        }
        else
        {
            EventHandler.ToggleDockingMenus();
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
                EventHandler.UpdateScrapText();
            }
        }
        IsDocked = !IsDocked;
        Engine.ShakeScreen(0.35f);
        return true;
    }
    public void Collide(int _damage)
    {
        Entity.Collide(_damage);
    }
}
