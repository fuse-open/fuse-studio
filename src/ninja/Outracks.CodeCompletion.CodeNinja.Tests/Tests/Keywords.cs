using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class Keywords : Test
    {
        [Test]
        public void Keywords00()
        {
            AssertCode(
@"

class Foo: Application
{
	float Bar: Sin(0.5f);

	public override void Draw()
	{
		draw this, Cube,
		{
			apply DefaultShading;
			float2 FragCoord: float2(pixel ClipPosition.X, pixel ClipPosition.Y);
			FragCoord: $(prev)
		};
	}
}
");
        }
    }
}
