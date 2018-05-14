using System;
using System.Threading.Tasks;

namespace Outracks
{
	public static class TaskExtensions
	{

		/// <exception cref="TimeoutException"></exception>
		public static async Task<T> TimeoutAfter<T>(this Task<T> task, TimeSpan timeout)
		{
			if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
				return await task;
			throw new TimeoutException("Task exceeded timeout of " + timeout);
		}

		public static Optional<T> GetResultAndUnpackExceptions<T>(this Task<T> task, TimeSpan timeout)
		{
			try
			{
				return task.GetResult(timeout);
			}
			catch (AggregateException e)
			{
				e.InnerException.RethrowWithStackTrace();
				throw e.InnerException;
			}
		}

		public static Optional<T> GetResult<T>(this Task<T> task, TimeSpan timeout)
		{
			return task.Wait(timeout)
				? Optional.Some(task.Result)
				: Optional.None<T>();
		}

		public static void WaitAndUnpackExceptions(this Task task)
		{
			try
			{
				task.Wait();
			}
			catch (AggregateException e)
			{
				e.InnerException.RethrowWithStackTrace();
				throw e.InnerException;
			}
		}


		public static T GetResultAndUnpackExceptions<T>(this Task<T> task)
		{
			try
			{
				return task.Result;
			}
			catch (AggregateException e)
			{
				e.InnerException.RethrowWithStackTrace();
				throw e.InnerException;
			}
		}
	}
}