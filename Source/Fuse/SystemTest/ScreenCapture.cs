using System;
using System.Diagnostics;
using System.Threading;
using Outracks.Diagnostics;
using Outracks.IO;

namespace Outracks.Fuse.SystemTest
{
	public static class ScreenCapture
	{
		public static void Shoot(string name)
		{
			try
			{
				var shell = new Shell();

				var dir = AbsoluteDirectoryPath.Parse(Environment.CurrentDirectory).Combine("diagnostics");
				shell.Create(dir);
				var file = dir.Combine(new FileName(name));

				// Not taking screenshots on other platforms than Mac right now,
				// The preview tests running on Windows is not that unstable, so not currently important to do that.
				if (Platform.OperatingSystem != OS.Mac)
					return;

				var psi = new ProcessStartInfo("screencapture", "\"" + file.NativePath + "\"")
				{
					UseShellExecute = false,
					CreateNoWindow = true
				};
				var p = Process.Start(psi);
				if (p == null)
				{
					throw new InvalidOperationException("Unable to start screencapture process");
				}

				if (!p.WaitForExit(5000))
				{
					p.Kill();
					throw new InvalidOperationException("Timeout while trying to take screenshot");
				}
				if (p.ExitCode != 0)
					throw new InvalidOperationException("Got exit code " + p.ExitCode + " while trying to take screenshot");

			}
			catch (Exception e)
			{
				Console.WriteLine("Unable to take screenshot, because: " + e);
			}
		}
	}
}