using System;
using Outracks.IO;

namespace Outracks.Fuse.SystemTest
{
	class FuseBuild
	{
		public static void Run(AbsoluteDirectoryPath root, FuseRunner fuseRunner)
		{
			var shell = new Shell();
			var projectDir = root / ".generated" / "FuseBuild";
			var project = projectDir / new FileName("SystemTest1.unoproj");

			Console.WriteLine("Setting up project");
			IOHelpers.DeleteAndCopyDirectory(root / "projects" / "SystemTest1", projectDir);


			Console.WriteLine("Starting build");
			fuseRunner.Run("build " + project.NativePath, Optional.None())
				.WaitOrThrow(TimeSpan.FromMinutes(3))
				.AssertExitCode(0);
			IOHelpers.AssertExists(shell, projectDir / "build");

		}
	}
}
