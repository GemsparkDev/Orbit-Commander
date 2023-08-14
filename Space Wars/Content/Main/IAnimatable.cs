using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Wars.Content.Main
{
    public interface IAnimatable
    {
        public abstract Texture2D SpriteSheet { get; set; }
        public abstract Vector2 TextureDimensions { get; set; }
        public abstract int currentFrame { get; set; }
        public abstract float TimePerFrame { get; set; }
        public abstract float CurrentFrameTime { get; set; }

        public abstract void UpdateSpriteTime();
    }
}
