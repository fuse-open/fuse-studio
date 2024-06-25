using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Outracks.Diagnostics;
using Outracks.Fuse.Auth;
using Outracks.Fuse.Protocol;
using Outracks.Fuse.Studio;
using Outracks.Fusion;
using Outracks.IO;

namespace Outracks.Fuse.Dashboard
{
	class Dashboard
	{
		readonly ISubject<bool> _isVisible = new BehaviorSubject<bool>(false);

		public Dashboard(CreateProject createProject, IFuse fuse, IMessagingService daemon, Activate activate)
		{
			Application.Desktop.CreateSingletonWindow(
				isVisible: _isVisible,
				window: window => new Window
				{
					Style = WindowStyle.Fat,
					Title = Observable.Return("fuse X"),
					Size = Optional.Some(Property.Constant(Optional.Some(new Size<Points>(970, 608)))),
					Content = Control.Lazy(() => CreateContent(createProject, window, fuse.Version, fuse.License, daemon, activate)),
					Foreground = Theme.DefaultText,
					Background = Theme.PanelBackground,
					Border = Separator.MediumStroke,
					// Title clashes with Tabs on macOS.
					HideTitle = Platform.IsMac,
				});
		}

		public void Show()
		{
			_isVisible.OnNext(true);
		}

		IControl CreateContent(CreateProject createProject, IDialog<object> dialog, string fuseVersion, ILicense license, IMessagingService daemon, Activate activate)
		{
			var versionStr = "v" + fuseVersion;
			var projectList = new ProjectList(new Shell(), createProject, dialog);
			var openProjectFromDialog =
				Command.Enabled(() => Application.ShowOpenDocumentDialog(DocumentTypes.Project));
			var openAboutDialog =
				Command.Enabled(new About(fuseVersion, license, daemon, new Studio.Debug(null), activate).Show);

			return Popover.Host(popover =>
				Layout.Dock()
						.Top(CreateDashTopBar(license))
						.Top(Separator.Medium)

						.Right(Layout.StackFromTop(
							Layout.Dock()
							.Fill(
								Layout.SubdivideVertically(
										InfoItem(Texts.Dashboard_Learn, Texts.Dashboard_Learn_Text, WebLinks.Tutorial, "Outracks.Fuse.Icons.Dashboard.Learn.png"),
										InfoItem(Texts.Dashboard_Docs, Texts.Dashboard_Docs_Text, WebLinks.Documentation, "Outracks.Fuse.Icons.Dashboard.Docs.png"),
										InfoItem(Texts.Dashboard_Community, Texts.Dashboard_Community_Text, WebLinks.Community, "Outracks.Fuse.Icons.Dashboard.Community.png" ))
								.WithPadding(
										left: new Points(32),
										top: new Points(40))
								.WithHeight(300)),
								LogoAndVersion.Logo()
									.WithSize(new Size<Points>(336 * 0.5, 173 * 0.5))
									.WithPadding(top: new Points(51))
									.OnMouse(openAboutDialog),
								Label.Create(versionStr,
										color: Theme.DescriptorText,
										font: Theme.DefaultFont)
									.SetToolTip("Version " + fuseVersion)
									.CenterHorizontally()
									.WithPadding(top: new Points(9), bottom: new Points(51))
									.OnMouse(openAboutDialog),
								CreateOpenButtons(openProjectFromDialog, projectList)
									.WithBackground(Theme.WorkspaceBackground))
							.WithWidth(260)
							.WithHeight(600)
							.WithBackground(Theme.WorkspaceBackground))
						.Fill(projectList.Page
								.WithPadding(
									top: new Points(24),
									bottom: new Points(24)))
						.WithBackground(Theme.WorkspaceBackground)
						.WithHeight(608)
						.WithWidth(970));
		}

		private static IControl InfoItem(Text title, Text message, string linkUrl, string icon)
		{
			return Layout.StackFromTop(
					Layout.StackFromLeft(
							Image.FromResource(icon, typeof(Dashboard).Assembly)
							.WithPadding(
									right: new Points(8)),
							Buttons.TextButton(
								text: title,
								color: Theme.DefaultText,
								font: Theme.HeaderFont,
								hoverColor: Theme.ActiveHover,
								cmd: Command.Enabled(action: () => Process.Start(linkUrl)))),
					Label.Create(
							message,
							color: Theme.DescriptorText,
							font: Theme.DescriptorFont)
					.WithPadding(top: new Points(8)));
		}

		private static IControl CreateOpenButtons(Command openProjectFromDialog, ProjectList projectList)
		{
			return Layout.StackFromLeft(
					Buttons.DefaultButton(
							text: Texts.Dashboard_Button_Browse,
							cmd: openProjectFromDialog)
						.WithWidth(104),
					Buttons.DefaultButtonPrimary(
							text: projectList.DefaultMenuItemName.AsText(),
							cmd: projectList.DefaultCommand)
						.WithWidth(104)
						.WithPadding(
							left: new Points(16)))
				.WithPadding(
					top: new Points(16),
					right: new Points(16),
					left: new Points(16),
					bottom: new Points(16));
		}

		private static IControl CreateDashTopBar(ILicense license)
		{
			return Layout.Dock()
					.Right(
						Layout.Dock().Right(
							Layout.StackFromRight(
								Control.Empty.WithWidth(16).ShowOnMac(),
								Control.Empty.WithWidth(8).ShowOnWindows()))
								.Fill(
						Layout.StackFromRight(
								MainWindow.CreateLicenseStatus(license, TextAlignment.Right)))
							.CenterVertically())

					.Left(
						Layout.StackFromLeft(
								Control.Empty
								.WithWidth(80)
								.ShowOnMac(),
								Control.Empty
								.WithWidth(16)
								.ShowOnWindows(),
								Layout.Dock()
									.Bottom(
										Shapes.Rectangle(
												fill: Theme.Active)
										.WithHeight(2))
									.Fill(
										Buttons.TextButton(
												text: Texts.Dashboard_Recent_Projects,
												color: Theme.DefaultText,
												font: Theme.DefaultFont,
												hoverColor: Theme.ActiveHover,
												cmd: Command.Enabled())
										.CenterVertically()))
									.WithPadding(left: new Points(24))
								.WithHeight(38))
					.Fill()
					.WithBackground(Theme.PanelBackground)
					.WithHeight(38);
		}
	}

	static class Selectable
	{
		public static IControl Create<T>(
			ISubject<T> selection,
			IObservable<T> data,
			Func<SelectionState, IControl> control,
			Menu menu,
			Command defaultCommand,
			Text toolTip = default(Text))
		{
			return control(
				new SelectionState
				{
					IsSelected = selection.CombineLatest(data, (s, d) => s.Equals(d)).Replay(1).RefCount(),
				})
				.WithBackground(Color.AlmostTransparent)
					.OnMouse(
						pressed: data.Switch(selection.Update),
						doubleClicked: defaultCommand)
					.SetContextMenu(menu)
					.SetToolTip(toolTip);
		}
	}

	public class SelectionState
	{
		public IObservable<bool> IsSelected { get; set; }
	}
}
