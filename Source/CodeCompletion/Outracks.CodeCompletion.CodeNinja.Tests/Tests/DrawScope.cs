using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class DrawScope : Test
	{
		[Test]
		public void DrawScope00()
		{
			AssertCode(
@"class Foo: Uno.Application
{
	float Bar: 1.0f;

	public override void Draw()
	{
		draw
		{
			$(drawable, Bar, ClipPosition)
		}
	}
}

"
			);
		}

		[Test]
		public void DrawScope01()
		{
			AssertCode(
@"

class Foo: Uno.Application
{
	apply $(Uno)
}

"
			);
		}

		[Test]
		public void DrawScope02()
		{
			AssertCode(
@"

class Foo: Uno.Application
{
	$(apply)
}

"
			);
		}

		public void DrawScope03()
		{
			AssertCode(
@"

class Foo: Uno.Application
{
	apply Fuse.Entities.$(DefaultShading)
}

"
			);
		}

		public void DrawScope04()
		{
			AssertCode(
@"

class Foo: Uno.Application
{
	apply Fuse.Entities.DefaultShading;
	float Bar: 1.0f;

	public override void Draw()
	{
		draw
		{
			$(drawable, Bar, DiffuseColor)
		}
	}
}

"
			);
		}

		[Test]
		public void DrawScope05()
		{
			AssertCode(
@"

class Foo: Uno.Application
{
	public override void Draw()
	{
		draw
		{
			$(ClipPosition, PixelColor, BlendSrc)
		}
	}
}

"
			);
		}

		[Test]
		public void DrawScope06()
		{
			AssertCode(
@"

class Foo: Uno.Application
{
	public override void Draw()
	{
		draw
		{
			PrimitiveType: Uno.Graphics.PrimitiveType.$(Triangles,!DiffuseColor,!Uno)
		}
	}
}

"
			);
		}

		[Test]
		public void DrawScope07()
		{
			AssertCode(
@"


drawable class Bar: Uno.Application
{
	apply Fuse.Entities.DefaultShading;
	float Bar: 1.0f;
	
	public override void Draw()
	{
		
		for (int i = 0; i < 10; i++) draw Sphere, $(Uno) { Position : float3(i, 0, 0); };
		
		draw
		{
			

		};
	}
}

"
			);
		}

		[Test]
		public void DrawScope08()
		{
			AssertCode(
@"

class Foo: Uno.Application
{
	public override void Draw()
	{
		draw
		{
			PixelColor: $(sample)
		};
	}
}

"
			);
		}

		public void DrawScope09()
		{
			AssertCode(
@"

using Uno;
using Uno.Collections;
using Uno.Graphics;
using Uno.Audio;
using Fuse;
using Fuse.Entities;
using Fuse.Entities.Primitives;
using Uno.Content;
using Uno.Content.Models;

namespace BaseTest
{
    class App : Uno.Application
    {
        public override void Draw()
        {
			draw Sphere
			{
				apply DefaultShading;
				
				$(Time)
			}
        }
    }
}

"
			);
		}

		[Test]
		public void DrawScope10()
		{
			AssertCode(
@"

using Uno;
using Uno.Collections;
using Uno.Graphics;
using Uno.Audio;
using Fuse.Entities;
using Fuse.Entities.Primitives;
using Uno.Content;
using Uno.Content.Models;

namespace BaseTest
{
    class App : Uno.Application
    {
		block Derp
		{
			float3 Herp: float3(2.0f);
			$(Herp)
		}
		
        public override void Draw()
        {
			draw Sphere
			{
				apply DefaultShading;
				
			}
        }
    }
}

"
            );
		}

		[Test]
		public void DrawScope11()
		{
			AssertCode(
@"

using Uno;
using Uno.Collections;
using Uno.Graphics;
using Uno.Audio;
using Fuse;
using Fuse.Entities;
using Fuse.Entities.Primitives;
using Uno.Content;
using Uno.Content.Models;

namespace BaseTest
{
    class App : Uno.Application
    {
		block Derp
		{
			float3 Herp: float3(2.0f);
			
		}
		
        public override void Draw()
        {
			draw Sphere
			{
				apply DefaultShading;
				float3 Color: float3(2.0f);
				$(Color)
			}
        }
    }
}"
            );
		}

        [Test]
        public void DrawScope12()
        {
            AssertCode(
@"

using Uno;
using Uno.Collections;
using Uno.Graphics;
using Fuse;
using Fuse.Entities;
using Fuse.Entities.Primitives;
using Uno.Content;
using Uno.Content.Models;
using Uno.Math;

class App : Uno.Application
{
	block Test : $(DefaultShading)
	{
	}

	public override void Draw()
	{
		draw $(DefaultShading) Cube
		{
			PixelColor: float4($(Sin)(0.5f), 0.5f, 0.0f, 1);
		};
	}
}"
            );
        }
        [Test]
        public void DrawScope13()
        {
            AssertCode(
@"

using Uno;
using Uno.Collections;
using Uno.Graphics;
using Fuse;
using Fuse.Entities;
using Fuse.Entities.Primitives;
using Uno.Content;
using Uno.Content.Models;
using Uno.Math;
using Uno.Vector;

class App : Uno.Application
{
	block Test : DefaultShading
	{
	}

	public override void Draw()
	{
		var aspect = Context.Aspect;
		float time = (float)Time.FrameTime;

		draw
		{
			float2 []vertices: new []
			{
				float2(-1, -1),
				float2(1, -1),
				float2(1,1),
				float2(1,1),
				float2(-1,1),
				float2(-1,-1)
			};

			float2 FragCoord: float2(pixel ClipPosition.X, pixel ClipPosition.Y / aspect);

			ClipPosition: float4(vertex_attrib(vertices), 0, 1);
			PixelColor:
			{
				float3 rayOrigin = float3(0, 0, 1);
				float3 rayStart = rayOrigin + float3(0, 0, 20);
				float3 rayDir = Normalize(float3(FragCoord,0) - rayStart);

				float t = 0;
				for(int i = 0;$(i) < 32;++i)
				{
					float3 p = rayStart + rayDir * $(t);
					float d = ParseScene(p, time);
					if(d < 0.01f)
					{
						float3 norm = $(GenNormal)(p, time);
						float diff = Max(0.05f, Dot(norm, float3(0.33f)));
						return diff * float4(1, 0, 0, 1);
					}
					t += d;
				}

				return float4(0);
			};
		};
	}

	public static float3 GenNormal(float3 p, float t)
	{
		float3 normal;
		float3 precis = float3(0.01f, 0, 0);
		normal.X = ParseScene(p + precis.XYZ, t) - ParseScene(p - precis.XYZ, t);
		normal.Y = ParseScene(p + precis.YXZ, t) - ParseScene(p - precis.YXZ, t);
		normal.Z = ParseScene(p + precis.YZX, t) - ParseScene(p - precis.YZX, t);
		return Normalize(normal);
	}

	public static float ParseScene(float3 p, float t)
	{
		float d = Length(p) - 0.2f;
		float c = 2.4f * Cos(2.0f*t);
		float f = Sin(c*p.X)*Sin(c*p.X)*Sin(c*p.Y);
		return d + f;
	}
}"
            );
        }

        [Test]
        public void DrawScope14()
        {
            AssertCode(
@"
using Uno;
using Uno.Collections;
using Uno.Graphics;
using Fuse;
using Fuse.Entities;
using Fuse.Entities.Primitives;
using Uno.Content;
using Uno.Content.Models;
using Uno.Math;
using Uno.Vector;
using Uno.Matrix;

class App : Uno.Application
{
	public override void Draw()
	{
		var aspect = Context.Aspect;
		float time = (float)Time.FrameTime;

		draw
		{
			float2 []vertices: new []
			{
				float2(-1, -1),
				float2(1, -1),
				float2(1,1),
				float2(1,1),
				float2(-1,1),
				float2(-1,-1)
			};

			float2 FragCoord: float2(pixel ClipPosition.X, pixel ClipPosition.Y / aspect);
            float2 Test: $(FragCoord);
		};
	}
}"
            );
        }

        [Test]
        public void DrawScope15()
        {
            AssertCode(
@"
using Uno;
using Uno.Collections;
using Uno.Graphics;
using Fuse;
using Fuse.Entities;
using Fuse.Entities.Primitives;
using Uno.Content;
using Uno.Content.Models;
using Uno.Math;
using Uno.Vector;
using Uno.Matrix;

class App : Uno.Application
{
    block Triangle
    {
        public float2 man: float2(0);
    };

	public override void Draw()
	{
		var aspect = Context.Aspect;
		float time = (float)Time.FrameTime;

    	draw
		{
            apply DefaultShading;
			float2 []vertices: new []
			{
				float2(-1, -1),
				float2(1, -1),
				float2(1,1),
				float2(1,1),
				float2(-1,1),
				float2(-1,-1)
			};

			public float2 Foo: float2(pixel ClipPosition.X, pixel ClipPosition.Y / aspect);
		};

		draw
		{
            apply Triangle;
			float2 []vertices: new []
			{
				float2(-1, -1),
				float2(1, -1),
				float2(1,1),
				float2(1,1),
				float2(-1,1),
				float2(-1,-1)
			};

            float2 Test: ClipPosition.$(X,!Abs,!pixel)
		};
	}
}"
            );
        }

        [Test]
        public void DrawScope16()
        {
            AssertCode(
@"
using Uno;
using Uno.Collections;
using Uno.Graphics;
using Fuse;
using Fuse.Entities;
using Fuse.Entities.Primitives;
using Uno.Content;
using Uno.Content.Models;
using Uno.Math;
using Uno.Vector;
using Uno.Matrix;

class App : Uno.Application
{
    block Triangle
    {
        public float2 man: float2(0);
    }

	public override void Draw()
	{
		var aspect = Context.Aspect;
		float time = (float)Time.FrameTime;

    	/*draw
		{
            apply DefaultShading;
			float2 []vertices: new []
			{
				float2(-1, -1),
				float2(1, -1),
				float2(1,1),
				float2(1,1),
				float2(-1,1),
				float2(-1,-1)
			};

			public float2 Foo: float2(pixel ClipPosition.X, pixel ClipPosition.Y / aspect);
		};*/

		draw
		{
            apply Triangle;
			float2 []vertices: new []
			{
				float2(-1, -1),
				float2(1, -1),
				float2(1,1),
				float2(1,1),
				float2(-1,1),
				float2(-1,-1)
			};

            float2 Test: $(vertices, man)
		};
	}
}"
            );
        }

        public void DrawScope17()
        {
            AssertCode(
@"
using Uno;
using Uno.Collections;
using Uno.Graphics;
using Fuse;
using Fuse.Entities;
using Fuse.Entities.Primitives;

class Foo: Application
{
	float Bar: 1.0f;

	block baraaa
	{
		apply Fuse.Entities.DefaultShading;
	}

	public override void Draw()
	{
		draw this, baraaa, Cube, Default$(DefaultShading, DefaultMaterial, !new)
		{
			float2 FragCoord: float2(pixel ClipPosition.X, pixel ClipPosition.Y);
			FragCoord: prev;
		};
	}
}"
            );
        }
        
        [Test]
        public void DrawScope18()
        {
            AssertCode(
@"
using Uno;
using Uno.Collections;
using Uno.Graphics;
using Fuse;
using Fuse.Entities;
using Fuse.Entities.Primitives;
using Uno.Math;

class Foo: Application
{
	float Bar: Sin(0.5f);

	public override void Draw()
	{
		draw this, Cube,
		{
			apply DefaultShading;
            float3 foo: float3(1,0,1);
			float2 FragCoord: float2(pixel ClipPosition.X, pixel ClipPosition.Y);
			FragCoord: prev.$(X, Y, !DefaultMaterial);
		};
	}
}"
            );
        }

        [Test]
        public void DrawScope19()
        {
            AssertCode(
@"
using Uno;
using Uno.Collections;
using Uno.Graphics;
using Fuse;
using Fuse.Entities;
using Fuse.Entities.Primitives;
using Uno.Math;

class Foo: Application
{
	float Bar: Sin(0.5f);

	public override void Draw()
	{
		draw this, Cube,
		{
			apply DefaultShading;
            float3 foo: float3(1,0,1);
			float2 FragCoord: float2(pixel ClipPosition.X, pixel ClipPosition.Y);
			$(FragCoord)
		};
	}
}"
            );
        }
    
    }
}