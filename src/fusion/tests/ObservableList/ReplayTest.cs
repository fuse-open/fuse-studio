using System;
using System.Collections.Generic;
using System.Reactive;
using NUnit.Framework;

namespace Outracks.Fusion.Tests.ObservableList
{
	[TestFixture]
	public class ReplayTest
	{
		[Test]
		public void Replay_doesnt_emit_until_connected()
		{
			var lbs = new ListBehaviorSubject<int>();

			lbs.OnAdd(1);

			var replay = lbs.Replay();

			lbs.OnAdd(2);

			var list = new List<int>();

			var sub = replay.Subscribe(Observer.Create<ListChange<int>>(change => change.Apply(list)));

			CollectionAssert.AreEqual(new int[] { }, list);

			var conn = replay.Connect();

			CollectionAssert.AreEqual(new[] { 1, 2 }, list);

			lbs.OnAdd(3);

			CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list);

			sub.Dispose();
			conn.Dispose();
		}

		[Test]
		public void Replay_handles_multiple_subscriptions()
		{
			var lbs = new ListBehaviorSubject<int>();

			lbs.OnAdd(1);

			var replay = lbs.Replay();

			lbs.OnAdd(2);

			var list = new List<int>();
			var list2 = new List<int>();

			var sub = replay.Subscribe(Observer.Create<ListChange<int>>(change => change.Apply(list)));
			var sub2 = replay.Subscribe(Observer.Create<ListChange<int>>(change => change.Apply(list2)));

			CollectionAssert.AreEqual(new int[] { }, list);
			CollectionAssert.AreEqual(new int[] { }, list2);

			var conn = replay.Connect();

			CollectionAssert.AreEqual(new[] { 1, 2 }, list);
			CollectionAssert.AreEqual(new[] { 1, 2 }, list2);

			lbs.OnAdd(3);

			CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list);
			CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list2);

			sub.Dispose();
			sub2.Dispose();
			conn.Dispose();
		}

		[Test]
		public void Replay_handles_subscription_after_connect()
		{
			var lbs = new ListBehaviorSubject<int>();

			lbs.OnAdd(1);

			var replay = lbs.Replay();

			lbs.OnAdd(2);

			var list = new List<int>();

			var sub = replay.Subscribe(Observer.Create<ListChange<int>>(change => change.Apply(list)));

			CollectionAssert.AreEqual(new int[] { }, list);

			var conn = replay.Connect();

			CollectionAssert.AreEqual(new[] { 1, 2 }, list);

			var list2 = new List<int>();
			var sub2 = replay.Subscribe(Observer.Create<ListChange<int>>(change => change.Apply(list2)));

			CollectionAssert.AreEqual(new[] { 1, 2 }, list2);

			lbs.OnAdd(3);

			CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list);
			CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list2);

			sub2.Dispose();

			lbs.OnAdd(4);

			CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, list);
			CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list2);

			sub.Dispose();
			conn.Dispose();
		}

		[Test]
		public void Replay_handles_completion()
		{
			var lbs = new ListBehaviorSubject<int>();

			lbs.OnAdd(1);

			var replay = lbs.Replay();

			lbs.OnAdd(2);

			var list = new List<int>();
			var completed = false;
			var sub = replay.Subscribe(Observer.Create<ListChange<int>>(change => change.Apply(list), () => completed = true));

			CollectionAssert.AreEqual(new int[] { }, list);

			var conn = replay.Connect();

			CollectionAssert.AreEqual(new[] { 1, 2 }, list);

			Assert.IsFalse(completed);

			lbs.OnCompleted();

			Assert.IsTrue(completed);

			var list2 = new List<int>();
			var completed2 = false;
			var sub2 = replay.Subscribe(Observer.Create<ListChange<int>>(change => change.Apply(list2), () => completed2 = true));

			Assert.IsTrue(completed2);

			sub2.Dispose();
			sub.Dispose();
			conn.Dispose();
		}

		[Test]
		public void Replay_handles_errors()
		{
			var lbs = new ListBehaviorSubject<int>();

			lbs.OnAdd(1);

			var replay = lbs.Replay();

			lbs.OnAdd(2);

			var list = new List<int>();
			Exception error = null;
			var sub = replay.Subscribe(Observer.Create<ListChange<int>>(change => change.Apply(list), e => error = e));

			CollectionAssert.AreEqual(new int[] { }, list);

			var conn = replay.Connect();

			CollectionAssert.AreEqual(new[] { 1, 2 }, list);

			Assert.IsNull(error);

			var sourceError = new Exception("Actual");

			lbs.OnError(sourceError);

			Assert.AreEqual(sourceError, error);

			var list2 = new List<int>();
			Exception error2 = null;
			var sub2 = replay.Subscribe(Observer.Create<ListChange<int>>(change => change.Apply(list2), e => error2 = e));

			Assert.AreEqual(sourceError, error2);

			sub2.Dispose();
			sub.Dispose();
			conn.Dispose();
		}
	}
}
