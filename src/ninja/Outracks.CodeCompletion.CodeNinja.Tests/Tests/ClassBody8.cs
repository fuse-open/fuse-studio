using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class ClassBody8 : Test
	{
		[Test]
		public void ClassBody800()
		{
			AssertCode(
@"public class Foo
{
	Uno.Exception ex = new Uno.$(Exception)
}

"
			);
		}

		[Test]
		public void ClassBody801()
		{
			AssertCode(
@"

using Uno;

public class Foo
{
	Exception ex = new $(Exception)
}

"
			);
		}

	    [Test]
	    public void ClassBody802()
	    {
		    AssertCode(
			    @"

using Uno;
using Fuse;

namespace StandardLib.UI
{
	class PointerEventTest : Test
	{
		class SomeClass
		{
			public $(event)
		}

		public PointerEventTest()
			: base(""PointerEventTest"")
		{
		}

		public override void Run()
		{
			var a = new PointerEventArgs(float2(1.0f, 2.0f), PointerEventType.Click);
			assert(a is PointerEventArgs);
			assert(a is EventArgs);
			assert(compare(a.Position, float2(1.0f, 2.0f)));
			assert(a.Type == PointerEventType.Click);
			assert(compare(a.WorldPosition, float3(0.0f)));
			assert(!a.Handled);
		}
	}
}
");
	    }

	    [Test]
	    public void ClassBody803()
	    {
		    AssertCode(
				@"
using Uno;

namespace Fuse.Gestures
{
	class ClickedArgs
	{
		public bool IsHandled()
		{
			return true;
		}
	}
}

public partial class MainView
{
	void ButtonClicked(object sender, Fuse.Gestures.ClickedArgs args)
	{
		args.$(IsHandled)
	}
}
");
	    }
	}
}