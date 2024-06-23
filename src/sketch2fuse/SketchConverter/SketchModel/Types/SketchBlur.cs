namespace SketchConverter.SketchModel
{
    public class SketchBlur
    {
        public readonly SketchPoint Center;
        public readonly int MotionAngle;
        public readonly double Radius;
        public readonly SketchBlurType BlurType;

        public SketchBlur(SketchPoint center, int motionAngle, double radius, SketchBlurType blurType)
        {
            Center = center;
            MotionAngle = motionAngle;
            Radius = radius;
            BlurType = blurType;
        }

        public enum SketchBlurType
        {
            Gaussian = 0,
            Motion = 1,
            Zoom = 2,
            Background = 3
        }
    }
}