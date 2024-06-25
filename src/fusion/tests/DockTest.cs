using NUnit.Framework;

namespace Outracks.Fusion.Tests
{
	[TestFixture]
	public class DockTest
	{
		readonly TestControl _d = new TestControl("docked control", 1, 1);
		readonly TestControl _r = new TestControl("right control", 2, 2);
		readonly TestControl _f = new TestControl("filled control", 4, 4);


		[Test]
		public void StackNone()
		{
			var a = Layout.StackFromLeft();
			a.AssertDesiredSize("a", 0, 0);
			a.MountRoot();

			var b = Layout.StackFromTop();
			b.AssertDesiredSize("b", 0, 0);
			b.MountRoot();
		}

		[Test]
		public void StackSingleFromLeft()
		{
			var dockLeft = Layout.StackFromLeft(_d);
			dockLeft.AssertDesiredSize("dock", 1, 1);
			dockLeft.MountRoot();

			_d.AssertFrame(left: 0, top: 0, right: 1, bottom: 4);
			_d.AssertAvailableSize(width: 4, height: 4);
			_d.AssertHasNativeParent();
		}

		[Test]
		public void StackMultipleFromLeft()
		{
			var dockLeft = Layout.StackFromLeft(_d, _r);
			dockLeft.AssertDesiredSize("dock", 3, 2);
			dockLeft.MountRoot();

			_d.AssertFrame(left: 0, top: 0, right: 1, bottom: 4);
			_d.AssertAvailableSize(width: 4, height: 4);
			_d.AssertHasNativeParent();

			_r.AssertFrame(left: 1, top: 0, right: 3, bottom: 4);
			//_r.AssertAvailableSize(width: 3, height: 4);
			_r.AssertHasNativeParent();
		}

		[Test]
		public void StackSingleFromTop()
		{
			var dockLeft = Layout.StackFromTop(_d);
			dockLeft.AssertDesiredSize("dock", 1, 1);
			dockLeft.MountRoot();

			_d.AssertFrame(left: 0, top: 0, right: 4, bottom: 1);
			_d.AssertAvailableSize(width: 4, height: 4);
			_d.AssertHasNativeParent();
		}

		[Test]
		public void StackMultipleFromTop()
		{
			var dockLeft = Layout.StackFromTop(_d, _r);
			dockLeft.AssertDesiredSize("dock", 2, 3);
			dockLeft.MountRoot();

			_d.AssertFrame(left: 0, top: 0, right: 4, bottom: 1);
			_d.AssertAvailableSize(width: 4, height: 4);
			_d.AssertHasNativeParent();

			_r.AssertFrame(left: 0, top: 1, right: 4, bottom: 3);
			//_r.AssertAvailableSize(width: 4, height: 3);
			_r.AssertHasNativeParent();
		}

		[Test]
		public void StackSingleFromBottom()
		{
			var dockLeft = Layout.StackFromBottom(_d);
			dockLeft.AssertDesiredSize("dock", 1, 1);
			dockLeft.MountRoot();

			_d.AssertFrame(left: 0, top: 3, right: 4, bottom: 4);
			_d.AssertAvailableSize(width: 4, height: 4);
			_d.AssertHasNativeParent();
		}

		[Test]
		public void StackMultipleFromBottom()
		{
			var dockLeft = Layout.StackFromBottom(_d, _r);
			dockLeft.AssertDesiredSize("dock", 2, 3);
			dockLeft.MountRoot();

			_d.AssertFrame(left: 0, top: 3, right: 4, bottom: 4);
			_d.AssertAvailableSize(width: 4, height: 4);
			_d.AssertHasNativeParent();

			_r.AssertFrame(left: 0, top: 1, right: 4, bottom: 3);
			//_r.AssertAvailableSize(width: 4, height: 3);
			_r.AssertHasNativeParent();
		}

		[Test]
		public void DockLeft()
		{
			var dockLeft = Layout.Dock(RectangleEdge.Left, _d, _f);
			dockLeft.AssertDesiredSize("dock", 5, 4);
			dockLeft.MountRoot();

			_d.AssertFrame(left: 0, top: 0, right: 1, bottom: 4);
			_d.AssertAvailableSize(width: 4, height: 4);
			_d.AssertHasNativeParent();

			_f.AssertFrame(left: 1, top: 0, right: 4, bottom: 4);
			_f.AssertAvailableSize(width: 4 - 1, height: 4);
			_f.AssertHasNativeParent();
		}

		[Test]
		public void DockLeftCollapseFill()
		{
			var dockLeft = Layout.Dock(RectangleEdge.Left, _d.WithWidth(100), _f);
			dockLeft.MountRoot();

			_f.AssertAvailableSize(width: 0, height: 4);
			_f.AssertFrame(left: 4, top: 0, right: 4, bottom: 4);
		}

		[Test]
		public void DockRight()
		{
			var dockRight = Layout.Dock(RectangleEdge.Right, _d, _f);
			dockRight.AssertDesiredSize("dock", 5, 4);
			dockRight.MountRoot();

			_d.AssertFrame(left: 3, top: 0, right: 4, bottom: 4);
			_d.AssertAvailableSize(width: 4, height: 4);
			_d.AssertHasNativeParent();

			_f.AssertFrame(left: 0, top: 0, right: 3, bottom: 4);
			_f.AssertAvailableSize(width: 4 - 1, height: 4);
			_f.AssertHasNativeParent();
		}

		[Test]
		public void DockRightCollapseFill()
		{
			var dockLeft = Layout.Dock(RectangleEdge.Right, _d.WithWidth(100), _f);
			dockLeft.MountRoot();

			_f.AssertAvailableSize(width: 0, height: 4);
			_f.AssertFrame(left: 0, top: 0, right: 0, bottom: 4);
		}

		[Test]
		public void DockTop()
		{
			var dockTop = Layout.Dock(RectangleEdge.Top, _d, _f);
			dockTop.AssertDesiredSize("dock", 4, 5);
			dockTop.MountRoot();

			_d.AssertFrame(left: 0, top: 0, right: 4, bottom: 1);
			_d.AssertAvailableSize(width: 4, height: 4);
			_d.AssertHasNativeParent();

			_f.AssertFrame(left: 0, top: 1, right: 4, bottom: 4);
			_f.AssertAvailableSize(width: 4, height: 4 - 1);
			_f.AssertHasNativeParent();
		}

		[Test]
		public void DockTopCollapseFill()
		{
			var dockLeft = Layout.Dock(RectangleEdge.Top, _d.WithHeight(100), _f);
			dockLeft.MountRoot();

			_f.AssertAvailableSize(width: 4, height: 0);
			_f.AssertFrame(left: 0, top: 4, right: 4, bottom: 4);
		}

		[Test]
		public void DockBottom()
		{
			var dockBottom = Layout.Dock(RectangleEdge.Bottom, _d, _f);
			dockBottom.AssertDesiredSize("dock", 4, 5);
			dockBottom.MountRoot();

			_d.AssertFrame(left: 0, top: 3, right: 4, bottom: 4);
			_d.AssertAvailableSize(width: 4, height: 4);
			_d.AssertHasNativeParent();

			_f.AssertFrame(left: 0, top: 0, right: 4, bottom: 3);
			_f.AssertAvailableSize(width: 4, height: 4 - 1);
			_f.AssertHasNativeParent();
		}

		[Test]
		public void DockBottomCollapseFill()
		{
			var dockLeft = Layout.Dock(RectangleEdge.Bottom, _d.WithHeight(100), _f);
			dockLeft.MountRoot();

			_f.AssertAvailableSize(width: 4, height: 0);
			_f.AssertFrame(left: 0, top: 0, right: 4, bottom: 0);
		}
	}
}
