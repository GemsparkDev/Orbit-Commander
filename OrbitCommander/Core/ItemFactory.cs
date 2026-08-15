using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using OrbitCommander.Components;
using OrbitCommander.Entities;

namespace OrbitCommander.Core;
public static class ItemFactory
{
    //Items
    public readonly static Dictionary<Items, ItemData> itemData = new()
    {
        { Items.Scrap, new ItemData(Sprites.RealMetalScrap,Sprites.MetalScrap, "Metal Salvage", -1, Color.White) },
        { Items.SpecializedParts, new ItemData(Sprites.RealSpecializedParts, Sprites.SpecializedParts, "Specialized Parts", -1, Color.White, Color.CornflowerBlue, 10) },
        { Items.CryoBarricade, new ItemData(Sprites.RealCryoBarricade, Sprites.CryoBarricade, "CryoBarricade", 5, Color.White,Color.White,60) },
        { Items.Trap, new ItemData(Sprites.RealTrap, Sprites.Trap, "Trap", 5, Color.White,Color.White,35) },
        { Items.Bomb, new ItemData(Sprites.RealBomb, Sprites.Bomb, "Bomb", 5, Color.White,Color.White,10) },
        { Items.Furnace, new ItemData(Sprites.RealFurnace, Sprites.Furnace, "Furnace", 5, Color.White, Color.White,10) },
        { Items.FaradayShield, new ItemData(Sprites.RealFaradayShield, Sprites.FaradayShield, "FaradayShield", 5, Color.White, Color.White, 25) }
    };
    public readonly static Dictionary<Modules, ModuleData> moduleData = new()
    {
        { Modules.Hull, new ModuleData(Sprites.RealHull, Sprites.Hull, "Hull", "Getting hit provides temporary damage resistance.", (int)ModuleType.Hull, 20, typeof(Hull)) },
        { Modules.Reflective, new ModuleData(Sprites.RealReflective, Sprites.Reflective, "Reflective", "Projectiles are periodically reflected towards enemies.", (int)ModuleType.Hull, 16, typeof(Reflective)) },
        { Modules.Stealth, new ModuleData(Sprites.RealStealth, Sprites.Stealth, "Stealth", "Provides a stealth bonus, which is amplified after hitting enemies.", (int)ModuleType.Hull, 18, typeof(StealthHull)) },
        { Modules.Turtle, new ModuleData(Sprites.RealHull, Sprites.Hull, "Turtle", "Provides resistance when being close to enemies.", (int)ModuleType.Hull, 22, typeof(Turtle)) },
        { Modules.Ablative, new ModuleData(Sprites.RealAblative, Sprites.Ablative, "Ablative", "Blocks all damage until the regenerating buffer is saturated.", (int)ModuleType.Hull, 17, typeof(Ablative)) },
        { Modules.Adaptive, new ModuleData(Sprites.RealAdaptive, Sprites.Adaptive, "Adaptive", "Buffs damage but resists less damage when this module has health.", (int)ModuleType.Hull, 20, typeof(Adaptive)) },
        { Modules.ThermalShield,new ModuleData(Sprites.RealHull, Sprites.Hull, "Thermal Shield", "Brings the player toward thermal equilibrium, resisting damage near extremes.", (int)ModuleType.Hull, 20, typeof(ThermalShield)) },

        { Modules.Basic, new ModuleData(Sprites.RealGuns, Sprites.Guns, "Basic", "2x Crit for 2.5 seconds randomly after firing.", (int)ModuleType.Guns, 20, typeof(Basic)) },
        { Modules.Spiral, new ModuleData(Sprites.RealSpiral,Sprites.Spiral, "Spiral", "1.25x Crit after using an ability for a time equal to one third the ability cooldown duration.", (int)ModuleType.Guns, 20, typeof(Spiral)) },
        { Modules.Shotgun, new ModuleData(Sprites.RealGuns,Sprites.Guns, "Shotgun", "2x Crit when getting hit for a time equal to one half the taken damage seconds.", (int)ModuleType.Guns, 20, typeof(Shotgun), Color.CornflowerBlue) },
        { Modules.Missile, new ModuleData(Sprites.RealMissileModule,Sprites.MissileModule, "Missile Launcher", "2x Crit on next shot when hitting 3 or more enemies within 0.05 seconds.", (int)ModuleType.Guns, 18, typeof(Missile), Color.CornflowerBlue) },
        { Modules.LMG, new ModuleData(Sprites.RealGuns,Sprites.Guns, "Chain Gun", "1.5x Crit after continually hitting enemies.", (int)ModuleType.Guns, 20, typeof(LMG), Color.CornflowerBlue) },
        { Modules.Antimaterial, new ModuleData(Sprites.RealSniperModule,Sprites.SniperModule, "Antimaterial Rifle", "2x Crit on next shot when hitting a distant enemy.", (int)ModuleType.Guns, 20, typeof(Antimaterial), Color.CornflowerBlue)},
        { Modules.Crossbow, new ModuleData(Sprites.RealCrossbow,Sprites.Crossbow, "Crossbow", "Charge to fire. 1.5x Crit when charging for an additional 0.5 seconds.", (int)ModuleType.Guns, 20, typeof(Crossbow))},
        { Modules.Flamethrower, new ModuleData(Sprites.RealFlamethrower,Sprites.Flamethrower, "Flamethrower", "3x Crit when dangerously hot.", (int)ModuleType.Guns, 18, typeof(Flamethrower), Color.Orange)},
        { Modules.Fireball, new ModuleData(Sprites.RealFireball,Sprites.Fireball, "Fireball", "3x Crit when dangerously hot.", (int)ModuleType.Guns, 18, typeof(Fireball), Color.Orange)},
        { Modules.GrenadeLauncher, new ModuleData(Sprites.RealCrossbow,Sprites.Crossbow, "Grenade Launcher", "1.66x Crit for 30 seconds after creating a construct.", (int)ModuleType.Guns, 20, typeof(GrenadeLauncher))},
        { Modules.Spewer, new ModuleData(Sprites.RealCrossbow,Sprites.Crossbow, "Spewer", "2x Crit when firing slower than once every 2 seconds.", (int)ModuleType.Guns, 15, typeof(SpewerModule))},
        { Modules.Railgun, new ModuleData(Sprites.RealCrossbow,Sprites.Crossbow, "Railgun", "Additional 0.5x Crit to penetrated enemies for every enemy hit.", (int)ModuleType.Guns, 15, typeof(Railgun), Color.Yellow)},
        { Modules.PrismArray, new ModuleData(Sprites.RealPrismArray,Sprites.PrismArray, "Prism Array", "2x Crit when below 20 HP.", (int)ModuleType.Guns, 15, typeof(PrismArray), Color.Cyan)},
        { Modules.MatrixLauncher, new ModuleData(Sprites.RealCrossbow,Sprites.Crossbow, "Matrix Launcher", "2x Crit when below 20 HP.", (int)ModuleType.Guns, 15, typeof(MatrixLauncher), Color.Cyan)},
        { Modules.Torch, new ModuleData(Sprites.RealTorch,Sprites.Torch, "Torch", "2.5x Crit when hitting enemies who are dangerously hot.", (int)ModuleType.Guns, 15, typeof(Torch), Color.Yellow)},
        { Modules.SplitterModule, new ModuleData(Sprites.RealTorch,Sprites.Torch, "Splitter", "1.5x Crit when nearby a planet.", (int)ModuleType.Guns, 20, typeof(SplitterModule))},
        { Modules.Fractal, new ModuleData(Sprites.RealTorch,Sprites.Torch, "Fractal", "1.5x Crit when the player is moving faster than 20 units per second. Bonus crit when hitting near the base of the fractal.", (int)ModuleType.Guns, 20, typeof(Fractal))},
        { Modules.CrackShot, new ModuleData(Sprites.RealTorch,Sprites.Torch, "Crackshot", "1.25x Crit for 1.2 seconds after reloading.", (int)ModuleType.Guns, 20, typeof(CrackShot))},
        { Modules.MicroRocketLauncher, new ModuleData(Sprites.RealMicroLauncher,Sprites.MicroLauncher, "Micro Rocket Launcher", "1.7x Crit when hitting 4 or more unique enemies within 5 seconds.", (int)ModuleType.Guns, 18, typeof(MicroRocketLauncher), Color.Yellow) },
        { Modules.AdaptiveShotgun, new ModuleData(Sprites.RealTorch,Sprites.Torch, "Adaptive Shotgun", "1.5x Crit on the second shot.", (int)ModuleType.Guns, 18, typeof(AdaptiveShotgun), Color.Yellow) },
        { Modules.GuidedRound, new ModuleData(Sprites.RealTorch,Sprites.Torch, "Guided Round", "1.5x Crit when releasing 3 rounds at once.", (int)ModuleType.Guns, 20, typeof(GuidedRound), Color.White) },

        { Modules.Engines, new ModuleData(Sprites.RealEngines, Sprites.Engines, "Engines", "Basic engines with average acceleration and torque.", (int)ModuleType.Engines, 20, typeof(StandardEngine)) },
        { Modules.Plasma, new ModuleData(Sprites.RealPlasma, Sprites.Plasma, "Plasma", "High acceleration, low torque. Has a short period of higher force.", (int)ModuleType.Engines, 15, typeof(PlasmaEngine)) },
        { Modules.Work, new ModuleData(Sprites.RealWork, Sprites.Work, "Work", "High torque, low acceleration. Constructs are immune to debuffs and decay.", (int)ModuleType.Engines, 25, typeof(WorkEngine)) },
        { Modules.Orion, new ModuleData(Sprites.RealOrion, Sprites.Orion, "Orion", "Uses explosions to move which can damage enemies. Attacking enemies causes an explosion.", (int)ModuleType.Engines, 20, typeof(OrionEngine)) },

        { Modules.Sensors, new ModuleData(Sprites.RealSensors,Sprites.Sensors, "Sensors", "Increases the crit damage bonus by 1.25x + 0.1, but reduces damage by 10%.", (int)ModuleType.Sensors, 20, typeof(TargettingModifer), Color.CornflowerBlue) },
        { Modules.ProjectingModifier, new ModuleData(Sprites.RealProjectingModifier,Sprites.ProjectingModifier, "Lidar", "Projects a path for where your projectiles will go.", (int)ModuleType.Sensors, 20, typeof(ProjectingModifier), Color.Yellow) },
        { Modules.AmplifyingModifier, new ModuleData(Sprites.RealAmplifyingModifier,Sprites.AmplifyingModifier, "Radar", "Doubles damage dealt and taken", (int)ModuleType.Sensors, 20, typeof(AmplifyingModifier), Color.Yellow) },
        { Modules.CloakingModifier, new ModuleData(Sprites.RealCloakingModifier,Sprites.CloakingModifier, "Pulse Emitter", "Fired bullets randomly become invisible to enemies and you", (int)ModuleType.Sensors, 20, typeof(CloakingModifier), Color.Yellow) },

        { Modules.Assault, new ModuleData(Sprites.RealAssault, Sprites.Assault, "Assault", "Shoots a variety of projectiles around the player.", (int)ModuleType.Core, 20, typeof(Assault)) },
        { Modules.Dash, new ModuleData(Sprites.RealDash,Sprites.Dash, "Dash Core", "Teleports the player a short distance with brief immunity.", (int)ModuleType.Core, 20, typeof(Dash)) },
        { Modules.GrapplingHook, new ModuleData(Sprites.RealGrapplingHook, Sprites.GrapplingHook, "Grapple Core", $"Launches a hook which can latch on to anything. Attach the other end to objects using RMB", (int)ModuleType.Core, 20, typeof(SummonGrapplingHook)) },
        { Modules.SummonShield, new ModuleData(Sprites.RealSummonShield, Sprites.SummonShield, "Shield Core", "Summons two shields which are destroyed on hit.", (int)ModuleType.Core, 20, typeof(SummonShield)) },
        { Modules.Nanomachines, new ModuleData(Sprites.RealNanomachines, Sprites.Nanomachines, "Nanomachines", "Consumes scrap to heal the player over a short period of time.", (int)ModuleType.Core, 20, typeof(Nanomachines)) },
        { Modules.CreateFighter, new ModuleData(Sprites.RealConstructFighter, Sprites.ConstructFighter, "Construct fighter", "Consumes scrap to create several fighters around the player.", (int)ModuleType.Core, 20, typeof(CreateFighter)) },
        { Modules.Expose, new ModuleData(Sprites.RealExpose, Sprites.Expose, "Exposure", "Creates a heating or cooling aura at the cursor, doing the opposite on the player./nHold left shift to summon the other type.", (int)ModuleType.Core, 20, typeof(Expose)) },
        { Modules.Decoy, new ModuleData(Sprites.RealDecoy,Sprites.Decoy, "Decoy", "Creates a false player for enemies to attack and boosts stealth.", (int)ModuleType.Core, 20, typeof(Decoy))},

    };
    public static Pickup NewScrap(Vector2 _position = default, Vector2 _velocity = default, float _angularVelocity = 0)
    {
        var p = new Pickup(itemData[0], _position, _velocity, _angularVelocity);
        p.AddComponent(new Smelt() { Value = 1 });
        return p;
    }
    public static Pickup TryDeserialize(string _data, LoadLogger _logger)
    {
        List<string> disassembly = SaveGame.Disassemble(_data);
        if (disassembly[0] is "" or "null")
        {
            return null;
        }
        if (Enum.TryParse(disassembly[0], true, out Items result1))
        {
            return new Pickup(itemData[result1], disassembly, _logger);
        }
        if (Enum.TryParse(disassembly[0], true, out Modules result2))
        {
            var module = moduleData[result2].Retrieve();
            module.Parse(disassembly, _logger);
            return module;
        }
        throw new IOException("The module could not be parsed properly.");
    }
}
