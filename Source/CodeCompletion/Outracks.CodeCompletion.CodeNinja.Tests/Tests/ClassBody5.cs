using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class ClassBody5 : Test
	{
		[Test]
		public void ClassBody500()
		{
			AssertCode(
@"public class b
{
	public class c
	{
		$(b,c)
	}
}

"
			);
		}

		[Test]
		public void ClassBody501()
		{
			AssertCode(
@"

public class b
{
	public class c
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

		[Test]
		public void ClassBody502()
		{
			AssertCode(
@"

public class b
{
	public class c
	{
		
	}
}
public class d
{
	$(!c)
}

"
			);
		}

		[Test]
		public void ClassBody503()
		{
			AssertCode(
@"

public class b
{
	public class c
	{
		
	}
}
public class d
{
	public class e : $(b,d,!e)
	{
	}
}
"
			);
		}

	}
}