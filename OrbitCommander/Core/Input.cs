using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;

namespace OrbitCommander.Core;
public static class Input
{
    public static void SetInputScheme(InputScheme _inputScheme)
    {
        inputScheme = _inputScheme;
    }
    private static InputScheme inputScheme = new KeyboardInput();
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
public abstract class InputScheme
{
    public abstract void Update();
    public IDirectionalControl Engine { get; protected set; }
    public IControl Dock { get; protected set; }
    public IControl Construct { get; protected set; }
    public IControl SwapPrimary { get; protected set; }
    public IControl OpenPanel { get; protected set; }
    public IControl ToggleAimAssist { get; protected set; }
    public IControl DropScrap { get; protected set; }
    public IControl Ability { get; protected set; }
    public IControl WarpForward { get; protected set; }
    public IControl ModifyAbility { get; protected set; }
    public IControl Reload { get; protected set; }
    public IControl Exit { get; protected set; }
    public IControl Tab { get; protected set; }
    public IControl LMB { get; protected set; }
    public IControl RMB { get; protected set; }
    public IScalarControl Zoom { get; protected set; }
    public IDirectionalControl MousePosition { get; protected set; }
}
public class KeyboardInput : InputScheme
{
    public KeyboardState NewState { get; set; }
    public KeyboardState OldState { get; set; }
    public MouseState NewMouseState { get; set; }
    public MouseState OldMouseState { get; set; }
    public KeyboardInput()
    {
        Engine = new KeyboardDirection(0, 1, 2, 3, this);
        Dock = new KeyboardControl(4, this);
        Construct = new KeyboardControl(5, this);
        SwapPrimary = new KeyboardControl(6, this);
        OpenPanel = new KeyboardControl(7, this);
        ToggleAimAssist = new KeyboardControl(8, this);
        DropScrap = new KeyboardControl(9, this);
        Ability = new KeyboardControl(10, this);
        WarpForward = new KeyboardControl(11, this);
        ModifyAbility = new KeyboardControl(12, this);
        Reload = new KeyboardControl(13, this);
        Exit = new KeyboardControl(14, this);
        Tab = new KeyboardControl(15, this);

        //Mouse
        LMB = new LeftButton(this);
        RMB = new RightButton(this);
        Zoom = new Zoom(this);
        MousePosition = new MousePosition(this);
    }
    public override void Update()
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
public class KeyboardControl(int _key, KeyboardInput _input) : IControl
{
    public bool IsDown => _input.NewState.IsKeyDown(UI.keys[_key]);
    public bool WasDown => _input.OldState.IsKeyDown(UI.keys[_key]);
    public string InputString => _key.ToString();
}
public class KeyboardDirection(int _up, int _down, int _left, int _right, KeyboardInput _input) : IDirectionalControl
{
    public Vector2 Direction 
    { 
        get 
        {
            Vector2 output = Vector2.Zero;
            if (_input.NewState.IsKeyDown(UI.keys[_up]))
                output += new Vector2(0, -1);
            if (_input.NewState.IsKeyDown(UI.keys[_down]))
                output += new Vector2(0, 1);
            if (_input.NewState.IsKeyDown(UI.keys[_left]))
                output += new Vector2(-1, 0);
            if (_input.NewState.IsKeyDown(UI.keys[_right]))
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
            if (_input.OldState.IsKeyDown(UI.keys[_up]))
                output += new Vector2(0, -1);
            if (_input.OldState.IsKeyDown(UI.keys[_down]))
                output += new Vector2(0, 1);
            if (_input.OldState.IsKeyDown(UI.keys[_left]))
                output += new Vector2(-1, 0);
            if (_input.OldState.IsKeyDown(UI.keys[_right]))
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
            if(Engine.Self.HandMode)
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
            if (Engine.Self.HandMode)
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
    public bool IsDown 
    { 
        get 
        {
            if(Engine.Self.HandMode)
            {
                return false;
            }
            return _input.NewMouseState.RightButton == ButtonState.Pressed;
        } 
    }

    public bool WasDown 
    { 
        get 
        { 
            if(Engine.Self.HandMode)
            {
                return false;
            }
            return _input.OldMouseState.RightButton == ButtonState.Pressed;
        } 
    }

    public string InputString => "RMB";
}
public class MousePosition(KeyboardInput _input) : IDirectionalControl
{
    public Vector2 Direction => new Vector2(Math.Clamp(_input.NewMouseState.Position.X, 0, Engine.BackBuffer.X), Math.Clamp(_input.NewMouseState.Position.Y, 0, Engine.BackBuffer.Y));
    public Vector2 OldDirection => new Vector2(Math.Clamp(_input.OldMouseState.Position.X, 0, Engine.BackBuffer.X), Math.Clamp(_input.OldMouseState.Position.Y, 0, Engine.BackBuffer.Y));
}
public class Zoom(KeyboardInput _input) : IScalarControl
{
    public float Value => _input.NewMouseState.ScrollWheelValue;
    public float OldValue => _input.OldMouseState.ScrollWheelValue;
}
