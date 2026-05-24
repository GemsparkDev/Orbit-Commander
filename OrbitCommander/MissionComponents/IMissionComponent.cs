using Microsoft.Xna.Framework.Graphics;

namespace OrbitCommander.MissionComponents;
public interface IMissionComponent
{
    public void Initialize();
    public void Update();
    public void Draw(SpriteBatch _spriteBatch);

}
