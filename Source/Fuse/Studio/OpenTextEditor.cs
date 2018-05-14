using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Outracks.Fuse.Designer
{
	using Fusion;
	using IO;
	using Diagnostics;

	public static class OpenTextEditor
	{
		public static Menu CreateMenu(IObservable<Optional<AbsoluteFilePath>> path)
		{
			var sublimeTextPath = ApplicationPaths.SublimeTextPath();

			return path
				.SelectPerElement(project => project.ContainingDirectory)
				.Select(dir => new []
				{
					Menu.Item(
						name: "Open in Sublime",
						command: Command.Create(
							isEnabled: dir.HasValue && sublimeTextPath.HasValue,
							action: () => Task.Run(() =>
							{
								if (Platform.OperatingSystem == OS.Mac)
								{
									ExternalApplication.FromAppBundle(sublimeTextPath.Value).Open(dir.Value);
								}
								else
								{
									var ps = new ProcessStartInfo()
									{
										FileName = (sublimeTextPath.Value / new FileName("sublime_text.exe")).NativePath,
										Arguments = "\"" + dir.Value.NativePath + "\"",
										UseShellExecute = false
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