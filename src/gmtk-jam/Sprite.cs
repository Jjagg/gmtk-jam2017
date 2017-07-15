using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace gmtk_jam
{
    public class Sprite
    {
        public Texture2D Texture { get; }
        public Vector2 UV1 { get; set; }
        public Vector2 UV2 { get; set; }
        public Vector2 UV3 { get; set; }
        public Vector2 UV4 { get; set; }

        public Sprite(Texture2D texture, Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4)
        {
            Texture = texture;
            UV1 = uv1;
            UV2 = uv2;
            UV3 = uv3;
            UV4 = uv4;
        }

        public Sprite(Texture2D texture)
            : this (texture, Vector2.Zero, Vector2.UnitX, Vector2.One, Vector2.UnitY)
        {
        }

        public static implicit operator Sprite(Texture2D texture)
        {
            return new Sprite(texture);
        }
    }
}
