using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Outracks.Common.Tests
{
	public static class Helpers
	{
		public static Process RunTestProcess(string argument, Assembly executingAssembly)
		{
			var testDir = new FileInfo(new Uri(executingAssembly.CodeBase).LocalPath).Directory;
			var testExe = Path.Combine(testDir.FullName, "Outracks.Common.TestProcess.exe");
			return Process.Start(testExe, argument);
		}
	}
}