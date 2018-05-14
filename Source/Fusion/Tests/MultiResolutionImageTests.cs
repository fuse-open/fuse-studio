using System.IO;
using NUnit.Framework;

namespace Outracks.Fusion.Tests
{
	[TestFixture]
	public class MultiResolutionImageTests
	{
		[Test]
		public void Load_returns_correct_image_version_for_various_optimal_scale_factors()
		{
			var x1Stream = new MemoryStream();
			var x2Stream = new MemoryStream();
			var multiResImage = new MultiResolutionImage(
				new[]
				{
					new ImageStream(1.0, () => x1Stream),
					new ImageStream(2.0, () => x2Stream)
				});
			var iv0_5 = multiResImage.Load<Stream>(0.5, cache: false);
			Assert.That(iv0_5.ScaleFactor.Value, Is.EqualTo(1.0));
			Assert.That(ReferenceEquals(iv0_5.Image, x1Stream));

			var iv1 = multiResImage.Load<Stream>(1, cache: false);
			Assert.That(iv1.ScaleFactor.Value, Is.EqualTo(1.0));
			Assert.That(ReferenceEquals(iv1.Image, x1Stream));

			var iv1_5 = multiResImage.Load<Stream>(1.5, cache: false);
			Assert.That(iv1_5.ScaleFactor.Value, Is.EqualTo(2.0));
			Assert.That(ReferenceEquals(iv1_5.Image, x2Stream));

			var iv2 = multiResImage.Load<Stream>(2, cache: false);
			Assert.That(iv2.ScaleFactor.Value, Is.EqualTo(2.0));
			Assert.That(ReferenceEquals(iv2.Image, x2Stream));

			var iv3 = multiResImage.Load<Stream>(3, cache: false);
			Assert.That(iv3.ScaleFactor.Value, Is.EqualTo(2.0));
			Assert.That(ReferenceEquals(iv3.Image, x2Stream));
		}
	}
}