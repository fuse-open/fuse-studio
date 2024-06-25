using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class SwizzlerTypes : Test
    {
        [Test]
        public void SwizzlerTypes01()
        {
            AssertCode(
@"
class Foo : Uno.Application
{
	public override void Draw()
	{
		draw this, Cube,
		{
			apply DefaultShading;
			float2 FragCoord: float2(pixel ClipPosition.X, pixel ClipPosition.Y);
			FragCoord: prev.$(XY, YX, !YZ)
		};
	}
}
");
        }

        [Test]
        public void SwizzlerTypes02()
        {
            AssertCode(
@"
class Foo
{
    public sbyte2 test;
	public void Test()
	{
        test.$(X, XX, !YZ)
	}
}
");
        }

        [Test]
        public void SwizzlerTypes03()
        {
            AssertCode(
@"
class Foo
{
    public ushort4 test;
	public void Test()
	{
        test.$(X, XX, YZW, WWWW)
	}
}
");
        }
    }
}
