using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Outracks.Diagnostics;
using Outracks.Fuse.Auth;
using Outracks.IO;
using Uno;
using Uno.Configuration;

namespace Outracks.Fuse
{
	class IncompleteInstallation : Exception
	{
		public ImmutableList<string> MissingComponents { get; private set; }

		public IncompleteInstallation(ImmutableList<string> missingComponents)
			: base("This installation of Fuse appears to be incomplete. The following components could not be found: " + missingComponents.Join(", "))
		{
			MissingComponents = missingComponents;
		}
	}

	public class FuseImpl : IFuse
	{
		/**
		 * The global license flag is managed by FuseApi
		 * 
		 * NB: This field is read using reflection by ViewportController
		 */
		internal static bool _IsLicenseValid;

		public bool IsLicenseValid()
		{
			return _IsLicenseValid;
		}

		public void CheckCompleteness()
		{
			var requiredApps = new List<IExternalApplication>
			{
				Fuse,
				CodeAssistance,
				Tray,
				Uno,
				UnoHost,
				Studio,
			};

			var missingApps =
				requiredApps
					.Where(app => !app.Exists)
					.Select(app => app.Name)
					.ToImmutableList();

			if (missingApps.Count > 0)
				throw new IncompleteInstallation(missingApps);
		}

		public Guid SessionId { get; set; }
		public Guid SystemId { get; set; }
		public string Version { get; set; }

		public string CommitSha
		{
			get
			{
				var config = typeof(FuseImpl).Assembly.GetCustomAttributes(true)
					.OfType<AssemblyConfigurationAttribute>()
					.FirstOrDefault()?.Configuration;
				return !string.IsNullOrEmpty(config)
					? config
					: "N/A";
			}
		}

		public AbsoluteDirectoryPath FuseRoot { get; set; }
		public AbsoluteDirectoryPath UserDataDir { get; set; }
		public AbsoluteDirectoryPath ProjectsDir { get; set; }

		public AbsoluteFilePath FuseExe { get; set; }

		public AbsoluteDirectoryPath ComponentsDir
		{
			get { return AbsoluteDirectoryPath.Parse(UnoConfig.Current.GetFullPath("Fuse.Components")); }
		}

		public IExternalApplication Uno { get; set; }
		public IExternalApplication Fuse { get; set; }
		public IExternalApplication Tray { get; set; }
		public IExternalApplication Studio { get; set; }
		public IExternalApplication CodeAssistance { get; set; }
		public IExternalApplication LogServer { get; set; }
		public IExternalApplication UnoHost { get; set; }

		public Process StartFuse(string command, params string[] commandArgs)
		{
#if DEBUG
			Console.WriteLine("Starting fuse " + command + " " + commandArgs.Select(global::Uno.Extensions.QuoteSpace).Join(" "));
#endif
			return Fuse.Start(
				new ProcessStartInfo()
				{
					Arguments = command + " " + commandArgs.Select(global::Uno.Extensions.QuoteSpace).Join(" "),
					UseShellExecute = false,
					CreateNoWindow = true,
					WindowStyle = ProcessWindowStyle.Hidden,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					RedirectStandardInput = true
				});
		}

		public Process StartFuse(string command, string[] commandArguments, bool redirect, bool hide)
		{
			return StartFuse(new[] { command }.Concat(commandArguments).ToArray(), redirect, hide);
		}

		public Process StartFuse(string[] arguments, bool redirect, bool hide)
		{
#if DEBUG
			Console.WriteLine("Starting fuse with arguments " + arguments.Select(global::Uno.Extensions.QuoteSpace).Join(" "));
#endif
			return Fuse.Start(
				new ProcessStartInfo
				{
					Arguments = arguments.Select(global::Uno.Extensions.QuoteSpace).Join(" "),
					WindowStyle = hide ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
					CreateNoWindow = hide,
					UseShellExecute = !redirect,
					RedirectStandardInput = redirect,
					RedirectStandardError = redirect,
					RedirectStandardOutput = redirect
				});
		}

		public void RestartFuse()
		{
			if (Platform.IsWindows)
			{
				var file = Path.Combine(Path.GetTempPath(), "restart-fuse.bat");
				var text = "@echo off\n" +
						$"\"{FuseExe}\" kill-all\n" +
						$"\"{FuseExe}\"";
				File.WriteAllText(file, text);
				var p = new Process();
				p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				p.StartInfo.FileName = "cmd.exe";
				p.StartInfo.Arguments = "/C " + global::Uno.Extensions.QuoteSpace(file);
				p.Start();
			}
			else
			{
				var file = "/tmp/restart-fuse.sh";
				var text = "#!/bin/bash\n" +
						"set -e\n" +
						"\n" +
						"# Mono and NPM\n" +
						"export PATH=/usr/local/bin:/opt/local/bin:/opt/homebrew/bin:$PATH\n" +
						"export PATH=/Library/Frameworks/Mono.framework/Versions/Current/Commands:$PATH\n" +
						"\n" +
						"# Restart fuse X\n" +
						$"\"{FuseExe}\" kill-all\n" +
						$"\"{FuseExe}\"\n";
				File.WriteAllText(file, text);
				Process.Start("bash", global::Uno.Extensions.QuoteSpace(file));
			}
		}

		public IReport Report { get; set; }
		public ILicense License { get; set; }
	}
}
