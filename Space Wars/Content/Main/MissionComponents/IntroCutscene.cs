using Microsoft.Xna.Framework.Graphics;
using System;

namespace Space_Wars.Content.Main.MissionComponents;
internal class IntroCutscene(Func<Cutscene> startCutscene) : IMissionComponent
{
    public void Draw(SpriteBatch _spriteBatch)
    {

    }

    public void Initialize()
    {
        CurrentGameState.SwitchState(startCutscene());
    }

    public void Update()
    {

    }
    public IMissionComponent Clone()
    {
        return new IntroCutscene(startCutscene);
    }
}
