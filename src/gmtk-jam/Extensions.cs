using System.Collections.Generic;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace gmtk_jam
{
    public static class Extensions
    {
        public static RectangleF ToRectangleF(this Rectangle rect)
        {
            return new RectangleF(rect.Location.ToVector2(), rect.Size.ToVector2());
        }

        public static RectangleF ToSimUnits(this RectangleF rect)
        {
            return new RectangleF(
                ConvertUnits.ToSimUnits(rect.Position),
                ConvertUnits.ToSimUnits(rect.Size));
        }

        public static RectangleF ToDisplayUnits(this RectangleF rect)
        {
            return new RectangleF(
                ConvertUnits.ToDisplayUnits(rect.Position),
                ConvertUnits.ToDisplayUnits(rect.Size));
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