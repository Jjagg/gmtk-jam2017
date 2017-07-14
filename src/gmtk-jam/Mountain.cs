using System;
using System.Collections.Generic;
using GameSandbox;
using Microsoft.Xna.Framework;

namespace gmtk_jam
{
    public class Mountain
    {
        public const int LineStep = 10;

        private readonly List<Vector2> _points;
        private readonly Camera _camera;

        private readonly Random _rand;
        private float _derivative = .3f;

        public Mountain(Camera camera)
        {
            _points = new List<Vector2>();
            _points.Add(new Vector2(-5, 350));
            _camera = camera;

            _rand = new Random();

            FillPoints();
        }

        public void Update()
        {
            RemovePoints();
            FillPoints();
        }

        private void RemovePoints()
        {
            while (_points.Count > 1 && _points[1].X < 0)
                _points.RemoveAt(0);
        }

        private void FillPoints()
        {
            while (_points[_points.Count - 1].X < _camera.Right)
                AddPoint();
        }

        private void AddPoint()
        {
            var diff = (float) (_rand.NextDouble() - .5) * 0.1f;
            _derivative = MathHelper.Clamp(_derivative + diff, -.1f, .3f);

            var lastPoint = _points[_points.Count - 1];
            _points.Add(lastPoint + new Vector2(LineStep, LineStep * _derivative));
        }

        public void Draw(Batcher2D batcher)
        {
            batcher.DrawLines(_points, Color.LawnGreen, 2);
        }
    }
}