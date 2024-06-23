using System;
using System.Threading.Tasks;
using Outracks.Fusion;
using Outracks.IO;

namespace Outracks.Fuse.Studio
{
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