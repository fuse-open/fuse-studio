using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Deployment.WindowsInstaller;

namespace Fuse.Installer.Actions
{
	public interface ILogger
	{
		void Log(string message);
	}

	public class SessionLogger : ILogger
	{
		readonly Session _session;

		public SessionLogger(Session session)
		{
			_session = session;
		}

		public void Log(string message)
		{
			_session.Log(message);
		}
	}

	public class FuseKillerService
	{
		static readonly string[] FuseAssembliesNames = { "Fuse", "Fuse Studio", "Fuse-Inspector", "Fuse.Monitor" };

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[CustomAction]
		public static ActionResult TryKillDaemon(Session session)
		{
			session.Log("Begin try kill fuse instances");

			try
			{
				Kill(new SessionLogger(session));
			}
			catch (Exception e)
			{				
				session.Log("Exception occurred: " + e.Message);
				return ActionResult.Failure;
			}

			return ActionResult.Success;
		}

		internal static void Kill(ILogger logger)
		{						
			if (!AnyInstanceOfFuseRunning())
			{
				logger.Log("Couldn't find any instances of Fuse running.");
				return;
			}

			logger.Log("Force killing all Fuse instances.");
			ForceKillAllFuseInstances();
		}

		static bool AnyInstanceOfFuseRunning()
		{
			return Process.GetProcesses()
				.Any(p =>
					FuseAssembliesNames
					.Contains(p.ProcessName));
		}

		internal static void ForceKillAllFuseInstances()
		{
			var fuseProcesses = Process.GetProcesses()
				.Where(p => 
					FuseAssembliesNames
					.Contains(p.ProcessName));
			foreach (var process in fuseProcesses)
			{
				process.Kill();
			}
		}
	}
}
