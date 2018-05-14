using System.Runtime.CompilerServices;
using NUnit.Framework;
using Outracks.Fuse.Components;
using Outracks.IO;

namespace Outracks.Fuse.Protocol.Tests.Components
{
	class ComponentInstallerTests
	{
		static readonly AbsoluteDirectoryPath ModulesPath = SetModulesPath();

		static AbsoluteDirectoryPath SetModulesPath([CallerFilePath] string thisFile = "" )
		{
			return AbsoluteFilePath.Parse(thisFile).ContainingDirectory / ".." / ".." / ".." / ".." / "Modules";
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
			Assert.AreEqual(ComponentStatus.Installed, new VsCodePlugin().Status);
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
			Assert.AreEqual(ComponentStatus.NotInstalled, new VsCodePlugin().Status);
			new VsCodePlugin().Install();
			Assert.AreEqual(ComponentStatus.NotInstalled, new VsCodePlugin().Status);
		}

		[Test]
		[Ignore("Just for manual testing while developing")]
		public void SublimePlugin_Install_ManualTest()
		{
			Assert.AreEqual(ComponentStatus.NotInstalled, new SublimePlugin(ModulesPath).Status);
			new SublimePlugin(ModulesPath).Install();
			Assert.AreEqual(ComponentStatus.Installed, new SublimePlugin(ModulesPath).Status);
		}

	}
}
