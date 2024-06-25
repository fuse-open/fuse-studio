using System;
using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Outracks.Fusion;

namespace Outracks.Fuse
{
	public class PopoverState
	{
		public BehaviorSubject<bool> IsVisible { get; set; }
	}

	public interface IPopover
	{
		IControl CreatePopover(
			RectangleEdge prefferedContentEdge,
			Func<PopoverState, IControl> content,
			Func<PopoverState, IControl> popover);
	}

	public static class Popover
	{
		public static IControl Host(Func<IPopover, IControl> content)
		{
			return LayoutTracker.Create(tracker =>
			{
				var hostFrame = new ReplaySubject<Rectangle<IObservable<Points>>>(1);
				var host = new Implementation
				{
					LayoutTracker = tracker,
					Popovers = new BehaviorSubject<IImmutableList<IControl>>(ImmutableList<IControl>.Empty),
					HostFrame = hostFrame
				};

				return content(host)
					.WithNativeOverlay(host.Popovers.Layer())
					.WithFrame(
						frame =>
						{
							hostFrame.OnNext(frame);
							return frame;
						});
			});
		}

		class Implementation : IPopover
		{
			public ILayoutTracker LayoutTracker;
			public BehaviorSubject<IImmutableList<IControl>> Popovers;
			public IObservable<Rectangle<IObservable<Points>>> HostFrame;

			public IControl CreatePopover(
				RectangleEdge prefferedContentEdge,
				Func<PopoverState, IControl> content,
				Func<PopoverState, IControl> popover)
			{
				var state = new PopoverState
				{
					IsVisible = new BehaviorSubject<bool>(false),
				};

				var contentFrameState = new ReplaySubject<Rectangle<Points>>(1);
				var contentFrame = contentFrameState.Transpose();

				var center = contentFrame.Center();

				var arrow = Shapes.Rectangle(fill: Theme.PanelBackground)
					.WithSize(new Size<Points>(13,13))
					.Rotate(Math.PI / 4)
					;

				var popoverContent = Control.Lazy(() => popover(state));

				var desiredContentEdge = contentFrame.GetEdge(prefferedContentEdge)
					.Add(popoverContent.DesiredSize.Height);
				var desiredHostEdge = HostFrame.Switch().GetEdge(prefferedContentEdge);

				var adjustedEdge = desiredContentEdge
					.CombineLatest(desiredHostEdge, (a, b) => a >= b)
					// Put popover on the opposite edge if there are not enough room for it on the preffered edge.
					.Select(opposite => opposite ? prefferedContentEdge.Opposite() : prefferedContentEdge);

				var arrowStartOffsetY = adjustedEdge
					.Select(e =>
						e.IsMinimal()
						? contentFrame.GetEdge(e).Sub(arrow.DesiredSize.Height)
						: contentFrame.GetEdge(e).Sub(4))
					.Switch();

				var contentStartOffsetY = adjustedEdge
					.Select(e =>
						e.IsMinimal()
						? contentFrame.GetEdge(e).Sub(popoverContent.DesiredSize.Height).Sub(arrow.DesiredSize.Height.Div(2))
						: contentFrame.GetEdge(e).Add(arrow.DesiredSize.Height.Div(2)).Sub(4))
					.Switch();

				var close = Command.Enabled(() => state.IsVisible.OnNext(false));

				var popoverControl = Control.Lazy(() =>
					Layout.Layer(self =>
					{
						var widthInterval = Observable.CombineLatest(
							self.NativeFrame.Width,
							popoverContent.DesiredSize.Width,
							center.X,
							(availableWidth, desiredWidth, desiredCenter) =>
							{
								desiredWidth += 8;// dropshadow compensation
								var halfWidth = desiredWidth / 2;

								return Interval.FromOffsetLength(
									offset:
										desiredCenter - halfWidth < 0
											? 0
											: desiredCenter + halfWidth > availableWidth
												? availableWidth - desiredWidth
												: desiredCenter - halfWidth,
									length:
										availableWidth.Min(desiredWidth));
							});

						return Observable.Return(
							new[]
							{
								Shapes.Rectangle(fill: Color.AlmostTransparent).OnMouse(pressed: close),
								arrow
									.WithFixedPosition(
										Rectangle.FromIntervals(
											Interval.FromOffsetLength(center.X.Sub(arrow.DesiredSize.Width.Div(2)).Sub(4), arrow.DesiredSize.Width),
											Interval.FromOffsetLength(arrowStartOffsetY, arrow.DesiredSize.Height))),
								popoverContent
									.WithBackground(
										Shapes.Rectangle(fill: Theme.PanelBackground, cornerRadius: Observable.Return(new CornerRadius(2))))
									.MakeHittable().Control
									.WithFixedPosition(
										Rectangle.FromIntervals(
											widthInterval.Transpose(),
											Interval.FromOffsetLength(contentStartOffsetY, popoverContent.DesiredSize.Height)))
							});
					})
					.DropShadow(radius: Observable.Return(new Points(4)), distance: Observable.Return(new Points(0.5))));

				state.IsVisible.SubscribeUsing(isVisible =>
					isVisible ? Popovers.AddTemporarily(popoverControl) : Disposable.Empty);

				return LayoutTracker.TrackVisualBounds(contentFrameState.OnNext, content(state));
			}
		}

		static IControl Rotate(this IControl control, double t)
		{
			return control.WithTransformation(Observable.Return(Matrix.Rotate(t)));
		}

		static IDisposable AddTemporarily(this BehaviorSubject<IImmutableList<IControl>> list, IControl control)
		{
			list.OnNext(list.Value.Add(control));
			return Disposable.Create(() => list.OnNext(list.Value.Remove(control)));
		}
	}

}
