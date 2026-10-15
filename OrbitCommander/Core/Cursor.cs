using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UILib.Content;
using OrbitCommander.UIElements;

namespace OrbitCommander.Core;
public class Cursor(Vector2 _center)
{
    public bool HandMode { get; private set; } = true; //false = cursor, true = hand
    public Vector2 HandPosition { get; private set; } = new Vector2(_center.X/2, _center.Y * 0.8f);
    public Vector2 OldHandPosition { get; private set; } = new Vector2(_center.X / 2, _center.Y * 0.8f);
    private Vector2 handVelocity = Vector2.Zero;
    public Vector2 CursorPosition { get; private set; } = _center;
    public Vector2 OldCursorPosition { get; private set; } = _center;
    private List<Vector2> relativePositions = [];
    public void Update()
    {
        //So the user can escape the game if needed
        if(!Engine.Self.IsActive)
        {
            return;
        }
        var state = Mouse.GetState();
        Vector2 relativePosition = new Vector2(state.Position.X, state.Position.Y) - Engine.BackBuffer / 2;
        Mouse.SetPosition((int)(Engine.BackBuffer.X / 2), (int)(Engine.BackBuffer.Y / 2));

        OldCursorPosition = CursorPosition;
        OldHandPosition = HandPosition;
        if(MathF.Abs(HandPosition.X - Engine.BackBuffer.X / 2) > Engine.BackBuffer.X / 2)
        {
            HandPosition = new Vector2(Math.Clamp(HandPosition.X, 0, Engine.BackBuffer.X), HandPosition.Y);
            handVelocity.X = 0;
        }
        if(MathF.Abs(HandPosition.Y - Engine.BackBuffer.Y / 2) > Engine.BackBuffer.Y / 2)
        {
            HandPosition = new Vector2(HandPosition.X, Math.Clamp(HandPosition.Y, 0, Engine.BackBuffer.Y));
            handVelocity.Y = 0;
        }
        CursorPosition = new Vector2(Math.Clamp(CursorPosition.X, 0, Engine.BackBuffer.X), Math.Clamp(CursorPosition.Y, 0, Engine.BackBuffer.Y));

        if (HandMode)
        {
            handVelocity += relativePosition * 0.03f;
            HandPosition += handVelocity;
            handVelocity *= 0.9f;
            Vector2 targetPosition = Engine.BackBuffer / 2;
            float speed = Engine.BackBuffer.X / (Engine.BackBuffer.X + Vector2.Distance(CursorPosition, targetPosition) * 10);
            CursorPosition = CursorPosition * (1 - speed) + targetPosition * speed;
            Engine.RealUI.Update(HandPosition, [state.LeftButton == ButtonState.Pressed, state.RightButton == ButtonState.Pressed]);
            Engine.VirtualUI.Update(CursorPosition, [false, false]);
        }
        else
        {
            var targetPosition = new Vector2(Engine.BackBuffer.X / 2, Engine.BackBuffer.Y * 0.8f);
            float distance = Vector2.Distance(HandPosition, targetPosition);
            float speed = Engine.BackBuffer.X / (Engine.BackBuffer.X + distance * 10);
            HandPosition = HandPosition * (1 - speed) + targetPosition * speed;
            if (distance < 50)
            {
                CursorPosition += relativePosition;
                Engine.RealUI.Update(HandPosition, [false, false]);
                Engine.VirtualUI.Update(CursorPosition, [state.LeftButton == ButtonState.Pressed, state.RightButton == ButtonState.Pressed]);
            }
        }
        if (Input.Tab.IsDown && !Input.Tab.WasDown)
        {
            HandMode = !HandMode;
            if (Engine.RealUI.selectedIcon is Fuse)
            {
                UI.FuseCounter.OnRisingInteract(Vector2.Zero);
            }
        }
    }
    public void DrawCursor(SpriteBatch _spriteBatch)
    {
        Vector2 newPosition = CursorPosition;
        float count = 2;
        float i = 0;
        relativePositions.Add(CursorPosition - OldCursorPosition);
        if (relativePositions.Count > count)
        {
            relativePositions.RemoveAt(0);
        }
        foreach (var pos in relativePositions)
        {
            newPosition -= pos;
        }
        Texture2D trailTexture = Assets.Get(Sprites.Circle);
        Texture2D mouseTexture;
        Color mouseColor;
        if (Input.LMB.IsDown)
        {
            mouseTexture = Assets.Get(Sprites.ClickedCursor);
            mouseColor = Color.Red;
        }
        else
        {
            mouseTexture = Assets.Get(Sprites.Cursor);
            mouseColor = Color.White;
        }
        for (int j = 0; j < relativePositions.Count; j++)
        {
            Vector2 relativePosition = relativePositions[j];
            float angle = Util.ToAngle(relativePosition);
            Vector2 nextRelative = (j < relativePositions.Count - 1) ? relativePositions[j + 1] : Vector2.Zero;
            float distance = (relativePosition != Vector2.Zero) ? relativePosition.Length() : 1;
            if (distance < 10)
            {
                newPosition += relativePosition;
                continue;
            }
            for (i = i; i < distance; i += 4)
            {
                float lerp = i / distance;
                //TODO; Figure out why the color isn't smooth.
                float t1 = 40 / (distance + 40) * (1 - lerp) + 40 / (nextRelative.Length() + 40) * (lerp);
                float transparency = MathF.Sqrt(t1) * MathF.Sqrt((j + 1) / count) * 0.01f;
                if (distance < 25)
                {
                    transparency *= Math.Clamp((distance - 10) / 15, 0, 1);
                }
                //new Color(24, 108, 80)
                _spriteBatch.Draw(trailTexture, newPosition + relativePosition * lerp, null, mouseColor * transparency, angle, UIManager.DimsOf(trailTexture) / 2, UIManager.UIScale / 16 * UIManager.DimsOf(mouseTexture) * (j / count * (1 - lerp) * 0.5f + (j + 1) / count * lerp * 0.5f + 0.5f), 0, 0.5f);
            }
            i -= distance;
            newPosition += relativePosition;
        }
        float t = (10 / (10 + (CursorPosition - OldCursorPosition).Length()));
        if(HandMode)
        {
            t *= 0.5f;
        }
        _spriteBatch.Draw(mouseTexture, CursorPosition, null, mouseColor * t, 0, UIManager.DimsOf(mouseTexture) / 2, UIManager.UIScale / 2, 0, 0.5f);
    }
    public void DrawHand(SpriteBatch _spriteBatch)
    {
        Vector2 offset = Vector2.Zero;
        float handAngle = 0;
        Texture2D handTexture;
        if (Engine.RealUI.FocusedWidget == UI.TerminalClose || !HandMode)
        {
            handTexture = Assets.Get(Sprites.Grip);
            if(HandMode)
            {
                offset = new Vector2(handTexture.Width - Assets.DimsOf(Sprites.TerminalSlider).Y * UIManager.UIScale / 4, 0);
                handAngle = -MathF.PI / 2;
                float x = UI.TerminalClose.Offset.X + UI.TerminalMenu.Position.X - handTexture.Height / 2 * UIManager.UIScale;
                HandPosition = new Vector2(Math.Clamp(HandPosition.X, x - Assets.DimsOf(Sprites.TerminalSlider).X * UIManager.UIScale / 4, x + Assets.DimsOf(Sprites.TerminalSlider).X * UIManager.UIScale / 4),
                    Math.Clamp(HandPosition.Y, Assets.DimsOf(Sprites.TerminalSlider).Y * UIManager.UIScale / 2, UI.TerminalMenu.Position.Y + UI.TerminalClose.Offset.Y - UI.TerminalMenu.Center.Y));
            }
        }
        else if (HandMode && (Mouse.GetState().LeftButton == ButtonState.Pressed || Engine.RealUI.selectedIcon != null))
        {
            handTexture = Assets.Get(Sprites.Pinch);
            handAngle = Util.ToAngle(HandPosition - new Vector2(Engine.BackBuffer.X * 3 / 4, Engine.BackBuffer.Y * 2));
            offset = new Vector2(0, 23);
        }
        else
        {
            handTexture = Assets.Get(Sprites.Hand);
            handAngle = Util.ToAngle(HandPosition - new Vector2(Engine.BackBuffer.X * 3 / 4, Engine.BackBuffer.Y * 2));
        }
        float t = 1;
        if(!HandMode)
        {
            t = 0.5f;
        }
        //Arm length modifiers
        //float a1 = 3f;
        //float a2 = 2f;
        float a3 = 1f;

        //var armOnePos = new Vector2(Engine.BackBuffer.X * 0.8f, Engine.BackBuffer.Y * 1.66f);
        //var handPosition = HandPosition + Util.RotateVector2(Assets.DimsOf(Sprites.Hand) * UIManager.UIScale * a3 * 0.75f, handAngle);

        //float a = 180 * a1 * UIManager.UIScale;
        //float b = 180 * a2 * UIManager.UIScale;
        //float csq = Vector2.DistanceSquared(armOnePos, handPosition);

        //float t1 = -MathF.Acos((b*b - a*a - csq) / (- 2 * a * MathF.Sqrt(csq))); //Law of cosines
        //float armAngle = Util.ToAngle(handPosition - armOnePos);
        //if (float.IsNaN(t1))
        //{
        //    t1 = 0;
        //}
        //_spriteBatch.Draw(Assets.Get(Sprites.Arm), armOnePos, null, Color.White, armAngle + t1, Assets.DimsOf(Sprites.Arm) - new Vector2(10, 10), UIManager.UIScale * a1, 0, 0.5f);
        //_spriteBatch.Draw(Assets.Get(Sprites.Arm), handPosition, null, Color.White, Util.ToAngle(armOnePos + Util.ToUnitVector(armAngle + t1) * a - handPosition), Assets.DimsOf(Sprites.Arm) - new Vector2(10, 10), UIManager.UIScale * a2, 0, 0.5f);
        _spriteBatch.Draw(handTexture, HandPosition + UIManager.DimsOf(handTexture) / 2, null, Color.Black * 0.2f * t, handAngle, offset, UIManager.UIScale * a3, 0, 0.5f);
        _spriteBatch.Draw(handTexture, HandPosition, null, Color.White * t, handAngle, offset, UIManager.UIScale * a3, 0, 0.5f);
    }
}
