using System;
using System.Reactive.Linq;
using Outracks.Diagnostics;

namespace Outracks.Fusion
{
	public static class Collapsable
	{
		public static IControl MakeCollapsable(
			this IControl content,
			RectangleEdge edge,
			IObservable<bool> expanded,
			bool lazy = true,
			bool unrootWhenCollapsed = false,
			bool animate = true)
		{
			edge = edge.FlipVerticallyOnMac();
			var expandedValueStep = expanded.Select(isExpanded => isExpanded ? 1.0 : 0.0);
			var expandedValue = animate ? expandedValueStep.LowPass() : expandedValueStep;

			var u = edge.NormalAxis();
			var v = edge.NormalAxis().Opposite();

			var lazyContent = lazy == false
				? content
				: expanded
					.SkipWhile(e => e == false).Take(1)
					.Select(isExpanded => content)
					.StartWith(Control.Empty)
					.Switch()
					;

			if (unrootWhenCollapsed)
				lazyContent = lazyContent.HideWhen(expandedValue.Select(ev => ev < 0.01));

			return Layout
				.Layer(self => new[]
				{
					lazyContent
						.Clip()
						.WithFrame(parentFrame =>
							Rectangle
								.FromPositionSize(
									position: edge.GetOffsetPosition(lazyContent.DesiredSize[u], expandedValue),
									size: Size.Create(
										lazyContent.DesiredSize[u],
										parentFrame.Size[v],
										firstAxis: u))
								.MoveTo(parentFrame.Position))
						.Clip()
				})
				.WithSize(desiredSize: Size.Create(
					lazyContent.DesiredSize[u].Mul(expandedValue),
					expanded.Switch(e => e ? lazyContent.DesiredSize[v] : ObservableMath.Zero),
					firstAxis: u));
		}

		public static Point<IObservable<Points>> GetOffsetPosition(this RectangleEdge edge, IObservable<Points> width,  IObservable<double> expandedValue)
		{
			if (edge.IsMinimal())
				return ObservableMath.ZeroPoint;

			var negativeWidth = width.Select(w => 0 - w);
			var collapsedValue = expandedValue.Select(v => 1 - v);

			return Point.Create(
				negativeWidth.Mul(collapsedValue),
				ObservableMath.Zero,
				firstAxis: edge.NormalAxis());
		}

		public static IControl ShowOnWindows(this IControl self)
		{
			return self.ShowWhen(Observable.Return(Platform.IsWindows));
		}

		public static IControl ShowOnMac(this IControl self)
		{
			return self.ShowWhen(Observable.Return(Platform.IsMac));
		}

		public static IControl ShowWhen(this IControl control, IObservable<bool> condition)
		{
			var empty = Control.Empty;
			return condition.ToControl(c => c ? control : empty);
		}

		public static IControl HideOnWindows(this IControl self)
		{
			return self.HideWhen(Observable.Return(Platform.IsWindows));
		}

		public static IControl HideOnMac(this IControl self)
		{
			return self.HideWhen(Observable.Return(Platform.IsMac));
		}

		public static IControl HideWhen(this IControl control, IObservable<bool> condition)
		{
			var empty = Control.Empty;
			return condition.ToControl(c => c ? empty : control);
		}
	}
}
