using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class KristianTests : Test
	{
		[Test]
		public void KristianTests00()
		{
			AssertCode(
@"

namespace IntellisenseTestTest
{
    class App : Uno.Application
    {

		public float[][] GenerateNoise(int size, int blockSize)
		{
			float[][] noise = new float[size][size];
			$(for, while, do)
		}


        public override void Draw()
        {
			draw Quad
			{

			}
        }
    }
}

"
            );
		}

		[Test]
		public void KristianTests01()
		{
			AssertCode(
@"

namespace IntellisenseTestTest
{
    class App : Uno.Application
    {

		public float[] GenerateNoise(int size, int blockSize)
		{
			float[] texture = new float[size*size];

			float[] gradients = new float[$(size) / blockSize];

		}
	}
}

"
            );
		}

		[Test]
		public void KristianTests02()
		{
			AssertCode(
@"

namespace IntellisenseTestTest
{
    class App : Uno.Application
    {

		public float[] GenerateNoise(int size, int blockSize)
		{
			float[] texture = new float[size*size];

			float[] gradients = new float[size / $(blockSize)];

		}
	}
}

"
            );
		}

		[Test]
		public void KristianTests03()
		{
			AssertCode(
@"

namespace IntellisenseTestTest
{
    class App : Uno.Application
    {

		public float[] GenerateNoise(int size, int blockSize)
		{
			float[] texture = new float[size*size];
			if (blockSize % size != 0) throw new Exception(""Size must be a multiple of blocksize"");
			float[] gradients = new float[(int)(size/blockSize) * (int)(size/blockSize)];
			$(for)
		}
	}
}

"
            );
		}

		[Test]
		public void KristianTests04()
		{
			AssertCode(
@"

namespace IntellisenseTestTest
{
    class App : Uno.Application
    {

		public float[] GenerateNoise(int size, int blockSize)
		{
			float[] texture = new float[size*size];
			if (blockSize % size != 0) throw new Exception(""Size must be a multiple of blocksize"");
			int gradientSize = (int)(size/blockSize) * (int)(size/blockSize);
			float[] gradients = new float[$(gradientSize)];
		}
	}
}

"
            );
		}

		[Test]
		public void KristianTests05()
		{
			AssertCode(
@"

namespace IntellisenseTestTest
{
    class App : Uno.Application
    {

		public float[] GenerateNoise(int size, int blockSize)
		{
			float[] texture = new float[size*size];
			if (blockSize % size != 0) throw new Exception(""Size must be a multiple of blocksize"");
			int gradientSize = (int)(size/blockSize) + 1;
			float2[] gradients = new float2[gradientSize * gradientSize];
			var random = new Random(123123);
			for (int i = 0; i < gradientSize * gradientSize; i++)
			{
				gradients[i] = random.NextFloat2();
			}

			float cbx = 0.0f;
			float cby = 0.0f;

			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					float2 grad1 = float2(x,y) - float2(cbx, cby);
					float2 grad2 = float2(x,y) - float2(cbx +  blockSize, cby);
					float2 grad3 = float2(x,y) - float2(cbx, cby + blockSize);
					float2 grad4 = float2(x,y) - float2(cbx + blockSize, cby + blockSize);

					float d1 = Vector.Dot(grad1, gradients[(int)cbx + size * (int)cby]);
					float d2 = Vector.Dot(grad2, gradients[(int)cbx + 1 + size  * (int)cby]);
					float d3 = Vector.Dot(grad3, gradients[(int)cbx  + size * ((int)cby + 1)]);
					float d4 = Vector.Dot(grad4, gradients[(int)cbx + 1 + size * ((int)cby + 1)]);

					float xPos = (x % blockSize) / blockSize;
					float yPos = (y % blockSize) / blockSize;

					float xx = Math.Lerp(d1, d2, xPos);
					float xxx = Math.Lerp(d3, d4, xPos);
					float res = Math.Lerp(xx, xxx, yPos);

					texture[y * size + x] = $(res);
		}
	}
}

"
            );
		}

		[Test]
		public void KristianTests06()
		{
			AssertCode(
@"

namespace IntellisenseTestTest
{
	class Test5
	{

		public class Ball
		{
			public float3 Position { get; set; }
			Camera camera = new Camera();
			$(camera)
		}

	}
}

"
            );
		}

		[Test]
		public void KristianTests07()
		{
			AssertCode(
@"

namespace IntellisenseTestTest
{
	class Test5
	{

		public class Ball
		{
			public float3 Position { get; set; }
			public float Radius { get { return 20f; } }

            public void Normalize()
            {
                Radius=1;
            }

			public Ball(float3 position)
			{
				Position = position;

			}

		}

		public Test5()
		{
			balls = new List<Ball>();
			Uno.UI.Window.MouseDown += OnMouseDown;

		}

		List<Ball> balls;

		public void Foo()
		{
			int2 mPos = arg.Position;
			foreach (Ball b in balls)
			{
				b.$(Position,Radius,Normalize)
			}
		}
	}
}

"
            );
		}

		[Test]
		public void KristianTests08()
		{
			AssertCode(
@"

namespace Spindler
{
	class SpindleInputController{}

    class App : Uno.Application
    {
		Spindle spindle;
		SpindleInputController spindleInput;
		public override void Load()
		{
			base.Load();
			spindle = new Spindle();
			$(spindleInput)
		}
	}
}

"
            );
		}

		[Test]
		public void KristianTests09()
		{
			AssertCode(
@"

namespace Spindler
{
	class SpindleInputController
	{}

    class App : Uno.Application
    {
		Spindle spindle;
		SpindleInputController spindleInput;
		public override void Load()
		{
			base.Load();
			spindle = new Spindle();
			spindleInput = new $(SpindleInputController)
		}
	}
}

"
            );
		}

		[Test]
		public void KristianTests10()
		{
			AssertCode(
@"

namespace Spindler
{
	class SpindleController
	{
		Spindle spindle;
		public SpindleController(Spindle spindle)
		{
			this.$(spindle)
		}

	}
}

"
            );
		}

		[Test]
		public void KristianTests11()
		{
			AssertCode(
@"

namespace Spindler
{
	class SpindleController
	{
		Spindle spindle;
		public SpindleController(Spindle spindle)
		{
			this.spindle = $(spindle)
		}

	}
}

"
            );
		}

		[Test]
		public void KristianTests12()
		{
			AssertCode(
@"

namespace Spindler
{
	class Test
	{
		private float s;
		public float S
		{
			get
			{
				return $(s)
			}
			set
			{
				s = value;
			}
		}
	}
}

"
            );
		}

        /*[Test]
        public void KristianTests13()
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
using Uno.Platform;
using Uno.UI;

namespace GameOfLife
{
    class App : Uno.Application
    {
        public void OnKeyUp(object sender, KeyEventArgs arg)
        {
            if (arg.$(IsAltKeyPressed, IsControlKeyPressed, Equals, GetHashCode, IsHandled, Key, IsMetaKeyPressed, IsShiftKeyPressed, ToString)
        }
    }
}

"
            );
        }


        [Test]
        public void KristianTests14()
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
using Uno.UI;

namespace IntellisenseTestTest
{
    class Test5
    {

        public void OnKeyDown(object sender, Uno.UI.KeyEventArgs arg)
        {
            if (arg.$(Alt, Control, Equals, GetHashCode, Handled, KeyCode, KeyData, Meta, Shift, ToString)
        }
    }
}

"
            );
        }*/

        /*[Test]
        public void KristianTests15()
        {
            AssertCode(
@"

using Uno;
using Uno.Collections;
using Uno.Graphics;
using Uno.Audio;
using Fuse;
using Fuse.Elements;
using Fuse.Entities;
using Fuse.Entities.Primitives;
using Uno.Content;
using Uno.Content.Models;

namespace IntellisenseTestTest
{
    class Test5
    {
        public bool WDown = false;
        public bool SDown = false;
        public bool ADown = false;
        public bool DDown = false;
        public void OnKeyDown(object sender, Uno.UI.KeyEventArgs arg)
        {
            if (arg.KeyCode == Keys.W) $(WDown, SDown, ADown, DDown)
        }
    }
}

"
            );
        }*/

        [Test]
		public void KristianTests16()
		{
			AssertCode(
@"

namespace IntellisenseTest2
{
    class App : Uno.Application
    {
        public override void Draw()
        {
			App.Current.$(ClearColor)
        }
    }
}

"
            );
		}

		[Test]
		public void KristianTests17()
		{
			AssertCode(
@"

namespace IntellisenseTest2
{
    class App : Uno.Application
    {

		const int size = 50;
		float[] heights = new float[size];

		public override void Load()
		{
			base.Load();

		}

		apply DefaultShading;
        public override void Draw()
        {
			float cubeSize = 5f;
			for (int i = 0; i < size; i++)
			{
				draw Cube
				{
					Position: prev + i * cubeSize;
					Scale: float3(cubeSize, heights[$(i)]), 0);
				};
			}
        }
    }
}

"
            );
		}

        /*[Test]
        public void KristianTests18()
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

namespace IntellisenseTest2
{
    class App : Uno.Application
    {
        public List<float> MergeSort(List<float> list)
        {
            if (list.Count <= 1) return list;
            var left = new List<float>();
            var right = new List<float();
            int middle = list.Count / 2;
            for (int i = 0; i < middle; i++)
            {
                left.Add(list[i]);
                right.$(Add, Clear, Contains, Count, Equals, GetEnumerator, GetHashCode, Insert, Remove, RemoveAt, Sort, ToArray, ToString);
            }
        }
    }
}

"
            );
        }*/

        [Test]
		public void KristianTests19()
		{
			AssertCode(
@"

namespace IntellisenseTest2
{
    class App : Uno.Application
    {

		const int size = 128;
		List<float> heights = new List<float>();
		List<float> sortedHeights = new List<float>();

		double time = 0.0;

		public override void Load()
		{
			base.Load();
			var random = new Random((int)Time.FrameTime);
			for (int i = 0; i < size; i++)
			{
				heights.Add(random.NextFloat(0f, 100f));
			}
			$(sortedHeights)

		}
	}
}

"
            );
		}

		[Test]
		public void KristianTests20()
		{
			AssertCode(
@"

namespace Othello
{
    class App : Uno.Application
    {
        public override void Draw()
        {
			float size = 5f;
            int a = 0;
			float test = $(a);
			draw Cube
			{
				Scale: float3(boardSize * $(size)
			};

        }
    }
}

"
            );
		}

		[Test]
		public void KristianTests21()
		{
			AssertCode(
@"

namespace DummyProj
{
    class App : Uno.Application
    {

        public override void Load()
        {
			List<List<float>> $(!List)

        }
    }
}

"
            );
		}

		[Test]
		public void KristianTests22()
		{
			AssertCode(
@"

namespace DummyProj
{
    class App : Uno.Application
    {

		Plane plane;

		public override void Load()
		{
			$(plane)

		}


    }
}

"
            );
		}

		[Test]
		public void KristianTests23()
		{
			AssertCode(
@"

using Uno.Collections;

namespace DummyProj
{
    class App : Uno.Application
    {


		apply DefaultShading;
		public override void Draw()
		{

			List<float> floats = new $(List<float>)

		}

    }
}
"
            );
		}

	}
}