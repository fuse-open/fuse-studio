using NUnit.Framework;
using Outracks.Diagnostics;
using Outracks.Fusion;

namespace Outracks.Fuse.Protocol.Tests
{
	class ApplicationPathsTests
	{
		[SetUp]
		public void SetUp()
		{
			Application.Initialize(new string[0]);
		}

		[Test]
		[Ignore("Just for manual testing while developing")]
		public void Sublime_ManualTest()
		{
			var path = ApplicationPaths.SublimeTextPath();
			if (Platform.IsWindows)
				Assert.AreEqual(@"C:\Program Files\Sublime Text 3", path.Value.NativePath);
			else
				Assert.AreEqual(@"TODO", path.Value.NativePath);
		}

		[Test]
		[Ignore("Just for manual testing while developing")]
		public void VsCode_ManualTest()
		{
			var path = ApplicationPaths.VsCodePath();
			if (Platform.IsWindows)
				Assert.AreEqual(@"C:\Program Files\Microsoft VS Code", path.Value.NativePath);
			else
				Assert.AreEqual(@"TODO", path.Value.NativePath);
		}

		[Test]
		[Ignore("Just for manual testing while developing")]
		public void Atom_ManualTest()
		{
			var path = ApplicationPaths.AtomPath();
			if (Platform.IsWindows)
				Assert.AreEqual(@"C:\Users\knatten\AppData\Local\atom\app-1.19.4", path.Value.NativePath);
			else
				Assert.AreEqual(@"TODO", path.Value.NativePath);
		}
	}
}
