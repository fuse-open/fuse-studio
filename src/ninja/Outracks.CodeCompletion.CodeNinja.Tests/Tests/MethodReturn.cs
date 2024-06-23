using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class MethodReturn : Test
    {
        [Test]
        public void MethodReturn01()
        {
            AssertCode(
@"
class Bar
{
    Bar GetBar()
    {
        return this;
    }

    void Foo()
    {
        GetBar().$(Foo)
    }
}
");
        }

        [Test]
        public void MethodReturn02()
        {
            AssertCode(
@"
class Bar
{
    void Foo()
    {
        GetBar().$(Foo)
    }

    Bar GetBar()
    {
        return this;
    }
}
");
        }
    }
}
