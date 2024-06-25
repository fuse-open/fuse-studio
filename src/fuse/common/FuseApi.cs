using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Outracks.Diagnostics;
using Outracks.Fuse.Analytics;
using Outracks.Fuse.Auth;
using Outracks.IO;
using Uno.Configuration;
using Uno.Diagnostics;

namespace Outracks.Fuse
{
	class SpecialFoldersNotFound : Exception
	{
		public SpecialFoldersNotFound(IEnumerable<Environment.SpecialFolder> missingFolders)
			: base("The following special folder(s) are not set: " + string.Join(", ", missingFolders.Distinct().Select(f => "'" + f.ToString() + "'")) + ". Please make sure your OS is set up correctly.")
		{
		}
	}

	public static class FuseApi
	{
		public static IFuse Initialize(string programName, List<string> args, bool remoteLicense = false)
		{
			var systemId = SystemGuidLoader.LoadOrCreateOrEmpty();
			var sessionId = Guid.NewGuid();

			var report = ReportFactory.GetReporter(systemId, sessionId, programName);
			AppDomain.CurrentDomain.ReportUnhandledExceptions(report);
			report.Info("Initializing with arguments '" + args.Join(" ") + "'");

			if (MonoInfo.IsRunningMono)
				report.Info("Running with Mono version " + MonoInfo.GetVersion());

			EnsureSpecialFoldersExist();

			var userData = Platform.IsMac
				? AbsoluteDirectoryPath.Parse(Environment.GetFolderPath(Environment.SpecialFolder.Personal)) / ".fuse"
				: AbsoluteDirectoryPath.Parse(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) / "fuse X";

			var fuseExe = GetConfigFilePath("Fuse.Exe");

			var impl = new FuseImpl
			{
				FuseRoot = GetConfigDirectoryPath("Fuse.Bin"),
				Version = SystemInfoFactory.GetBuildVersion(typeof(FuseApi).Assembly),

				FuseExe = fuseExe,

				Fuse = ExternalApplication.FromNativeExe(fuseExe),

				// Tools

				Studio = Platform.IsMac
					? ExternalApplication.FromAppBundle(GetConfigDirectoryPath("Fuse.Studio"))
					: ExternalApplication.FromNativeExe(GetConfigFilePath("Fuse.Studio")),

				// Services

				CodeAssistance = ExternalApplication.FromNativeExe(GetConfigFilePath("Fuse.CodeAssistance")),

				Tray = Platform.IsMac
					? ExternalApplication.FromNativeExe(GetConfigDirectoryPath("Fuse.Tray") / "Contents" / "MacOS" / new FileName("fuse X (menu bar)"))
					: ExternalApplication.FromNativeExe(GetConfigFilePath("Fuse.Tray")),

				LogServer = Platform.IsMac
					? ExternalApplication.FromNativeExe(GetConfigFilePath("Fuse.LogServer"))
					: null,

				// Uno

				Uno = ExternalApplication.FromNativeExe(GetConfigFilePath("Uno.Command")),

				UnoHost = Platform.IsMac
					? ExternalApplication.FromNativeExe(GetConfigDirectoryPath("Fuse.UnoHost") / "Contents" / "MacOS" / new FileName("UnoHost"))
					: ExternalApplication.FromNativeExe(GetConfigFilePath("Fuse.UnoHost")),

				// System paths

				UserDataDir = userData,

				ProjectsDir =
					AbsoluteDirectoryPath.Parse(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)) / "fuse X",

				SystemId = systemId,
				SessionId = sessionId,
				Report = report,
				License = new License(userData, remoteLicense)
			};

			// Set global license flag
			impl.License.IsValid.Subscribe(value => FuseImpl._IsLicenseValid = value);

			var partial = args.Remove("--partial") | args.Remove("-p");
			if (!partial)
				impl.CheckCompleteness();

			return impl;
		}

		static FuseApi()
		{
			// Set assembly configuration for .unoconfig,
			// and be able to find correct bin/$(Configuration) directories.
#if DEBUG
			UnoConfigFile.Constants["Configuration"] = "Debug";
#else
			UnoConfigFile.Constants["Configuration"] = "Release";
#endif

			// Define STUDIO for .unoconfig.
			UnoConfigFile.Defines.Add("STUDIO");
		}

		static AbsoluteDirectoryPath GetConfigDirectoryPath(string key)
		{
			return AbsoluteDirectoryPath.Parse(UnoConfig.Current.GetFullPath(key));
		}

		static AbsoluteFilePath GetConfigFilePath(string key)
		{
			var absolute = UnoConfig.Current.GetFullPath(key);
#if DEBUG
			var relative = Path.Combine(Path.GetDirectoryName(typeof(FuseApi).Assembly.Location),
										Path.GetFileName(absolute));

			// Windows: Prefer services near the running module (npm run daemon).
			return Platform.IsWindows && File.Exists(relative)
				? AbsoluteFilePath.Parse(relative)
				: AbsoluteFilePath.Parse(absolute);
#else
			return AbsoluteFilePath.Parse(absolute);
#endif
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
