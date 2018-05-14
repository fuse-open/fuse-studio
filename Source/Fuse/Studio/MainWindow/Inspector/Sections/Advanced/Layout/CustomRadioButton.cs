using System;
using System.Reactive.Linq;
using Outracks.Fusion;

namespace Outracks.Fuse.Inspector.Sections
{
	static class CustomRadioButton
	{
		public static readonly Points ButtonDim = new Points(20.0);
		public static readonly Size<Points> ButtonSize = new Size<Points>(ButtonDim, ButtonDim);

		public static readonly Size<Points> SmallRectSize = new Size<Points>(10.0, 4.0);

		public static IControl Create<T>(
			IAttribute<T> attribute,
			T value,
			Text toolTip,
			Func<Brush, Brush, Stroke, IControl> content)
			where T: struct
		{
			var isSelected = attribute.LocalValue()
				.Select(maybeValue => maybeValue.Equals(value))
				.DistinctUntilChanged()
				.Replay(1).RefCount();


			return Button.Create(
						clicked: Command.Enabled(() => attribute.Write(value, save: true)),
						content: state =>
						{
							var color = isSelected.Select(s => s ? Theme.Active : Theme.DisabledText).Switch();
							var backgroundColor = isSelected.Select(s => s
									? Theme.PanelBackground.Mix(Theme.Active, Observable.Return(0.25))
									: Observable.CombineLatest(
											state.IsEnabled, state.IsHovered,
											(enabled, hovering) =>
												hovering
													? Theme.FaintBackground
													: Theme.PanelBackground)
										.Switch())
								.Switch();

							var stroke = Stroke.Create(1.0, color);

							return content(backgroundColor, color, stroke);
						})
					.WithSize(ButtonSize)
					.SetToolTip(toolTip);
		}

		public static IControl CreateBackgroundRect(Brush fillColor, Brush strokeColor)
		{
			return Shapes.Rectangle(
				fill: fillColor,
				stroke: Stroke.Create(1.0, strokeColor));
		}

		public static IControl CreateSmallRect(Brush fillColor)
		{
			return Shapes.Rectangle(fill: fillColor).WithSize(SmallRectSize);
		}
	}
}