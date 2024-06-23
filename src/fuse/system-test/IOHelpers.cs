using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Outracks.IO;

namespace Outracks.Fuse.SystemTest {
	public static class IOHelpers
	{
		public static void ReplaceTextInFile(AbsoluteFilePath mainView, string oldValue, string newValue)
		{
			var newText = File.ReadAllText(mainView.NativePath).Replace(oldValue, newValue);
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			while (true)
			{
				try
				{
					File.WriteAllText(mainView.NativePath, newText);
					return;
				}
				catch (Exception)
				{
					Thread.Sleep(Math.Max(stopWatch.Elapsed.Milliseconds, 100));
					if (stopWatch.Elapsed > TimeSpan.FromSeconds(5))
					{
						throw;
					}
				}
			}
		}

		public static void DeleteAndCopyDirectory(AbsoluteDirectoryPath source, AbsoluteDirectoryPath destination)
		{
			var shell = new Shell();
			if (Directory.Exists(destination.NativePath))
				shell.Delete(destination);
			shell.Copy(source, destination);
		}

		public static void DeleteIfExistsAndCreateDirectory(this IShell shell, AbsoluteDirectoryPath directory)
		{
			if (shell.Exists(directory))
			{
				shell.Delete(directory);
			}
			shell.Create(directory);
		}

		public static void WaitForFileToExist(AbsoluteFilePath file, TimeSpan timeout)
		{
			var w = new Stopwatch();
			w.Start();
			while (!File.Exists(file.NativePath))
			{
				if (w.Elapsed > timeout)
				{
					throw new Exception("Waited for " + timeout + ", but '" + file + "' didn't appear.");
				}
				Thread.Sleep(TimeSpan.FromSeconds(0.1));
			}
		}

		public static void AssertExists(Shell shell, IAbsolutePath mainView)
		{
			if (!shell.Exists(mainView))
			{
				throw new TestFailure("Expected to see '" + mainView + "', but it doesn't exist.");
			}
		}
	}
}