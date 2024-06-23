using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Outracks
{
	public static partial class Property
	{
		public static IProperty<T> Create<T>(T initialValue)
		{
			return new MutableProperty<T>(initialValue);
		}

		public static IProperty<T> Create<T>(BehaviorSubject<T> subject)
		{
			return new MutableProperty<T>(subject);
		}
	}

	public class MutableProperty<T> : IProperty<T>
	{
		readonly BehaviorSubject<T> _subject;
		readonly IObservable<T> _distinctValue;

		public MutableProperty(T initialValue)
		{
			_subject = new BehaviorSubject<T>(initialValue);
			_distinctValue = _subject.DistinctUntilChanged();
		}

		public MutableProperty(BehaviorSubject<T> subject)
		{
			_subject = subject;
			_distinctValue = _subject.DistinctUntilChanged();
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			return _distinctValue.Subscribe(observer);
		}

		public IObservable<bool> IsReadOnly
		{
			get { return Observable.Return(false); }
		}

		public void Write(T value, bool save)
		{
			_subject.OnNext(value);
		}
	}
}
