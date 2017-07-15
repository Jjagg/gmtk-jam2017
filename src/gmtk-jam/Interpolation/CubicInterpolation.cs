using Microsoft.Xna.Framework;

namespace gmtk_jam.Interpolation
{
    public class CubicInterpolation : IInterpolation
    {
        private const float SampleStep = 0.05f;
        private readonly Vector2 _p1;
        private readonly Vector2 _p2;

        private readonly Curve _sampledCurve;

        public CubicInterpolation(Vector2 p1, Vector2 p2)
        {
            _p1 = p1;
            _p2 = p2;
            _sampledCurve = new Curve();
            ComputeCurve();
        }

        public CubicInterpolation(float x1, float y1, float x2, float y2) :
            this(new Vector2(x1, y1), new Vector2(x2, y2)) {
        }

        public float Map(float val)
        {
            return _sampledCurve.Evaluate(val);
        }

        private Vector2 Evaluate(float t)
        {
            var u = 1 - t;
            return 3 * t * u * u * _p1 + 3 * t * t * u * _p2 + t * t * t * Vector2.One;
        }

        private void ComputeCurve()
        {
            var keys = _sampledCurve.Keys;
            keys.Clear();
            keys.Add(new CurveKey(0, 0));
            for (var t = SampleStep; t < 1; t += SampleStep)
            {
                var p = Evaluate(t);
                keys.Add(new CurveKey(p.X, p.Y));
            }
            keys.Add(new CurveKey(1, 1));
            _sampledCurve.ComputeTangents(CurveTangent.Smooth);
        }

        protected bool Equals(CubicInterpolation other)
        {
            return _p1.Equals(other._p1) && _p2.Equals(other._p2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CubicInterpolation) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_p1.GetHashCode()*397) ^ _p2.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"Cubic Interpolation [P1: {_p1}, P2: {_p2}]";
        }

        public static CubicInterpolation EaseIn { get; } = new CubicInterpolation(0.42f, 0f, 1f, 1f);

        public static CubicInterpolation EaseOut { get; } = new CubicInterpolation(0f, 0f, 0.58f, 1f);

        public static CubicInterpolation EaseInOut { get; } = new CubicInterpolation(0.42f, 0f, 0.58f, 1f);
    }
}