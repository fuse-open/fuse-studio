using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class ClassBody2 : Test
	{
		[Test]
		public void ClassBody200()
		{
			AssertCode(
@"﻿public class A : Uno.Application
{
	public void F() {}
}

public class B
{
	public void C()
	{
		$(!F)
	}
}

"
			);
		}

		[Test]
		public void ClassBody201()
		{
			AssertCode(
@"

public class A  : Uno.Application
{
	public void F() {}
}

public class B
{
	$(!F)
}

"
			);
		}

		[Test]
		public void ClassBody202()
		{
			AssertCode(
@"

public class A : Uno.Application
{
	public framebuffer f = new framebuffer(int2(100, 100), Uno.Graphics.Format.RGBA_8_8_8_8_UInt_Normalize, true);

	public void F()
	{
		this.F().$(!f)
	}
}

"
			);
		}

		[Test]
		public void ClassBody203()
		{
			AssertCode(
@"
public class a : Uno.Application{}

public class C<T>: Uno.Collections.List<T>
{
	$(Uno)
}"
			);
		}

	}
}