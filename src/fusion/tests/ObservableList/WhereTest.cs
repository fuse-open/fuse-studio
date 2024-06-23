using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Outracks.Fusion.Tests.ObservableList
{
	[TestFixture]
	public class WhereTest
	{
		[Test]
		public void Basics()
		{
			var src = new ListBehaviorSubject<int>();

			var list = new List<int>();

			var sub = src.Where(i => i % 2 == 0).Subscribe(change => change.Apply(list));

			CollectionAssert.IsEmpty(list);

			for (var i = 0; i < 6; ++i)
				src.OnAdd(i);

			CollectionAssert.AreEqual(new[] { 0, 2, 4 }, list);

			src.OnRemove(1);

			CollectionAssert.AreEqual(new[] { 0, 2, 4 }, list);

			src.OnReplace(1, 4);

			CollectionAssert.AreEqual(new[] { 0, 4, 4 }, list);

			src.OnInsert(0, -2);

			CollectionAssert.AreEqual(new[] { -2, 0, 4, 4 }, list);

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

			var sub = src.Where(i => i > 2).Subscribe(change => change.Apply(list), () => completed = true);

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

			var sub = src.Where(i => i > 2).Subscribe(change => change.Apply(list), e => error = e);

			Assert.IsNull(error);

			var ex = new Exception("AAa");

			src.OnError(ex);

			Assert.AreEqual(ex, error);

			sub.Dispose();
		}
	}
}
