using System.IO;
using NUnit.Framework;
using Outracks.Fuse;

namespace Outracks.CLI.Tests
{
	class VersionWriterTests
	{
		[Test]
		public void WritesVersion()
		{
			var writer = new StringWriter() {  NewLine = "\n" };
			VersionWriter.Write(writer, "1.2.3-moo");
			Assert.AreEqual("Fuse 1.2.3-moo\n", writer.ToString());
		}
	}
}
