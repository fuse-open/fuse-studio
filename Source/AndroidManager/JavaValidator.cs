using System;
using System.Diagnostics;
using System.Threading;
using Outracks.Diagnostics;
using Outracks.IO;

namespace Outracks.AndroidManager
{
	class JavacFetchVersionFailed : Exception
	{ }

	public class JavaValidator : IInstallerValidator
	{
		readonly IFileSystem _fs;

		public JavaValidator(IFileSystem fs)
		{
			_fs = fs;
		}

		public bool IsInstalledAt(AbsoluteDirectoryPath installPath, IProgress<InstallerEvent> progress)
		{
			var javacName = Platform.OperatingSystem == OS.Windows
				? new FileName("javac.exe")
				: new FileName("javac");
			var javac = installPath / new DirectoryName("bin") / javacName;

			return _fs.Exists(installPath)
				&& CheckIfCorrupt(progress, javac)
				&& IsVersionCompatibleWithFuse(progress, javac);
		}

		bool CheckIfCorrupt(IProgress<InstallerEvent> progress, AbsoluteFilePath javac)
		{
			if (!_fs.Exists(javac))
			{
				progress.Report(new InstallerMessage("The javac executable was expected to be found here " + javac.NativePath));
				return false;
			}

			return true;
		}

		bool IsVersionCompatibleWithFuse(IProgress<InstallerEvent> progress, AbsoluteFilePath javac)
		{
			try
			{
				var version = GetJavacVersion(javac);
				if (version < new Version(1, 8))
				{
					progress.Report(new InstallerMessage("Found JDK version " + version + ". However Fuse requires a JDK version that is 1.7 or higher"));
					return false;
				}
			}
			catch (JavacFetchVersionFailed)
			{
				return false;
			}

			return true;
		}

		Version GetJavacVersion(AbsoluteFilePath javac)
		{
			var javacVersion = new Version(0, 0);

			ProcessHelper.StartProcess(
				new ProcessStartInfo(javac.NativePath, "-version")
				{
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				},
				CancellationToken.None,
				process =>
				{
					var versionLine = process.StandardError.ReadLine();
					if (versionLine == null)
						throw new JavacFetchVersionFailed();

					Version version;
					if (!Version.TryParse(versionLine.Replace("javac ", "").Replace("_", ""), out version))
						throw new JavacFetchVersionFailed();

					javacVersion = version;
				});

			return javacVersion;
		}
	}
}

