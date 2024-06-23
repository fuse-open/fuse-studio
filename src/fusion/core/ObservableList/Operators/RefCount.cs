using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Outracks.Fusion
{
	public static partial class ObservableList
	{
		public static IObservableList<T> RefCount<T>(this IConnectableObservableList<T> self)
		{
			return ((IConnectableObservable<ListChange<T>>)self).RefCount().UnsafeAsObservableList();
		}
	}
}