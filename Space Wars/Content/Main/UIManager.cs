using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.UI_Elements;

namespace Space_Wars.Content.Main
{
    public class UIManager
    {
        private static Engine Root { get; set; }
        private MouseState oldState;
        private List<Container> containers = new();
        private Container focusedContainer;
        public Container PauseMenu { get; } = new Window(Engine.screenSize / 2, Assets.Sprites["Player UI"]) { Enabled = false };
        public Container MainMenu { get; } = new Window(Engine.screenSize / 2, Assets.Sprites["Player UI"]) { Enabled = true };
        public IFunctional ExitButton;
        public IFunctional SingleplayerButton;

        public UIManager(Engine root)
        {
            Root = root;
            containers.Add(PauseMenu);
            containers.Add(MainMenu);
            ExitButton = new Button(MainMenu.Size/2, Assets.Sprites["Button"], "Exit");
            SingleplayerButton = new Button(MainMenu.Size/2, Assets.Sprites["Button"], "Singleplayer");
            PauseMenu.AddWidget(ExitButton);
            MainMenu.AddWidget(SingleplayerButton);
            ExitButton.AddBehaviour(Exit());
            SingleplayerButton.AddBehaviour(Startgame());
        }

        public void MainMenuToggle()
        {
            MainMenu.Enabled = false;
        }

        public void PauseMenuToggle()
        {
            PauseMenu.Enabled = !PauseMenu.Enabled;
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

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Container container in containers.Where(c => c.Enabled))
            {
                spriteBatch.Draw(container.Texture, container.Position, null, Color.White, 0, Vector2.One / 2, Engine.UIScale, SpriteEffects.None, 0);
                container.Draw(spriteBatch);
            }
        }

        static IEnumerable<int> Exit()
        {
            Root.Exit();

            yield return 0;
        }
        static IEnumerable<int> Startgame()
        {
            Root.Startgame();

            yield return 0;
        }
    }
}
