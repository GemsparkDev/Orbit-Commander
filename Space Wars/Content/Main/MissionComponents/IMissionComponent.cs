using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Wars.Content.Main.MissionComponents;
public interface IMissionComponent
{
    public void Initialize();
    public void Update();
    public void Draw(SpriteBatch _spriteBatch);
    
}
