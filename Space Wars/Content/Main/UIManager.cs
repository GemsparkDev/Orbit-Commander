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
        public Container MainMenu { get; } = new TabbedWindow(Engine.screenSize / 2, Assets.Sprites["Large Panel"], 2, 1) { enabled = true };
        public Container PauseMenu { get; } = new Window(Engine.screenSize / 2, Assets.Sprites["Large Panel"], 1) { enabled = false };
        public Container PlayerMenu { get; } = new Window(Engine.screenSize / 2, Assets.Sprites["Large Panel"], 1) { enabled = false };
        public Container MothershipMenu { get; } = new TabbedWindow(Engine.screenSize / 2, Assets.Sprites["Large Panel"], 3, 1) { enabled = false };
        public Container GarageMenu { get; } = new Window(Engine.screenSize/2, Assets.Sprites["Gargantuan Panel"], 1) { enabled = false };
        public Button exitButton;
        public Button singleplayerButton;
        public Button trainingButton;
        public Button quitToMenuButton;
        public Button garageButton;
        public Button craftButton;
        public Button optionsButton;
        public Button repairButton;
        public ItemSlot repairSlot;
        public ItemSlot smeltingSlot;
        public ItemSlot furnaceSlot;
        public Slider furnaceSlider;
        public Slider craftingSlider;
        public Slider enemySlider;
        public Decal mothershipScrap;
        public Decal repairText;
        public Decal requiredCraftsText;
        public Decal smeltingText;
        public Decal garagePlayerImage;
        public Decal validConfigText;
        public Decal waveText;
        public Decal titleName;
        public Item selectedIcon;
        public ItemSlot[] moduleSlots = new ItemSlot[5]; 
        public ItemSlot[,] inventorySlots = new ItemSlot[1, 4];

        public UIManager()
        {
            List<Texture2D> iconGroup1 = new() { Assets.Sprites["Play Icon"], Assets.Sprites["Settings Icon"] };
            List<Texture2D> iconGroup2 = new() { Assets.Sprites["Smelt Icon"], Assets.Sprites["Repair Icon"], Assets.Sprites["Victory Icon"] };
            TabbedWindow menu = MainMenu as TabbedWindow;
            menu.icons = iconGroup1;
            menu = MothershipMenu as TabbedWindow;
            menu.icons = iconGroup2;

            containers.Add(PauseMenu);
            containers.Add(MainMenu);
            containers.Add(PlayerMenu);
            containers.Add(MothershipMenu);
            containers.Add(GarageMenu);

            singleplayerButton = new Button(new Vector2(MainMenu.Size.X / 2, MainMenu.Size.Y / 4), Assets.Sprites["Wide Button"], "Singleplayer", Color.White);
            trainingButton = new Button(new Vector2(MainMenu.Size.X / 2, MainMenu.Size.Y * 2 / 4), Assets.Sprites["Wide Button"], "Training", Color.White);
            exitButton = new Button(new Vector2(MainMenu.Size.X / 2, MainMenu.Size.Y * 3 / 4), Assets.Sprites["Wide Button"], "Exit", Color.White);
            titleName = new Decal(new Vector2(MainMenu.Size.X / 2, -MainMenu.Size.Y) - new Vector2(Assets.Sprites["Title Logo"].Width, Assets.Sprites["Title Logo"].Height)/2, Assets.Sprites["Title Logo"]);

            quitToMenuButton = new Button(PauseMenu.Size/2, Assets.Sprites["Wide Button"], "Quit to Menu", Color.White);

            smeltingSlot = new ItemSlot(new Vector2(MainMenu.Size.X / 2 - Assets.Sprites["Button"].Width / 2, 63), Assets.Sprites["Empty Slot"], this, -1, true);
            furnaceSlider = new Slider(new Vector2(MainMenu.Size.X / 3 - 30, 30), 60, true, Color.White, Color.Gray);
            furnaceSlot = new ItemSlot(new Vector2(MainMenu.Size.X / 3, 63), Assets.Sprites["Empty Slot"], this, -1, false);
            garageButton = new Button(new Vector2(MainMenu.Size.X / 2, MainMenu.Size.Y / 4), Assets.Sprites["Wide Button"], "To Garage", Color.White);
            craftButton = new Button(new Vector2(MainMenu.Size.X / 2, MainMenu.Size.Y * 3 / 4), Assets.Sprites["Button"], "Repair", Color.LightBlue);
            requiredCraftsText = new(MainMenu.Size/2 + new Vector2(0, -6), "25", Color.White);
            craftingSlider = new Slider(new Vector2(MainMenu.Size.X / 2 - 30, MainMenu.Size.Y / 4), 60, true, Color.White, Color.Gray);

            repairSlot = new ItemSlot(new Vector2(GarageMenu.Size.X / 4 - 25, GarageMenu.Size.Y / 2), Assets.Sprites["Empty Slot"], this, -1, true);
            mothershipScrap = new(new Vector2(GarageMenu.Size.X / 2.2f, 20), "0", Color.Gray);
            repairText = new(new Vector2(GarageMenu.Size.X / 4 - 60 / 2.5f, GarageMenu.Size.Y / 2 + 40), "None", Color.White);
            garagePlayerImage = new Decal(GarageMenu.Size/2 + new Vector2(GarageMenu.Size.X/4, 0) - new Vector2(Assets.Sprites["Player UI"].Width, Assets.Sprites["Player UI"].Height)/2, Assets.Sprites["Player UI"], null, Color.White);
            validConfigText = new Decal(GarageMenu.Size / 4 + new Vector2(20, GarageMenu.Size.Y/1.5f), "", Color.Red);
            repairButton = new Button(new Vector2(GarageMenu.Size.X / 4 - 25, GarageMenu.Size.Y / 2 - 40), Assets.Sprites["Button"], "Repair", Color.LightBlue);

            enemySlider = new Slider(new Vector2(PlayerMenu.Size.X / 3 - 30, 30), PlayerMenu.Size.X / 1.25f, true, Color.White, Color.Gray);
            waveText = new Decal(new Vector2(PlayerMenu.Size.X / 3 - 30, 42), "0", Color.White);

            for (int x = 0; x < moduleSlots.GetLength(0); x++)
            {
                if(x % 2 == 0)
                {
                    moduleSlots[x] = new ItemSlot(new Vector2((GarageMenu.Size.X / 2.75f),
                        (Assets.Sprites["Empty Slot"].Height) * x/2 + GarageMenu.Size.Y / 2 - Assets.Sprites["Empty Slot"].Height), Assets.Sprites["Empty Slot"], this, x+1, true);
                }
                else
                {
                    moduleSlots[x] = new ItemSlot(new Vector2((GarageMenu.Size.X / 2.75f + Assets.Sprites["Empty Slot"].Width / 1.4142f),
                        (Assets.Sprites["Empty Slot"].Height) * x/2 + GarageMenu.Size.Y/2 - Assets.Sprites["Empty Slot"].Height), Assets.Sprites["Empty Slot"], this, x+1, true);
                }
                GarageMenu.AddWidget(moduleSlots[x] as IFunctional);
                moduleSlots[x].AddBehaviour(new DelegateMethod(EventHandler.UpdateModules));
            }
            for (int x = 0; x < inventorySlots.GetLength(0); x++)
            {
                for (int y = 0; y < inventorySlots.GetLength(1); y++)
                {
                    inventorySlots[x, y] = new ItemSlot(new Vector2((Assets.Sprites["Empty Slot"].Width) * x + Assets.Sprites["Large Panel"].Width / 1.33f,
                        (Assets.Sprites["Empty Slot"].Height) * y + Assets.Sprites["Empty Slot"].Height), Assets.Sprites["Empty Slot"], this, -1, false);
                    MothershipMenu.AddWidget(inventorySlots[x, y] as IFunctional, 0);
                    inventorySlots[x, y].AddBehaviour(new DelegateMethod(EventHandler.UpdateInventory));
                }
            }

            MainMenu.AddWidget(exitButton as IFunctional, 0);
            MainMenu.AddWidget(singleplayerButton as IFunctional, 0);
            MainMenu.AddWidget(trainingButton as IFunctional, 0);
            MainMenu.AddWidget(titleName as Widget, 0);
            MainMenu.AddWidget(titleName as Widget, 1);

            PauseMenu.AddWidget(quitToMenuButton as IFunctional);

            GarageMenu.AddWidget(mothershipScrap as Widget);
            GarageMenu.AddWidget(repairButton as IFunctional);
            GarageMenu.AddWidget(repairSlot as IFunctional);
            GarageMenu.AddWidget(repairText as Widget);
            GarageMenu.AddWidget(garagePlayerImage as Widget);
            GarageMenu.AddWidget(validConfigText as Widget);

            MothershipMenu.AddWidget(furnaceSlider as IFunctional, 0);
            MothershipMenu.AddWidget(furnaceSlot as IFunctional, 0);
            MothershipMenu.AddWidget(garageButton as IFunctional, 1);
            MothershipMenu.AddWidget(craftingSlider as IFunctional, 2);
            MothershipMenu.AddWidget(requiredCraftsText as Widget, 2);
            MothershipMenu.AddWidget(craftButton as IFunctional, 2);

            PlayerMenu.AddWidget(enemySlider as IFunctional);
            PlayerMenu.AddWidget(waveText as Widget);

            exitButton.AddBehaviour(new DelegateMethod(EventHandler.Exit));
            singleplayerButton.AddBehaviour(new DelegateMethod(EventHandler.Startgame));
            trainingButton.AddBehaviour(new DelegateMethod(EventHandler.StartTraining));
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
                container.Draw(spriteBatch);
            }
            if (selectedIcon != null)
            {
                spriteBatch.Draw(selectedIcon.texture, new Vector2(Mouse.GetState().X - selectedIcon.texture.Width / 2, Mouse.GetState().Y - selectedIcon.texture.Height / 2), null, Color.White, 0, Vector2.One / 2, Engine.UIScale, SpriteEffects.None, 0.35f);
            }
        }
    }
}
