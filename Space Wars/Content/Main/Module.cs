using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Space_Wars.Content.Main
{
    internal struct Module
    {
        public float Health;
        public readonly float[] Cost;
        public readonly Texture2D Texture;
        public readonly string Name;

        public Module(float health, float[] cost, Texture2D texture, string name)
        {
            Health = health;
            Cost = cost;
            Texture = texture;
            Name = name;
        }
    }
}
