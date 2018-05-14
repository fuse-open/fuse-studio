using System.Linq;

namespace Outracks.Fusion
{
	public static partial class ObservableList
	{
		public static IObservableList<T> Join<T>(this IObservableList<IObservableList<T>> self)
		{
			return self.AggregateAssoc(Empty<T>(), Concat).Switch();
		}
	}
}