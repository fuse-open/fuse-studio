using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
	class GenericsTest : Test
	{
		[Test]
		public void Generics00()
		{
			AssertCode(
@"


class MyType
{
	public int Blam(){
		return 1;
	}
}

class GenericType<T>
{
	public T a;
	public GenericType(){}
}

class Foo
{
	void Bar()
	{
		var item = new GenericType<MyType>();
		item.a.$(Blam)
	}
}"
			);
		}
	}
}
