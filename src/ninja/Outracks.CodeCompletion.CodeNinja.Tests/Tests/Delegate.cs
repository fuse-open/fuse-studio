using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class Delegate : Test
	{
		[Test]
		public void Delegate00()
		{
			AssertCode(
@"class b
{
	static int theInt = 0;
	public void addAnInt(int a)
	{
		theInt += a;
	}
	public void subtractAnInt(int a)
	{
		theInt -= a;
	}
	delegate void voidDelegate(int i);
	public void derp()
	{
		$(voidDelegate)
		voidDelegate del1,del2,del3;

	}

}

"
			);
		}

		[Test]
		public void Delegate01()
		{
			AssertCode(
@"using Uno;

class b
{
	static int theInt = 0;
	public void addAnInt(int a)
	{
		theInt += a;
	}
	public void subtractAnInt(int a)
	{
		theInt -= a;
	}
	delegate void voidDelegate(int i);
	public void derp()
	{
		voidDelegate del1,del2,del3;

		del1 = addAnInt;
		del2 = subtractAnInt;
		del3 = (voidDelegate)Delegate.$(Combine,Remove)
	}
}

"
            );
		}

		[Test]
		public void Delegate02()
		{
			AssertCode(
@"

class b
{
	static int theInt = 0;
	public void addAnInt(int a)
	{
		theInt += a;
	}
	public void subtractAnInt(int a)
	{
		theInt -= a;
	}
	delegate void voidDelegate(int i);
	public void derp()
	{
		voidDelegate del1,del2,del3;

		del1 = addAnInt;
		del2 = subtractAnInt;
		del3 = (voidDelegate)Delegate.Combine(del1,del2);
		$(del1,del2,del3)
	}

}


"
			);
		}

	}
}