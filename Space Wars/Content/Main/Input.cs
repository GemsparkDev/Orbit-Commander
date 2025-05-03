using Microsoft.Xna.Framework.Input;

namespace Space_Wars.Content.Main;
public static class Input
{
    public static KeyboardState NewState { get; private set; }
    public static KeyboardState OldState { get; private set; }
    public static void Update()
    {
        OldState = NewState;
        NewState = Keyboard.GetState();
    }
}
