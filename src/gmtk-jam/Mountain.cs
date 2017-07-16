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
        private readonly RollingClouds _clouds;

        public int PointCount => _points.Count;

        public Mountain(World world, Camera camera, int seed = 42)
        {
            _world = world;
            _camera = camera;
            _points = new RollingPoints(seed);
            _plants = new RollingPlants(seed);
            _adversaries = new RollingAdversaries(_world, seed);
            _clouds = new RollingClouds(_world, seed);

            _points.SpawnEvent += (obj, left, r) => _plants.Update(left, obj);
            _points.SpawnEvent += (obj, left, r) => _adversaries.Update(left, obj);

            Update(true);
        }

        public void Update(bool forceBodyUpdate = false)
        {
            var rect = _camera.BoundingRect.ToRectangleF().ToSimUnits();
            if (_points.Update(rect.Left, rect.Right) || forceBodyUpdate)
                CreateBody();
            _clouds.Update(rect.Left, rect.TopRight);
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
            _body.CollidesWith = Physics.TommyCategory;
        }

        public void Draw(Batcher2D batcher)
        {
            foreach (var cloud in _clouds)
                cloud.Draw(batcher);

            // bottom of the screen + a little margin
            var b = _camera.BoundingRect.Bottom + 10;
            var ps = _points.SelectMany(v => VerticalPair(v, b));
            batcher.FillTriangleStrip(ps, new Color(0x15, 0x32, 0x22));
            batcher.DrawLineStrip(_points.Select(ConvertUnits.ToDisplayUnits), Color.Black, 4);

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
        public event SpawnHandler DespawnEvent;
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

            DespawnLeft(left, right);
            return true;
        }

        private void DespawnLeft(float left, TRight right)
        {
            if (_objects.Count == 0)
                return;
            if (GetX(_objects.First.Value) > left - BufferZone)
                return;

            var before = Count;

            while (_objects.Count > 0 && GetX(_objects.First.Value) < left - TightBufferZone)
            {
                var obj = _objects.First.Value;
                _objects.RemoveFirst();
                DespawnEvent?.Invoke(obj, left, right);
            }

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

            while (Count == 0 || GetX(Last()) < right + BufferZone)
                Add(ConstructPoint());

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
        private const double AdversaryProbability = 0.01;

        private readonly Random _rand;
        private readonly World _world;

        public RollingAdversaries(World world, int seed)
        {
            _world = world;
            _rand = new Random(seed);

            DespawnEvent += (adversary, left, right) => adversary.Dispose();
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
            var adv = new Adversary(_world, point);
            return adv;
        }

        protected override float GetX(Adversary adv) => adv.Position.X;
    }

    internal class RollingClouds : RollingObjects<Cloud, Vector2>
    {
        private const float MaxHeight = 5;
        private const float MinHeight = 0;

        private readonly Random _rand;
        private readonly World _world;

        public RollingClouds(World world, int seed)
        {
            _rand = new Random(seed);
            _world = world;

            DespawnEvent += (cloud, left, right) => cloud.Dispose();
        }

        protected override bool SpawnRight(Vector2 rightTop)
        {
            if (Count > 0 && GetX(Last()) > rightTop.X + TightBufferZone)
                return false;

            if (Count > 0 && Math.Abs(GetX(Last()) - rightTop.X) < 6.0 * _rand.NextDouble() + 2.0)
                return false;

            if (Count > 20)
                return true; // allow removal!

            Add(ConstructCloud(rightTop));

            return true;
        }

        protected override float GetX(Cloud cloud) => cloud.Position.X;

        private Cloud ConstructCloud(Vector2 rightTop)
        {
            var velocity = (float) (1.5 * _rand.NextDouble() - 2f);
            var pos = rightTop;
            pos.Y += MinHeight + ((float) _rand.NextDouble() * (MaxHeight - MinHeight));

            return new Cloud(_world, pos, velocity, _rand.Next(0, Assets.CloudsSheetSize));
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

    internal class Adversary
    {
        public const float Size = 2f;

        private readonly Body _body;
        public Vector2 Position => _body.Position;

        public Adversary(World world, Vector2 position)
        {
            //Position = position;
            var p = position - 0.5f * Size * Vector2.UnitY;
            _body = BodyFactory.CreateRectangle(world, Size, Size, 1.0f, p, rotation: 0f, bodyType: BodyType.Static);
            _body.Friction = 0.8f;
            _body.Restitution = 0.05f;
            _body.CollisionCategories = Physics.ObstaclesCategory;
            _body.CollidesWith = Physics.TommyCategory;
        }

        public void Draw(Batcher2D batcher)
        {
            var pos = ConvertUnits.ToDisplayUnits(Position);
            var d = ConvertUnits.ToDisplayUnits(Size);
            batcher.DrawRect(new RectangleF(pos - new Vector2(d / 2f), new Vector2(d)), Color.IndianRed, lineWidth: 2);
        }

        public void Dispose() => _body?.Dispose();
    }

    internal class Cloud : IDisposable
    {
        private readonly int _type;
        public const float Width = 3f;
        public const float Height = 1.5f;

        private readonly Body _body;
        public Vector2 Position => _body.Position;

        public Cloud(World world, Vector2 position, float velocity, int type)
        {
            _type = type;
            _body = BodyFactory.CreateRectangle(world, 2, 4, 1, position, bodyType: BodyType.Kinematic);
            _body.LinearVelocity = Vector2.UnitX * velocity;
            _body.CollidesWith = Category.None;
        }

        public void Draw(Batcher2D batcher)
        {
            var pos = ConvertUnits.ToDisplayUnits(Position);
            var w = ConvertUnits.ToDisplayUnits(Width);
            var h = ConvertUnits.ToDisplayUnits(Height);
            batcher.FillRect(new RectangleF(pos, new Vector2(w, h)), Assets.CloudsSheet.GetSprite(_type));
        }

        public void Dispose() => _body?.Dispose();
    }
}
