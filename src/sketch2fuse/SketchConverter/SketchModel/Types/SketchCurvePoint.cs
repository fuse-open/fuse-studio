namespace SketchConverter.SketchModel
{
    public class SketchCurvePoint
    {
        public readonly SketchPoint Point;
        public readonly SketchPoint CurveFrom;
        public readonly SketchPoint CurveTo;
        public readonly double CornerRadius;
        public readonly CurveMode Mode;
        public readonly bool HasCurveFrom;
        public readonly bool HasCurveTo;


        //[JsonConverter(typeof(StringEnumConverter))] can we use this?
        public enum CurveMode
        {
            Line = 1,
            Curve = 2,
            Asymmetric = 3,
            Disconnected = 4
        }

        public SketchCurvePoint(
            SketchPoint point,
            SketchPoint curveFrom,
            SketchPoint curveTo,
            double cornerRadius,
            CurveMode mode,
            bool hasCurveFrom,
            bool hasCurveTo)
        {
            Point = point;
            CurveFrom = curveFrom;
            CurveTo = curveTo;
            CornerRadius = cornerRadius;
            Mode = mode;
            HasCurveFrom = hasCurveFrom;
            HasCurveTo = hasCurveTo;
        }
    }
}