using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Space_Wars.Content.Main
{
    internal abstract class Module
    {
        public int health;
        public string name;
        public Texture2D icon;

        public static void Update()
        {

        }
    }
}
