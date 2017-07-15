using System;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using gmtk_jam.Rendering;
using Microsoft.Xna.Framework;

namespace gmtk_jam
{
    public class Tommy
    {
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

        private float RoundingRadius => (CurrentCapacity * MaxCapacity + 0.5f) * 10;
        private float Size => (1 + CurrentCapacity * MaxCapacity) * 100f;
        private float HalfSize => Size / 2f;

        private Vector2 SizeVector => new Vector2(Size);
        private Vector2 HalfSizeVector => new Vector2(HalfSize);

        public Rectangle Bounding => new Rectangle((-HalfSizeVector).ToPoint(), SizeVector.ToPoint());

        public Tommy(World world)
        {
            _world = world;
            UpdateBody();
        }

        private void UpdateBody()
        {
            _body?.Dispose();

            var oldBody = _body;
            var sm = ConvertUnits.ToSimUnits(Size);
            var rm = ConvertUnits.ToSimUnits(RoundingRadius);
            var pos = oldBody?.Position ?? Vector2.Zero;
            var rot = oldBody?.Rotation ?? 0f;

            if (Math.Abs(rm) < 1e-5f)
                _body = BodyFactory.CreateRectangle(_world, sm, sm, 1, pos, rot, BodyType.Dynamic);
            else
                _body = BodyFactory.CreateRoundedRectangle(_world, sm, sm, rm, rm, 10, 1, pos, rot, BodyType.Dynamic);

            _body.Mass = Mass;

            _body.AngularDamping = 0.03f;
            _body.CollidesWith = Category.All;
            _body.CollisionCategories = Physics.TommyCategory;
        }

        public void Update(GameTime gameTime)
        {
            if (Input.IsDown(Input.Action.BreatheIn))
            {
                CurrentCapacity = MaxCapacity;
            }
            else if (Input.IsDown(Input.Action.BreatheOut))
            {
                CurrentCapacity = MathHelper.Max(0, CurrentCapacity - .01f);
                var vec = -new Vector2((float) Math.Cos(Rotation), (float) Math.Sin(Rotation));
                _body.ApplyLinearImpulse(vec * 0.1f);
            }
        }

        public void Draw(Batcher2D batcher)
        {
            var rot = Matrix.CreateRotationZ(Rotation);
            var trans = Matrix.CreateTranslation(Position.X, Position.Y, 0f);
            var scale = Matrix.CreateScale(1f + Progress);
            var mat = scale * rot * trans;

            var spriteNumber = (CurrentCapacity > 0.5f) ? 1 : 0;

            batcher.Matrices.Push(ref mat);
            batcher.FillRect(Bounding, Assets.TommySheet.GetSprite(spriteNumber));
            batcher.Matrices.Pop();
            batcher.Flush();
        }
    }
}
