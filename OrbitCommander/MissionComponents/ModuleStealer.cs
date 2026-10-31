using Microsoft.Xna.Framework.Graphics;
using OrbitCommander.Core;
using OrbitCommander.Entities;
using Microsoft.Xna.Framework;
using OrbitCommander.Components;

namespace OrbitCommander.MissionComponents;
public class ModuleStealer : IMissionComponent
{
    public void Draw(SpriteBatch _spriteBatch) { }

    public void Initialize()
    {
        if (Engine.SaveGame.StolenModule != null)
        {
            Entity v;
            Engine.SaveGame.CurrentMission.Add(v = Entity.NewFighter(new Vector2(0, 1000), Vector2.Zero, 0));
            v.GetComponent<Behaviour>().AddBehaviour(v.DropItem(delegate(Vector2 _position, Vector2 _velocity, float _angle)
            {
                var m = Engine.SaveGame.StolenModule;
                Engine.SaveGame.StolenModule = null;
                m.Position = _position;
                m.Velocity = _velocity;
                return m;
            }));
        }
    }

    public void Update() { }
}
