using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using gmtk_jam.Rendering;
using Microsoft.Xna.Framework;

namespace gmtk_jam
{
    public class Mountain
    {
        public const int LineStep = 15;

        private readonly World _world;
        private Body _body;

        private readonly List<Vector2> _points;
        private readonly Camera _camera;

        private readonly Random _rand;
        private float _derivative = .3f;

        public float MinDerivative { get; set; } = 0f;
        public float MaxDerivative { get; set; } = 1f;
        public float DerivativeChangeRate { get; set; } = 0.1f;
        public float DiscontinuityChance { get; set; } = 0.02f;

        public Mountain(World world, Camera camera)
        {
            _world = world;
            _points = new List<Vector2>();
            _points.Add(new Vector2(-5, 350));
            _camera = camera;

            _rand = new Random();

            FillPoints();
            CreateBody();
        }

        public void Update()
        {
            RemovePoints();
            FillPoints();
            CreateBody();
        }

        private void RemovePoints()
        {
            while (_points.Count > 1 && _points[1].X < _camera.Left)
                _points.RemoveAt(0);
        }

        private void FillPoints()
        {
            while (_points[_points.Count - 1].X < _camera.Right)
                AddPoint();
        }

        private void AddPoint()
        {
            if (_rand.NextDouble() < DiscontinuityChance)
            {
                _derivative = (float) _rand.NextDouble() * (MaxDerivative - MinDerivative) + MinDerivative;
            }
            else
            {
                var diff = (float) (_rand.NextDouble() - .5) * DerivativeChangeRate;
                _derivative = MathHelper.Clamp(_derivative + diff, MinDerivative, MaxDerivative);
            }

            var lastPoint = _points[_points.Count - 1];
            _points.Add(lastPoint + new Vector2(LineStep, LineStep * _derivative));
        }

        private void CreateBody()
        {
            _body?.Dispose();

            // todo two lists for efficiency
            var vertices = new Vertices(_points.Select(ConvertUnits.ToSimUnits));
            _body = BodyFactory.CreateChainShape(_world, vertices);
            
            _body.CollisionCategories = Physics.MountainCategory;
            _body.CollidesWith = Category.All;
        }

        public void Draw(Batcher2D batcher)
        {
            batcher.DrawLines(_points, Color.LawnGreen, 2);
        }
    }
}