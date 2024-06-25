using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using SketchConverter.SketchModel;
using SketchConverter.UxBuilder;

namespace SketchConverterTests
{
	[TestFixture]
	public class SketchPathToSvgTests
	{
		[Test]
		public void LocaleDecimalSeparatorIgnored()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("nb-NO"); // make `,` the decimal separator
			var points = StraightLine(new SketchPoint(3.14, 2.1), new SketchPoint(13.1, 4.1));
			var svg = SketchCurvePointsToSvg.ToSvgString(new SketchPath(points: points, isClosed: false));

			Assert.That(svg.Contains(","), Is.False);
		}

		[Test]
		public void StraightLineToSvgString()
		{
			var points = StraightLine(new SketchPoint(10.0, 2.0), new SketchPoint(10.0, 10.0));
			var path = new SketchPath(points: points, isClosed: false);

			var svg = SketchCurvePointsToSvg.ToSvgString(path);

			Assert.That(svg, Does.Match("^M 10 2 L 10 10$"));

		}

		private static List<SketchCurvePoint> StraightLine(SketchPoint p1, SketchPoint p2)
		{
			var points = new List<SketchCurvePoint>()
			{
				new SketchCurvePoint(
					point: p1,
					curveFrom: p1,
					curveTo: p1,
					cornerRadius: 0,
					mode: SketchCurvePoint.CurveMode.Line,
					hasCurveFrom: false,
					hasCurveTo: false
				),
				new SketchCurvePoint(
					point: p2,
					curveFrom: p2,
					curveTo: p2,
					cornerRadius: 0,
					mode: SketchCurvePoint.CurveMode.Line,
					hasCurveFrom: true,
					hasCurveTo: true)
			};
			return points;
		}

		[Test]
		public void ClosedStarPolygon()
		{
			var points = new[]
			{
				CurvePointUtils.LinePoint(0.5, 0.75),
				CurvePointUtils.LinePoint(0.20610737385376349, 0.90450849718747373),
				CurvePointUtils.LinePoint(0.26223587092621159, 0.57725424859373686),
				CurvePointUtils.LinePoint(0.024471741852423179, 0.34549150281252639),
				CurvePointUtils.LinePoint(0.35305368692688166, 0.29774575140626314),
				CurvePointUtils.LinePoint(0.49999999999999989, 0),
				CurvePointUtils.LinePoint(0.64694631307311823, 0.29774575140626314),
				CurvePointUtils.LinePoint(0.97552825814757682, 0.34549150281252616),
				CurvePointUtils.LinePoint(0.73776412907378841, 0.57725424859373675),
				CurvePointUtils.LinePoint(0.79389262614623668, 0.90450849718747361),
			};
			var path = new SketchPath(points, true);

			var svg = SketchCurvePointsToSvg.ToSvgString(path);

			Assert.That(svg, Does.Match("^M 0.5 0.75 L 0.2061073738538 0.9045084971875 L 0.2622358709262 0.5772542485937 L 0.02447174185242 0.3454915028125 L 0.3530536869269 0.2977457514063 L 0.5 0 L 0.6469463130731 0.2977457514063 L 0.9755282581476 0.3454915028125 L 0.7377641290738 0.5772542485937 L 0.7938926261462 0.9045084971875 L 0.5 0.75$"));
		}

		[Test]
		public void Oval()
		{
			var points = new[]
			{
				CurvePointUtils.CurvePoint(new[]
				{
					0.5, 1, 0.77614237490000004, 1, 0.22385762510000001, 1
				}),
				CurvePointUtils.CurvePoint(new[]
				{
					1, 0.5, 1, 0.22385762510000001, 1, 0.77614237490000004
				}),
				CurvePointUtils.CurvePoint(new[]
				{
					0.5, 0, 0.22385762510000001, 0, 0.77614237490000004, 0
				}),
				CurvePointUtils.CurvePoint(new[]
				{
					0, 0.5, 0, 0.77614237490000004, 0, 0.22385762510000001
				}),
			};

			var oval = new SketchPath(points, true);
			var svg = SketchCurvePointsToSvg.ToSvgString(oval);
			Assert.That(svg, Does.Match("^M 0.5 1 C 0.7761423749 1 1 0.7761423749 1 0.5 C 1 0.2238576251 0.7761423749 0 0.5 0 C 0.2238576251 0 0 0.2238576251 0 0.5 C 0 0.7761423749 0.2238576251 1 0.5 1$"));
		}

		[Test]
		public void CurveFromFirstPoint()
		{
			var points = new[]
			{
				CurvePointUtils.CurvePoint(new[] {0.0, 0.0},
										   new[] {123.20975114271207, 8.196425347535656},
										   new[] {-123.20975114271207, -8.1964253475356769}),
				CurvePointUtils.LinePoint(94.613074518933793, 99)
			};

			var curve = new SketchPath(points, false);
			var svg = SketchCurvePointsToSvg.ToSvgString(curve);
			Console.WriteLine(svg);
			Assert.That(svg, Does.Match("^M 0 0 C 82.13983409514 5.464283565024 113.6775256015 38.46428356502 94.61307451893 99$"));
		}

		[Test]
		public void StartWithCurve()
		{
			var points = new[]
			{
				CurvePointUtils.CurvePoint(new [] {0.0, 100, 0, 0, 0, 200}),
				CurvePointUtils.LinePoint(50, 0),
				CurvePointUtils.LinePoint(100, 100)
			};
			var curve = new SketchPath(points, false);
			var svg = SketchCurvePointsToSvg.ToSvgString(curve);
			Console.WriteLine(svg);
			Assert.That(svg, Does.Match("^M 0 100 C 0 33.33333333333 16.66666666667 0 50 0 L 100 100$"));
		}

		[Test]
		public void EndWithCurve()
		{
			var points = new[]
			{
				CurvePointUtils.LinePoint(0, 100),
				CurvePointUtils.LinePoint(50, 0),
				CurvePointUtils.CurvePoint(new []{100.0, 100, 100, 200, 100, 0})
			};

			var curve = new SketchPath(points, false);
			var svg = SketchCurvePointsToSvg.ToSvgString(curve);
			Console.WriteLine(svg);
			Assert.That(svg, Does.Match("M 0 100 L 50 0 C 83.33333333333 0 100 33.33333333333 100 100$"));
		}

		[Test]
		public void CoordinatesToSvgSkipsLastDecimals()
		{
			Assert.That(SketchCurvePointsToSvg.CoordinatesToSvg(new SketchPoint(Math.PI/10, Math.PI*10)),
				Is.EqualTo("0.314159265359 31.4159265359"));
		}

	}
}
