using Microsoft.Xna.Framework.Graphics;

namespace gmtk_jam
{
    public class Camera
    {
        private readonly GraphicsDevice _gd;

        public double Right => _gd.Viewport.Bounds.Right;

        public Camera(GraphicsDevice gd)
        {
            _gd = gd;
        }
    }
}