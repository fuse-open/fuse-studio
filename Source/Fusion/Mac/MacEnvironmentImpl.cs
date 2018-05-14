using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreServices;
using Foundation;
using Outracks.IO;

namespace Outracks.Fusion.OSX
{
	class MacEnvironmentImpl : IMacEnvironmentImpl
	{
		public IEnumerable<AbsoluteDirectoryPath> GetPathToApplicationsThatContains(string name)
		{
			foreach (var app in GetApplications(name))
				yield return app;

			var directories = Directory.GetDirectories("/Applications");
			foreach (var directory in directories)
			{
				if (directory.Contains(name))
					yield return AbsoluteDirectoryPath.Parse(directory);
			}
		}

		public IEnumerable<AbsoluteDirectoryPath> GetApplications(string identifier)
		{
			var appUrls = LaunchServices.GetApplicationUrlsForBundleIdentifier(new NSString(identifier));
			if (appUrls == null)
				return Enumerable.Empty<AbsoluteDirectoryPath>();
			return appUrls.Select(p => AbsoluteDirectoryPath.Parse(p.Path));
		}
	}
}
