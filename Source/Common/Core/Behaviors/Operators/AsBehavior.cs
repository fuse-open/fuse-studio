using System;
using System.Reactive.Subjects;

namespace Outracks
{
	public static partial class Behavior
	{
		public static IBehaviorSubject<T> AsBehavior<T>(this BehaviorSubject<T> subject)
		{
			return new BehaviorSubjectWrapper<T>(subject);
		}

		public class BehaviorSubjectWrapper<T> : IBehaviorSubject<T>
		{
			readonly BehaviorSubject<T> _subject;

			public BehaviorSubjectWrapper(BehaviorSubject<T> subject)
			{
				_subject = subject;
			}

			public IDisposable Subscribe(IObserver<T> observer)
			{
				return _subject.Subscribe(observer);
			}

			public T Value
			{
				get { return _subject.Value; }
			}

			public void OnNext(T value)
			{
				_subject.OnNext(value);
			}

			public void OnError(Exception error)
			{
				_subject.OnError(error);
			}

			public void OnCompleted()
			{
				_subject.OnCompleted();
			}
		}
	}
}