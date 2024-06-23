using System;

namespace Outracks
{
	public interface IBehavior<out T> : IObservable<T>
	{
		T Value { get; }
	}
}