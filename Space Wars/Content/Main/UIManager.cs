using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using Space_Wars.Content.Main.Entities;
using Myra;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.Properties;
using System.Numerics;

namespace Space_Wars.Content.Main
{
    public class UIManager
    {
        private static Desktop desktop;
        private static Engine root;
        private static Window pauseMenu;
        private static Window mothershipMenu;
        public UIManager(Engine Root)
        {
            desktop = new Desktop();
            root = Root;
            GenerateMenu();
        }
        private void GenerateMenu()
        {
            pauseMenu = new Window()
            {
                Background = DefaultAssets.UITextureRegionAtlas["button"],
                Width = 250,
                Height = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Enabled = false,
            };
            var paddedCenteredButton = new TextButton();
            paddedCenteredButton.Text = "Quit";
            paddedCenteredButton.HorizontalAlignment = HorizontalAlignment.Center;
            paddedCenteredButton.VerticalAlignment = VerticalAlignment.Center;
            paddedCenteredButton.TouchDown += (s, a) =>
            {
                root.Exit();
            };
            pauseMenu.Closed += (s, a) =>
            {
                Engine.playingGame = true;
            };
            pauseMenu.Content = paddedCenteredButton;

            mothershipMenu = new Window()
            {
                Background = DefaultAssets.UITextureRegionAtlas["button"],
                Width = 250,
                Height = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Enabled = false,
            };
            var resourceText = new Label();
            resourceText.Text = "Scrap:";
            resourceText.HorizontalAlignment = HorizontalAlignment.Center;
            resourceText.VerticalAlignment = VerticalAlignment.Center;
            mothershipMenu.Content = resourceText;
        }
        public static void MainMenu()
        {

        }
        public static void PauseMenu()
        {
            if (pauseMenu.Enabled == false)
            {
                desktop.Root = pauseMenu;
                pauseMenu.Enabled = true;
            }
            else
            {
                desktop.Root = null;
                pauseMenu.Enabled = false;
            }

        }
        public static void mothershipInventory(PlayerResources resources)
        {
            PropertyGrid propertyGrid = new PropertyGrid
            {
                Object = resources,
                Width = 350,
            };

            Window window = new Window
            {
                Title = "Object Editor",
                Content = propertyGrid
            };

            desktop.Root = window;
        }

        public void Draw(SpriteBatch spritebatch)
        {
            desktop.Render();
        }
    }
}
