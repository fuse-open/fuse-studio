using System;
using System.Collections.Generic;
using System.Threading;
using Outracks.Diagnostics;
using Outracks.IO;

namespace Outracks.Fuse
{
	class DashboardCommand : DefaultCliCommand
	{
		public static DashboardCommand CreateDashboardCommand()
		{
			var fuse = FuseApi.Initialize("Dashboard", new List<string>());
			var Log = fuse.Report;
			var shell = new Shell();
			return new DashboardCommand(
				log: Log,
				fuseVersion: fuse.Version,
				fs: shell,
				userDataDir:
				fuse.UserDataDir,
				launchFuse: fuse,
				fuseKiller: new FuseKiller(Log, fuse.FuseRoot),
				outWriter: ColoredTextWriter.Out);
		}

		readonly Version _fuseVersion;
		readonly IFileSystem _fs;
		readonly IReport _log;
		readonly IFuseLauncher _launchFuse;
		readonly IFuseKiller _fuseKiller;
		readonly ColoredTextWriter _outWriter;
		readonly AbsoluteFilePath _versionFile;

		public DashboardCommand(
			Version fuseVersion,
			IFileSystem fs,
			IReport log,
			IFuseLauncher launchFuse, 
			IFuseKiller fuseKiller, 
			AbsoluteDirectoryPath userDataDir, 
			ColoredTextWriter outWriter)
			: base("dashboard", "Fire up the dashboard")
		{
			_fuseVersion = fuseVersion;
			_fs = fs;
			_log = log;
			_launchFuse = launchFuse;
			_fuseKiller = fuseKiller;
			_outWriter = outWriter;
			_versionFile = userDataDir / new FileName(".fuseVersion");
		}

		public override void Help() {}

		public override void Run(string[] args, CancellationToken ct)
		{
			KillFuseInstancesIfNeccessary();
			_launchFuse.StartFuse("open");
		}

		public override void RunDefault(string[] args, CancellationToken ct)
		{
			Run(args, ct);
		}

		void KillFuseInstancesIfNeccessary()
		{
			try
			{
				if (ShouldKillFuse(_fuseVersion))
				{
					TryWriteCurrentVersion(_fuseVersion);
					_fuseKiller.Execute(_outWriter);
				}
			}
			catch (Exception e)
			{
				_log.Exception("Failed to kill fuse instances", e);
			}			
		}

		bool ShouldKillFuse(Version version)
		{
			if (Platform.OperatingSystem != OS.Mac)
				return false;

			var currentInstalledVersion = GetCurrentInstalledVersion().Or(new Version());
			return currentInstalledVersion != version;
		}

		Optional<Version> GetCurrentInstalledVersion()
		{
			try
			{
				return Version.Parse(_fs.ReadAllText(_versionFile, 5));
			}
			catch (Exception e)
			{
				_log.Exception("Failed to get current version", e);	
				return Optional.None();
			}					
		}

		bool TryWriteCurrentVersion(Version version)
		{
			try
			{
				_fs.ReplaceText(_versionFile, version.ToString(4), 5);
				return true;
			}
			catch (Exception e)
			{
				_log.Exception("Failed to write version to disk", e);
				return false;
			}
		}
	}
}