using NUnit.Framework;

namespace Outracks.Fusion.Tests
{
	[TestFixture]
	public class ScissorTest 
	{
		readonly TestControl _d = new TestControl("scissored control", 3, 3);

		[Test]
		public void ScissorLeft()
		{
			var cut = _d.ScissorLeft(1);
			cut.AssertDesiredSize("cut", 3 - 1, 3);

			cut.CenterHorizontally().MountRoot();

			_d.AssertFrame(left: 0, top: 0, right: 3, bottom: 4);
			_d.AssertHasNativeParent();
		}

		[Test]
		public void ScissorRight()
		{
			var cut = _d.ScissorRight(1);
			cut.AssertDesiredSize("cut", 3 - 1, 3);

			cut.CenterHorizontally().MountRoot();

			_d.AssertFrame(left: 1, top: 0, right: 4, bottom: 4);
			_d.AssertHasNativeParent();
		}

		[Test]
		public void ScissorTop()
		{
			var cut = _d.ScissorTop(1);
			cut.AssertDesiredSize("cut", 3, 3 - 1);

			cut.CenterVertically().MountRoot();

			_d.AssertFrame(left: 0, top: 0, right: 4, bottom: 3);
			_d.AssertHasNativeParent();
		}

		[Test]
		public void ScissorBottom()
		{
			var cut = _d.ScissorBottom(1);
			cut.AssertDesiredSize("cut", 3, 3 - 1);

			cut.CenterVertically().MountRoot();

			_d.AssertFrame(left: 0, top: 1, right: 4, bottom: 4);
			_d.AssertHasNativeParent();
		}
	}
}
