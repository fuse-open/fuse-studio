using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using NUnit.Framework;
using Outracks.Fusion;

namespace Outracks.Tests
{
	class YieldingDispatcher : IDispatcher
	{
		readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();
		readonly EventWaitHandle _flushEvt = new AutoResetEvent(false);
		readonly EventWaitHandle _doneFlush = new AutoResetEvent(false);
		readonly Thread _thread;

		public YieldingDispatcher(CancellationToken ct)
		{
			_thread = new Thread(
				() =>
				{
					while (ct.IsCancellationRequested == false)
					{
						_flushEvt.WaitOne();
						Drain();
						_doneFlush.Set();
					}
				});
			_thread.Start();
		}

		public void Flush()
		{
			_flushEvt.Set();
			_doneFlush.WaitOne();
		}

		public void FlushNoneBlocking()
		{
			_flushEvt.Set();
		}

		public void Enqueue(Action action)
		{
			_queue.Enqueue(action);
			if(_thread == Thread.CurrentThread)
				Drain();
		}

		void Drain()
		{
			Action action;
			while (_queue.TryDequeue(out action))
			{
				try
				{
					action();
				}
				catch (Exception e)
				{
					Console.WriteLine("CatchAll failed with: " + e);
				}
			}
		}
	}

	[TestFixture]
	public class ObserveOnTest
	{
		[Test]
		public static void TestYieldNormalSequence()
		{
			var ctSource = new CancellationTokenSource();
			var dispatcher = new YieldingDispatcher(ctSource.Token);

			var sub = new Subject<int>();
			var obs = sub.ObserveOn(dispatcher);
			TestSequenceOrder(dispatcher, sub, obs);

			ctSource.Cancel();
		}

		[Test]
		public static void TestYieldConnectableSequence()
		{
			var ctSource = new CancellationTokenSource();
			var dispatcher = new YieldingDispatcher(ctSource.Token);

			var sub = new Subject<int>();
			var obs = sub.Publish().ObserveOn(dispatcher);
			obs.Connect();

			TestSequenceOrder(dispatcher, sub, obs);

			ctSource.Cancel();
		}

		static void TestSequenceOrder(YieldingDispatcher dispatcher, ISubject<int> sub, IObservable<int> obs)
		{
			IList<int> result = null;
			obs.SelectMany(i => Observable.FromAsync(() => dispatcher.InvokeAsync(
					() =>
					{
						Thread.Sleep(100);
						return i;
					})))
				.Buffer(2)
				.Subscribe(
					l =>
					{
						result = l;
					});

			sub.OnNext(1);
			dispatcher.FlushNoneBlocking(); // This is the key here, we start processing item '1' before we add item '2'
			sub.OnNext(2);

			while (result == null)
			{
				dispatcher.Flush();
				Thread.Sleep(1);
			}

			Assert.True(result.Count == 2, "Unexpected number of items in observable sequence. Was expecting 2");
			Assert.True(result[0] == 1, "Unexpected observable sequence order. Was expecting 1");
			Assert.True(result[1] == 2, "Unexpected observable sequence order. Was expecting 2");
		}
	}
}