using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Space_Wars.Content.Main.MissionComponents;
internal class Tip(string tip, Vector2 _position) : IMissionComponent
{
    public void Draw(SpriteBatch _spriteBatch)
    {
    }
    public void Initialize()
    {
    }
    public void Update()
    {
        if (tip != null)
        {
            ParticleManager.Add(new Particle(null, tip.Length, _position, Vector2.Zero, 0, 0, Color.White, Color.Transparent) { drawText = tip });
            tip = null;
        }
    }
}
