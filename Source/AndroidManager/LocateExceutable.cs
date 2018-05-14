using System;
using System.Diagnostics;
using System.Threading;
using Outracks.Diagnostics;
using Outracks.IO;

namespace Outracks.AndroidManager
{
	static class LocateExceutable 
	{
		public static Optional<AbsoluteFilePath> TryFindExecutableInPath(string name, IObserver<string> warnings)
		{
			try
			{
				if (Platform.OperatingSystem == OS.Windows)
				{
					var whereStartInfo = new ProcessStartInfo("where")
					{
						Arguments = '"' + name + '"',
						RedirectStandardError = true,
						RedirectStandardOutput = true,
						UseShellExecute = false,
					};

					var where = Process.Start(whereStartInfo);
					var path = @where.StandardOutput.ReadLine();
					@where.WaitForExit();

					return @where.ExitCode != 0 ? Optional.None() : AbsoluteFilePath.TryParse(path.Trim());
				}
				else if (Platform.OperatingSystem == OS.Mac)
				{
					Optional<AbsoluteFilePath> path = Optional.None();
					var result = ProcessHelper.StartProcess(
						new ProcessStartInfo("which", '"' + name + '"')
						{
							UseShellExecute = false,
							RedirectStandardOutput = true,
							RedirectStandardError = true
						},
						CancellationToken.None,
						which =>
						{
							var pathRaw = which.StandardOutput.ReadLine();
							if(pathRaw != null)
								path = AbsoluteFilePath.TryParse(pathRaw.Trim());
						});

					return result != 0 ? Optional.None() : path.Select<AbsoluteFilePath, AbsoluteFilePath>(ReadLinkIfLink);
				}
			}
			catch (Exception e)
			{
				warnings.OnNext("WARNING: " + e.Message);
				return Optional.None();
			}

			throw new PlatformNotSupportedException();
		}

		static AbsoluteFilePath ReadLinkIfLink(AbsoluteFilePath input)
		{
			Optional<AbsoluteFilePath> linkPointsToPath = Optional.None();
			var result = ProcessHelper.StartProcess(
				new ProcessStartInfo("readlink", '"' + input.NativePath + '"')
				{
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true
				},
				CancellationToken.None,
				readlink =>
				{
					var pathRaw = readlink.StandardOutput.ReadLine();
					linkPointsToPath = AbsoluteFilePath.TryParse(pathRaw.Trim());
				});

			return result != 0 ? input : linkPointsToPath.Or(input);
		}
	}
}