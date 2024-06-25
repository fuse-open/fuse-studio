using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class Jonny1 : Test
	{
		[Test]
		public void Jonny100()
		{
			AssertCode(
@"class Foo
{
    int a = 1;
    void Bar()
    {
        $(Uno)
    }
}

"
			);
		}

		[Test]
		public void Jonny101()
		{
			AssertCode(
@"class Foo
{
    float2 a;
    void Bar()
    {
        if(a == float2(0,0)){
            $(Uno)
        }
    }
}"
			);
		}

	}
}