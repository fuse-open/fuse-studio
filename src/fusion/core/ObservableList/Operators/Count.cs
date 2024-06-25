using System;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static partial class ObservableList
	{
		public static IObservable<int> Count<T>(this IObservableList<T> self)
		{
			return self.Scan(0,
				(count, change) =>
				{
					change(
						insert: (i, item) => ++count,
						replace: (i, item) => { },
						remove: i => --count,
						clear: () => count = 0);
					return count;
				}).DistinctUntilChanged();
		}

		public static IObservable<bool> IsEmpty<T>(this IObservableList<T> self)
		{
			return self.Count().Select(c => c == 0).DistinctUntilChanged();
		}

		public static IObservable<bool> IsNonEmpty<T>(this IObservableList<T> self)
		{
			return self.Count().Select(c => c > 0).DistinctUntilChanged();
		}
	}
}