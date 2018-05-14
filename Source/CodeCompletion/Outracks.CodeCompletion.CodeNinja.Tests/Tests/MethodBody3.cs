using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class MethodBody3 : Test
	{
		[Test]
		public void MethodBody300()
		{
			AssertCode(
@"using Uno;

class Stack<T>
{
	T[] data;
	int index;
	
	public Stack(int size)
	{
		if (size <= 0) throw new Exception(""Stack size must be positive and greater than zero"");
		data = new T[size];
		index = 0;
	}
	
	public void Push(T item)
	{
		if ($(index)
	}
	
	public T Pop()
	{
		
	}
}

"
			);
		}

		[Test]
		public void MethodBody301()
		{
			AssertCode(
@"

class Stack<T>
{
	$(T)
}

"
			);
		}

		[Test]
		public void MethodBody302()
		{
			AssertCode(
@"

using Uno;

class Stack<T>
{
	T[] data;
	int index;
	
	public Stack(int size)
	{
		if (size <= 0) throw new Exception(""Stack size must be positive and greater than zero"");
		data = new T[size];
		index = 0;
	}
	
	public void Push(T item)
	{
		if ($(index)
	}
	
	public $(T)
}

"
			);
		}

		[Test]
		public void MethodBody303()
		{
			AssertCode(
@"

using Uno;

class Stack<T>
{
	T[] data;
	int index;
	
	public Stack(int size)
	{
		if (size <= 0) throw new Exception(""Stack size must be positive and greater than zero"");
		data = new $(T)
}
"
			);
		}

	}
}