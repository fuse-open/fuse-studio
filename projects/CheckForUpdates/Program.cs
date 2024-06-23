using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace ConsoleApp1
{
	class Program
	{
		static void Main(string[] args)
		{
			var json = File.ReadAllText("../../releases.json");
			var releases = JsonConvert.DeserializeObject<List<JObject>>(json);
			Console.WriteLine(IsUpdateAvailable(releases, "2.0.0-beta.5"));
		}

		static bool IsUpdateAvailable(List<JObject> releases, string currentVersion)
		{
			var currentIsPrerelease = currentVersion.IndexOf('-') != -1;
			var foundRelease = false;

			foreach (var release in releases)
			{
				var assets = (JArray)release["assets"];
				var prerelease = (bool)release["prerelease"];
				var tag_name = (string)release["tag_name"];
				var version = tag_name.TrimStart('v');

				if (version == currentVersion)
					return foundRelease;

				if (!ContainsCompatibleInstaller(assets))
					continue;

				if (currentIsPrerelease || !prerelease)
					foundRelease = true;
			}

			return false;
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
	}

	class Platform
	{
		public static bool IsMac = true;
		public static bool IsWindows = false;
	}
}
