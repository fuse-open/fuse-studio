using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Outracks.Fuse.Designer;

namespace Outracks.Fuse.Setup
{
	using Fusion;

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
					Title = Observable.Return("Setup guide"),
					Size = Optional.Some(Property.Constant(Optional.Some(new Size<Points>(500, 300)))),
					Content = Control.Lazy(() => Create(softwares, report, dialog).ShowWhen(showSetupGuide))
						.WithBackground(Theme.PanelBackground),
					Background = Theme.PanelBackground,
					Foreground = Theme.DefaultText,
					Border = Separator.MediumStroke,
					Style = WindowStyle.Fat,
				});

			Menu 
				= Menu.Item("Install Android SDKs", () => fuse.RunFuse("install", new [] { "android" }, Observer.Create<string>(output.OnNext)))
				+ Menu.Item("Setup guide", () => showSetupGuide.OnNext(true));
		}

		public IObservable<string> LogMessages { get; private set; } 

		public Menu Menu { get; private set; }

		static IControl Create(SoftwareCollectionStatus softwares, IReport report, IDialog<object> dialog)
		{
			return Layout.Dock()
				.Top(CreateSublimeHeaderText().WithMacWindowStyleCompensation())
				.Top(Spacer.Medium)
				.Top(CreatePluginLine(softwares.SublimeApp, report))
				.Top(CreatePluginLine(softwares.SublimePlugin, report))
				.Top(Spacer.Medium)
				.Top(CreateAtomHeaderText())
				.Bottom(
					Buttons.DefaultButtonPrimary(text: "Ok", cmd: Command.Enabled(() => dialog.Close()))
						.WithWidth(100)
						.DockRight())
				.Bottom(Spacer.Medium)
				.Fill()
				.WithPadding(new Thickness<Points>(20));
		}

		static IControl CreateSublimeHeaderText()
		{
			return 
				Label.Create("A text editor is required to use Fuse." + 
					" We recommend Sublime Text 3 and our accompanying plugin." + 
					" This wizard will look for Sublime Text 3 and help you install or update our plugin.",
					color: Theme.DefaultText, 
					font: Theme.DefaultFont, 
					lineBreakMode: LineBreakMode.Wrap);
		}

		static IControl CreateAtomHeaderText()
		{
			return Theme.Link.Select(linkColor =>
				Label.FormattedText(
					textParts: Observable.Return(
						AttributedText.Parts().Text("We also have an ").Link("Atom Plugin", "https://atom.io/packages/fuse", linkColor).Text(".")),
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

		public SoftwareCollectionStatus(IFuse fuse)
		{
			SublimeApp = new SublimeAppStatus(fuse.Report);
			SublimePlugin = new SublimePluginStatus(fuse);
		}
	}

}
