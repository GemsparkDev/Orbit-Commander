using Microsoft.Xna.Framework.Graphics;

namespace Space_Wars.Content.Main.MissionComponents;
public interface IMissionComponent
{
    public void Initialize();
    public void Update();
    public void Draw(SpriteBatch _spriteBatch);

}
