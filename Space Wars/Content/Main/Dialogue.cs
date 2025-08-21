using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;

namespace Space_Wars.Content.Main;
public class Dialogue(string _text, Texture2D _icon)
{
    private float time = 0;
    private const float SPEED = 0.05f;
    int prevCharCount = 0;
    public void Update()
    {
        time += Engine.DeltaSeconds;
    }
    public bool IsComplete()
    {
        return time > _text.Length * SPEED * 5 + 2;
    }
    public void Draw(SpriteBatch _spriteBatch)
    {
        float size = 1;
        float end = time - _text.Length * SPEED * 5 - 1;
        if (time < 1)
        {
            size = time;
        }
        else if (end is >= 0)
        {
            size = 1 - end;
        }
        Vector2 pos = Engine.Camera.Position + Engine.MousePositionOffset + new Vector2(0, Engine.ScreenSize.Y / 2 - 100);
        _spriteBatch.Draw(Assets.Get(Sprite.WideButton), pos, null, Color.White, 0, Assets.DimsOf(Sprite.WideButton) / 2, new Vector2(size, size), 0, 0);
        if (_icon != null)
        {
            _spriteBatch.Draw(_icon, pos, null, Color.White, 0, new Vector2(_icon.Width, _icon.Height)/2 + new Vector2(Assets.Get(Sprite.WideButton).Width / 3, 0), new Vector2(size, size), 0, 0);
        }
        int characters = (int)Math.Clamp((time - 1) / SPEED, 0, _text.Length);
        if (characters != prevCharCount)
        {
            SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
        }
        prevCharCount = characters;
        _spriteBatch.DrawString(Assets.TextFont, _text[0..characters], pos, Color.White, 0, Assets.TextFont.MeasureString(_text) / 2, size, 0, 0);
    }
}
