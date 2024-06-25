using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class TryCatch : Test
	{
		[Test]
		public void TryCatch00()
		{
			AssertCode(
@"using Uno;

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

}"
			);
		}

        [Test]
        public void TryCatch01()
        {
            AssertCode(
@"using Uno;

class b
{
	public void derp()
	{
		try
		{
			int i = 5;
		}
		catch(Exception e)
        {
        }
	}

    public void Foo()
    {
        int test = 20;
        $(test)
    }

}"
            );
        }

	}
}