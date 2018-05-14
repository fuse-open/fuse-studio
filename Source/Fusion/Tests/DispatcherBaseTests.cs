using System;
using System.Threading;
using NUnit.Framework;
using Outracks.Fusion;
using Outracks.Fusion.Threading;
using System.Reactive.Concurrency;

namespace Outracks.Common.Tests
{
	[TestFixture]
	public class DispatcherBaseTests
	{
		class TestDispatcher : DispatcherBase
		{
			public override void Enqueue(Action action)
			{
				action();
			}
		}

		[Test]
		public void Schedule_with_state_and_action()
		{
			var dispatcher = new TestDispatcher();
			bool disposeCalled = false;

			using (dispatcher.Schedule(1337, (sched, state) =>
				{
					Assert.That(state, Is.EqualTo(1337));
					Assert.That(sched, Is.EqualTo(dispatcher));
					return Disposable.Create(() => disposeCalled = true);
				}))
			{
				// Should be scheduled immediately
				Assert.That(disposeCalled, Is.False);
			}

			Assert.That(disposeCalled, Is.True);
		}

		[Test]
		[Ignore("Fails (very) randomly on AppVeyor")]
		public void Schedule_in_future_using_TimeSpan_invokes_Enqueue_in_background_on_threadpool()
		{
			var dispatcher = new TestDispatcher();
			bool disposeCalled = false;
			var eventWaitHandle = new ManualResetEvent(false);
			int? statePassedToCallback = null;
			IScheduler schedulerPassedToCallback = null;
			using (dispatcher.Schedule(1337, TimeSpan.FromMilliseconds(15), (sched, state) =>
				{
					statePassedToCallback = state;
					schedulerPassedToCallback = sched;
					eventWaitHandle.Set();
					return Disposable.Create(() => disposeCalled = true);
				}))
			{
				Assert.That(eventWaitHandle.WaitOne(1000), Is.True, "Timed out waiting for action to get invoked in background");
				Assert.That(disposeCalled, Is.False);
			}

			Assert.That(statePassedToCallback, Is.EqualTo(1337));
			Assert.That(schedulerPassedToCallback, Is.EqualTo(dispatcher));
			Assert.That(disposeCalled, Is.True);
		}

		[Test]
		public void Schedule_in_future_using_DateTimeOffset_invokes_Enqueue_in_background_on_threadpool()
		{
			var dispatcher = new TestDispatcher();
			bool disposeCalled = false;
			var eventWaitHandle = new ManualResetEvent(false);
			var dueTime = DateTimeOffset.Now.AddMilliseconds(100);
			int? statePassedToCallback = null;
			IScheduler schedulerPassedToCallback = null;
			using (dispatcher.Schedule(1337, dueTime, (sched, state) =>
				{
					statePassedToCallback = state;
					schedulerPassedToCallback = sched;
					eventWaitHandle.Set();
					return Disposable.Create(() => disposeCalled = true);
				}))
			{
				Assert.That(eventWaitHandle.WaitOne(1000), Is.True, "Timed out waiting for action to get invoked in background");
				Assert.That(disposeCalled, Is.False);
			}

			Assert.That(statePassedToCallback, Is.EqualTo(1337));
			Assert.That(schedulerPassedToCallback, Is.EqualTo(dispatcher));
			Assert.That(disposeCalled, Is.True);
		}
	}
}
