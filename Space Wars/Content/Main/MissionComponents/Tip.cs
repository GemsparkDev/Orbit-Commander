using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Particles;

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
    public IMissionComponent Clone()
    {
        return new Tip(tip, _position);
    }
}
