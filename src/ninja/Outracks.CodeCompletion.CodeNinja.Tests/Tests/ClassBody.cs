using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class ClassBody : Test
	{
		public void ClassBody00()
		{
			AssertCode(
@"using Uno;
using Fuse;

namespace StandardLib.UI
{
	class PointerEventTest
	{
		class SomeClass
		{
			public event $(PointerEventArgs)
		}
	}
}

"
			);
		}

		[Test]
		public void ClassBody01()
		{
			AssertCode(
@"

class b<T>
{
	public static int Tx;

	public void Foo()
	{
		b<T>.$(!Uno, Tx)
	}
}

"
			);
		}

		[Test]
		public void ClassBody02()
		{
			AssertCode(
@"

using Uno;
using Fuse.Drawing.Primitives;

namespace RenderingTests
{
	public static class Verifier
	{
		public static extern bool Verify(string testName);
	}

	class Main : Uno.Application
	{
		framebuffer f = new framebuffer(int2(128, 128), Uno.Graphics.$(Format)

		public override void Draw()
		{
			draw Quad
			{
				PixelColor : float4(1, 0, 0, 1);
			};

			Verifier.Verify(""foobar"");

		}
	}
}"
			);
		}

        [Test]
        public void ClassBody03()
        {
            AssertCode(
@"

class b<T>
{
	public static int Tx;

	public void Foo()
	{
		b<int>.Tx.$(ToString, !T)
	}
}

"
            );
        }

	}
}