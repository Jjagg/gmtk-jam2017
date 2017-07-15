using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace gmtk_jam
{
    public class SpriteSheet
    {
        public Texture2D Texture { get; }
        public int Rows { get; }
        public int Cols { get; }

        public SpriteSheet(Texture2D texture, int cols, int rows)
        {
            Texture = texture;
            Cols = cols;
            Rows = rows;
        }

        public Sprite GetSprite(int number)
        {
            return GetSprite(number % Cols, number / Cols);
        }

        public Sprite GetSprite(int row, int col)
        {
            var w = 1.0f / Rows;
            var h = 1.0f / Cols;

            var uv1 = new Vector2(col * w, row * h);
            var uv2 = new Vector2((col+1) * w, row * h);
            var uv3 = new Vector2((col+1) * w, (row+1) * h);
            var uv4 = new Vector2(col * w, (row+1) * h);

            return new Sprite(Texture, uv1, uv2, uv3, uv4);
        }
    }
}
