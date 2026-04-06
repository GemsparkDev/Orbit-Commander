using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Wars.Content.Main.MissionComponents;
internal class Cutscenes(Func<Cutscene> startCutscene, Func<Cutscene> endCutscene) : IMissionComponent
{
    public void Draw(SpriteBatch _spriteBatch)
    {
        
    }

    public void Initialize()
    {
        if (startCutscene != null)
        {
            CurrentGameState.SwitchState(startCutscene());
        }
        else
        {
            CurrentGameState.SwitchState(new PlayingGame());
        }
    }

    public void Update()
    {
        if(Engine.SaveGame.CurrentMission.RestartTimer != -1)
        {
            if (endCutscene != null)
            {
                EventHandler.MissionSelectTrigger(endCutscene());
            }
            else
            {
                EventHandler.MissionSelectTrigger(new MissionSelect());
            }
        }
    }
}
