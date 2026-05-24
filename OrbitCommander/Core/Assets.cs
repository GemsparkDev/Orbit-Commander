using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OrbitCommander.Components;
using System.Collections.Generic;
using System;

namespace OrbitCommander.Core;

public static class Assets
{
    private static Dictionary<Sprites, Texture2D> Sprites { get; } = [];
    private static Dictionary<Sound, SoundEffect> SoundFX { get; } = [];
    public static SpriteFont TextFont { get; private set; }
    private static Effect effect;
    public static Effect GlobalShader => SaveGame.UseShader ? effect : null;
    public static void LoadFinal(ContentManager Content)
    {

        //
        //Sprites
        //

        //Entities
        Add(Core.Sprites.Fighter, Content.Load<Texture2D>("Images/Entity_1"));
        Add(Core.Sprites.Player, Content.Load<Texture2D>("Images/Entity_2"));
        Add(Core.Sprites.Asteroid, Content.Load<Texture2D>("Images/Entity_3"));
        Add(Core.Sprites.Cruiser, Content.Load<Texture2D>("Images/Entity_4"));
        Add(Core.Sprites.Shotgunner, Content.Load<Texture2D>("Images/Entity_5"));
        Add(Core.Sprites.ShotgunShield, Content.Load<Texture2D>("Images/Entity_5 Shield"));
        Add(Core.Sprites.Arrow, Content.Load<Texture2D>("Images/Entity_7"));
        Add(Core.Sprites.Sniper, Content.Load<Texture2D>("Images/Entity_8"));
        Add(Core.Sprites.Missile, Content.Load<Texture2D>("Images/Entity_9"));
        Add(Core.Sprites.PlayerGun, Content.Load<Texture2D>("Images/Entity_10"));
        Add(Core.Sprites.SymmetryBoss, Content.Load<Texture2D>("Images/Entity_11"));
        Add(Core.Sprites.OverloadBoss, Content.Load<Texture2D>("Images/Entity_12"));
        Add(Core.Sprites.OverloadShield, Content.Load<Texture2D>("Images/Entity_12 Shield"));
        Add(Core.Sprites.TurretBase, Content.Load<Texture2D>("Images/Entity_13"));
        Add(Core.Sprites.TurretHead, Content.Load<Texture2D>("Images/Entity_13 Turret"));
        Add(Core.Sprites.TurretTracker, Content.Load<Texture2D>("Images/Entity_13 Targeting"));
        Add(Core.Sprites.Miner, Content.Load<Texture2D>("Images/Entity_14"));
        Add(Core.Sprites.Hovercraft, Content.Load<Texture2D>("Images/Entity_15"));
        Add(Core.Sprites.ExcursionBoss, Content.Load<Texture2D>("Images/Entity_16"));
        Add(Core.Sprites.Orbiter, Content.Load<Texture2D>("Images/Entity_17"));
        Add(Core.Sprites.WyvernBoss, Content.Load<Texture2D>("Images/Entity_18"));
        Add(Core.Sprites.AdvancedFighter, Content.Load<Texture2D>("Images/Entity_19"));
        Add(Core.Sprites.PickupDrone, Content.Load<Texture2D>("Images/Entity_20"));
        Add(Core.Sprites.Explosion, Content.Load<Texture2D>("Images/Explosion"));
        Add(Core.Sprites.ExodusBoss, Content.Load<Texture2D>("Images/Entity_24"));
        Add(Core.Sprites.Healer, Content.Load<Texture2D>("Images/Entity_25"));
        Add(Core.Sprites.LargeMinerArm, Content.Load<Texture2D>("Images/Entity_26"));
        Add(Core.Sprites.LargeMiner, Content.Load<Texture2D>("Images/Entity_27"));
        Add(Core.Sprites.WarpGate, Content.Load<Texture2D>("Images/Entity_28"));
        Add(Core.Sprites.VeilBoss, Content.Load<Texture2D>("Images/Entity_29"));
        Add(Core.Sprites.InfernoBoss, Content.Load<Texture2D>("Images/Entity_30"));
        Add(Core.Sprites.FlareBoss, Content.Load<Texture2D>("Images/Entity_31"));
        Add(Core.Sprites.QuantumResonator, Content.Load<Texture2D>("Images/Entity_32"));
        Add(Core.Sprites.Communicator, Content.Load<Texture2D>("Images/Entity_33"));
        Add(Core.Sprites.Trader, Content.Load<Texture2D>("Images/Entity_34"));
        Add(Core.Sprites.MassRelayOne, Content.Load<Texture2D>("Images/Entity_35-1"));
        Add(Core.Sprites.MassRelayTwo, Content.Load<Texture2D>("Images/Entity_35-2"));
        Add(Core.Sprites.MassRelayThree, Content.Load<Texture2D>("Images/Entity_35-3"));
        Add(Core.Sprites.MassRelayFour, Content.Load<Texture2D>("Images/Entity_35-4"));
        Add(Core.Sprites.SurgeChild, Content.Load<Texture2D>("Images/Entity_36"));
        Add(Core.Sprites.SurgeBoss, Content.Load<Texture2D>("Images/Entity_37"));
        Add(Core.Sprites.Engineer, Content.Load<Texture2D>("Images/Entity_38"));
        Add(Core.Sprites.Wyrm, Content.Load<Texture2D>("Images/Entity_39"));
        Add(Core.Sprites.BloomHead, Content.Load<Texture2D>("Images/Entity_40-1"));
        Add(Core.Sprites.BloomBody, Content.Load<Texture2D>("Images/Entity_40-2"));
        Add(Core.Sprites.BloomTail, Content.Load<Texture2D>("Images/Entity_40-3"));
        Add(Core.Sprites.StreamlineBoss, Content.Load<Texture2D>("Images/Entity_41"));
        Add(Core.Sprites.StreamlineLeftWing, Content.Load<Texture2D>("Images/Entity_41-1"));
        Add(Core.Sprites.StreamlineRightWing, Content.Load<Texture2D>("Images/Entity_41-2"));
        Add(Core.Sprites.ClockworkBoss, Content.Load<Texture2D>("Images/Entity_42"));
        Add(Core.Sprites.Cog, Content.Load<Texture2D>("Images/Entity_42-1"));
        Add(Core.Sprites.ContinuumBoss, Content.Load<Texture2D>("Images/Entity_43"));
        Add(Core.Sprites.DeadeyeBoss, Content.Load<Texture2D>("Images/Entity_44"));
        Add(Core.Sprites.EpitomeOne, Content.Load<Texture2D>("Images/Entity_45"));
        Add(Core.Sprites.EpitomeTwo, Content.Load<Texture2D>("Images/Entity_45-1"));
        Add(Core.Sprites.EpitomeThree, Content.Load<Texture2D>("Images/Entity_45-2"));
        Add(Core.Sprites.DropPod, Content.Load<Texture2D>("Images/Entity_46"));
        Add(Core.Sprites.StealthFighter, Content.Load<Texture2D>("Images/Entity_48"));
        Add(Core.Sprites.Hunter, Content.Load<Texture2D>("Images/Entity_49"));

        //Items
        Add(Core.Sprites.MetalScrap, Content.Load<Texture2D>("Images/Items/Item_0"));
        Add(Core.Sprites.RealMetalScrap, Content.Load<Texture2D>("Images/Items/Item_0-1"));
        Add(Core.Sprites.Guns, Content.Load<Texture2D>("Images/Items/Item_1"));
        Add(Core.Sprites.RealGuns, Content.Load<Texture2D>("Images/Items/Item_1-1"));
        Add(Core.Sprites.MissileModule, Content.Load<Texture2D>("Images/Items/Item_2"));
        Add(Core.Sprites.RealMissileModule, Content.Load<Texture2D>("Images/Items/Item_2-1"));
        Add(Core.Sprites.SniperModule, Content.Load<Texture2D>("Images/Items/Item_3"));
        Add(Core.Sprites.RealSniperModule, Content.Load<Texture2D>("Images/Items/Item_3-1"));
        Add(Core.Sprites.Crossbow, Content.Load<Texture2D>("Images/Items/Item_4"));
        Add(Core.Sprites.RealCrossbow, Content.Load<Texture2D>("Images/Items/Item_4-1"));
        Add(Core.Sprites.Spiral, Content.Load<Texture2D>("Images/Items/Item_5"));
        Add(Core.Sprites.RealSpiral, Content.Load<Texture2D>("Images/Items/Item_5-1"));
        Add(Core.Sprites.Flamethrower, Content.Load<Texture2D>("Images/Items/Item_6"));
        Add(Core.Sprites.RealFlamethrower, Content.Load<Texture2D>("Images/Items/Item_6-1"));
        Add(Core.Sprites.SpecializedParts, Content.Load<Texture2D>("Images/Items/Item_7"));
        Add(Core.Sprites.RealSpecializedParts, Content.Load<Texture2D>("Images/Items/Item_7-1"));
        Add(Core.Sprites.Torch, Content.Load<Texture2D>("Images/Items/Item_8"));
        Add(Core.Sprites.RealTorch, Content.Load<Texture2D>("Images/Items/Item_8-1"));
        Add(Core.Sprites.Assault, Content.Load<Texture2D>("Images/Items/Item_9"));
        Add(Core.Sprites.RealAssault, Content.Load<Texture2D>("Images/Items/Item_9-1"));
        Add(Core.Sprites.Shield, Content.Load<Texture2D>("Images/Items/Item_10"));
        Add(Core.Sprites.RealShield, Content.Load<Texture2D>("Images/Items/Item_10-1"));
        Add(Core.Sprites.Lidar, Content.Load<Texture2D>("Images/Items/Item_11"));
        Add(Core.Sprites.RealLidar, Content.Load<Texture2D>("Images/Items/Item_11-1"));
        Add(Core.Sprites.Nanomachines, Content.Load<Texture2D>("Images/Items/Item_12"));
        Add(Core.Sprites.RealNanomachines, Content.Load<Texture2D>("Images/Items/Item_12-1"));
        Add(Core.Sprites.Expose, Content.Load<Texture2D>("Images/Items/Item_13"));
        Add(Core.Sprites.RealExpose, Content.Load<Texture2D>("Images/Items/Item_13-1"));
        Add(Core.Sprites.Stealth, Content.Load<Texture2D>("Images/Items/Item_14"));
        Add(Core.Sprites.RealStealth, Content.Load<Texture2D>("Images/Items/Item_14-1"));
        Add(Core.Sprites.Radar, Content.Load<Texture2D>("Images/Items/Item_15"));
        Add(Core.Sprites.RealRadar, Content.Load<Texture2D>("Images/Items/Item_15-1"));
        Add(Core.Sprites.GrapplingHook, Content.Load<Texture2D>("Images/Items/Item_16"));
        Add(Core.Sprites.RealGrapplingHook, Content.Load<Texture2D>("Images/Items/Item_16-1"));
        Add(Core.Sprites.Fireball, Content.Load<Texture2D>("Images/Items/Item_17"));
        Add(Core.Sprites.RealFireball, Content.Load<Texture2D>("Images/Items/Item_17-1"));
        Add(Core.Sprites.PrismArray, Content.Load<Texture2D>("Images/Items/Item_18"));
        Add(Core.Sprites.RealPrismArray, Content.Load<Texture2D>("Images/Items/Item_18-1"));
        Add(Core.Sprites.PulseEmitter, Content.Load<Texture2D>("Images/Items/Item_19"));
        Add(Core.Sprites.RealPulseEmitter, Content.Load<Texture2D>("Images/Items/Item_19-1"));
        Add(Core.Sprites.Ablative, Content.Load<Texture2D>("Images/Items/Item_20"));
        Add(Core.Sprites.RealAblative, Content.Load<Texture2D>("Images/Items/Item_20-1"));
        Add(Core.Sprites.Orion, Content.Load<Texture2D>("Images/Items/Item_21"));
        Add(Core.Sprites.RealOrion, Content.Load<Texture2D>("Images/Items/Item_21-1"));
        Add(Core.Sprites.Hull, Content.Load<Texture2D>("Images/Items/Item_22"));
        Add(Core.Sprites.RealHull, Content.Load<Texture2D>("Images/Items/Item_22-1"));
        Add(Core.Sprites.Engines, Content.Load<Texture2D>("Images/Items/Item_23"));
        Add(Core.Sprites.RealEngines, Content.Load<Texture2D>("Images/Items/Item_23-1"));
        Add(Core.Sprites.Sensors, Content.Load<Texture2D>("Images/Items/Item_24"));
        Add(Core.Sprites.RealSensors, Content.Load<Texture2D>("Images/Items/Item_24-1"));
        Add(Core.Sprites.Core, Content.Load<Texture2D>("Images/Items/Item_25"));
        Add(Core.Sprites.RealCore, Content.Load<Texture2D>("Images/Items/Item_25-1"));
        Add(Core.Sprites.Reflective, Content.Load<Texture2D>("Images/Items/Item_26"));
        Add(Core.Sprites.RealReflective, Content.Load<Texture2D>("Images/Items/Item_26-1"));
        Add(Core.Sprites.Barricade, Content.Load<Texture2D>("Images/Items/Item_27"));
        Add(Core.Sprites.RealBarricade, Content.Load<Texture2D>("Images/Items/Item_27-1"));
        Add(Core.Sprites.Trap, Content.Load<Texture2D>("Images/Items/Item_28"));
        Add(Core.Sprites.RealTrap, Content.Load<Texture2D>("Images/Items/Item_28-1"));
        Add(Core.Sprites.Bomb, Content.Load<Texture2D>("Images/Items/Item_29"));
        Add(Core.Sprites.RealBomb, Content.Load<Texture2D>("Images/Items/Item_29-1"));
        Add(Core.Sprites.Furnace, Content.Load<Texture2D>("Images/Items/Item_30"));
        Add(Core.Sprites.RealFurnace, Content.Load<Texture2D>("Images/Items/Item_30-1"));
        Add(Core.Sprites.Work, Content.Load<Texture2D>("Images/Items/Item_31"));
        Add(Core.Sprites.RealWork, Content.Load<Texture2D>("Images/Items/Item_31-1"));
        Add(Core.Sprites.Plasma, Content.Load<Texture2D>("Images/Items/Item_32"));
        Add(Core.Sprites.RealPlasma, Content.Load<Texture2D>("Images/Items/Item_32-1"));
        Add(Core.Sprites.Mace, Content.Load<Texture2D>("Images/Items/Item_33"));
        Add(Core.Sprites.RealMace, Content.Load<Texture2D>("Images/Items/Item_33-1"));
        Add(Core.Sprites.Dash, Content.Load<Texture2D>("Images/Items/Item_34"));
        Add(Core.Sprites.RealDash, Content.Load<Texture2D>("Images/Items/Item_34-1"));
        Add(Core.Sprites.Adaptive, Content.Load<Texture2D>("Images/Items/Item_35"));
        Add(Core.Sprites.RealAdaptive, Content.Load<Texture2D>("Images/Items/Item_35-1"));
        Add(Core.Sprites.MicroLauncher, Content.Load<Texture2D>("Images/Items/Item_36"));
        Add(Core.Sprites.RealMicroLauncher, Content.Load<Texture2D>("Images/Items/Item_36-1"));

        //Projectiles
        Add(Core.Sprites.SpiralShot, Content.Load<Texture2D>("Images/Projectile_0"));
        Add(Core.Sprites.PulseShot, Content.Load<Texture2D>("Images/Projectile_1"));
        Add(Core.Sprites.Microshot, Content.Load<Texture2D>("Images/Projectile_2"));
        Add(Core.Sprites.CrossbowShot, Content.Load<Texture2D>("Images/Projectile_3"));
        Add(Core.Sprites.Explosive, Content.Load<Texture2D>("Images/Projectile_4"));

        //Particles
        Add(Core.Sprites.Dollar, Content.Load<Texture2D>("Images/Particle_1"));
        Add(Core.Sprites.Glow, Content.Load<Texture2D>("Images/Particle_3"));
        Add(Core.Sprites.Trail, Content.Load<Texture2D>("Images/Particle_4"));

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

        //UI Elements
        Add(Core.Sprites.SwitchOne, Content.Load<Texture2D>("Images/UI_40-1"));
        Add(Core.Sprites.SwitchTwo, Content.Load<Texture2D>("Images/UI_40-2"));
        Add(Core.Sprites.SwitchThree, Content.Load<Texture2D>("Images/UI_40-3"));
        Add(Core.Sprites.SwitchFour, Content.Load<Texture2D>("Images/UI_40-4"));
        Add(Core.Sprites.Textbox, Content.Load<Texture2D>("Images/UI_36"));
        Add(Core.Sprites.Miniplayer, Content.Load<Texture2D>("Images/miniship"));

        Add(Sound.main, Content.Load<SoundEffect>("Sounds/main"));
        Add(Sound.boss, Content.Load<SoundEffect>("Sounds/boss"));
        Add(Sound.secretBoss, Content.Load<SoundEffect>("Sounds/secretBoss"));
        Add(Sound.finalBoss, Content.Load<SoundEffect>("Sounds/finalBoss"));

        Add(Sound.None, null);

    }
    public static void LoadStageOne(ContentManager Content)
    {
        //Assets used in the main menu
        effect = Content.Load<Effect>("Shaders/BloomShader");
        TextFont = Content.Load<SpriteFont>("Fonts/RobotoMono");

        //UI Elements
        Add(Core.Sprites.Title, Content.Load<Texture2D>("Images/UI_2"));
        Add(Core.Sprites.Button, Content.Load<Texture2D>("Images/UI_3"));
        Add(Core.Sprites.LargePanel, Content.Load<Texture2D>("Images/UI_8"));
        Add(Core.Sprites.GargantuanPanel, Content.Load<Texture2D>("Images/UI_4"));
        Add(Core.Sprites.SelectedTab, Content.Load<Texture2D>("Images/UI_13"));
        Add(Core.Sprites.Tab, Content.Load<Texture2D>("Images/UI_14"));
        Add(Core.Sprites.Knob, Content.Load<Texture2D>("Images/UI_16"));
        Add(Core.Sprites.WideButton, Content.Load<Texture2D>("Images/UI_18"));
        Add(Core.Sprites.ToggleButton, Content.Load<Texture2D>("Images/UI_25"));
        Add(Core.Sprites.PlayerUI, Content.Load<Texture2D>("Images/UI_1"));
        Add(Core.Sprites.EmptySlot, Content.Load<Texture2D>("Images/UI_6"));
        Add(Core.Sprites.SwitchFive, Content.Load<Texture2D>("Images/UI_40-5"));
        Add(Core.Sprites.DeadFile, Content.Load<Texture2D>("Images/UI_47"));
        Add(Core.Sprites.LEDGlow, Content.Load<Texture2D>("Images/UI_51"));
        Add(Core.Sprites.Floppy, Content.Load<Texture2D>("Images/UI_44"));
        Add(Core.Sprites.FloppyFlat, Content.Load<Texture2D>("Images/UI_45"));
        Add(Core.Sprites.RightSideOpen, Content.Load<Texture2D>("Images/UI_46"));
        Add(Core.Sprites.RightSidePanel, Content.Load<Texture2D>("Images/UI_48"));
        Add(Core.Sprites.Dial, Content.Load<Texture2D>("Images/UI_49"));
        Add(Core.Sprites.Indicator, Content.Load<Texture2D>("Images/UI_50"));
        Add(Core.Sprites.FuseDetailing, Content.Load<Texture2D>("Images/UI_52"));
        Add(Core.Sprites.FuseSlot, Content.Load<Texture2D>("Images/UI_53"));
        Add(Core.Sprites.Terminal, Content.Load<Texture2D>("Images/UI_26"));
        Add(Core.Sprites.Overlay, Content.Load<Texture2D>("Images/UI_42"));
        Add(Core.Sprites.SmeltIcon, Content.Load<Texture2D>("Images/UI_19"));
        Add(Core.Sprites.RepairIcon, Content.Load<Texture2D>("Images/UI_20"));
        Add(Core.Sprites.VictoryIcon, Content.Load<Texture2D>("Images/UI_21"));
        Add(Core.Sprites.PlayIcon, Content.Load<Texture2D>("Images/UI_22"));
        Add(Core.Sprites.SettingsIcon, Content.Load<Texture2D>("Images/UI_23"));
        Add(Core.Sprites.PlanetIcon, Content.Load<Texture2D>("Images/UI_24"));
        Add(Core.Sprites.Fuse, Content.Load<Texture2D>("Images/UI_29"));

        Add(Core.Sprites.Dot, Content.Load<Texture2D>("Images/Particle_0"));
        Add(Core.Sprites.Circle, Content.Load<Texture2D>("Images/Particle_2"));

        Add(Core.Sprites.Mothership, Content.Load<Texture2D>("Images/Entity_6"));

        //Misc
        Add(Core.Sprites.Cursor, Content.Load<Texture2D>("Images/Cursor_1"));
        Add(Core.Sprites.ClickedCursor, Content.Load<Texture2D>("Images/Cursor_2"));

        //Menu Sounds
        Add(Sound.OpenMenu, Content.Load<SoundEffect>("Sounds/OpenMenu"));
        Add(Sound.CloseMenu, Content.Load<SoundEffect>("Sounds/CloseMenu"));
        Add(Sound.Interact, Content.Load<SoundEffect>("Sounds/Interact_0"));
        Add(Sound.Click, Content.Load<SoundEffect>("Sounds/Interact_1"));
        Add(Sound.Fail, Content.Load<SoundEffect>("Sounds/Interact_2"));

        Add(Sound.menu, Content.Load<SoundEffect>("Sounds/menu"));
    }
    private static void Add(Sprites _sprite, Texture2D _texture2D)
    {
        Sprites.Add(_sprite, _texture2D);
    }
    private static void Add(Sound _sound, SoundEffect _soundEffect)
    {
        SoundFX.Add(_sound, _soundEffect);
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
