using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Outracks
{
	public static partial class Property
	{
		public static IProperty<T> AutoInvalidate<T>(this IProperty<T> self, Optional<TimeSpan> throttle = default (Optional<TimeSpan>))
		{
			return new AutoInvalidateProperty<T>(self, throttle);
		}
	}

	class AutoInvalidateProperty<T> : IProperty<T>
	{
		readonly Subject<T> _subject = new Subject<T>();
		readonly IProperty<T> _source;

		public AutoInvalidateProperty(IProperty<T> source, Optional<TimeSpan> throttle = default (Optional<TimeSpan>))
		{
			_source = source;

			throttle
				.Select(t => _subject.Throttle(t))
				.Or((IObservable<T>) _subject)
				.Subscribe(v => source.Write(v, save: true));
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
			_subject.OnNext(value);
		}
	}
}