using System.Collections.Generic;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace gmtk_jam
{
    public static class Extensions
    {
        public static RectangleF ToDisplayUnits(this RectangleF rect)
        {
            var center = rect.Center;
            var halfExtents = new Vector2(rect.Size.X / 2f, rect.Size.Y / 2f);
            halfExtents = ConvertUnits.ToDisplayUnits(halfExtents);
            var p = halfExtents;
            var p2 = (2 * halfExtents);
            return new RectangleF(center - p, p2);
        }

        /// <summary>
        /// Wraps this object instance into an IEnumerable&lt;T&gt;
        /// consisting of a single item.
        /// </summary>
        /// <typeparam name="T"> Type of the object. </typeparam>
        /// <param name="item"> The instance that will be wrapped. </param>
        /// <returns> An IEnumerable&lt;T&gt; consisting of a single item. </returns>
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }
    }
}