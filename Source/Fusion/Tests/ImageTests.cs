using NUnit.Framework;

namespace Outracks.Fusion.Tests
{
	[TestFixture]
	public class ImageTests
	{
		[Test]
		public void GetImageFromResource_given_resource_name_with_svg_extension_returns_SvgImage()
		{
			var image = Image.GetImageFromResource(
				typeof(ImageTests).Namespace + ".Images." + "GreenBox.svg",
				typeof(ImageTests).Assembly);
			Assert.That(image, Is.TypeOf<SvgImage>());
		}
	}
}