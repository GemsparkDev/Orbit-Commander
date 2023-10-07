
using Microsoft.Xna.Framework;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;
using Space_Wars.Content.Main.UI_Elements;

namespace Space_Wars.Content.Main
{
    public static class EventHandler
    {
        public static Player player;
        public static Mothership mothership;
        public static UIManager UIManager;
        public static Engine root;
        public static bool isTraining = false;
        public static void Initialize(Player _player, Mothership _mothership)
        {
            player = _player;
            mothership = _mothership;
        }
        public static void PairPlayerUIManager()
        {
            for (int i = 0; i < UIManager.moduleSlots.Length; i++)
            {
                ItemSlot slot = UIManager.moduleSlots[i];
                slot.daughterItem = player.modules[i];
                slot.daughterItem.parent = slot;
            }
            UIManager.selectedIcon = null;
        }
        public static void Exit()
        {
            root.Exit();
            SoundManager.PlayGlobalSound(Assets.SoundFX["Interact"]);
        }
        public static void Startgame()
        {
            root.Startgame();
            SoundManager.PlayGlobalSound(Assets.SoundFX["Interact"]);
        }
        public static void StartTraining()
        {
            ParticleManager.Initialize();
            EntityManager.Initialize(root);
            PairPlayerUIManager();
            EntityManager.trainingSimulator = new(EntityManager.player, root);
            CurrentGameState.SwitchState(new TrainingMode());
            SoundManager.PlayGlobalSound(Assets.SoundFX["Interact"]);
        }
        public static void QuitToMenu()
        {
            UIManager.GetContainer(Containers.PauseMenu).enabled = false;
            UIManager.ToggleMenu(Containers.MainMenu);
            ParticleManager.Initialize();
            SoundManager.SetAllSounds(false);
            SoundManager.Initialize();
            EntityManager.player.velocity = Vector2.Zero;
            Engine.ingameTime = new();
            SoundManager.PlayGlobalSound(Assets.SoundFX["Interact"]);
            Engine.camera.Position = Vector2.Zero;
            CurrentGameState.SwitchState(new MainMenu());
        }
        public static void RepairModule()
        {
            ItemSlot slot = UIManager.repairSlot;
            Module daughterModule;
            if (slot.daughterItem != null)
            {
                daughterModule = slot.daughterItem as Module;
            }
            else
            { 
                SoundManager.PlayGlobalSound(Assets.SoundFX["Fail"]);
                return;
            }
            if (daughterModule.health < 20 && EntityManager.player.mothership.scrap >= 3)
            {
                daughterModule.health = 20;
                EntityManager.player.mothership.scrap -= 3;
                SoundManager.PlayGlobalSound(Assets.SoundFX["Interact"]);
                UpdateRepairText();
                UIManager.mothershipScrap.text = EntityManager.player.mothership.scrap.ToString();
            }
            else
            {
                SoundManager.PlayGlobalSound(Assets.SoundFX["Fail"]);
            }
        }
        public static void UpdateRepairText()
        {
            ItemSlot slot = UIManager.repairSlot as ItemSlot;
            Module daughterModule = slot.daughterItem as Module;
            if (slot.daughterItem != null)
            {
                UIManager.repairText.text = daughterModule.health.ToString();
            }
            else
            {
                UIManager.repairText.text = "None";
            }
        }

        public static void UpdateInventoryUI()
        {
            for (int y = 0; y < UIManager.inventorySlots.GetLength(1); y++)
            {
                for (int x = 0; x < UIManager.inventorySlots.GetLength(0); x++)
                {
                    UIManager.inventorySlots[x, y].daughterItem = mothership.inventory[x, y];
                    if(UIManager.inventorySlots[x, y].daughterItem != null)
                    {
                        UIManager.inventorySlots[x, y].daughterItem.parent = UIManager.inventorySlots[x, y];
                    }
                }
            }
        }
        public static void UpdateInventory()
        {
            for (int y = 0; y < mothership.inventory.GetLength(1); y++)
            {
                for (int x = 0; x < mothership.inventory.GetLength(0); x++)
                {
                    mothership.inventory[x, y] = UIManager.inventorySlots[x, y].daughterItem;
                }
            }
        }
        public static void UpdateModulesUI()
        {
            for (int x = 0; x < UIManager.moduleSlots.Length; x++)
            {
                UIManager.moduleSlots[x].daughterItem = player.modules[x];
                if (UIManager.moduleSlots[x].daughterItem != null)
                {
                    UIManager.moduleSlots[x].daughterItem.parent = UIManager.moduleSlots[x];
                }
            }
        }
        public static void UpdateModules()
        {
            for (int x = 0; x < player.modules.Length; x++)
            {
                player.modules[x] = UIManager.moduleSlots[x].daughterItem as Module;
            }
            foreach (Module module in EntityManager.player.modules)
            {
                if (module == null)
                {
                    UIManager.validConfigText.text = "Invalid Configuration";
                    return;
                }
            }
            UIManager.validConfigText.text = "";
        }
        public static void UpdateFurnaceUI(float _value, float _maxValue)
        {
            UIManager.furnaceSlot.daughterItem = mothership.furnaceItem;
            if (UIManager.furnaceSlot.daughterItem != null)
            {
                UIManager.furnaceSlot.daughterItem.parent = UIManager.furnaceSlot;
            }
            UIManager.furnaceSlider.SetInterval(_value, _maxValue);
            UIManager.mothershipScrap.text = EntityManager.player.mothership.scrap.ToString();
        }
        public static void UpdateFurnace()
        {
            mothership.furnaceItem = UIManager.furnaceSlot.daughterItem;
        }
        public static void ReturnItemToParent()
        {
            UIManager.selectedIcon.parent.Interact(UIManager.focusedContainer.position);
        }
        public static void CraftItem()
        {
            if(mothership.scrap >= 5)
            {
                mothership.scrap -= 5;
                mothership.currentlyCrafting = true;
                SoundManager.PlayGlobalSound(Assets.SoundFX["Interact"]);
            }
            else
            {
                SoundManager.PlayGlobalSound(Assets.SoundFX["Fail"]);
            }
        }
        public static void UpdateCraftingUI(float _value, float _maxValue)
        {
            UIManager.craftingSlider.SetInterval(_value, _maxValue);
            UIManager.mothershipScrap.text = EntityManager.player.mothership.scrap.ToString();
            UIManager.requiredCraftsText.text = EntityManager.player.mothership.requiredCraftsLeft.ToString();
        }
        public static void UpdateEnemyCountdownUI(float _value, float _maxValue, float _wave)
        {
            UIManager.enemySlider.sliderInterval = _value / _maxValue;
            UIManager.waveText.text = $"{_wave}";
        }
        public static void GarageTrigger()
        {
            foreach(Module module in EntityManager.player.modules)
            {
                if(module == null)
                {
                    return;
                }
            }
            SoundManager.PlayGlobalSound(Assets.SoundFX["Interact"]);
            UIManager.ToggleMenu(UIManager.GarageMenu);
            UIManager.ToggleMenu(Containers.MothershipMenu);
            if (UIManager.GarageMenu.enabled == true)
            {
                CurrentGameState.SwitchState(new Garage());
            }
            else if (UIManager.GarageMenu.enabled == false)
            {
                if(isTraining == false)
                {
                    CurrentGameState.SwitchState(new PlayingGame());
                }
                else
                {
                    CurrentGameState.SwitchState(new TrainingMode());
                }
            }
        }
    }
}
