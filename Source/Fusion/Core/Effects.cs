using System;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static class Effects
	{
		public static IControl DropShadow(
			this IControl control, 
			Brush color = default(Brush), 
			IObservable<Points> radius = null,
			IObservable<double> angle = null, 
			IObservable<Points> distance = null)
		{
			return Implementation.DropShadow(control, 
				color | Color.Black.WithAlpha(a: 0.4f),
				radius ?? Observable.Return<Points>(15),
				angle ?? Observable.Return(-90.0),
				distance ?? Observable.Return<Points>(2));
		}

		public static class Implementation
		{
			public static Func<
				IControl,
				Brush,
				IObservable<Points>,
				IObservable<double>,
				IObservable<Points>,
				IControl> DropShadow;
		}
	}
}