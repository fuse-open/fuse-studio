using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Outracks.Diagnostics;
using Outracks.Fuse.Setup;
using Outracks.Fusion;

namespace Outracks.Fuse.Studio
{
	class PreviewOnDevice
	{
		public PreviewOnDevice(IFuse fuse, IProject project, BuildArgs args)
		{
			var output = new Subject<string>();

			LogMessages = output;

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

			PreviewAndroidEmulatorApp = project.FilePath.CombineLatest(args.All, (path, allArgs) =>
				Command.Enabled(
					action: () =>
					{
						startedAndroidPreview.OnNext(Unit.Default);
						fuse.RunFuse(
							"preview",
							new[] { path.NativePath, "-t=android-emu", "--quit-after-apk-launch" }.Concat(allArgs).ToArray(),
							Observer.Create<string>(output.OnNext));
					}))
				.Switch();

			PreviewIosApp = project.FilePath.CombineLatest(args.All, (path, allArgs) =>
				Command.Create(
					isEnabled: Platform.IsMac,
					action: () => fuse.RunFuse(
						"preview",
						new[] { path.NativePath, "-t=ios" }.Concat(allArgs).ToArray(),
						Observer.Create<string>(output.OnNext))))
				.Switch();

			PreviewIosSimulatorApp = project.FilePath.CombineLatest(args.All, (path, allArgs) =>
				Command.Create(
					isEnabled: Platform.IsMac,
					action: () => fuse.RunFuse(
						"preview",
						new[] { path.NativePath, "-t=ios-sim" }.Concat(allArgs).ToArray(),
						Observer.Create<string>(output.OnNext))))
				.Switch();

			PreviewNativeApp = project.FilePath.CombineLatest(args.All, (path, allArgs) =>
					Command.Enabled(
						action: () => fuse.RunFuse(
							"preview",
							new[] { path.NativePath, "-t=native" }.Concat(allArgs).ToArray(),
							Observer.Create<string>(output.OnNext))))
				.Switch();

			Menu = Menu.Item(Texts.SubMenu_Preview_Android, PreviewAndroidApp)
				 + Menu.Item(Texts.SubMenu_Preview_iOS + (!Platform.IsMac ? " (" + Texts.Label_MacOnly + ")" : ""), PreviewIosApp)
				 + Menu.Item(Texts.SubMenu_Preview_Native, PreviewNativeApp);
			Menu = Menu.Item(Texts.SubMenu_Preview_Android, PreviewAndroidApp)
				 + Menu.Item(Texts.SubMenu_Preview_AndroidEmulator, PreviewAndroidEmulatorApp)
				 + Menu.Separator
				 + Menu.Item(Texts.SubMenu_Preview_iOS + (!Platform.IsMac ? " (" + Texts.Label_MacOnly + ")" : ""), PreviewIosApp)
				 + Menu.Item(Texts.SubMenu_Preview_iOSSimulator + (!Platform.IsMac ? " (" + Texts.Label_MacOnly + ")" : ""), PreviewIosSimulatorApp)
				 + Menu.Separator
				 + Menu.Item(Texts.SubMenu_Preview_Native, PreviewNativeApp);

		}

		public Menu Menu { get; private set; }

		public Command PreviewIosApp { get; private set; }
		public Command PreviewIosSimulatorApp { get; private set; }
		public Command PreviewAndroidApp { get; private set; }
		public Command PreviewAndroidEmulatorApp { get; private set; }
		public Command PreviewNativeApp { get; private set; }
		public IObservable<string> LogMessages { get; private set; }
	}
}
