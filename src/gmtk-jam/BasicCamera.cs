using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace gmtk_jam
{
    /// <summary>
    /// A simple Camera with operations for moving, rotating and zooming.
    /// </summary>
    public sealed class BasicCamera : Camera
    {
        private Vector2 _position = Vector2.Zero;
        private float _rotation;
        private float _zoom;

        /// <summary>
        /// Position of the camera.
        /// </summary>
        public override Vector2 Position => _position;

        /// <summary>
        /// Rotation of the camera in radians clockwise.
        /// </summary>
        public override float Rotation => _rotation;

        /// <summary>
        /// The zoom factor of the camera.
        /// </summary>
        public override float Zoom => _zoom;

        public override Matrix View =>
                Matrix.CreateTranslation(new Vector3(-Position, 0.0f))*
                Matrix.CreateRotationZ(-Rotation)*
                Matrix.CreateScale(Zoom, Zoom, 1)*
                Matrix.CreateTranslation(new Vector3(HalfWidth, HalfHeight, 0.0f));

        /// <summary>
        /// Creates a new BasicCamera.
        /// </summary>
        public BasicCamera(GraphicsDevice gd) : base(gd) {
            _zoom = 1f;
        }

        /// <summary>
        /// Move the camera by the given amount.
        /// </summary>
        /// <param name="by">Pixels to move by.</param>
        public void Move(Vector2 by) {
            _position += by;
        }

        /// <summary>
        /// Move the camera to the given position.
        /// </summary>
        /// <param name="v">The position to move to.</param>
        public void MoveTo(Vector2 v) {
            _position = v;
        }

        /// <summary>
        /// Rotate the camera clockwise by the given angle.
        /// </summary>
        /// <param name="r">Angle to rotate in radians.</param>
        public void Rotate(float r) {
            _rotation += r;
        }

        /// <summary>
        /// Set the camera rotation to the given angle.
        /// </summary>
        /// <param name="r">Angle to set rotation to in radians.</param>
        public void RotateTo(float r) {
            _rotation = r;
        }

        /// <summary>
        /// Zoom the camera by the given factor.
        /// </summary>
        /// <param name="z">Factor to zoom by.</param>
        public void ZoomBy(float z) {
            _zoom *= z;
        }

        /// <summary>
        /// Set the zoom to the given factor.
        /// </summary>
        /// <param name="z">Factor to set the zoom to.</param>
        public void ZoomTo(float z) {
            _zoom = z;
        }
    }
}