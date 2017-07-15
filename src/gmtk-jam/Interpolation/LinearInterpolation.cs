namespace gmtk_jam.Interpolation
{
    public class LinearInterpolation : IInterpolation
    {

        private static LinearInterpolation _instance;

        public static LinearInterpolation Instance => 
            _instance ?? (_instance = new LinearInterpolation());

        private LinearInterpolation()
        {
        }

        public float Map(float val)
        {
            return val;
        }

        public override string ToString()
        {
            return "Linear Interpolation";
        }
    }
}