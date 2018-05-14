using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class Numbers : Test
	{
		[Test]
		public void Numbers00()
		{
			AssertCode(
@"class Foo
{
	void Bar()
	{
		1.$(!ToString, !GetHashCode)
	}
}"
			);
		}

	}
}