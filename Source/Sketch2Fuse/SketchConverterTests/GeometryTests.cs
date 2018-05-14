using System.Collections.Generic;
using NUnit.Framework;
using SketchConverter.SketchModel;
using SketchConverter.UxBuilder;

namespace SketchConverterTests
{
	[TestFixture]
	public class GeometryTests
	{

		[TestCase(new []{0.5, 0.0},new []{1.0, 0.0}, new []{1.0, 1.0}, new []{0.0, 1.0})] // first point moved
		[TestCase(new []{0.0, 0.0},new []{1.0, 0.2}, new []{1.0, 1.0}, new []{0.0, 1.0})] // second point moved
		[TestCase(new []{0.0, 0.0},new []{1.0, 0.0}, new []{0.9, 0.9}, new []{0.0, 1.0})] // third point moved
		[TestCase(new []{0.0, 0.0},new []{1.0, 0.0}, new []{1.0, 1.0}, new []{0.25, 0.75})] // fourth point moved
		[TestCase(new []{0.5, 0.0},new []{1.0, 0.5}, new []{0.5, 1.0}, new []{0.0, 0.5})] // rotated square
		[TestCase(new []{0.0, 0.0},new []{5.0, 0.0}, new []{5.0+3.0, 4.0}, new []{3.0, 4.0})] // rhombus (pythagoras <3)
		public void QuadrilateralIsNotRectangle(double [] p1, double [] p2, double [] p3, double [] p4)
		{
			var points = new List<SketchCurvePoint>
			{
				CurvePointUtils.LinePoint(p1[0], p1[1]),
				CurvePointUtils.LinePoint(p2[0], p2[1]),
				CurvePointUtils.LinePoint(p3[0], p3[1]),
				CurvePointUtils.LinePoint(p4[0], p4[1]),
			};

			Assert.That(Geometry.EdgesAreAxisAligned(points), Is.False);
		}

		[Test]
		public void OpenAxisAlignedPathWithFourPointsIsARectangle()
		{
			var rectangle = new SketchPath(CurvePointUtils.RectanglePath(new CornerRadius(0)), false);
			Assert.That(Geometry.IsAxisAlignedRectangle(rectangle), Is.False);
		}

		[Test]
		public void ClosedAxisAlignedPathWithFourPointsIsARectangle()
		{
			var rectangle = new SketchPath(CurvePointUtils.RectanglePath(new CornerRadius(0)), true);
			Assert.That(Geometry.IsAxisAlignedRectangle(rectangle), Is.True);
		}

		[Test]
		public void PathWithMoreThanFourPointsIsNotRectangle()
		{
			var points = CurvePointUtils.RectanglePath(new CornerRadius(0));
			points.Add(CurvePointUtils.LinePoint(0.5, 0.5));
			var path = new SketchPath(points, true);
			Assert.That(Geometry.IsAxisAlignedRectangle(path), Is.False);
		}

		[Test]
		public void PathWithLessThanFourPointsIsNotARectangle()
		{
			var path = new SketchPath(new List<SketchCurvePoint>(), false);
			Assert.That(Geometry.IsAxisAlignedRectangle(path), Is.False);
		}

	}
}
