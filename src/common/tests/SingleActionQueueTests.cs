using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Outracks.Tests
{
	class SingleActionQueueTests
	{
		[Test]
		public void EnqueuingBeforeDequeuing()
		{
			var q = new SingleActionQueue();
			var didAction = false;
			q.Enqueue(() => didAction = true);
			q.Dequeue()();
			Assert.That(didAction, Is.True);
		}

		[Test]
		public void DequeuingBeforeEnqueuing()
		{
			var q = new SingleActionQueue();
			var didAction = false;
			var dequeue = Task.Run(
				() =>
				{
					q.Dequeue()();
				});
			Thread.Sleep(1000); //Sorry :( Need to give task time to start and block, to avoid false positives
			q.Enqueue(() => didAction = true);
			dequeue.Wait();
			Assert.That(didAction, Is.True);
		}

		[Test]
		public void EnqueuingReplacesActionInQueue()
		{
			var q = new SingleActionQueue();
			var value = 0;
			q.Enqueue(() => { value = 1; });
			q.Enqueue(() => { value = 2; });
			q.Enqueue(() => { value = 3; });
			q.Dequeue()();
			Assert.That(value, Is.EqualTo(3));
		}

		[Test]
		public void OnlyDequeuesActionOnce()
		{
			var q = new SingleActionQueue();
			int dequeues = 0;
			Task.Run(() => q.Dequeue()());
			Task.Run(() => q.Dequeue()());
			q.Enqueue(() => Interlocked.Increment(ref dequeues));
			Thread.Sleep(1000); //Again, sorry :(
			Assert.That(dequeues, Is.EqualTo(1));
		}

		//Should ideally have been tested more, but it's very hard to test concurrent stuff like this properly
	}
}
