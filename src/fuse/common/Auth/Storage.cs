using Microsoft.Win32;
using Outracks.Diagnostics;
using Outracks.IO;
using System.IO;

namespace Outracks.Fuse.Auth
{
	abstract class Storage
	{
		public abstract string LoadLicense();
		public abstract void StoreLicense(string data);

		public abstract string LoadSession();
		public abstract void StoreSession(string data);

		public static Storage Get(AbsoluteDirectoryPath userData)
		{
			return Platform.IsMac
				? (Storage) new MacStorage(userData)
				:           new WindowsStorage();
		}

		class MacStorage : Storage
		{
			readonly AbsoluteFilePath _license;
			readonly AbsoluteFilePath _session;

			public MacStorage(AbsoluteDirectoryPath userData)
			{
				_license = userData / new FileName("fuse.dat");
				_session = userData / new FileName("session.dat");
			}

			public override string LoadLicense()
			{
				return File.Exists(_license.NativePath)
					? File.ReadAllText(_license.NativePath)
					: null;
			}

			public override void StoreLicense(string data)
			{
				File.WriteAllText(_license.NativePath, data ?? "");
			}

			public override string LoadSession()
			{
				return File.Exists(_session.NativePath)
					? File.ReadAllText(_session.NativePath)
					: null;
			}

			public override void StoreSession(string data)
			{
				File.WriteAllText(_session.NativePath, data ?? "");
			}
		}

		class WindowsStorage : Storage
		{
			public override string LoadLicense()
			{
				using (var Software = Registry.CurrentUser.CreateSubKey("Software"))
				using (var Fuseapps = Software.CreateSubKey("Fuseapps"))
				using (var fuse_X = Fuseapps.CreateSubKey("fuse X"))
					return fuse_X.GetValue("data") as string;
			}

			public override void StoreLicense(string data)
			{
				using (var Software = Registry.CurrentUser.CreateSubKey("Software"))
				using (var Fuseapps = Software.CreateSubKey("Fuseapps"))
				using (var fuse_X = Fuseapps.CreateSubKey("fuse X"))
					fuse_X.SetValue("data", data ?? "");
			}

			public override string LoadSession()
			{
				using (var Software = Registry.CurrentUser.CreateSubKey("Software"))
				using (var Fuseapps = Software.CreateSubKey("Fuseapps"))
				using (var fuse_X = Fuseapps.CreateSubKey("fuse X"))
					return fuse_X.GetValue("session") as string;
			}

			public override void StoreSession(string data)
			{
				using (var Software = Registry.CurrentUser.CreateSubKey("Software"))
				using (var Fuseapps = Software.CreateSubKey("Fuseapps"))
				using (var fuse_X = Fuseapps.CreateSubKey("fuse X"))
					fuse_X.SetValue("session", data ?? "");
			}
		}
	}
}
