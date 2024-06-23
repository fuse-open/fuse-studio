using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using Outracks.IO;

namespace Outracks.Fusion
{
	public class WindowsEnvironment
	{
		public Optional<AbsoluteFilePath> GetDefaultApplicationForFile(AbsoluteFilePath fileName)
		{
			var pathRes = new StringBuilder();
			var res = FindExecutable(fileName.ToString(), null, pathRes);
			return res > 32 ? Optional.Some(AbsoluteFilePath.Parse(pathRes.ToString())) : Optional.None<AbsoluteFilePath>();
		}

		[DllImport("shell32.dll")]
		static extern int FindExecutable(string lpFile, string lpDirectory, [Out] StringBuilder lpResult);


		public static IEnumerable<AbsoluteDirectoryPath> LookForPathInUninstall(string partOfName)
		{
			var registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
			var key = Registry.LocalMachine.OpenSubKey(registryKey);
			if (key != null)
			{
				foreach (var subkey in key.GetSubKeyNames().Select(keyName => key.OpenSubKey(keyName)))
				{
					var displayName = subkey.GetValue("DisplayName") as string;
					if (displayName != null && displayName.Contains(partOfName))
					{
						var installLocation = subkey.GetValue("InstallLocation") as string;
						yield return AbsoluteDirectoryPath.Parse(installLocation);
					}
				}
				key.Close();
			}

			registryKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
			key = Registry.LocalMachine.OpenSubKey(registryKey);
			if (key != null)
			{
				foreach (var subkey in key.GetSubKeyNames().Select(keyName => key.OpenSubKey(keyName)))
				{
					var displayName = subkey.GetValue("DisplayName") as string;
					if (displayName != null && displayName.Contains(partOfName))
					{
						var installLocation = subkey.GetValue("InstallLocation") as string;
						yield return AbsoluteDirectoryPath.Parse(installLocation);
					}
				}
				key.Close();
			}
		}

		public static IEnumerable<AbsoluteDirectoryPath> LookForPathInApplications(string fullName)
		{
			var applications = Registry.ClassesRoot.OpenSubKey("Applications");
			if (applications != null)
			{
				foreach (var application in applications.GetSubKeyNames().Select(keyName => applications.OpenSubKey(keyName)))
				{
					if (application.Name.EndsWith(fullName))
					{
						application.GetSubKeyNames().Select(keyName => application.OpenSubKey(keyName));
						var kjell = application.OpenSubKey("shell");
						var open = kjell.OpenSubKey("open");
						var command = open.OpenSubKey("command");
						var command_string = (string)command.GetValue("");
						var exe_path = command_string.Split(" ")[0].Trim('"');
						yield return AbsoluteDirectoryPath.Parse(exe_path);
					}
				}
				applications.Close();
			};
		}

		public static IEnumerable<AbsoluteDirectoryPath> LookForPathInPATH(string file)
		{
			foreach (var part in Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process).Split(';'))
			{
				var path = Path.Combine(part, file);
				if (File.Exists(path))
					yield return AbsoluteDirectoryPath.Parse(part);
			}

			foreach (var part in Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User).Split(';'))
			{
				var path = Path.Combine(part, file);
				if (File.Exists(path))
					yield return AbsoluteDirectoryPath.Parse(part);
			}
		}
	}
}
