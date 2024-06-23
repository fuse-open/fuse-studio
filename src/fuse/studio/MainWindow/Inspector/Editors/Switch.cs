using System.Reactive.Linq;
using Outracks.Fusion;

namespace Outracks.Fuse.Inspector.Editors
{
	class SwitchEditor
	{
		public static IControl Create(IProperty<bool> enabled)
		{
			return Create(enabled, circleRadius: 18, additionalWidth: 11);
		}

		public static IControl CreateSmall(IProperty<bool> enabled)
		{
			return Create(enabled, circleRadius: 15, additionalWidth: 10);
		}

		static IControl Create(IProperty<bool> enabled, Points circleRadius, Points additionalWidth)
		{
			double outlineThickness = 1;
			Points backgroundPadding = 1;

			var enabledValue = enabled.Select(isEnabled => isEnabled ? 1.0 : 0.0).LowPass(0.3);

			var circleSize = Size.Create(circleRadius);
			var cornerRadius = Observable.Return(new CornerRadius(circleRadius / 2.5));
			var circle = Shapes.Circle(
					Stroke.Create(1.0, Theme.SwitchThumbStroke),
					Theme.SwitchThumbFill)
				.WithSize(circleSize);

			var foreground = circle.WithPadding(
				enabledValue.Select(v => Thickness.Create<Points>(v * additionalWidth, 0, (1 - v) * additionalWidth, 0)));

			var backgroundStroke = Stroke.Create(
				Observable.Return(outlineThickness),
				Theme.SwitchInactiveStroke.Mix(Theme.SwitchActiveStroke, enabledValue),
				Observable.Return(StrokeDashArray.Solid));
			var backgroundBrush = Theme.SwitchInactiveBackground.Mix(Theme.SwitchActiveBackground, enabledValue);
			var background = Shapes.Rectangle(
				backgroundStroke,
				backgroundBrush,
				cornerRadius);

			var content = foreground
				.WithBackground((background.WithPadding(new Thickness<Points>(backgroundPadding))));
			return Button.Create(
				clicked: enabled.Toggle(),
				content: buttonStates => content);
		}
	}
}