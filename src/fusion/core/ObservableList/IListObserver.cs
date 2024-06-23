using System;

namespace Outracks.Fusion
{
	public interface IListObserver<in T> : IObserver<ListChange<T>>
	{
	}
}