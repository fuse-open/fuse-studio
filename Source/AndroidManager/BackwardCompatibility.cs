using System;
using Outracks.Diagnostics;
using Outracks.IO;

namespace Outracks.AndroidManager
{
	class BackwardCompatibility
	{
		static AbsoluteFilePath OldConfigPath
		{
			get
			{
				if (Platform.OperatingSystem == OS.Windows)
				{
					var configFilePath = PathExtensions.GetEnvironmentPath(Environment.SpecialFolder.LocalApplicationData)
						/ new DirectoryName("Fusetools")
						/ new DirectoryName("Fuse")
						/ new FileName("sdkConfig.json");

					return configFilePath;
				}
				if (Platform.OperatingSystem == OS.Mac)
				{
					var configFilePath = PathExtensions.GetEnvironmentPath(Environment.SpecialFolder.Personal)
						/ new DirectoryName(".fuse")
						/ new FileName("sdkConfig.json");

					return configFilePath;
				}

				throw new PlatformNotSupportedException();
			}
		}

		public static void RemoveOldConfigFileIfItExists(IFileSystem fs)
		{
			try
			{
				if (fs.Exists(OldConfigPath))
					fs.Delete(OldConfigPath);
			}
			catch (Exception)
			{
				// We don't care about errors.
			}			
		}

		public static void RemoveOldNdk(IFileSystem fs, AbsoluteDirectoryPath ndkPath)
		{
			try
			{
				var oldBasePath = Platform.OperatingSystem == OS.Mac 
					? PathExtensions.GetEnvironmentPath(Environment.SpecialFolder.Personal)
						/ new DirectoryName("Library")
						/ new DirectoryName("Application Support")
						/ new DirectoryName("Fusetools")
						/ new DirectoryName("Fuse")
						/ new DirectoryName("Android")
					: PathExtensions.GetEnvironmentPath(Environment.SpecialFolder.LocalApplicationData)
						/ new DirectoryName("Fusetools")
						/ new DirectoryName("Fuse")
						/ new DirectoryName("Android");

				if (ndkPath.IsRootedIn(oldBasePath) && fs.Exists(ndkPath))
					fs.Delete(ndkPath);
			}
			catch (Exception)
			{
				// We don't care about errors.
			}
		}
	}
}