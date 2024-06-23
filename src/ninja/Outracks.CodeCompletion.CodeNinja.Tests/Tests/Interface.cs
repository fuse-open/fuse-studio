using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class Interface : Test
    {
        [Test]
        public void Interface01()
        {
            AssertCode(
@"
interface IRolf
{
    IRolf Rolf { get; }
}

class Bar
{
    IRolf IRolf.Rolf
    {
        get
        {
            return null;
        }
    }

    public Bar()
    {
        $(Test)
    }

    void Test()
    {
    }
}
"
                );
        }
    }
}
