using gmtk_jam.Rendering;
using Microsoft.Xna.Framework;

namespace gmtk_jam
{
    public class HudBar
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public Color BorderColor { get; set; }
        public Color FillColor { get; set; }
        public int BorderThickness { get; set; } = 4;
        private int HalfBorderThickness => BorderThickness / 2;

        public HudBar(Vector2 pos, Vector2 size)
        {
            Position = pos;
            Size = size;
        }

        public void Draw(Batcher2D batcher)
        {
            var rect = new Rectangle(Position.ToPoint(), Size.ToPoint());
            var borderRect = rect;
            borderRect.Inflate(HalfBorderThickness, HalfBorderThickness);

            batcher.DrawRect(borderRect, BorderColor, BorderThickness);
            batcher.FillRect(rect, FillColor);
        }
    }
}