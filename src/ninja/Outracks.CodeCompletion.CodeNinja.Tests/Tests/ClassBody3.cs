using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class ClassBody3 : Test
	{
		[Test]
		public void ClassBody300()
		{
			AssertCode(
@"public class C<T>
{
	T[] foo;
	$(Uno)
}

"
			);
		}

		[Test]
		public void ClassBody301()
		{
			AssertCode(
@"

public class Foo<T>
{
	T[] data = new T[100];

	public T this[int index]
    {
        get { return data[index]; }
        set { data[index] = value; }
    }

	$(Uno)
}

"
			);
		}

		[Test]
		public void ClassBody302()
		{
			AssertCode(
@"

drawable class Bart
{
	meta int foo;
	texture2D moo: import ""boo.png"";
	$(Uno)
}

"
			);
		}

		[Test]
		public void ClassBody303()
		{
			AssertCode(
@"


class Foo: Uno.Application
{
	public Foo(): base(""asd"") {}

	$(Uno)

}
"
			);
		}

	}
}