using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class GetSet : Test
	{
		[Test]
		public void GetSet00()
		{
			AssertCode(
@"[Ignore(""Deadlock"")]
public class A
{
	public void F()
	{
		int a;
	}

	public bool K
	{
		$(get,set,!K,!Uno);
	}
}

"
			);
		}

		[Test]
		public void GetSet01()
		{
			AssertCode(
@"
[Ignore(""Deadlock"")]
public class A
{
	public void F()
	{
		int a;
	}

	public bool K
	{
		get { return true; }
		$(set,!K,!Uno);
	}
}

"
			);
		}

		[Test]
		public void GetSet02()
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
		set { }
		$(get,!K,!Uno);
	}
}


"
			);
		}

		[Test]
		public void GetSet03()
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
		get{ return true;}
		set { }
		$(!K,!Uno);
	}
}

"
			);
		}

		[Test]
		public void GetSet04()
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
		get{ return true;}
		set { $(value)}
	}
}"
			);
		}

	}
}