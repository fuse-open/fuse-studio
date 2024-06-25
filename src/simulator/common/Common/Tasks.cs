using Uno;
using Uno.Collections;

using Uno.Threading;

namespace Outracks.Simulator
{
	public class Task<T>
	{
		internal readonly Mutex Mutex = new Mutex();

		readonly Action _wait;

		internal Task(Action wait)
		{
			_wait = wait;
		}

		public bool IsCompleted { get; internal set; }

		public bool IsFaulted { get; internal set; }

		public Exception Exception { get; internal set; }

		T _result;
		public T Result
		{
			get
			{
				_wait();

				if (IsFaulted)
					throw Exception; // TODO: don't mess up stack trace

				return _result;
			}
			internal set { _result = value; }
		}
	}


	public static class Tasks
	{
		public static Task<T> WaitForFirstResult<T>(IEnumerable<Task<T>> tasks, Func<IEnumerable<Exception>, T> onNoResult)
		{
			return Run<T>(new WaitForFirstResult<T>(tasks, onNoResult).Execute);
		}

		public static Task<T> Run<T>(Func<T> method)
		{
			var t = new TaskThread<T>(method);

			if defined(DotNet)
			{
				t.Thread.IsBackground = true;
			}

			t.Thread.Start();

			return t.Task;
		}
	}


	class TaskThread<T>
	{
		public readonly Task<T> Task;
		public readonly Thread Thread;

		readonly Func<T> _func;

		public TaskThread(Func<T> func)
		{
			_func = func;
			Thread = new Thread(Run);
			Task = new Task<T>(Thread.Join);
		}

		void Run()
		{
			try
			{
				Task.Result = _func();
				Task.IsCompleted = true;
			}
			catch (Exception e)
			{
				Task.Exception = e;
				Task.IsFaulted = true;
				Task.IsCompleted = true;
			}
		}
	}

	class WaitForFirstResult<T>
	{
		readonly List<Task<T>> _pending;
		readonly Func<IEnumerable<Exception>, T> _onNoResult;

		public WaitForFirstResult(
			IEnumerable<Task<T>> pending,
			Func<IEnumerable<Exception>, T> onNoResult)
		{
			_pending = pending.ToList();
			_onNoResult = onNoResult;
		}

		public T Execute()
		{
			var exceptions = new List<Exception>();

			while (_pending.Count != 0)
			{
				foreach (var task in _pending.ToArray())
				{
					if (task.IsCompleted)
					{
						if (task.IsFaulted)
							exceptions.Add(task.Exception);
						else
							return task.Result;

						_pending.Remove(task);
					}

					Thread.Sleep(10);
				}
			}

			return _onNoResult(exceptions);
		}
	}

}
