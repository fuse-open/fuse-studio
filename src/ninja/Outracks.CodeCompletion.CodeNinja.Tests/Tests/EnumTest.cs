using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
	public class EnumTest : Test
	{
		[Test]
		public void EnumTest01()
		{
			AssertCode(@"
enum Test
{
	Foo
}

class Lol
{
	public void Bar()
	{
		Test test;
		test.$(HasFlag)
	}
}
");
		}
	}
}