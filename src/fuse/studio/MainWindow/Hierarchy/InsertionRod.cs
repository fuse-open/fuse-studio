using System;
using System.Reactive.Linq;
using Outracks.Fusion;

namespace Outracks.Fuse.Hierarchy
{
	static class InsertionRod
	{
		public static IControl Create(IObservable<Optional<Point<IObservable<Points>>>> hoverIndicator)
		{
			var color = Color.FromBytes(94, 184, 205);

			var indicator = Layout.Dock()
				.Left(Shapes.Circle(Stroke.Create(2, color))
					.WithHeight(6).WithWidth(6))
				.Fill(Shapes.Rectangle(fill: color)
					.WithHeight(2).CenterVertically());

			return indicator
				.WithPadding(
					left: hoverIndicator.NotNone().Select(v => v.X.Sub(indicator.DesiredSize.Width).Sub(2)).Switch(),
					top: hoverIndicator.NotNone().Select(v => v.Y).Switch().Sub(indicator.DesiredSize.Height.Div(2)))
				.DockTop()
				.ShowWhen(hoverIndicator
					.Select(i => i.HasValue)
					.DistinctUntilChanged()
					.Throttle(TimeSpan.FromSeconds(1.0 / 100.0))
					.ObserveOn(Application.MainThread));
		}
	}
}