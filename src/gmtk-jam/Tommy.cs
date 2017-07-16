using System;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using gmtk_jam.Interpolation;
using gmtk_jam.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace gmtk_jam
{
    public class Tommy
    {
        public const float SizeTStep = 4f;

        public float Mass = 3f;
        //public float Density = 1;

        private readonly World _world;
        private Body _body;

        public Vector2 Velocity => _body.LinearVelocity;
        public float AngularVelocity => _body.AngularVelocity;

        public Vector2 Position
        {
            get => _body.Position;
            set => _body.Position = value;
        }

        public float Rotation
        {
            get => _body.Rotation;
            set => _body.Rotation = value;
        }

        public float Progress = 1;

        public int BurstsLeft;
        public int MaxBursts = 1;

        public float MaxCapacity => Progress;
        public float CurrentCapacity => BurstsLeft / (float) MaxBursts; // number between 0 and 1
        public float Oxygen { get; } = 1f; // between 0 and 1

        private float RoundingRadius => Size / 4f;
        public float TargetSize => (1 + CurrentCapacity * MaxCapacity *.5f);

        private IInterpolation _sizeInterpolation = CubicInterpolation.EaseOut;
        public float _sizeT = 1;

        public float MinSize = 1;
        public float Size => MathHelper.Lerp(MinSize, TargetSize, _sizeInterpolation.Map(_sizeT));
        private float HalfSize => Size / 2f;

        private Vector2 SizeVector => new Vector2(Size);
        private Vector2 HalfSizeVector => new Vector2(HalfSize);

        public RectangleF Bounding => new RectangleF(-HalfSizeVector, SizeVector);

        public float AirTime;

        public Tommy(World world)
        {
            _world = world;
            _body = new Body(world);
            UpdateBody();
        }

        private bool OnCollideMountain(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            AirTime = 0;
            if (fixtureB.CollisionCategories == Physics.MountainCategory)
            {
                if (_sizeT == 1)
                {

                }
                else
                {
                    var multiplier = 1 / _sizeT;
                    if (multiplier > 20)
                    {
                        // TODO SUPER MULTIPLIER
                        multiplier = 20;
                    }
                    _body.ApplyLinearImpulse(.3f * multiplier * new Vector2(.2f, -1f));
                }
            }
            else if (fixtureB.CollisionCategories == Physics.ObstaclesCategory)
            {

            }
            return true;
        }

        private void UpdateBody()
        {
            var sm = Size;
            var rm = RoundingRadius;
            var pos = _body.Position;
            var rot = _body.Rotation;
            var v = Velocity;
            var av = AngularVelocity;

            _body.OnCollision -= OnCollideMountain;
            _body.Dispose();

            _body = Math.Abs(rm) < 1e-5f
                ? BodyFactory.CreateRectangle(_world, sm, sm, 1, pos, rot, BodyType.Dynamic)
                : BodyFactory.CreateRoundedRectangle(_world, sm, sm, rm, rm, 10, 1, pos, rot, BodyType.Dynamic);

            _body.LinearVelocity = v;
            _body.AngularVelocity = av;
            _body.Mass = Mass;

            _body.LinearDamping = 0.2f;
            _body.AngularDamping = 0.4f;
            _body.CollidesWith = Category.All;
            _body.CollisionCategories = Physics.TommyCategory;
            _body.OnCollision += OnCollideMountain;
        }

        public void Update(GameTime gameTime)
        {
            if (Input.IsPressed(Input.Action.BreatheIn))
                BreatheIn(gameTime);
            else if (Input.IsPressed(Input.Action.BreatheOut))
                BreatheOut(gameTime);
            else if (Input.IsPressed(Input.Action.BreatheDown))
                BreatheTurn(gameTime, -1);
            else if (Input.IsPressed(Input.Action.BreatheUp))
                BreatheTurn(gameTime, 1);

            if (_sizeT < 1)
            {
                var ds = SizeTStep * (float) gameTime.ElapsedGameTime.TotalSeconds;
                _sizeT = Math.Min(1, _sizeT + ds);
                UpdateBody();
            }

            AirTime += (float) gameTime.ElapsedGameTime.TotalSeconds;
        }

        private void BreatheIn(GameTime gameTime)
        {
            if (BurstsLeft > 0)
                return;

            BurstsLeft = MaxBursts;
            _sizeT = 0;
        }

        private void BreatheOut(GameTime gameTime)
        {
            if (BurstsLeft < 1)
                return;
            BurstsLeft--;
            var vec = -new Vector2((float) Math.Cos(Rotation), (float) Math.Sin(Rotation));
            _body.ApplyLinearImpulse(8f * vec);

            UpdateBody();
        }

        private void BreatheTurn(GameTime gameTime, int dir)
        {
            if (BurstsLeft < 1)
                return;
            BurstsLeft--;
            var vec = -new Vector2((float) Math.Cos(Rotation), (float) Math.Sin(Rotation));
            _body.ApplyAngularImpulse(dir);

            UpdateBody();
        }

        public void Draw(Batcher2D batcher)
        {
            var rot = Matrix.CreateRotationZ(Rotation);
            var p = ConvertUnits.ToDisplayUnits(Position);
            var trans = Matrix.CreateTranslation(p.X, p.Y, 0f);
            var scale = Matrix.CreateScale(Size * 1.5f);
            var mat = scale * rot * trans;

            var tommyNo = (int)Math.Floor(CurrentCapacity * 3.99f);
            int eyesNo;

            if (AngularVelocity > 12f)
                eyesNo = 2; // sick
            else if (Velocity.LengthSquared() > 300f)
                eyesNo = 4; // happy
            else if (Velocity.LengthSquared() < 50f)
                eyesNo = 0; // bored
            else
                eyesNo = 2; // chill

            var rect = Bounding.ToDisplayUnits();

            batcher.Matrices.Push(ref mat);
            batcher.FillRect(rect, Assets.TommySheet.GetSprite(tommyNo));
            batcher.FillRect(rect, Assets.EyesSheet.GetSprite(eyesNo));
            batcher.Matrices.Pop();
            batcher.Flush();
        }
    }
}
