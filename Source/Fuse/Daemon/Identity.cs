using System;
using System.Reactive.Subjects;

namespace Outracks
{
	static class Identity
	{
		public static Identity<T> Create<T>(T initialValue)
		{
			return new Identity<T>(initialValue);
		}
	}

	class Identity<T> : IDisposable //where T : class
    {
		readonly BehaviorSubject<T> _subject;
		readonly object _mutex = new object();

		T _ref;
		
		public T Value
		{
			get { return _subject.Value; }
		}

		public Identity(T initialValue)
		{
			_ref = initialValue;
			_subject = new BehaviorSubject<T>(initialValue);
			//_subject.Do(t => Console.WriteLine(Stopwatch.GetTimestamp() + " Next: " + t)).Subscribe();
		}

		public void Update(Func<T, T> transform)
		{
			_subject.OnNext(Apply(transform));
		}

		T Apply(Func<T, T> transform)
		{
			lock (_mutex)
				return _ref = transform(_ref);
		}

		//int subscriptionCount = 0;

		public IDisposable Subscribe(IObserver<T> observer)
		{
			//subscriptionCount++;
			//Console.WriteLine(GetHashCode().ToString("X") + " has " + subscriptionCount + " listeners");
		    return //Disposable.Combine(
				_subject.Subscribe(observer)
				//, Disposable.Create(() => subscriptionCount--))
				;
	    }

	    public void Dispose()
	    {
		    _subject.Dispose();
	    }
    }

}
