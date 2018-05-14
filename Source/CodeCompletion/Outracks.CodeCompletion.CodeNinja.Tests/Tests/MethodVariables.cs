using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class MethodVariables : Test
	{
		[Test]
		public void MethodVariables00()
		{
			AssertCode(
@"public class b
{
	public void c(int derk)
	{
		$(derk)
	}
}

"
			);
		}

		[Test]
		public void MethodVariables01()
		{
			AssertCode(
@"
public class b
{
	public void c(int derk)
	{
		if($(derk)
	}
}

"
			);
		}

		[Test]
		public void MethodVariables02()
		{
			AssertCode(
@"
public class b
{
	public int c(int derk)
	{
		return $(derk)
	}
}

"
			);
		}

		[Test]
		public void MethodVariables03()
		{
			AssertCode(
@"
public class b
{
	public int c(int derk)
	{
			return derk;
	}
	public int d()
	{
		return $(!derk)
	}
}

"
			);
		}

		[Test]
		public void MethodVariables04()
		{
			AssertCode(
@"
public class b
{
	int e  = 5;
	public int c(int derk)
	{
			return derk;
	}
	public int d()
	{
		return $(e)
	}
}"
			);
		}

	}
}