using System;
 using System.Linq;
 using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace gmtk_jam
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private Batcher2D _batcher;

        private BasicCamera _camera;
        private Tommy _tommy;
        private Mountain _mountain;
        private HudBar _oxygenBar;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            var input = new Input(this);
            Components.Add(input);

            var width = GraphicsDevice.Viewport.Width;
            var height = GraphicsDevice.Viewport.Height;

            _camera = new BasicCamera(GraphicsDevice);
            _camera.MoveTo(new Vector2(width / 2f, height / 2f));

            _tommy = new Tommy();
            _tommy.Position = new Vector2(100f);

            _mountain = new Mountain(_camera);
            var barSize = new Vector2(40f, 100f);
            var oxygenBarPos = new Vector2(width - barSize.X * 2f, barSize.X);
            _oxygenBar = new HudBar(oxygenBarPos, barSize);
            _oxygenBar.FillColor = Color.Lime;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _batcher = new Batcher2D(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (Input.KeyDown(Keys.Escape))
                Exit();

#if DEBUG
            HandleCameraInput();
#endif

            _tommy.Update();

            base.Update(gameTime);
        }

        private void HandleCameraInput()
        {
            if (Input.KeyDown(Keys.D))
                _camera.Move(new Vector2(5f, 0));
            if (Input.KeyDown(Keys.A))
                _camera.Move(new Vector2(-5f, 0));
            if (Input.KeyDown(Keys.S))
                _camera.Move(new Vector2(0, 5f));
            if (Input.KeyDown(Keys.W))
                _camera.Move(new Vector2(0, -5f));

            if (Input.KeyDown(Keys.Q))
                _camera.Rotate(-0.01f);
            if (Input.KeyDown(Keys.E))
                _camera.Rotate(0.01f);

            if (Input.KeyDown(Keys.R))
                _camera.ZoomTo(_camera.Zoom + .01f);
            if (Input.KeyDown(Keys.F))
                _camera.ZoomTo(_camera.Zoom - .01f);


        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _batcher.CameraMatrix = _camera.Transform;
            _tommy.Draw(_batcher);
            _mountain.Draw(_batcher);
            _oxygenBar.Draw(_batcher);

            _batcher.Flush();

            var pts = _tommy.Points().ToList();

            _batcher.DrawLine(pts[0], pts[1], Color.AliceBlue, 2);
            _batcher.DrawLine(pts[1], pts[2], Color.AliceBlue, 2);
            _batcher.DrawLine(pts[2], pts[3], Color.AliceBlue, 2);
            _batcher.DrawLine(pts[3], pts[0], Color.AliceBlue, 2);

            base.Draw(gameTime);
        }
    }
}
