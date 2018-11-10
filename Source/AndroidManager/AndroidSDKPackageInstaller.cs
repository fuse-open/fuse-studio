using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Outracks.IO;

namespace Outracks.AndroidManager
{
	class AndroidPackageInstallResult
	{
		public readonly AbsoluteDirectoryPath NdkBundle;

		public AndroidPackageInstallResult(AbsoluteDirectoryPath ndkBundle)
		{
			NdkBundle = ndkBundle;
		}
	}

	class AndroidSDKPackageInstaller
	{
		readonly IFileSystem _fs;
		readonly AbsoluteDirectoryPath _androidSdkRoot;
		readonly AbsoluteFilePath _androidCmd;
		readonly AbsoluteFilePath _packageManager;

		public AndroidSDKPackageInstaller(IFileSystem fs, AbsoluteDirectoryPath androidSdkRoot, AbsoluteFilePath androidCmd, AbsoluteFilePath packageManager)
		{
			_fs = fs;
			_androidSdkRoot = androidSdkRoot;
			_androidCmd = androidCmd;
			_packageManager = packageManager;
		}

		public AndroidPackageInstallResult Install(CancellationToken ct, IDialog dialog, IProgress<InstallerEvent> installerProgress)
		{
			if (_fs.Exists(_androidCmd))
			{
				// The sdkmanager was introduced in 25.2.x, so we'll try to upgrade to that version
				int tries = 0;
				while (TryGetToolsVersion().Select(v => v < new Version(25, 2, 3)).Or(false))
				{
					UpdateTools(ct, installerProgress, "platform-tools");
					UpdateTools(ct, installerProgress, "tools");
					if (++tries == 5)
						throw new InstallerError("Failed to update Android SDK");
				}
			}

			InstallPackages(new []
			{
				"platform-tools",
				"ndk-bundle",
				"extras;android;m2repository",
				"extras;google;m2repository"
			}, ct, installerProgress);

			return new AndroidPackageInstallResult(
				_androidSdkRoot / new DirectoryName("ndk-bundle"));
		}

		void UpdateTools(CancellationToken ct, IProgress<InstallerEvent> progress, string package)
		{
			var psi = new ProcessStartInfo()
			{
				FileName = _androidCmd.NativePath,
				Arguments = "update sdk -s -u -a -t " + package,
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				RedirectStandardInput = true,
			};
			
			var exitCode = ProcessHelper.StartProcessWithProgress(
				psi,
				ct,
				process => process.StandardInput.WriteLine("y"),
				progress);

			if (exitCode != 0)
				throw new InstallerError("Failed to install update Android SDK tools");
		}		

		void InstallPackages(IEnumerable<string> packages, CancellationToken ct, IProgress<InstallerEvent> progress)
		{
			foreach (var package in packages)
			{
				progress.Report(new InstallerMessage("Installing " + package + " which may take a long while..."));
				InstallPackage(package, ct, progress);
			}
		}

		void InstallPackage(string package, CancellationToken ct, IProgress<InstallerEvent> progress)
		{
			var psi = new ProcessStartInfo()
			{
				FileName = _packageManager.NativePath,
				Arguments = "--no_https " + package,
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				RedirectStandardInput = true,
			};

			var exitCode = ProcessHelper.StartProcessWithProgress(
				psi,
				ct,
				process => process.StandardInput.WriteLine("y"),
				progress);

			if (exitCode != 0)
				throw new InstallerError("Failed to install package '" + package + "'");
		}

		Optional<Version> TryGetToolsVersion()
		{
			var buildToolsFolder = _androidSdkRoot / new DirectoryName("tools");
			if (!_fs.Exists(buildToolsFolder))
				return Optional.None();

			var buildToolsPackages = AndroidSDKPackageParser.SearchForPackagesIn(_fs, buildToolsFolder);
			var latestPackage = buildToolsPackages.OrderBy(p => p.SortableRevision).LastOrNone();

			return latestPackage.Select(package => Version.Parse(package.OriginalRevision));
		}
	}
}
