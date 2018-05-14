using System;
using System.Threading.Tasks;

namespace Outracks.Fuse.Designer
{
	using Fusion;
	using IO;

	static class OpenTerminal
	{
		public static Command CreateCommand(IShell shell, IObservable<Optional<AbsoluteFilePath>> path)
		{
			return path.Switch(file =>
				Command.Create(
					isEnabled: file.HasValue,
					action: () => Task.Run(() => shell.OpenTerminal(file.Value.ContainingDirectory))));
		}
	}
}