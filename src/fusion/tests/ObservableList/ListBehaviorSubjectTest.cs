using System.Collections.Generic;
using System.Reactive;
using NUnit.Framework;

namespace Outracks.Fusion.Tests.ObservableList
{
	[TestFixture]
	public class ListBehaviorSubjectTest
	{
		[Test]
		public void ListBehaviorSubject_Subscribe_reflects_changes()
		{
			var lbs = new ListBehaviorSubject<int>();

			lbs.OnAdd(1);
			lbs.OnAdd(2);
			lbs.OnAdd(3);

			var list = new List<int>();
			var sub = lbs.Subscribe(Observer.Create<ListChange<int>>(change => change.Apply(list)));

			CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list);

			lbs.OnAdd(4);

			CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, list);

			lbs.OnInsert(0, 0);

			CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4 }, list);

			lbs.OnRemove(1);

			CollectionAssert.AreEqual(new[] { 0, 2, 3, 4 }, list);

			lbs.OnClear();

			CollectionAssert.AreEqual(new int[] {}, list);

			sub.Dispose();

			lbs.OnAdd(41);

			CollectionAssert.AreEqual(new int[] { }, list);
		}
	}
}
