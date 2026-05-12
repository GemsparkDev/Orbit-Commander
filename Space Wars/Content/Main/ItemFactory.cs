using Microsoft.Xna.Framework;
using Space_Wars.Content.Main.Entities;
using System;
using System.Collections.Generic;
using System.IO;

namespace Space_Wars.Content.Main;
public static class ItemFactory
{
    //Items
    public readonly static Dictionary<Items, ItemData> itemData = new()
    {
        { Items.Scrap, (new ItemData(Sprites.RealMetalScrap,Sprites.MetalScrap, "Metal Salvage", 1, Color.White)) },
        { Items.Barricade, new ItemData(Sprites.RealBarricade, Sprites.Barricade, "Barricade", 1, Color.White,Color.White,20) },
        { Items.Trap, new ItemData(Sprites.RealTrap, Sprites.Trap, "Trap", 1, Color.White,Color.White,8) },
        { Items.Bomb, new ItemData(Sprites.RealBomb, Sprites.Bomb, "Bomb", 1, Color.White,Color.White,3) },
        { Items.Furnace, new ItemData(Sprites.RealFurnace, Sprites.Furnace, "Furnace", 1, Color.White, Color.White,10) },
        { Items.SpecializedParts, new ItemData(Sprites.RealSpecializedParts, Sprites.SpecializedParts, "Specialized Parts", 1, Color.White, Color.CornflowerBlue, 5) },
        { Items.Mace, new ItemData(Sprites.RealSpecializedParts, Sprites.SpecializedParts, "Mace", 1, Color.White, Color.White, 50) }
    };
    public readonly static Dictionary<Modules, ModuleData> moduleData = new()
    {
        { Modules.Hull, new ModuleData(Sprites.RealHull, Sprites.Hull, "Hull", (int)ModuleType.Hull, 20, typeof(Hull)) },
        { Modules.Shield, new ModuleData(Sprites.RealShield, Sprites.Shield, "Shield", (int)ModuleType.Hull, 16, typeof(Shield)) },
        { Modules.Stealth, new ModuleData(Sprites.RealStealth, Sprites.Stealth, "Stealth", (int)ModuleType.Hull, 18, typeof(StealthHull)) },
        { Modules.Reflective, new ModuleData(Sprites.RealReflective, Sprites.Reflective, "Reflective", (int)ModuleType.Hull, 18, typeof(Reflective)) },
        { Modules.Turtle, new ModuleData(Sprites.RealHull, Sprites.Hull, "Turtle", (int)ModuleType.Hull, 22, typeof(Turtle)) },
        { Modules.Ablative, new ModuleData(Sprites.RealAblative, Sprites.Ablative, "Ablative", (int)ModuleType.Hull, 17, typeof(Ablative)) },
        { Modules.Adaptive, new ModuleData(Sprites.RealHull, Sprites.Hull, "Ablative", (int)ModuleType.Hull, 20, typeof(Adaptive)) },
        { Modules.ThermalShield,new ModuleData(Sprites.RealHull, Sprites.Hull, "Thermal Shield", (int)ModuleType.Hull, 20, typeof(ThermalShield)) },

        { Modules.Basic, new ModuleData(Sprites.RealGuns, Sprites.Guns, "Basic", (int)ModuleType.Guns, 20, typeof(Basic)) },
        { Modules.Spiral, new ModuleData(Sprites.RealSpiral,Sprites.Spiral, "Spiral", (int)ModuleType.Guns, 20, typeof(Spiral)) },
        { Modules.Shotgun, new ModuleData(Sprites.RealGuns,Sprites.Guns, "Shotgun", (int)ModuleType.Guns, 20, typeof(Shotgun), Color.CornflowerBlue) },
        { Modules.Missile, new ModuleData(Sprites.RealMissileModule,Sprites.MissileModule, "Missile Launcher", (int)ModuleType.Guns, 18, typeof(Missile), Color.CornflowerBlue) },
        { Modules.LMG, new ModuleData(Sprites.RealGuns,Sprites.Guns, "Chain Gun", (int)ModuleType.Guns, 20, typeof(LMG), Color.CornflowerBlue) },
        { Modules.Sniper, new ModuleData(Sprites.RealSniperModule,Sprites.SniperModule, "Antimaterial Rifle", (int)ModuleType.Guns, 20, typeof(Antimaterial), Color.CornflowerBlue)},
        { Modules.Crossbow, new ModuleData(Sprites.RealCrossbow,Sprites.Crossbow, "Crossbow", (int)ModuleType.Guns, 20, typeof(Crossbow))},
        { Modules.Flamethrower, new ModuleData(Sprites.RealFlamethrower,Sprites.Flamethrower, "Flamethrower", (int)ModuleType.Guns, 18, typeof(Flamethrower), Color.Orange)},
        { Modules.Fireball, new ModuleData(Sprites.RealFireball,Sprites.Fireball, "Fireball", (int)ModuleType.Guns, 18, typeof(Fireball), Color.Orange)},
        { Modules.GrenadeLauncher, new ModuleData(Sprites.RealCrossbow,Sprites.Crossbow, "Grenade Launcher", (int)ModuleType.Guns, 20, typeof(GrenadeLauncher))},
        { Modules.Spewer, new ModuleData(Sprites.RealCrossbow,Sprites.Crossbow, "Spewer", (int)ModuleType.Guns, 15, typeof(SpewerModule))},
        { Modules.Antimaterial, new ModuleData(Sprites.RealCrossbow,Sprites.Crossbow, "Railgun", (int)ModuleType.Guns, 15, typeof(Railgun), Color.Yellow)},
        { Modules.PrismArray, new ModuleData(Sprites.RealPrismArray,Sprites.PrismArray, "Prism Array", (int)ModuleType.Guns, 15, typeof(PrismArray), Color.Cyan)},
        { Modules.MatrixLauncher, new ModuleData(Sprites.RealCrossbow,Sprites.Crossbow, "Matrix Launcher", (int)ModuleType.Guns, 15, typeof(MatrixLauncher), Color.Cyan)},
        { Modules.Torch, new ModuleData(Sprites.RealTorch,Sprites.Torch, "Torch", (int)ModuleType.Guns, 15, typeof(Torch), Color.Yellow)},
        { Modules.SplitterModule, new ModuleData(Sprites.RealTorch,Sprites.Torch, "Splitter", (int)ModuleType.Guns, 20, typeof(SplitterModule))},
        { Modules.Fractal, new ModuleData(Sprites.RealTorch,Sprites.Torch, "Fractal", (int)ModuleType.Guns, 20, typeof(Fractal))},
        { Modules.CrackShot, new ModuleData(Sprites.RealTorch,Sprites.Torch, "Crackshot", (int)ModuleType.Guns, 20, typeof(CrackShot))},
        { Modules.MicroRocketLauncher, new ModuleData(Sprites.RealTorch,Sprites.Torch, "Micro Rocket Launcher", (int)ModuleType.Guns, 18, typeof(MicroRocketLauncher), Color.Yellow) },
        { Modules.AdaptiveShotgun, new ModuleData(Sprites.RealTorch,Sprites.Torch, "Adaptive Shotgun", (int)ModuleType.Guns, 18, typeof(AdaptiveShotgun), Color.Yellow) },
        { Modules.GuidedRound, new ModuleData(Sprites.RealTorch,Sprites.Torch, "Guided Round", (int)ModuleType.Guns, 20, typeof(GuidedRound), Color.White) },

        { Modules.Engines, new ModuleData(Sprites.RealEngines, Sprites.Engines, "Engines", (int)ModuleType.Engines, 20, typeof(StandardEngine)) },
        { Modules.Plasma, new ModuleData(Sprites.RealEngines, Sprites.Engines, "Plasma", (int)ModuleType.Engines, 15, typeof(PlasmaEngine)) },
        { Modules.Work, new ModuleData(Sprites.RealWork, Sprites.Work, "Work", (int)ModuleType.Engines, 25, typeof(WorkEngine)) },
        { Modules.Orion, new ModuleData(Sprites.RealOrion, Sprites.Orion, "Orion", (int)ModuleType.Engines, 20, typeof(OrionEngine)) },

        { Modules.Sensors, new ModuleData(Sprites.RealSensors,Sprites.Sensors, "Sensors", (int)ModuleType.Sensors, 20, typeof(Sensors), Color.CornflowerBlue) },
        { Modules.Lidar, new ModuleData(Sprites.RealLidar,Sprites.Lidar, "Lidar", (int)ModuleType.Sensors, 20, typeof(Lidar), Color.Yellow) },
        { Modules.Radar, new ModuleData(Sprites.RealRadar,Sprites.Radar, "Radar", (int)ModuleType.Sensors, 20, typeof(Radar), Color.Yellow) },
        { Modules.PulseEmitter, new ModuleData(Sprites.RealPulseEmitter,Sprites.PulseEmitter, "Pulse Emitter", (int)ModuleType.Sensors, 20, typeof(PulseEmitter), Color.Yellow) },

        { Modules.Assault, new ModuleData(Sprites.RealAssault, Sprites.Assault, "Assault", (int)ModuleType.Core, 20, typeof(Assault)) },
        { Modules.Dash, new ModuleData(Sprites.RealCore,Sprites.Core, "Dash Core", (int)ModuleType.Core, 20, typeof(Dash)) },
        { Modules.GrapplingHook, new ModuleData(Sprites.RealGrapplingHook, Sprites.GrapplingHook, "Grapple Core", (int)ModuleType.Core, 20, typeof(SummonGrapplingHook)) },
        { Modules.SummonShield, new ModuleData(Sprites.RealCore, Sprites.Core, "Shield Core", (int)ModuleType.Core, 20, typeof(SummonShield)) },
        { Modules.Nanomachines, new ModuleData(Sprites.RealNanomachines, Sprites.Nanomachines, "Nanomachines", (int)ModuleType.Core, 20, typeof(Nanomachines)) },
        { Modules.CreateFighter, new ModuleData(Sprites.RealCore, Sprites.Core, "Construct fighter", (int)ModuleType.Core, 20, typeof(CreateFighter)) },
        { Modules.Expose, new ModuleData(Sprites.RealExpose, Sprites.Expose, "Exposure", (int)ModuleType.Core, 20, typeof(Expose)) },
        { Modules.Decoy, new ModuleData(Sprites.RealTorch,Sprites.Torch, "Decoy", (int)ModuleType.Guns, 20, typeof(Decoy))},

    };
    public static Pickup NewScrap(Vector2 _position = default, Vector2 _velocity = default, float _angularVelocity = 0)
    {
        return new Pickup(itemData[0], _position, _velocity, _angularVelocity);
    }
    public static Pickup TryDeserialize(string _data, LoadLogger _logger)
    {
        List<string> disassembly = SaveGame.Disassemble(_data);
        if (disassembly[0] == "")
        {
            return null;
        }
        if (Enum.TryParse<Items>(disassembly[0], true, out Items result1))
        {
            return new Pickup(itemData[result1], disassembly, _logger);
        }
        if (Enum.TryParse<Modules>(disassembly[0], true, out Modules result2))
        {
            var module = moduleData[result2].Retrieve();
            module.Parse(disassembly, _logger);
            return module;
        }
        throw new IOException("The module could not be parsed properly.");
    }
}
