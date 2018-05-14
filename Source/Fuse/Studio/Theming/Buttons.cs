using System.Reactive.Linq;
using Outracks.Fusion;
using Button = Outracks.Fusion.Button;
using Label = Outracks.Fusion.Label;
using Cursor = Outracks.Fusion.Cursor;
using Outracks.Fuse.Theming.Themes;

namespace Outracks.Fuse.Designer
{
	public static class Buttons
	{
		public static IControl CloseButton(
			Command close,
			Brush idleColor,
			Brush pressedColor,
			int height = 11,
			int thickness = 1,
			int padding = 7)
		{
			return Button.Create(
				close,
				states =>
				{
					var color = states.IsPressed
						.Select(clicked => clicked ? pressedColor : idleColor)
						.Switch();

					return MainWindowIcons
						.CloseIcon(new Points(height), Stroke.Create(thickness, color))
						.WithPadding(new Thickness<Points>(padding));
				});
		}

		public static IControl NotificationButton(Text text, Command cmd, Brush foreground)
		{
			return SolidColorButton(text, cmd, 
				foreground: foreground,
				background: Color.White,
				hoverColor: Color.White,
				altTextColor: foreground);
		}

		public static IControl DefaultButtonPrimary(Text text, Command cmd)
		{
			return SolidColorButton(text, cmd,
				foreground: Color.White,
				background: Theme.Active,
				hoverColor: Theme.ActiveHover,
				altTextColor: Theme.Background);
		}

		public static IControl DefaultButton(Text text, Command cmd)
		{
			return StrokeButton(text, cmd,
				foreground: Theme.Active,
				background: Color.Transparent,
				hoverColor: Theme.ActiveHover);
		}

		static IControl SolidColorButton(Text text, Command cmd, Brush foreground, Brush background, Brush hoverColor, Brush altTextColor)
		{
			return Button.Create(cmd,
					state =>
						Label.Create(
								text,
								textAlignment: TextAlignment.Center,
								font: Theme.DescriptorFont,
								color: Theme.CurrentTheme.Select( theme => (
									theme == Themes.OriginalDark
										? altTextColor
										: foreground))
									.Switch())
							.Center()
							.WithBackground(Shapes.Rectangle(
								fill: Observable.CombineLatest(
										state.IsEnabled, state.IsHovered,
										(enabled, hovering) =>
											hovering
												? hoverColor
												: background)
									.Switch(),
								cornerRadius: Observable.Return(new CornerRadius(2)))))
					.WithHeight(DefaultButtonHeight)
					.SetCursor(Cursor.Pointing);

		}

		static IControl StrokeButton(Text text, Command cmd, Brush foreground, Brush background, Brush hoverColor)
		{
			return Button.Create(cmd,
						state =>
							Label.Create(
									text,
									textAlignment: TextAlignment.Center,
									font: Theme.DescriptorFont,
									color: Observable.CombineLatest(
											state.IsEnabled, state.IsHovered,
											(enabled, hovering) =>
												hovering
													? hoverColor
													: foreground)
										.Switch())
								.CenterVertically()
								.WithBackground(Shapes.Rectangle(
									fill: background,
									cornerRadius: Observable.Return(new CornerRadius(2)),
									stroke: Stroke.Create(1, Observable.CombineLatest(
											state.IsEnabled, state.IsHovered,
											(enabled, hovering) =>
												hovering
													? hoverColor
													: foreground)
										.Switch()))))
					.WithHeight(DefaultButtonHeight)
					.SetCursor(Cursor.Pointing);

		}

		public static Points DefaultButtonHeight = 24;

		public static IControl TextButton(Text text, Command cmd, Brush color, Brush hoverColor, Font font)
		{
			return Button.Create(cmd,
					state =>
						Label.Create(
								text,
								textAlignment: TextAlignment.Left,
								font: font,
								color: Observable.CombineLatest(
										state.IsEnabled, state.IsHovered,
										(enabled, hovering) =>
											hovering
												? hoverColor
												: color)
									.Switch()))
				.SetCursor(Cursor.Pointing);

		}
	}
}
