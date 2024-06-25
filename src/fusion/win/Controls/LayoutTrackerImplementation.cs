using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Windows;

namespace Outracks.Fusion.Windows.Controls
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
					.Select(elm => (FrameworkElement) elm);

				var content = trackerFunc(new LayoutTracker(layerElm));
				child.OnNext(new[] { content });
				return layer.WithSize(content.DesiredSize);
			};
		}

		class LayoutTracker : ILayoutTracker
		{
			readonly IObservable<FrameworkElement> _toElement;

			public LayoutTracker(IObservable<FrameworkElement> toElement)
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
					.Select(elm => (FrameworkElement)elm);

				Observable.CombineLatest(fromElement, _toElement, CalculateRelativeFrame)
					.Switch()
					.ConnectWhile(content.IsRooted)
					.Subscribe(frame);

				return content;
			}

			private static IObservable<Rectangle<Points>> CalculateRelativeFrame(FrameworkElement from, FrameworkElement to)
			{
				return Observable.Interval(TimeSpan.FromMilliseconds(100), new DispatcherScheduler(from.Dispatcher))
					.Select(_ => RelativeFrame(from: from, to: to))
					.DistinctUntilChanged();
			}

			static Rectangle<Points> RelativeFrame(FrameworkElement from, FrameworkElement to)
			{
				var translation = from.TranslatePoint(new System.Windows.Point(), to);
				return Rectangle.FromPositionSize(Point.Create<Points>(translation.X, translation.Y), new Size<Points>(from.ActualWidth, from.ActualHeight));
			}
		}
	}
}