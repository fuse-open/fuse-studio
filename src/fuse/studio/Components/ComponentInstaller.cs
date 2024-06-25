using System;
using System.Diagnostics;
using System.Text;
using Outracks.IO;
using Uno;

namespace Outracks.Fuse.Components
{
	public abstract class ComponentInstaller
	{
		public abstract void Install(); //TODO should take some sort of text writer / logger to write output to. Conosle from `fuse install`, otherwise log window in Fuse if possible.
		public abstract ComponentStatus Status { get; }

		public readonly string Name;
		public readonly string Description;
		protected ComponentInstaller(string name, string description)
		{
			Name = name;
			Description = description;
		}

		protected static ProcessResult RunScript(IAbsolutePath path, string args, TimeSpan timeout, AbsoluteDirectoryPath workingDir = null)
		{
			return RunProcess(AbsoluteFilePath.Parse("node"), path.NativePath.QuoteSpace() + " " + args, timeout, workingDir);
		}

		protected static ProcessResult RunProcess(IAbsolutePath path, string args, TimeSpan timeout, AbsoluteDirectoryPath workingDir = null )
		{
			var stdout = new StringBuilder();
			var stderr = new StringBuilder();
			var p = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = path.NativePath,
					Arguments = args,
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardError = true,
					RedirectStandardOutput = true
				}
			};
			if (workingDir != null)
			{
				p.StartInfo.WorkingDirectory = workingDir.NativePath;
			}
			p.OutputDataReceived += (s, e) => stdout.Append(e.Data + "\n");
			p.ErrorDataReceived += (s, e) => stderr.Append(e.Data + "\n");
			p.Start();
			p.BeginOutputReadLine();
			p.BeginErrorReadLine();
			var exited = p.WaitForExit((int)timeout.TotalMilliseconds);
			if (!exited)
			{
				throw new Exception("'" + p.StartInfo.FileName + "' failed to exit");
			}
			return new ProcessResult(p.ExitCode, stdout.ToString(), stderr.ToString());
		}
	}

	public struct ProcessResult
	{
		public readonly int ExitCode;
		public readonly string StdOut;
		public readonly string StdErr;
		public ProcessResult(int exitCode, string stdOut, string stdErr)
		{
			ExitCode = exitCode;
			StdOut = stdOut;
			StdErr = stdErr;
		}
	}

	public class PluginInstallerFailed : Exception
	{
		public PluginInstallerFailed(string msg) : base(msg) { }
	}

	public enum ComponentStatus
	{
		Installed = 0,
		NotInstalled = 100,
		UpdateAvailable = 200
	}
}
