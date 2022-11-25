using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using System.Linq;
using Space_Wars.Content.Main.Entities;
using Myra;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.Properties;
using System.Numerics;
using Myra.Graphics2D.UI.Styles;
using System.Diagnostics;

namespace Space_Wars.Content.Main
{
    public class UIManager
    {
        private static Desktop desktop;
        private static Engine root;
        private static Window pauseMenu;
        private static Window mothershipMenu;
        private static Dictionary<ListItem, ListItem> listItemPairs;
        private static List<ListItem> UIitems;
        public UIManager(Engine Root)
        {
            desktop = new Desktop();
            root = Root;
            GenerateMenu();
        }
        private void GenerateMenu()
        {
            UIitems = new List<ListItem>();
            UIitems.Add(new ListItem() { Text = "0", });
            UIitems.Add(new ListItem() { Text = "0", });
            listItemPairs = new Dictionary<ListItem, ListItem>();
            listItemPairs.Add(new ListItem { Text = "Scrap" }, UIitems[0]);
            listItemPairs.Add(new ListItem { Text = "Copper" }, UIitems[1]);

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
            var grid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                ColumnSpacing = 8,
                RowSpacing = 8,
            };
            grid.ColumnsProportions.Add(new Proportion());
            grid.ColumnsProportions.Add(new Proportion());
            grid.RowsProportions.Add(new Proportion());
            grid.RowsProportions.Add(new Proportion());
            var listBox1 = new ListBox() { SelectionMode = SelectionMode.Multiple };
            var listBox2 = new ListBox() { SelectionMode = SelectionMode.Multiple };
            foreach (KeyValuePair<ListItem, ListItem> entry in listItemPairs)
            {
                listBox1.Items.Add(entry.Key);
                listBox2.Items.Add(entry.Value);
            }
            listBox2.GridColumn = 1;
            grid.Widgets.Add(listBox1);
            grid.Widgets.Add(listBox2);
            mothershipMenu.Content = grid;
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
            listItemPairs.ElementAt(0).Value.Text = resources.scrap.ToString();
            listItemPairs.ElementAt(1).Value.Text = resources.copper.ToString();

            if (mothershipMenu.Enabled == false)
            {
                desktop.Root = mothershipMenu;
                mothershipMenu.Enabled = true;
            }
            else
            {
                desktop.Root = null;
                mothershipMenu.Enabled = false;
            }
        }

        public void Draw(SpriteBatch spritebatch)
        {
            desktop.Render();
        }
    }
}
