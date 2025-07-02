namespace Space_Wars.Content.Main;

public enum ModuleType
{
    Hull,
    Guns,
    Engines,
    Sensors,
    Core,
}
public enum Modules
{
    Engines,
    Sensors,
    Hull,
    //Weapons
    Basic,
    Spiral,
    Shotgun,
    Missile,
    LMG,
    Sniper,
    Crossbow,
    Flamethrower,
    Fireball,
    //Cores
    Dash,
    GrapplingHook,
    SummonShield,
    Nanomachines,
}
public enum Sprite
{
    Fighter,
    Player,
    Asteroid,
    Cruiser,
    Shotgunner,
    ShotgunShield,
    Mothership,
    Arrow,
    Sniper,
    Missile,
    PlayerGun,
    SymmetryBoss,
    OverloadBoss,
    OverloadShield,
    TurretBase,
    TurretHead,
    TurretTracker,
    Miner,
    Hovercraft,
    ExcursionBoss,
    Orbiter,
    WyvernBoss,
    AdvancedFighter,
    PickupDrone,
    Barricade,
    Trap,
    Explosion,
    Bomb,
    ExodusBoss,
    Healer,
    LargeMinerArm,
    LargeMiner,

    MetalScrap,
    RealMetalScrap,
    HullModule,
    EngineModule,
    GunModule,
    RealGunModule,
    MissileModule,
    RealMissileModule,
    SniperModule,
    RealSniperModule,
    SensorModule,
    CoreModule,
    RealTrap,
    RealBarricade,
    RealBomb,
    CrossbowModule,
    RealCrossbowModule,
    SpiralModule,
    RealSpiralModule,

    SpiralShot,
    PulseShot,
    Microshot,
    CrossbowShot,

    Dot,
    Dollar,
    Circle,

    PlayerUI,
    Title,
    Button,
    LargePanel,
    GargantuanPanel,
    EmptySlot,
    SelectedTab,
    Tab,
    Knob,
    WideButton,
    ToggleButton,
    Terminal,
    Miniplayer,

    SmeltIcon,
    RepairIcon,
    VictoryIcon,
    PlayIcon,
    SettingsIcon,
    PlanetIcon,
    Fuse,

    Cursor,
}
public enum Sound
{
    LMGFire,
    PulseFire,
    MissileFire,
    SniperFire,
    Explosion,
    ShotgunFire,

    Hit,
    ShieldHit,
    Death,

    Dock,
    Undock,
    Full,
    FireEngines,
    Beep,

    OpenMenu,
    CloseMenu,
    Interact,
    Click,
    Fail,

    main,
    boss,
    menu,
}
public enum ComponentType
{
    DockableComponent,
    TeamComponent,
}
public enum Message
{
    MothershipCraftItem,
    MothershipUpdateFurnace,
    MothershipUpdateInventory,
    ToggleTerminal,
    RestartModules,
    EscapeDroneLeave,
}
public enum Containers
{
    MainMenu,
    PauseMenu,
    PlayerMenu,
    MothershipMenu,
    GarageMenu,
    MissionMenu,
    PickupDroneMenu,
    FuseMenu,
}
public enum Condition
{
    None,
    Protect,
    Kill,
    CustomIncomplete,
    CustomComplete,
    DelayedSpawn,
}