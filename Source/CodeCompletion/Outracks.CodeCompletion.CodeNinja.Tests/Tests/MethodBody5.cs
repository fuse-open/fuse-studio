using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class MethodBody5 : Test
	{
		[Test]
		public void MethodBody500()
		{
			AssertCode(
@"static class ArrayTest
{
	public static void Run()
	{
		int[] foo = new [] { 2, 1, 3 };
		
		$(foo, Uno)		
	}
}

"
			);
		}

		[Test]
		public void MethodBody501()
		{
			AssertCode(
@"

namespace Uno
{
	public class Foo
	{
		public void Sort(Comparison<T> comparer)
		{
			$(Uno)
			//Array.(data, comparer, 0, used);
		}
	}
}


"
			);
		}

		/*[Test]
		public void MethodBody502()
		{
			AssertCode(
@"

public class Foo: Uno.Application
{
	public Foo()
	{
		$(Clear)
	}
}

"
			);
		}*/

		[Test]
		public void MethodBody503()
		{
			AssertCode(
@"

namespace dfhagds
{
    class App : Uno.Application
    {
        public override void Draw()
        {
			
			$(App)

        }
    }
}
"
            );
		}

	}
}