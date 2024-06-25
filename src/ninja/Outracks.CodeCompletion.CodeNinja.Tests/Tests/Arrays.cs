using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class Arrays : Test
	{
		[Test]
		public void Arrays00()
		{
			AssertCode(
@"class Foo: Uno.Application
{
	public Foo()
	{
		var k = new int[100];
		k[0].$(Equals, !Length);
	}
}

"
			);
		}

		[Test]
		public void Arrays01()
		{
			AssertCode(
@"

class Foo: Uno.Application
{
	public Foo()
	{
		var k = new int[100];
		k.$(Equals, Length);
	}
}

"
			);
		}

		[Test]
		public void Arrays02()
		{
			AssertCode(
@"

class b
{
	public int somenum;
	private int anothernum;
}
class c
{
	c()
	{
		var arr = new b[100];
		arr[0].$(somenum,!anothernum)
	}
}

"
			);
		}

		[Test]
		public void Arrays03()
		{
			AssertCode(
@"

class b
{
	int numA;
	static int numC;
	class c
	{
		int numB;
		class d
		{
			$(!numA,!numB,numC)
		}
	}
}

"
			);
		}

		[Test]
		public void Arrays04()
		{
			AssertCode(
@"

class b
{
	int numA;
	static int numC;
	class c
	{
		int numB;
		class d
		{
			void Foo()
			{
				var k = new b();

				k.$(numA,!numB,!numC)
			}
		}
	}
}

"
			);
		}

	}
}