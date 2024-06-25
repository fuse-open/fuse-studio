using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace Outracks.Fusion.Threading
{
	public abstract class DispatcherBase : IScheduler
	{
		public abstract void Enqueue(Action action);

		public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
		{
			var disposable = new SingleAssignmentDisposable();
			Enqueue(() => { disposable.Disposable = action(this, state); });
			return disposable;
		}

		public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
		{
			if (Scheduler.Normalize(dueTime).Ticks == 0)
				return Schedule(state, action);
			return Scheduler.Default.Schedule(state, dueTime, (sched, s) => Schedule(state, action));
		}

		public IDisposable Schedule<TState>(
			TState state,
			DateTimeOffset dueTime,
			Func<IScheduler, TState, IDisposable> action)
		{
			return Scheduler.Default.Schedule(state, dueTime, (sched, s) => Schedule(state, action));
		}

		public DateTimeOffset Now
		{
			get { return Scheduler.Default.Now; }
		}
	}
}