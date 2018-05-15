using System;
using NUnit.Framework;
using System.IO;
using NSubstitute;
using Outracks.Fuse;

namespace Outracks.Common.CLI.Tests
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
