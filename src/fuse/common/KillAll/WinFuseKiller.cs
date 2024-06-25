using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Outracks.IO;

namespace Outracks.Fuse
{
	public class WinFuseKiller : IFuseKiller
	{
		readonly IReport _report;
		readonly AbsoluteDirectoryPath _fuseRoot;

		public WinFuseKiller(IReport report, AbsoluteDirectoryPath fuseRoot)
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
					console.WriteLine("Killing " + p.ProcessName);
					p.Kill();
					p.WaitForExit(5000);
				}
				catch (Exception e)
				{
					_report.Error(e.Message, ReportTo.LogAndUser);
				}
			}
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
					case "fuse-lang":
					case "fuse-studio":
					case "fuse-tray":
					case "unohost":
						return true;
				}

				var path = GetPathForProcess(p);
				if (path.IsOrIsRootedIn(_fuseRoot))
					return true;
				else
					return false;
			}
			catch (Win32Exception e)
			{
				const int accessDenied = -2147467259;
				if (e.HResult == accessDenied)
				{
					var isLikelyAFuseProcess = p
						.ProcessName
						.ToUpper(CultureInfo.InvariantCulture)
						.Contains("FUSE");

					// We only care about access denied if this is likely a fuse process.
					if (isLikelyAFuseProcess)
						_report.Error(p.ProcessName + ": " + e.Message);
				}

				return false;
			}
			catch (InvalidOperationException e)
			{
				const int processHasExited = -2146233079;
				if (e.HResult != processHasExited)
				{
					_report.Error(p.ProcessName + ": " + e.Message);
				}

				return false;
			}
			catch (Exception e)
			{
				_report.Error(p.ProcessName + ": " + e.Message);
				return false;
			}
		}

		static AbsoluteFilePath GetPathForProcess(Process process)
		{
			int maxPath = 1024 * 4;
			var exePath = new StringBuilder(maxPath);
			var res = QueryFullProcessImageName(process.Handle, 0, exePath, ref maxPath);
			if (!res)
			{
				throw new FailedToAcquirePathForPid(Marshal.GetLastWin32Error());
			}

			var path = AbsoluteFilePath.Parse(exePath.ToString());
			return path;
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool QueryFullProcessImageName([In]IntPtr hProcess, [In]int dwFlags, [Out]StringBuilder lpExeName, ref int lpdwSize);
	}
}
