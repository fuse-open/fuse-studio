using System.Collections.Generic;

namespace SketchConverter.SketchModel
{
    public class SketchPath
    {
        public readonly IList<SketchCurvePoint> Points;
        public readonly bool IsClosed;

        public SketchPath(IList<SketchCurvePoint> points,
            bool isClosed)
        {
            Points = points;
            IsClosed = isClosed;
        }
    }
}