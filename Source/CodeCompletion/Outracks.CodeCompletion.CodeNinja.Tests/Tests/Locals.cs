using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class Locals : Test
	{
		[Test]
		public void Locals00()
		{
			AssertCode(
@"public class A
{
	public void F()
	{
		int a;
		$(a)
	}
}

"
			);
		}

		[Test]
		public void Locals01()
		{
			AssertCode(
@"

public class A
{
	public void F()
	{
		if (1 == 2)
		{
			int a;
		}
		$(!a)
	}
}

"
			);
		}

		[Test]
		public void Locals02()
		{
			AssertCode(
@"

public class A
{
	public void F()
	{
		int a;
	}

	public bool K
	{
		get
		{
			$(!a)
		}
	}
}

"
			);
		}

		[Test]
		public void Locals03()
		{
			AssertCode(
@"

public class A
{
	public void F()
	{
		int a;
	}

	public bool K
	{
		get
		{
			int a;
			if (1 == 2)
			{
				$(a)
			}
		}
	}
}

"
			);
		}

	}
}