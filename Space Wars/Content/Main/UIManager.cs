using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.UI_Elements;
using System.Collections.Generic;
using System.Linq;

namespace Space_Wars.Content.Main
{
    public enum Containers
    {
        PauseMenu = 0,
        MainMenu = 1,
        PlayerMenu = 2,
        MothershipMenu = 3,
        GarageMenu = 4,
    }
    public delegate void DelegateMethod();
    public class UIManager
    {
        public static bool lockMouseInput = false;
        private MouseState oldState;
        private static readonly List<Container> containers = new();
        public Container focusedContainer;
        public Container PauseMenu { get; } = new Window(Engine.screenSize / 2, Assets.Sprites["Large Panel"], 0.8f) { enabled = false };
        public Container MainMenu { get; } = new Window(Engine.screenSize / 2, Assets.Sprites["Large Panel"], 0.8f) { enabled = true };
        public Container PlayerMenu { get; } = new Window(Engine.screenSize / 2, Assets.Sprites["Large Panel"], 0.8f) { enabled = false };
        public Container MothershipMenu { get; } = new TabbedWindow(Engine.screenSize / 2, Assets.Sprites["Large Panel"], 2, 0.8f) { enabled = false };
        public Container GarageMenu { get; } = new Window(Engine.screenSize / 2, Assets.Sprites["Large Panel"], 0.8f) { enabled = false };
        public Button exitButton;
        public Button singleplayerButton;
        public Button quitToMenuButton;
        public Button garageButton;
        public Button craftButton;
        public Button optionsButton;
        public Button repairButton;
        public ItemSlot repairSlot;
        public ItemSlot smeltingSlot;
        public ItemSlot furnaceSlot;
        public ItemSlot craftingSlot;
        public Slider furnaceSlider;
        public Slider craftingSlider;
        public Decal mothershipScrap;
        public Decal repairText;
        public Decal smeltingText;
        public Item selectedIcon;
        public ItemSlot[] moduleSlots = new ItemSlot[5]; 
        public ItemSlot[,] inventorySlots = new ItemSlot[1, 3];

        public UIManager()
        {

            containers.Add(PauseMenu);
            containers.Add(MainMenu);
            containers.Add(PlayerMenu);
            containers.Add(MothershipMenu);
            containers.Add(GarageMenu);
            exitButton = new Button(new Vector2(MainMenu.Size.X / 2, MainMenu.Size.Y * 3 / 4), Assets.Sprites["Button"], "Exit", Color.White);
            singleplayerButton = new Button(new Vector2(MainMenu.Size.X / 2, MainMenu.Size.Y / 4), Assets.Sprites["Button"], "Singleplayer", Color.White);
            quitToMenuButton = new Button(MainMenu.Size/2, Assets.Sprites["Button"], "Quit to Menu", Color.White);
            garageButton = new Button(new Vector2(MainMenu.Size.X / 2, MainMenu.Size.Y / 4), Assets.Sprites["Button"], "To Garage", Color.White);
            repairButton = new Button(new Vector2(MainMenu.Size.X / 2 - Assets.Sprites["Button"].Width/2, MainMenu.Size.Y / 4), Assets.Sprites["Button"], "Repair", Color.LightBlue);
            craftButton = new Button(new Vector2(MainMenu.Size.X / 2, MainMenu.Size.Y * 3 / 4), Assets.Sprites["Button"], "Craft", Color.LightBlue);
            mothershipScrap = new(new Vector2(MainMenu.Size.X / 2 + 48, 12), "0", Color.Gray);
            repairText = new(new Vector2(MainMenu.Size.X / 2 - Assets.Sprites["Button"].Width / 2, 90), "None", Color.White);
            repairSlot = new ItemSlot(new Vector2(MainMenu.Size.X / 2 - Assets.Sprites["Button"].Width / 2, 63), Assets.Sprites["Empty Slot"], this, -1, true);
            smeltingSlot = new ItemSlot(new Vector2(MainMenu.Size.X / 2 - Assets.Sprites["Button"].Width / 2, 63), Assets.Sprites["Empty Slot"], this, -1, true);
            furnaceSlider = new Slider(new Vector2(MainMenu.Size.X / 3 - 30, 30), 60, true, Color.White, Color.Gray);
            furnaceSlot = new ItemSlot(new Vector2(MainMenu.Size.X / 3, 63), Assets.Sprites["Empty Slot"], this, -1, false);
            craftingSlider = new Slider(new Vector2(MainMenu.Size.X / 2 - 30, 30), 60, true, Color.White, Color.Gray);
            craftingSlot = new ItemSlot(new Vector2(MainMenu.Size.X / 2, 63), Assets.Sprites["Empty Slot"], this, -1, false);

            for (int x = 0; x < moduleSlots.GetLength(0); x++)
            {
                if(x % 2 == 0)
                {
                    moduleSlots[x] = new ItemSlot(new Vector2((MainMenu.Size.X * 2 / 3),
                        (Assets.Sprites["Empty Slot"].Height + 1) * x/2 + Assets.Sprites["Empty Slot"].Height), Assets.Sprites["Empty Slot"], this, x+1, true);
                }
                else
                {
                    moduleSlots[x] = new ItemSlot(new Vector2((MainMenu.Size.X * 2 / 3 + Assets.Sprites["Empty Slot"].Width / 2),
                        (Assets.Sprites["Empty Slot"].Height + 1) * x/2 + Assets.Sprites["Empty Slot"].Height), Assets.Sprites["Empty Slot"], this, x+1, true);
                }
                GarageMenu.AddWidget(moduleSlots[x] as IFunctional);
                moduleSlots[x].AddBehaviour(new DelegateMethod(EventHandler.UpdateModules));
            }
            for (int x = 0; x < inventorySlots.GetLength(0); x++)
            {
                for (int y = 0; y < inventorySlots.GetLength(1); y++)
                {
                    inventorySlots[x, y] = new ItemSlot(new Vector2((Assets.Sprites["Empty Slot"].Width + 1) * x + Assets.Sprites["Large Panel"].Width / 1.33f,
                        (Assets.Sprites["Empty Slot"].Height + 1) * y + Assets.Sprites["Empty Slot"].Height), Assets.Sprites["Empty Slot"], this, -1, false);
                    MothershipMenu.AddWidget(inventorySlots[x, y] as IFunctional, 1);
                    inventorySlots[x, y].AddBehaviour(new DelegateMethod(EventHandler.UpdateInventory));
                }
            }

            MainMenu.AddWidget(exitButton as IFunctional);
            MainMenu.AddWidget(singleplayerButton as IFunctional);
            PauseMenu.AddWidget(quitToMenuButton as IFunctional);
            GarageMenu.AddWidget(mothershipScrap as Widget);
            GarageMenu.AddWidget(repairButton as IFunctional);
            GarageMenu.AddWidget(repairSlot as IFunctional);
            GarageMenu.AddWidget(repairText as Widget);
            MothershipMenu.AddWidget(garageButton as IFunctional, 0);
            MothershipMenu.AddWidget(furnaceSlider as IFunctional, 1);
            MothershipMenu.AddWidget(furnaceSlot as IFunctional, 1);
            MothershipMenu.AddWidget(craftingSlider as IFunctional, 2);
            MothershipMenu.AddWidget(craftingSlot as IFunctional, 2);
            MothershipMenu.AddWidget(craftButton as IFunctional, 2);
            exitButton.AddBehaviour(new DelegateMethod(EventHandler.Exit));
            singleplayerButton.AddBehaviour(new DelegateMethod(EventHandler.Startgame));
            quitToMenuButton.AddBehaviour(new DelegateMethod(EventHandler.QuitToMenu));
            garageButton.AddBehaviour(new DelegateMethod(EventHandler.GarageTrigger));
            repairButton.AddBehaviour(new DelegateMethod(EventHandler.RepairModule));
            craftButton.AddBehaviour(new DelegateMethod(EventHandler.CraftItem));
            repairSlot.AddBehaviour(new DelegateMethod(EventHandler.UpdateRepairText));
            furnaceSlot.AddBehaviour(new DelegateMethod(EventHandler.UpdateFurnace));
        }
        public static void ToggleMenu(Container container)
        {
            container.enabled = !container.enabled;
        }
        public static void ToggleMenu(Containers containerType)
        {
            Container container = GetContainer(containerType);
            container.enabled = !container.enabled;
        }
        public bool PauseMenuTrigger()
        {
            foreach (Container container in containers)
            {
                if (container.enabled == true && container != PauseMenu)
                {
                    ToggleMenu(container);
                    return false;
                }
            }
            if (PauseMenu.enabled == true)
            {
                SoundManager.PlayGlobalSound(Assets.SoundFX["Close Menu"]);
            }
            else
            {
                SoundManager.PlayGlobalSound(Assets.SoundFX["Open Menu"]);
            }
            ToggleMenu(PauseMenu);
            return true;
        }

        public void Update()
        {
            focusedContainer = containers.Where(c => c.enabled && c.GetMouseOver())
                                          .FirstOrDefault() ?? new DummyWindow();
            focusedContainer ??= new DummyWindow();

            MouseState newState = Mouse.GetState();
            if (oldState.LeftButton == ButtonState.Released && newState.LeftButton == ButtonState.Pressed)
            {
                focusedContainer.GetWidgetOver().Interact(focusedContainer.position);
                if(focusedContainer is not DummyWindow)
                {
                    lockMouseInput = true;
                }
            }
            if (newState.LeftButton == ButtonState.Pressed)
            {
                focusedContainer.GetWidgetOver().ContinuousInteract(focusedContainer.position);
            }
            else if (oldState.LeftButton == ButtonState.Pressed && newState.LeftButton == ButtonState.Released)
            {
                if(selectedIcon != null)
                {
                    IFunctional widget = focusedContainer.GetWidgetOver();
                    if (widget is ItemSlot && ((widget as ItemSlot).id == selectedIcon.id || (widget as ItemSlot).id == -1))
                    {
                        widget.Interact(focusedContainer.position);
                    }
                    else
                    {
                        EventHandler.ReturnItemToParent();
                    }
                }
                lockMouseInput = false;
            }
            oldState = newState;
        }

        public void AddContainer(Container container)
        {
            containers.Add(container);
        }

        public static Container GetContainer(Containers container)
        {
            return containers[(int)container];
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Container container in containers.Where(c => c.enabled))
            {
                spriteBatch.Draw(container.texture, container.position, null, Color.White * container.transparency, 0, Vector2.One / 2, Engine.UIScale, SpriteEffects.None, 0.35f);
                container.Draw(spriteBatch);
            }
            if (selectedIcon != null)
            {
                spriteBatch.Draw(selectedIcon.texture, new Vector2(Mouse.GetState().X - selectedIcon.texture.Width / 2, Mouse.GetState().Y - selectedIcon.texture.Height / 2), null, Color.White, 0, Vector2.One / 2, Engine.UIScale, SpriteEffects.None, 0.35f);
            }
        }
    }
}
