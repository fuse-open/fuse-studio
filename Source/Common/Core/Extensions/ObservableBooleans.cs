using System;
using System.Reactive.Linq;

namespace Outracks
{
	public static class ObservableBooleanExtensions
	{
		public static IObservable<bool> Or(this IObservable<bool> a, IObservable<bool> b)
		{
			return a.CombineLatest(b, (ab, bb) => ab || bb).DistinctUntilChanged();
		}

		public static IObservable<bool> Or(this IObservable<bool> self, Func<IObservable<bool>> fallback)
		{
			return self.Select(s => s
					? Observable.Return(true)
					: fallback())
				.Switch()
				.DistinctUntilChanged();
		}

		public static IObservable<bool> And(this IObservable<bool> a, IObservable<bool> b)
		{
			return a.CombineLatest(b, (ab, bb) => ab && bb).DistinctUntilChanged();
		}

		public static IObservable<bool> And(this IObservable<bool> self, Func<IObservable<bool>> fallback)
		{
			return self.Select(s => !s
					? Observable.Return(false)
					: fallback())
				.Switch()
				.DistinctUntilChanged();
		}


		public static IObservable<bool> IsFalse(this IObservable<bool> self)
		{
			return self.Select(b => !b);
		}

		public static IObservable<bool> Is<T>(this IObservable<T> self, T value)
		{
			return self.Select(s => s.Equals(value));
		}

	}
}
