using NUnit.Framework;

namespace Outracks.Fusion.Tests
{
	[TestFixture]
	public class FrameTest 
	{
		readonly TestControl _control = new TestControl("centered control", 0, 0);

		[Test]
		public void FrameLeft()
		{
			var root = _control.WithPadding(left: new Points(1));
			root.AssertDesiredSize("root control", 1, 0);
			root.MountRoot();

			_control.AssertFrame(1, 0, 4, 4);
			_control.AssertAvailableSize(width: 4-1, height: 4);
			_control.AssertHasNativeParent();
		}

		[Test]
		public void FrameRight()
		{
			var root = _control.WithPadding(right: new Points(1));
			root.AssertDesiredSize("root control", 1, 0);
			root.MountRoot();
	
			_control.AssertFrame(0, 0, 3, 4);
			_control.AssertAvailableSize(width: 4 - 1, height: 4);
			_control.AssertHasNativeParent();
		}

		[Test]
		public void FrameTop()
		{
			var root = _control.WithPadding(top: new Points(1));
			root.AssertDesiredSize("root control", 0, 1);
			root.MountRoot();

			_control.AssertFrame(0, 1, 4, 4);
			_control.AssertAvailableSize(width: 4, height: 4 - 1);
			_control.AssertHasNativeParent();
		}

		[Test]
		public void FrameBottom()
		{
			var root = _control.WithPadding(bottom: new Points(1));
			root.AssertDesiredSize("root control", 0, 1);
			root.MountRoot();

			_control.AssertFrame(0, 0, 4, 3);
			_control.AssertAvailableSize(width: 4, height: 4 - 1);
			_control.AssertHasNativeParent();
		}

		[Test]
		public void FrameAll()
		{
			var root = _control.WithPadding(new Thickness<Points>(1, 1, 2, 2));
			root.AssertDesiredSize("root control", 3, 3);
			root.MountRoot();

			_control.AssertFrame(1, 1, 2, 2);
			_control.AssertAvailableSize(width: 4 - 1 - 2, height: 4 - 1 - 2);
			_control.AssertHasNativeParent();
		}

		[Test]
		public void FrameVertically()
		{
			var root = _control.WithPadding(top: new Points(1), bottom: new Points(2));
			root.AssertDesiredSize("root control", 0, 3);
			root.MountRoot();

			_control.AssertHasNativeParent();
			_control.AssertFrame(0, 1, 4, 2);
			_control.AssertAvailableSize(width: 4, height: 4 - 1 - 2);
		}
	}
}