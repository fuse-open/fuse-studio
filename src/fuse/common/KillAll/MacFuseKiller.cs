using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Outracks.IO;

namespace Outracks.Fuse
{
	public class MacFuseKiller : IFuseKiller
	{
		readonly IReport _report;
		readonly AbsoluteDirectoryPath _fuseRoot;

		public MacFuseKiller(IReport report, AbsoluteDirectoryPath fuseRoot)
		{
			_report = report;
			_fuseRoot = fuseRoot;
		}

		public void Execute(ColoredTextWriter console)
		{
			var fuseProcesses = Process.GetProcesses()
				.Where(ProcessIsRootedInFuseRoot)
				.Where(NotOurSelf);

			foreach (var p in fuseProcesses)
			{
				try
				{
					_report.Info("Killing " + ProcessDescription(p), ReportTo.LogAndUser);
					p.Kill();
				}
				catch (Exception e)
				{
					_report.Error("Error killing '" + ProcessDescription(p) + "': " + e.Message, ReportTo.LogAndUser);
				}
			}
		}

		private static string ProcessDescription(Process p)
		{
			var name = "<unknown process name>";
			try
			{
				name = p.ProcessName;
			}
			catch (InvalidOperationException)
			{
			}
			return "process " + p.Id + " - " + name;
		}

		bool NotOurSelf(Process process)
		{
			if (process.Id == Process.GetCurrentProcess().Id)
				return false;

			return true;
		}

		bool ProcessIsRootedInFuseRoot(Process p)
		{
			try
			{
				switch (p.ProcessName)
				{
					case "fuse":
					case "fuse X":
					case "fuse X (menu bar)":
					case "fuse-lang":
					case "Fuse Studio":	// Legacy
					case "Fuse Tray":	// Legacy
					case "UnoHost":
						return true;
				}

				var path = GetPathForPid(p.Id);
				return path.IsOrIsRootedIn(_fuseRoot);
			}
			catch (Exception e)
			{
				_report.Error("Error checking rooting of " + p.Id + ": " + e.Message);
				return false;
			}
		}

		static AbsoluteFilePath GetPathForPid(int pid)
		{
			const int maxPath = 1024 * 4;
			var pathBuffer = new StringBuilder(maxPath);
			var res = proc_pidpath(pid, pathBuffer, maxPath);
			if (res <= 0)
				throw new FailedToAcquirePathForPid(res);

			var path = AbsoluteFilePath.Parse(pathBuffer.ToString());
			return path;
		}

		[DllImport("libproc")]
		extern static int proc_pidpath(int pid, StringBuilder buffer, int buffersize);
	}

	class FailedToAcquirePathForPid : Exception
	{
		public readonly int Code;

		public FailedToAcquirePathForPid(int code) : base("Failed to acquire path for pid. Error code: " + code)
		{
			Code = code;
		}
	}
}