using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
        public const float LineStep = 1f;

        public int Seed { get; set; } = 42;

        private readonly World _world;
        private Body _body;

        private readonly LinkedList<Vector2> _points;
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

            _points = new LinkedList<Vector2>();
            _points.AddLast(ConvertUnits.ToSimUnits(new Vector2(-500, 350)));
            _points.AddLast(ConvertUnits.ToSimUnits(new Vector2(-5, 350)));

            _camera = camera;

            _rand = new Random(Seed);

            FillPoints(ConvertUnits.ToSimUnits(camera.BoundingRect.Right));
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
            Debug.Assert(_points.First.Next != null, "At least two elements");
            while (_points.Count > 1 && _points.First.Next.Value.X < until)
                _points.RemoveFirst();
        }

        private void FillPoints(float to)
        {
            while (_points.Last.Value.X < to)
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

            var lastPoint = _points.Last.Value;
            _points.AddLast(lastPoint + new Vector2(LineStep, LineStep * _derivative));
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

    internal struct Decoration
    {
        public Vector2 Location { get; }
        public int DecorationType { get; }

        public float X => Location.X;
        public float Y => Location.Y;

        public Decoration(Vector2 location, int decorationType)
        {
            Location = location;
            DecorationType = decorationType;
        }
    }
}
