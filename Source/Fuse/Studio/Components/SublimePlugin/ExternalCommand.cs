using System.Diagnostics;
using System.IO;
using Outracks.IO;

namespace Outracks.Fuse.ModuleInstaller
{
	public abstract class ExternalCommand
	{
		public readonly string Command;
		public readonly Optional<string[]> Arguments;

		protected ExternalCommand(string command, string []arguments = null)
		{
			Command = command;
			Arguments = arguments.ToOptional();
		}

		public virtual Process Run(AbsoluteDirectoryPath workingDir)
		{
			var fileName = workingDir.NativePath + Path.DirectorySeparatorChar + Command;

			var processStartInfo = new ProcessStartInfo()
			{
				FileName = fileName,
				UseShellExecute = false,
				WorkingDirectory = workingDir.NativePath,
				RedirectStandardInput = false,
				RedirectStandardOutput = false,
				RedirectStandardError = false,
				Arguments = Arguments.Or(new string[0]).Join(" ")
			};

			var p = Process.Start(processStartInfo);
			return p;
		}
	}
}

