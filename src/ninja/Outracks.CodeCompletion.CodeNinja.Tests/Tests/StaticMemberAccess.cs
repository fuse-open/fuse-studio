using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class StaticMemberAccess : Test
	{
		[Test]
		public void StaticMemberAccess00()
		{
			AssertCode(
@"class Foo
{
	void Bar()
	{
		Uno.Math.$(Log, !Foobar)
	}
}

"
			);
		}

		[Test]
		public void StaticMemberAccess01()
		{
			AssertCode(
@"

using Uno;

class Foo
{
	void Bar()
	{
		Math.$(Log, !Foobar)
	}
}"
			);
		}
	}
}