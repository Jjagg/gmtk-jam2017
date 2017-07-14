using Microsoft.Xna.Framework;

namespace gmtk_jam
{
    public class OxygenBar
    {
        private const int BorderThickness = 4;
        private const int HalfBorderThickness = BorderThickness / 2;
        private const int InitialSize = 100;
        private const int Width = 100;

        private int _size = InitialSize;

        public OxygenBar()
        {
        }

        public void Update()
        {
        }

        public void Draw(Vector2 position, Batcher2D batcher)
        {
            var rect = new Rectangle(position.ToPoint(), new Point(Width, _size));
            var borderRect = rect;
            borderRect.Inflate(HalfBorderThickness, HalfBorderThickness);

            batcher.DrawRect(rect, Color.Black, BorderThickness);
            batcher.FillRect(borderRect, Color.Black);
        }
    }
}