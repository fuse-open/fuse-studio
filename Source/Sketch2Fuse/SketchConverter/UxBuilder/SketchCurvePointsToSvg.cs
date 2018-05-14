using System.Globalization;
using System.Text;
using SketchConverter.SketchModel;

namespace SketchConverter.UxBuilder
{
	public static class SketchCurvePointsToSvg
	{
		public static string ToSvgString(SketchPath path)
		{
			var svg = new StringBuilder();
			using (var point = path.Points.GetEnumerator())
			{
				point.MoveNext();
				var first = point.Current;
				svg.MoveTo(first.Point);
				var previous = first;
				while (point.MoveNext())
				{
					svg.Append(PointToSvg(previous, point.Current));
					previous = point.Current;
				}
				if (path.IsClosed)
				{
					svg.Append(PointToSvg(previous, first));
				}
			}
			return svg.ToString();
		}

		private static string PointToSvg(SketchCurvePoint previous, SketchCurvePoint current)
		{
			var svg = new StringBuilder();
			if (previous.Mode == SketchCurvePoint.CurveMode.Line &&
				current.Mode == SketchCurvePoint.CurveMode.Line)
			{
				svg.LineTo(current.Point);
			}
			else if (previous.Mode != SketchCurvePoint.CurveMode.Line
					 && !current.HasCurveTo)
			{
				// We only have one control point and the simplest solution would be a
				// quadratic Bezier curve represented with the Q format in SVG
				// UX path currently doesn't support Q-format, so we have to calculate
				// the control points for a cubic Bezier curve.
				// 2017-11-28 anette
				var c0 = previous.Point + (previous.CurveFrom - previous.Point) * 2.0 / 3.0;
				var c1 = current.Point + (previous.CurveFrom - current.Point) * 2.0 / 3.0;
				svg.CubicBezierTo(c0, c1, current.Point);
			}
			else if (current.Mode != SketchCurvePoint.CurveMode.Line &&
					 !previous.HasCurveFrom)
			{
				// See comment above about quadratic vs cubic Bezier curves
				var c0 = previous.Point + (current.CurveTo - previous.Point) * 2.0 / 3.0;
				var c1 = current.Point + (current.CurveTo - current.Point) * 2.0 / 3.0;
				svg.CubicBezierTo(c0, c1, current.Point);
			}
			else if (previous.Mode != SketchCurvePoint.CurveMode.Line
					 && current.HasCurveTo)
			{
				svg.CubicBezierTo(previous.CurveFrom, current.CurveTo, current.Point);
			}
			return svg.ToString();
		}

		public static string CoordinatesToSvg(SketchPoint point)
		{
			//Due to floating point precision, we don't want to write the final digits, to avoid slight changes to the generated ux on different systems
			//The choice of 13 is arbitrary.
			return point.X.ToString("G13",CultureInfo.InvariantCulture) + " " + point.Y.ToString("G13",CultureInfo.InvariantCulture);
		}

		private static void CubicBezierTo(this StringBuilder svgString,
										  SketchPoint c0,
										  SketchPoint c1,
										  SketchPoint endPoint)
		{
			svgString.Append(" C ")
					 .Append(CoordinatesToSvg(c0))
					 .Append(" ")
					 .Append(CoordinatesToSvg(c1))
					 .Append(" ")
					 .Append(CoordinatesToSvg(endPoint));
		}

		private static void LineTo(this StringBuilder svgString,
								   SketchPoint to)
		{
			svgString.Append(" L ")
					 .Append(CoordinatesToSvg(to));
		}

		private static void MoveTo(this StringBuilder svgString,
								   SketchPoint to)
		{
			svgString.Append("M ")
					 .Append(CoordinatesToSvg(to));
		}
	}
}
