using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Deployment.WindowsInstaller;

namespace Fuse.Installer.Actions
{
	public class UnoUninstaller
	{
		[CustomAction]
		public static ActionResult UninstallUno(Session session)
		{		
			try
			{
				var installationFolder = session.GetTargetPath("INSTALLFOLDER");
				var p = Process.Start(new ProcessStartInfo()
				{
					FileName = Path.Combine(installationFolder, "uno.exe"), 
					Arguments = "uninstall -f *",
					WorkingDirectory = installationFolder,
					CreateNoWindow = true,
					UseShellExecute = false
				});

				p.WaitForExit();

				return p.ExitCode == 0 ? ActionResult.Success : ActionResult.Failure;
			}
			catch (Exception e)
			{
				session.Log("Failed to uninstall uno packages\n" + e.Message);
				return ActionResult.Failure;
			}
		}
	}
}