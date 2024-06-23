using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Outracks.Diagnostics;
using Outracks.Fuse.Auth;
using Outracks.Fuse.Protocol;
using Outracks.Fusion;

namespace Outracks.Fuse.Studio
{
	public class Activate
	{
		public Activate(ILicense license, IMessagingService daemon)
		{
			var showActivateDialog = new BehaviorSubject<bool>(false);
			var activationCode = new BehaviorSubject<string>("");
			var errorMessage = Property.Create("");

			Application.Desktop.CreateSingletonWindow(
				isVisible: showActivateDialog,
				window: dialog =>
					new Window {
						Title = Texts.Activate_Title,
						Content = Control.Lazy(() =>
							Layout.Dock()
								.Bottom(Buttons.DefaultButtonPrimary(Texts.Activate_Button, Command.Enabled(() => {
									var code = activationCode.Value;

									if (string.IsNullOrEmpty(code))
									{
										errorMessage.Write(Strings.Activate_FeedbackEmpty);
										return;
									}

									new Thread(() => {
										try
										{
											LicenseData.Decode(code);
											license.ActivateLicense(code, daemon);
											showActivateDialog.OnNext(false);
											activationCode.OnNext("");
											errorMessage.Write("");
										}
										catch (Exception e)
										{
											Console.Error.WriteLine(e);
											errorMessage.Write(Strings.Activate_FeedbackInvalid);
										}
									}).Start();
								}))
									.WithWidth(160)
									.WithPadding(Thickness.Create(new Points(20), 0, 20, 20))
									.CenterHorizontally())
								.Fill(Layout.StackFromTop(
										TextBox.Create(Property.Create(activationCode), isMultiline: true, doWrap: true, foregroundColor: Theme.DefaultText)
											.WithHeight(130)
											.WithOverlay(Shapes.Rectangle(stroke: Theme.FieldStroke))
											.WithBackground(Theme.PanelBackground),
										errorMessage
											.Select(error =>
												Label.Create(error, Theme.DescriptorFont, color: Theme.FieldErrorStroke.Brush)
													.WithHeight(15))
											.Switch())
									.WithPadding(Thickness.Create(new Points(20), 20, 20, 0))
									.WithBackground(Theme.WorkspaceBackground))
								.WithBackground(Theme.WorkspaceBackground)
								.WithMacWindowStyleCompensation()),
						Size = Optional.Some(Property.Constant(Optional.Some(new Size<Points>(
													Platform.IsWindows ? 520 : 550,
													Platform.IsWindows ? 220 : 250)))),
						Background = Theme.PanelBackground,
						Foreground = Theme.DefaultText,
						Border = Separator.MediumStroke,
						Style = WindowStyle.Fat
					}
			);

			Show = () => {
				showActivateDialog.OnNext(true);
				activationCode.OnNext("");
				errorMessage.Write("");
			};
			Menu = Menu.Item(Texts.Activate_Title, Show);
		}

		public Action Show { get; private set; }
		public Menu Menu { get; private set; }
	}
}
