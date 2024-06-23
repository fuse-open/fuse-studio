using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class iterator : Test
	{
		[Test]
		public void iterator00()
		{
			AssertCode(
@"public class derp
{
	public void derk()
	{
		for(int i = 0;$(i) i < 4;i++)
		{
		}
	}

}
"
			);
		}

	}
}