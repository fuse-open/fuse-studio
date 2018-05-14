using System.Collections.Generic;
using System.Reactive;
using NUnit.Framework;

namespace Outracks.Fusion.Tests.ObservableList
{
	[TestFixture]
	public class ConcatTest
	{
		[Test]
		public void Add()
		{
			var left = new ListBehaviorSubject<int>();
			var right = new ListBehaviorSubject<int>();

			left.OnAdd(1);
			left.OnAdd(2);

			right.OnAdd(4);
			right.OnAdd(5);

			var list = new List<int>();

			var sub = left.Concat(right).Subscribe(Observer.Create<ListChange<int>>(change => change.Apply(list)));

			CollectionAssert.AreEqual(new[] { 1, 2, 4, 5 }, list);

			left.OnAdd(3);
			right.OnAdd(6);

			CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, list);

			sub.Dispose();

			left.OnAdd(100);
			right.OnAdd(200);

			CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, list);
		}

		[Test]
		public void Remove()
		{
			var left = new ListBehaviorSubject<int>();
			var right = new ListBehaviorSubject<int>();

			left.OnAdd(1);
			left.OnAdd(2);
			left.OnAdd(3);

			right.OnAdd(4);
			right.OnAdd(5);
			right.OnAdd(6);

			var list = new List<int>();

			var sub = left.Concat(right).Subscribe(Observer.Create<ListChange<int>>(change => change.Apply(list)));

			CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, list);

			left.OnRemove(2);
			right.OnRemove(2);

			CollectionAssert.AreEqual(new[] { 1, 2, 4, 5 }, list);

			left.OnRemove(0);
			right.OnRemove(0);

			CollectionAssert.AreEqual(new[] { 2, 5 }, list);

			sub.Dispose();
		}

		[Test]
		public void Replace()
		{
			var left = new ListBehaviorSubject<int>();
			var right = new ListBehaviorSubject<int>();

			left.OnAdd(1);
			left.OnAdd(2);
			left.OnAdd(3);

			right.OnAdd(4);
			right.OnAdd(5);
			right.OnAdd(6);

			var list = new List<int>();

			var sub = left.Concat(right).Subscribe(Observer.Create<ListChange<int>>(change => change.Apply(list)));

			CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, list);

			left.OnReplace(0, 100);
			right.OnReplace(0, 200);

			CollectionAssert.AreEqual(new[] { 100, 2, 3, 200, 5, 6 }, list);

			left.OnReplace(2, 0);
			right.OnReplace(2, 0);

			CollectionAssert.AreEqual(new[] { 100, 2, 0, 200, 5, 0 }, list);

			sub.Dispose();
		}

		[Test]
		public void Clear()
		{
			var left = new ListBehaviorSubject<int>();
			var right = new ListBehaviorSubject<int>();

			left.OnAdd(1);
			left.OnAdd(2);
			left.OnAdd(3);

			right.OnAdd(4);
			right.OnAdd(5);
			right.OnAdd(6);

			var list = new List<int>();

			var sub = left.Concat(right).Subscribe(Observer.Create<ListChange<int>>(change => change.Apply(list)));

			CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, list);

			left.OnClear();

			CollectionAssert.AreEqual(new[] { 4, 5, 6 }, list);

			right.OnClear();

			CollectionAssert.AreEqual(new int[] { }, list);

			left.OnAdd(1);
			left.OnAdd(2);
			left.OnAdd(3);

			right.OnAdd(4);
			right.OnAdd(5);
			right.OnAdd(6);

			CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, list);

			right.OnClear();

			CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list);

			left.OnClear();

			CollectionAssert.AreEqual(new int[] { }, list);

			sub.Dispose();
		}

		[Test]
		public void Completed1()
		{
			var left = new ListBehaviorSubject<int>();
			var right = new ListBehaviorSubject<int>();

			var list = new List<int>();

			bool completed = false;

			var sub = left.Concat(right)
				.Subscribe(Observer.Create<ListChange<int>>(change => change.Apply(list), () => completed = true));

			left.OnCompleted();
			Assert.IsFalse(completed);
			right.OnCompleted();
			Assert.IsTrue(completed);
		}

		[Test]
		public void Completed2()
		{
			var left = new ListBehaviorSubject<int>();
			var right = new ListBehaviorSubject<int>();

			var list = new List<int>();

			bool completed = false;

			var sub = left.Concat(right)
				.Subscribe(Observer.Create<ListChange<int>>(change => change.Apply(list), () => completed = true));

			right.OnCompleted();
			Assert.IsFalse(completed);
			left.OnCompleted();
			Assert.IsTrue(completed);
		}
	}
}