using System;
using Microsoft.Xna.Framework;

namespace gmtk_jam.Extended
{
    public class Util
    {
        public static Vector2[] CreateCircle(Vector2 center, double radius, int sides)
        {
            const double max = MathHelper.TwoPi;
            var points = new Vector2[sides];
            var step = max / sides;
            var theta = 0.0;

            for (var i = 0; i < sides; i++)
            {
                points[i] = center + new Vector2((float) (radius*Math.Cos(theta)), (float) (radius*Math.Sin(theta)));
                theta += step;
            }

            return points;
        }
    }
}
