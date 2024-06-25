using NUnit.Framework;

namespace Outracks.Fusion.Tests
{
	[TestFixture]
	public class WithSizeTest
	{
		readonly TestControl _control = new TestControl("sized control", 100, 100);

		[Test]
		public void WithSize()
		{
			var root = _control.WithSize(Size.Create<Points>(2,4));
			root.AssertDesiredSize("root control", 2,4);
			root.MountRoot();

			_control.AssertHasNativeParent();
			_control.AssertFrame(0,0,4,4);
		}

		[Test]
		public void WithHeight()
		{
			var root = _control.WithHeight(2);
			root.AssertDesiredSize("root control", 100, 2);
			root.MountRoot();

			_control.AssertHasNativeParent();
			_control.AssertFrame(0, 0, 4, 4);
		}

		[Test]
		public void WithWidth()
		{
			var root = _control.WithWidth(2);
			root.AssertDesiredSize("root control", 2, 100);
			root.MountRoot();

			_control.AssertHasNativeParent();
			_control.AssertFrame(0, 0, 4, 4);
		}
	}
}