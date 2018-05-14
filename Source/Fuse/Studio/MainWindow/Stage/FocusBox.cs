using System.Reactive.Linq;

namespace Outracks.Fuse.Stage
{
	using Fusion;

	static class FocusBox
	{
		public static IControl Create(IProperty<bool> enabled)
		{
			var size = new Size<Points>(14, 14);
			var innerPadding = new Thickness<Points>(1.5);
			const float backgroundOpacity = 0.55f;
			var innerCornerRadius = Observable.Return(new CornerRadius(3));
			var outerCornerRadius = Observable.Return(new CornerRadius(4));

			var enabledValue = enabled.Select(isEnabled => isEnabled ? 1.0 : 0.0).LowPass(0.3).Select(d => (float)d);
			var backgroundOpacityValue = enabledValue.Select(v => v * backgroundOpacity);

			var inactiveContent = Shapes.Rectangle(
				stroke: Theme.FieldStroke,
				cornerRadius: innerCornerRadius);

			var content = Shapes.Rectangle(
					fill: Theme.Active.WithAlpha(enabledValue),
					cornerRadius: innerCornerRadius)
				.WithBackground(inactiveContent)
				.WithPadding(innerPadding)
				.WithSize(size)
				.WithBackground(Shapes.Rectangle(
					fill: Theme.Active.WithAlpha(backgroundOpacityValue),
					cornerRadius: outerCornerRadius));

			var toolTip = enabled.Select(
				b => b
					? "This is the currently active viewport."
					: "Click to make this the currently active viewport.");

			return Button.Create(
				clicked: Command.Enabled(() => enabled.Write(true)),
				content: buttonState => content,
				toolTip: toolTip.AsText());
		}
	}
}