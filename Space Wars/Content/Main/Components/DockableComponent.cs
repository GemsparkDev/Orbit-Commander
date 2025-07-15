using UILib.Content.Main;
using Microsoft.Xna.Framework;
using Space_Wars.Content.Main.Entities;
using Assimp;

namespace Space_Wars.Content.Main.Components;

public class DockableComponent(Entity _parentEntity, Containers _menu) : IComponent
{
    public Vector2 Position => _parentEntity.position;
    public Vector2 Velocity => _parentEntity.velocity;
    public bool IsDocked { get; private set; } = false;
    public ComponentType Type => ComponentType.DockableComponent;
    public bool IsValid => !_parentEntity.isExpired;
    public Containers Menu { get; private set; } = _menu;

    public void AddItem(Pickup _pickup)
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
    public bool IsFull()
    {
        for (int i = 0; i < Engine.SaveGame.Inventory.Length; i++)
        {
            if (Engine.SaveGame.Inventory[i] == null)
            {
                return false;
            }
        }
        return true;
    }
    public bool Dock(Player _player)
    {
        EventHandler.DisableDockingMenus();
        if (EntityManager.DistanceSqr(_player, _parentEntity) > 1250)
        {
            return false;
        }
        if (IsDocked)
        {
            _player.velocity += new Vector2(0, -2);
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Undock));
            Pickup pickup = Engine.MoveSelectedPickup();
            if (pickup != null)
            {
                EventHandler.UpdateInventoryUI();
                pickup.isExpired = false;
                pickup.position = Position;
                _player.leashedMaterials.Add(pickup);
                Engine.EntityManager.Add(pickup);
            }
        }
        else
        {
            EventHandler.ToggleDockingMenus();
            for (int i = 0; i < _player.leashedMaterials.Count; i++)
            {
                //Launches the leashed material away if the docking module cannot store it
                _player.leashedMaterials[i].velocity += Engine.SaveGame.CurrentMission.GetNormalizedAcceleration(_player.leashedMaterials[i].position) * 15;
                if (IsFull())
                {
                    continue;
                }
                AddItem(_player.leashedMaterials[i]);
                _player.leashedMaterials[i].isExpired = true;
            }
            _player.leashedMaterials.Clear();
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Dock));
            EventHandler.UpdateScrapText();
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
