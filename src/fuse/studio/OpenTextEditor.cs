using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Outracks.Diagnostics;
using Outracks.Fusion;
using Outracks.IO;
using Uno;

namespace Outracks.Fuse.Studio
{
	public static class OpenTextEditor
	{
		public static Menu CreateMenu(IObservable<Optional<AbsoluteFilePath>> path)
		{
			var sublimeTextPath = ApplicationPaths.SublimeTextPath();
			var vsCodePath = ApplicationPaths.VsCodePath();

			return path
				.SelectPerElement(project => project.ContainingDirectory)
				.Select(dir => new []
				{
					Menu.Item(
						name: Texts.SubMenu_Project_OpenSublime,
						command: Command.Create(
							isEnabled: dir.HasValue && sublimeTextPath.HasValue,
							action: () => Task.Run(() =>
							{
								if (Platform.IsMac)
								{
									ExternalApplication.FromAppBundle(sublimeTextPath.Value).Open(dir.Value);
								}
								else
								{
									var ps = new ProcessStartInfo()
									{
										FileName = (sublimeTextPath.Value / new FileName("sublime_text.exe")).NativePath,
										Arguments = dir.Value.NativePath.QuoteSpace(),
										UseShellExecute = false,
										WindowStyle = ProcessWindowStyle.Hidden
									};

									ps.UpdatePathEnvironment();

									Process.Start(ps);
								}
							}))),
					Menu.Item(
						name: Texts.SubMenu_Project_OpenVSCode,
						command: Command.Create(
							isEnabled: dir.HasValue && vsCodePath.HasValue,
							action: () => Task.Run(() =>
							{
								if (Platform.IsMac)
								{
									var ps = new ProcessStartInfo()
									{
										FileName = (vsCodePath.Value / "Contents" / "Resources" / "app" / "bin" / new FileName("code")).NativePath,
										Arguments = dir.Value.NativePath.QuoteSpace(),
										UseShellExecute = false
									};

									Process.Start(ps);
								}
								else
								{
									var ps = new ProcessStartInfo()
									{
										FileName = (vsCodePath.Value / "bin" / new FileName("code.cmd")).NativePath,
										Arguments = dir.Value.NativePath.QuoteSpace(),
										UseShellExecute = false,
										WindowStyle = ProcessWindowStyle.Hidden
									};

									ps.UpdatePathEnvironment();

									Process.Start(ps);
								}
							}))),
				})
				.Concat();
		}

		public static void UpdatePathEnvironment(this ProcessStartInfo ps)
		{
			ps.EnvironmentVariables["Path"] =
				(Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine)) + ";" +
					(Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User));
		}
	}
}
