using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using Outracks.Diagnostics;
using Outracks.Fuse.Auth;
using Outracks.Fuse.Protocol;
using Outracks.Fusion;

namespace Outracks.Fuse.Studio
{
	public class About
	{
		public About(string version, ILicense license, IMessagingService daemon, Debug debug, Activate activate)
		{
			var showAboutDialog = new BehaviorSubject<bool>(false);

			Application.Desktop.CreateSingletonWindow(
				isVisible: showAboutDialog,
				window: dialog =>
					new Window
					{
						Title = Texts.About_Title,
						Content = Control.Lazy(() =>
							Layout.Dock()
								.Top(LogoAndVersion.Create(version).WithMacWindowStyleCompensation())
								.Bottom(CreateOkButton(Command.Enabled(() => dialog.Close())))
								.Bottom(Separator.Medium)
								.Bottom(Label.Create(""))
								.Bottom(Buttons.DefaultButtonPrimary(Texts.Activate_Title, Command.Enabled(activate.Show))
										.WithWidth(160)
										.CenterHorizontally())
								.Fill(Theme.Link.Select(linkColor =>
									Label.FormattedText(font: Theme.DefaultFont, color:Theme.DefaultText, lineBreakMode: LineBreakMode.Wrap,
											textParts: license.IsRegistered.CombineLatest(
														license.Type, license.Data, license.Session, Tuple.Create).Select(tuple => {
												var isRegistered = tuple.Item1;
												var type = tuple.Item2;
												var data = tuple.Item3;
												var session = tuple.Item4;
												return AttributedText.Parts()
													.Text("SHA: " + CommitSha + "\n")
													.Text("UID: " + Hardware.UID + "\n")
													.Text("\n")
													.Text("------------------------------------------------------------------------\n", Color.FromRgb(0x7f7f7f))
													.Text("  " + (isRegistered ? Strings.About_RegisteredTo : Strings.About_NotRegistered) + "\n", Color.FromRgb(0x7f7f7f))
													.Text("------------------------------------------------------------------------\n", Color.FromRgb(0x7f7f7f))
													.Text("  ").Parts(GetRegisteredToParts(isRegistered, data.Name, data.Email, linkColor, session)).Text("\n")
													//.Text("  " + (isRegistered ? Strings.About_Company + data.Company : "") + "\n")
													.Text("  " + (isRegistered ? Strings.About_License + type : "") + "\n")
													.Text("  " + (isRegistered ? Strings.About_Issued + data.UtcIssued.ToLocalTime().ToString() : "") + "\n")
													.Text("  " + (isRegistered ? Strings.About_Expires + data.UtcExpires.ToLocalTime().ToString() : "") + "\n")
													.Text("  " + (isRegistered ? Strings.About_Authority + data.Authority : "") + "\n")
													.Text("------------------------------------------------------------------------\n", Color.FromRgb(0x7f7f7f))
													.Text("\n")
													.Text(Strings.About_MoreInfo1)
													.Link(Strings.About_MoreInfo2, WebLinks.Website, linkColor);
											}))).Switch()
										.OnMouse(debug.EnableDebugMenuByRapidClicking)
										.WithPadding(Thickness.Create(new Points(20), 10, 20, 10))
								)
							.WithOverlay(Shapes.Rectangle(stroke: Stroke.Create(1, Theme.PanelBackground)))
							.WithBackground(Theme.WorkspaceBackground)),
						Size = Optional.Some(Property.Constant(Optional.Some(new Size<Points>(
													Platform.IsWindows ? 370 : 430,
													Platform.IsWindows ? 480 : 560)))),
						Background = Theme.PanelBackground,
						Foreground = Theme.DefaultText,
						Style = WindowStyle.Sheet
					}
			);

			Show = () => showAboutDialog.OnNext(true);
			Menu = Menu.Item(Platform.IsWindows ? Strings.About_Menu : Strings.About_Title, Show);
		}

		static IList<TextPart> GetRegisteredToParts(bool isRegistered, string name, string email, Color linkColor, string session)
		{
			return isRegistered
				? AttributedText.Parts()
					.Link($"@{name}", WebLinks.Dashboard + "?session=" + Uri.EscapeUriString(session), linkColor)
					.Text($" ({email})")
				: AttributedText.Parts()
					.Text(Strings.Auth_Text_SignIn1)
					.Link(Strings.Auth_Text_SignIn2, WebLinks.SignIn, linkColor)
					.Text(Strings.Auth_Text_SignIn3);
		}

		static IControl CreateOkButton(Command clicked)
		{
			return Button.Create(
				clicked: clicked,
				content: bs => Layout.Dock()
					.Left(Fuse.Icons.Confirm(Theme.Active).CenterVertically())
					.Left(Spacer.Small)
					.Fill(Label.Create(
						text: Texts.Button_Ok,
						color: Theme.DefaultText))
					.Center())
				.WithHeight(45);
		}

		public Action Show { get; private set; }
		public Menu Menu { get; private set; }

		public static string CommitSha
		{
			get
			{
				var config = GetAttribute<AssemblyConfigurationAttribute>()?.Configuration;
				return !string.IsNullOrEmpty(config)
					? config
					: "N/A";
			}
		}

		static T GetAttribute<T>()
		{
			return typeof(About).Assembly.GetCustomAttributes(true).OfType<T>()
				.FirstOrDefault();
		}
	}
}
