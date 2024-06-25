using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class Event : Test
    {
        [Test]
        public void Event01()
        {
            AssertCode(
@"
class Foo
{
    public event Uno.Action Event
    {
        add {
        }

        remove {
        }
    }

    public float2 test;

    public void Draw()
    {
       $(test)
    }
}
"
                );
        }

        [Test]
        public void Event02()
        {
            AssertCode(
@"
class Foo
{
    public event Uno.Action Event;

    public float2 test;

    public void Draw()
    {
       $(test)
    }
}
"
                );
        }

        [Test]
        public void Event03()
        {
            AssertCode(
@"
class Foo
{
    public float2 test;

    public event Uno.Action Event
    {
        add {
            $(test)
        }

        remove {
        }
    }

    public void Draw()
    {
    }
}
"
                );
        }
    }
}
