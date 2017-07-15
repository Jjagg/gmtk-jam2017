﻿using System;
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
        public const float LineStep = .25f;

        private readonly World _world;
        private Body _body;

        private readonly List<Vector2> _points;
        private readonly Camera _camera;

        private readonly Random _rand;
        private float _derivative = .2f;

        public float MinDerivative { get; set; } = 0.2f;
        public float MaxDerivative { get; set; } = .8f;
        public float DerivativeChangeRate { get; set; } = 0.07f;
        public float DiscontinuityChance { get; set; } = 0.02f;

        public int PointCount => _points.Count;

        public Mountain(World world, Camera camera)
        {
            _world = world;
            _points = new List<Vector2>();
            _points.Add(ConvertUnits.ToSimUnits(new Vector2(-500, 350)));
            _points.Add(ConvertUnits.ToSimUnits(new Vector2(-5, 350)));
            _camera = camera;

            _rand = new Random();

            FillPoints(ConvertUnits.ToSimUnits(500));
            CreateBody();
        }

        public void Update()
        {
            var rect = _camera.BoundingRect.ToRectangleF().ToSimUnits();
            RemovePoints(rect.Left);
            FillPoints(rect.Right);
            CreateBody();
        }

        private void RemovePoints(float until)
        {
            while (_points.Count > 1 && _points[1].X < until)
                _points.RemoveAt(0);
        }

        private void FillPoints(float to)
        {
            while (_points[_points.Count - 1].X < to)
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
            var vertices = new Vertices(_points);
            _body = BodyFactory.CreateChainShape(_world, vertices);
            _body.Friction = 0.8f;
            _body.Restitution = 0.05f;
            
            _body.CollisionCategories = Physics.MountainCategory;
            _body.CollidesWith = Category.All;
        }

        public void Draw(Batcher2D batcher)
        {
            var ps = _points.Select(ConvertUnits.ToDisplayUnits).ToArray();
            batcher.DrawLineStrip(ps, Color.Black, 4);
        }
    }
}
