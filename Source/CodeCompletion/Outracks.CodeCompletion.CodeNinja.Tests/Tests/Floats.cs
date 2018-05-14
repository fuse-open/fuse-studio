using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class Floats : Test
	{
		[Test]
		public void Floats00()
		{
			AssertCode(
@"public class b
{
	float3x3 berp = float3x3.$(Identity)
}

"
			);
		}

		[Test]
		public void Floats01()
		{
			AssertCode(
@"

public class b{float3x3 berp=float3x3.$(Identity)}

"
			);
		}

		[Test]
		public void Floats02()
		{
			AssertCode(
@"

public class b
{
	float4x4 berp = float4x4.$(Identity)
}

"
			);
		}

		[Test]
		public void Floats03()
		{
			AssertCode(
@"

public class b
{
	float4x4 berp = Uno.Float4x4.$(Identity)
}

"
			);
		}

		[Test]
		public void Floats04()
		{
			AssertCode(
@"

public class b
{
	public void Verrdpp()
	{
		float3x3 berp = float3x3.$(Identity)
	}
}

"
			);
		}

		[Test]
		public void Floats05()
		{
			AssertCode(
@"

public class b
{
	public void Verrdpp()
	{
		float4x4 berp = float4x4.$(Identity)
	}
}

"
			);
		}

		[Test]
		public void Floats06()
		{
			AssertCode(
@"

public class b
{
	public void c()
	{
		float4x4 herp = float4x4.Identity;
		herp.$(M11,M12,M13,M14,M21,M22,M23,M24,M31,M32,M33,M34,M41,M42,M43,M44)
	}
}

"
			);
		}

		[Test]
		public void Floats07()
		{
			AssertCode(
@"
public class b
{
	public void c()
	{
		float3x3 derp = float3x3.Identity;
		float herk = derp.$(M11,M12,M13,M21,M22,M23,M31,M32,M33)
	}
}

"
			);
		}

		[Test]
		public void Floats08()
		{
			AssertCode(
@"

public class b
{
	public void c()
	{
		float3 der = float3(1.0f);
		der.$(X,Y,Z)
	}
}

"
			);
		}

		[Test]
		public void Floats09()
		{
			AssertCode(
@"

class b
{
	b()
	{
		float2 temp2 = float2(1.0f);
		float4x4 test = float4x4(float4($(temp2)
	}
}

"
			);
		}

		[Test]
		public void Floats10()
		{
			AssertCode(
@"

class b
{
	b()
	{
		float2 temp2 = float2(1.0f);
		float4x4 test = float4x4(float4(temp2.YX,$(temp2)
	}
}

"
			);
		}

		[Test]
		public void Floats11()
		{
			AssertCode(
@"

class b
{
	b()
	{
		float2 temp2 = float2(1.0f);
		float4x4 test = float4x4(float4(float4x4( float4(1.0f),float4(1.0f),float4(1.0f),float4(1.0f) ).$(M11,M12,M13,M14)
	}
}

"
			);
		}

	}
}