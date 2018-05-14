using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Outracks.IO
{
	public static class SleepFunctions
	{
		public static Func<int, TimeSpan> DiskRetry = i => TimeSpan.FromMilliseconds(1 << i);
	}

	public static class RetryLoop
	{
		/// <summary>
		/// Retries func until it succeeds or the specified maximum number of attempts is reached
		/// This function will sleep 2^i ms between each attempt.
		/// </summary>
		/// <exception cref="AggregateException"></exception>
		public static T Try<T>(
			int maxAttempts,
			Func<T> func,
			Func<int, TimeSpan> sleepFunction = null)
		{
			sleepFunction = sleepFunction ?? SleepFunctions.DiskRetry;

			return LoopUntilSuccess(_ => func(), sleepFunction, maxAttempts);
		}

		/// <summary>
		/// Retries func until it succeeds or the specified maximum number of attempts is reached
		/// </summary>
		/// <exception cref="AggregateException"></exception>
		public static T LoopUntilSuccess<T>(
			Func<int, T> func,
			Func<int, TimeSpan> sleepFunction = null, 
			int? maxAttempts = null)
		{
			sleepFunction = sleepFunction ?? SleepFunctions.DiskRetry;

			var exceptions = new List<Exception>();
			for (int i = 0; maxAttempts.HasValue == false || i < maxAttempts; i++)
			{
				try
				{
					return func(i);
				}
				catch (Exception e)
				{
					exceptions.Add(e);
				}
				Thread.Sleep(sleepFunction(i));
			}
			throw new AggregateException("Retry count of " + maxAttempts + " exceeded", exceptions.ToArray());
		}

		/// <summary>
		/// Retries func until it succeeds or the specified maximum number of attempts is reached
		/// </summary>
		/// <exception cref="AggregateException"></exception>
		public static async Task<T> AsyncLoopUntilSuccess<T>(
			Func<int, Task<T>> func,
			Func<int, TimeSpan> sleepFunction = null,
			int? maxAttempts = null)
		{
			sleepFunction = sleepFunction ?? SleepFunctions.DiskRetry;

			var exceptions = new List<Exception>();
			for (int i = 0; maxAttempts.HasValue == false || i < maxAttempts; i++)
			{
				try
				{
					return await func(i);
				}
				catch (Exception e)
				{
					exceptions.Add(e);
				}
				await Task.Delay(sleepFunction(i));
			}
			throw new AggregateException("Retry count of " + maxAttempts + " exceeded", exceptions.ToArray());
		}

	}
}
