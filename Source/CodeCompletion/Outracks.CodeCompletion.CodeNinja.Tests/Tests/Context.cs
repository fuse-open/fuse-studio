using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class Context : Test
	{
		[Test]
		public void Context00()
		{
			AssertCode(
@"using Uno.Collections;

class a : Uno.Application{}

drawable class BoardRenderer
{
	public const int MaxVertices = 15000;

	float2[] vertices = new float2[MaxVertices];
	float2[] tcs = new float2[MaxVertices];
	float3[] DasColor = new float3[MaxVertices];
	float3[] PlayerOwn = new float3[MaxVertices];
	float[] Armies = new float[MaxVertices];
	float2[] SeaLines = new float2[250];

	meta int VertexCount;
	meta int TexCount;

	public void BuildBoard(List<float3> tiles,List<float2> lines)
	{
		int c = 0;
		for (int i = 0; i < tiles.Count; i++)
		{
			var t = tiles[i];
			vertices[c+1] = float2(t.X + t.X, t.Y);
			vertices[c+2] = float2(t.X + t.X, t.Y);

			vertices[c+4] = float2(t.X + t.X, t.Y + t.X);
			vertices[c+5] = float2(t.X, t.Y + t.Y);

			tcs[c+0] = float2(0,0);
			tcs[c+1] = float2(1,0);
			tcs[c+2] = float2(1,1);
			tcs[c+3] = float2(0,0);
			tcs[c+4] = float2(1,1);
			tcs[c+5] = float2(0,1);
			c += 6;
		}
		
		
		$(int,float,double,bool,!public,!private,!class);
	}

	
}"
			);
		}

	}
}