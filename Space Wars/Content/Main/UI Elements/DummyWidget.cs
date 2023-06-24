using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

namespace Space_Wars.Content.Main.UI_Elements
{
    public class DummyWidget : Widget, IFunctional
    {
        public new Vector2 Size
        {
            get { return Vector2.Zero; }
        }
        public DummyWidget()
        {
            Offset = Vector2.Zero;
            Texture = null;
        }
        public void Interact() {}
        public void AddBehaviour(IEnumerable<int> behaviour) {}
        public void ApplyBehaviours() {}
        public override void Draw(SpriteBatch spriteBatch, Vector2 parentPositon) { }
    }
}
