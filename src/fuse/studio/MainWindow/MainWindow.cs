using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Outracks.Fuse.Auth;
using Outracks.Fuse.Studio.Icons;
using Outracks.Fuse.Stage;
using Outracks.Fusion;
using Outracks.Diagnostics;
using System.Collections.Generic;

namespace Outracks.Fuse.Studio
{
	public interface IInspectorControl
	{
		IControl Create();
	}

	static class MainWindow
	{
		public static Points ResizeBorderSize = 2;

		public static Window Create(
			IObservable<string> projectName,
			IControl search,
			IControl outline,
			IControl bookmarks,
			StageController stage,
			IControl notifications,
			IControl inspector,
			IControl attributes,
			LogView logview,
			Menu menu,
			Command closed,
			IProperty<bool> selectionEnabled,
			IObservable<bool> topMost,
			IProperty<Mode> mode,
			Command mainWindowFocused,
			IContext context,
			CodeView codeView,
			ILicense license)
		{
			IControl topBar = null;

			var newView = stage.NewViewport;

			var content =
				Popover.Host(
					popover =>
					{
						topBar = CreateTopBar(
							popover,
							selectionEnabled: selectionEnabled,
							mode: mode,
							addViewport: newView,
							codeView: codeView,
							license: license);

						return CreateContent(
							logview: logview,
							outline: outline,
							search: search,
							notifications: notifications,
							attributes: attributes,
							bookmarks: bookmarks,
							inspector: inspector,
							stage: new StageView(stage, context, mode),
							topBar: topBar,
							mode: mode);
					});

			var mainWindowSize = UserSettings.Size("MainWindowSize");
			var mainWindowPosition = UserSettings.Position("MainWindowPosition");
			var mainWindowState = UserSettings.Settings.Property<WindowState>("MainWindowState");

			var compactSize = UserSettings.Size("CompactWindowSize");
			var compactPosition = UserSettings.Position("CompactWindowPosition");

			var compactContentSize =
				compactSize.With(value:
					compactSize.Merge(
						stage.DefaultViewportSize.Select(Optional.Some)
							.CombineLatest(
								topBar.DesiredSize.Height,
								(deviceSize, topBarHeight) =>
									deviceSize.Select(s =>
										Size.Create(
											s.Width + ResizeBorderSize * 2 /* padding */,
											s.Height + topBarHeight + ResizeBorderSize * 2 + LogViewHeader.HeaderHeight /* padding */)))));

			var windowSize = mode.Select(x => x == Mode.Normal ? mainWindowSize : compactContentSize).Switch();
			var windowState = mode.Select(x => x == Mode.Normal ? mainWindowState : Property.Constant(Optional.Some(WindowState.Normal))).Switch();
			var windowPosition = mode.Select(x => x == Mode.Normal ? mainWindowPosition : compactPosition).Switch();

			return new Window
			{
				Closed = closed,
				Title = projectName.CombineLatest(license.Status)
					.Select(tuple => {
						var title = "fuse X";
						if (!string.IsNullOrEmpty(tuple.Item1))
							title = tuple.Item1 + " - " + title;
						if (!string.IsNullOrEmpty(tuple.Item2))
							title = title + $" [{tuple.Item2}]";
						return title;
					}),
				Size = Optional.Some(windowSize),
				Position = Optional.Some(windowPosition),
				State = Optional.Some(windowState),
				Menu = menu,
				Content = content,
				TopMost = Optional.Some(topMost),
				HideMenu = Optional.Some((IObservable<bool>)mode.Convert(a => a == Mode.Compact, a => a ? Mode.Compact : Mode.Normal)),
				Focused = mainWindowFocused,
				Foreground = Theme.DefaultText,
				Background = Theme.PanelBackground,
				Border = Separator.MediumStroke,
				Style = WindowStyle.Fat
			};
		}

		public static IControl CreateContent(
			LogView logview,
			IControl outline,
			IControl search,
			IControl notifications,
			IControl attributes,
			IControl bookmarks,
			IControl inspector,
			StageView stage,
			IControl topBar,
			IObservable<Mode> mode)
		{
			Points inspectorWidth = 295;

			var right = Property.Constant(Fuse.Inspector.Inspector.Width);
			var left = UserSettings.Point("LeftPanelWidth").Or(new Points(260));

			var inNormalMode = mode.Select(x => x == Mode.Normal);

			return Layout.Layer(
				Shapes.Rectangle(fill: Theme.WorkspaceBackground),
				Layout.Dock()
					.Top(topBar)
					.Top(Separator.Medium)

					.Panel(RectangleEdge.Right, right,
						inNormalMode,
						control: inspector,
						minSize: inspectorWidth,
						resizable: false)

					.Panel(RectangleEdge.Left, left,
						inNormalMode,
						control: outline,
						minSize: 10)

					.Fill(stage.CreateStage(logview, notifications)));
		}

		public static DockBuilder Panel(
			this DockBuilder dock, RectangleEdge dockEdge, IProperty<Points> size,
			IObservable<bool> isExpanded,
			IControl control,
			Points minSize,
			bool resizable = true)
		{
			var availableSize = new BehaviorSubject<Size<IObservable<Points>>>(
				new Size<IObservable<Points>>(Observable.Return<Points>(double.MaxValue), Observable.Return<Points>(double.MaxValue)));
			var maxWidth = availableSize.Switch()[dockEdge.NormalAxis()];

			control = control
				.WithBackground(Theme.PanelBackground)
				.WithFrame(f => f, a => a.WithAxis(dockEdge.NormalAxis(), s => size.Min(maxWidth)))
				.WithDimension(dockEdge.NormalAxis(), size.Min(maxWidth));

			control = Layout.Dock()
				.Dock(edge: dockEdge, control: control)
				.Dock(edge: dockEdge, control: Separator.Medium)
				.Fill();

			if (resizable)
				control = control.MakeResizable(dockEdge.Opposite(), size, minSize: minSize);

			control = control.MakeCollapsable(dockEdge.Opposite(), isExpanded, lazy: false);

			control = control.WithFrame(
				frame => frame,
				availSize =>
				{
					availableSize.OnNext(availSize);
					return availSize;
				});

			return dock.Dock(edge: dockEdge, control: control);
		}

		static IControl CreateHeaderControl(
			Command command,
			string buttonText,
			string tooltipText,
			IControl icon,
			Brush labelColor)
		{
			return
				Button.Create(command, state =>
						Layout.StackFromLeft(
								Control.Empty.WithWidth(4),
								icon,
								Control.Empty.WithWidth(4),
								Label.Create(
										text: buttonText,
										color:labelColor,
										font: Theme.DescriptorFont)
									.CenterVertically(),
								Control.Empty.WithWidth(4))
							.SetToolTip(tooltipText)
							.WithBackground(
								background: Observable.CombineLatest(
										state.IsEnabled, state.IsHovered,
										(enabled, hovering) =>
											hovering
												? Theme.FaintBackground
												: Color.Transparent)
									.Switch()));
		}

		static IControl CreateFullSelectionControl(
			IProperty<bool> selectionEnabled)
		{
			return
				Layout.StackFromLeft(
					Layout.Dock()
						.Bottom(
							Shapes.Rectangle(
									fill: selectionEnabled.IsFalse()
										.Select(e => e ? Color.Transparent : Theme.Active)
										.Switch())
								.WithHeight(2))
						.Fill(
							CreateHeaderControl(
								icon: SelectionIcon.Create(selectionEnabled, true),
								tooltipText: Strings.Toolbar_Selection_Tooltip,
								buttonText : Strings.Toolbar_Selection_Button,
								labelColor: selectionEnabled.IsFalse()
									.Select(e => e ? Theme.DescriptorText : Theme.DefaultText)
									.Switch(),
								command: selectionEnabled.Toggle())),
					Control.Empty.WithWidth(24),
					Layout.Dock()
						.Bottom(
							Shapes.Rectangle(
									fill: selectionEnabled.IsFalse()
										.Select(e => e ? Theme.Active : Color.Transparent)
										.Switch())
								.WithHeight(2))
						.Fill(
							CreateHeaderControl(
								icon: TouchIcon.Create(selectionEnabled, true),
								tooltipText: Strings.Toolbar_Touch_Tooltip,
								buttonText : Strings.Toolbar_Touch_Button,
								labelColor: selectionEnabled
									.Select(e => e ? Theme.DescriptorText : Theme.DefaultText)
									.Switch(),
								command: selectionEnabled.Toggle())));
		}

		static IControl CreateCompactSelectionControl(
			IProperty<Mode> mode,
			IProperty<bool> selectionEnabled,
			Command toggleMode)
		{
			return
				Layout.StackFromLeft(
					Button.Create(selectionEnabled.Toggle(), state =>
						Layout.Dock()
							.Bottom(
								Shapes.Rectangle(
										fill: Theme.Active)
									.WithSize(new Size<Points>(1, 1)))
							.Fill(
								Layout.StackFromLeft(
									SelectionIcon.Create(selectionEnabled, true)
										.OnMouse(pressed: selectionEnabled.Toggle())
										.ShowWhen(selectionEnabled),
									TouchIcon.Create(selectionEnabled, true)
										.Center()
										.OnMouse(pressed: selectionEnabled.Toggle())
										.ShowWhen(selectionEnabled.IsFalse())))
							.WithPadding(new Thickness<Points>(4, 0, 4, 0))
							.WithBackground(
								background: Observable.CombineLatest(
										state.IsEnabled, state.IsHovered,
										(enabled, hovering) =>
											hovering
												? Theme.FaintBackground
												: Color.Transparent)
									.Switch())
							.SetToolTip(Strings.Toolbar_Selection_Tooltip2)),
					Control.Empty.WithWidth(8),
					Button.Create(toggleMode, state =>
							MinimizeAndMaximizeIcon.Create(mode)
							.WithPadding(new Thickness<Points>(4, 0, 4, 0))
							.WithBackground(
								background: Observable.CombineLatest(
										state.IsEnabled, state.IsHovered,
										(enabled, hovering) =>
											hovering
												? Theme.FaintBackground
												: Color.Transparent)
									.Switch()))
							.SetToolTip(Strings.Toolbar_Compact_Tooltip));
		}

		static IControl CreateTopBar(
			IPopover popover,
			IProperty<bool> selectionEnabled,
			IProperty<Mode> mode,
			Command addViewport,
			CodeView codeView,
			ILicense license)
		{
			var isCompact = mode.Convert(m => m == Mode.Compact, m => m ? Mode.Compact : Mode.Normal);
			var toggleMode = isCompact.Toggle();

			return
				Layout.Layer(
					Layout.StackFromLeft(CreateLicenseStatus(license, TextAlignment.Left))
						.WithPadding(Thickness.Create(new Points(0), 3, 0, 0))
						.DockLeft(),
					Layout
						.StackFromLeft(
							codeView.Create(popover)
								.HideWhen(isCompact),

							Control.Empty.WithWidth(16),

							Layout
								.StackFromLeft(CreateHeaderControl(
									icon: Fuse.Icons.AddViewport(),
									tooltipText: Strings.Toolbar_Viewport_Tooltip,
									buttonText: Strings.Toolbar_Viewport_Button,
									command: addViewport,
									labelColor: Theme.DefaultText),
									Control.Empty.WithWidth(16))
								.HideWhen(isCompact),

							CreateHeaderControl(
									icon: MinimizeAndMaximizeIcon.Create(mode),
									tooltipText: Strings.Toolbar_Compact_Tooltip,
									buttonText : Strings.Toolbar_Compact_Button,
									labelColor: Theme.DefaultText,
									command: toggleMode)
								.HideWhen(isCompact),

							CreateCompactSelectionControl(mode, selectionEnabled, toggleMode)
								.ShowWhen(isCompact)
								.Center(),

							Control.Empty.WithWidth(4))
						.DockRight(),
						CreateFullSelectionControl(selectionEnabled)
							.HideWhen(isCompact)
							.CenterHorizontally())
				.WithHeight(38)
				.WithPadding(new Thickness<Points>(8, 0, 8, 0))
				.WithBackground(Theme.PanelBackground);
		}

		internal static IControl CreateLicenseStatus(ILicense license, TextAlignment textAlignment)
		{
			// HACK: Hide on macOS.
			if (Platform.IsMac)
				return Label.FormattedText(Observable.Never<IList<TextPart>>());

			return Theme.Link.Select(linkColor =>
				Label.FormattedText(font: Theme.DescriptorFont, color: Theme.DescriptorText,
					lineBreakMode: LineBreakMode.Clip, textAlignment: textAlignment,
					textParts: license.IsValid.CombineLatest(license.IsExpired, license.Name,
							license.IsTrial.CombineLatest(license.DaysLeft, license.Session)).Select(tuple => {
						var name = tuple.Item3;
						var isLoggedIn = tuple.Item1 && !string.IsNullOrEmpty(name);
						var isExpired = tuple.Item2;
						var isTrial = tuple.Item4.Item1;
						var daysLeft = tuple.Item4.Item2;
						var session = tuple.Item4.Item3;
								return isLoggedIn ?
								AttributedText.Parts()
									.Text(Strings.Auth_Text_LicensedTo1)
									.Link("@" + name, WebLinks.Dashboard + "?session=" + Uri.EscapeUriString(session), linkColor)
									.Text(Strings.Auth_Text_LicensedTo2 + (
										isTrial
											? " - " + daysLeft + " " + (
												daysLeft == 1
													? Strings.Auth_Text_TrialDayLeft
													: Strings.Auth_Text_TrialDaysLeft
											)
											: ""
									))
							: isExpired ?
								AttributedText.Parts()
									.Text(Strings.Auth_Text_LicenseExpired1)
									.Link(Strings.Auth_Text_LicenseExpired2, WebLinks.SignIn, linkColor)
									.Text(Strings.Auth_Text_LicenseExpired3)
							:
								AttributedText.Parts()
									.Text(Strings.Auth_Text_SignIn1)
									.Link(Strings.Auth_Text_SignIn2, WebLinks.SignIn, linkColor)
									.Text(Strings.Auth_Text_SignIn3);
					}))
					.WithWidth(new Points(300))
					.WithPadding(
						left: new Points(4),
						right: new Points(4),
						bottom: new Points(4))
					.CenterVertically())
				.Switch();
		}
	}
}
