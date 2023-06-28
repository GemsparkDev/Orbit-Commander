using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
    }
    public delegate void DelegateMethod();
    public class UIManager
    {
        private static Engine Root { get; set; }
        private MouseState oldState;
        private static readonly List<Container> containers = new();
        private Container focusedContainer;
        public Container PauseMenu { get; } = new Window(Engine.screenSize / 2, Assets.Sprites["Player UI"]) { enabled = false };
        public Container MainMenu { get; } = new Window(Engine.screenSize / 2, Assets.Sprites["Player UI"]) { enabled = true };
        public Container PlayerMenu { get; } = new Window(Engine.screenSize / 2, Assets.Sprites["Player UI"]) { enabled = false };
        public Container MothershipMenu { get; } = new Window(Engine.screenSize / 2, Assets.Sprites["Large Panel"]) { enabled = false };
        public IFunctional exitButton;
        public IFunctional singleplayerButton;
        public IFunctional repairButton;
        public IFunctional testSlot;
        public Decal playerEngineHealth;
        public Decal playerGunsHealth;
        public Decal playerSensorsHealth;
        public Decal playerHullHealth;
        public Decal playerCoreHealth;
        public Decal mothershipScrap;
        public DraggableIcon testIcon;
        public DraggableIcon selectedIcon;

        public UIManager(Engine root)
        {
            Root = root;
            containers.Add(PauseMenu);
            containers.Add(MainMenu);
            containers.Add(PlayerMenu);
            containers.Add(MothershipMenu);
            exitButton = new Button(MainMenu.Size / 2, Assets.Sprites["Button"], "Exit", Color.White);
            singleplayerButton = new Button(MainMenu.Size / 2, Assets.Sprites["Button"], "Singleplayer", Color.White);
            repairButton = new Button(new Vector2(MainMenu.Size.X / 2 - Assets.Sprites["Button"].Width, MainMenu.Size.Y / 2), Assets.Sprites["Button"], "Repair", Color.LightBlue);
            playerHullHealth = new(new Vector2(MainMenu.Size.X / 2, 6), "20", Color.White);
            playerGunsHealth = new(new Vector2(MainMenu.Size.X / 2, 18), "20", Color.White);
            playerEngineHealth = new(new Vector2(MainMenu.Size.X / 2, 30), "20", Color.White);
            playerSensorsHealth = new(new Vector2(MainMenu.Size.X / 2, 42), "20", Color.White);
            playerCoreHealth = new(new Vector2(MainMenu.Size.X / 2, 54), "20", Color.Yellow);
            mothershipScrap = new(new Vector2(MainMenu.Size.X / 2 + 30, 30), "0", Color.Gray);
            testIcon = new DraggableIcon(Assets.Sprites["Upgrade 1"]);
            testSlot = new IconSlot(new Vector2(MainMenu.Size.X / 2 + 30, 75), Assets.Sprites["Empty Slot"], testIcon, this);
            PauseMenu.AddWidget(exitButton);
            MainMenu.AddWidget(singleplayerButton);
            PlayerMenu.AddWidget(playerHullHealth);
            PlayerMenu.AddWidget(playerGunsHealth);
            PlayerMenu.AddWidget(playerEngineHealth);
            PlayerMenu.AddWidget(playerSensorsHealth);
            PlayerMenu.AddWidget(playerCoreHealth);
            MothershipMenu.AddWidget(repairButton);
            MothershipMenu.AddWidget(playerHullHealth);
            MothershipMenu.AddWidget(playerGunsHealth);
            MothershipMenu.AddWidget(playerEngineHealth);
            MothershipMenu.AddWidget(playerSensorsHealth);
            MothershipMenu.AddWidget(playerCoreHealth);
            MothershipMenu.AddWidget(mothershipScrap);
            MothershipMenu.AddWidget(testSlot);
            exitButton.AddBehaviour(new DelegateMethod(Exit));
            singleplayerButton.AddBehaviour(new DelegateMethod(Startgame));
            repairButton.AddBehaviour(new DelegateMethod(RepairShip));
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
                Engine.PlayGlobalSound(Assets.SoundFX["Close Menu"]);
            }
            else
            {
                Engine.PlayGlobalSound(Assets.SoundFX["Open Menu"]);
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
                focusedContainer.GetWidgetOver().Interact();
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
                spriteBatch.Draw(container.texture, container.position, null, Color.White, 0, Vector2.One / 2, Engine.UIScale, SpriteEffects.None, 0.35f);
                container.Draw(spriteBatch);
            }
            if (selectedIcon != null)
            {
                spriteBatch.Draw(selectedIcon.texture, new Vector2(Mouse.GetState().X - selectedIcon.texture.Width / 2, Mouse.GetState().Y - selectedIcon.texture.Height / 2), null, Color.White, 0, Vector2.One / 2, Engine.UIScale, SpriteEffects.None, 0.35f);
            }
        }

        public static void Exit()
        {
            Root.Exit();
            Engine.PlayGlobalSound(Assets.SoundFX["Interact"]);
        }
        public static void Startgame()
        {
            Root.Startgame();
            Engine.PlayGlobalSound(Assets.SoundFX["Interact"]);
        }
        public static void RepairShip()
        {
            EntityManager.player.RepairShip();
        }
    }
}
