using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using NUnit.Framework;

namespace Outracks.Fusion.Tests
{
	[TestFixture]
	public class PropertyBindingExtensionsTests
	{
		[Test]
		public void BindNativeProperty_invokes_update_action_while_is_rooted_is_true()
		{
			var isRooted = new BehaviorSubject<bool>(false);
			var value = new BehaviorSubject<int>(1337);
			var mountLocation = new MountLocation.Mutable { IsRooted = isRooted };
			int lastUpdatedValue = -1;
			mountLocation.BindNativeProperty(
				new PollingDispatcher(Thread.CurrentThread),
				string.Empty,
				value,
				v => lastUpdatedValue = v);
			Assert.That(value.HasObservers, Is.False);
			isRooted.OnNext(true);
			Assert.That(value.HasObservers, Is.True);
			Assert.That(lastUpdatedValue, Is.EqualTo(1337));
			isRooted.OnNext(false);
			Assert.That(value.HasObservers, Is.False);
		}

		[Test]
		public void BindNativeProperty_only_propagates_changed_values()
		{
			var value = new Subject<int>();
			var mountLocation = new MountLocation.Mutable { IsRooted = Observable.Return(true) };
			int lastUpdatedValue = -1;
			int updateCount = 0;
			mountLocation.BindNativeProperty(
				new PollingDispatcher(Thread.CurrentThread),
				string.Empty,
				value,
				v =>
				{
					updateCount++;
					lastUpdatedValue = v;
				});
			value.OnNext(1337);
			Assert.That(lastUpdatedValue, Is.EqualTo(1337));
			Assert.That(updateCount, Is.EqualTo(1));
			value.OnNext(1337);
			Assert.That(updateCount, Is.EqualTo(1));
			value.OnNext(200);
			Assert.That(lastUpdatedValue, Is.EqualTo(200));
			Assert.That(updateCount, Is.EqualTo(2));
		}
	}
}