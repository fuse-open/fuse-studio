using System;
using Microsoft.Deployment.WindowsInstaller;
using NUnit.Framework;

namespace Fuse.Installer.Actions.Tests
{
	public class TestFuseKill
	{
		[Test, Ignore("This is for internal testing of our fuse killing method.")]
		public void TestFuseKillTest0()
		{
			FuseKillerService.ForceKillAllFuseInstances();
		}
	}
}
