using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using AppKit;
using CoreGraphics;

namespace Outracks.Fusion.OSX
{
	class LayoutTrackerImplementation
	{
		public static void Initialize()
		{
			Fusion.LayoutTracker.Implementation = trackerFunc =>
			{
				var child = new ReplaySubject<IEnumerable<IControl>>(1);
				var layer = child.Layer();
				var layerElm = Fusion.Application.MainThread
					.InvokeAsync(() => layer.NativeHandle)
					.ToObservable()
					.Select(elm => (NSView)elm);

				var content = trackerFunc(new LayoutTracker(layerElm));
				child.OnNext(new[] { content });
				return layer.WithSize(content.DesiredSize);
			};
		}

		class LayoutTracker : ILayoutTracker
		{
			readonly IObservable<NSView> _toElement;

			public LayoutTracker(IObservable<NSView> toElement)
			{
				_toElement = toElement;
			}

			public IControl TrackVisualBounds(Action<Rectangle<Points>> frame, IControl content)
			{
				content = Layout.Layer(content)
					.WithSize(content.DesiredSize);

				var fromElement = Fusion.Application.MainThread
					.InvokeAsync(() => content.NativeHandle)
					.ToObservable()
					.Select(elm => (NSView)elm);

				Observable.CombineLatest(fromElement, _toElement, CalculateRelativeFrame)
					.Switch()
					.ConnectWhile(content.IsRooted)
					.Subscribe(frame);

				return content;
			}

			private static IObservable<Rectangle<Points>> CalculateRelativeFrame(NSView from, NSView to)
			{
				return Observable.Interval(TimeSpan.FromMilliseconds(100))
					.Select(_ => RelativeFrame(from: from, to: to))
					.Switch()
					.DistinctUntilChanged();
			}

			static Task<Rectangle<Points>> RelativeFrame(NSView from, NSView to)
			{
				return Fusion.Application.MainThread.InvokeAsync(
					() =>
					{
						var translation = from.ConvertPointToView(new CGPoint(from.Bounds.Left, from.Bounds.Height), to).ToFusion();

						return Rectangle.FromPositionSize(
							left: translation.X,
							top: new Points(to.Frame.Height) - translation.Y,
							width: new Points(from.Frame.Width), 
							height: new Points(from.Frame.Height));
					});
			}
		}
	}
}