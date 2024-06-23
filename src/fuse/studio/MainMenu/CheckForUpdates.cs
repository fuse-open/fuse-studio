using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Outracks.Diagnostics;
using Outracks.Fuse.Net;
using Outracks.Fusion;
using Outracks.Fusion.Dialogs;

namespace Outracks.Fuse.Studio
{
	class CheckForUpdates
	{
		public Menu Menu { get; private set; }

		public CheckForUpdates(string version)
		{
			var checkForUpdates = CheckForUpdatesCommand(version);
			var checkAutomatically = UserSettings.Bool("CheckForUpdates").Or(true);
			InitCheckForUpdates(checkForUpdates, checkAutomatically);

			Menu = Menu.Item(Texts.SubMenu_Updates_CheckForUpdates, checkForUpdates)
				 + Menu.Toggle(Texts.SubMenu_Updates_CheckAutomatically, checkAutomatically);
		}

		async static void InitCheckForUpdates(Command checkForUpdates, IProperty<bool> checkAutomatically)
		{
			if (await checkAutomatically.FirstAsync())
				(await checkForUpdates.Action.FirstAsync()).Value.Invoke();
		}

		static Command CheckForUpdatesCommand(string version)
		{
			return Command.Enabled(() => {
				Console.WriteLine("Checking for updates...");
				GetRemoteReleases().ContinueWith(task => {
					if (task.Exception != null)
					{
						Console.Error.WriteLine("Error checking for updates: " + task.Exception);
						return;
					}

					try
					{
						var updateVersion = GetUpdateVersion(task.Result, version.TrimStart('v'));

						if (string.IsNullOrEmpty(updateVersion))
						{
							Console.WriteLine("No updates available for version " + version + ".");
							return;
						}

						Console.WriteLine("Update available: " + updateVersion);

						if (MessageBox.ShowConfirm(
								Strings.SubMenu_Updates_UpdateAvailable
									+ ": " + updateVersion + "\n\n"
									+ Strings.SubMenu_Updates_DownloadLatestQuestion,
								"fuse X", MessageBoxType.Information))
							Process.Start(WebLinks.Download);
					}
					catch (Exception e)
					{
						Console.Error.WriteLine(e);
					}
				});
			});
		}

		static string GetUpdateVersion(List<JObject> releases, string currentVersion)
		{
			var currentIsPrerelease = currentVersion.IndexOf('-') != -1;
			string foundVersion = null;

			foreach (var release in releases)
			{
				var assets = (JArray)release["assets"];
				var prerelease = (bool)release["prerelease"];
				var tag_name = (string)release["tag_name"];
				var version = tag_name.TrimStart('v');

				if (version == currentVersion)
					return foundVersion;

				if (!ContainsCompatibleInstaller(assets))
					continue;

				if (foundVersion == null && (currentIsPrerelease || !prerelease))
					foundVersion = version;
			}

			return null;
		}

		static bool ContainsCompatibleInstaller(JArray assets)
		{
			foreach (var asset in assets)
			{
				var name = (string)asset["name"];

				if (Platform.IsMac && name.EndsWith(".dmg") ||
					Platform.IsMac && name.EndsWith(".pkg") ||
					Platform.IsWindows && name.EndsWith(".exe") ||
					Platform.IsWindows && name.EndsWith(".msi"))
					return true;
			}

			return false;
		}

		async static Task<List<JObject>> GetRemoteReleases()
		{
			using (var client = new FuseWebClient())
			{
				var body = await client.DownloadStringTaskAsync("https://api.github.com/repos/fuse-x/studio/releases");
				return JsonConvert.DeserializeObject<List<JObject>>(body);
			}
		}
	}
}
