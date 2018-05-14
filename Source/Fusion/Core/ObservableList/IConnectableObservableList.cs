using System.Reactive.Subjects;

namespace Outracks.Fusion
{
	public interface IConnectableObservableList<out T> : IObservableList<T>, IConnectableObservable<ListChange<T>>
	{
	}
}