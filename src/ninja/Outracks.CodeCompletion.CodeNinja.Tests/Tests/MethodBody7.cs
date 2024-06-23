using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class MethodBody7 : Test
	{
		[Test]
		public void MethodBody700()
		{
			AssertCode(
@"

namespace StandardLib.UI
{
	class PointerEventTest : Test
	{
		public PointerEventTest()
			: base(""PointerEventTest"")
		{
		}

		public override void Run()
		{
			var a = new PointerEventArgs($(float2)
			assert(a is PointerEventArgs);
			assert(a is EventArgs);

		}
	}
}

"
			);
		}

		[Test]
		public void MethodBody701()
		{
			AssertCode(
@"

namespace StandardLib.UI
{
	class EventArgsTest : Test
	{
		class SomeClass
		{
			public event EventHandler SomeEvent;

			public void OnSomeEvent()
			{
				if (SomeEvent != $(null)
			}
		}

		public EventArgsTest()
			: base(""EventArgsTest"")
		{
		}

		public override void Run()
		{
			var a = EventArgs.Empty;
			assert(a is EventArgs);

			var b = new SomeClass();
			handled = false;
			assert(!handled);
			b.OnSomeEvent();
			assert(!handled);
			b.SomeEvent += someHandler;
			assert(!handled);
			b.OnSomeEvent();
			assert(handled);
		}

		bool handled;
		void someHandler(object sender, EventArgs e)
		{
			handled = true;
			assert(e is EventArgs);
		}
	}
}

"
			);
		}

        [Test]
        public void MethodBody702()
        {
            AssertCode(
            @"
                class Foo
                {
                    public void Test(int foo=0)
                    {
                        $(foo)
                    }
                }
            ");
        }

        [Test]
        public void MethodBody703()
        {
            AssertCode(
            @"
                class Foo
                {
                    public void foobar(float2 targetPos = float2(0))
                    {
                        $(targetPos)
                    }
                }
            ");
        }

        [Test]
        public void MethodBody704()
        {
            AssertCode(
            @"
                class Foo
                {
                    public void foobar(Uno.UI.Element el, float2 targetPos = float2(0))
                    {
                        $(el, targetPos)
                    }
                }
            ");
        }

		/*[Test]
		public void MethodBody702()
		{
			AssertCode(
@"

using Uno;
using Uno.UI;

namespace StandardLib.UI
{
	class PointerEventTest : Test
	{
		public PointerEventTest()
			: base(""PointerEventTest"")
		{
		}

		public override void Run()
		{
			var a = new PointerEventArgs(float2(1.0f, 2.0f), $(PointerEventType, !a)
			assert(a is PointerEventArgs);
			assert(a is EventArgs);

		}
	}
}

"
			);
		}

		[Test]
		public void MethodBody703()
		{
			AssertCode(
@"

using Uno;
using Uno.UI;

namespace StandardLib.UI
{
	class PointerEventTest : Test
	{
		public PointerEventTest()
			: base(""PointerEventTest"")
		{
		}

		public override void Run()
		{
			var a = new PointerEventArgs(float2(1.0f, 2.0f), PointerEventType.Click);
			assert(a is PointerEventArgs);
			assert(a is EventArgs);
			assert(compare(a.Position, float2(1.0f, 2.0f));
			assert(a.Type == $(PointerEventType)
		}
	}
}
"
			);
		}*/

	}
}