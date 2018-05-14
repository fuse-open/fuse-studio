using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class DemoTest : Test
	{
		public void DemoTest00()
		{
			AssertCode(
@"using Uno;
using Fuse.Entities;

public class Landscape : Entit$(Uno,Entity)

"
			);
		}

		public void DemoTest01()
		{
			AssertCode(
@"

using Uno;
using Fuse.Entities;
using Uno.Geometry;

public class Landscape : Fuse.Entity
{
	
	apply $(DefaultShading)
	
	public Landscape()
	{
		Collider = new BoxCollider(Box(float3(10.0f,0.0f,10.0f),float3(-10.0f,0.0f,-10.0f)));
	}
	
	Transform: this.Transform;
}

"
			);
		}

        /*[Test]
        public void DemoTest02()
        {
            AssertCode(
@"

using Uno;
using Fuse;
using Fuse.Entities;
using Fuse.Entities.Primitives;

using Uno.Geometry;

public class Landscape : Entity
{

    apply DefaultShading;

    public Landscape()
    {
        Collider = new BoxCollider(Box(float3(10.0f,0.0f,10.0f),float3(-10.0f,0.0f,-10.0f)));
    }

    Transform: this.Transform;
    Camera: Context.$(Camera)

    public override void OnDraw(Fuse.Entities.RenderingPass pass)
    {
        draw;
    }

}

"
            );
        }*/

        [Test]
		public void DemoTest03()
		{
			AssertCode(
@"

using Uno;
using Fuse;
using Fuse.Entities;
using Uno.Geometry;

public class Landscape : Entity
{

	apply DefaultShading;
	
	
	static int gridx = 100;
	static int gridz = 100;
	static int numCells = gridx * gridz;
	static float trisize = 50.0f / (float)gridz;
	
	protected float2[] verts = new Float2[numCells * 3 * 2];//2 tri per cell, 3 vert per tri

	public Landscape()
	{
		Collider = new BoxCollider(Box(float3(10.0f,0.0f,10.0f),float3(-10.0f,0.0f,-10.0f)));
		
		int currentindex = 0;
		for(int z=0; z < gridz;z++)
		{
			float z1 = (float)z / (float)gridz;
			float z2 = ((float)z +1.0f) / (float)gridz;
			
			for(int x=0;x<gridx;x++)
			{
				float x1 = (float)x / (float)gridx;
				float x2 = ((float)x +1.0f) / (float)gridx;
				
				verts[currentindex + 0] = float2(x1,z1);
				verts[currentindex + 1] = float2(x1,z2);
				verts[currentindex + 2] = float2(x2,z1);
				
				verts[currentindex + 3] = float2(x2,z1);
				verts[currentindex + 4] = float2(x1,z2);
				verts[currentindex + 5] = float2(x2,z2);
			}
		}
	}

	Transform: this.Transform;
	Camera: Camera.Current;
	
	float2 Vert: $(vertex_attrib)

	public override void OnDraw(RenderingPass pass)
	{
		draw
		{
			
		};
		
	}

}


"
			);
		}

	}
}