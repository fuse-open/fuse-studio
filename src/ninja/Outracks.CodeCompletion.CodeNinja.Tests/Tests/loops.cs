using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class loops : Test
	{
		[Test]
		public void loops00()
		{
			AssertCode(
@"class b
{
	public void Foo()
	{
		for(int i=0;$(i)
	}
}

"
			);
		}

		/*[Test]
		public void loops01()
		{
			AssertCode(
@"

class b
{
	public void Foo()
	{
		for(int i=0;$(!if,!while,!var)
	}
}

"
			);
		}*/

		/*[Test]
		public void loops02()
		{
			AssertCode(
@"

class b
{
	public void Foo()
	{
		for(int i=0;$(!int)
	}
}

"
			);
		}*/

		[Test]
		public void loops03()
		{
			AssertCode(
@"


class b
{
	public void Foo()
	{
		do
		{
		// TODO: also check that it does _not_ suggest stuff like if/try/else
		}$(while)
	}
}

"
			);
		}

		[Test]
		public void loops04()
		{
			AssertCode(
@"

class b
{
	public void Foo()
	{
		$(foreach)
	}
}

"
			);
		}

		[Test]
		public void loops05()
		{
			AssertCode(
@"

class b
{
	public void Foo()
	{
		while($(true, false)
	}
}"
			);
		}

	}
}