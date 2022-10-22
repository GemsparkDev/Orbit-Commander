using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using Space_Wars.Content.Main.Entities;
using Myra;
using Myra.Graphics2D.UI;

namespace Space_Wars.Content.Main
{
    public class UIManager
    {
        private Desktop desktop;
        public UIManager()
        {
            desktop = new Desktop();
            var panel = new Panel()
            {
                Background = DefaultAssets.UITextureRegionAtlas["button"],
                Width = 250,
                Height = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            desktop.Root = panel;
            var paddedCenteredButton = new TextButton();
            paddedCenteredButton.Text = "Padded Centered Button";
            paddedCenteredButton.HorizontalAlignment = HorizontalAlignment.Center;
            paddedCenteredButton.VerticalAlignment = VerticalAlignment.Center;
            panel.Widgets.Add(paddedCenteredButton);
            panel.Widgets.Add(paddedCenteredButton);
        }

        public static void MainMenu()
        {

        }
        public static void PauseMenu()
        {
            
        }

        public void Draw(SpriteBatch spritebatch)
        {
            desktop.Render();
        }
    }
}
