using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class GDC : Test
	{
		[Test]
		public void GDC00()
		{
			AssertCode(
@"


public class PrismDofGlowEntity3D : Entity
{
	public static float DofFocalPlane = 0.0f;
	public static float DofFocalRange = 400.0f;

	public static texture2D DofBlurredTexture = null;
}

block PrismDofGlowBlock
{
	 $(float,int,PixelColor,pixel,texture2D)
	float DofFocalPlane: PrismDofGlowEntity3D.DofFocalPlane;
	float DofFocalRange: PrismDofGlowEntity3D.DofFocalRange;
	texture2D DofBlurredTexture: PrismDofGlowEntity3D.DofBlurredTexture;

	float DofLol: req(ViewPosition as float3) Math.Clamp(Math.Abs(-ViewPosition.Z - DofFocalPlane) / DofFocalRange, 0.0f, 1.0f);
	pixel float2 CornerVec: ClipPosition.XY / ClipPosition.W;
	float2 viewportSize: float2((float)Uno.Application.Viewport.Size.X, (float)Uno.Application.Viewport.Size.Y);
	float2 viewportPos: float2((float)Uno.Application.Viewport.Position.X, (float)Uno.Application.Framebuffer.Size.Y - (float)Uno.Application.Viewport.Bottom);
	float2 framebufferSize: float2((float)Uno.Application.Framebuffer.Size.X, (float)Uno.Application.Framebuffer.Size.Y);
	pixel float2 ScreenSpaceUv: ((CornerVec * .5f + .5f) * viewportSize + viewportPos) / framebufferSize;
	pixel float4 Blurred: sample(DofBlurredTexture, ScreenSpaceUv);

	float4 OriginalColor: req(prev PixelColor) prev PixelColor * (1.0f - DofLol) + Blurred * DofLol;
	PixelColor: OriginalColor;
	PixelColor: prev;//+ Blurred;
		
	pixel float vignette: Math.Pow(Vector.Length(CornerVec) / .78f, 2.8f) * .1f;
	PixelColor: prev - float4(float3(vignette),0.0f);
}


"
            );
		}

		[Test]
		public void GDC01()
		{
			AssertCode(
@"


class Dust : Entity
{
    public static Random random = null;

    class Particle
    {
        public float3 Pos;
        public float Scale, Rot;
        public double TimeScalar1;
        public double TimeScalar2;
        public float3 MoveVector;

        public Particle()
        {
            Pos = Dust.random.NextFloat3() * 400.0f - 200.0f;
            Scale = (Dust.random.NextFloat() * .1f + .9f) * .23f;
			Rot = Dust.random.NextFloat() * 2.0f * PIf;
            TimeScalar1 = (double)(Dust.random.NextFloat() * .6f + .4f) * .9f;
            TimeScalar2 = (double)(Dust.random.NextFloat() * .8f + .2f) * .6f;
            MoveVector = Normalize(Dust.random.NextFloat3() - .5f) * 2.0f;
        }
	}
	$(Particle)
}

"
            );
		}

		[Test]
		public void GDC02()
		{
			AssertCode(
@"

class Dust : Entity
{
    public static Random random = null;

    class Particle
    {
        public float3 Pos;
        public float Scale, Rot;
        public double TimeScalar1;
        public double TimeScalar2;
        public float3 MoveVector;

        public Particle()
        {
            Pos = Dust.random.NextFloat3() * 400.0f - 200.0f;
            Scale = (Dust.random.NextFloat() * .1f + .9f) * .23f;
			Rot = Dust.random.NextFloat() * 2.0f * PIf;
            TimeScalar1 = (double)(Dust.random.NextFloat() * .6f + .4f) * .9f;
            TimeScalar2 = (double)(Dust.random.NextFloat() * .8f + .2f) * .6f;
            MoveVector = Normalize(Dust.random.NextFloat3() - .5f) * 2.0f;
        }
		public $(Particle)
	}
	
}

"
            );
		}

		[Test]
		public void GDC03()
		{
			AssertCode(
@"

public drawable class Quad
{
	
	protected float2[] verts = new []
	{
		$(!draw,!try,!throw,float2,float3)
		float2(-1.0f, -1.0f),
		float2( 1.0f, -1.0f),
		float2( 1.0f,  1.0f),
		float2(-1.0f,  1.0f),
	};
	protected ushort[] indices = new ushort[]
	{
		0, 1, 2,
		2, 3, 0,
	};

	float2 CornerVec: vertex_attrib(verts, indices);
	float2 Uv: CornerVec * .5f + .5f;

	VertexCount: 6;
	ClipPosition: float4(CornerVec, 0, 1);
	PixelColor: float4(1);

	public void TestDraw() { draw { ClipPosition: float4(CornerVec * .5f, 0, 1); }; }
}



"
			);
		}

		[Test]
		public void GDC04()
		{
			AssertCode(
@"

$(block)

"
			);
		}

		[Test]
		public void GDC05()
		{
			AssertCode(
@"

block $(!using,!class,!enum,!partial,!namespace)
{
}

"
			);
		}

		[Test]
		public void GDC06()
		{
			AssertCode(
@"

public class Fairground_canon : Entity
{
	apply DefaultShading;
	apply Model(""../Data/fairground_canon.FBX"", Uno.Importers.ModelImportFlags.DefaultBatch);

	texture2D LightMap : import Texture2D(""../Data/fairground_canon_lightmap.png"");


	texture2D diffuseAtlas : import Texture2D(""../Data/fairground_canon.png"");
	//float4 lightmapSampled : sample(LightMap, TexCoord1);
	DiffuseMap : diffuseAtlas;

	public Fairground_canon()
	{
		Collider = new Fuse.Entities.BoxCollider(Uno.Geometry.Box(float3(-10), float3(10)));
	}

	Transform: this.Transform;


	public override void OnDraw(RenderingPass pass)
	{
		if(pass is FairgroundPipeline.CompositingPass)
		{
			draw $(float,int,!enum,Uno,pass,this)
				{

					Specular : 0;

					PixelColor : DiffuseMapColor;
					apply FairGroundLightMap;
					apply FairgroundBloomBlock;
				};
				Transform.Position = float3(30.638f, -24.051f, 28.206f);
		}
		else if(pass is FairgroundPipeline.BlurBufferPass)
		{
			draw
			{

				Specular : 0;
				PixelColor : DiffuseMapColor;
				apply FairGroundLightMap;
				apply BloomPowBlock;
			};
			Transform.Position = float3(30.638f, -24.051f, 28.206f);
		}
		else draw;
	}
}

"
            );
		}

        /*[Test]
        public void GDC07()
        {
            AssertCode(
@"

using Uno;
using Uno.Graphics;
using Fuse;
using Fuse.Entities;


public class Fairground_canon : Entity
{
    public override void OnDraw(RenderingPass pass)
    {
        if(pass is $(!pass)) FairgroundPipeline.CompositingPass)
        {
			
        }
		
    }
}

"
            );
        }*/

        [Test]
		public void GDC08()
		{
			AssertCode(
@"

namespace Uno.Content.Models
{
	public static class ModelReader
	{
		public static ModelParameterList<T> ReadParameterList<T>(Reader r, ModelParameter[] parameters, ModelParameterValueType itemType)
		{
			$(r,parameters,itemType)
			var items = new ModelParameter<T>[r.ReadCompressedInt()];
			for (int i = 0; i < items.Length; i++) items[i] = (ModelParameter<T>)parameters[r.ReadCompressedInt()];
			return new ModelParameterList<T>(itemType, items);
		}
	}
}

"
			);
		}

		[Test]
		public void GDC09()
		{
			AssertCode(
@"

namespace Uno.Content.Models
{
	public static class ModelReader
	{
		public static ModelParameterList<T> ReadParameterList<T>(Reader r, ModelParameter[] parameters, ModelParameterValueType itemType)
		{
			this.$(!Assert,!Application,!Int)
			var items = new ModelParameter<T>[r.ReadCompressedInt()];
			for (int i = 0; i < items.Length; i++) items[i] = (ModelParameter<T>)parameters[r.ReadCompressedInt()];
			return new ModelParameterList<T>(itemType, items);
		}
	}
}
"
			);
		}

		[Test]
		public void GDC10()
		{
			AssertCode(
@"

namespace Uno.Content.Models
{
	public static class ModelReader
	{
		public static ModelParameterList<T> ReadParameterList<T>(Reader r, ModelParameter[] parameters, ModelParameterValueType itemType)
		{
			$(!public,!private,!abstract)
			var items = new ModelParameter<T>[r.ReadCompressedInt()];
			for (int i = 0; i < items.Length; i++) items[i] = (ModelParameter<T>)parameters[r.ReadCompressedInt()];
			return new ModelParameterList<T>(itemType, items);
		}

	}
}

"
			);
		}

		[Test]
		public void GDC11()
		{
			AssertCode(
@"

namespace Uno.Content.Models
{
	public static class ModelReader
	{
		public static ModelParameterList<T> ReadParameterList<T>(Reader r, ModelParameter[] parameters, ModelParameterValueType itemType)
		{	
			var items = new ModelParameter<T>[r.ReadCompressedInt()];
			for (int i = 0; i < items.Length; i++) items[i] = (ModelParameter<T>)parameters[r.ReadCompressedInt()];
			return new ModelParameterList<T>(itemType, items);
		}
		$(float,int,double)
	}
}

"
			);
		}

		[Test]
		public void GDC12()
		{
			AssertCode(
@"

namespace Uno.Content.Models
{
	public static class ModelReader
	{
		public static ModelParameterList<T> ReadParameterList<T>(Reader r, ModelParameter[] parameters, ModelParameterValueType itemType)
		{	
			var items = new ModelParameter<T>[r.ReadCompressedInt()];
			for (int i = 0; i < items.Length; i++) items[i] = (ModelParameter<T>)parameters[r.ReadCompressedInt()];
			return new ModelParameterList<T>(itemType, items);
		}
		public static FullsampledSpline<T> ReadAnimationSampledSpline<T>(Reader r, int keyStride, Func<Reader, T> readKey)
		{
			$(!class,!namespace,!using)
		}
	}

}

"
			);
		}

		[Test]
		public void GDC13()
		{
			AssertCode(
@"

namespace StandardLib.Scenes
{
	class Entity3DTest 
	{
		public Entity3DTest()
		{

		}
		class someRenderer : Renderer {}
		public void Run()
		{
			Entity aEntity = new Entity();
			Children3D aChild = new Children3D(aEntity);
			aEntity.Components.Add(aChild);
			
			bool tempBool = aEntity.HasChildren;
			
			Children3D tempchild = aEntity.Children;
			
			Entity aParent = aEntity.Parent;
			
			Transform tempTrans = new Transform();
			Transform tempTrans2 = new Transform(float3(1.0f),float4(1.0f),float3(1.0f));
			Transform tempTrans3 = aEntity.Transform;
			aEntity.Transform = tempTrans;
			aEntity.Transform = tempTrans2;
			
			someRenderer tempRender = new someRenderer();
			someRenderer tempRender2 = new someRenderer();
			aEntity.Renderer = tempRender;
			aEntity.Renderer = tempRender2;
			Renderer tempRender3 = aEntity.Renderer;
			
			MeshBinding aMeshbind = aEntity.MeshBinding;
			assert(aMeshbind == null);
			MeshBinding bMeshBind = new MeshBinding();
			MeshBinding cMeshBind = new MeshBinding();
			aEntity.MeshBinding = bMeshBind;
			assert(aEntity.MeshBinding == bMeshBind);
			aEntity.MeshBinding = cMeshBind;
			assert(aEntity.MeshBinding == cMeshBind);
			
			Collider aCollid = aEntity.Collider;
			assert(aCollid == $(null,!nullReferenceException)
			

		}
	}
}"
            );
		}

        [Test]
        public void GDC14()
        {
            AssertCode(
@"


namespace StandardLib.Scenes
{
	class DrawTest : Entity
	{
        protected void override OnDraw()
        {
            draw $(this, Uno, float, int)
        }
	}
}"
            );
        }
	}
}