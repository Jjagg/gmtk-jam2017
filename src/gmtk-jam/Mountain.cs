using System;
using System.Collections;
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
        public static readonly float LineStep = .5f;

        private readonly World _world;
        private readonly Camera _camera;

        private Body _body;

        private readonly RollingPoints _points;
        private readonly RollingPlants _plants;

        public int PointCount => _points.Count;

        public Mountain(World world, Camera camera, int seed = 42)
        {
            _world = world;
            _camera = camera;
            _points = new RollingPoints(seed);
            _plants = new RollingPlants(_points, seed);

            Update(true);
        }

        public void Update(bool forceBodyUpdate = false)
        {
            var rect = _camera.BoundingRect.ToRectangleF().ToSimUnits();
            if (_points.Update(rect.Left, rect.Right) || forceBodyUpdate)
            {
                _plants.Update(rect.Left, rect.Right);

                Console.WriteLine("Updating body!");
                CreateBody();
            }
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
            // bottom of the screen + a little margin
            var b = _camera.BoundingRect.Bottom + 10;
            var ps = _points.SelectMany(v => VerticalPair(v, b));
            batcher.FillTriangleStrip(ps, Color.Black);

            foreach (var plant in _plants)
                plant.Draw(batcher);
        }

        private IEnumerable<Vector2> VerticalPair(Vector2 v, float bottom)
        {
            var vd = ConvertUnits.ToDisplayUnits(v);
            // make sure we start clockwise because of backface culling
            yield return new Vector2(vd.X, bottom);
            yield return vd;
        }
    }

    internal abstract class RollingObjects<T> : IEnumerable<T>
    {
        protected readonly LinkedList<T> Objects;
        protected readonly float BufferZone;
        private readonly float _tightBufferZone;

        protected RollingObjects(float bufferZone)
        {
            Objects = new LinkedList<T>();
            BufferZone = bufferZone;
            _tightBufferZone = 2f;
        }

        public T Last() => Objects.Last.Value;
        public int Count => Objects.Count;

        public IEnumerator<T> GetEnumerator() => Objects.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) Objects).GetEnumerator();

        public bool Update(float left, float right)
        {
            if (!AddLast(right)) return false;   // side effects!
            RemoveFirst(left);
            return true;
        }

        private bool RemoveFirst(float left)
        {
            if (Objects.Count == 0)
                return false;
            if (GetX(Objects.First.Value) > left - BufferZone)
                return false;

            while (Objects.Count > 0 && GetX(Objects.First.Value) < left - _tightBufferZone)
                Objects.RemoveFirst();

            Console.WriteLine($"{GetType().Name}: removed objects: {Count}");

            return true;
        }

        private bool AddLast(float right)
        {
            if (Objects.Count > 0 && GetX(Objects.Last.Value) > right + _tightBufferZone)
                return false;

            var before = Count;

            while (WantNew(right))
                Objects.AddLast(Construct());

            if (Count > before)
                Console.WriteLine($"{GetType().Name}: new objects: {before}->{Count}");

            return true;
        }

        protected abstract bool WantNew(float right);
        protected abstract float GetX(T obj);
        protected abstract T Construct();
    }

    internal class RollingPoints : RollingObjects<Vector2>
    {
        private const float MinDerivative = .2f;
        private const float MaxDerivative = .5f;
        private const float DerivativeChangeRate = .07f;
        private const float DiscontinuityChance = .02f;

        private float _currentDerivative = .01f;

        private readonly Random _rand;

        public RollingPoints(int seed) : base(bufferZone: 10)
        {
            _rand = new Random(seed);
            Objects.AddLast(ConvertUnits.ToSimUnits(new Vector2(-500, 350)));
            Objects.AddLast(ConvertUnits.ToSimUnits(new Vector2(-5, 350)));
        }

        protected override bool WantNew(float right)
        {
            return Objects.Count == 0 || GetX(Objects.Last.Value) < right + BufferZone;
        }

        protected override float GetX(Vector2 point) => point.X;

        protected override Vector2 Construct()
        {
            if (_rand.NextDouble() < DiscontinuityChance)
            {
                _currentDerivative = (float) _rand.NextDouble() * (MaxDerivative - MinDerivative) + MinDerivative;
            }
            else
            {
                var diff = (float) (_rand.NextDouble() - .5) * DerivativeChangeRate;
                _currentDerivative = MathHelper.Clamp(_currentDerivative + diff, MinDerivative, MaxDerivative);
            }

            return Last() + new Vector2(Mountain.LineStep, Mountain.LineStep * _currentDerivative);
        }
    }

    internal class RollingPlants : RollingObjects<Plant>
    {
        private const double PlantProbability = .5;

        private readonly RollingPoints _points;
        private readonly Random _rand;
        private Vector2 _lastConsideredPoint;

        public RollingPlants(RollingPoints points, int seed) : base(bufferZone: 2)
        {
            _points = points;
            _rand = new Random(seed);
            _lastConsideredPoint = Vector2.Zero;
        }

        protected override float GetX(Plant dec) => dec.Position.X;

        protected override bool WantNew(float right)
        {
            if (_points.Count == 0)
                return false;

            var lastLastConsideredPoint = _lastConsideredPoint;
            _lastConsideredPoint = _points.Last();

            return lastLastConsideredPoint != _points.Last()
                   && _rand.NextDouble() < PlantProbability;
        }

        protected override Plant Construct()
        {
            var plantType = _rand.Next(0, Assets.GrassSheetSize);
            return new Plant(_points.Last(), plantType);
        }
    }

    internal class Plant
    {
        public const float Height = 100;
        public Vector2 Position { get; }
        public int DecorationType { get; }

        public Plant(Vector2 position, int decorationType)
        {
            Position = position;
            DecorationType = decorationType;
        }

        public void Draw(Batcher2D batcher)
        {
            var pos = ConvertUnits.ToDisplayUnits(Position);
            pos.Y -= .75f * Height;
            batcher.FillRect(new RectangleF(pos, new Vector2(Height)), Assets.GrassSheet.GetSprite(DecorationType));
        }
    }
}
