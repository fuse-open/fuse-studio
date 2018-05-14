using System;
using System.Threading.Tasks;

namespace Outracks.Fuse.Designer
{
	using Fusion;
	using IO;

	static class OpenFolder
	{
		public static Command CreateCommand(IShell shell, IObservable<Optional<AbsoluteFilePath>> project)
		{
			return project.Switch(file =>
				Command.Create(
					isEnabled: file.HasValue,
					action: () => Task.Run(() => shell.ShowInFolder(file.Value))));
		}
	}
}