using System;
using Outracks.IO;

namespace Outracks.AndroidManager
{
	public class AndroidSDKValidator : IInstallerValidator
	{
		readonly IFileSystem _fs;

		public AndroidSDKValidator(IFileSystem fs)
		{
			_fs = fs;
		}

		public bool IsInstalledAt(AbsoluteDirectoryPath installPath, IProgress<InstallerEvent> progress)
		{
			if (ElCapitanWorkaround.IsInvalidWithMessage(progress, installPath))
				return false;

			var androidTools = (installPath / new DirectoryName("tools"));

			return _fs.Exists(installPath)
				&& _fs.Exists(androidTools)
				&& CheckIfCorruptInstallation(installPath, progress)
				&& NotSpaceInPath(installPath, progress);
		}

		bool NotSpaceInPath(IAbsolutePath path, IProgress<InstallerEvent> progress)
		{
			if (path.NativePath.Contains(" "))
			{
				progress.Report(
					new InstallerMessage("The native build system for Android doesn't support having Android SDK in a path that contains space(s)."));
				return false;
			}

			return true;
		}

		bool CheckIfCorruptInstallation(AbsoluteDirectoryPath installPath, IProgress<InstallerEvent> progress)
		{
			var progressExists = _fs.Exists(installPath / new FileName(".progress"));
			if (progressExists)
				progress.Report(new InstallerMessage("Looks like a corrupt installation of Android SDK."));

			return !progressExists;
		}

		public bool AreRequiredPackagesInstalled(AbsoluteDirectoryPath installPath, IProgress<InstallerEvent> progress)
		{
			try
			{
				if (!IsPlatformToolsInstalled(progress, installPath)) return false;

				if (!IsAndroidSupportRepositoryInstalled(progress, installPath)) return false;

				if (!IsGoogleRepositoryInstalled(progress, installPath)) return false;

				if (!IsNdkInstalled(installPath)) return false;

				return true;
			}
			catch (InstallerError)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new InstallerError(e.Message);
			}
		}

		public bool IsNdkInstalled(AbsoluteDirectoryPath installPath)
		{
			var basePath = installPath / new DirectoryName("ndk-bundle");
			return _fs.Exists(basePath / new FileName("package.xml"));
		}

		public bool IsPlatformToolsInstalled(IProgress<InstallerEvent> progress, AbsoluteDirectoryPath installPath)
		{
			if (!_fs.Exists(installPath / new DirectoryName("platform-tools") / new FileName("package.xml")))
			{
				progress.Report(new InstallerMessage("Android Platform Tools was not found and is required"));
				return false;
			}

			return true;
		}

		public bool IsAndroidSupportRepositoryInstalled(IProgress<InstallerEvent> progress, AbsoluteDirectoryPath installPath)
		{
			var basePath = installPath / new DirectoryName("extras") / new DirectoryName("android") / new DirectoryName("m2repository");
			if (!_fs.Exists(basePath / new FileName("package.xml")))
			{
				progress.Report(new InstallerMessage("Android Support Repository was not found and is required since Fuse(v. 0.21.x)"));
				return false;
			}

			return true;
		}

		public bool IsGoogleRepositoryInstalled(IProgress<InstallerEvent> progress, AbsoluteDirectoryPath installPath)
		{
			var basePath = installPath / new DirectoryName("extras") / new DirectoryName("google") / new DirectoryName("m2repository");
			if (!_fs.Exists(basePath / new FileName("package.xml")))
			{
				progress.Report(new InstallerMessage("Google Repository was not found and is required since Fuse(v. 0.21.x)"));
				return false;
			}

			return true;
		}
	}
}