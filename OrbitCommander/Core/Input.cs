using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace OrbitCommander.Core;
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
        if (OldState.IsKeyUp(Keys.OemTilde) && NewState.IsKeyDown(Keys.OemTilde))
        {
            SaveGame.DebugMode = !SaveGame.DebugMode;
        }
    }
    public static IDirectionalControl Engine { get; set; } = new KeyboardDirection(Keys.W, Keys.S, Keys.A, Keys.D);
    public static IControl Dock { get; set; } = new KeyboardControl(Keys.Space);
    public static IControl Construct { get; set; } = new KeyboardControl(Keys.C);
    public static IControl SwapPrimary { get; set; } = new KeyboardControl(Keys.E);
    public static IControl OpenPanel { get; set; } = new KeyboardControl(Keys.I);
    public static IControl ToggleAimAssist { get; set; } = new KeyboardControl(Keys.LeftControl);
    public static IControl DropScrap { get; set; } = new KeyboardControl(Keys.F);
    public static IControl Ability { get; set; } = new KeyboardControl(Keys.Q);
    public static IControl WarpForward { get; set; } = new KeyboardControl(Keys.RightShift);
    public static IControl WarpBackward { get; set; } = new KeyboardControl(Keys.LeftShift);
    public static IControl SkipCutscene { get; set; } = new KeyboardControl(Keys.Escape);

}
public interface IControl
{
    public bool IsDown { get; }
    public bool WasDown { get; }
    public string InputString { get; }
}
public interface IDirectionalControl
{
    public Vector2 Direction { get; }
}
public class KeyboardControl(Keys _key) : IControl
{
    public bool IsDown => Input.NewState.IsKeyDown(_key);
    public bool WasDown => Input.OldState.IsKeyDown(_key);
    public string InputString => _key.ToString();
}
public class KeyboardDirection(Keys _up, Keys _down, Keys _left, Keys _right) : IDirectionalControl
{
    public Vector2 Direction 
    { 
        get 
        {
            Vector2 output = Vector2.Zero;
            if (Input.NewState.IsKeyDown(_up))
                output += new Vector2(0, -1);
            if (Input.NewState.IsKeyDown(_down))
                output += new Vector2(0, 1);
            if (Input.NewState.IsKeyDown(_left))
                output += new Vector2(-1, 0);
            if (Input.NewState.IsKeyDown(_right))
                output += new Vector2(1, 0);
            float length = output.Length();
            if (length > 0.0001f)
                output /= length;
            return output;
        }
    }
}


