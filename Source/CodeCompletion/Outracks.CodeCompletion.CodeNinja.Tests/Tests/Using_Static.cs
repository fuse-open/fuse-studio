using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class Using_Static : Test
	{
		[Test]
		public void Using_Static00()
		{
			AssertCode(
@"using Uno;
using $(Math, Matrix, Vector);

"
			);
		}

		[Test]
        public void Using_Static01()
        {
            AssertCode(
@"

using Uno;
using $(Uno, !Math)Uno.Math;
using Matrix;

class Foo
{
	void Bar()
	{
		Math.A$(Abs);
	}
}"
            );
        }

		[Test]
		public void Using_Static02()
		{
			AssertCode(
@"

using Uno;
using $(Uno, !Math)Uno.Math;
using Matrix;

class Foo
{
	void Bar()
	{
		$(Abs, Sin, Sqrt, PI, Transpose);
	}
}"
			);
		}

        [Test]
        public void Using_Static03()
        {
            AssertCode(
@"

using Uno;
using $(Uno, !Math)Uno.Math;
using $(Foo)Foo;

class Foo
{
    public static int i;
	void Bar()
	{
	}
}

class Test
{
    Test()
    {
        ++i;
    }
}
"
            );
        }

	}
}