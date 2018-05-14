using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class ClassBody6 : Test
	{
		[Test]
		public void ClassBody600()
		{
			AssertCode(
@"public class b
{
	public class c
	{
		
	}
}
public class d
{
	public class e : b.$(c, !d, !e)
	{
	}
}

"
			);
		}

		[Test]
		public void ClassBody601()
		{
			AssertCode(
@"

class b
{
	public int abc;
	private int efg;
	protected int hij;
}
class c : b
{
	c()
	{
		$(abc,hij,!efg)
	}
	
}

"
			);
		}

		[Test]
		public void ClassBody602()
		{
			AssertCode(
@"
class b
{
	public int abc;
	private int efg;
	protected int hij;
}
class c
{
	b test;
	c()
	{
		 test.$(abc,!efg,!hij)
	}
}

"
			);
		}

		[Test]
		public void ClassBody603()
		{
			AssertCode(
@"

class b
{
	p$(private,public,protected)
}
"
			);
		}

	}
}