using System;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using gmtk_jam.Interpolation;
using gmtk_jam.Rendering;
using Microsoft.Xna.Framework;

namespace gmtk_jam
{
    public class Tommy
    {
        public const float SizeTStep = 4f;

        public float Mass = 3f;

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

        public float Progress = 1; // how much have we progressed -> is translated to growth

        public int BurstsLeft = 0;
        public int MaxBursts = 1;

        public float CurrentCapacity => BurstsLeft / (float) MaxBursts; // number between 0 and 1

        private readonly IInterpolation _sizeInterpolation = CubicInterpolation.EaseOut;
        public float _sizeT = 0; // breathe in = -1->0, breathe out = 1->0

        public float MinSize = 1.5f;
        public float MaxSize => 2f * Progress;
        public float RealSize => MinSize + (MaxSize - MinSize) * CurrentCapacity;
        public float LastRecordedSize = 1.5f;

        public float Size => MathHelper.Lerp(LastRecordedSize, RealSize, _sizeInterpolation.Map(1-Math.Abs(_sizeT)));
        private float RoundingRadius => Size / 4f;

        private Vector2 SizeVector => new Vector2(Size);
        private Vector2 HalfSizeVector => new Vector2(Size / 2f);

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
                if (_sizeT >= 0) // breathe out
                {

                }
                else // breathe in
                {
                    var multiplier = 1 / (1+_sizeT);
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
                HitObstacle?.Invoke(this, EventArgs.Empty);
            }
            return true;
        }

        public event EventHandler<EventArgs> HitObstacle;

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

            if (Math.Abs(_sizeT) > 1e-5)
            {
                var ds = SizeTStep * (float) gameTime.ElapsedGameTime.TotalSeconds;
                if (_sizeT < 0)
                    _sizeT = Math.Min(0, _sizeT + ds);
                else
                    _sizeT = Math.Max(0, _sizeT - ds);
                UpdateBody();
            }

            AirTime += (float) gameTime.ElapsedGameTime.TotalSeconds;
        }

        private void BreatheIn(GameTime gameTime)
        {
            if (BurstsLeft > 0)
                return;

            LastRecordedSize = RealSize;
            BurstsLeft = MaxBursts;
            _sizeT = -1;

            Assets.BreatheInSfx.Play();
        }

        private void BreatheOut(GameTime gameTime)
        {
            if (BurstsLeft < 1)
                return;

            LastRecordedSize = RealSize;
            BurstsLeft--;
            _sizeT = 1;

            var vec = -new Vector2((float) Math.Cos(Rotation), (float) Math.Sin(Rotation));
            _body.ApplyLinearImpulse(8f * vec);

            UpdateBody();
            Assets.BreatheOutSfx.Play();
        }

        private void BreatheTurn(GameTime gameTime, int dir)
        {
            if (BurstsLeft < 1)
                return;
            BurstsLeft--;
            var vec = -new Vector2((float) Math.Cos(Rotation), (float) Math.Sin(Rotation));
            _body.ApplyAngularImpulse(dir);

            UpdateBody();
            Assets.BreatheOutSfx.Play();
        }

        public void Draw(Batcher2D batcher)
        {
            var rot = Matrix.CreateRotationZ(Rotation);
            var p = ConvertUnits.ToDisplayUnits(Position);
            var trans = Matrix.CreateTranslation(p.X, p.Y, 0f);
            var scale = Matrix.CreateScale(Size);
            var mat = scale * rot * trans;

            Sprite tommyTex = Assets.TommySheet.GetSprite((int) Math.Floor(Math.Abs(_sizeT) * 3.99f));
            Sprite eyesTex;

            if (_sizeT < -1e-3)
                eyesTex = Assets.Eyes2Sheet.GetSprite(0); // breathe in
            else if (_sizeT > 1e-3)
                eyesTex = Assets.Eyes2Sheet.GetSprite(1); // breathe out
            else if (AngularVelocity > 12f)
                eyesTex = Assets.EyesSheet.GetSprite(2); // sick
            else if (Velocity.LengthSquared() > 280f)
                eyesTex = Assets.EyesSheet.GetSprite(4); // happy
            else if (Velocity.LengthSquared() < 50f)
                eyesTex = Assets.EyesSheet.GetSprite(0); // bored
            else
                eyesTex = Assets.EyesSheet.GetSprite(2); // chill

            var rect = Bounding.ToDisplayUnits();

            batcher.Matrices.Push(ref mat);
            batcher.FillRect(rect, tommyTex);
            batcher.FillRect(rect, eyesTex);
            batcher.Matrices.Pop();
            batcher.Flush();
        }
    }
}
