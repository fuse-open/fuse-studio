using NUnit.Framework;

namespace Outracks.Fusion.Tests
{
	[TestFixture]
	public class CenterTest
	{
		readonly TestControl _control = new TestControl("centered control", 2, 2);
		[Test]
		public void CenterHorizontally()
		{
			var root = _control.CenterHorizontally();
			root.AssertDesiredSize("root control", 2, 2);
			root.MountRoot();

			_control.AssertFrame(left: 1, top: 0, right: 3, bottom: 4);
			_control.AssertHasNativeParent();
		}

		[Test]
		public void CenterVertically()
		{
			var root = _control.CenterVertically();
			root.AssertDesiredSize("root control", 2, 2);
			root.MountRoot();

			_control.AssertFrame(left: 0, top: 1, right: 4, bottom: 3);
			_control.AssertHasNativeParent();
		}

		[Test]
		public void Center()
		{
			var root = _control.Center();
			root.AssertDesiredSize("root control", 2, 2);
			root.MountRoot();

			_control.AssertFrame(left: 1, top: 1, right: 3, bottom: 3);
			_control.AssertHasNativeParent();
		}

		[Test]
		public void CenterHorizontallyCollapseFill()
		{
			var root = _control.WithWidth(10).Center(/*fillMin: _min, fillMax: _max,*/ axis: Axis2D.Horizontal);
			root.MountRoot();

			/*
			_min.AssertAvailableSize(0, 4);
			_min.AssertFrame(0-3, 0, 0-3, 4);

			_max.AssertAvailableSize(0, 4);
			_max.AssertFrame(4+3, 0, 4+3, 4);
			*/

			_control.AssertAvailableSize(4, 4);
			_control.AssertFrame(0 - 3, 0, 4 + 3, 4);
		}


		[Test]
		public void CenterVerticallyCollapseFill()
		{
			var root = _control.WithHeight(10).Center(/*fillMin: _min, fillMax: _max,*/ axis: Axis2D.Vertical);
			root.MountRoot();

			/*
			_min.AssertAvailableSize(4, 0);
			_min.AssertFrame(0, 0-3, 4, 0-3);

			_max.AssertAvailableSize(4, 0);
			_max.AssertFrame(0, 4+3, 4, 4+3);
			*/

			_control.AssertAvailableSize(4, 4);
			_control.AssertFrame(0, 0 - 3, 4, 4 + 3);
		}

	}
}