using System.Diagnostics.Contracts;
using Microsoft.Xna.Framework;

namespace gmtk_jam
{
    public class RectangleF
    {
        private Rectangle boundingRect;

        public Vector2 Position { get; }
        public Vector2 Size { get; }

        public float Left => Position.X;
        public float Top => Position.Y;
        public float Right => Position.X + Size.X;
        public float Bottom => Position.Y + Size.Y;

        public Vector2 TopLeft => Position;
        public Vector2 TopRight => new Vector2(Position.X + Size.X, Position.Y);
        public Vector2 BottomLeft => new Vector2(Position.X, Position.Y + Size.Y);
        public Vector2 BottomRight => Position + Size;

        public Vector2 Center => Position + Size / 2f;

        public float X => Position.X;
        public float Y => Position.Y;
        public float Width => Size.X;
        public float Height => Size.Y;

        public RectangleF(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }

        public RectangleF(float x, float y, float width, float height)
        {
            Position = new Vector2(x, y);
            Size = new Vector2(width, height);
        }

        public RectangleF(Rectangle boundingRect)
        {
            this.boundingRect = boundingRect;
        }

        [Pure]
        public RectangleF Inflate(float x, float y)
        {
            var halfExtents = new Vector2((Size.X + x) / 2f, (Size.Y + y) / 2f);
            return new RectangleF(Center - halfExtents, 2 * halfExtents);
        }

        public Rectangle ToRectangle()
        {
            return new Rectangle(Position.ToPoint(), Size.ToPoint());
        }
    }
}