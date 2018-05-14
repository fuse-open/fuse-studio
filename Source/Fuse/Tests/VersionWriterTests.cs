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
			var writer = Substitute.For<TextWriter>();
			VersionWriter.Write(writer, new Version(1, 2, 3, 4));
			writer.Received().WriteLine("Fuse 1.2.3 (build 4)");
		}
	}
}
