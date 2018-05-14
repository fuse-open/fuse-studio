using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Outracks.Fuse.Designer
{
	using Fusion;
	using Diagnostics;

	class Export
	{
		public Export(IProject project, IFuse fuse, BuildArgs args)
		{
			var output = new Subject<string>();

			LogMessages = output;

			var isMac = fuse.Platform == OS.Mac;

			ExportForAndroid = project.FilePath.CombineLatest(args.All, (path, allArgs) =>
				Command.Enabled(
					action: () => fuse.RunFuse(
						"build",
						new[] { path.NativePath, "-t=android", "--run" }.Concat(allArgs).ToArray(),
						Observer.Create<string>(output.OnNext))))
				.Switch();

			ExportForIos =  project.FilePath.CombineLatest(args.All, (path, allArgs) =>
				Command.Create(
					isEnabled: isMac,
					action: () => fuse.RunFuse(
						"build",
						new[] { path.NativePath, "-t=ios", "--run" }.Concat(allArgs).ToArray(),
						Observer.Create<string>(output.OnNext))))
				.Switch();

			Menu = Menu.Item("Export for Android", ExportForAndroid)
				+ Menu.Item("Export for iOS" + (!isMac ? " (Mac only)" : ""), ExportForIos);
		}

		public Menu Menu { get; private set; }

		public Command ExportForAndroid { get; private set; }
		public Command ExportForIos { get; private set; }
		public IObservable<string> LogMessages { get; private set; } 
	}
}