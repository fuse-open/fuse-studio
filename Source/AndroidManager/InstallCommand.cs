using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading;
using CommandLine;
using Outracks.Diagnostics;
using Outracks.IO;

namespace Outracks.AndroidManager
{
	[Verb("install", HelpText = "Installs or updates all dependencies used during Android build & deployment")]
	public class InstallOptions
	{
		[Option('i', "non-interactive", HelpText = "Run the installer in non-interactive mode.")]
		public bool Noninteractive { get; set; }
	}

	public class InstallCommand
	{
		readonly IFileSystem _fs;
		readonly ConfigLoader _configLoader;
		readonly IDialog _dialog;
		readonly IProgress<InstallerEvent> _progress;

		public InstallCommand(IFileSystem fs, ConfigLoader configLoader, IDialog dialog, IProgress<InstallerEvent> progress)
		{
			_fs = fs;
			_configLoader = configLoader;
			_dialog = dialog;
			_progress = progress;
		}

		public void Run(InstallOptions opts)
		{
			BackwardCompatibility.RemoveOldConfigFileIfItExists(_fs);
			var config = InstallationValidator.VerifyInstallation(_fs, _configLoader.GetCurrentConfig(), _progress);

			try
			{
				Install(config, opts);
			}
			finally
			{
				// Save config
				_configLoader.Save(config);
			}
		}

		void Install(OptionalSdkConfigOptions config, InstallOptions opts)
		{
			// Remove old NDK installation
			config.AndroidNdkDirectory.Do(ndkPath => BackwardCompatibility.RemoveOldNdk(_fs, ndkPath));

			if (config.JavaJdkDirectory.HasValue == false)
			{
				var javaValidator = new JavaValidator(_fs);
				config.JavaJdkDirectory = TryToFindJdk(_fs, javaValidator, _progress)
					.Or(() => opts.Noninteractive ? Optional.None() : AskUserToProvidePath("Java Development Kit", javaValidator, _progress));
			}

			if (config.AndroidSdkDirectory.HasValue == false)
			{
				var androidSdkValidator = new AndroidSDKValidator(_fs);
				config.AndroidSdkDirectory = TryToFindAndroidSdk(androidSdkValidator, _progress)
					.Or(() => opts.Noninteractive ? Optional.None() : AskUserToProvidePath("Android SDK", androidSdkValidator, _progress));
			}

			// Then install based on configuration
			config.JavaJdkDirectory = config.JavaJdkDirectory.MatchWith(
				_ => _,
				() =>
				{
					var defaultPath = Platform.OperatingSystem == OS.Windows
						? PathExtensions.GetEnvironmentPath(Environment.SpecialFolder.ProgramFiles)
							/ new DirectoryName("Java")
							/ new DirectoryName("jdk1.8.0_40")
						: AbsoluteDirectoryPath.Parse("/Library/Java/JavaVirtualMachines")
							/ new DirectoryName("jdk1.8.0_40.jdk")
							/ new DirectoryName("Contents")
							/ new DirectoryName("Home");

					var installer = new JavaInstaller(_fs);
					installer.Install(CancellationToken.None, _dialog, _progress, opts);
					return defaultPath;
				});

			config.JavaJdkDirectory.Do(jdk => Environment.SetEnvironmentVariable("JAVA_HOME", jdk.NativePath, EnvironmentVariableTarget.Process));

			config.AndroidSdkDirectory = config.AndroidSdkDirectory.MatchWith(
				_ => _,
				() =>
				{
					var defaultPath = Platform.OperatingSystem == OS.Mac
						? PathExtensions.GetEnvironmentPath(Environment.SpecialFolder.Personal)
							/ new DirectoryName("Library")
							/ new DirectoryName("Android")
							/ new DirectoryName("sdk")
						: PathExtensions.GetEnvironmentPath(Environment.SpecialFolder.LocalApplicationData)
							/ new DirectoryName("Fusetools")
							/ new DirectoryName("Fuse")
							/ new DirectoryName("Android")
							/ new DirectoryName("AndroidSDK");

					var installer = new AndroidSDKInstaller(defaultPath, _fs);
					installer.Install(CancellationToken.None, _dialog, _progress, opts);
					return defaultPath;
				});

			config.AndroidSdkDirectory.Do(
				androidRoot =>
				{
					if (config.HaveAllSdkPackages 
						&& config.AndroidNdkDirectory.HasValue)
						return;

					var installer = new AndroidSDKPackageInstaller(
						_fs,
						androidRoot,
						Platform.OperatingSystem == OS.Windows
							? androidRoot / new DirectoryName("tools") / new FileName("android.bat")
							: androidRoot / new DirectoryName("tools") / new FileName("android"),
						Platform.OperatingSystem == OS.Windows
							? androidRoot / new DirectoryName("tools") / new DirectoryName("bin") / new FileName("sdkmanager.bat")
							: androidRoot / new DirectoryName("tools") / new DirectoryName("bin") / new FileName("sdkmanager"));

					var result = installer.Install(CancellationToken.None, _dialog, _progress);

					config.AndroidNdkDirectory = result.NdkBundle;
				});
		}

		Optional<AbsoluteDirectoryPath> TryToFindJdk(
			IFileSystem fs,
			JavaValidator javaVerifier, 
			IProgress<InstallerEvent> progress)
		{
			var searchPaths = GetJdkSearchPaths(fs);
			foreach (var searchPath in searchPaths)
			{
				if(javaVerifier.IsInstalledAt(searchPath, progress))
					return searchPath;
			}			
			
			// Install Jdk since we couldn't find it
			return Optional.None();
		}

		public IEnumerable<AbsoluteDirectoryPath> GetJdkSearchPaths(IFileSystem fs)
		{
			var defaultInstallLocations = Enumerable.Empty<AbsoluteDirectoryPath>();
			if (Platform.OperatingSystem == OS.Windows)
			{
				var defaultSearchLocation = PathExtensions.GetEnvironmentPath(Environment.SpecialFolder.ProgramFiles)
							/ new DirectoryName("Java");

				if(fs.Exists(defaultSearchLocation))
					defaultInstallLocations = fs.GetDirectories(defaultSearchLocation, "jdk*").Reverse();

				return defaultInstallLocations;
			}						
			else if (Platform.OperatingSystem == OS.Mac)
			{
				var javaHome = Environment
					.GetEnvironmentVariable("JAVA_HOME")
					.ToOptional()
					.SelectMany(AbsoluteDirectoryPath.TryParse);

				var defaultSearchLocation = AbsoluteDirectoryPath.Parse("/Library/Java/JavaVirtualMachines");
				if (fs.Exists(defaultSearchLocation))
				{
					var jdkBaseDirectories = fs.GetDirectories(defaultSearchLocation, "jdk*").Reverse();
					defaultInstallLocations = jdkBaseDirectories.Select(
						dir =>
							dir / new DirectoryName("Contents") / new DirectoryName("Home"));
				}

				return new[]
					{
						javaHome,
					}
					.NotNone()
					.Concat(defaultInstallLocations);
			}

			throw new PlatformNotSupportedException();
		}

		Optional<AbsoluteDirectoryPath> TryToFindAndroidSdk(AndroidSDKValidator sdkValidator, IProgress<InstallerEvent> progress)
		{
			var searchPaths = GetAndroidSDKSearchPaths(Observer.Create<string>(Console.WriteLine));
			foreach (var searchPath in searchPaths)
			{
				if (sdkValidator.IsInstalledAt(searchPath, progress))
					return searchPath;
			}

			return Optional.None();
		}

		public static IEnumerable<AbsoluteDirectoryPath> GetAndroidSDKSearchPaths(IObserver<string> warnings)
		{
			var androidSdkManager = LocateExceutable.TryFindExecutableInPath("SDK Manager", warnings).Select(path => path.ContainingDirectory);
			var adb = LocateExceutable.TryFindExecutableInPath("adb", warnings).Select(path => path.ContainingDirectory.ContainingDirectory);
			if (Platform.OperatingSystem == OS.Windows)
			{
				return new[]
				{
					androidSdkManager,
					adb,
						PathExtensions.GetEnvironmentPath(Environment.SpecialFolder.LocalApplicationData) 
						/ new DirectoryName("Android") 
						/ new DirectoryName("sdk")
				}.NotNone();
			}
			else if (Platform.OperatingSystem == OS.Mac)
			{
				return new[]
				{
					androidSdkManager,
					adb,
					PathExtensions.GetEnvironmentPath(Environment.SpecialFolder.Personal)
						/ new DirectoryName("Library") 
						/ new DirectoryName("Android") 
						/ new DirectoryName("sdk")
				}.NotNone();
			}

			throw new PlatformNotSupportedException();
		}

		Optional<AbsoluteDirectoryPath> AskUserToProvidePath(string packageName, IInstallerValidator validator, IProgress<InstallerEvent> progress)
		{
			DialogPathResult result;
			do
			{
				result = _dialog.Start(new DialogQuestion<DialogPathResult>(
					packageName + " wasn't found or an incompatible version of " + packageName + " found.\n"
					+ "If you have an existing version of " + packageName + ", please specify its path here. If not, just press Enter to continue:\n"));

				var path = result.DirectoryPath;
				if (path.HasValue && validator.IsInstalledAt(path.Value, progress))
				{
					return path;
				}

			} while (!result.Empty);

			return Optional.None();
		}
	}
}