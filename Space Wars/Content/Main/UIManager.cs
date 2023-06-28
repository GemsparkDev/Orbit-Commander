using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.UI_Elements;

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
        public Container PauseMenu { get; } = new Window(Engine.screenSize / 2, Assets.Sprites["Player UI"]) { Enabled = false };
        public Container MainMenu { get; } = new Window(Engine.screenSize / 2, Assets.Sprites["Player UI"]) { Enabled = true };
        public Container PlayerMenu { get; } = new Window(Engine.screenSize / 2, Assets.Sprites["Player UI"]) { Enabled = false };
        public Container MothershipMenu { get; } = new Window(Engine.screenSize / 2, Assets.Sprites["Large Panel"]) { Enabled = false };
        public IFunctional ExitButton;
        public IFunctional SingleplayerButton;
        public IFunctional RepairButton;
        public IFunctional TestSlot;
        public Decal PlayerEngineHealth;
        public Decal PlayerGunsHealth;
        public Decal PlayerSensorsHealth;
        public Decal PlayerHullHealth;
        public Decal PlayerCoreHealth;
        public Decal MothershipScrap;
        public DraggableIcon TestIcon;
        public DraggableIcon selectedIcon;

        public UIManager(Engine root)
        {
            Root = root;
            containers.Add(PauseMenu);
            containers.Add(MainMenu);
            containers.Add(PlayerMenu);
            containers.Add(MothershipMenu);
            ExitButton = new Button(MainMenu.Size/2, Assets.Sprites["Button"], "Exit", Color.White);
            SingleplayerButton = new Button(MainMenu.Size/2, Assets.Sprites["Button"], "Singleplayer", Color.White);
            RepairButton = new Button(new Vector2(MainMenu.Size.X / 2 - Assets.Sprites["Button"].Width, MainMenu.Size.Y / 2), Assets.Sprites["Button"], "Repair", Color.LightBlue);
            PlayerHullHealth = new(new Vector2(MainMenu.Size.X / 2, 6), "20", Color.White);
            PlayerGunsHealth = new(new Vector2(MainMenu.Size.X / 2, 18), "20", Color.White);
            PlayerEngineHealth = new(new Vector2(MainMenu.Size.X / 2, 30), "20", Color.White);
            PlayerSensorsHealth = new(new Vector2(MainMenu.Size.X / 2, 42), "20", Color.White);
            PlayerCoreHealth = new(new Vector2(MainMenu.Size.X / 2, 54), "20", Color.Yellow);
            MothershipScrap = new(new Vector2(MainMenu.Size.X / 2 + 30, 30), "0", Color.Gray);
            TestIcon = new DraggableIcon(Assets.Sprites["Upgrade 1"]);
            TestSlot = new IconSlot(new Vector2(MainMenu.Size.X / 2 + 30, 75), Assets.Sprites["Empty Slot"], TestIcon, this);
            PauseMenu.AddWidget(ExitButton);
            MainMenu.AddWidget(SingleplayerButton);
            PlayerMenu.AddWidget(PlayerHullHealth);
            PlayerMenu.AddWidget(PlayerGunsHealth);
            PlayerMenu.AddWidget(PlayerEngineHealth);
            PlayerMenu.AddWidget(PlayerSensorsHealth);
            PlayerMenu.AddWidget(PlayerCoreHealth);
            MothershipMenu.AddWidget(RepairButton);
            MothershipMenu.AddWidget(PlayerHullHealth);
            MothershipMenu.AddWidget(PlayerGunsHealth);
            MothershipMenu.AddWidget(PlayerEngineHealth);
            MothershipMenu.AddWidget(PlayerSensorsHealth);
            MothershipMenu.AddWidget(PlayerCoreHealth);
            MothershipMenu.AddWidget(MothershipScrap);
            MothershipMenu.AddWidget(TestSlot);
            ExitButton.AddBehaviour(new DelegateMethod(Exit));
            SingleplayerButton.AddBehaviour(new DelegateMethod(Startgame));
            RepairButton.AddBehaviour(new DelegateMethod(RepairShip));
        }
        public static void ToggleMenu(Container container)
        {
            container.Enabled = !container.Enabled;
        }
        public static void ToggleMenu(Containers containerType)
        {
            Container container = GetContainer(containerType);
            container.Enabled = !container.Enabled;
        }
        public bool PauseMenuTrigger()
        {
            foreach (Container container in containers)
            {
                if(container.Enabled == true && container != PauseMenu)
                {
                    ToggleMenu(container);
                    return false;
                }
            }
            if(PauseMenu.Enabled == true)
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
            focusedContainer = containers.Where(c => c.Enabled && c.GetMouseOver())
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
            foreach (Container container in containers.Where(c => c.Enabled))
            {
                spriteBatch.Draw(container.Texture, container.Position, null, Color.White, 0, Vector2.One / 2, Engine.UIScale, SpriteEffects.None, 0.35f);
                container.Draw(spriteBatch);
            }
            if(selectedIcon != null)
            {
                spriteBatch.Draw(selectedIcon.Texture, new Vector2(Mouse.GetState().X - selectedIcon.Texture.Width/2, Mouse.GetState().Y - selectedIcon.Texture.Height/2), null, Color.White, 0, Vector2.One / 2, Engine.UIScale, SpriteEffects.None, 0.35f);
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
