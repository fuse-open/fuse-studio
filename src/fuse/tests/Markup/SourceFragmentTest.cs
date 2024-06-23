using System.IO;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;

namespace Outracks.Fuse.Markup
{
	[TestFixture]
	class SourceFragmentTest
	{
		static readonly SourceFragment Fragment = SourceFragment.FromString("<App><DockPanel></DockPanel></App>");

		[Test]
		public static void ToStringIsRoundtripInvariant()
		{
			var fragmentString = Fragment.ToString();
			var trippedFragment = SourceFragment.FromString(fragmentString);
			Assert.AreEqual(Fragment, trippedFragment);
		}

		[Test]
		public static void ToXmlIsRoundtripInvariant()
		{
			var fragmentXml = Fragment.ToXml();
			var trippedFragment = SourceFragment.FromXml(fragmentXml);
			Assert.AreEqual(Fragment, trippedFragment);
		}

		[Test]
		public static void ToXmlReturnsXElementWithNoParent()
		{
			Assert.AreEqual(null, Fragment.ToXml().Parent);
		}

		[Test]
		public static void ToXmlReturnsXElementWithNoDocument()
		{
			Assert.AreEqual(null, Fragment.ToXml().Document);
		}

		[Test]
		public static void ToXmlReturnsNewXElement()
		{
			var element = new XElement("Tjoho");
			var fragment = SourceFragment.FromXml(element);
			Assert.AreNotEqual(element, fragment.ToXml());
		}

		[Test]
		public static void ToXmlThrowsOnInvalidUx()
		{
			Assert.Throws<XmlException>(() => SourceFragment.FromString("<App><asdf</App>").ToXml());
		}

		[Test]
		public static void WriteDataIsRoundtripInvariant()
		{
			using (var stream = new MemoryStream())
			using (var reader = new BinaryReader(stream))
			using (var writer = new BinaryWriter(stream))
			{
				Fragment.WriteTo(writer);
				stream.Seek(0, SeekOrigin.Begin);
				var trippedFragment = SourceFragment.ReadFrom(reader);
				Assert.AreEqual(Fragment, trippedFragment);
			}
		}
	}
}
