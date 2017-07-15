using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace gmtk_jam.Extended
{
    public class ExtendedUtil
    {
        public static List<Vector2> CreateCircle(Vector2 center, double radius, int sides, float start = 0f,
            float end = MathHelper.TwoPi)
        {
            return CreateCircle(center, radius, radius, sides, start, end);
        }

        public static List<Vector2> CreateCircle(Vector2 center, double xRadius, double yRadius, int sides, float start = 0f, float end = MathHelper.TwoPi)
        {
            if (start > end)
            {
                var tmp = start;
                start = end;
                end = tmp;
            }
            var step = (end - start) / sides;
            var theta = start;

            var ps = new List<Vector2>();
            for (var i = 0; i < sides; i++)
            {
                ps.Add(center + new Vector2((float) (xRadius*Math.Cos(theta)), (float) (yRadius*Math.Sin(theta))));
                theta += step;
            }
            ps.Add(center + new Vector2((float) (xRadius*Math.Cos(end)), (float) (yRadius*Math.Sin(end))));
            return ps;
        }
    }
}
