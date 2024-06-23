using System;

namespace Outracks.Fusion
{
	public interface IObservableList<out T> : IObservable<ListChange<T>>
	{
	}
}