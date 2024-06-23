using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class Using_ : Test
	{
		[Test]
		public void Using_00()
		{
			AssertCode(
@"using $(Uno, OpenGL, !Static, !Platform);

"
			);
		}

		[Test]
		public void Using_01()
		{
			AssertCode(
@"

using Uno, $(!Uno)

"
			);
		}

		[Test]
		public void Using_02()
		{
			AssertCode(
@"

using Uno;

class Bar
{
	void Foo() { $(Math, Matrix, Vector); }
}

"
			);
		}

		[Test]
		public void Using_03()
		{
			AssertCode(
@"

class Bar: $(!Application)

"
			);
		}

		[Test]
		public void Using_04()
		{
			AssertCode(
@"

using Uno;

class Bar: $(Application, Vector, !UInt)

"
			);
		}

		[Test]
		public void Using_05()
		{
			AssertCode(
@"

using Uno;

namespace hah
{
	public class Bar: Application
	{
		$(Uno, Application, Float, Vector)
	}
}

"
			);
		}

		[Test]
		public void Using_06()
		{
			AssertCode(
@"

namespace hah
{
	public class Bar: Application
	{
		$(Uno, !Application, !Float, !Vector)
	}
}

"
			);
		}
	}
}