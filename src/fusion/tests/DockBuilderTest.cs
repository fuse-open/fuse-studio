using NUnit.Framework;

namespace Outracks.Fusion.Tests
{
	[TestFixture]
	public class DockBuilderTest
	{
		readonly TestControl _d = new TestControl("docked control", 1, 1);
		readonly TestControl _r = new TestControl("right control", 2, 2);
		readonly TestControl _f = new TestControl("filled control", 4, 4);

		[Test]
		public void BuildHorizontally()
		{
			var root = Layout
				.Dock()
				.Left(_d)
				.Right(_r)
				.Fill(_f);

			root.AssertDesiredSize("dock", 7, 4);
			root.MountRoot();

			_d.AssertFrame(left: 0, top: 0, right: 1, bottom: 4);
			_d.AssertAvailableSize(width: 4, height: 4);
			_d.AssertHasNativeParent();

			_r.AssertFrame(left: 2, top: 0, right: 4, bottom: 4);
			_r.AssertAvailableSize(width: 4 - 1, height: 4);
			_r.AssertHasNativeParent();

			_f.AssertFrame(left: 1, top: 0, right: 2, bottom: 4);
			_f.AssertAvailableSize(width: 4 - 1 - 2, height: 4);
			_f.AssertHasNativeParent();
		}

		[Test]
		public void BuildVertically()
		{
			var root = Layout
				.Dock()
				.Top(_d)
				.Bottom(_r)
				.Fill(_f);

			root.AssertDesiredSize("dock", 4, 7);
			root.MountRoot();

			_d.AssertFrame(left: 0, top: 0, right: 4, bottom: 1);
			_d.AssertAvailableSize(width: 4, height: 4);
			_d.AssertHasNativeParent();

			_r.AssertFrame(left: 0, top: 2, right: 4, bottom: 4);
			_r.AssertAvailableSize(width: 4, height: 4 - 1);
			_r.AssertHasNativeParent();

			_f.AssertFrame(left: 0, top: 1, right: 4, bottom: 2);
			_f.AssertAvailableSize(width: 4, height: 4 - 1 - 2);
			_f.AssertHasNativeParent();
		}
	}
}