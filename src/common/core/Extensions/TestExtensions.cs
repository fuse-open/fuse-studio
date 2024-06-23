using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Outracks
{
	public static class TestExtensions
	{
		public static ObservableResults<T> Check<T>(this IObservable<T> o)
		{
			var results = new ObservableResults<T>();
			o.Check(results);
			return results;
		}

		public static IObservable<T> Check<T>(this IObservable<T> o, ObservableResults<T> results)
		{
			o.Subscribe(
				v => { results.Results.Add(v); },
				e => { results.Error = e; },
				() => { results.Completed = true; });
			return o;
		}
	}

	public class ObservableResults<T>
	{
		public readonly List<T> Results = new List<T>();
		public Exception Error;
		public bool Completed;

		public void AssertResult(bool completed, T singleResult, Exception error = null, Func<T, T, bool> resultComparer = null)
		{
			AssertResults(completed, new List<T> { singleResult }, error, resultComparer);
		}

		public void AssertResults(bool completed, IEnumerable<T> results, Exception error = null, Func<T, T, bool> resultComparer = null)
		{
			Assert.AreEqual(completed, Completed, "Observable complete state differs from expected. Expected: " + completed + ", but was " + Completed);
			Assert.AreEqual(error, Error, "Observable error state differs from expected. Expected: " + error + ", but was " + Error);
			if (resultComparer == null)
			{
				Assert.AreEqual(results, Results);
			}
			else
			{
				var comparer = Comparer<T>.Create((x, y) => { return resultComparer(x, y) ? 0 : 1; });
				Assert.That(Results, Is.EqualTo(results).Using<T>(comparer));
			}
		}
	}
}