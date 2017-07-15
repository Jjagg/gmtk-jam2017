using System;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using gmtk_jam.Interpolation;
using gmtk_jam.Rendering;
using Microsoft.Xna.Framework;

namespace gmtk_jam
{
    public class Tommy
    {
        public const float SizeTStep = 5f;

        public float Mass = 3f;
        //public float Density = 1;

        private readonly World _world;
        private Body _body;

        public Vector2 Velocity => _body.LinearVelocity;
        public float AngularVelocity => _body.AngularVelocity;

        public Vector2 Position
        {
            get => ConvertUnits.ToDisplayUnits(_body.Position);
            set => _body.Position = ConvertUnits.ToSimUnits(value);
        }

        public float Rotation
        {
            get => _body.Rotation;
            set => _body.Rotation = value;
        }

        public float Progress = 1;

        public float MaxCapacity => Progress;
        public float CurrentCapacity { get; private set; } = 0.1f; // number between 0 and 1
        public float Oxygen { get; } = 1f; // between 0 and 1

        private float RoundingRadius => Size / 4f;
        private float TargetSize => (1 + CurrentCapacity * MaxCapacity) * 1.5f;

        private IInterpolation _sizeInterpolation = CubicInterpolation.EaseOut;
        private float _sizeT = 1;

        public float Size => 1f + 0.02f * _sizeInterpolation.Map(_sizeT);
        private float HalfSize => Size / 2f;

        private Vector2 SizeVector => new Vector2(Size);
        private Vector2 HalfSizeVector => new Vector2(HalfSize);

        public RectangleF Bounding => new RectangleF(-HalfSizeVector, SizeVector);

        public Tommy(World world)
        {
            _world = world;
            UpdateBody();
        }

        private void UpdateBody()
        {
            _body?.Dispose();

            var oldBody = _body;
            var sm = Size;
            var rm = RoundingRadius;
            var pos = oldBody?.Position ?? Vector2.Zero;
            var rot = oldBody?.Rotation ?? 0f;

            if (Math.Abs(rm) < 1e-5f)
                _body = BodyFactory.CreateRectangle(_world, sm, sm, 1, pos, rot, BodyType.Dynamic);
            else
                _body = BodyFactory.CreateRoundedRectangle(_world, sm, sm, rm, rm, 10, 1, pos, rot, BodyType.Dynamic);

            _body.Mass = Mass;

            _body.AngularDamping = 0.1f;
            _body.CollidesWith = Category.All;
            _body.CollisionCategories = Physics.TommyCategory;
        }

        public void Update(GameTime gameTime)
        {
            if (Input.IsDown(Input.Action.BreatheIn))
                BreatheIn(gameTime);
            else if (Input.IsDown(Input.Action.BreatheOut))
                BreatheOut(gameTime);

            if (_sizeT < 1)
            {
                var ds = SizeTStep * (float) gameTime.ElapsedGameTime.TotalSeconds;
                _sizeT = Math.Min(1, _sizeT + ds);
            }
        }

        private void BreatheIn(GameTime gameTime)
        {
            if (CurrentCapacity != 0)
                return;

            CurrentCapacity = MaxCapacity;
        }

        private void BreatheOut(GameTime gameTime)
        {
            CurrentCapacity = MathHelper.Max(0, CurrentCapacity - .01f);
            var vec = -new Vector2((float) Math.Cos(Rotation), (float) Math.Sin(Rotation));
            _body.ApplyLinearImpulse(0.1f * vec * (float) gameTime.ElapsedGameTime.TotalSeconds);
        }

        public void Draw(Batcher2D batcher)
        {
            var rot = Matrix.CreateRotationZ(Rotation);
            var trans = Matrix.CreateTranslation(Position.X, Position.Y, 0f);
            var scale = Matrix.CreateScale(1f + Progress);
            var mat = scale * rot * trans;

            var tommyNo = (int)Math.Floor(CurrentCapacity * (Assets.TommySheet.Sprites - .1f));
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
