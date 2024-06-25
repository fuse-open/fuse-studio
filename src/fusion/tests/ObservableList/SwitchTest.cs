using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Subjects;
using NUnit.Framework;

namespace Outracks.Fusion.Tests.ObservableList
{
	[TestFixture]
	public class SwitchTest1
	{
		[Test]
		public void Basics()
		{
			var lbs1 = new ListBehaviorSubject<int>();
			var lbs2 = new ListBehaviorSubject<int>();

			lbs1.OnAdd(1);
			lbs1.OnAdd(2);

			lbs2.OnAdd(4);
			lbs2.OnAdd(5);

			var source = new BehaviorSubject<IObservableList<int>>(lbs1);

			var list = new List<int>();

			var sub = source.Switch().Subscribe(Observer.Create<ListChange<int>>(change => change.Apply(list)));

			CollectionAssert.AreEqual(new[] { 1, 2 }, list);

			lbs1.OnAdd(3);
			lbs1.OnAdd(6);

			CollectionAssert.AreEqual(new[] { 1, 2, 3, 6 }, list);

			source.OnNext(lbs2);

			CollectionAssert.AreEqual(new[] { 4, 5 }, list);

			source.OnNext(lbs2);

			CollectionAssert.AreEqual(new[] { 4, 5 }, list);

			lbs2.OnRemove(0);

			CollectionAssert.AreEqual(new[] { 5 }, list);

			lbs2.OnReplace(0, 41);

			CollectionAssert.AreEqual(new[] { 41 }, list);

			lbs2.OnClear();

			CollectionAssert.IsEmpty(list);

			lbs2.OnAdd(4);
			lbs2.OnAdd(5);

			sub.Dispose();

			lbs1.OnAdd(100);
			lbs2.OnAdd(200);

			CollectionAssert.AreEqual(new[] { 4, 5 }, list);
		}

		[Test]
		public void OnComplete()
		{
			var lbs1 = new ListBehaviorSubject<int>();
			var lbs2 = new ListBehaviorSubject<int>();

			lbs1.OnAdd(1);
			lbs1.OnAdd(2);

			lbs2.OnAdd(3);
			lbs2.OnAdd(4);

			var source = new BehaviorSubject<IObservableList<int>>(lbs1);

			var list = new List<int>();

			var completed = false;

			var sub = source
				.Switch()
				.Subscribe(
					Observer.Create<ListChange<int>>(
						change => change.Apply(list),
						() => completed = true));

			lbs1.OnCompleted();

			// Inner completion doesn't imply completion until outer is complete
			Assert.IsFalse(completed);
			CollectionAssert.AreEqual(new[] { 1, 2 }, list);

			source.OnNext(lbs2);

			CollectionAssert.AreEqual(new[] { 3, 4 }, list);

			source.OnCompleted();

			// Outer completion implies completion when last inner is complete
			Assert.IsFalse(completed);

			lbs2.OnCompleted();

			Assert.IsTrue(completed);

			CollectionAssert.AreEqual(new[] { 3, 4 }, list);

			sub.Dispose();
		}

		[Test]
		public void OnInnerError()
		{
			var lbs1 = new ListBehaviorSubject<int>();

			lbs1.OnAdd(1);
			lbs1.OnAdd(2);

			var source = new BehaviorSubject<IObservableList<int>>(lbs1);

			var list = new List<int>();

			var completed = false;
			Exception error = null;

			var sub = source
				.Switch()
				.Subscribe(
					Observer.Create<ListChange<int>>(
						change => change.Apply(list),
						e => error = e,
						() => completed = true));

			var ex = new Exception("AAAAAA");

			lbs1.OnError(ex);

			Assert.AreEqual(ex, error);
			Assert.IsFalse(completed);

			sub.Dispose();
		}

		[Test]
		public void OnOuterError()
		{
			var lbs1 = new ListBehaviorSubject<int>();

			lbs1.OnAdd(1);
			lbs1.OnAdd(2);

			var source = new BehaviorSubject<IObservableList<int>>(lbs1);

			var list = new List<int>();

			var completed = false;
			Exception error = null;

			var sub = source
				.Switch()
				.Subscribe(
					Observer.Create<ListChange<int>>(
						change => change.Apply(list),
						e => error = e,
						() => completed = true));

			var ex = new Exception("AAAAAA");

			source.OnError(ex);

			Assert.AreEqual(ex, error);
			Assert.IsFalse(completed);

			sub.Dispose();
		}
	}

	[TestFixture]
	public class SwitchTest2
	{
		[Test]
		public void Basics()
		{
			var src1 = new Subject<int>();
			var src2 = new Subject<int>();
			var src3 = new Subject<int>();

			var src = new ListBehaviorSubject<IObservable<int>>();

			var list = new List<int>();

			var sub = src.Switch().Subscribe(change => change.Apply(list));

			CollectionAssert.IsEmpty(list);

			src.OnAdd(src1);
			src.OnAdd(src2);
			src.OnAdd(src3);

			CollectionAssert.IsEmpty(list);

			src1.OnNext(1);

			CollectionAssert.AreEqual(new[] { 1 }, list);

			src3.OnNext(3);

			CollectionAssert.AreEqual(new[] { 1, 3 }, list);

			src2.OnNext(2);

			CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list);

			src1.OnNext(11);
			src2.OnNext(22);
			src3.OnNext(33);

			CollectionAssert.AreEqual(new[] { 11, 22, 33 }, list);

			src.OnRemove(1);

			CollectionAssert.AreEqual(new[] { 11, 33 }, list);

			src.OnReplace(1, src2);

			CollectionAssert.AreEqual(new[] { 11 }, list);

			src2.OnNext(22);

			CollectionAssert.AreEqual(new[] { 11, 22 }, list);

			src.OnClear();

			CollectionAssert.IsEmpty(list);

			sub.Dispose();
		}

		[Test]
		public void OnCompleteOuterFirst()
		{
			var src1 = new Subject<int>();
			var src2 = new Subject<int>();
			var src3 = new Subject<int>();

			var src = new ListBehaviorSubject<IObservable<int>>();

			var list = new List<int>();

			bool completed = false;

			var sub = src.Switch().Subscribe(change => change.Apply(list), () => completed = true);

			src.OnAdd(src1);
			src.OnAdd(src2);
			src.OnAdd(src3);

			src.OnCompleted();
			Assert.IsFalse(completed);
			src1.OnCompleted();
			Assert.IsFalse(completed);
			src3.OnCompleted();
			Assert.IsFalse(completed);
			src2.OnCompleted();
			Assert.IsTrue(completed);

			sub.Dispose();
		}

		[Test]
		public void OnCompleteInnerFirst()
		{
			var src1 = new Subject<int>();
			var src2 = new Subject<int>();
			var src3 = new Subject<int>();

			var src = new ListBehaviorSubject<IObservable<int>>();

			var list = new List<int>();

			bool completed = false;

			var sub = src.Switch().Subscribe(change => change.Apply(list), () => completed = true);

			src.OnAdd(src1);
			src.OnAdd(src2);
			src.OnAdd(src3);

			src1.OnCompleted();
			Assert.IsFalse(completed);
			src3.OnCompleted();
			Assert.IsFalse(completed);
			src2.OnCompleted();
			Assert.IsFalse(completed);
			src.OnCompleted();
			Assert.IsTrue(completed);

			sub.Dispose();
		}

		[Test]
		public void OnOuterError()
		{
			var src1 = new Subject<int>();
			var src2 = new Subject<int>();
			var src3 = new Subject<int>();

			var src = new ListBehaviorSubject<IObservable<int>>();

			var list = new List<int>();

			Exception error = null;

			var sub = src.Switch().Subscribe(change => change.Apply(list), e => error = e);

			src.OnAdd(src1);
			src.OnAdd(src2);
			src.OnAdd(src3);

			var ex = new Exception("AAAAAAAaa");

			src.OnError(ex);

			Assert.AreEqual(ex, error);

			sub.Dispose();
		}

		[Test]
		public void OnInnerError()
		{
			var src1 = new Subject<int>();
			var src2 = new Subject<int>();
			var src3 = new Subject<int>();

			var src = new ListBehaviorSubject<IObservable<int>>();

			var list = new List<int>();

			Exception error = null;

			var sub = src.Switch().Subscribe(change => change.Apply(list), e => error = e);

			CollectionAssert.IsEmpty(list);

			src.OnAdd(src1);
			src.OnAdd(src2);
			src.OnAdd(src3);

			var ex = new Exception("AAAAAAAaa");

			src2.OnError(ex);

			Assert.AreEqual(ex, error);

			sub.Dispose();
		}
	}
}