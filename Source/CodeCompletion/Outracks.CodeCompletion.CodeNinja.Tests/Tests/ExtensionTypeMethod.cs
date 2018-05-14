using NUnit.Framework;
namespace Outracks.CodeNinja.Tests.Tests
{
    public class ExtensionTypeMethod : Test
    {
		[Test]
        public void ExtensionTypeMethod00()
        {
            AssertCode(
@"
namespace UnoExtensions
{
    public static class StringExtensions
    {
        public static string Hello(this string self)
        {
            return ""
            Hello,
            "" + self;
        }

        public void Foo(this int self)
        {
        }
    }
}
namespace CompilerTests.General
{
    using UnoExtensions;

    class ExtensionMethods
    {
        public static void Run()
        {
            assert(""foo"".$(Hello, !Foo) == ""Hello, foo"");
        }
    }
            ");
        }

        [Test]
        public void ExtensionTypeMethod01()
        {
            AssertCode(
@"
    namespace Bar
    {
        class Git
        {
            void Foo($(this)
        }
    } ");
        }

        [Test]
        public void ExtensionTypeMethod02()
        {
            AssertCode(
@"
    namespace Bar
    {
        class Git
        {
            void Foo($(this)) {}
        }
    }
            ");
        }
    }
}
