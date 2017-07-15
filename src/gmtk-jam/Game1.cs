using System;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using gmtk_jam.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace gmtk_jam
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _sb;
        private Batcher2D _batcher;

        private BasicCamera _camera;
        private Tommy _tommy;
        private Mountain _mountain;
        private HudBar _oxygenBar;

        // Physics
        private World _physicsWorld;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            Components.Add(new Input(this));

            _physicsWorld = new World(new Vector2(0, 10f));
            ConvertUnits.SetDisplayUnitToSimUnitRatio(64f);

            var width = GraphicsDevice.Viewport.Width;
            var height = GraphicsDevice.Viewport.Height;

            _camera = new BasicCamera(GraphicsDevice);
            _camera.MoveTo(new Vector2(width / 2f, height / 2f));

            _tommy = new Tommy(_physicsWorld);
            _tommy.Position = new Vector2(100f);

            _mountain = new Mountain(_physicsWorld, _camera);
            var barSize = new Vector2(40f, 100f);
            var oxygenBarPos = new Vector2(width - barSize.X * 2f, barSize.X);
            _oxygenBar = new HudBar(oxygenBarPos, barSize);
            _oxygenBar.FillColor = Color.Lime;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _sb = new SpriteBatch(GraphicsDevice);
            _batcher = new Batcher2D(GraphicsDevice);

            Assets.Load(Content);

#if DEBUG
#endif
            Components.Add(new FrameRateCounter(this));

            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            base.LoadContent();
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
            _tommy.Update(gameTime);
            var zoomTarget = Math.Min(1f, 3f / (_tommy.Velocity.X == 0 ? 1f : _tommy.Velocity.X));
            var z = MathHelper.Lerp(_camera.Zoom, zoomTarget, 0.02f);
            _camera.ZoomTo(z);
            _camera.MoveTo(_tommy.Position);
            _camera.OffsetScreen(new Vector2(0.3f, 0.2f));
            _mountain.Update();

            _physicsWorld.Step((float) gameTime.ElapsedGameTime.TotalSeconds);

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
            if (Input.KeyPressed(Keys.T))
                _physicsWorld.Enabled = !_physicsWorld.Enabled;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _batcher.CameraMatrix = _camera.Transform;
            _tommy.Draw(_batcher);
            _mountain.Draw(_batcher);

            DrawHud();

            _batcher.Flush();

            base.Draw(gameTime);
        }

        private void DrawHud()
        {
            _batcher.CameraMatrix = _camera.Projection;
            _oxygenBar.Draw(_batcher);

            _sb.Begin();

            _sb.DrawString(Assets.Font12, "Slightly Rounded Square", new Vector2(10f, 460f), Color.White);
#if DEBUG
            _sb.DrawString(Assets.Font12, $"Mountain points = {_mountain.PointCount}", new Vector2(10f, 450f), Color.White);
#endif

            _sb.End();
        }
    }
}
