using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class ImportAndApply : Test
	{
		[Test]
		public void ImportAndApply00()
		{
			AssertCode(
@"class Foo
{
	texture2D tex = import $(Uno, !Texture2D)
}

"
			);
		}

		[Test]
		public void ImportAndApply01()
		{
			AssertCode(
@"

using Uno.Graphics;

class Foo
{
	texture2D tex = import $(Texture2D, !int, !Texture2DImporter)
}

"
			);
		}

		[Test]
		public void ImportAndApply02()
		{
			AssertCode(
@"

using Uno.Graphics;

block DefaultShading
{

}

class Foo
{
	apply $(DefaultShading)

	void Draw()
	{
		draw
		{
			texture2D DiffuseMap: import Texture2D();

		};
	}
}

"
			);
		}

		[Test]
		public void ImportAndApply03()
		{
			AssertCode(
@"

class Foo
{
	apply Uno.$(!DefaultShading)

	void Draw()
	{
		draw
		{


		};
	}
}"
            );
		}

	}
}