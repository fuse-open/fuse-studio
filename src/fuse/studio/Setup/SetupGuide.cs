using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Outracks.Diagnostics;
using Outracks.Fuse.Studio;
using Outracks.Fusion;

namespace Outracks.Fuse.Setup
{
	public class SetupGuide
	{
		public SetupGuide(IFuse fuse, IObservable<Unit> doneLoadingMainWindow)
		{
			var output = new Subject<string>();
			LogMessages = output;

			var report = fuse.Report;
			var softwares = new SoftwareCollectionStatus(fuse);

			var showSetupGuide = new BehaviorSubject<bool>(false);
			MissingPluginNotification.Create(fuse, doneLoadingMainWindow, showSetupGuide);

			Application.Desktop.CreateSingletonWindow(
				isVisible: showSetupGuide.CombineLatest(doneLoadingMainWindow, (s, _) => s),
				window: dialog => new Window
				{
					Title = Texts.Dialog_SetupGuide_Caption,
					Size = Optional.Some(Property.Constant(Optional.Some(new Size<Points>(500, Platform.IsWindows ? 320 : 340)))),
					Content = Control.Lazy(() => Create(softwares, report, dialog).ShowWhen(showSetupGuide))
						.WithBackground(Theme.WorkspaceBackground),
					Background = Theme.PanelBackground,
					Foreground = Theme.DefaultText,
					Border = Separator.MediumStroke,
					Style = WindowStyle.Fat,
				});

			Menu = Menu.Item(Texts.SubMenu_Tools_Setup, () => showSetupGuide.OnNext(true))
				 + Menu.Separator
				 + Menu.Item(Texts.SubMenu_Tools_InstallSDK, () => fuse.RunFuse("install", new[] { "android" }, Observer.Create<string>(output.OnNext)))
				 + Menu.Separator
				 + Menu.Item(Texts.SubMenu_Tools_InstallAtomPlugin, () => fuse.RunFuse("install", new[] { "atom-plugin" }, Observer.Create<string>(output.OnNext)))
				 + Menu.Item(Texts.SubMenu_Tools_InstallSublimePlugin, () => fuse.RunFuse("install", new[] { "sublime-plugin" }, Observer.Create<string>(output.OnNext)))
				 + Menu.Item(Texts.SubMenu_Tools_InstallVSCodeExtension, () => fuse.RunFuse("install", new[] { "vscode-extension" }, Observer.Create<string>(output.OnNext)))
				 + Menu.Separator
				 + Menu.Item(Texts.SubMenu_Tools_Config, () => fuse.RunFuse("config", new string[0], Observer.Create<string>(output.OnNext)));
		}

		public IObservable<string> LogMessages { get; private set; }

		public Menu Menu { get; private set; }

		static IControl Create(SoftwareCollectionStatus softwares, IReport report, IDialog<object> dialog)
		{
			return Layout.Dock()
				.Top(CreateSublimeHeaderText().WithMacWindowStyleCompensation())
				.Top(Spacer.Medium)
				.Top(Layout.StackFromLeft(
					Layout.StackFromTop(
						CreatePluginLine(softwares.SublimeApp, report),
						CreatePluginLine(softwares.SublimePlugin, report))
						.WithWidth(225),
					Layout.StackFromTop(
						CreatePluginLine(softwares.VsCodeApp, report),
						CreatePluginLine(softwares.VsCodeExtension, report))))
				.Top(Spacer.Medium)
				.Top(CreateAtomHeaderText())
				.Bottom(
					Buttons.DefaultButtonPrimary(text: Texts.Button_Ok, cmd: Command.Enabled(() => dialog.Close()))
						.WithWidth(100)
						.DockRight())
				.Bottom(Spacer.Medium)
				.Fill()
				.WithPadding(new Thickness<Points>(20))
				.WithBackground(Theme.WorkspaceBackground);
		}

		static IControl CreateSublimeHeaderText()
		{
			return Layout.StackFromTop(
				Label.Create(Texts.Dialog_SetupGuide_Comment1,
						color: Theme.DefaultText,
						font: Theme.DefaultFont,
						lineBreakMode: LineBreakMode.Wrap),
				Theme.Link.Select(linkColor =>
				Label.FormattedText(
						textParts: Observable.Return(
							AttributedText.Parts()
								.Text(Strings.Dialog_SetupGuide_Comment2_1)
								.Link("Sublime Text 3", "http://www.sublimetext.com/3", linkColor)
								.Text(Strings.Dialog_SetupGuide_Comment2_2)
								.Link("Visual Studio Code", "https://code.visualstudio.com/", linkColor)
								.Text(Strings.Dialog_SetupGuide_Comment2_3)),
						color: Theme.DefaultText,
						font: Theme.DefaultFont))
					.Switch()
					.WithPadding(top: new Points(12)),
				Label.Create(Texts.Dialog_SetupGuide_Comment3,
						color: Theme.DefaultText,
						font: Theme.DefaultFont,
						lineBreakMode: LineBreakMode.Wrap)
					.WithPadding(top: new Points(12)));
		}

		static IControl CreateAtomHeaderText()
		{
			return Theme.Link.Select(linkColor =>
				Label.FormattedText(
					textParts: Observable.Return(
						AttributedText.Parts()
							.Text(Strings.Dialog_SetupGuide_SeeAlso1)
							.Link("Atom", "https://atom.io/packages/fuse", linkColor)
							.Text(Strings.Dialog_SetupGuide_SeeAlso2)),
					color: Theme.DefaultText,
					font: Theme.DefaultFont)).Switch();
		}

		static IControl CreatePluginLine(SoftwareStatus software, IReport report)
		{
			return Layout.Dock()
				.Left(CreateIconForStatus(software, report)
					.WithWidth(25).WithHeight(25)
					.CenterVertically())
				.Left(Spacer.Medium)
				.Left(Label.Create(software.Name, color: Theme.DefaultText, font: Theme.DefaultFont)
					.CenterVertically())
				.Right(CreateInstallButton(software)
					.CenterVertically())
				.Fill()
				.WithMediumPadding();
		}

		static IControl CreateInstallButton(SoftwareStatus software)
		{
			return software.Status
				.StartWith(InstallStatus.Unknown)
				.Select(s =>
				{
					switch (s)
					{
						case InstallStatus.NotInstalled:
							return Buttons.DefaultButton("Install", cmd: Command.Enabled(software.Install)).WithWidth(100);

						case InstallStatus.UpdateAvailable:
							return Buttons.DefaultButton("Update", cmd: Command.Enabled(software.Install)).WithWidth(100);

						case InstallStatus.Installing:
							return Label.Create("Installing...", color: Theme.DefaultText, font: Theme.DefaultFont)
								.WithHeight(Buttons.DefaultButtonHeight);
						default:
							return Control.Empty
								.WithHeight(Buttons.DefaultButtonHeight);
					}
				})
				.Switch();
		}

		static IControl ImageFromResource(string resourceName)
		{
			return Image.FromResource(resourceName, typeof(SetupGuide).Assembly);
		}

		static IControl CreateIconForStatus(SoftwareStatus software, IReport report)
		{
			var iconPath = "Outracks.Fuse.Icons.";
			var unknown = Layout.Layer();
			var error = ImageFromResource(iconPath + "Plugin_Error.png");
			var success = ImageFromResource(iconPath + "Plugin_Success.png");
			var warning = ImageFromResource(iconPath + "Plugin_Warning.png");
			return software.Status
				.Select(s =>
				{
					switch (s)
					{
						case InstallStatus.Unknown:
							return unknown;
						case InstallStatus.NotInstalled:
							return error;
						case InstallStatus.Installed:
							return success;
						case InstallStatus.Installing:
						case InstallStatus.UpdateAvailable:
							return warning;
						default:
							report.Error("Unknown InstallStatus  " + s, ReportTo.Headquarters);
							return error;
					}
				})
				.StartWith(unknown)
				.Switch();
		}
	}

	public class SoftwareCollectionStatus
	{
		public readonly SoftwareStatus SublimeApp;
		public readonly SoftwareStatus SublimePlugin;
		public readonly SoftwareStatus VsCodeApp;
		public readonly SoftwareStatus VsCodeExtension;

		public SoftwareCollectionStatus(IFuse fuse)
		{
			SublimeApp = new SublimeAppStatus(fuse.Report);
			SublimePlugin = new SublimePluginStatus(fuse);
			VsCodeApp = new VsCodeAppStatus(fuse.Report);
			VsCodeExtension = new VsCodeExtensionStatus(fuse);
		}
	}
}
