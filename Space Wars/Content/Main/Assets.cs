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
        public static void LoadAssets(Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            //Sprites

            //Entities
            Sprites.Add("Miner", Content.Load<Texture2D>("Images/Entity_0"));
            Sprites.Add("Fighter", Content.Load<Texture2D>("Images/Entity_1"));
            Sprites.Add("Player", Content.Load<Texture2D>("Images/Entity_2"));
            Sprites.Add("Asteroid", Content.Load<Texture2D>("Images/Entity_3"));
            Sprites.Add("Cruiser", Content.Load<Texture2D>("Images/Entity_4"));
            Sprites.Add("Basic Turret", Content.Load<Texture2D>("Images/Entity_5"));
            Sprites.Add("Mothership", Content.Load<Texture2D>("Images/Entity_6"));
            Sprites.Add("Arrow", Content.Load<Texture2D>("Images/Entity_7"));
            Sprites.Add("Sniper", Content.Load<Texture2D>("Images/Entity_8"));
            Sprites.Add("Missile", Content.Load<Texture2D>("Images/Entity_9"));

            //Items
            Sprites.Add("Metal Scrap", Content.Load<Texture2D>("Images/Item_0"));
            Sprites.Add("Turret Base", Content.Load<Texture2D>("Images/Item_1"));
            Sprites.Add("Hull Module", Content.Load<Texture2D>("Images/UI_7"));
            Sprites.Add("Engine Module", Content.Load<Texture2D>("Images/UI_9"));
            Sprites.Add("Gun Module", Content.Load<Texture2D>("Images/UI_10"));
            Sprites.Add("Sensor Module", Content.Load<Texture2D>("Images/UI_11"));
            Sprites.Add("Core Module", Content.Load<Texture2D>("Images/UI_12"));

            //Projectiles
            Sprites.Add("PulseShot", Content.Load<Texture2D>("Images/Projectile_1"));

            //Particles
            Sprites.Add("Dot", Content.Load<Texture2D>("Images/Particle_0"));
            Sprites.Add("Dollar", Content.Load<Texture2D>("Images/Particle_1"));
            Sprites.Add("Circle", Content.Load<Texture2D>("Images/Particle_2"));

            //UI Elements
            Sprites.Add("Ship Tab", Content.Load<Texture2D>("Images/UI_1"));
            Sprites.Add("Player UI", Content.Load<Texture2D>("Images/UI_2"));
            Sprites.Add("Button", Content.Load<Texture2D>("Images/UI_3"));
            Sprites.Add("Empty Bar", Content.Load<Texture2D>("Images/UI_4"));
            Sprites.Add("Full Bar", Content.Load<Texture2D>("Images/UI_5"));
            Sprites.Add("Empty Slot", Content.Load<Texture2D>("Images/UI_6"));
            Sprites.Add("Large Panel", Content.Load<Texture2D>("Images/UI_8"));
            Sprites.Add("Selected Tab", Content.Load<Texture2D>("Images/UI_13"));
            Sprites.Add("Tab", Content.Load<Texture2D>("Images/UI_14"));
            Sprites.Add("Item Slot", Content.Load<Texture2D>("Images/UI_15"));
            Sprites.Add("Knob", Content.Load<Texture2D>("Images/UI_16"));
            Sprites.Add("Slider Slot", Content.Load<Texture2D>("Images/UI_17"));
            Sprites.Add("Wide Button", Content.Load<Texture2D>("Images/UI_18"));

            //Misc
            Sprites.Add("Cursor", Content.Load<Texture2D>("Images/Cursor"));
            Sprites.Add("Stars", Content.Load<Texture2D>("Images/Background_0"));
            Sprites.Add("Planet", Content.Load<Texture2D>("Images/Background_1"));
            Sprites.Add("Moon", Content.Load<Texture2D>("Images/Background_2"));

            //Sound FX

            //Weapon Sounds
            SoundFX.Add("Fire_1", Content.Load<SoundEffect>("Sounds/Fire_1"));
            SoundFX.Add("Fire_2", Content.Load<SoundEffect>("Sounds/Fire_2"));
            SoundFX.Add("Fire_3", Content.Load<SoundEffect>("Sounds/Fire_3"));
            SoundFX.Add("Explosion", Content.Load<SoundEffect>("Sounds/Fire_4"));

            //Hit Sounds
            SoundFX.Add("Hit", Content.Load<SoundEffect>("Sounds/Hit"));
            SoundFX.Add("Death", Content.Load<SoundEffect>("Sounds/Death"));

            //Misc Sounds
            SoundFX.Add("Dock", Content.Load<SoundEffect>("Sounds/Dock"));
            SoundFX.Add("Undock", Content.Load<SoundEffect>("Sounds/Undock"));
            SoundFX.Add("Full", Content.Load<SoundEffect>("Sounds/Full"));
            SoundFX.Add("Fire Engines", Content.Load<SoundEffect>("Sounds/Loop_0"));

            //Menu Sounds
            SoundFX.Add("Open Menu", Content.Load<SoundEffect>("Sounds/OpenMenu"));
            SoundFX.Add("Close Menu", Content.Load<SoundEffect>("Sounds/CloseMenu"));
            SoundFX.Add("Interact", Content.Load<SoundEffect>("Sounds/Interact"));

            //Current Text Font
            textFont = Content.Load<SpriteFont>("Fonts/RobotoMono");

            //TODO: Add more sound effects for menu sounds
            //TODO: Rename basic module elements to Item_2 - Item_6
        }
    }
}
