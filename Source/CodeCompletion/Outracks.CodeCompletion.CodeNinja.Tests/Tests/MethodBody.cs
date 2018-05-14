using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class MethodBody : Test
	{
		[Test]
		public void MethodBody00()
		{
			AssertCode(
@"public class A
{
	framebuffer f = new framebuffer(int2(100, 100), Uno.Graphics.Format.RGBA_8_8_8_8_UInt_Normalize, true);

	static int poop;
	
	public void F()
	{
		$(f,poop)
	}

	public static void G()
	{
	}
}

"
			);
		}

		[Test]
		public void MethodBody01()
		{
			AssertCode(
@"

public class A
{
	public void F()
	{
		int $(!Parse)
	}
}


"
			);
		}

		[Test]
		public void MethodBody02()
		{
			AssertCode(
@"

using Uno.Math;

class Foo
{
	void Bar()
	{
		Sin($(this)
	}
}


"
			);
		}

		[Test]
		public void MethodBody03()
		{
			AssertCode(
@"

class Foo
{
	void Bar()
	{
		int k = Sin(this.GetHashCode());
		if (k == k)
		{
			Uno.Content.Splines.Spline.$(!k, !Bar, !GetHashCode)
		}
	}
}

"
			);
		}

		[Test]
		public void MethodBody04()
		{
			AssertCode(
@"

class Foo
{
	void Bar()
	{
		1.0f$(!Equals, !MaxValue)
	}
}

"
			);
		}

		[Test]
		public void MethodBody05()
		{
			AssertCode(
@"

class Foo
{
	bool Bar()
	{
		var a = true;
		if (a) return !a;
		else $(a)
	}
}
"
			);
		}

	}
}