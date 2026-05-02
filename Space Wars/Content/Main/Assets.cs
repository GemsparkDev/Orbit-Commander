using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Components;
using System.Collections.Generic;

namespace Space_Wars.Content.Main;

public static class Assets
{
    private static Dictionary<Sprites, Texture2D> Sprites { get; } = [];
    private static Dictionary<Sound, SoundEffect> SoundFX { get; } = [];
    public static SpriteFont TextFont { get; private set; }
    private static Effect effect;
    public static Effect GlobalShader => SaveGame.UseShader ? effect : null;
    public static void LoadAssets(ContentManager Content)
    {
        TextFont = Content.Load<SpriteFont>("Fonts/RobotoMono");

        //
        //Sprites
        //

        //Entities
        Add(Main.Sprites.Fighter, Content.Load<Texture2D>("Images/Entity_1"));
        Add(Main.Sprites.Player, Content.Load<Texture2D>("Images/Entity_2"));
        Add(Main.Sprites.Asteroid, Content.Load<Texture2D>("Images/Entity_3"));
        Add(Main.Sprites.Cruiser, Content.Load<Texture2D>("Images/Entity_4"));
        Add(Main.Sprites.Shotgunner, Content.Load<Texture2D>("Images/Entity_5"));
        Add(Main.Sprites.ShotgunShield, Content.Load<Texture2D>("Images/Entity_5 Shield"));
        Add(Main.Sprites.Mothership, Content.Load<Texture2D>("Images/Entity_6"));
        Add(Main.Sprites.Arrow, Content.Load<Texture2D>("Images/Entity_7"));
        Add(Main.Sprites.Sniper, Content.Load<Texture2D>("Images/Entity_8"));
        Add(Main.Sprites.Missile, Content.Load<Texture2D>("Images/Entity_9"));
        Add(Main.Sprites.PlayerGun, Content.Load<Texture2D>("Images/Entity_10"));
        Add(Main.Sprites.SymmetryBoss, Content.Load<Texture2D>("Images/Entity_11"));
        Add(Main.Sprites.OverloadBoss, Content.Load<Texture2D>("Images/Entity_12"));
        Add(Main.Sprites.OverloadShield, Content.Load<Texture2D>("Images/Entity_12 Shield"));
        Add(Main.Sprites.TurretBase, Content.Load<Texture2D>("Images/Entity_13"));
        Add(Main.Sprites.TurretHead, Content.Load<Texture2D>("Images/Entity_13 Turret"));
        Add(Main.Sprites.TurretTracker, Content.Load<Texture2D>("Images/Entity_13 Targeting"));
        Add(Main.Sprites.Miner, Content.Load<Texture2D>("Images/Entity_14"));
        Add(Main.Sprites.Hovercraft, Content.Load<Texture2D>("Images/Entity_15"));
        Add(Main.Sprites.ExcursionBoss, Content.Load<Texture2D>("Images/Entity_16"));
        Add(Main.Sprites.Orbiter, Content.Load<Texture2D>("Images/Entity_17"));
        Add(Main.Sprites.WyvernBoss, Content.Load<Texture2D>("Images/Entity_18"));
        Add(Main.Sprites.AdvancedFighter, Content.Load<Texture2D>("Images/Entity_19"));
        Add(Main.Sprites.PickupDrone, Content.Load<Texture2D>("Images/Entity_20"));
        Add(Main.Sprites.Explosion, Content.Load<Texture2D>("Images/Explosion"));
        Add(Main.Sprites.ExodusBoss, Content.Load<Texture2D>("Images/Entity_24"));
        Add(Main.Sprites.Healer, Content.Load<Texture2D>("Images/Entity_25"));
        Add(Main.Sprites.LargeMinerArm, Content.Load<Texture2D>("Images/Entity_26"));
        Add(Main.Sprites.LargeMiner, Content.Load<Texture2D>("Images/Entity_27"));
        Add(Main.Sprites.WarpGate, Content.Load<Texture2D>("Images/Entity_28"));
        Add(Main.Sprites.VeilBoss, Content.Load<Texture2D>("Images/Entity_29"));
        Add(Main.Sprites.InfernoBoss, Content.Load<Texture2D>("Images/Entity_30"));
        Add(Main.Sprites.FlareBoss, Content.Load<Texture2D>("Images/Entity_31"));
        Add(Main.Sprites.QuantumResonator, Content.Load<Texture2D>("Images/Entity_32"));
        Add(Main.Sprites.Communicator, Content.Load<Texture2D>("Images/Entity_33"));
        Add(Main.Sprites.Trader, Content.Load<Texture2D>("Images/Entity_34"));
        Add(Main.Sprites.MassRelayOne, Content.Load<Texture2D>("Images/Entity_35-1"));
        Add(Main.Sprites.MassRelayTwo, Content.Load<Texture2D>("Images/Entity_35-2"));
        Add(Main.Sprites.MassRelayThree, Content.Load<Texture2D>("Images/Entity_35-3"));
        Add(Main.Sprites.MassRelayFour, Content.Load<Texture2D>("Images/Entity_35-4"));
        Add(Main.Sprites.SurgeChild, Content.Load<Texture2D>("Images/Entity_36"));
        Add(Main.Sprites.SurgeBoss, Content.Load<Texture2D>("Images/Entity_37"));
        Add(Main.Sprites.Engineer, Content.Load<Texture2D>("Images/Entity_38"));
        Add(Main.Sprites.Wyrm, Content.Load<Texture2D>("Images/Entity_39"));
        Add(Main.Sprites.BloomHead, Content.Load<Texture2D>("Images/Entity_40-1"));
        Add(Main.Sprites.BloomBody, Content.Load<Texture2D>("Images/Entity_40-2"));
        Add(Main.Sprites.BloomTail, Content.Load<Texture2D>("Images/Entity_40-3"));
        Add(Main.Sprites.StreamlineBoss, Content.Load<Texture2D>("Images/Entity_41"));
        Add(Main.Sprites.StreamlineLeftWing, Content.Load<Texture2D>("Images/Entity_41-1"));
        Add(Main.Sprites.StreamlineRightWing, Content.Load<Texture2D>("Images/Entity_41-2"));
        Add(Main.Sprites.ClockworkBoss, Content.Load<Texture2D>("Images/Entity_42"));
        Add(Main.Sprites.Cog, Content.Load<Texture2D>("Images/Entity_42-1"));
        Add(Main.Sprites.ContinuumBoss, Content.Load<Texture2D>("Images/Entity_43"));
        Add(Main.Sprites.DeadeyeBoss, Content.Load<Texture2D>("Images/Entity_44"));
        Add(Main.Sprites.EpitomeOne, Content.Load<Texture2D>("Images/Entity_45"));
        Add(Main.Sprites.EpitomeTwo, Content.Load<Texture2D>("Images/Entity_45-1"));
        Add(Main.Sprites.EpitomeThree, Content.Load<Texture2D>("Images/Entity_45-2"));
        Add(Main.Sprites.DropPod, Content.Load<Texture2D>("Images/Entity_46"));
        Add(Main.Sprites.StealthFighter, Content.Load<Texture2D>("Images/Entity_48"));
        Add(Main.Sprites.Hunter, Content.Load<Texture2D>("Images/Entity_49"));

        //Items
        Add(Main.Sprites.MetalScrap, Content.Load<Texture2D>("Images/Items/Item_0"));
        Add(Main.Sprites.RealMetalScrap, Content.Load<Texture2D>("Images/Items/Item_0-1"));
        Add(Main.Sprites.Guns, Content.Load<Texture2D>("Images/Items/Item_1"));
        Add(Main.Sprites.RealGuns, Content.Load<Texture2D>("Images/Items/Item_1-1"));
        Add(Main.Sprites.MissileModule, Content.Load<Texture2D>("Images/Items/Item_2"));
        Add(Main.Sprites.RealMissileModule, Content.Load<Texture2D>("Images/Items/Item_2-1"));
        Add(Main.Sprites.SniperModule, Content.Load<Texture2D>("Images/Items/Item_3"));
        Add(Main.Sprites.RealSniperModule, Content.Load<Texture2D>("Images/Items/Item_3-1"));
        Add(Main.Sprites.Crossbow, Content.Load<Texture2D>("Images/Items/Item_4"));
        Add(Main.Sprites.RealCrossbow, Content.Load<Texture2D>("Images/Items/Item_4-1"));
        Add(Main.Sprites.Spiral, Content.Load<Texture2D>("Images/Items/Item_5"));
        Add(Main.Sprites.RealSpiral, Content.Load<Texture2D>("Images/Items/Item_5-1"));
        Add(Main.Sprites.Flamethrower, Content.Load<Texture2D>("Images/Items/Item_6"));
        Add(Main.Sprites.RealFlamethrower, Content.Load<Texture2D>("Images/Items/Item_6-1"));
        Add(Main.Sprites.SpecializedParts, Content.Load<Texture2D>("Images/Items/Item_7"));
        Add(Main.Sprites.RealSpecializedParts, Content.Load<Texture2D>("Images/Items/Item_7-1"));
        Add(Main.Sprites.Torch, Content.Load<Texture2D>("Images/Items/Item_8"));
        Add(Main.Sprites.RealTorch, Content.Load<Texture2D>("Images/Items/Item_8-1"));
        Add(Main.Sprites.Assault, Content.Load<Texture2D>("Images/Items/Item_9"));
        Add(Main.Sprites.RealAssault, Content.Load<Texture2D>("Images/Items/Item_9-1"));
        Add(Main.Sprites.Shield, Content.Load<Texture2D>("Images/Items/Item_10"));
        Add(Main.Sprites.RealShield, Content.Load<Texture2D>("Images/Items/Item_10-1"));
        Add(Main.Sprites.Lidar, Content.Load<Texture2D>("Images/Items/Item_11"));
        Add(Main.Sprites.RealLidar, Content.Load<Texture2D>("Images/Items/Item_11-1"));
        Add(Main.Sprites.Nanomachines, Content.Load<Texture2D>("Images/Items/Item_12"));
        Add(Main.Sprites.RealNanomachines, Content.Load<Texture2D>("Images/Items/Item_12-1"));
        Add(Main.Sprites.Expose, Content.Load<Texture2D>("Images/Items/Item_13"));
        Add(Main.Sprites.RealExpose, Content.Load<Texture2D>("Images/Items/Item_13-1"));
        Add(Main.Sprites.Stealth, Content.Load<Texture2D>("Images/Items/Item_14"));
        Add(Main.Sprites.RealStealth, Content.Load<Texture2D>("Images/Items/Item_14-1"));
        Add(Main.Sprites.Radar, Content.Load<Texture2D>("Images/Items/Item_15"));
        Add(Main.Sprites.RealRadar, Content.Load<Texture2D>("Images/Items/Item_15-1"));
        Add(Main.Sprites.GrapplingHook, Content.Load<Texture2D>("Images/Items/Item_16"));
        Add(Main.Sprites.RealGrapplingHook, Content.Load<Texture2D>("Images/Items/Item_16-1"));
        Add(Main.Sprites.Fireball, Content.Load<Texture2D>("Images/Items/Item_17"));
        Add(Main.Sprites.RealFireball, Content.Load<Texture2D>("Images/Items/Item_17-1"));
        Add(Main.Sprites.PrismArray, Content.Load<Texture2D>("Images/Items/Item_18"));
        Add(Main.Sprites.RealPrismArray, Content.Load<Texture2D>("Images/Items/Item_18-1"));
        Add(Main.Sprites.PulseEmitter, Content.Load<Texture2D>("Images/Items/Item_19"));
        Add(Main.Sprites.RealPulseEmitter, Content.Load<Texture2D>("Images/Items/Item_19-1"));
        Add(Main.Sprites.Ablative, Content.Load<Texture2D>("Images/Items/Item_20"));
        Add(Main.Sprites.RealAblative, Content.Load<Texture2D>("Images/Items/Item_20-1"));
        Add(Main.Sprites.Orion, Content.Load<Texture2D>("Images/Items/Item_21"));
        Add(Main.Sprites.RealOrion, Content.Load<Texture2D>("Images/Items/Item_21-1"));
        Add(Main.Sprites.Hull, Content.Load<Texture2D>("Images/Items/Item_22"));
        Add(Main.Sprites.RealHull, Content.Load<Texture2D>("Images/Items/Item_22-1"));
        Add(Main.Sprites.Engines, Content.Load<Texture2D>("Images/Items/Item_23"));
        Add(Main.Sprites.RealEngines, Content.Load<Texture2D>("Images/Items/Item_23-1"));
        Add(Main.Sprites.Sensors, Content.Load<Texture2D>("Images/Items/Item_24"));
        Add(Main.Sprites.RealSensors, Content.Load<Texture2D>("Images/Items/Item_24-1"));
        Add(Main.Sprites.Core, Content.Load<Texture2D>("Images/Items/Item_25"));
        Add(Main.Sprites.RealCore, Content.Load<Texture2D>("Images/Items/Item_25-1"));
        Add(Main.Sprites.Reflective, Content.Load<Texture2D>("Images/Items/Item_26"));
        Add(Main.Sprites.RealReflective, Content.Load<Texture2D>("Images/Items/Item_26-1"));
        Add(Main.Sprites.Barricade, Content.Load<Texture2D>("Images/Items/Item_27"));
        Add(Main.Sprites.RealBarricade, Content.Load<Texture2D>("Images/Items/Item_27-1"));
        Add(Main.Sprites.Trap, Content.Load<Texture2D>("Images/Items/Item_28"));
        Add(Main.Sprites.RealTrap, Content.Load<Texture2D>("Images/Items/Item_28-1"));
        Add(Main.Sprites.Bomb, Content.Load<Texture2D>("Images/Items/Item_29"));
        Add(Main.Sprites.RealBomb, Content.Load<Texture2D>("Images/Items/Item_29-1"));
        Add(Main.Sprites.Furnace, Content.Load<Texture2D>("Images/Items/Item_30"));
        Add(Main.Sprites.RealFurnace, Content.Load<Texture2D>("Images/Items/Item_30-1"));

        //Projectiles
        Add(Main.Sprites.SpiralShot, Content.Load<Texture2D>("Images/Projectile_0"));
        Add(Main.Sprites.PulseShot, Content.Load<Texture2D>("Images/Projectile_1"));
        Add(Main.Sprites.Microshot, Content.Load<Texture2D>("Images/Projectile_2"));
        Add(Main.Sprites.CrossbowShot, Content.Load<Texture2D>("Images/Projectile_3"));
        Add(Main.Sprites.Explosive, Content.Load<Texture2D>("Images/Projectile_4"));

        //Particles
        Add(Main.Sprites.Dot, Content.Load<Texture2D>("Images/Particle_0"));
        Add(Main.Sprites.Dollar, Content.Load<Texture2D>("Images/Particle_1"));
        Add(Main.Sprites.Circle, Content.Load<Texture2D>("Images/Particle_2"));
        Add(Main.Sprites.Glow, Content.Load<Texture2D>("Images/Particle_3"));

        //UI Elements
        Add(Main.Sprites.PlayerUI, Content.Load<Texture2D>("Images/UI_1"));
        Add(Main.Sprites.Title, Content.Load<Texture2D>("Images/UI_2"));
        Add(Main.Sprites.Button, Content.Load<Texture2D>("Images/UI_3"));
        Add(Main.Sprites.LargePanel, Content.Load<Texture2D>("Images/UI_8"));
        Add(Main.Sprites.GargantuanPanel, Content.Load<Texture2D>("Images/UI_4"));
        Add(Main.Sprites.EmptySlot, Content.Load<Texture2D>("Images/UI_6"));
        Add(Main.Sprites.SelectedTab, Content.Load<Texture2D>("Images/UI_13"));
        Add(Main.Sprites.Tab, Content.Load<Texture2D>("Images/UI_14"));
        Add(Main.Sprites.Knob, Content.Load<Texture2D>("Images/UI_16"));
        Add(Main.Sprites.WideButton, Content.Load<Texture2D>("Images/UI_18"));
        Add(Main.Sprites.ToggleButton, Content.Load<Texture2D>("Images/UI_25"));
        Add(Main.Sprites.Terminal, Content.Load<Texture2D>("Images/UI_26"));
        Add(Main.Sprites.Miniplayer, Content.Load<Texture2D>("Images/miniship"));
        Add(Main.Sprites.SwitchOne, Content.Load<Texture2D>("Images/UI_40-1"));
        Add(Main.Sprites.SwitchTwo, Content.Load<Texture2D>("Images/UI_40-2"));
        Add(Main.Sprites.SwitchThree, Content.Load<Texture2D>("Images/UI_40-3"));
        Add(Main.Sprites.SwitchFour, Content.Load<Texture2D>("Images/UI_40-4"));
        Add(Main.Sprites.SwitchFive, Content.Load<Texture2D>("Images/UI_40-5"));
        Add(Main.Sprites.Overlay, Content.Load<Texture2D>("Images/UI_42"));
        Add(Main.Sprites.Floppy, Content.Load<Texture2D>("Images/UI_44"));
        Add(Main.Sprites.FloppyFlat, Content.Load<Texture2D>("Images/UI_45"));
        Add(Main.Sprites.RightSideOpen, Content.Load<Texture2D>("Images/UI_46"));
        Add(Main.Sprites.DeadFile, Content.Load<Texture2D>("Images/UI_47"));
        Add(Main.Sprites.RightSidePanel, Content.Load<Texture2D>("Images/UI_48"));
        Add(Main.Sprites.Dial, Content.Load<Texture2D>("Images/UI_49"));
        Add(Main.Sprites.Indicator, Content.Load<Texture2D>("Images/UI_50"));
        Add(Main.Sprites.LEDGlow, Content.Load<Texture2D>("Images/UI_51"));
        Add(Main.Sprites.FuseDetailing, Content.Load<Texture2D>("Images/UI_52"));
        Add(Main.Sprites.FuseSlot, Content.Load<Texture2D>("Images/UI_53"));
        Add(Main.Sprites.Textbox, Content.Load<Texture2D>("Images/UI_36"));

        Add(Main.Sprites.SmeltIcon, Content.Load<Texture2D>("Images/UI_19"));
        Add(Main.Sprites.RepairIcon, Content.Load<Texture2D>("Images/UI_20"));
        Add(Main.Sprites.VictoryIcon, Content.Load<Texture2D>("Images/UI_21"));
        Add(Main.Sprites.PlayIcon, Content.Load<Texture2D>("Images/UI_22"));
        Add(Main.Sprites.SettingsIcon, Content.Load<Texture2D>("Images/UI_23"));
        Add(Main.Sprites.PlanetIcon, Content.Load<Texture2D>("Images/UI_24"));
        Add(Main.Sprites.Fuse, Content.Load<Texture2D>("Images/UI_29"));

        //Misc
        Add(Main.Sprites.Cursor, Content.Load<Texture2D>("Images/Cursor_1"));
        Add(Main.Sprites.ClickedCursor, Content.Load<Texture2D>("Images/Cursor_2"));

        //
        //Sound FX
        //

        //Weapon Sounds
        Add(Sound.LMGFire, Content.Load<SoundEffect>("Sounds/Fire_0"));
        Add(Sound.PulseFire, Content.Load<SoundEffect>("Sounds/Fire_1"));
        Add(Sound.MissileFire, Content.Load<SoundEffect>("Sounds/Fire_2"));
        Add(Sound.SniperFire, Content.Load<SoundEffect>("Sounds/Fire_3"));
        Add(Sound.Explosion, Content.Load<SoundEffect>("Sounds/Fire_4"));
        Add(Sound.ShotgunFire, Content.Load<SoundEffect>("Sounds/Fire_5"));

        //Hit Sounds
        Add(Sound.Hit, Content.Load<SoundEffect>("Sounds/Hit_0"));
        Add(Sound.ShieldHit, Content.Load<SoundEffect>("Sounds/Hit_1"));
        Add(Sound.Death, Content.Load<SoundEffect>("Sounds/Death"));

        //Misc Sounds
        Add(Sound.Dock, Content.Load<SoundEffect>("Sounds/Dock"));
        Add(Sound.Undock, Content.Load<SoundEffect>("Sounds/Undock"));
        Add(Sound.Full, Content.Load<SoundEffect>("Sounds/Full"));
        Add(Sound.FireEngines, Content.Load<SoundEffect>("Sounds/Loop_0"));
        Add(Sound.FireLaser, Content.Load<SoundEffect>("Sounds/Loop_1"));
        Add(Sound.ComputerSounds, Content.Load<SoundEffect>("Sounds/Loop_2"));
        Add(Sound.Beep, Content.Load<SoundEffect>("Sounds/Timer"));

        //Menu Sounds
        Add(Sound.OpenMenu, Content.Load<SoundEffect>("Sounds/OpenMenu"));
        Add(Sound.CloseMenu, Content.Load<SoundEffect>("Sounds/CloseMenu"));
        Add(Sound.Interact, Content.Load<SoundEffect>("Sounds/Interact_0"));
        Add(Sound.Click, Content.Load<SoundEffect>("Sounds/Interact_1"));
        Add(Sound.Fail, Content.Load<SoundEffect>("Sounds/Interact_2"));

        Add(Sound.main, Content.Load<SoundEffect>("Sounds/main"));
        Add(Sound.menu, Content.Load<SoundEffect>("Sounds/menu"));
        Add(Sound.boss, Content.Load<SoundEffect>("Sounds/boss"));
        Add(Sound.secretBoss, Content.Load<SoundEffect>("Sounds/secretBoss"));
        Add(Sound.finalBoss, Content.Load<SoundEffect>("Sounds/finalBoss"));

        Add(Sound.None, null);

        effect = Content.Load<Effect>("Shaders/BloomShader");
    }
    private static void Add(Main.Sprites _sprite, Texture2D _texture2D)
    {
        Sprites.Add(_sprite, _texture2D);
        Engine.LoadText = _sprite.ToString();
    }
    private static void Add(Sound _sound, SoundEffect _soundEffect)
    {
        SoundFX.Add(_sound, _soundEffect);
        Engine.LoadText = _sound.ToString();
    }
    public static Texture2D Get(Sprites sprite)
    {
        return Sprites[sprite];
    }
    public static Vector2 DimsOf(Sprites sprite)
    {
        Texture2D texture = Sprites[sprite];
        return new Vector2(texture.Width, texture.Height);
    }
    public static SoundEffect Get(Sound sound)
    {
        return SoundFX[sound];
    }
}
