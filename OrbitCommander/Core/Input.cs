using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UILib.Content;

namespace OrbitCommander.Core;
public static class Input
{
    public static void SetInputScheme(IInputScheme _inputScheme)
    {
        inputScheme = _inputScheme;
    }
    private static IInputScheme inputScheme = new KeyboardInput();
    public static void Update()
    {
        inputScheme.Update();
    }
    //Keyboard
    public static IDirectionalControl Engine => inputScheme.Engine;
    public static IControl Dock => inputScheme.Dock;
    public static IControl Construct => inputScheme.Construct;
    public static IControl SwapPrimary => inputScheme.SwapPrimary;
    public static IControl OpenPanel => inputScheme.OpenPanel;
    public static IControl ToggleAimAssist => inputScheme.ToggleAimAssist;
    public static IControl DropScrap => inputScheme.DropScrap;
    public static IControl Ability => inputScheme.Ability;
    public static IControl WarpForward => inputScheme.WarpForward;
    public static IControl ModifyAbility => inputScheme.ModifyAbility;
    public static IControl Reload => inputScheme.Reload;
    public static IControl Exit => inputScheme.Exit;
    public static IControl Tab => inputScheme.Tab;

    //Mouse
    public static IControl LMB => inputScheme.LMB;
    public static IControl RMB => inputScheme.RMB;
    public static IScalarControl Zoom => inputScheme.Zoom;
    public static IDirectionalControl MousePosition => inputScheme.MousePosition;
}
public interface IInputScheme
{
    public void Update();
    public IDirectionalControl Engine { get; }
    public IControl Dock { get; }
    public IControl Construct { get; }
    public IControl SwapPrimary { get; }
    public IControl OpenPanel { get; }
    public IControl ToggleAimAssist { get; }
    public IControl DropScrap { get; }
    public IControl Ability { get; }
    public IControl WarpForward { get; }
    public IControl ModifyAbility { get; }
    public IControl Reload { get; }
    public IControl Exit { get; }
    public IControl Tab { get; }
    public IControl LMB { get; }
    public IControl RMB { get; }
    public IScalarControl Zoom { get; }
    public IDirectionalControl MousePosition { get; }
}
public class KeyboardInput() : IInputScheme
{
    public KeyboardState NewState { get; set; }
    public KeyboardState OldState { get; set; }
    public MouseState NewMouseState { get; set; }
    public MouseState OldMouseState { get; set; }
    public IDirectionalControl Engine => new KeyboardDirection(Keys.W, Keys.S, Keys.A, Keys.D, this);
    public IControl Dock => new KeyboardControl(Keys.Space, this);
    public IControl Construct => new KeyboardControl(Keys.C, this);
    public IControl SwapPrimary => new KeyboardControl(Keys.E, this);
    public IControl OpenPanel => new KeyboardControl(Keys.I, this);
    public IControl ToggleAimAssist => new KeyboardControl(Keys.LeftControl, this);
    public IControl DropScrap => new KeyboardControl(Keys.F, this);
    public IControl Ability => new KeyboardControl(Keys.Q, this);
    public IControl WarpForward => new KeyboardControl(Keys.RightShift, this);
    public IControl ModifyAbility => new KeyboardControl(Keys.LeftShift, this);
    public IControl Reload => new KeyboardControl(Keys.R, this);
    public IControl Exit => new KeyboardControl(Keys.Escape, this);
    public IControl Tab => new KeyboardControl(Keys.Tab, this);

    //Mouse
    public IControl LMB => new LeftButton(this);
    public IControl RMB => new RightButton(this);
    public IScalarControl Zoom => new Zoom(this);
    public IDirectionalControl MousePosition => new MousePosition(this);
    public void Update()
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
}
public interface IControl
{
    public bool IsDown { get; }
    public bool WasDown { get; }
    public string InputString { get; }
}
public interface IScalarControl
{
    public float Value { get; }
    public float OldValue { get; }
}
public interface IDirectionalControl
{
    public Vector2 Direction { get; }
    public Vector2 OldDirection { get; }
}
public class KeyboardControl(Keys _key, KeyboardInput _input) : IControl
{
    public bool IsDown => _input.NewState.IsKeyDown(_key);
    public bool WasDown => _input.OldState.IsKeyDown(_key);
    public string InputString => _key.ToString();
}
public class KeyboardDirection(Keys _up, Keys _down, Keys _left, Keys _right, KeyboardInput _input) : IDirectionalControl
{
    public Vector2 Direction 
    { 
        get 
        {
            Vector2 output = Vector2.Zero;
            if (_input.NewState.IsKeyDown(_up))
                output += new Vector2(0, -1);
            if (_input.NewState.IsKeyDown(_down))
                output += new Vector2(0, 1);
            if (_input.NewState.IsKeyDown(_left))
                output += new Vector2(-1, 0);
            if (_input.NewState.IsKeyDown(_right))
                output += new Vector2(1, 0);
            float length = output.Length();
            if (length > 0.0001f)
                output /= length;
            return output;
        }
    }
    public Vector2 OldDirection
    {
        get
        {
            Vector2 output = Vector2.Zero;
            if (_input.OldState.IsKeyDown(_up))
                output += new Vector2(0, -1);
            if (_input.OldState.IsKeyDown(_down))
                output += new Vector2(0, 1);
            if (_input.OldState.IsKeyDown(_left))
                output += new Vector2(-1, 0);
            if (_input.OldState.IsKeyDown(_right))
                output += new Vector2(1, 0);
            float length = output.Length();
            if (length > 0.0001f)
                output /= length;
            return output;
        }
    }
}
public class LeftButton(KeyboardInput _input) : IControl
{
    public bool IsDown 
    { 
        get 
        {
            if(UIManager.Self.IsOver)
            {
                return false;
            }
            return _input.NewMouseState.LeftButton == ButtonState.Pressed;
        } 
    }

    public bool WasDown
    {
        get
        {
            if (UIManager.Self.IsOver)
            {
                return false;
            }
            return _input.OldMouseState.LeftButton == ButtonState.Pressed;
        }
    }

    public string InputString => "LMB";
}
public class RightButton(KeyboardInput _input) : IControl
{
    public bool IsDown => _input.NewMouseState.RightButton == ButtonState.Pressed;

    public bool WasDown => _input.OldMouseState.RightButton == ButtonState.Pressed;

    public string InputString => "RMB";
}
public class MousePosition(KeyboardInput _input) : IDirectionalControl
{
    public Vector2 Direction => new Vector2(_input.NewMouseState.Position.X, _input.NewMouseState.Position.Y);
    public Vector2 OldDirection => new Vector2(_input.OldMouseState.Position.X, _input.OldMouseState.Position.Y);
}
public class Zoom(KeyboardInput _input) : IScalarControl
{
    public float Value => _input.NewMouseState.ScrollWheelValue;
    public float OldValue => _input.OldMouseState.ScrollWheelValue;
}
