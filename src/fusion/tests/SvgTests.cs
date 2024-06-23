using System;
using System.Drawing;
using System.IO;
using NUnit.Framework;

namespace Outracks.Fusion.Tests
{
	[TestFixture]
	public class SvgImageTests
	{
		[Test]
		public void Loads_image_with_specified_color_map()
		{
			// With no color map defined
			LoadAndAssertTestImage(
				"GreenBox.svg",
				b =>
				{
					Assert.That(b.Height, Is.EqualTo(4));
					Assert.That(b.Width, Is.EqualTo(4));
					AssertPixelEqualWithinTolerance(b, 1, 1, Color.FromRgba(0x00ff00ff));
				});
			// With red color map defined
			LoadAndAssertTestImage(
				"GreenBox.svg",
				b =>
				{
					Assert.That(b.Height, Is.EqualTo(4));
					Assert.That(b.Width, Is.EqualTo(4));
					AssertPixelEqualWithinTolerance(b, 1, 1, Color.FromRgba(0xff0000ff));
				},
				colorMap: new AlwaysRedColorMap());
		}

		[Test]
		public void Loads_image_with_specified_scale_factor()
		{
			LoadAndAssertTestImage(
				"HalfTransparentBox.svg",
				b =>
				{
					Assert.That(b.Height, Is.EqualTo(8));
					Assert.That(b.Width, Is.EqualTo(8));
				},
				scaleFactor: new Ratio<Pixels, Points>(2));
		}


		[Test]
		public void Blending_of_half_pixel_against_background()
		{
			LoadAndAssertTestImage(
				"GreenHalfPixel.svg",
				b =>
				{
					Assert.That(b.Height, Is.EqualTo(1));
					Assert.That(b.Width, Is.EqualTo(1));
					AssertPixelEqualWithinTolerance(b, 0, 0, Color.FromRgba(0x00FF0080));
				});
		}

		[Test]
		public void Alpha_blending_on_transparent_background()
		{
			LoadAndAssertTestImage(
				"HalfTransparentBox.svg",
				b =>
				{
					Assert.That(b.Height, Is.EqualTo(4));
					Assert.That(b.Width, Is.EqualTo(4));
					AssertPixelEqualWithinTolerance(b, 2, 2, Color.FromRgba(0xc7c7c77f));
				});
		}

		static void AssertPixelEqualWithinTolerance(Bitmap b, int x, int y, Color expected)
		{
			var pixel = b.GetPixel(x, y);
			var actual = Color.FromBytes(pixel.R, pixel.G, pixel.B, pixel.A);
			var tolerance = 2 / 255.0;
			Assert.That(actual.R, Is.EqualTo(expected.R).Within(tolerance));
			Assert.That(actual.G, Is.EqualTo(expected.G).Within(tolerance));
			Assert.That(actual.B, Is.EqualTo(expected.B).Within(tolerance));
			Assert.That(actual.A, Is.EqualTo(expected.A).Within(tolerance));
		}

		static void LoadAndAssertTestImage(
			string resourceName,
			Action<Bitmap> assertions,
			Ratio<Pixels, Points> scaleFactor = default(Ratio<Pixels, Points>),
			Optional<IColorMap> colorMap = default(Optional<IColorMap>))
		{
			var image = Image.GetImageFromResource(
				typeof(ImageTests).Namespace + ".Images." + resourceName,
				typeof(ImageTests).Assembly);
			using (var s = image.Load<Stream>(scaleFactor, colorMap, cache: false).Image)
			{
				using (var bitmap = new Bitmap(s))
				{
					assertions(bitmap);
				}
			}
		}

		class AlwaysRedColorMap : IColorMap
		{
			public Color Map(Color color)
			{
				return Color.FromRgb(0xff0000);
			}
		}
	}
}