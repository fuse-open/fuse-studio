using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;

namespace Outracks.Fusion.Tests.ObservableList
{
	[TestFixture]
	public class DisposeElementsTest
	{
		[Test]
		public void DisposeElements_disposes_elements()
		{
			var lbs = new ListBehaviorSubject<MyDisposable>();

			var d1 = new MyDisposable();
			var d2 = new MyDisposable();
			var d3 = new MyDisposable();
			var d4 = new MyDisposable();

			lbs.OnAdd(d1);
			lbs.OnAdd(d2);
			lbs.OnAdd(d3);

			var list = new List<MyDisposable>();

			var sub = lbs.DisposeElements().Subscribe(change => change.Apply(list));

			CollectionAssert.AreEqual(new[] { d1, d2, d3 }, list);

			Assert.AreEqual(0, d1.DisposeCount);
			Assert.AreEqual(0, d2.DisposeCount);
			Assert.AreEqual(0, d3.DisposeCount);
			Assert.AreEqual(0, d4.DisposeCount);

			lbs.OnRemove(1);

			CollectionAssert.AreEqual(new [] { d1, d3 }, list);

			Assert.AreEqual(0, d1.DisposeCount);
			Assert.AreEqual(1, d2.DisposeCount);
			Assert.AreEqual(0, d3.DisposeCount);
			Assert.AreEqual(0, d4.DisposeCount);

			lbs.OnReplace(1, d4);

			CollectionAssert.AreEqual(new [] { d1, d4 }, list);

			Assert.AreEqual(0, d1.DisposeCount);
			Assert.AreEqual(1, d2.DisposeCount);
			Assert.AreEqual(1, d3.DisposeCount);
			Assert.AreEqual(0, d4.DisposeCount);

			lbs.OnClear();

			CollectionAssert.AreEqual(new MyDisposable[] { }, list);

			Assert.AreEqual(1, d1.DisposeCount);
			Assert.AreEqual(1, d2.DisposeCount);
			Assert.AreEqual(1, d3.DisposeCount);
			Assert.AreEqual(1, d4.DisposeCount);

			sub.Dispose();
		}

		class MyDisposable : IDisposable
		{
			public int DisposeCount;

			public void Dispose()
			{
				++DisposeCount;
			}
		}
	}
}