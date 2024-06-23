using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Fuse.Preview;
using Outracks.Fusion;
using Outracks.Simulator;
using Outracks.Simulator.Protocol;

namespace Outracks.Fuse.Studio
{
	class Build
	{
		public Build(IProject project, ProjectPreview preview, PreviewOnDevice previewOnDevice, Command enableUsbMode, BuildArgs args, Export export)
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

			Menu = Menu.Item(Texts.SubMenu_Preview_Refresh, Refresh, hotkey: HotKey.Create(ModifierKeys.Meta, Key.R))
				 + Menu.Item(Texts.SubMenu_Preview_Rebuild, Rebuild, hotkey: HotKey.Create(ModifierKeys.Meta | ModifierKeys.Shift, Key.R))
				 + Menu.Separator
				 + Menu.Item(Texts.SubMenu_Export_Clean, export.Clean)
				 + Menu.Item(Texts.SubMenu_Preview_Reconnect, enableUsbMode)
				 + Menu.Separator
				 + previewOnDevice.Menu
				 + Menu.Separator
				 + Menu.Item(Texts.SubMenu_Preview_BuildFlags, BuildFlags);
		}

		public Menu Menu { get; private set; }

		public Command Refresh { get; private set; }
		public Command Rebuild { get; private set; }
		public Command BuildFlags { get; private set; }

		public IObservable<object> Rebuilt { get; private set; }

		public IObservable<BuildProject> BuildArguments { get; private set; }
	}
}
