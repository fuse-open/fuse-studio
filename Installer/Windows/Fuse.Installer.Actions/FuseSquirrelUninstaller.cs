using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;

namespace Fuse.Installer.Actions
{
	public class FuseSquirrelUninstaller
	{
		[CustomAction]
		public static ActionResult FuseSquirrelCleanup(Session session)
		{
			try
			{
				
				var fuseSquirrelKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Fuse");
				session.Log("Uninstalling Fuse squirrel installer");
				if (fuseSquirrelKey != null)
				{
					try
					{
						var uninstallCmdParts = fuseSquirrelKey.GetValue("QuietUninstallString").ToString().Split(' ');
						var p = Process.Start(new ProcessStartInfo()
						{
							FileName = uninstallCmdParts.Take(1).First(),
							Arguments = string.Join(" ", uninstallCmdParts.Skip(1)),
							WorkingDirectory = fuseSquirrelKey.GetValue("InstallLocation").ToString(),
							CreateNoWindow = true,
							UseShellExecute = false
						});

						p.WaitForExit();

						return p.ExitCode == 0 ? ActionResult.Success : ActionResult.Failure;
					}
					catch (Exception e)
					{
						session.Log("Failed to uninstall old fuse squirrel installation\n" + e.Message);
						return ActionResult.Failure;
					}
				}
				
				session.Log("Could not find old Fuse in registry");
				return ActionResult.Success;
			}
			catch (Exception e)
			{
				session.Log("Failed to uninstall Fuse with Squirrel setup-script\n" + e.Message);
				return ActionResult.Failure;
			}
		}
	}
}
