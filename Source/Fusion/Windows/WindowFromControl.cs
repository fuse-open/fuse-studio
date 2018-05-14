using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;

namespace Outracks.Fusion.Windows
{
	public static class WindowFromControl
	{
		public static IObservable<Optional<T>> GetWindow<T>(this FrameworkElement dummyElement) 
			where T : System.Windows.Window
		{
			// TODO: this might be expensive
			return DataBinding.ObservableFromNativeEvent<object>(dummyElement, "LayoutUpdated")
				.StartWith(new object())
				.Select(_ =>
					Fusion.Application.MainThread.InvokeAsync(() =>
					{
						var hwndSource = PresentationSource.FromVisual(dummyElement);
						if (hwndSource == null)
							return Optional.None();

						var window = hwndSource.RootVisual as T;
						if (window == null)
							return Optional.None();

						return Optional.Some(window);
					}))
				.Switch()
				.DistinctUntilChanged();
		}

		public static IObservable<Ratio<Pixels, Points>> GetDpi(this FrameworkElement dummyElement)
		{
			return GetWindow<DpiAwareWindow>(dummyElement)
				.Select(win => win
					.Select(w => w.DensityFactor))
				.NotNone()
				.DistinctUntilChanged()
				.Switch();
		}
	}
}