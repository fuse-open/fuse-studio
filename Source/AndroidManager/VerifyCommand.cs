using System;
using Outracks.IO;

namespace Outracks.AndroidManager
{
	public class VerifyCommand 
	{
		readonly IFileSystem _fs;
		readonly ConfigLoader _configLoader;
		readonly IProgress<InstallerEvent> _progress;

		public VerifyCommand(IFileSystem fs, ConfigLoader configLoader, IProgress<InstallerEvent> progress)
		{
			_fs = fs;
			_configLoader = configLoader;
			_progress = progress;
		}

		public bool Run()
		{
			var newConfig = InstallationValidator.VerifyInstallation(_fs, _configLoader.GetCurrentConfig(), _progress);
			var failed = false;
			if (newConfig.JavaJdkDirectory.HasValue == false)
			{
				failed = true;
				_progress.Report(new InstallerMessage("Failed to find JDK."));				
			}

			if (newConfig.AndroidSdkDirectory.HasValue == false)
			{
				failed = true;
				_progress.Report(new InstallerMessage("Failed to find Android SDK."));
			}

			if (newConfig.AndroidNdkDirectory.HasValue == false)
			{
				failed = true;
				_progress.Report(new InstallerMessage("Failed to find NDK."));
			}

			if (newConfig.AndroidSdkBuildToolsVersion.HasValue == false)
			{
				failed = true;
				_progress.Report(new InstallerMessage("Failed to find Android Build tools."));
			}

			if (newConfig.CMake.HasValue == false)
			{
				failed = true;
				_progress.Report(new InstallerMessage("Failed to find CMake."));
			}


			if (newConfig.HaveAllSdkPackages == false)
			{
				failed = true;
				_progress.Report(new InstallerMessage("Failed to find one or more Android SDK packages."));
			}

			return failed == false;
		}
	}
}