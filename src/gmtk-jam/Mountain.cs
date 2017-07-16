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
        private readonly RollingAdversaries _adversaries;

        public int PointCount => _points.Count;

        public Mountain(World world, Camera camera, int seed = 42)
        {
            _world = world;
            _camera = camera;
            _points = new RollingPoints(seed);
            _plants = new RollingPlants(seed);
            _adversaries = new RollingAdversaries(seed);

            _points.SpawnEvent += (obj, left, r) => _plants.Update(left, obj);
            _points.SpawnEvent += (obj, left, r) => _adversaries.Update(left, obj);

            Update(true);
        }

        public void Update(bool forceBodyUpdate = false)
        {
            var rect = _camera.BoundingRect.ToRectangleF().ToSimUnits();
            if (!_points.Update(rect.Left, rect.Right) && !forceBodyUpdate) return;
            CreateBody();
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

            foreach (var adv in _adversaries)
                adv.Draw(batcher);
        }

        private IEnumerable<Vector2> VerticalPair(Vector2 v, float bottom)
        {
            var vd = ConvertUnits.ToDisplayUnits(v);
            // make sure we start clockwise because of backface culling
            yield return new Vector2(vd.X, bottom);
            yield return vd;
        }
    }

    internal abstract class RollingObjects<T, TRight> : IEnumerable<T>
    {
        public static float BufferZone { get; } = 10f;
        public static float TightBufferZone { get; } = 2f;

        private readonly LinkedList<T> _objects;

        protected RollingObjects()
        {
            _objects = new LinkedList<T>();
        }

        public T Last() => _objects.Last.Value;
        public int Count => _objects.Count;
        protected void Add(T obj) => _objects.AddLast(obj);

        public IEnumerator<T> GetEnumerator() => _objects.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _objects).GetEnumerator();

        public delegate void SpawnHandler(T obj, float left, TRight right);
        public event SpawnHandler SpawnEvent;

        public bool Update(float left, TRight right)
        {
            var lastNode = _objects.Last;

            if (!SpawnRight(right)) return false; // side effects!

            // invoke event for each newly added item
            while (lastNode?.Next != null)
            {
                lastNode = lastNode.Next;
                SpawnEvent?.Invoke(lastNode.Value, left, right);
            }

            DespawnLeft(left);
            return true;
        }

        private void DespawnLeft(float left)
        {
            if (_objects.Count == 0)
                return;
            if (GetX(_objects.First.Value) > left - BufferZone)
                return;

            var before = Count;

            while (_objects.Count > 0 && GetX(_objects.First.Value) < left - TightBufferZone)
                _objects.RemoveFirst();

            if (Count < before)
                Console.WriteLine($"{GetType().Name}: removed objects: {before}->{Count}");
        }

        protected abstract bool SpawnRight(TRight right);
        protected abstract float GetX(T obj);
    }

    internal class RollingPoints : RollingObjects<Vector2, float>
    {
        private const float MinDerivative = .2f;
        private const float MaxDerivative = .5f;
        private const float DerivativeChangeRate = .07f;
        private const float DiscontinuityChance = .02f;

        private float _currentDerivative = .01f;

        private readonly Random _rand;

        public RollingPoints(int seed) : base()
        {
            _rand = new Random(seed);
            Add(ConvertUnits.ToSimUnits(new Vector2(-500, 350)));
            Add(ConvertUnits.ToSimUnits(new Vector2(-5, 350)));
        }

        protected override bool SpawnRight(float right)
        {
            if (Count > 0 && GetX(Last()) > right + TightBufferZone)
                return false;

            var before = Count;

            while (Count == 0 || GetX(Last()) < right + BufferZone)
                Add(ConstructPoint());

            if (Count > before)
                Console.WriteLine($"{GetType().Name}: added objects: {before}->{Count}");

            return true;
        }

        protected override float GetX(Vector2 point) => point.X;

        private Vector2 ConstructPoint()
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

    internal class RollingPlants : RollingObjects<Plant, Vector2>
    {
        private const double PlantProbability = .07f;

        private readonly Random _rand;

        public RollingPlants(int seed) : base()
        {
            _rand = new Random(seed);
        }

        protected override bool SpawnRight(Vector2 point)
        {
            if (!(_rand.NextDouble() < PlantProbability))
                return false;
            Add(ConstructPlant(point));
            return true;
        }

        protected override float GetX(Plant dec) => dec.Position.X;

        private Plant ConstructPlant(Vector2 point)
        {
            var plantType = _rand.Next(0, Assets.GrassSheetSize);
            return new Plant(point, plantType);
        }
    }

    internal class RollingAdversaries : RollingObjects<Adversary, Vector2>
    {
        public static readonly double AdversaryProbability = 0.01;
        private readonly Random _rand;

        public RollingAdversaries(int seed)
        {
            _rand = new Random(seed);
        }

        protected override bool SpawnRight(Vector2 point)
        {
            if (!(_rand.NextDouble() < AdversaryProbability))
                return false;
            Add(ConstructAdversary(point));
            return true;
        }

        private Adversary ConstructAdversary(Vector2 point)
        {
            return new Adversary(point);
        }

        protected override float GetX(Adversary adv) => adv.Position.X;
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

    internal class Adversary
    {
        public Vector2 Position { get; }
        public Adversary(Vector2 position)
        {
            Position = position;
        }

        public void Draw(Batcher2D batcher)
        {
            var height = 120;
            var pos = ConvertUnits.ToDisplayUnits(Position);
            pos.Y -= .75f * height;
            batcher.DrawRect(new RectangleF(pos, new Vector2(height)), Color.IndianRed, lineWidth: 2);
        }
    }
}
