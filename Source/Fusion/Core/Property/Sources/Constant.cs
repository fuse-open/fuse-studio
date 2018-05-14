using System;
using System.Reactive.Linq;

namespace Outracks
{
	public static partial class Property
	{
		public static IProperty<T> Default<T>()
		{
			return Constant(default(T));
		}

		public static IProperty<T> Constant<T>(T value)
		{
			return new ConstantProperty<T>(value);
		}
	}

	sealed class ConstantProperty<T> : IProperty<T>
	{
		readonly T _value;

		public ConstantProperty(T value)
		{
			_value = value;
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			observer.OnNext(_value);
			observer.OnCompleted();
			return Disposable.Empty;
		}

		public IObservable<bool> IsReadOnly
		{
			get { return Observable.Return(true); }
		}

		public void Write(T value, bool save)
		{
			// Writing to read only property is NOP
		}
	}
}