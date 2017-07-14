using GameSandbox;
using Microsoft.Xna.Framework;

namespace gmtk_jam
{
    public class Tommy
    {
        public Vector2 Position { get; set; }

        public float Progress = 1;

        public float MaxCapacity => Progress;
        public float CurrentCapacity { get; private set; }
        public float Oxygen { get; } = 1f; // between 0 and 1

        private float Size => (1 + CurrentCapacity) * 100f;

        private Vector2 SizeVector => new Vector2(Size);
        private Vector2 HalfSizeVector => new Vector2(Size / 2f);

        public Rectangle Bounding => new Rectangle((Position - HalfSizeVector).ToPoint(), SizeVector.ToPoint()); 

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
        }

        public void Draw(Batcher2D batcher)
        {
            batcher.FillRect(Bounding, Color.MonoGameOrange);
        }
    }
}