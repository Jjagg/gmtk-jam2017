using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace gmtk_jam
{
    /// <summary>
    /// A camera that can be used to manipulate display.
    /// </summary>
    public abstract class Camera
    {
        private readonly GraphicsDevice _gd;
        protected Rectangle Viewport => _gd.Viewport.Bounds;

        /// <summary>
        /// The size of the viewport.
        /// </summary>
        protected Vector2 ScreenSize => Viewport.Size.ToVector2();

        /// <summary>
        /// Position of the camera.
        /// </summary>
        public abstract Vector2 Position { get; }

        /// <summary>
        /// Rotation of the camera in radians clockwise.
        /// </summary>
        public abstract float Rotation { get; }

        /// <summary>
        /// The zoom factor of the camera.
        /// </summary>
        public abstract float Zoom { get; }

        /// <summary>
        /// Creates a new Camera.
        /// </summary>
        protected Camera(GraphicsDevice gd)
        {
            _gd = gd;
            Projection = Matrix.CreateOrthographicOffCenter(0, Width, Height, 0, 0, 1);
        }

        /// <summary>
        /// The width of the screen.
        /// </summary>
        public float Width => Viewport.Width;

        /// <summary>
        /// The height of the screen.
        /// </summary>
        public float Height => Viewport.Height;

        /// <summary>
        /// Half of <see cref="Width"/>
        /// </summary>
        public float HalfWidth => Viewport.Width * 0.5f;

        /// <summary>
        /// Half of <see cref="Height"/>
        /// </summary>
        public float HalfHeight => Viewport.Height * 0.5f;

        public Matrix Transform => View * Projection;
        public Matrix InvTransform => Matrix.Invert(Transform);

        public abstract Matrix View { get; }
        public Matrix Projection { get; set; }

        /// <summary>
        /// Inverse matrix of <see cref="View"/>
        /// <seealso cref="Matrix.Invert(Matrix)"/>
        /// </summary>
        public virtual Matrix InvView => Matrix.Invert(View);

        /// <summary>
        /// Get the world-space coordinates of the given vector in screen-space coordinates.
        /// </summary>
        /// <returns>A new vector with the world-space coordinates</returns>
        public Vector2 ScreenToWorld(Vector2 v) {
            return Vector2.Transform(v, InvView);
        }

        /// <summary>
        /// Get the screen-space coordinates of the given vector in world-space coordinates.
        /// </summary>
        /// <returns>A new vector with the screen-space coordinates</returns>
        public Vector2 WorldToScreen(Vector2 v) {
            return Vector2.Transform(v, View);
        }

        /// <summary>
        /// The smallest rectangle containing this camera.
        /// </summary>
        public Rectangle BoundingRect
        {
            get {
                var screenSize = ScreenSize;
                var tl = ScreenToWorld(Vector2.Zero);
                var tr = ScreenToWorld(new Vector2(screenSize.X, 0));
                var bl = ScreenToWorld(new Vector2(0, screenSize.Y));
                var br = ScreenToWorld(screenSize);
                var min = new Vector2(
                    Math.Min(tl.X, Math.Min(tr.X, Math.Min(bl.X, br.X))),
                    Math.Min(tl.Y, Math.Min(tr.Y, Math.Min(bl.Y, br.Y))));
                var max = new Vector2(
                    Math.Max(tl.X, Math.Max(tr.X, Math.Max(bl.X, br.X))),
                    Math.Max(tl.Y, Math.Max(tr.Y, Math.Max(bl.Y, br.Y))));
                return new Rectangle((int) min.X, (int) min.Y,
                    (int) (max.X - min.X), (int) (max.Y - min.Y));
            }
        }
    }
}