using System;

namespace Outracks
{
	public static partial class Behavior
	{
		public static IBehavior<T> Return<T>(T value)
		{
			return new ConstantBehavior<T>(value);
		}
	}

	class ConstantBehavior<T> : IBehavior<T>
	{
		public T Value { get; private set; }

		public ConstantBehavior(T value)
		{
			Value = value;
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			observer.OnNext(Value);
			observer.OnCompleted();
			return Disposable.Empty;
		}
	}
}