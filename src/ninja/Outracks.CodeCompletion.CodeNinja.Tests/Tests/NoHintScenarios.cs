using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class NoHintScenarios : Test
	{
		[Test]
		public void NoHintScenarios00()
		{
			AssertCode(
@"class Bar
{
    string Foo()
    {
        return ""fooo $(!Uno, !using, !public)
    }
}

"
			);
		}

		[Test]
		public void NoHintScenarios01()
		{
			AssertCode(
@"

class Bar
{
    string Foo()
    {
        return ""$(!Uno, !using, !public)
    }
}

"
			);
		}

		[Test]
		public void NoHintScenarios02()
		{
			AssertCode(
@"

class Bar
{
    string Foo()
    {
        return ""asd"".$(Equals)
    }
}

"
			);
		}

		[Test]
		public void NoHintScenarios03()
		{
			AssertCode(
@"

class Bar
{
    string Foo()
    {
        return // $(!Uno)
    }
}

"
			);
		}

		[Test]
		public void NoHintScenarios04()
		{
			AssertCode(
@"

class Bar
{
    string Foo()
    {
        /*

            return $(!Uno)

        */
    }
}

"
			);
		}

		[Test]
		public void NoHintScenarios05()
		{
			AssertCode(
@"


public class A
{
	public void F()
	{
		int b;
		int a = new int();

		// asd
        $(Uno);

	}
}

"
			);
		}

		[Test]
		public void NoHintScenarios06()
		{
			AssertCode(
@"

using Uno;
using Uno.Collections;
using Uno.Graphics;
using Uno.Audio;
using Uno.Content;
using Uno.Content.Models;

namespace BaseTest
{
	class A
	{
		public A(string lol)
		{
		}
	}

	class B
	{
		public B()
			: $(!Uno, !int, !BaseStructure, !BaseVertex)
		{
		}
	}

    class App : Uno.Application
    {
        public override void Draw()
        {
        }
    }
}

"
            );
		}

		[Test]
		public void NoHintScenarios07()
		{
			AssertCode(
@"

using Uno;
using Uno.Collections;
using Uno.Graphics;
using Uno.Audio;
using Uno.Content;
using Uno.Content.Models;

namespace BaseTest
{
	class A
	{
		public A(string lol)
		{
		}
	}

	class B
	{
		public B()
			: b$(!Uno, !int, !BaseStructure, !BaseVertex)
		{
		}
	}

    class App : Uno.Application
    {
        public override void Draw()
        {
        }
    }
}"
            );
		}

	}
}