using System.IO;
using NUnit.Framework;

namespace Fuse.Preview.Tests
{
	[TestFixture]
	class BinaryEncodingTest
	{
		[Test]
		public void Integers()
		{
			Test(1337);
			Test(-7);
			Test(0);
			Test(int.MaxValue);
			Test(int.MinValue);
			Test(new[] { 1, -4, 3301 });
		}

		[Test]
		public void Booleans()
		{
			Test(true);
			Test(false);
			Test(new[] { true, false, true, true });
		}

		[Test]
		public void Strings()
		{
			Test("C=");
			Test("");
			Test("  ");
			Test(new[] { "Hey this is a string..", "{}7}112[{[$€^£@ £12.2,,3|23425", " ...four" });
		}

		static void Test(object value)
		{
			using (var stream = new MemoryStream())
			using (var reader = new BinaryReader(stream))
			using (var writer = new BinaryWriter(stream))
			{
				writer.WriteTaggedValue(value);
				var length = stream.Position;
				stream.Seek(0, SeekOrigin.Begin);
				Assert.That(value, Is.EqualTo(reader.ReadTaggedValue()));
				Assert.That(stream.Position, Is.EqualTo(length));
			}
		}
	}
}
