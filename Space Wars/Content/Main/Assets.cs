using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Space_Wars.Content.Main
{
    public static class Assets
    {
        public static Dictionary<string, Texture2D> Sprites { get; } = new();
        public static Dictionary<string, SoundEffect> SoundFX { get; } = new();
        public static SpriteFont textFont;
        //public static Effect textOutline;
        //Loads all assets in the images folder to the dictionary with a specific sprite keyword
        public static void LoadAssets(Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            Sprites.Add("Miner", Content.Load<Texture2D>("Images/Entity_0"));
            Sprites.Add("Fighter", Content.Load<Texture2D>("Images/Entity_1"));
            Sprites.Add("Player", Content.Load<Texture2D>("Images/Entity_2"));
            Sprites.Add("Asteroid", Content.Load<Texture2D>("Images/Entity_3"));
            Sprites.Add("Cruiser", Content.Load<Texture2D>("Images/Entity_4"));
            Sprites.Add("Metal Scrap", Content.Load<Texture2D>("Images/Entity_5"));
            Sprites.Add("Mothership", Content.Load<Texture2D>("Images/Entity_6"));
            Sprites.Add("Arrow", Content.Load<Texture2D>("Images/Entity_7"));
            Sprites.Add("Sniper", Content.Load<Texture2D>("Images/Entity_8"));
            Sprites.Add("PulseShot", Content.Load<Texture2D>("Images/Projectile_1"));
            Sprites.Add("Cursor", Content.Load<Texture2D>("Images/Cursor"));
            Sprites.Add("Stars", Content.Load<Texture2D>("Images/Background_0"));
            Sprites.Add("Planet", Content.Load<Texture2D>("Images/Background_1"));
            Sprites.Add("Moon", Content.Load<Texture2D>("Images/Background_2"));
            Sprites.Add("Ship Tab", Content.Load<Texture2D>("Images/UI_1"));
            Sprites.Add("Player UI", Content.Load<Texture2D>("Images/UI_2"));
            Sprites.Add("Button", Content.Load<Texture2D>("Images/UI_3"));
            Sprites.Add("Empty Bar", Content.Load<Texture2D>("Images/UI_4"));
            Sprites.Add("Full Bar", Content.Load<Texture2D>("Images/UI_5"));
            Sprites.Add("Empty Slot", Content.Load<Texture2D>("Images/UI_6"));
            Sprites.Add("Upgrade 1", Content.Load<Texture2D>("Images/UI_7"));
            Sprites.Add("Large Panel", Content.Load<Texture2D>("Images/UI_8"));

            SoundFX.Add("Fire_1", Content.Load<SoundEffect>("Sounds/Fire_1"));
            SoundFX.Add("Fire_2", Content.Load<SoundEffect>("Sounds/Fire_2"));
            SoundFX.Add("Fire_3", Content.Load<SoundEffect>("Sounds/Fire_3"));
            SoundFX.Add("Hit", Content.Load<SoundEffect>("Sounds/Hit"));
            SoundFX.Add("Death", Content.Load<SoundEffect>("Sounds/Death"));
            SoundFX.Add("Interact", Content.Load<SoundEffect>("Sounds/Interact"));
            SoundFX.Add("Open Menu", Content.Load<SoundEffect>("Sounds/OpenMenu"));
            SoundFX.Add("Close Menu", Content.Load<SoundEffect>("Sounds/CloseMenu"));
            SoundFX.Add("Dock", Content.Load<SoundEffect>("Sounds/Dock"));
            SoundFX.Add("Undock", Content.Load<SoundEffect>("Sounds/Undock"));
            SoundFX.Add("Full", Content.Load<SoundEffect>("Sounds/Full"));

            textFont = Content.Load<SpriteFont>("Fonts/RobotoMono");
            //textOutline = Content.Load<Effect>("Fonts/FontOutline");
        }
    }
}
