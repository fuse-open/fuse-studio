using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Outracks.Diagnostics;
using Outracks.IO;
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
		public void CheckCompleteness()
		{
			var requiredApps = new List<IExternalApplication>
			{
				Fuse, 
				CodeAssistance, 
				Tray,
				Uno,
				Designer,
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

		public OS Platform 
		{
			get { return Diagnostics.Platform.OperatingSystem; }
		}

		public string Version { get; set; }
		public bool IsInstalled
		{
			get
			{
#if DEBUG
				return false;
#else
				return Version.Contains("-dev");
#endif
			}
		}


		public AbsoluteDirectoryPath FuseRoot { get; set; }
		public AbsoluteDirectoryPath UserDataDir { get; set; }
		public AbsoluteDirectoryPath ProjectsDir { get; set; }

		public AbsoluteFilePath FuseExe { get; set; }
		public AbsoluteFilePath UnoExe { get; set; }
		public Optional<AbsoluteFilePath> MonoExe { get; set; }


		public AbsoluteDirectoryPath ModulesDir
		{
			get { return AbsoluteDirectoryPath.Parse(UnoConfig.Current.GetFullPath("ModulesDirectory")); }
		}

		public IExternalApplication Uno { get; set; }
		public IExternalApplication Fuse { get; set; }
		public IExternalApplication Tray { get; set; }
		public IExternalApplication Designer { get; set; }
		public IExternalApplication CodeAssistance { get; set; }
		public IExternalApplication LogServer { get; set; }


		public Process StartFuse(string command, params string[] commandArgs)
		{
			return Fuse.Start(
				new ProcessStartInfo()
				{
					Arguments = command + " " + commandArgs.Select(s => "\"" + s + "\"").Join(" "),
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
			return Fuse.Start(
				new ProcessStartInfo
				{
					Arguments = arguments.Select(
						s =>
						{
							if (s.EndsWith("\\"))
								return "\"" + s + " \"";
							else
								return "\"" + s + "\"";
						}).Join(" "),
					WindowStyle = hide ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
					CreateNoWindow = hide,
					UseShellExecute = !redirect,
					RedirectStandardInput = redirect,
					RedirectStandardError = redirect,
					RedirectStandardOutput = redirect
				});
		}

		public IReport Report { get; set; }
	}
}
