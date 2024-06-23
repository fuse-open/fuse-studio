using System;
using System.Reactive.Linq;

namespace Outracks
{
	public static class CombineExtensions
	{
		public static IObservable<Tuple<T1, T2>> CombineLatest<T1, T2>(this IObservable<T1> self, IObservable<T2> other)
		{
			return self.CombineLatest(other, Tuple.Create);
		}

		public static IObservable<Tuple<T1, T2, T3>> CombineLatest<T1, T2, T3>(this IObservable<T1> self, IObservable<T2> other, IObservable<T3> other2)
		{
			return self.CombineLatest(other, other2, Tuple.Create);
		}

		public static IObservable<Tuple<T1, T2, T3, T4>> CombineLatest<T1, T2, T3, T4>(this IObservable<T1> self, IObservable<T2> other, IObservable<T3> other2, IObservable<T4> other3)
		{
			return self.CombineLatest(other, other2, other3, Tuple.Create);
		}

		public static IDisposable Subscribe<T1, T2>(this IObservable<Tuple<T1, T2>> self, Action<T1, T2> action)
		{
			return self.Subscribe(t => action(t.Item1, t.Item2));
		}

	}
}
