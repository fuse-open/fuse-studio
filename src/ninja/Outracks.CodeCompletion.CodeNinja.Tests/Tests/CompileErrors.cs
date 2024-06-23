using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class CompileErrors : Test
    {
        [Test]
        public void CompileErrors01()
        {
            AssertCode(@"
class Foo
{
    int fieldTest;
    void Draw()
    {
        int lol = $(fieldTest)
    }
}
            ");
        }

        [Test]
        public void CompileErrors02()
        {
            AssertCode(@"
class Foo
{
    void Draw()
    {
        int lol = $(fieldTest)
    }
    int fieldTest;
}
            ");
        }

        [Test]
        public void CompileErrors03()
        {
            AssertCode(@"
class Foo
{
    void Draw()
    {
        bool foo = true;
        int lol = $(fieldTest)
    }
    int fieldTest;
}
            ");
        }

        [Test]
        public void CompileErrors04()
        {
            AssertCode(@"
class Foo
{
    void Draw()
    {
        draw
        {
            $(fieldTest)
        }
    }
    int fieldTest;
}
            ");
        }
    }
}
