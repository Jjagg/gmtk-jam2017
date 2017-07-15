using gmtk_jam.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace gmtk_jam
{
    public class ParallaxingBackground
    {
        public Camera Camera { get; }
        public RectangleF Rect { get; }
        public Texture2D Texture { get; }
        public float MultX { get; }
        public float MultY { get; }

        private readonly Sprite _sprite;

        public ParallaxingBackground(Camera camera, RectangleF rect, Texture2D texture, float multX = 0, float multY = 0)
        {
            Camera = camera;
            Rect = rect;
            Texture = texture;
            MultX = multX;
            MultY = multY;

            _sprite = new Sprite(texture);
        }

        public void Draw(Batcher2D batcher)
        {
            SetUvs();
            batcher.FillRect(Rect, _sprite);
        }

        private void SetUvs()
        {
            var uv = Camera.Position * new Vector2(MultX, MultY);
            _sprite.UV1 = uv;
            _sprite.UV2 = uv + Vector2.UnitX;
            _sprite.UV3 = uv + Vector2.One;
            _sprite.UV4 = uv + Vector2.UnitY;
        }
    }
}