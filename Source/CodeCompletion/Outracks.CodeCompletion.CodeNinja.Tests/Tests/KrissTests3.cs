using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class KrissTests3 : Test
	{
		[Test]
		public void KrissTests300()
		{
			AssertCode(
@"

using Uno.Collections;

class A
{
    public A()
    {
        List<int> a = new $(!<root>)
    }
}

"
			);
		}

		[Test]
		public void KrissTests301()
		{
			AssertCode(
@"

class A
{
    public A()
    {
        $(!<root>)
    }
}

"
			);
		}

		[Test]
		public void KrissTests302()
		{
			AssertCode(
@"

class A
{
	$(!<root>)
	
    public A()
    {
        
    }
}"
			);
		}

	}
}