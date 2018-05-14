using System;
using Outracks.IO;

namespace Outracks.Fuse.SystemTest
{
	public class PreviewTest
	{
		public static void Run(AbsoluteDirectoryPath root, FuseRunner fuseRunner)
		{
			var projectDir = root / "GeneratedTestData" / "SystemTest1";
			var project = projectDir / new FileName("SystemTest1.unoproj");
			var mainView = projectDir / new FileName("MainView.ux");
			var dataDir = projectDir / "build" / "Local" / "Designer" / "fs_data";
			var timeout = TimeSpan.FromMinutes(3);

			Console.WriteLine("Setting up project");
			IOHelpers.DeleteAndCopyDirectory(root / "Projects" / "SystemTest1", projectDir);

			try
			{
				Console.WriteLine("Starting preview");
				fuseRunner.Preview(project, Optional.None());
				IOHelpers.WaitForFileToExist(dataDir / new FileName("output1"), timeout);
				Console.WriteLine("Replacing text");
				IOHelpers.ReplaceTextInFile(mainView, "output1", "output2");
				IOHelpers.WaitForFileToExist(dataDir / new FileName("output2"), timeout);
			}
			catch (Exception e)
			{
				throw new TestFailure(e.Message);
			}
			finally
			{
				ScreenCapture.Shoot("PreviewTestBeforeKill.png");
				fuseRunner.KillOrThrow();
			}
		}
	}
}