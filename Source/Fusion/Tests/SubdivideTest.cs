using NUnit.Framework;

namespace Outracks.Fusion.Tests
{
	[TestFixture]
	public class SubdivideTest 
	{
		readonly TestControl _a = new TestControl("control a", 1, 1);
		readonly TestControl _b = new TestControl("control b", 2, 2);
		readonly TestControl _c = new TestControl("control c", 3, 3);

		[Test]
		public void SubdivideHorizontally()
		{
			var root = Layout.Subdivide(Axis2D.Horizontal, _a.Span(1), _b.Span(2), _c.Span(1));
			root.AssertDesiredSize("root control", 0, 3);
			root.MountRoot();

			_a.AssertFrame(left: 0, top: 0, right: 1, bottom: 4);
			_a.AssertAvailableSize(1, 4);
			_a.AssertHasNativeParent();

			_b.AssertFrame(left: 1, top: 0, right: 3, bottom: 4);
			_b.AssertAvailableSize(2, 4);
			_b.AssertHasNativeParent();

			_c.AssertFrame(left: 3, top: 0, right: 4, bottom: 4);
			_c.AssertAvailableSize(1, 4);
			_c.AssertHasNativeParent();
		}

		[Test]
		public void SubdivideVertically()
		{
			var root = Layout.Subdivide(Axis2D.Vertical, _a.Span(1), _b.Span(2), _c.Span(1));
			root.AssertDesiredSize("root control", 3, 0);
			root.MountRoot();

			_a.AssertFrame(left: 0, top: 0, right: 4, bottom: 1);
			_a.AssertAvailableSize(4, 1);
			_a.AssertHasNativeParent();

			_b.AssertFrame(left: 0, top: 1, right: 4, bottom: 3);
			_b.AssertAvailableSize(4, 2);
			_b.AssertHasNativeParent();

			_c.AssertFrame(left: 0, top: 3, right: 4, bottom: 4);
			_c.AssertAvailableSize(4, 1);
			_c.AssertHasNativeParent();
		}

		[Test]
		public void Subdivide_SnapsToPoints()
		{
			var root = Layout.Subdivide(Axis2D.Horizontal, _a.Span(1.25), _b.Span(1.75), _c.Span(1)).WithWidth(8);
			root.AssertDesiredSize("root control", 8, 3);
			root.MountRoot();

			_a.AssertFrame(left: 0, top: 0, right: 1, bottom: 4);
			_a.AssertAvailableSize(1.25, 4);
			_a.AssertHasNativeParent();

			_b.AssertFrame(left: 1, top: 0, right: 3, bottom: 4);
			_b.AssertAvailableSize(1.75, 4);
			_b.AssertHasNativeParent();

			_c.AssertFrame(left: 3, top: 0, right: 4, bottom: 4);
			_c.AssertAvailableSize(1, 4);
			_c.AssertHasNativeParent();
		}
	}
}