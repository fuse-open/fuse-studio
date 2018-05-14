using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class ClassBody7 : Test
	{
		[Test]
		public void ClassBody700()
		{
			AssertCode(
@"abstract class b
{
	$(abstract)
}

"
			);
		}

		[Test]
		public void ClassBody701()
		{
			AssertCode(
@"
abstract class b
{
	public abstract void somemethod();
	public virtual void someothermethod()
	{
	}
}
abstract class c : b
{
	
}

class d : $(b,c)

"
			);
		}

		[Test]
		public void ClassBody702()
		{
			AssertCode(
@"

class Foo
{
	$(class)
}

"
			);
		}

		[Test]
		public void ClassBody703()
		{
			AssertCode(
@"

class Foo
{
	public void Derp()
	{
		$(!class)
	}
}

"
			);
		}

		[Test]
		public void ClassBody704()
		{
			AssertCode(
@"

public class Foo<T>
{
	T[] data = new $(T)
}

"
			);
		}

		[Test]
		public void ClassBody705()
		{
			AssertCode(
@"

public class Foo<T>
{
	T[] data = new$(!T)
}
"
			);
		}

	}
}