using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace gmtk_jam.Extended
{
    public class ExtendedUtil
    {
        public static IEnumerable<Vector2> CreateCircle(Vector2 center, double radius, int sides, float start = 0f, float end = MathHelper.TwoPi)
        {
            var step = end / sides;
            var theta = start;

            for (var i = 0; i < sides; i++)
            {
                yield return center + new Vector2((float) (radius*Math.Cos(theta)), (float) (radius*Math.Sin(theta)));
                theta += step;
            }
        }
    }
}
