using System;
using Outracks.IO;

namespace Outracks.Fuse.SystemTest
{
	class FuseInstallAndroid
	{
		public static void Run(AbsoluteDirectoryPath root, FuseRunner fuseRunner)
		{
			var exitCode = fuseRunner.Run("install -s android", Optional.None())
				.WaitOrThrow(TimeSpan.FromMinutes(1))
				.ExitCode;
			const int isInstalled = 0;
			const int isNotInstalled = 100;
			const int updateAvailable = 200;
			if (exitCode != isInstalled && exitCode != isNotInstalled && exitCode != updateAvailable)
			{
				throw new TestFailure("Unexpected exit code " + exitCode);
			}
		}
	}
}
