using Microsoft.Xna.Framework.Graphics;
using OrbitCommander.Core;
using System;

namespace OrbitCommander.MissionComponents;
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
