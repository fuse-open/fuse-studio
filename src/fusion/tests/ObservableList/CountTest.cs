using System.Reactive;
using NUnit.Framework;

namespace Outracks.Fusion.Tests.ObservableList
{
	[TestFixture]
	public class CountTest
	{
		[Test]
		public void Count_reflects_changes()
		{
			var lbs = new ListBehaviorSubject<int>();

			lbs.OnAdd(1);
			lbs.OnAdd(2);
			lbs.OnAdd(3);

			var count = 0;

			var sub = lbs.Count().Subscribe(Observer.Create<int>(newCount => count = newCount));

			Assert.AreEqual(3, count);

			lbs.OnRemove(0);

			Assert.AreEqual(2, count);

			lbs[0] = 5;

			Assert.AreEqual(2, count);

			lbs.OnClear();

			Assert.AreEqual(0, count);

			sub.Dispose();

			lbs.OnAdd(41);

			Assert.AreEqual(0, count);
		}
	}
}
