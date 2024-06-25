using System;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static class Slider
	{
		public static IControl Create(IProperty<double> value = null, double min = 0.0, double max = 1.0)
		{
			return Implementation.Factory(value ?? Property.Create(0.0), Observable.Return(min), Observable.Return(max));
		}

		public static class Implementation
		{
			public static Func<IProperty<double>, IObservable<double>, IObservable<double>, IControl> Factory;
		}
	}
}