using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

namespace Space_Wars.Content.Main;

public static class Assets
{
    private static Dictionary<Sprite, Texture2D> Sprites { get; } = new();
    private static Dictionary<Sound, SoundEffect> SoundFX { get; } = new();
    public static SpriteFont TextFont { get; private set; }
    public static void LoadAssets(Microsoft.Xna.Framework.Content.ContentManager Content)
    {
        //
        //Sprites
        //

        //Entities
        Sprites.Add(Sprite.Fighter, Content.Load<Texture2D>("Images/Entity_1"));
        Sprites.Add(Sprite.Player, Content.Load<Texture2D>("Images/Entity_2"));
        Sprites.Add(Sprite.Asteroid, Content.Load<Texture2D>("Images/Entity_3"));
        Sprites.Add(Sprite.Cruiser, Content.Load<Texture2D>("Images/Entity_4"));
        Sprites.Add(Sprite.Shotgunner, Content.Load<Texture2D>("Images/Entity_5"));
        Sprites.Add(Sprite.ShotgunShield, Content.Load<Texture2D>("Images/Entity_5 Shield"));
        Sprites.Add(Sprite.Mothership, Content.Load<Texture2D>("Images/Entity_6"));
        Sprites.Add(Sprite.Arrow, Content.Load<Texture2D>("Images/Entity_7"));
        Sprites.Add(Sprite.Sniper, Content.Load<Texture2D>("Images/Entity_8"));
        Sprites.Add(Sprite.Missile, Content.Load<Texture2D>("Images/Entity_9"));
        Sprites.Add(Sprite.PlayerGun, Content.Load<Texture2D>("Images/Entity_10"));
        Sprites.Add(Sprite.SymmetryBoss, Content.Load<Texture2D>("Images/Entity_11"));
        Sprites.Add(Sprite.OverloadBoss, Content.Load<Texture2D>("Images/Entity_12"));
        Sprites.Add(Sprite.OverloadShield, Content.Load<Texture2D>("Images/Entity_12 Shield"));
        Sprites.Add(Sprite.TurretBase, Content.Load<Texture2D>("Images/Entity_13"));
        Sprites.Add(Sprite.TurretHead, Content.Load<Texture2D>("Images/Entity_13 Turret"));
        Sprites.Add(Sprite.TurretTracker, Content.Load<Texture2D>("Images/Entity_13 Targeting"));
        Sprites.Add(Sprite.Miner, Content.Load<Texture2D>("Images/Entity_14"));
        Sprites.Add(Sprite.Hovercraft, Content.Load<Texture2D>("Images/Entity_15"));
        Sprites.Add(Sprite.ExcursionBoss, Content.Load<Texture2D>("Images/Entity_16"));
        Sprites.Add(Sprite.Orbiter, Content.Load<Texture2D>("Images/Entity_17"));
        Sprites.Add(Sprite.WyvernBoss, Content.Load<Texture2D>("Images/Entity_18"));
        Sprites.Add(Sprite.AdvancedFighter, Content.Load<Texture2D>("Images/Entity_19"));
        Sprites.Add(Sprite.PickupDrone, Content.Load<Texture2D>("Images/Entity_20"));

        //Items
        Sprites.Add(Sprite.MetalScrap, Content.Load<Texture2D>("Images/Item_0"));
        Sprites.Add(Sprite.RealMetalScrap, Content.Load<Texture2D>("Images/Item_0 Real"));
        Sprites.Add(Sprite.GunModule, Content.Load<Texture2D>("Images/Item_1"));
        Sprites.Add(Sprite.HullModule, Content.Load<Texture2D>("Images/UI_7"));
        Sprites.Add(Sprite.EngineModule, Content.Load<Texture2D>("Images/UI_9"));
        Sprites.Add(Sprite.RealGunModule, Content.Load<Texture2D>("Images/UI_10"));
        Sprites.Add(Sprite.SensorModule, Content.Load<Texture2D>("Images/UI_11"));
        Sprites.Add(Sprite.CoreModule, Content.Load<Texture2D>("Images/UI_12"));
        Sprites.Add(Sprite.RealMissileModule, Content.Load<Texture2D>("Images/UI_27"));
        Sprites.Add(Sprite.MissileModule, Content.Load<Texture2D>("Images/Item_2"));
        Sprites.Add(Sprite.SniperModule, Content.Load<Texture2D>("Images/Item_3"));
        Sprites.Add(Sprite.RealSniperModule, Content.Load<Texture2D>("Images/UI_28"));

        //Projectiles
        Sprites.Add(Sprite.SpiralShot, Content.Load<Texture2D>("Images/Projectile_0"));
        Sprites.Add(Sprite.PulseShot, Content.Load<Texture2D>("Images/Projectile_1"));
        Sprites.Add(Sprite.Microshot, Content.Load<Texture2D>("Images/Projectile_2"));

        //Particles
        Sprites.Add(Sprite.Dot, Content.Load<Texture2D>("Images/Particle_0"));
        Sprites.Add(Sprite.Dollar, Content.Load<Texture2D>("Images/Particle_1"));
        Sprites.Add(Sprite.Circle, Content.Load<Texture2D>("Images/Particle_2"));

        //UI Elements
        Sprites.Add(Sprite.PlayerUI, Content.Load<Texture2D>("Images/UI_1"));
        Sprites.Add(Sprite.Title, Content.Load<Texture2D>("Images/UI_2"));
        Sprites.Add(Sprite.Button, Content.Load<Texture2D>("Images/UI_3"));
        Sprites.Add(Sprite.LargePanel, Content.Load<Texture2D>("Images/UI_8"));
        Sprites.Add(Sprite.GargantuanPanel, Content.Load<Texture2D>("Images/UI_4"));
        Sprites.Add(Sprite.EmptySlot, Content.Load<Texture2D>("Images/UI_6"));
        Sprites.Add(Sprite.SelectedTab, Content.Load<Texture2D>("Images/UI_13"));
        Sprites.Add(Sprite.Tab, Content.Load<Texture2D>("Images/UI_14"));
        Sprites.Add(Sprite.Knob, Content.Load<Texture2D>("Images/UI_16"));
        Sprites.Add(Sprite.WideButton, Content.Load<Texture2D>("Images/UI_18"));
        Sprites.Add(Sprite.ToggleButton, Content.Load<Texture2D>("Images/UI_25"));
        Sprites.Add(Sprite.Terminal, Content.Load<Texture2D>("Images/UI_26"));

        Sprites.Add(Sprite.SmeltIcon, Content.Load<Texture2D>("Images/UI_19"));
        Sprites.Add(Sprite.RepairIcon, Content.Load<Texture2D>("Images/UI_20"));
        Sprites.Add(Sprite.VictoryIcon, Content.Load<Texture2D>("Images/UI_21"));
        Sprites.Add(Sprite.PlayIcon, Content.Load<Texture2D>("Images/UI_22"));
        Sprites.Add(Sprite.SettingsIcon, Content.Load<Texture2D>("Images/UI_23"));
        Sprites.Add(Sprite.PlanetIcon, Content.Load<Texture2D>("Images/UI_24"));
        Sprites.Add(Sprite.Fuse, Content.Load<Texture2D>("Images/UI_29"));

        //Misc
        Sprites.Add(Sprite.Cursor, Content.Load<Texture2D>("Images/Cursor"));

        //
        //Sound FX
        //

        //Weapon Sounds
        SoundFX.Add(Sound.LMGFire, Content.Load<SoundEffect>("Sounds/Fire_0"));
        SoundFX.Add(Sound.PulseFire, Content.Load<SoundEffect>("Sounds/Fire_1"));
        SoundFX.Add(Sound.MissileFire, Content.Load<SoundEffect>("Sounds/Fire_2"));
        SoundFX.Add(Sound.SniperFire, Content.Load<SoundEffect>("Sounds/Fire_3"));
        SoundFX.Add(Sound.Explosion, Content.Load<SoundEffect>("Sounds/Fire_4"));
        SoundFX.Add(Sound.ShotgunFire, Content.Load<SoundEffect>("Sounds/Fire_5"));

        //Hit Sounds
        SoundFX.Add(Sound.Hit, Content.Load<SoundEffect>("Sounds/Hit_0"));
        SoundFX.Add(Sound.ShieldHit, Content.Load<SoundEffect>("Sounds/Hit_1"));
        SoundFX.Add(Sound.Death, Content.Load<SoundEffect>("Sounds/Death"));

        //Misc Sounds
        SoundFX.Add(Sound.Dock, Content.Load<SoundEffect>("Sounds/Dock"));
        SoundFX.Add(Sound.Undock, Content.Load<SoundEffect>("Sounds/Undock"));
        SoundFX.Add(Sound.Full, Content.Load<SoundEffect>("Sounds/Full"));
        SoundFX.Add(Sound.FireEngines, Content.Load<SoundEffect>("Sounds/Loop_0"));
        SoundFX.Add(Sound.Beep, Content.Load<SoundEffect>("Sounds/Timer"));

        //Menu Sounds
        SoundFX.Add(Sound.OpenMenu, Content.Load<SoundEffect>("Sounds/OpenMenu"));
        SoundFX.Add(Sound.CloseMenu, Content.Load<SoundEffect>("Sounds/CloseMenu"));
        SoundFX.Add(Sound.Interact, Content.Load<SoundEffect>("Sounds/Interact_0"));
        SoundFX.Add(Sound.Click, Content.Load<SoundEffect>("Sounds/Interact_1"));
        SoundFX.Add(Sound.Fail, Content.Load<SoundEffect>("Sounds/Interact_2"));

        SoundFX.Add(Sound.main, Content.Load<SoundEffect>("Sounds/main"));
        SoundFX.Add(Sound.menu, Content.Load<SoundEffect>("Sounds/menu"));
        SoundFX.Add(Sound.boss, Content.Load<SoundEffect>("Sounds/boss"));


        //Current Text Font
        TextFont = Content.Load<SpriteFont>("Fonts/RobotoMono");

        //effect = Content.Load<Effect>("Shaders/BloomShader");
        //TODO: Add more sound effects for menu sounds
        //TODO: Rename basic module elements to Item_2 - Item_6
    }
    public static Texture2D Get(Sprite sprite)
    {
        return Sprites[sprite];
    }
    public static Vector2 DimsOf(Sprite sprite)
    {
        Texture2D texture = Sprites[sprite];
        return new Vector2(texture.Width, texture.Height);
    }
    public static SoundEffect Get(Sound sound)
    {
        return SoundFX[sound];
    }
}
