using System;
using Microsoft.Win32;
using Outracks.IO;

namespace Outracks.Fuse
{
	class DefaultApplicaitonWin
	{
		public void SetAsDefaultApplicationFor(AbsoluteFilePath exeFilePath, string extension)
		{
			using (var extKey = CreateOrOpenKeyInClassesRoot(extension))
			{
				var extType = extKey.GetValue("");
				if (extType == null)
				{
					extType = extension.TrimStart('.') + "_file";
					extKey.SetValue("", extType);
				}

				var command = "\"" + exeFilePath.NativePath + "\"" + "\"%1\"";
				var extCommandKey = extType + @"\shell\open\command";
				using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\" + extCommandKey))
				{
					if (key == null)
						throw new InvalidOperationException("Couldn't create subkey");

					key.SetValue("", command);
				}
			}
		}

		static RegistryKey CreateOrOpenKeyInClassesRoot(string key)
		{
			var extKey = Registry.ClassesRoot.OpenSubKey(key);
			if (extKey == null)
			{
				extKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\" + key, RegistryKeyPermissionCheck.ReadWriteSubTree);
				if (extKey == null)
					throw new InvalidOperationException("Couldn't create key in register.");
			}

			return extKey;
		}
	}
}