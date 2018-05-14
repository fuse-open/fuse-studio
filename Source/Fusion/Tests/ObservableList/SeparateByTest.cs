using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Outracks.Fusion.Tests.ObservableList
{
	[TestFixture]
	public class SeparateByTest
	{
		[Test]
		public void Basics()
		{
			var src = new ListBehaviorSubject<int>();

			var list = new List<int>();

			var sub = src.SeparateBy(() => -1).Subscribe(change => change.Apply(list));

			CollectionAssert.IsEmpty(list);

			for (var i = 0; i < 3; ++i)
				src.OnAdd(i);

			CollectionAssert.AreEqual(new[] { 0, -1, 1, -1, 2 }, list);

			src.OnRemove(0);

			CollectionAssert.AreEqual(new[] { 1, -1, 2 }, list);

			src.OnRemove(1);

			CollectionAssert.AreEqual(new[] { 1 }, list);

			src.OnInsert(0, 4);

			CollectionAssert.AreEqual(new[] { 4, -1, 1 }, list);

			src.OnAdd(5);

			CollectionAssert.AreEqual(new[] { 4, -1, 1, -1, 5 }, list);

			src.OnReplace(1, 11);

			CollectionAssert.AreEqual(new[] { 4, -1, 11, -1, 5 }, list);

			src.OnClear();

			CollectionAssert.IsEmpty(list);

			sub.Dispose();
		}

		[Test]
		public void OnComplete()
		{
			var src = new ListBehaviorSubject<int>();

			var list = new List<int>();

			var completed = false;

			var sub = src.SeparateBy(() => -1).Subscribe(change => change.Apply(list), () => completed = true);

			Assert.IsFalse(completed);

			src.OnCompleted();

			Assert.IsTrue(completed);

			sub.Dispose();
		}

		[Test]
		public void OnError()
		{
			var src = new ListBehaviorSubject<int>();

			var list = new List<int>();

			Exception error = null;

			var sub = src.SeparateBy(() => -1).Subscribe(change => change.Apply(list), e => error = e);

			Assert.IsNull(error);

			var ex = new Exception("AAa");

			src.OnError(ex);

			Assert.AreEqual(ex, error);

			sub.Dispose();
		}
	}
}
