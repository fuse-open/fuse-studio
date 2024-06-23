using System;
using Outracks.IO;

namespace Outracks.Fuse.SystemTest
{
	public class FuseCreate
	{
		public static void Run(AbsoluteDirectoryPath root, FuseRunner fuseRunner)
		{
			var shell = new Shell();
			var testDir = root / ".generated" / "FuseCreate";
			var appDir = testDir / "appname";
			shell.DeleteIfExistsAndCreateDirectory(testDir);
			using (TestHelpers.ChangeWorkingDirectory(testDir))
			{
				fuseRunner.Run("create app appname", Optional.None())
					.WaitOrThrow(TimeSpan.FromSeconds(10))
					.AssertExitCode(0);
				IOHelpers.AssertExists(shell, appDir / new FileName("MainView.ux"));
				IOHelpers.AssertExists(shell, appDir / new FileName("appname.unoproj"));
			}
		}
	}
}