using System;
using System.Threading;
using Outracks.IO;

namespace Outracks.Fuse.SystemTest
{
	class AutomaticTestApp
	{
		public static void Run(AbsoluteDirectoryPath root, FuseRunner fuseRunner)
		{
			var projectDir = root / "GeneratedTestData" / "AutomaticTestApp" / "App";
			var project = projectDir / new FileName("App.unoproj");
			var timeout = TimeSpan.FromMinutes(3);

			Console.WriteLine("Setting up project");
			IOHelpers.DeleteAndCopyDirectory(root / "Stuff" / "AutomatictestApp" / "App", projectDir);

			var testAppError = false;
			try
			{
				var wait = new ManualResetEvent(false);

				Console.WriteLine("Starting preview");
				fuseRunner.Preview(project, Optional.Some<Action<string>>(
					s =>
					{
						if (s.Contains("TEST_APP_MSG:OK"))
						{
							wait.Set();
						}
						if (s.Contains("TEST_APP_MSG:ERROR"))
						{
							testAppError = true;
							wait.Set();
						}
					}));
				if (!wait.WaitOne(timeout))
				{
					throw new TestFailure("Test timed out after " + timeout);
				}
				if (testAppError)
				{
					throw new TestFailure("Test app failed");
				}
			}
			catch (Exception e)
			{
				throw new TestFailure(e.Message);
			}
			finally
			{
				ScreenCapture.Shoot("AutomaticTestApp-before-kill.png");
				fuseRunner.KillOrThrow();
			}
		}
	}
}
