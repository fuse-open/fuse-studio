using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Fuse.Preview;

namespace Outracks.Fuse.Designer
{
	using Fusion;
	using Simulator;
	using Simulator.Protocol;

	class Build
	{
		public Build(IProject project, ProjectPreview preview, PreviewOnDevice previewOnDevice, Command enableUsbMode, BuildArgs args)
		{
			var buildLibraries = new BehaviorSubject<bool>(false);

			var rebuilt = new Subject<object>();
			Rebuilt = rebuilt;

			Rebuild = Command.Enabled(() =>
			{
				rebuilt.OnNext(new object());
				preview.Rebuild();
			});
			
			Refresh = Command.Enabled(preview.Refresh);

			var buildFlagsWindowVisible = new BehaviorSubject<bool>(false);

			BuildFlags = Command.Enabled(() => buildFlagsWindowVisible.OnNext(true));

			Application.Desktop.CreateSingletonWindow(
				isVisible: buildFlagsWindowVisible,
				window: window => BuildFlagsWindow.Create(buildFlagsWindowVisible, args));

			BuildArguments = Observable.CombineLatest(
				args.Defines, buildLibraries, args.Verbose, project.FilePath,
				(d, bl, vb, pp) => new BuildProject(pp.NativePath, List.Create(d.ToArray()), bl, vb));

			Menu =
				  Menu.Item("Refresh", Refresh, hotkey: HotKey.Create(ModifierKeys.Meta, Key.R))
				+ Menu.Item("Rebuild", Rebuild, hotkey: HotKey.Create(ModifierKeys.Meta | ModifierKeys.Shift, Key.R))
				+ Menu.Separator
				+ Menu.Item("Reconnect USB (Android)", enableUsbMode)
				+ Menu.Separator
				+ previewOnDevice.Menu
				+ Menu.Separator
				+ Menu.Item("Build flags", BuildFlags);
		}

		public Menu Menu { get; private set; }

		public Command Refresh { get; private set; }
		public Command Rebuild { get; private set; }
		public Command BuildFlags { get; private set; }

		public IObservable<object> Rebuilt { get; private set; }

		public IObservable<BuildProject> BuildArguments { get; private set; } 
	}
}