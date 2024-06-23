using System.Collections.Generic;

namespace Outracks.Fusion
{
	public interface IListBehavior<out T> : IObservableList<T>
	{
		IReadOnlyList<T> Value { get; }
	}
}