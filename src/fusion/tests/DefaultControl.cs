using NUnit.Framework;

namespace Outracks.Fusion.Tests
{
	[TestFixture]
	public class DefaultControl
	{
		[Test]
		public void ShouldDesireZeroSize()
		{
			var root = Control.Create(_ => null);
			root.AssertDesiredSize("Default control", 0, 0);
		}
	}
}