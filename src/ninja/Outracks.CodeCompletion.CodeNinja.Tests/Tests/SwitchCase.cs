using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class SwitchCase : Test
	{
		[Test]
		public void SwitchCase00()
		{
			AssertCode(
@"class b
{
	b()
	{
		int num = 0;
		switch(num)
		{
		 	$(case)
		}

	}
}

"
			);
		}

		[Test]
		public void SwitchCase01()
		{
			AssertCode(
@"

class b
{
	b()
	{
		int num = 0;
		switch(num)
		{
		 	case 0:
				$(break)
		}

	}
}


"
			);
		}

		[Test]
		public void SwitchCase02()
		{
			AssertCode(
@"

class b
{
	b()
	{
		int num = 0;
		switch(num)
		{
		 	case 0:
				break;
			$(case,default)
		}

	}
}

"
			);
		}

		[Test]
		public void SwitchCase03()
		{
			AssertCode(
@"

class b
{
	b()
	{
		int num = 0;
		switch(num)
		{
		 	case 0:
				break;
			case:
				break;
			$(case,default)
		}

	}
}


"
			);
		}

		[Test]
		public void SwitchCase04()
		{
			AssertCode(
@"

using Uno;

class b
{
	public void derp()
	{
		try
		{
			int i = 5;
		}
		catch($(Exception,IndexOutOfRangeException,InvalidCastException,InvalidOperationException,!int,!float,!double,!bool)
	}

}
"
			);
		}

	}
}