using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class PrivateMembers : Test
	{
		[Test]
		public void PrivateMembers00()
		{
			AssertCode(
@"public class A
{
	int p;

	public void F() { $(p) }
}

"
			);
		}

		[Test]
		public void PrivateMembers01()
		{
			AssertCode(
@"


public class A
{
	int p { get { return 1; } }

	public void F() { $(p) }
}

"
			);
		}

		[Test]
		public void PrivateMembers02()
		{
			AssertCode(
@"


public class A
{
	int p() { return 1; }

	public void F() { $(p) }
}

"
			);
		}

		[Test]
		public void PrivateMembers03()
		{
			AssertCode(
@"


public class A
{
	protected int p;
}


public class Baaa: Uno.Application
{
	public void F() { $(Draw) }
}


"
			);
		}

		[Test]
		public void PrivateMembers04()
		{
			AssertCode(
@"

public class A
{
	private int k;
	protected int p;
}


public class Baaa: A
{
	public void F() { $(p, !k) }
}

"
			);
		}

		[Test]
		public void PrivateMembers05()
		{
			AssertCode(
@"

using Uno.Graphics;

class Bar
{
	Texture2D t = new Texture2D();

	public int k;

	public Bar()
	{
		$(k, t)
	}

	public void Foo()
	{
	}
}"
			);
		}

	}
}