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
			var fuse = FuseApi.Initialize("fuse", new List<string>());
			var Log = fuse.Report;
			var shell = new Shell();
			return new DashboardCommand(
				log: Log,
				fuseVersion: fuse.Version,
				fs: shell,
				userDataDir: fuse.UserDataDir,
				studio: fuse.Studio,
				fuseKiller: new FuseKiller(Log, fuse.FuseRoot),
				outWriter: ColoredTextWriter.Out);
		}

		readonly string _fuseVersion;
		readonly IFileSystem _fs;
		readonly IReport _log;
		readonly IExternalApplication _studio;
		readonly IFuseKiller _fuseKiller;
		readonly ColoredTextWriter _outWriter;
		readonly AbsoluteFilePath _versionFile;

		public DashboardCommand(
			string fuseVersion,
			IFileSystem fs,
			IReport log,
			IExternalApplication studio,
			IFuseKiller fuseKiller,
			AbsoluteDirectoryPath userDataDir,
			ColoredTextWriter outWriter)
			: base("dashboard", "Fire up the dashboard")
		{
			_fuseVersion = fuseVersion;
			_fs = fs;
			_log = log;
			_studio = studio;
			_fuseKiller = fuseKiller;
			_outWriter = outWriter;
			_versionFile = userDataDir / new FileName(".fuseVersion");
		}

		public override void Help() {}

		public override void Run(string[] args, CancellationToken ct)
		{
			KillFuseInstancesIfNeccessary();
			_studio.Start();
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

		bool ShouldKillFuse(string version)
		{
			if (!Platform.IsMac)
				return false;

			var currentInstalledVersion = GetCurrentInstalledVersion().Or("0.0.0-dev");
			return currentInstalledVersion != version;
		}

		Optional<string> GetCurrentInstalledVersion()
		{
			try
			{
				return _fs.ReadAllText(_versionFile, 5);
			}
			catch (Exception e)
			{
				_log.Exception("Failed to get current version", e);
				return Optional.None();
			}
		}

		bool TryWriteCurrentVersion(string version)
		{
			try
			{
				_fs.ReplaceText(_versionFile, version, 5);
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
