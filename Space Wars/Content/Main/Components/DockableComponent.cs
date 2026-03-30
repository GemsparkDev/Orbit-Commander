using UILib.Content.Main;
using Microsoft.Xna.Framework;
using Space_Wars.Content.Main.Entities;
using System;

namespace Space_Wars.Content.Main.Components;


public class DockableComponent(Entity _parentEntity, Container _menu, bool hasInventory = true) : Component(_parentEntity, ComponentType.None)
{
    public bool IsDocked { get; private set; } = false;
    //public ComponentType Type => ComponentType.DockableComponent;
    public bool IsValid => !_parentEntity.isExpired;
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
        throw new NotImplementedException();
        EventHandler.DisableDockingMenus();
        if (EntityManager.DistanceSqr(_player, _parentEntity) > 1250)
        {
            return false;
        }
        if (IsDocked)
        {
            if (_withVelocity)
            {
                _player.velocity += new Vector2(0, -2);
            }
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Undock));
            Pickup pickup = Engine.MoveSelectedPickup();
            if (pickup != null)
            {
                EventHandler.UpdateInventoryUI();
                pickup.isExpired = false;
                //pickup.position = entity.Position;
                _player.leashedMaterials.Add(pickup);
                Engine.EntityManager.Add(pickup);
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
                    _player.leashedMaterials[i].velocity += Engine.SaveGame.CurrentMission.GetNormalizedAcceleration(_player.leashedMaterials[i].position) * 15;
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
        _parentEntity.Collide(_damage);
    }
}
