using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Space_Wars.Content.Main
{
    public static class Assets
    {
        public readonly static Dictionary<string, Texture2D> Sprites = new();
        public readonly static Dictionary<string, SoundEffect> SoundFX = new();
        public static SpriteFont textFont;
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

            SoundFX.Add("Fire_1", Content.Load<SoundEffect>("Sounds/Fire_1"));
            SoundFX.Add("Hit", Content.Load<SoundEffect>("Sounds/Hit"));
            SoundFX.Add("Death", Content.Load<SoundEffect>("Sounds/Death"));
            SoundFX.Add("Fire_2", Content.Load<SoundEffect>("Sounds/Fire_2"));

            textFont = Content.Load<SpriteFont>("Fonts/RobotoMono");
        }
    }
}
