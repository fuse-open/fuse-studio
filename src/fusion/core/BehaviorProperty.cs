using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Outracks.Fusion
{
	public static class BehaviorProperty
	{
		public static IBehaviorProperty<T> Create<T>(
			BehaviorSubject<T> subject,
			Optional<Command> invalidate = default(Optional<Command>),
			IObservable<bool> isReadOnly = null)
		{
			return new BehaviorSubjectWrapper<T>(subject, invalidate, isReadOnly ?? Observable.Return(false));
		}

		class BehaviorSubjectWrapper<T> : IBehaviorProperty<T>
		{
			readonly BehaviorSubject<T> _subject;
			readonly Command _invalidate;
			readonly IObservable<bool> _isReadOnly;

			public BehaviorSubjectWrapper(
				BehaviorSubject<T> subject,
				Optional<Command> invalidate,
				IObservable<bool> isReadOnly)
			{
				_subject = subject;
				_invalidate = invalidate.Or(Command.Disabled);
				_isReadOnly = isReadOnly;
			}

			public IDisposable Subscribe(IObserver<T> observer)
			{
				return _subject.Subscribe(observer);
			}

			public T Value
			{
				get { return _subject.Value; }
			}

			public IObservable<bool> IsReadOnly
			{
				get { return _isReadOnly; }
			}

			public void Write(T value, bool save)
			{
				_subject.OnNext(value);
				if (save)
					_invalidate.ExecuteOnce();
			}

		}
	}
}