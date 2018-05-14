using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class NewTest : Test
    {
		[Test]
        public void NewTest00()
        {
            AssertCode(
@"
using Uno.Collections;

class A
{
    public A()
    {
        List<int> a = new $(List<int>)
    }
}

"
            );
        }

        [Test]
        public void NewTest01()
        {
            AssertCode(
@"

enum A
{
    A, B
}

class B
{
    A a = $(A)
}"
            );
        }

		[Test]
        public void NewTest03()
        {
            AssertCode(
@"
    using Uno.Collections;

    class Foo
    {
        List<int> bar;
        void foo()
        {
            bar = new $(List<int>)
        }
    
");

        }

        [Test]
        public void NewTest04()
        {
            AssertCode(
@"

    class Foo
    {
        Uno.Application bar;
        void foo()
        {
            bar = new $(Uno.Application)
        }
");

        }

        [Test]
        public void NewTest05()
        {
            AssertCode(
@"

class A
{
    public A()
    {
        Uno.Collections.List<int> a = new $(Uno.Collections.List<int>)
    }
}

"
            );
        }

        [Test]
        public void NewTest06()
        {
            AssertCode(
@"
using Uno;

class A
{
    Uno.Collections.List<int> a;
    public A()
    {
        a = new $(!Collections.List<int>)
    }
}

"
            );
        }
    }
}