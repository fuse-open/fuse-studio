using System;
using System.Diagnostics;
using System.IO;
using Outracks.IO;

namespace Outracks.Fuse.SystemTest
{
	public static class TestHelpers
	{
		public static Process WaitOrThrow(this Process process, TimeSpan timeout)
		{
			var ms = timeout.TotalMilliseconds;
			if (ms > int.MaxValue)
			{
				throw new TestFailure("Can't wait for more than " + int.MaxValue + "ms., you tried to wait for " + ms + " ms.");
			}
			if (!process.WaitForExit((int)ms))
			{
				throw new TestFailure("Waited " + timeout + " ms. for `fuse build`, timed out");
			}
			return process;
		}

		public static Process AssertExitCode(this Process process, int exitCode)
		{
			if (process.ExitCode != exitCode)
			{
				throw new TestFailure("Process exited with code " + process.ExitCode + ", expected " + exitCode);
			}
			return process;
		}

		public static IDisposable ChangeWorkingDirectory(AbsoluteDirectoryPath directory)
		{
			var current = Directory.GetCurrentDirectory();
			Directory.SetCurrentDirectory(directory.NativePath);
			return Disposable.Create(() => Directory.SetCurrentDirectory(current));
		}
	}
}
