using System;

namespace Outracks.Fusion
{
	public static partial class ObservableList
	{
		public static IObservableList<T> ConnectWhile<T>(this IObservableList<T> self, IObservable<bool> pred)
		{
			return ((IObservable<ListChange<T>>) self).ConnectWhile(pred).UnsafeAsObservableList();
		}
	}
}