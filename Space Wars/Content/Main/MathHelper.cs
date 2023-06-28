using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Space_Wars.Content.Main
{
    public static class MathHelper
    {
        //Converts a vector 2 to degrees in radians
        public static float ToDirection(this Vector2 vector, float offset)
        {
            return MathF.Atan2(vector.X, -vector.Y) - MathF.PI/2 + offset;
        }

        //Converts an angle in radians to a corresponding vector 2 on the unit circle
        public static Vector2 ToUnitVector(this float angle, float offset)
        {
            return new Vector2(MathF.Cos(angle + offset), MathF.Sin(angle + offset));
        }

        //Converts a vector2 to the corresponding vector 2 on the unit circle
        public static Vector2 ToUnitVector(this Vector2 vector, float offset)
        {
            float angle;
            angle = MathF.Atan2(vector.X, -vector.Y) - MathF.PI/2 + offset;
            return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        }
    }
}
