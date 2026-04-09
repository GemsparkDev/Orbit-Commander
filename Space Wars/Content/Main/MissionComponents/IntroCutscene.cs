using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
