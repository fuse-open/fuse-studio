using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class KristianTests2 : Test
	{
		/*[Test]
		public void KristianTests200()
		{
			AssertCode(
@"using Uno;
using Uno.Audio;

namespace DummyProj
{
    class App : Uno.Application
    {
		public override void Load()
		{
			SoundSourceNode ssn = new $(SoundSourceNode)
		}

    }
}

"
			);
		}*/

		/*
		[Test]
		public void KristianTests201()
		{
			AssertCode(
@"

using Uno;
using Uno.Audio;

namespace DummyProj
{
    class App : Uno.Application
    {
		public override void Load()
		{
			List<GainNode> gainNodes = new $(List<GainNode>)
		}
    }
}

"
			);
		}*/

		[Test]
		public void KristianTests202()
		{
			AssertCode(
@"

namespace DummyProj
{
    class App : Uno.Application
    {
		public override void Load()
		{
			float[] floats = new $(float)
		}

    }
}   

"
			);
		}

		[Test]
		public void KristianTests203()
		{
			AssertCode(
@"

namespace DummyProj
{
    class App : Uno.Application
    {
		class Foo
		{
		}
		
		public override void Load()
		{
			$(Foo)
		}

    }
}   

"
			);
		}

		[Test]
		public void KristianTests204()
		{
			AssertCode(
@"

namespace DummyProj
{
    class App : Uno.Application
    {
		class Foo
		{
		}
		
		public override void Load()
		{
			Foo foo = new $(Foo)
		}

    }
}

"
			);
		}

		[Test]
		public void KristianTests205()
		{
			AssertCode(
@"

namespace DummyProj
{
    class App : Uno.Application
    {
		public class Foo
		{
			public class Bar
			{
				public class FooBar
				{
					Foo foo = new $(Foo);
				}
			}
		}
    }
}

"
			);
		}

		[Test]
		public void KristianTests206()
		{
			AssertCode(
@"

namespace DummyProj
{
    class App : Uno.Application
    {
		
		public override void Load()
		{
			base.Load();
		}
		
		block FooBlock
		{
			apply DefaultShading;
		}
		
		apply $(FooBlock)
	}
}

"
            );
		}

		[Test]
		public void KristianTests207()
		{
			AssertCode(
@"

namespace DummyProj
{
    class App : Uno.Application
    {
		public override void Load()
		{
			var tex = $(import)
		}
    }
}   

"
            );
		}

		[Test]
		public void KristianTests208()
		{
			AssertCode(
@"

namespace DummyProj
{
    class App : Uno.Application
    {
		
		float3[] positions = new float3[100];
		public override void Load()
		{
		
			for (int i = 0; i < positions.$(Length)
		}

    }
}   

"
            );
		}

		[Test]
		public void KristianTests209()
		{
			AssertCode(
@"

using Uno.Collections;

namespace DummyProj
{
    class App : Uno.Application
    {
		
		List<float> list = new List<float>();
		public override void Load()
		{
		
			for (int i = 0; i < list.$(Count)
		}

    }
}

"
            );
		}

		[Test]
		public void KristianTests210()
		{
			AssertCode(
@"

namespace DummyProj
{
    class App : Uno.Application
    {
		
		public class Node
		{
			Node parent;
			List<Node> children = new List<Node>();
			
			public Node()
			{
				$(children);
			}
		}
	}
}
			
"
            );
		}

		public void KristianTests211()
		{
			AssertCode(
@"

namespace DummyProj
{
    class App : Uno.Application
    {
		
		apply DefaultShading;
		
		$(CameraPosition)
		
		public override void Draw()
		{
			base.Draw();
			
			draw Cone
			{
				Scale: 50f;
			};
		}
	}
}

"
            );
		}

		[Test]
		public void KristianTests212()
		{
			AssertCode(
@"

namespace DummyProj
{
    class App : Uno.Application
    {
		
		apply DefaultShading;
		
		CameraPosition: float3(0, 100,0);
		Dictionary<float, int> dict = new Dictionary<float, int>();
		public override void Draw()
		{
			base.Draw();
			
			$(dict)
		}
	}
}

"
            );
		}

		public void KristianTests213()
		{
			AssertCode(
@"

namespace DummyProj
{
    class App : Uno.Application
    {
		apply DefaultShading;
		public override void Load()
		{
			base.Load();	
		}
		
		public override void Draw()
		{
			base.Draw();
			draw Cone
			{
				$(Diffuse)
			};
		}	
	}	
}
"
            );
		}

	}
}