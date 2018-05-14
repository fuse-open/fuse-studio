using System.IO;
using NUnit.Framework;
using Outracks.Diagnostics;
using Outracks.IO;

namespace Outracks.Common.Tests.Paths
{
	class FilePathTests
	{
		[Test]
		public void ParseAndMakeAbsolute_RelativeWithoutRoot()
		{
			Assert.That(
				FilePath.ParseAndMakeAbsolute("foo"),
				Is.EqualTo(DirectoryPath.GetCurrentDirectory() / new FileName("foo")));
		}

		[Test]
		public void ParseAndMakeAbsolute_AbsoluteWithoutRoot()
		{
			var nativePath = Platform.OperatingSystem == OS.Windows ? @"C:\foo" : "/foo";
			Assert.That(
				FilePath.ParseAndMakeAbsolute(nativePath),
				Is.EqualTo(AbsoluteFilePath.Parse(nativePath)));
		}

		[Test]
		public void ParseAndMakeAbsolute_RelativeWithRoot()
		{
			var root = Platform.OperatingSystem == OS.Windows ? @"C:\root" : "/root";
			var rootPath = AbsoluteDirectoryPath.Parse(root);
			Assert.That(
				FilePath.ParseAndMakeAbsolute("foo", rootPath),
				Is.EqualTo(rootPath / new FileName("foo")));
		}

		[Test]
		public void ParseAndMakeAbsolute_AbsoluteWithRoot()
		{
			var root = Platform.OperatingSystem == OS.Windows ? @"C:\root" : "/root";
			var foo = Platform.OperatingSystem == OS.Windows ? @"C:\foo" : "/foo";
			Assert.That(
				FilePath.ParseAndMakeAbsolute(foo, AbsoluteDirectoryPath.Parse(root)),
				Is.EqualTo(AbsoluteFilePath.Parse(foo)));
		}
	}
}
