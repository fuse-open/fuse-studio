using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using SketchConverter.SketchModel;

namespace SketchConverterTests
{
	public class CornerRadius
	{
		public readonly double TopLeft;
		public readonly double TopRight;
		public readonly double BottomRight;
		public readonly double BottomLeft;

		public CornerRadius(double radius)
		{
			TopLeft = TopRight = BottomRight = BottomLeft = radius;
		}
	}

	public class CurvePointUtils
	{

		// create CurvePoint representing a straight line with corner radius = 0
		public static SketchCurvePoint LinePoint(double x, double y, double cornerRadius = 0)
		{
			var p = new SketchPoint(x, y);
			return new SketchCurvePoint(
				p,
				p,
				p,
				cornerRadius,
				SketchCurvePoint.CurveMode.Line, false, false
			);
		}

		public static SketchCurvePoint CurvePoint(double[] p,
												  double[] f,
												  double[] t,
												  double cr = 0)
		{
			return CurvePoint(
				new[]
				{
					p[0], p[1], f[0], f[1], t[0], t[1]
				},
				cr);
		}

		public static SketchCurvePoint CurvePoint(double[] pts,
										   double cornerRadius = 0)
		{
			return new SketchCurvePoint(
				point: new SketchPoint(pts[0], pts[1]),
				curveFrom: new SketchPoint(pts[2], pts[3]),
				curveTo: new SketchPoint(pts[4], pts[5]),
				cornerRadius: cornerRadius,
				mode: SketchCurvePoint.CurveMode.Curve,
				hasCurveFrom: true,
				hasCurveTo: true
			);
		}

		public static IList<SketchCurvePoint> RectanglePath(CornerRadius cornerRadii)
		{
			return new List<SketchCurvePoint>
			{
				LinePoint(0, 0, cornerRadii.TopLeft),
				LinePoint(1, 0, cornerRadii.TopRight),
				LinePoint(1, 1, cornerRadii.BottomRight),
				LinePoint(0, 1, cornerRadii.BottomLeft)
			};
		}
	}
}
