using System;
using Outracks.IO;

namespace Outracks.AndroidManager
{
	class InstallationValidator {
		public static OptionalSdkConfigOptions VerifyInstallation(IFileSystem fs, OptionalSdkConfigOptions config, IProgress<InstallerEvent> progress)
		{			
			// Verify everything
			var javaValidator = new JavaValidator(fs);
			if (!IsInstalled(config.JavaJdkDirectory, javaValidator.IsInstalledAt, progress))
			{
				config.JavaJdkDirectory = Optional.None();
			}

			var androidSdkValidator = new AndroidSDKValidator(fs);
			if (!IsInstalled(config.AndroidSdkDirectory, androidSdkValidator.IsInstalledAt, progress))
			{
				config.AndroidSdkDirectory = Optional.None();
			}

			config.HaveAllSdkPackages = IsInstalled(config.AndroidSdkDirectory, androidSdkValidator.AreRequiredPackagesInstalled, progress);

			return config;
		}

		public static bool IsInstalled(Optional<AbsoluteDirectoryPath> installPath, Func<AbsoluteDirectoryPath, IProgress<InstallerEvent>, bool> isInstalledAt, IProgress<InstallerEvent> progress)
		{
			return installPath.HasValue &&
				isInstalledAt(installPath.Value, progress);
		}
	}
}