using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace RegressionTests
{
	internal static class CommandRunner
	{
		private static readonly string Command = FindCommand();

		private static string FindCommand()
		{
			var codeBase = Assembly.GetExecutingAssembly().CodeBase;
			var uri = new UriBuilder(codeBase);
			var path = Uri.UnescapeDataString(uri.Path);
			var assemblyDirectory = Path.GetDirectoryName(path);
			return Path.Combine(assemblyDirectory, "Command.exe");
		}

		public static CommandResult Run(string arguments)
		{
			var p = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = Command,
					Arguments = arguments,
					CreateNoWindow = true,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true
				}
			};
			var stdOut = new StringBuilder();
			var stdErr = new StringBuilder();
			p.OutputDataReceived += (sender, args) => {
				Console.WriteLine(args.Data);
				stdOut.AppendLine(args.Data);
			};
			p.ErrorDataReceived += (sender, args) => {
				Console.Error.WriteLine(args.Data);
				stdErr.AppendLine(args.Data);
			};
			p.Start();
			p.BeginOutputReadLine();
			p.BeginErrorReadLine();
			p.WaitForExit();
			return new CommandResult(p.ExitCode, stdOut.ToString(), stdErr.ToString());
		}
	}

	internal class CommandResult
	{
		public readonly int ExitCode;
		public readonly string StdOut;
		public readonly string StdErr;

		public CommandResult(int exitCode, string stdOut, string stdErr)
		{
			ExitCode = exitCode;
			StdOut = stdOut;
			StdErr = stdErr;
		}
	}
}
