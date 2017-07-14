using Microsoft.Xna.Framework;

namespace gmtk_jam
{
    public struct LineSegment
    {
        public LineSegment(Vector2 p1, Vector2 p2)
        {
            P1 = p1;
            P2 = p2;
        }

        public Vector2 P1 { get; }
        public Vector2 P2 { get; }
    }
}