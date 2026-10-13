using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using OrbitCommander.Components;
using OrbitCommander.Entities;

namespace OrbitCommander.Core;
public static class ItemFactory
{
    public readonly static Dictionary<Modules, Type> moduleData = new()
    {
        { Modules.Hull, typeof(Hull) },
        { Modules.Reflective, typeof(Reflective) },
        { Modules.Stealth, typeof(StealthHull) },
        { Modules.Turtle, typeof(Turtle) },
        { Modules.Ablative, typeof(Ablative) },
        { Modules.Adaptive, typeof(Adaptive) },
        { Modules.ThermalShield, typeof(ThermalShield) },

        { Modules.Engines, typeof(StandardEngine) },
        { Modules.Plasma, typeof(PlasmaEngine) },
        { Modules.Work, typeof(WorkEngine) },
        { Modules.Orion, typeof(OrionEngine) },

        { Modules.Basic, typeof(Basic) },
        { Modules.Antimaterial, typeof(Antimaterial)},
        { Modules.Railgun, typeof(Railgun)},
        { Modules.Spiral, typeof(Spiral) },
        { Modules.Shotgun, typeof(Shotgun) },
        { Modules.Missile, typeof(Missile) },
        { Modules.LMG, typeof(LMG) },
        { Modules.Crossbow, typeof(Crossbow)},
        { Modules.Flamethrower, typeof(Flamethrower)},
        { Modules.Fireball, typeof(Fireball)},
        { Modules.GrenadeLauncher, typeof(GrenadeLauncher)},
        { Modules.Spewer, typeof(SpewerModule)},
        { Modules.PrismArray, typeof(PrismArray)},
        { Modules.MatrixLauncher, typeof(MatrixLauncher)},
        { Modules.Torch, typeof(Torch)},
        { Modules.SplitterModule, typeof(SplitterModule)},
        { Modules.Fractal, typeof(Fractal)},
        { Modules.CrackShot, typeof(CrackShot) },
        { Modules.MicroRocketLauncher, typeof(MicroRocketLauncher) },
        { Modules.AdaptiveShotgun, typeof(AdaptiveShotgun) },
        { Modules.GuidedRound, typeof(GuidedRound) },

        { Modules.Dash, typeof(Dash) },
        { Modules.SummonShield, typeof(SummonShield) },
        { Modules.GrapplingHook, typeof(SummonGrapplingHook) },
        { Modules.Nanomachines, typeof(Nanomachines) },
        { Modules.CreateFighter, typeof(CreateFighter) },
        { Modules.Assault, typeof(Assault) },
        { Modules.Decoy, typeof(Decoy)},
        { Modules.Expose, typeof(Expose) },

        { Modules.TargettingModifier, typeof(TargettingModifer) },
        { Modules.CloakingModifier, typeof(CloakingModifier) },
        { Modules.ProjectingModifier, typeof(ProjectingModifier) },
        { Modules.AmplifyingModifier, typeof(AmplifyingModifier) },

        { Modules.EmptyModule, typeof(EmptyModule) },
        { Modules.EmergencyEngine, typeof(EmergencyEngine) },
        { Modules.PointDefense, typeof(PointDefense) },
    };
    public static Module Retrieve(int index)
    {
        return (Module)Activator.CreateInstance(moduleData[UI.setModules[index]]);
    }
    public static Module Retrieve(Modules _module)
    {
        return (Module)Activator.CreateInstance(moduleData[_module]);
    }
    public static Pickup NewScrap(Vector2 _position = default, Vector2 _velocity = default, float _angularVelocity = 0)
    {
        var p = new Pickup(Sprites.MetalScrap, _position, _velocity, _angularVelocity, "Metal Salvage", Color.White, -1, 10);
        p.AddComponent(new Smelt() { Value = 1 });
        return p;
    }
    /*
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
    */
}
