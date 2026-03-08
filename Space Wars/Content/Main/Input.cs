using Microsoft.Xna.Framework.Input;

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
    public static bool WasJustPressed(Keys key)
    {
        return OldState.IsKeyUp(key) && NewState.IsKeyDown(key);
    }
    public static bool WasJustReleased(Keys key)
    {
        return OldState.IsKeyDown(key) && NewState.IsKeyUp(key);
    }
    public static Keys Up { get; private set; } = Keys.W;
    public static Keys Down { get; private set; } = Keys.S;
    public static Keys Left { get; private set; } = Keys.A;
    public static Keys Right { get; private set; } = Keys.D;
    public static Keys Dock { get; private set; } = Keys.Space;
    public static Keys Construct { get; private set; } = Keys.C;
    public static Keys SwapPrimary { get; private set; } = Keys.E;
    public static Keys OpenPanel { get; private set; } = Keys.I;
    public static Keys ToggleAimAssist { get; private set; } = Keys.LeftControl;
    public static Keys DropScrap { get; private set; } = Keys.F;
    public static Keys Ability { get; private set; } = Keys.Q;
    public static Keys WarpForward { get; private set; } = Keys.RightShift;
    public static Keys WarpBackward { get; private set; } = Keys.LeftShift;
    public static Keys SkipCutscene { get; private set; } = Keys.Escape;
}
