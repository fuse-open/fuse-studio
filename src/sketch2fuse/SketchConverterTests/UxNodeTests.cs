using NUnit.Framework;
using SketchImporter.UxGenerator;

namespace SketchConverterTests
{
	public class UxNodeTests
	{
		[Test]
		public void WritesCommentsForNullNodes()
		{
			var uxNode = new NullNode(new UxComment("Please skip me"));
			var ux = uxNode.SerializeUx(new UxSerializerContext());
			Assert.That(ux, Is.EqualTo("<!-- Please skip me -->"));
		}
	}
}
