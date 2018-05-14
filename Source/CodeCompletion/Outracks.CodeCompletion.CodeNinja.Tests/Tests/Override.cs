using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class Override : Test
	{
		[Test]
		public void Override00()
		{
			AssertCode(
@"class A
{
	public void B() {}
	public virtual void C() {}

	public float D { get { return 0.0f; } set { } }
	public abstract float E { get { return 0.0f; } set { } }

}

class G: A
{
	override $(!B, C, E, !D)
}"
			);
		}

	}
}