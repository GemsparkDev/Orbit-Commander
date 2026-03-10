using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Space_Wars.Content.Main;
public static class Input
{
    public static KeyboardState NewState { get; private set; }
    public static KeyboardState OldState { get; private set; }
    public static MouseState NewMouseState { get; private set; }
    public static MouseState OldMouseState { get; private set; }
    public static void Update()
    {
        OldState = NewState;
        NewState = Keyboard.GetState();
        OldMouseState = NewMouseState;
        NewMouseState = Mouse.GetState();
    }
    public static bool WasJustPressed(Binding binding)
    {
        Keys key = Keybinds[binding];
        return OldState.IsKeyUp(key) && NewState.IsKeyDown(key);
    }
    public static bool WasJustReleased(Binding binding)
    {
        Keys key = Keybinds[binding];
        return OldState.IsKeyDown(key) && NewState.IsKeyUp(key);
    }
    public static bool IsDown(Binding binding)
    {
        return NewState.IsKeyDown(Keybinds[binding]);
    }
    public static bool WasDown(Binding binding)
    {
        return OldState.IsKeyDown(Keybinds[binding]);
    }
    public static Dictionary<Binding, Keys> Keybinds = new() 
    {
        { Binding.Up, Keys.W },
        { Binding.Down, Keys.S },
        { Binding.Left, Keys.A },
        { Binding.Right, Keys.D },
        { Binding.Dock, Keys.Space },
        { Binding.Construct, Keys.C },
        { Binding.SwapPrimary, Keys.E },
        { Binding.OpenPanel, Keys.I },
        { Binding.ToggleAimAssist, Keys.LeftControl },
        { Binding.DropScrap, Keys.F },
        { Binding.Ability, Keys.Q },
        { Binding.WarpForward, Keys.RightShift },
        { Binding.WarpBackward, Keys.LeftShift },
        { Binding.SkipCutscene, Keys.Escape },
    };
}
