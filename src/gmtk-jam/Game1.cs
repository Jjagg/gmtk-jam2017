﻿using GameSandbox;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace gmtk_jam
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Batcher2D _batcher;

        private Camera _camera;
        private Tommy _tommy;
        private Mountain _mountain;
        
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            var input = new Input(this);
            Components.Add(input);

            _camera = new Camera(GraphicsDevice);

            _tommy = new Tommy();
            _tommy.Position = new Vector2(100f);

            _mountain = new Mountain(_camera);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _batcher = new Batcher2D(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (Input.KeyDown(Keys.Escape))
                Exit();

            _tommy.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _tommy.Draw(_batcher);
            _mountain.Draw(_batcher);

            base.Draw(gameTime);
        }
    }
}
