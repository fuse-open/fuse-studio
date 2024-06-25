using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Outracks
{
	public static partial class Property
	{
		public static IProperty<T> DistinctUntilChangedOrSet<T>(this IProperty<T> self)
		{
			var userSet = new Subject<Optional<T>>();
			return self
				.With(value:
					self.Select(Optional.Some)
						.Merge(userSet)
						.DistinctUntilChanged()
						.NotNone())
				.Intercept(onWrite: value =>
					userSet.OnNext(Optional.None()));
		}

		public static IProperty<T> Intercept<T>(this IProperty<T> self, Action<T> onWrite)
		{
			return new InterceptingProperty<T>(self, onWrite);
		}
	}

	class InterceptingProperty<T> : IProperty<T>
	{
		readonly IProperty<T> _source;
		readonly Action<T> _onWrite;

		public InterceptingProperty(IProperty<T> source, Action<T> onWrite)
		{
			_source = source;
			_onWrite = onWrite;
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			return _source.Subscribe(observer);
		}

		public IObservable<bool> IsReadOnly
		{
			get { return _source.IsReadOnly; }
		}

		public void Write(T value, bool save)
		{
			_source.Write(value, save);
			_onWrite(value);
		}
	}

}