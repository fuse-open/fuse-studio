using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class MethodBody4 : Test
	{
		[Test]
		public void MethodBody400()
		{
			AssertCode(
@"public class A
{
	public void F() 
	{
		int a;
		int p = $(Uno, int, a)
	}
}


"
			);
		}

		[Test]
		public void MethodBody401()
		{
			AssertCode(
@"

public class A
{
	public void F() 
	{
		int b;
		int $(!ToString, !GetHashCode) 
	}
}

"
			);
		}

		[Test]
		public void MethodBody402()
		{
			AssertCode(
@"

public class A
{
	public void F() 
	{
		A a = new $(A, Uno)
	}
}

"
			);
		}

		[Test]
		public void MethodBody403()
		{
			AssertCode(
@"

public class A
{
	framebuffer f = new framebuffer(int2(100, 100), Uno.Graphics.Format.RGBA_8_8_8_8_UInt_Normalize, true);

	public void F()
	{
		$(f)
	}
}

"
			);
		}

		[Test]
		public void MethodBody404()
		{
			AssertCode(
@"

public class A
{
	framebuffer f = new framebuffer(int2(100, 100), Uno.Graphics.Format.RGBA_8_8_8_8_UInt_Normalize, true);

	static int poop;
	
	public void F()
	{
		
	}

	public static void G()
	{
		$(poop,!f)
	}
}"
			);
		}

	}
}