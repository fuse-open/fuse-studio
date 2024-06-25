using System.Runtime.CompilerServices;
using NUnit.Framework;
using Outracks.Fuse.Components;
using Outracks.IO;

namespace Outracks.Fuse.Protocol.Tests.Components
{
	class ComponentInstallerTests
	{
		static readonly AbsoluteDirectoryPath ComponentsPath = SetComponentsPath();

		static AbsoluteDirectoryPath SetComponentsPath([CallerFilePath] string thisFile = "")
		{
			return AbsoluteFilePath.Parse(thisFile).ContainingDirectory / ".." / ".." / ".." / ".." / "components";
		}

		[Test]
		[Ignore("Just for manual testing while developing")]
		public void AtomPlugin_IsInstalled_ManualTest()
		{
			Assert.AreEqual(ComponentStatus.Installed, new AtomPlugin().Status);
		}

		[Test]
		[Ignore("Just for manual testing while developing")]
		public void VsCodePlugin_IsInstalled_ManualTest()
		{
			Assert.AreEqual(ComponentStatus.Installed, new VsCodeExtension(ComponentsPath).Status);
		}

		[Test]
		[Ignore("Just for manual testing while developing")]
		public void AtomPlugin_Install_ManualTest()
		{
			Assert.AreEqual(ComponentStatus.NotInstalled, new AtomPlugin().Status);
			new AtomPlugin().Install();
			Assert.AreEqual(ComponentStatus.Installed, new AtomPlugin().Status);
		}

		[Test]
		[Ignore("Just for manual testing while developing")]
		public void VsCodePlugin_Install_ManualTest()
		{
			Assert.AreEqual(ComponentStatus.NotInstalled, new VsCodeExtension(ComponentsPath).Status);
			new VsCodeExtension(ComponentsPath).Install();
			Assert.AreEqual(ComponentStatus.NotInstalled, new VsCodeExtension(ComponentsPath).Status);
		}

		[Test]
		[Ignore("Just for manual testing while developing")]
		public void SublimePlugin_Install_ManualTest()
		{
			Assert.AreEqual(ComponentStatus.NotInstalled, new SublimePlugin(ComponentsPath).Status);
			new SublimePlugin(ComponentsPath).Install();
			Assert.AreEqual(ComponentStatus.Installed, new SublimePlugin(ComponentsPath).Status);
		}

	}
}
