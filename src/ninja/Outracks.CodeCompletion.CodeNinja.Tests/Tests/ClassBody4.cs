using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class ClassBody4 : Test
	{
		[Test]
		public void ClassBody400()
		{
			AssertCode(
@"public class b
{
	public $(class)
}

"
			);
		}

		[Test]
		public void ClassBody401()
		{
			AssertCode(
@"

public class b
{
	private $(class)
}

"
			);
		}

		[Test]
		public void ClassBody402()
		{
			AssertCode(
@"

public class b
{
	private class c
	{
		$(b,c)
	}
}

"
			);
		}

		[Test]
		public void ClassBody403()
		{
			AssertCode(
@"

public class b
{
	private class c
	{
		public class d
		{
			$(b,c,d)
		}
	}
}
"
			);
		}

	}
}