using System;
using System.Collections.Generic;
using System.Diagnostics;
using FarseerPhysics;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace gmtk_jam
{
    public static class Util
    {
        // This method is modified from an implementation in Farseer Physics
        // original implementation was contributed by Jonathan Smars - jsmars@gmail.com
        // code is licensed under Microsoft Permissive License (Ms-PL)
        public static List<Vector2> CreateRoundedRectangle(float width, float height, float xRadius, float yRadius, int segments)
        {
            if (yRadius > height / 2 || xRadius > width / 2)
                throw new Exception("Rounding amount can't be more than half the height and width respectively.");
            if (segments < 0)
                throw new Exception("Segments must be zero or more.");

            //We need at least 8 vertices to create a rounded rectangle
            Debug.Assert(Settings.MaxPolygonVertices >= 8);

            var vertices = new List<Vector2>();
            if (segments == 0)
            {
                vertices.Add(new Vector2(width * .5f - xRadius, -height * .5f));
                vertices.Add(new Vector2(width * .5f, -height * .5f + yRadius));

                vertices.Add(new Vector2(width * .5f, height * .5f - yRadius));
                vertices.Add(new Vector2(width * .5f - xRadius, height * .5f));

                vertices.Add(new Vector2(-width * .5f + xRadius, height * .5f));
                vertices.Add(new Vector2(-width * .5f, height * .5f - yRadius));

                vertices.Add(new Vector2(-width * .5f, -height * .5f + yRadius));
                vertices.Add(new Vector2(-width * .5f + xRadius, -height * .5f));
            }
            else
            {
                int numberOfEdges = (segments * 4 + 8);

                float stepSize = MathHelper.TwoPi / (numberOfEdges - 4);
                int perPhase = numberOfEdges / 4;

                Vector2 posOffset = new Vector2(width / 2 - xRadius, height / 2 - yRadius);
                vertices.Add(posOffset + new Vector2(xRadius, -yRadius + yRadius));
                short phase = 0;
                for (int i = 1; i < numberOfEdges; i++)
                {
                    if (i - perPhase == 0 || i - perPhase * 3 == 0)
                    {
                        posOffset.X *= -1;
                        phase--;
                    }
                    else if (i - perPhase * 2 == 0)
                    {
                        posOffset.Y *= -1;
                        phase--;
                    }

                    vertices.Add(posOffset + new Vector2(xRadius * (float)Math.Cos(stepSize * -(i + phase)),
                                                         -yRadius * (float)Math.Sin(stepSize * -(i + phase))));
                }
            }

            return vertices;
}
    }
}