using System;
using Microsoft.Win32;
using Outracks.Diagnostics;
using Outracks.Fuse.Protocol;
using Outracks.IO;

namespace Outracks.Fuse.Auth
{
	public class UriHandler
	{
		readonly IFuse _fuse;
		readonly IMessagingService _daemon;

		public UriHandler(IFuse fuse, IMessagingService daemon)
		{
			_fuse = fuse;
			_daemon = daemon;
		}

		public void OnUri(string uri)
		{
			const string protocol1 = "fuse-x://";
			const string protocol2 = "fusestudio://";	// Legacy.
			if (uri == null || (!uri.StartsWith(protocol1) && !uri.StartsWith(protocol2)))
				throw new ArgumentException($"Invalid {protocol1} URI.", nameof(uri));

			if (uri.StartsWith(protocol1))
				uri = uri.Substring(protocol1.Length);
			else if (uri.StartsWith(protocol2))
				uri = uri.Substring(protocol2.Length);

			var firstSeparator = uri.IndexOf('/');
			if (firstSeparator < 0)
				firstSeparator = uri.Length;

			var selector = uri.Substring(0, firstSeparator);
			var data = firstSeparator < uri.Length ? uri.Substring(firstSeparator + 1) : null;

			switch (selector)
			{
				case "activate":
				{
					Console.WriteLine(_fuse.License.ActivateSession(data, _daemon));
					return;
				}
				case "license":
				{
					Console.WriteLine(_fuse.License.ActivateLicense(data, _daemon));
					return;
				}
				case "deactivate":
				{
					_fuse.License.Deactivate(_daemon);
					Console.WriteLine("Deactivated.");
					return;
				}
			}

			throw new ArgumentException($"Unsupported {protocol1} URI.", nameof(uri));
		}

		internal static bool Register(AbsoluteFilePath fuse)
		{
			if (!Platform.IsWindows)
			{
				Console.Error.WriteLine("UriHandler is not supported.");
				return false;
			}

			try
			{
				using (var Software = Registry.CurrentUser.CreateSubKey("Software"))
				using (var Classes = Software.CreateSubKey("Classes"))
				using (var fuse_x = Classes.CreateSubKey("fuse-x"))
				using (var DefaultIcon = fuse_x.CreateSubKey("DefaultIcon"))
				using (var shell = fuse_x.CreateSubKey("shell"))
				using (var open = shell.CreateSubKey("open"))
				using (var command = open.CreateSubKey("command"))
				{
					fuse_x.SetValue(null, "URL:fuse X");
					fuse_x.SetValue("URL Protocol", "");
					DefaultIcon.SetValue(null, $"{fuse},1");
					command.SetValue(null, $"\"{fuse.ContainingDirectory}\\fuse-uri.exe\" \"%1\"");
				}

				// Legacy
				using (var Software = Registry.CurrentUser.CreateSubKey("Software"))
				using (var Classes = Software.CreateSubKey("Classes"))
				using (var fusestudio = Classes.CreateSubKey("fusestudio"))
				using (var DefaultIcon = fusestudio.CreateSubKey("DefaultIcon"))
				using (var shell = fusestudio.CreateSubKey("shell"))
				using (var open = shell.CreateSubKey("open"))
				using (var command = open.CreateSubKey("command"))
				{
					fusestudio.SetValue(null, "URL:fuse X");
					fusestudio.SetValue("URL Protocol", "");
					DefaultIcon.SetValue(null, $"{fuse},1");
					command.SetValue(null, $"\"{fuse.ContainingDirectory}\\fuse-uri.exe\" \"%1\"");
				}

				return true;
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e);
				return false;
			}
		}
	}
}
