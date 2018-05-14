using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Outracks.Diagnostics;
using Outracks.Fuse.Analytics;
using Outracks.IO;
using Uno.Configuration;

namespace Outracks.Fuse
{
	public class FuseRootDirectoryWasNotFound : Exception
	{
		public FuseRootDirectoryWasNotFound()
			: base("Fuse root directory was not found.")
		{
		}
	}

	class UnsupportedPlatformException : Exception
	{
		public UnsupportedPlatformException(OS platform)
			: base("Unsupported platform: " + platform.ToString())
		{
		}
	}

	class SpecialFoldersNotFound : Exception
	{
		public SpecialFoldersNotFound(IEnumerable<Environment.SpecialFolder> missingFolders)
			: base("The following special folder(s) are not set: " + string.Join(", ", missingFolders.Distinct().Select(f => "'" + f.ToString() + "'")) + ". Please make sure your OS is set up correctly.")
		{
		}
	}

	public static class FuseApi
	{
		public static IFuse Initialize(string programName, List<string> args)
		{
			var systemId = SystemGuidLoader.LoadOrCreateOrEmpty();
			var sessionId = Guid.NewGuid();

			var report = ReportFactory.GetReporter(systemId, sessionId, programName);
			AppDomain.CurrentDomain.ReportUnhandledExceptions(report);
			report.Info("Initializing with arguments '" + args.Join(" ") + "'");

			// Report running mono version
			Type type = Type.GetType("Mono.Runtime");
			if (type != null)
			{
				MethodInfo displayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
				if (displayName != null)
					report.Info("Running with Mono " + displayName.Invoke(null, null));
			}

			EnsureSpecialFoldersExist();

			var fuseExeOverride = args
				.TryParse("override-fuse-exe")
				.SelectMany(AbsoluteFilePath.TryParse);

			var rootDirSetByArgs = Optional.None<AbsoluteDirectoryPath>(); /*args
				.FirstOrNone(a => a.StartsWith("fuse-root"))
				.Select(a => a.Substring(a.IndexOf("=", StringComparison.InvariantCulture) + 1, a.Length - 1 - a.IndexOf("=", StringComparison.InvariantCulture)))
				.Select(AbsoluteDirectoryPath.Parse);*/

			var os = Platform.OperatingSystem;

			var fuseRoot = rootDirSetByArgs
				.Or(() => GetFuseRoot(os))
				.OrThrow(new FuseRootDirectoryWasNotFound());

			if (os != OS.Mac && os != OS.Windows)
				throw new UnsupportedPlatformException(os);

			var isMac = os == OS.Mac;


			var mono = isMac ? Optional.Some(FindMonoExe(fuseRoot)) : Optional.None();

			var codeAssistanceService = new FileName("Fuse.CodeAssistanceService.exe");
			
			var fuseExe = fuseExeOverride.Or(isMac
				? fuseRoot / "MacOS" / new FileName("Fuse")
				: fuseRoot / new FileName("Fuse.exe"));

			var unoExe = FindUnoExe(fuseRoot);
				
			var impl = new FuseImpl
			{
				FuseRoot = fuseRoot,
				Version = SystemInfoFactory.GetBuildVersion(),

				UnoExe = unoExe,
				FuseExe = fuseExe,
				MonoExe = mono,

				Fuse = ExternalApplication.FromNativeExe(fuseExe),
				
				// Tools

				Designer = isMac
					? ExternalApplication.FromAppBundle(fuseRoot / new FileName("Fuse Studio.app"))
					: ExternalApplication.FromNativeExe(fuseRoot / new FileName("Fuse Studio.exe")),
				
				// Services

				CodeAssistance = isMac
					? ExternalApplication.FromMonoExe(fuseRoot / "MonoBundle" / codeAssistanceService, mono)
					: ExternalApplication.FromNativeExe(fuseRoot / codeAssistanceService),

				Tray = isMac
					? ExternalApplication.FromNativeExe(fuseRoot / "Fuse Tray.app" / "Contents" / "MacOS" / new FileName("Fuse Tray"))
					: ExternalApplication.FromNativeExe(fuseRoot / new FileName("Fuse-Tray.exe")),

				LogServer = isMac
					? ExternalApplication.FromNativeExe(fuseRoot / new FileName("fuse-logserver"))
					: null,

				// Uno

				Uno = isMac
					? ExternalApplication.FromMonoExe(unoExe, mono)
					: ExternalApplication.FromNativeExe(unoExe),

				// System paths

				UserDataDir = isMac
					? AbsoluteDirectoryPath.Parse(Environment.GetFolderPath(Environment.SpecialFolder.Personal)) / ".fuse"
					: AbsoluteDirectoryPath.Parse(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) /
						"Fusetools" / "Fuse",

				ProjectsDir =
					AbsoluteDirectoryPath.Parse(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)) / "Fuse",

				SystemId = systemId,
				SessionId = sessionId,
				Report = report
			};

			var partial = args.Remove("--partial") | args.Remove("-p");
			if (!partial)
				impl.CheckCompleteness();

            // Set assembly configuration for .unoconfig, to be able to find correct bin/$(Configuration) directories.
#if DEBUG
		    UnoConfigFile.Constants["Configuration"] = "Debug";
#else
            UnoConfigFile.Constants["Configuration"] = "Release";
#endif
			return impl;
		}

		static AbsoluteFilePath FindUnoExe(AbsoluteDirectoryPath fuseRoot)
		{
			var unoExe = new FileName("uno.exe");

			var installedPath = Platform.OperatingSystem == OS.Mac 
				? fuseRoot / new DirectoryName("Uno") / unoExe
				: fuseRoot / unoExe;

			if (File.Exists(installedPath.NativePath))
				return installedPath;

			// We didn't find uno exe as it would be located in an installed version of Fuse
			// Maybe we're running directly in the source tree. Let's see if we find .unoversion file in parent dir..
			var repoRoot = fuseRoot;
			while (repoRoot != null)
			{
				var unoVersionPath = repoRoot / ".unoversion";
				if (File.Exists(unoVersionPath.NativePath))
				{
					var unoVer = File.ReadAllText(unoVersionPath.NativePath).Trim(" \t\r\n".ToCharArray());
					return repoRoot / "packages" / ("FuseOpen.Uno.Tool." + unoVer) / "tools" / unoExe;
				}

				repoRoot = repoRoot.ContainingDirectory;
			}

			// Just default to installed path
			return installedPath;
		}


		static AbsoluteFilePath FindMonoExe(AbsoluteDirectoryPath fuseRoot)
		{
			var monoRoot = File.ReadAllText((fuseRoot / new FileName(".mono_root")).NativePath);
			var monoPath = RelativeDirectoryPath.TryParse(monoRoot)
				.Select(monoRelative => fuseRoot / monoRelative)
				.Or(AbsoluteDirectoryPath.TryParse(monoRoot))
				.OrThrow(new InvalidPath(monoRoot, new Exception("Invalid mono root path.")));

			return monoPath / new DirectoryName("bin") / new FileName("mono");
		}

		static Optional<AbsoluteDirectoryPath> GetFuseRoot(OS os)
		{
			var codeBase = typeof(FuseApi).Assembly.GetCodeBaseFilePath();
			if (os == OS.Windows)
			{				
				return GetFuseRootWin(codeBase.ContainingDirectory);
			}
			else if (os == OS.Mac)
			{
				return GetFuseRootMac(codeBase.ContainingDirectory);
			}

			return Optional.None();
		}

		static Optional<AbsoluteDirectoryPath> GetFuseRootWin(AbsoluteDirectoryPath baseSearchDir)
		{			
			var fuseExe = Directory
				.GetFiles(baseSearchDir.NativePath)
				.Select(Path.GetFileName)
				.FirstOrNone(f => 
					f.Equals(RelativeFilePath.Parse("Fuse.exe").Name.ToString(), StringComparison.InvariantCultureIgnoreCase));
			
			if (fuseExe.HasValue)
				return baseSearchDir;
			
			if (baseSearchDir.ContainingDirectory != null)
				return GetFuseRootWin(baseSearchDir.ContainingDirectory);
			
			return Optional.None();
		}

		static Optional<AbsoluteDirectoryPath> GetFuseRootMac(AbsoluteDirectoryPath baseSearchDir)
		{
			var subDirs = HasSubdirectoryNamed(baseSearchDir, "Fuse Studio.app")
				? Optional.Some(Directory.GetDirectories(baseSearchDir.NativePath)) 
				: Optional.None();

			var fuseExe = subDirs
				.SelectMany(dirs => dirs.FirstOrNone(d => IsSubdirectoryOf("MacOS", d)))
				.Select(Directory.GetFiles)
				.SelectMany(files =>
					files
						.Select(Path.GetFileName)
						.FirstOrNone(f => f.Equals(RelativeFilePath.Parse("MacOS/Fuse").Name.ToString(), StringComparison.InvariantCultureIgnoreCase)));

			if (fuseExe.HasValue)
				return baseSearchDir;

			if (baseSearchDir.ContainingDirectory != null)
				return GetFuseRootMac(baseSearchDir.ContainingDirectory);

			return Optional.None();
		}

		static bool HasSubdirectoryNamed(IAbsolutePath containingDirectory, string subdirectoryName)
		{
			return Directory.GetDirectories(containingDirectory.NativePath)
				.Any(s => IsSubdirectoryOf(subdirectoryName, s));
		}

		static bool IsSubdirectoryOf(string subDirName, string fullPath)
		{
			var dirName = Path.GetFileName(fullPath);
			return dirName != null && dirName.Equals(subDirName, StringComparison.InvariantCultureIgnoreCase);
		}

		static void EnsureSpecialFoldersExist()
		{
			var requiredFolders = new[] { Environment.SpecialFolder.Personal, Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolder.MyDocuments };
			var missingFolders = requiredFolders.Where((f) => Environment.GetFolderPath(f).IsEmpty()).ToArray();
			if (missingFolders.Any())
			{
				throw new SpecialFoldersNotFound(missingFolders);
			}
		}
	}
}
