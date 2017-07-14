using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace gmtk_jam
{
    public class Tommy
    {
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }

        public Vector2 Velocity { get; set; }
        public float RotVelocity { get; set; }

        public float Progress = 1;

        public float MaxCapacity => Progress;
        public float CurrentCapacity { get; private set; } // number between 0 and 1
        public float Oxygen { get; } = 1f; // between 0 and 1

        private float Size => (1 + CurrentCapacity * MaxCapacity) * 100f;
        private float HalfSize => Size / 2f;

        private Vector2 SizeVector => new Vector2(Size);
        private Vector2 HalfSizeVector => new Vector2(HalfSize);

        public Rectangle Bounding => new Rectangle((-HalfSizeVector).ToPoint(), SizeVector.ToPoint());

        public void Update()
        {
            if (Input.IsDown(Input.Action.BreatheIn))
            {
                CurrentCapacity = MaxCapacity;
            }
            else if (Input.IsDown(Input.Action.BreatheOut))
            {
                CurrentCapacity = MathHelper.Max(0, CurrentCapacity - .01f);
            }

            Rotation += 0.002f;
        }

        public void Draw(Batcher2D batcher)
        {
            var rot = Matrix.CreateRotationZ(Rotation);
            var trans = Matrix.CreateTranslation(Position.X, Position.Y, 0f);

            batcher.Matrices.Push(ref trans);
            batcher.Matrices.Push(ref rot);
            batcher.FillRect(Bounding, Color.Red);
            batcher.Matrices.Pop();
            batcher.Matrices.Pop();
            batcher.Flush();
        }

        public IEnumerable<Vector2> Points()
        {
            var c = (float)Math.Cos(Rotation);
            var s = (float)Math.Sin(Rotation);

            // [c, -s * [vs
            //  s,  c]   vy]

            yield break;
        }
    }
}
