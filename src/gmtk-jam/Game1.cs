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

        private const float BackSceneryScreenHeight = 200f;
        private const float MiddleSceneryScreenHeight = 250f;
        private const float FrontSceneryScreenHeight = 300f;
        private const float BackSceneryMul = .00005f;
        private const float MiddleSceneryMul = .0001f;
        private const float FrontSceneryMul = .0002f;
        private ParallaxingBackground _backScenery;
        private ParallaxingBackground _middleScenery;
        private ParallaxingBackground _frontScenery;

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
            _tommy.Position = ConvertUnits.ToSimUnits(new Vector2(100f));

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
            Components.Add(new FrameRateCounter(this));
#endif

            var w = GraphicsDevice.Viewport.Width;
            var bh = Assets.Scenery1.Height;
            var mh = Assets.Scenery2.Height;
            var fh = Assets.Scenery3.Height;
            _backScenery = new ParallaxingBackground(_camera, new RectangleF(0, BackSceneryScreenHeight, w, bh), Assets.Scenery1, BackSceneryMul);
            _middleScenery = new ParallaxingBackground(_camera, new RectangleF(0, MiddleSceneryScreenHeight, w, mh), Assets.Scenery2, MiddleSceneryMul);
            _frontScenery = new ParallaxingBackground(_camera, new RectangleF(0, FrontSceneryScreenHeight, w, fh), Assets.Scenery3, FrontSceneryMul);

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
            var zoomTarget = Math.Min(1f, 3f / (_tommy.Velocity.X < 0.001 ? 1f : _tommy.Velocity.X));
            var z = MathHelper.Clamp(MathHelper.Lerp(_camera.Zoom, zoomTarget, 0.002f), 0.1f, 1f);
            _camera.ZoomTo(z);
            _camera.MoveTo(ConvertUnits.ToDisplayUnits(_tommy.Position));
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

            DrawScenery();

            _batcher.CameraMatrix = _camera.Transform;
            _mountain.Draw(_batcher);
            _tommy.Draw(_batcher);

            DrawHud();

            _batcher.Flush();

            base.Draw(gameTime);
        }

        private void DrawScenery()
        {
            _batcher.CameraMatrix = _camera.Projection;
            DrawSky();
            _backScenery.Draw(_batcher);
            _middleScenery.Draw(_batcher);
            _frontScenery.Draw(_batcher);
        }

        private readonly Color[] _skyColors =
        {
            new Color(0xb1, 0xe3, 0xff),
            new Color(0xb1, 0xf3, 0xff),
            new Color(0x11, 0x99, 0xd4),
            new Color(0xee, 0x99, 0xd4)
        };

        private void DrawSky()
        {
            var width = GraphicsDevice.Viewport.Width;
            var height = GraphicsDevice.Viewport.Height;
            _batcher.FillRect(new RectangleF(0, 0, width, height),
                _skyColors[0], _skyColors[1], _skyColors[2], _skyColors[3]);
        }
        private void DrawHud()
        {
            _batcher.CameraMatrix = _camera.Projection;
            _oxygenBar.Draw(_batcher);

            _sb.Begin();

            _sb.DrawString(Assets.Font12, "Slightly Rounded Square", new Vector2(10f, 460f), Color.White);
#if DEBUG
            _sb.DrawString(Assets.Font12, $"Tommy Position = {_tommy.Position}", new Vector2(10f, 390f), Color.White);
            _sb.DrawString(Assets.Font12, $"Tommy Speed = {_tommy.Velocity}", new Vector2(10f, 400f), Color.White);
            _sb.DrawString(Assets.Font12, $"Tommy Size = {_tommy.Size}", new Vector2(10f, 410f), Color.White);
            _sb.DrawString(Assets.Font12, $"Tommy Target Size = {_tommy.TargetSize}", new Vector2(10f, 420f), Color.White);
            _sb.DrawString(Assets.Font12, $"Tommy t = {_tommy._sizeT}", new Vector2(10f, 430f), Color.White);
            _sb.DrawString(Assets.Font12, $"Cap = {_tommy.CurrentCapacity}", new Vector2(10f, 440f), Color.White);
            _sb.DrawString(Assets.Font12, $"Mountain points = {_mountain.PointCount}", new Vector2(10f, 450f), Color.White);
#endif

            _sb.End();
        }

    }
}
