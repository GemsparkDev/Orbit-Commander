namespace Space_Wars.Content.Main;

public enum ModuleType
{
    //Categorical
    Hull,
    Guns,
    Engines,
    Sensors,
    Core,
    //Weapons
    Basic,
    Spiral,
    Shotgun,
    Missile,
    LMG,
    Sniper,
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

    SpiralShot,
    PulseShot,
    Microshot,

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

    SmeltIcon,
    RepairIcon,
    VictoryIcon,
    PlayIcon,
    SettingsIcon,
    PlanetIcon,

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