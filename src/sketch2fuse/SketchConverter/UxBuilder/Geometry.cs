using System.Collections.Generic;
using System.Linq;
using SketchConverter.SketchModel;

namespace SketchConverter.UxBuilder
{
	public static class Geometry
	{
		public static bool IsAxisAlignedRectangle(SketchPath path)
		{
			return path.Points.Count == 4
				   && path.IsClosed
				   && path.Points.All(p => p.Mode == SketchCurvePoint.CurveMode.Line)
				   && EdgesAreAxisAligned(path.Points);
		}

		// public static for testing purposes
		// Check that the four edges of a quadrilateral are axis aligned.
		// The coordinates for paths in the sketch format is saved as a unit square. Width
		// and height is deteremined by the size of the layer.
		public static bool EdgesAreAxisAligned(IList<SketchCurvePoint> pathPoints)
		{
			if (pathPoints.Count != 4)
			{
				throw new UxBuilderException("Checking if quadrilateral is axis aligned, expecting four points");
			}

			var edge1 = pathPoints[0].Point - pathPoints[1].Point;
			var edge2 = pathPoints[1].Point - pathPoints[2].Point;
			var edge3 = pathPoints[2].Point - pathPoints[3].Point;
			var edge4 = pathPoints[3].Point - pathPoints[0].Point;

			var angle1 = edge1.Dot(edge2);
			var angle2 = edge2.Dot(edge3);
			var angle3 = edge3.Dot(edge4);

			return
				(edge1.Y.Equals(0) && edge2.X.Equals(0) || edge1.X.Equals(0) && edge2.Y.Equals(0)) &&
				angle1.Equals(0) &&
				angle2.Equals(0) &&
				angle3.Equals(0);
		}

		private static double Dot(this SketchPoint point, SketchPoint other)
		{
			var res = point.X * other.X + point.Y * other.Y;
			return res;
		}

	}
}
