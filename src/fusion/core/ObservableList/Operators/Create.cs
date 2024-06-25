using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Outracks.Fusion
{
	public static partial class ObservableList
	{
		public static IObservableList<T> Empty<T>()
		{
			return new ConstantObservableList<T>(ImmutableList<T>.Empty);
		}

		public static IObservableList<T> Return<T>(IEnumerable<T> elements)
		{
			return new ConstantObservableList<T>(elements.ToImmutableList());
		}

		public static IObservableList<T> Create<T>(Func<IObserver<ListChange<T>>, IDisposable> subscribe)
		{
			return new ObservableListFromDelegate<T>(subscribe);
		}

		public static IObservableList<T> UnsafeAsObservableList<T>(this IObservable<ListChange<T>> self)
		{
			return Create<T>(self.Subscribe);
		}
	}

	class ConstantObservableList<T> : IObservableList<T>
	{
		readonly ImmutableList<T> _value;
		ListChange<T> _changes;
		public Optional<ImmutableList<T>> ConstantValue
		{
			get { return _value; }
		}

		public ConstantObservableList(ImmutableList<T> value)
		{
			_value = value;
		}

		public IDisposable Subscribe(IObserver<ListChange<T>> observer)
		{
			if (_changes == null)
				_changes = ListChange.Combine(_value.Select((t, i) => ListChange.Insert(i, t)));
			observer.OnNext(_changes);
			observer.OnCompleted();
			return Disposable.Empty;
		}
	}

	class ObservableListFromDelegate<T> : IObservableList<T>
	{
		readonly Func<IObserver<ListChange<T>>, IDisposable> _subscribe;

		public ObservableListFromDelegate(Func<IObserver<ListChange<T>>, IDisposable> subscribe)
		{
			_subscribe = subscribe;
		}

		public IDisposable Subscribe(IObserver<ListChange<T>> observer)
		{
			return _subscribe(observer);
		}
	}
}
