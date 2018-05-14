using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class MetaProperties : Test
	{
		public void MetaProperties00()
		{
			AssertCode(
@"[Ignore]
class Foo: Uno.Application
{
	apply Fuse.Entities.DefaultShading;
	float Bar: 1.0f;

	public override void Draw()
	{
		draw this
		{
			int Foo: $(Diffuse)
		}
	}
}

"
			);
		}

		public void MetaProperties01()
		{
			AssertCode(
@"
[Ignore]
class Foo: Uno.Application
{
	apply Fuse.Entities.DefaultShading;
	float Bar: 1.0f;

	public override void Draw()
	{
		draw
		{
			int Foo: 1+1, $(Diffuse)
		}
	}
}

"
			);
		}

		public void MetaProperties02()
		{
			AssertCode(
@"
[Ignore]
class Foo: Uno.Application
{
	apply Fuse.Entities.DefaultShading;
	float Bar: 1.0f;

	int Foo: 1+1, $(Diffuse)

	public override void Draw()
	{
		draw
		{
			
		}
	}
}

"
			);
		}

		public void MetaProperties03()
		{
			AssertCode(
@"
class Foo: Uno.Application
{
	float Bar: 1.0f;

	block baraaa
	{
        apply Fuse.Entities.DefaultShading;
		int Foo: 1+1, $(Diffuse);
	}

	public override void Draw()
	{
	}
}

"
            );
		}

		public void MetaProperties04()
		{
			AssertCode(
@"

class Foo: Uno.Application
{
	apply Fuse.Entities.DefaultShading;
	float Bar: 1.0f;

	public override void Draw()
	{
		draw this
		{
			$(AmbientLightColor)
		}
	}
}

"
			);
		}

		public void MetaProperties05()
		{
			AssertCode(
@"
[Ignore]
class Foo: Uno.Application
{
	apply Fuse.Entities.DefaultShading;
	float Bar: 1.0f;

	public override void Draw()
	{
		draw this
		{
			float foo: AmbientLightColor.$(X, !Diffuse)
		}
	}
}

"
			);
		}

		public void MetaProperties06()
		{
			AssertCode(
@"
[Ignore]
class Foo: Uno.Application
{
	apply Fuse.Entities.DefaultShading;
	float Bar: 1.0f;

	public override void Draw()
	{
		draw this
		{
			float foo: 1, AmbientLightColor.$(X, !Diffuse)
		}
	}
}

"
			);
		}

		[Test]
		public void MetaProperties07()
		{
			AssertCode(
@"
[Ignore]
class Foo: Uno.Application
{
	apply Fuse.Entities.DefaultShading;
	float Bar: 1.0f;

	public override void Draw()
	{
		draw
		{
			float foo: { $(!apply)
		}
	}
}



"
			);
		}

        public void MetaProperties08()
        {
            AssertCode(
@"
class Foo: Fuse.Entities.Material
{
    apply Fuse.Entities.DefaultShading;
    PixelColor:
    {
        $(TexCoord)
    };
}



"
            );
        }

        public void MetaProperties09()
        {
            AssertCode(
@"
class Foo: Uno.Application
{
	float Bar: 1.0f;

	public override void Draw()
	{
		draw
		{
            apply Fuse.Entities.DefaultShading;
			PixelColor:
            {
                $(TexCoord)
            };
		}
	}
}
"
            );
        }

        [Test]
        public void MetaProperties10()
        {
            AssertCode(
@"
class Foo: Uno.Application
{
    float2 foo = float2(200, 0);
    float2 lol: float2(0, 0);
	public override Draw()
    {
        draw
        {
            PixelColor: this.$(foo)
        };
    }
}
"
            );
        }

        /*[Test]
        public void MetaProperties11()
        {
            AssertCode(
@"
class Foo: Uno.Application
{
    float3 test: float3(0);
	float3 Bar: prev.$(X, Item)
}
"
            );
        }

        [Test]
        public void MetaProperties12()
        {
            AssertCode(
@"
class Foo: Uno.Application
{
    float3 test: float3(0);
	float3 Bar: (prev+float3(0)).$(X, Item)
}
"
            );
        }*/

        [Test]
        public void MetaProperties13()
        {
            AssertCode(
@"
class Foo: Uno.Application
{
    float3 test: float3(0);
	float3 Bar: (prev PixelColor).$(X,Y,Z,W,!DefaultShading)
}
"
            );
        }

        public void MetaProperties14()
        {
            AssertCode(
@"

class Foo: Uno.Application
{
    apply Default$(DefaultShading);
}
"
            );
        }

        [Test]
        public void MetaProperties15()
        {
            AssertCode(
@"

class Foo: Uno.Application
{
    float2 Test: float2(0);
    $(Test)
}
"
            );
        }

        [Test]
        public void MetaProperties16()
        {
            AssertCode(
@"

class Foo: Uno.Application
{
    block Triangle
    {
        float2 Test: float2(0);
        $(Test)
    }
}
"
            );
        }

		[Test]
		public void MetaProperties17()
		{
			AssertCode(
@"

class Foo
{
	block Test
	{
		float3 foo: float3(0,0,0);
		float3 lol: req(foo) float3(0,1,1),
			foo.XYY;

		lol: Uno.Vector.$(Cross,!CullFace);
	}
}
"
			);
		}

		[Test]
		public void MetaProperties18()
		{
			AssertCode(
@"

class Foo
{
	block Test
	{
		float3 foo: float3(0,0,0);
		float3 lol: req(foo) float3(0,1,1),
			foo.XYY;

		lol: req(Cull$(CullFace)
	}
}
"
			);
		}
        /*[Test]
        public void MetaProperties08()
        {
            AssertCode(
@"

using Uno;
using Fuse;
using Fuse.Entities;
using Fuse.Entities.Primitives;

using static Uno.Vector;
using static Uno.Math;

block ObeliskMaterial : Fuse.Entities.DefaultShading
{
    PixelColor:$(!apply) float4(1, 0, 0, 1);
}

"
            );
        }*/

    }
}