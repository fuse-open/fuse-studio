using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Outracks
{
	public static partial class Property
	{
		/// <summary>
		/// `prop.Deferred()` returns a property that remembers its last set value but defers pushing it to `prop` until invalidated.
		/// </summary>
		public static IProperty<T> Deferred<T>(this IProperty<T> self)
		{
			return new DeferredProperty<T>(self);
		}
	}

	class DeferredProperty<T> : IProperty<T>
	{
		readonly IProperty<T> _source;
		readonly BehaviorSubject<Optional<T>> _maybeSetValue = new BehaviorSubject<Optional<T>>(Optional.None<T>());

		readonly IObservable<T> _realtimeValue;

		public DeferredProperty(IProperty<T> source)
		{
			_source = source;

			_realtimeValue = Observable.Merge(
				_maybeSetValue.NotNone(),
				source.Do(_ => _maybeSetValue.OnNext(Optional.None<T>())));
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			return _realtimeValue.Subscribe(observer);
		}

		public IObservable<bool> IsReadOnly
		{
			get { return _source.IsReadOnly; }
		}

		public void Write(T value, bool save)
		{
			_maybeSetValue.OnNext(value);
			if (save)
				_source.Write(value, save: true);
		}
	}

}