using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Outracks.Diagnostics;
using Outracks.IO;

namespace Outracks.AndroidManager
{
	class AndroidSDKInstaller
	{
		readonly AbsoluteDirectoryPath _installPath;
		readonly Uri _downloadUrl;
		readonly IFileSystem _fileSystem;

		public AndroidSDKInstaller(
			AbsoluteDirectoryPath installPath,
			IFileSystem fileSystem)
		{
			_installPath = installPath;
			_downloadUrl = Platform.OperatingSystem == OS.Windows
				? new Uri("https://dl.google.com/android/repository/sdk-tools-windows-4333796.zip")
				: new Uri("https://dl.google.com/android/repository/sdk-tools-darwin-4333796.zip");
			_fileSystem = fileSystem;
		}

		public void Install(CancellationToken ct, IDialog dialog, IProgress<InstallerEvent> installerProgress, InstallOptions opts)
		{
			if (_installPath.NativePath.Contains(" "))
			{
				throw new InstallerError("Android SDK can't be installed to a path that contains spaces '" + _installPath
					+ "'.\nPlease install Android SDK manually from: " + _downloadUrl);
			}

			if (opts.Noninteractive == false)
			{
				var result = dialog.ShowLicenseDialog(LicenseText);
				if (result.No)
				{
					throw new InstallerError("Aborting because license was not accepted.");
				}
			}

			InternalDownload(_installPath, ct, installerProgress);
		}

		void InternalDownload(AbsoluteDirectoryPath installPath, CancellationToken ct, IProgress<InstallerEvent> installerProgress)
		{
			var toolsPath = installPath / new DirectoryName("tools");
			if (!_fileSystem.Exists(toolsPath) || _fileSystem.Exists(toolsPath / new FileName(".progress")))
			{
				installerProgress.DoInstallerStep("Starting download of Android SDK",
					() =>
					{
						ZipHelper.DownloadZip(ct, _downloadUrl, toolsPath, installerProgress, _fileSystem);
						if (Platform.OperatingSystem == OS.Mac)
						{
							Mono.Unix.Native.Syscall.chmod(
								(toolsPath / new DirectoryName("bin") / new FileName("sdkmanager")).NativePath, Mono.Unix.Native.FilePermissions.ACCESSPERMS);
						}
					},
					e =>
					{
						installerProgress.Report(new InstallerMessage(e.Message + "\nTrying to delete " + installPath.NativePath, InstallerMessageType.Error));
						_fileSystem.Delete(installPath);
					});
			}
			else
			{
				installerProgress.Report(new InstallerMessage("Looks like Android SDK core features is already installed"));
			}

			installerProgress.Report(new InstallerStep("Succesfully installed Android SDK"));
		}

		string LicenseText
		{
			get
			{
				return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Outracks.AndroidManager.android_sdk_license.txt"))
					.ReadToEnd();
			}
		}
	}
}