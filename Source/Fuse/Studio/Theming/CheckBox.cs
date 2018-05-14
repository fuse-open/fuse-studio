using System;
using System.Reactive.Linq;
using Outracks.Fusion;

namespace Outracks.Fuse
{
	public static class CheckBox
	{
		public static IControl Create(IObservable<bool> enabled, Command toggle)
		{
			double outlineThickness = 1;
			var cornerRadius = Observable.Return(new CornerRadius(3));

			var enabledValue = enabled.Select(isEnabled => isEnabled ? 1.0 : 0.0).LowPass(0.3);

			var foreground = Icons.Checkmark(Brush.Transparent.Mix(Theme.SwitchThumbFill, enabledValue)).Center();

			var backgroundStroke = Stroke.Create(
				Observable.Return(outlineThickness),
				Theme.SwitchInactiveStroke.Mix(Theme.SwitchActiveStroke, enabledValue),
				Observable.Return(StrokeDashArray.Solid));
			var backgroundBrush = Theme.SwitchInactiveBackground.Mix(Theme.SwitchActiveBackground, enabledValue);
			var background =
				Shapes.Rectangle(
					backgroundStroke,
					backgroundBrush,
					cornerRadius);

			var content = foreground.WithBackground(background);
			return Button.Create(
					toggle,
					buttonStates => content)
				.WithSize(new Size<Points>(17, 17));
		}
	}
}