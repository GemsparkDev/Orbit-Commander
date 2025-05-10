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
}
