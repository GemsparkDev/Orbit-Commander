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
using Myra.Graphics2D;

namespace Space_Wars.Content.Main
{
    public class UIManager
    {
        private static Desktop desktop;
        private static Engine root;
        private static Grid screen;
        private static Panel pauseMenu;
        private static Panel mothershipMenu;
        private static Panel playerMenu;
        private static Dictionary<ListItem, ListItem> listItemPairs;
        private static List<ListItem> UIitems;
        public UIManager(Engine Root, Desktop Desktop)
        {
            desktop = Desktop;
            root = Root;
            GenerateMenu();
        }
        private void GenerateMenu()
        {

            screen = new Grid
            {
                RowSpacing = 8,
                ColumnSpacing = 8,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            screen.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            screen.RowsProportions.Add(new Proportion(ProportionType.Auto));

            UIitems = new List<ListItem>();
            UIitems.Add(new ListItem() { Text = "0", });
            UIitems.Add(new ListItem() { Text = "0", });
            listItemPairs = new Dictionary<ListItem, ListItem>();
            listItemPairs.Add(new ListItem { Text = "Scrap" }, UIitems[0]);
            listItemPairs.Add(new ListItem { Text = "Copper" }, UIitems[1]);

            pauseMenu = new Panel()
            {
                Background = DefaultAssets.UITextureRegionAtlas["button"],
                Width = 250,
                Height = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Visible = false,
                Enabled = false,
                ZIndex = 6
            };
            var quitButton = new TextButton()
            {
                Text = "Quit",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            quitButton.TouchDown += (s, a) =>
            {
                root.Exit();
            };
            pauseMenu.Widgets.Add(quitButton);

            mothershipMenu = new Panel()
            {
                Background = DefaultAssets.UITextureRegionAtlas["button"],
                Width = 250,
                Height = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Visible = false,
                Enabled = false,
                ZIndex = 4
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
            mothershipMenu.Widgets.Add(grid);

            playerMenu = new Panel()
            {
                Background = DefaultAssets.UITextureRegionAtlas["button"],
                Width = 250,
                Height = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Visible = false,
                Enabled = false,
                ZIndex = 5
            };

            var image = new Image()
            {
                //Renderable = MyraEnvironment.DefaultAssetManager.Load<TextureRegion>()
            };

            screen.Widgets.Add(pauseMenu);
            screen.Widgets.Add(mothershipMenu);
            //screen.Widgets.Add(playerMenu);
            desktop.Root = screen;
        }
        public static void MainMenu()
        {
            
        }
        public static void PauseMenu()
        {
            if (pauseMenu.Visible == false)
            {
                pauseMenu.Visible = true;
                pauseMenu.Enabled = true;
                Engine.playingGame = true;
            }
            else
            {
                pauseMenu.Visible = false;
                pauseMenu.Enabled = false;
                Engine.playingGame = false;
            }

        }
        public static void mothershipInventory(PlayerResources resources)
        {
            listItemPairs.ElementAt(0).Value.Text = resources.scrap.ToString();
            listItemPairs.ElementAt(1).Value.Text = resources.copper.ToString();

            if (mothershipMenu.Visible == false)
            {
                mothershipMenu.Visible = true;
                mothershipMenu.Enabled = true;
            }
            else
            {
                mothershipMenu.Visible = false;
                mothershipMenu.Enabled = false;
            }
        }

        public static void playerInventory()
        {
            if (playerMenu.Visible == false)
            {
                playerMenu.Visible = true;
                playerMenu.Enabled = true;
            }
            else
            {
                playerMenu.Visible = false;
                playerMenu.Enabled = false;
            }
        }

        public void Draw()
        {
            desktop.Render();
        }
    }
}
