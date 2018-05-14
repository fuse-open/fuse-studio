using System;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Interop;

namespace Outracks.Fusion.Windows
{
	public class DpiAwareWindow : System.Windows.Window
	{
		readonly ReplaySubject<Ratio<Pixels, Points>> _dpi = new ReplaySubject<Ratio<Pixels, Points>>(1);

		public IObservable<Ratio<Pixels, Points>> DensityFactor
		{
			get { return _dpi; }
		}

		public DpiAwareWindow()
		{
			Loaded += OnLoaded;	
		}

		void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			var source = (HwndSource)PresentationSource.FromVisual(this);
			_dpi.OnNext(source.CompositionTarget.TransformToDevice.M11);
		}
	}
}