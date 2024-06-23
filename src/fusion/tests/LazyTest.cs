using NUnit.Framework;

namespace Outracks.Fusion.Tests
{
	[TestFixture]
	public class LazyTest
	{
		[Test]
		public void Lazy_is_not_forced_until_mounted()
		{
			var ran = false;

			var lazyControl = Control.Lazy(
				() =>
				{
					ran = true;
					return Control.Empty;
				}).WithSize(Size.Create<Points>(100, 100));

			lazyControl.AssertDesiredSize("lazy control", 100, 100);
			Assert.False(ran);
			lazyControl.MountRoot();
			Assert.True(ran);
		}

		[Test]
		public void Lazy_is_not_forced_more_than_once()
		{
			var ran = 0;

			var lazyControl = Control.Lazy(
				() =>
				{
					++ran;
					return Control.Empty;
				}).WithSize(Size.Create<Points>(100, 100));

			lazyControl.AssertDesiredSize("lazy control", 100, 100);
			Assert.AreEqual(0, ran);
			lazyControl.MountRoot();
			Assert.AreEqual(1, ran);
			lazyControl.MountRoot();
			Assert.AreEqual(1, ran);
		}
	}
}