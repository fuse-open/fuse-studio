using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Outracks.Diagnostics;
using Outracks.Fusion;

namespace Outracks.Fuse.Studio
{
	class Export
	{
		public Export(IProject project, IFuse fuse, BuildArgs args)
		{
			var output = new Subject<string>();

			LogMessages = output;

			ExportForAndroid = project.FilePath.CombineLatest(args.All, (path, allArgs) =>
				Command.Enabled(
					action: () => fuse.RunFuse(
						"build",
						new[] { path.NativePath, "-t=android", "--run" }.Concat(allArgs).ToArray(),
						Observer.Create<string>(output.OnNext))))
				.Switch();

			ExportForAndroidEmulator = project.FilePath.CombineLatest(args.All, (path, allArgs) =>
				Command.Enabled(
					action: () => fuse.RunFuse(
						"build",
						new[] { path.NativePath, "-t=android-emu", "--run" }.Concat(allArgs).ToArray(),
						Observer.Create<string>(output.OnNext))))
				.Switch();

			ExportForIos =  project.FilePath.CombineLatest(args.All, (path, allArgs) =>
				Command.Create(
					isEnabled: Platform.IsMac,
					action: () => fuse.RunFuse(
						"build",
						new[] { path.NativePath, "-t=ios", "--debug" }.Concat(allArgs).ToArray(),
						Observer.Create<string>(output.OnNext))))
				.Switch();

			ExportForIosSimulator =  project.FilePath.CombineLatest(args.All, (path, allArgs) =>
				Command.Create(
					isEnabled: Platform.IsMac,
					action: () => fuse.RunFuse(
						"build",
						new[] { path.NativePath, "-t=ios-sim", "--run" }.Concat(allArgs).ToArray(),
						Observer.Create<string>(output.OnNext))))
				.Switch();

			ExportForDotNet = project.FilePath.CombineLatest(args.All, (path, allArgs) =>
				Command.Enabled(
					action: () => fuse.RunFuse(
						"build",
						new[] { path.NativePath, "-t=dotnet", "--run" }.Concat(allArgs).ToArray(),
						Observer.Create<string>(output.OnNext))))
				.Switch();

			ExportForNative = project.FilePath.CombineLatest(args.All, (path, allArgs) =>
				Command.Enabled(
					action: () => fuse.RunFuse(
						"build",
						new[] { path.NativePath, "-t=native", "--run" }.Concat(allArgs).ToArray(),
						Observer.Create<string>(output.OnNext))))
				.Switch();

			Clean = project.FilePath.CombineLatest(args.All, (path, allArgs) =>
				Command.Enabled(
					action: () => fuse.RunFuse(
						"clean",
						new[] { path.NativePath, "-v" },
						Observer.Create<string>(output.OnNext))))
				.Switch();

			Menu = Menu.Item(Texts.SubMenu_Export_ExportForAndroid, ExportForAndroid)
				 + Menu.Item(Texts.SubMenu_Export_ExportForAndroidEmulator, ExportForAndroidEmulator)
				 + Menu.Separator
				 + Menu.Item(Texts.SubMenu_Export_ExportForiOS + (!Platform.IsMac ? " (" + Texts.Label_MacOnly + ")" : ""), ExportForIos)
				 + Menu.Item(Texts.SubMenu_Export_ExportForiOSSimulator + (!Platform.IsMac ? " (" + Texts.Label_MacOnly + ")" : ""), ExportForIosSimulator)
				 + Menu.Separator
				 + Menu.Item(Texts.SubMenu_Export_ExportForDotNet, ExportForDotNet)
				 + Menu.Item(Texts.SubMenu_Export_ExportForNative, ExportForNative)
				 + Menu.Separator
				 + Menu.Item(Texts.SubMenu_Export_Clean, Clean);
		}

		public Menu Menu { get; private set; }

		public Command ExportForAndroid { get; private set; }
		public Command ExportForAndroidEmulator { get; private set; }
		public Command ExportForIos { get; private set; }
		public Command ExportForIosSimulator { get; private set; }
		public Command ExportForDotNet { get; private set; }
		public Command ExportForNative { get; private set; }
		public Command Clean { get; private set; }
		public IObservable<string> LogMessages { get; private set; }
	}
}
