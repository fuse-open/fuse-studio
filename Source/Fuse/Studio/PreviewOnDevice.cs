using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Linq;

namespace Outracks.Fuse.Designer
{
	using Setup;
	using Fusion;
	using Diagnostics;
	
	class PreviewOnDevice
	{
		public PreviewOnDevice(IFuse fuse, IProject project, BuildArgs args)
		{
			var output = new Subject<string>();

			LogMessages = output;

			var isMac = fuse.Platform == OS.Mac;
			var startedAndroidPreview = new Subject<Unit>();
			MissingAndroidNotification.Create(fuse, startedAndroidPreview);

			PreviewAndroidApp = project.FilePath.CombineLatest(args.All, (path, allArgs) =>
				Command.Enabled(
					action: () =>
					{
						startedAndroidPreview.OnNext(Unit.Default);
						fuse.RunFuse(
							"preview",
							new[] { path.NativePath, "-t=android", "--quit-after-apk-launch" }.Concat(allArgs).ToArray(),
							Observer.Create<string>(output.OnNext));
					}))
				.Switch();

			PreviewIosApp = project.FilePath.CombineLatest(args.All, (path, allArgs) =>
				Command.Create(
					isEnabled: isMac,
					action: () => fuse.RunFuse(
						"preview",
						new[] { path.NativePath, "-t=ios" }.Concat(allArgs).ToArray(),
						Observer.Create<string>(output.OnNext))))
				.Switch();

				Menu = Menu.Item("Preview on Android", PreviewAndroidApp)
					+ Menu.Item("Preview on iOS" + (!isMac ? " (Mac only)" : ""), PreviewIosApp);

		}

		public Menu Menu { get; private set; }

		public Command PreviewIosApp { get; private set; }
		public Command PreviewAndroidApp { get; private set; }
		public IObservable<string> LogMessages { get; private set; }
	}
}