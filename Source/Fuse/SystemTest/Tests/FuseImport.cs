using System;
using Outracks.IO;

namespace Outracks.Fuse.SystemTest
{
	class FuseImport
	{
		public static void Run(AbsoluteDirectoryPath root, FuseRunner fuseRunner)
		{
			var shell = new Shell();
			var projectDir = root / "GeneratedTestData" / "FuseImport";

			Console.WriteLine("Setting up project");
			IOHelpers.DeleteAndCopyDirectory(root / "Projects" / "SketchImportApp", projectDir);

			Console.WriteLine("Starting import");
			using (TestHelpers.ChangeWorkingDirectory(projectDir))
			{
				fuseRunner.Run("import Foo.sketch", Optional.None())
					.WaitOrThrow(TimeSpan.FromMinutes(1))
					.AssertExitCode(0);
				IOHelpers.AssertExists(shell, projectDir / "SketchSymbols" / new FileName("Sketch.Fuse.ux"));
			}
		}
	}
}
