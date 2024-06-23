using NUnit.Framework;
using Outracks.Fuse.Components;
using Outracks.Fuse.Setup;

namespace Outracks.Fuse.Protocol.Tests.Setup
{
	class ComponentStatusExetensionsTest
	{
		[Test]
		public void SupportsAllValues()
		{
			foreach (var enumValue in typeof(ComponentStatus).GetEnumValues())
			{
				((ComponentStatus) enumValue).AsInstallStatus();
			}
		}

		[Test]
		public void TranslatesCorrectly()
		{
			Assert.AreEqual(InstallStatus.Installed, ComponentStatus.Installed.AsInstallStatus());
			Assert.AreEqual(InstallStatus.NotInstalled, ComponentStatus.NotInstalled.AsInstallStatus());
			Assert.AreEqual(InstallStatus.UpdateAvailable, ComponentStatus.UpdateAvailable.AsInstallStatus());
		}
	}
}
