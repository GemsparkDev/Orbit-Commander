using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Space_Wars.Content.Main.Story;
public class Dialogue
{
    public Dialogue(List<string> _text, Texture2D _icon)
    {
        text=_text;
        icon=_icon;
    }
    public Dialogue(string _text, Texture2D _icon)
    {
        text=[_text];
        icon=_icon;
    }
    private List<string> text;
    private Texture2D icon;
    private float time = 0;
    private int index = 0;
    private const float WINDUP = 0.5f;
    private const float SPEED = 0.033f;
    int prevCharCount = 0;
    public void Update()
    {
        time += Engine.DeltaSeconds;
        if(time > NextDialogueTime())
        {
            index++;
        }
    }
    public bool IsComplete()
    {
        return time > GetLength();
    }
    public float GetLength()
    {
        return text.Sum(t=>t.Length) * SPEED + WINDUP * 2 + 4*text.Count;
    }
    public float NextDialogueTime()
    {
        if(time > GetLength() - WINDUP)
        {
            return GetLength();
        }
        float t = WINDUP;
        for(int i = 0; i < index+1; i++)
        {
            t += (float)(text[i].Length) * SPEED + 4f;
        }
        return t;
    }
    public void Draw(SpriteBatch _spriteBatch)
    {
        float size = 1;
        float end = time - GetLength() + WINDUP;
        if (time < WINDUP)
        {
            size = MathF.Sqrt(time / WINDUP);
        }
        else if (end >= 0)
        {
            size = 1 - MathF.Sqrt(end/WINDUP);
        }
        var pos = new Vector2(Engine.ScreenSize.X / 2, Engine.ScreenSize.Y * 3 / 4);
        _spriteBatch.Draw(Assets.Get(Sprite.Textbox), pos, null, Color.White, 0, Assets.DimsOf(Sprite.Textbox) / 2, new Vector2(size, size), 0, 0);
        if (icon != null)
        {
            _spriteBatch.Draw(icon, pos - new Vector2(30, 0), null, Color.White, 0, new Vector2(icon.Width, icon.Height) / 2 + new Vector2(Assets.Get(Sprite.WideButton).Width / 2 + 50, 0), new Vector2(size, size), 0, 0);
        }
        int characters = text[index].Length - (int)Math.Clamp((NextDialogueTime() - time -4) / SPEED, 0, text[index].Length);
        if (characters != prevCharCount)
        {
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
        }
        prevCharCount = characters;
        int increment;
        int offset = 0;
        for (int i = 0; i < characters; i += increment)
        {
            increment = NextSpace(i, index);
            _spriteBatch.DrawString(Assets.TextFont, text[index][i..Math.Min(i + increment, characters)], pos, Color.White, 0, -new Vector2(0, 15) * offset - new Vector2(-70, -45), size, 0, 0);
            offset++;
        }
    }
    private int NextSpace(int start, int index)
    {
        for (int end = start + 24; end >= start; end--)
        {
            if (end >= text[index].Length)
            {
                return text[index].Length;
            }
            if (text[index][end] == ' ')
            {
                return end - start + 1;
            }
        }
        return 0;
    }
}
